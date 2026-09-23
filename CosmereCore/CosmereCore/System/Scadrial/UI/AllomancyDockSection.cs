using Cosmere.Core.Ability;
using Cosmere.Core.Gene;
using Cosmere.Core.Investiture;
using Cosmere.Core.Tab;
using Cosmere.Core.UI;
using Cosmere.Core.UI.Dock;
using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Radial;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Feruchemy;
using Cosmere.System.Scadrial.Gene;
using Cosmere.System.Scadrial.Savant;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.System.Scadrial.UI;

public sealed class AllomancyDockSection : DockSectionBase {
    private static readonly Color FlaringTint = new Color(0.878f, 0.416f, 0.271f);
    private static readonly Color QuadHeader = new Color(0.490f, 0.384f, 0.259f);

    private const float StripPadding = 7f;
    private const float ReserveBarHeight = 10f;

    private static readonly Color Accent = new Color(0.478f, 0.400f, 0.263f);

    private static float ChromeHeight => StripPadding * 2f + ReserveBarHeight + 16f;

    private static float StripHeightFor(InvestitureCell? cell) {
        if (cell == null) return 0f;

        return ChromeHeight + AbilityRowLayout.HeightFor(RowsFor(cell));
    }

    private static List<AbilityRow> RowsFor(InvestitureCell cell) {
        List<AbilityRow> rows = new List<AbilityRow>(cell.Abilities.Count);
        for (int i = 0; i < cell.Abilities.Count; i++) {
            InvestitureAbility a = cell.Abilities[i];
            rows.Add(new AbilityRow(a.AbilityDefName, a.Label, a.IsTargeted, a.CanFlare));
        }

        return AbilityRowLayout.Ordered(rows);
    }

    private readonly ScadrialCrest crest = new ScadrialCrest(false);
    private readonly Dictionary<string, string> labelCache = new Dictionary<string, string>();
    private readonly Reveal reveal = new Reveal();

    private string? expandedMetal;

    /// <summary>
    ///     Which strip is on screen - not the same as which one the player has open. A closing
    ///     strip keeps drawing until it finishes sliding away.
    /// </summary>
    private string? revealedMetal;
    private float revealedHeight;

    /// <summary>
    ///     Picked while another metal is still open, held until that one finishes sliding away.
    /// </summary>
    private string? pendingMetal;
    private IReadOnlyList<MetalGroup>? cachedGroups;
    private int cachedPawnId = -1;
    private int cachedCellCount = -1;

    /// <summary>
    ///     Idempotent - height gets asked for several times a frame. Reveal only advances once per
    ///     frame; this just re-reads where it got to.
    /// </summary>
    private void StepReveal(float openHeight) {
        // one panel at a time: open metal finishes closing before the queued one opens, so mid-slide never jumps rows
        if (expandedMetal == null && pendingMetal != null && revealedHeight < 1f) {
            expandedMetal = pendingMetal;
            pendingMetal = null;
        }

        revealedHeight = reveal.Toward(expandedMetal == null ? 0f : openHeight);
        if (expandedMetal != null) revealedMetal = expandedMetal;
        else if (revealedHeight < 1f) revealedMetal = null;
    }

    private static InvestitureCell? CellFor(InvestitureSnapshot snapshot, string? subsystemId) {
        if (subsystemId == null) return null;

        for (int i = 0; i < snapshot.Cells.Count; i++) {
            if (snapshot.Cells[i].SubsystemId == subsystemId) return snapshot.Cells[i];
        }

        return null;
    }

    /// <summary>
    ///     Clicking the open metal closes it; clicking a different one closes it first and queues behind.
    /// </summary>
    private void ToggleMetal(string subsystemId) {
        if (expandedMetal == subsystemId) {
            expandedMetal = null;
            pendingMetal = null;
            return;
        }

        if (expandedMetal == null && revealedHeight < 1f) {
            expandedMetal = subsystemId;
            pendingMetal = null;
            return;
        }

        expandedMetal = null;
        pendingMetal = subsystemId;
    }

    private bool IsCollapsed(MetalGroup group) {
        return IsGroupCollapsed(group.LabelKey);
    }

    /// <summary>
    ///     Folding a quadrant closes any metal open inside it - the strip would otherwise keep its
    ///     height with nothing above it to explain where it came from.
    /// </summary>
    private void ToggleGroup(MetalGroup group) {
        if (!ToggleGroupFold(group.LabelKey)) return;

        for (int i = 0; i < group.Rows.Count; i++) {
            string id = group.Rows[i].Cell.SubsystemId;
            if (expandedMetal == id) expandedMetal = null;
            if (pendingMetal == id) pendingMetal = null;
        }
    }

    public override string SystemId => "Allomancy";

    public override float GetHeaderHeight() {
        return 28f;
    }

    public override float GetExpandedBodyHeight(Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        crest.Refresh(pawn, snapshot);
        float revealTarget = StripHeightFor(CellFor(snapshot, expandedMetal ?? pendingMetal));
        StepReveal(revealTarget);
        return crest.Height + MetallicArtsTable.HeightFor(
            GroupsFor(pawn, snapshot),
            revealedMetal,
            revealedHeight,
            IsCollapsed
        );
    }

    private static int BurningRowCount(Pawn pawn, IReadOnlyList<MetalGroup> groups) {
        int count = 0;
        for (int g = 0; g < groups.Count; g++) {
            IReadOnlyList<MetalRow> rows = groups[g].Rows;
            for (int r = 0; r < rows.Count; r++) {
                IReadOnlyList<InvestitureAbility> abilities = rows[r].Cell.Abilities;
                for (int a = 0; a < abilities.Count; a++) {
                    if (abilities[a].IsActive) count++;
                }

                count += UnclaimedDrainCount(pawn, rows[r].Cell);
            }
        }

        return count;
    }

    /// <summary>
    ///     Drains no burning ability accounts for - a koloss hold keeps a live source while the
    ///     ability it is keyed to reads idle, and the reserve would fall with nothing on screen.
    /// </summary>
    private static int UnclaimedDrainCount(Pawn pawn, InvestitureCell cell) {
        Allomancer? gene = FindGene(pawn, cell.SubsystemId);
        if (gene == null) return 0;

        int count = 0;
        List<DrainSource> sources = gene.Sources;
        for (int i = 0; i < sources.Count; i++) {
            if (sources[i].Rate > 0f && !IsClaimed(cell, sources[i].Def?.defName)) count++;
        }

        return count;
    }

    private static bool IsClaimed(InvestitureCell cell, string? abilityDefName) {
        if (abilityDefName == null) return false;

        for (int i = 0; i < cell.Abilities.Count; i++) {
            if (cell.Abilities[i].IsActive && cell.Abilities[i].AbilityDefName == abilityDefName) return true;
        }

        return false;
    }

    public override float GetPinnedHeight(Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        return BurningStripLayout.HeightFor(BurningRowCount(pawn, GroupsFor(pawn, snapshot)));
    }

    /// <summary>
    ///     What is burning right now, above the scroll view. Scrolling to the far end of the table,
    ///     or folding a quadrant, must never hide a metal that is draining.
    /// </summary>
    public override void DrawPinned(Rect rect, Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        UIText.EllipsisLabel(
            new Rect(rect.x, rect.y, rect.width, BurningStripLayout.HeaderHeight),
            "CC_Dock_Burning_Header".Translate(),
            GameFont.Tiny,
            TextAnchor.MiddleLeft,
            DockPalette.GroupLabel
        );

        float y = rect.y + BurningStripLayout.HeaderHeight + BurningStripLayout.Padding;
        IReadOnlyList<MetalGroup> groups = GroupsFor(pawn, snapshot);

        for (int g = 0; g < groups.Count; g++) {
            IReadOnlyList<MetalRow> rows = groups[g].Rows;
            for (int r = 0; r < rows.Count; r++) {
                InvestitureCell cell = rows[r].Cell;
                for (int a = 0; a < cell.Abilities.Count; a++) {
                    if (!cell.Abilities[a].IsActive) continue;

                    DrawBurningRow(new Rect(rect.x, y, rect.width, BurningStripLayout.RowHeight), pawn, cell, cell.Abilities[a]);
                    y += BurningStripLayout.RowHeight;
                }

                y = DrawUnclaimedRows(rect, y, pawn, cell);
            }
        }

        Widgets.DrawBoxSolid(new Rect(rect.x, Mathf.Round(rect.yMax - BurningStripLayout.Footer / 2f), rect.width, 1f), DockPalette.BorderSubtle);
    }

    /// <summary>
    ///     Nothing to click - the hold that owns the drain is ended where it was made, not here.
    /// </summary>
    private float DrawUnclaimedRows(Rect strip, float y, Pawn pawn, InvestitureCell cell) {
        Allomancer? gene = FindGene(pawn, cell.SubsystemId);
        if (gene == null) return y;

        List<DrainSource> sources = gene.Sources;
        for (int i = 0; i < sources.Count; i++) {
            if (sources[i].Rate <= 0f || IsClaimed(cell, sources[i].Def?.defName)) continue;

            Rect rect = new Rect(strip.x, y, strip.width, BurningStripLayout.RowHeight);
            string label = sources[i].Def?.LabelCap ?? sources[i].Def?.defName ?? cell.SubsystemId;

            UIText.EllipsisLabel(
                new Rect(rect.x, rect.y, rect.width * 0.55f, rect.height),
                "CC_Dock_Burning_Row".Translate(MetalLabel(cell).Named("METAL"), label.Named("ABILITY")),
                GameFont.Tiny,
                TextAnchor.MiddleLeft,
                DockPalette.HotLabel
            );

            UIText.EllipsisLabel(
                new Rect(rect.x + rect.width * 0.55f, rect.y, rect.width * 0.45f, rect.height),
                "CC_Dock_Burning_Reading".Translate(
                    Mathf.RoundToInt(cell.Bar.Fraction * 100f).Named("PERCENT"),
                    $"{ReservePercentPerSecond(gene, sources[i].Rate):+0.00;-0.00;0.00}".Named("RATE")
                ),
                GameFont.Tiny,
                TextAnchor.MiddleRight,
                DockPalette.MutedText
            );

            TooltipHandler.TipRegion(rect, "CC_Dock_Burning_UnclaimedTip".Translate(label.Named("SOURCE")));
            y += BurningStripLayout.RowHeight;
        }

        return y;
    }

    private void DrawBurningRow(Rect rect, Pawn pawn, InvestitureCell cell, InvestitureAbility ability) {
        if (Mouse.IsOver(rect)) Widgets.DrawBoxSolid(rect, DockPalette.PanelRaised);

        UIText.EllipsisLabel(
            new Rect(rect.x, rect.y, rect.width * 0.55f, rect.height),
            "CC_Dock_Burning_Row".Translate(
                MetalLabel(cell).Named("METAL"),
                ability.Label.Named("ABILITY")
            ),
            GameFont.Tiny,
            TextAnchor.MiddleLeft,
            ability.IsFlaring ? FlaringTint : DockPalette.HotLabel
        );

        UIText.EllipsisLabel(
            new Rect(rect.x + rect.width * 0.55f, rect.y, rect.width * 0.45f, rect.height),
            "CC_Dock_Burning_Reading".Translate(
                Mathf.RoundToInt(cell.Bar.Fraction * 100f).Named("PERCENT"),
                $"{BurnReservePercentPerSecond(pawn, cell, ability):+0.00;-0.00;0.00}".Named("RATE")
            ),
            GameFont.Tiny,
            TextAnchor.MiddleRight,
            DockPalette.MutedText
        );

        TooltipHandler.TipRegion(rect, "CC_Dock_Burning_StopTip".Translate(ability.Label.Named("ABILITY")));
        MouseoverSounds.DoRegion(rect);
        if (!Widgets.ButtonInvisible(rect)) return;

        ToggleAbility(pawn, ability, false);
        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
        Event.current?.Use();
    }

    public override void DrawBody(Rect rect, Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        crest.Refresh(pawn, snapshot);
        crest.Draw(new Rect(rect.x, rect.y, rect.width, crest.Height - ScadrialCrest.Gap), Skin);
        float revealTarget = StripHeightFor(CellFor(snapshot, expandedMetal ?? pendingMetal));
        StepReveal(revealTarget);
        float revealedStripHeight = StripHeightFor(CellFor(snapshot, revealedMetal));

        Rect table = new Rect(rect.x, rect.y + crest.Height, rect.width, rect.height - crest.Height);
        MetallicArtsTable.Draw(
            table,
            GroupsFor(pawn, snapshot),
            QuadHeader,
            revealedMetal,
            revealedHeight,
            revealedStripHeight,
            (tileRect, row) => DrawTile(tileRect, pawn, row),
            (stripRect, row, tileRect) => DrawStrip(stripRect, pawn, row, tileRect),
            IsCollapsed,
            ToggleGroup
        );
    }

    private void DrawTile(Rect rect, Pawn pawn, MetalRow row) {
        InvestitureCell cell = row.Cell;
        MetalTileState state = cell.IsFlaring
            ? MetalTileState.Flaring
            : cell.IsActive || UnclaimedDrainCount(pawn, cell) > 0
                ? MetalTileState.Active
                : MetalTileState.Idle;

        MetallicArtsMetalDef? def = DefDatabase<MetallicArtsMetalDef>.GetNamedSilentFail(cell.SubsystemId);
        int savant = def != null && pawn.records != null
            ? ScadrialSavantUtility.GetAllomanticSavantStage(pawn, def)
            : 0;

        MetalTile.Draw(
            rect,
            cell.Icon,
            MetalLabel(cell),
            $"{cell.Bar.Fraction * 100f:0}%",
            cell.Bar.Fraction,
            MetalPalette.For(cell.SubsystemId),
            state,
            savantStage: savant,
            joinedBelow: revealedMetal == cell.SubsystemId,
            foldLabel: FindGene(pawn, cell.SubsystemId) == null
                ? null
                : revealedMetal == cell.SubsystemId
                    ? (string)"CC_Dock_Fold_Hide".Translate()
                    : (string)"CC_Dock_Fold_Show".Translate()
        );

        TooltipHandler.TipRegion(rect, () => Tooltip(pawn, cell), cell.SubsystemId.GetHashCode());

        if (!Widgets.ButtonInvisible(rect)) return;

        Event? current = Event.current;
        if (current is { button: 1 }) {
            OpenThreshold(pawn, cell);
            current.Use();
            return;
        }

        ToggleMetal(cell.SubsystemId);
        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
        current?.Use();
    }

    private void DrawStrip(Rect rect, Pawn pawn, MetalRow row, Rect openTile) {
        InvestitureCell cell = row.Cell;

        // Nothing to draw for a metal this pawn cannot burn.
        if (FindGene(pawn, cell.SubsystemId) == null) return;

        // tile and strip are one surface. No accent - burning is already said by the name, the band and this strip.
        Panel.DrawNotchedTop(rect, DockPalette.StripFill, MetalTile.Border, openTile.x, openTile.xMax);

        Rect inner = rect.ContractedBy(StripPadding);

        Rect bar = new Rect(inner.x, inner.y, inner.width, ReserveBarHeight);
        Panel.Draw(bar, new Color(0.047f, 0.043f, 0.035f), new Color(0.259f, 0.227f, 0.169f));
        Widgets.DrawBoxSolid(
            new Rect(bar.x + 2f, bar.y + 2f, (bar.width - 4f) * Mathf.Clamp01(cell.Bar.Fraction), bar.height - 4f),
            MetalPalette.For(cell.SubsystemId)
        );

        List<AbilityRow> rows = RowsFor(cell);
        float y = bar.yMax + 10f;
        bool drawnSustained = false;
        bool drawnTargeted = false;

        for (int i = 0; i < rows.Count; i++) {
            AbilityRow row2 = rows[i];
            if (!row2.IsTargeted && !drawnSustained) {
                DrawGroupHeader(
                    new Rect(inner.x, y, inner.width, AbilityRowLayout.GroupHeaderHeight),
                    "CC_Dock_Allomancy_GroupSustained".Translate()
                );
                y += AbilityRowLayout.GroupHeaderHeight;
                drawnSustained = true;
            }

            if (row2.IsTargeted && !drawnTargeted) {
                DrawGroupHeader(
                    new Rect(inner.x, y, inner.width, AbilityRowLayout.GroupHeaderHeight),
                    "CC_Dock_Allomancy_GroupTargeted".Translate()
                );
                y += AbilityRowLayout.GroupHeaderHeight;
                drawnTargeted = true;
            }

            InvestitureAbility ability = FindAbility(cell, row2.DefName);
            DrawAbilityRow(new Rect(inner.x, y, inner.width, AbilityRowLayout.RowHeight), pawn, cell, ability);
            y += AbilityRowLayout.RowHeight + AbilityRowLayout.RowGap;
        }
    }

    private static InvestitureAbility FindAbility(InvestitureCell cell, string defName) {
        for (int i = 0; i < cell.Abilities.Count; i++) {
            if (cell.Abilities[i].AbilityDefName == defName) return cell.Abilities[i];
        }

        return cell.Abilities[0];
    }

    private static void DrawGroupHeader(Rect rect, string label) {
        UIText.EllipsisLabel(
            rect,
            label,
            GameFont.Tiny,
            TextAnchor.LowerLeft,
            new Color(0.420f, 0.373f, 0.298f)
        );
    }

    private void DrawAbilityRow(Rect rect, Pawn pawn, InvestitureCell cell, InvestitureAbility ability) {
        // rate, not IsActive: a koloss hold drains through an ability whose status reads idle
        float burnRate = BurnReservePercentPerSecond(pawn, cell, ability);
        float chipWidth = ability.CanFlare ? 44f : 0f;
        float mainWidth = rect.width - (ability.CanFlare ? chipWidth + 4f : 0f);

        string? blocked = ability.IsTargeted ? null : BurnBlockedReason(pawn, ability, false);

        Rect mainRect = new Rect(rect.x, rect.y, mainWidth, rect.height);
        TooltipHandler.TipRegion(
            mainRect,
            blocked ?? "CC_Dock_Allomancy_AbilityTip".Translate(
                ability.Label.Named("ABILITY"),
                (ability.Def.description ?? string.Empty).Named("DESC")
            )
        );

        if (DockButton.Draw(
                mainRect,
                ability.Label,
                ability.IsFlaring ? FlaringTint : DockPalette.HotLabel,
                active: ability.IsActive,
                enabled: blocked == null,
                icon: ability.Def.uiIcon,
                aside: burnRate != 0f
                    ? (string)"CC_Dock_Allomancy_BurnRate".Translate(
                        $"{burnRate:+0.00;-0.00;0.00}".Named("RATE")
                    )
                    : null
            )) {
            if (ability.IsTargeted) RadialDispatcher.CastOrToggle(pawn, ability.Def);
            else ToggleAbility(pawn, ability, false);
            Event.current?.Use();
        }

        if (!ability.CanFlare) return;

        string? flareBlocked = BurnBlockedReason(pawn, ability, true);

        Rect chipRect = new Rect(rect.xMax - chipWidth, rect.y, chipWidth, rect.height);
        TooltipHandler.TipRegion(
            chipRect,
            flareBlocked ?? "CC_Dock_Allomancy_AbilityFlareTip".Translate(ability.Label.Named("ABILITY"))
        );
        if (DockButton.Draw(
                chipRect,
                ability.IsFlaring ? "CC_Dock_Allomancy_StopFlare".Translate() : "CC_Dock_Allomancy_Flare".Translate(),
                ability.IsFlaring ? FlaringTint : Accent,
                active: ability.IsFlaring,
                enabled: flareBlocked == null
            )) {
            ToggleAbility(pawn, ability, true);
            Event.current?.Use();
        }
    }

    /// <summary>
    ///     Shown negative because a reserve going down should read as going down. One ability's own
    ///     drain, not gene.BurnRate - a metal running two abilities would double-report.
    /// </summary>
    private static float BurnReservePercentPerSecond(Pawn pawn, InvestitureCell cell, InvestitureAbility ability) {
        Allomancer? gene = FindGene(pawn, cell.SubsystemId);
        if (gene == null) return 0f;

        float rate = 0f;
        List<DrainSource> sources = gene.Sources;
        for (int i = 0; i < sources.Count; i++) {
            if (sources[i].Def?.defName != ability.AbilityDefName) continue;

            rate = sources[i].Rate;
            break;
        }

        return ReservePercentPerSecond(gene, rate);
    }

    private static float ReservePercentPerSecond(Allomancer gene, float rate) {
        if (rate <= 0f) return 0f;

        return -UpkeepRate.ReservePercentPerSecond(
            rate,
            Invested.UpkeepTicks,
            ScadrialMetallurgyConstants.BreathEquivalentUnitsPerMetalUnit,
            gene.Max
        );
    }

    private static Allomancer? FindGene(Pawn pawn, string metalDefName) {
        if (pawn.genes == null) return null;

        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is Allomancer a && a.metal.defName == metalDefName && !a.Overridden) return a;
        }

        return null;
    }

    private string Tooltip(Pawn pawn, InvestitureCell cell) {
        // Carries its own newline so an idle metal does not leave a blank line.
        string state = cell.IsFlaring
            ? "\n" + "CC_Dock_State_Flaring".Translate()
            : cell.IsActive
                ? "\n" + "CC_Dock_State_Burning".Translate()
                : string.Empty;

        return "CC_Dock_Allomancy_Tip".Translate(
            MetalLabel(cell).Named("METAL"),
            Mathf.RoundToInt(cell.Bar.Fraction * 100f).Named("PERCENT"),
            state.Named("STATE"),
            MetalEffect(pawn, cell).Named("EFFECT")
        );
    }

    /// <summary>
    ///     What burning this metal actually does, in the metal defs own words. Before this, the
    ///     tooltip described the click rather than the power.
    /// </summary>
    private static string MetalEffect(Pawn pawn, InvestitureCell cell) {
        MetallicArtsMetalDef? metal =
            DefDatabase<MetallicArtsMetalDef>.GetNamedSilentFail(cell.SubsystemId);
        string? description = metal?.allomancy?.description;

        return string.IsNullOrEmpty(description)
            ? string.Empty
            : description!.Formatted(pawn.LabelShort.Named("PAWN")).Resolve();
    }

    private IReadOnlyList<MetalGroup> GroupsFor(Pawn pawn, InvestitureSnapshot snapshot) {
        if (cachedGroups == null || cachedPawnId != pawn.thingIDNumber || cachedCellCount != snapshot.Cells.Count) {
            cachedGroups = MetalGroupTable.AllomancyGroups(snapshot.Cells);
            cachedPawnId = pawn.thingIDNumber;
            cachedCellCount = snapshot.Cells.Count;
        }

        MetalGroupTable.RefreshCells(cachedGroups, snapshot.Cells);
        return cachedGroups;
    }

    private string MetalLabel(InvestitureCell cell) {
        if (labelCache.TryGetValue(cell.SubsystemId, out string? cached)) return cached;
        string label = DefDatabase<MetallicArtsMetalDef>.GetNamedSilentFail(cell.SubsystemId)?.LabelCap ?? cell.SubsystemId;
        labelCache[cell.SubsystemId] = label;
        return label;
    }

    // The flare rule lives in BurnToggle so the dock and the wheel cannot drift apart.
    private static AllomancyAbility? FindAllomancyAbility(Pawn pawn, InvestitureAbility ability) {
        if (pawn.abilities == null) return null;

        List<Ability> all = pawn.abilities.AllAbilitiesForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is AllomancyAbility a && a.def == ability.Def) return a;
        }

        return null;
    }

    /// <summary>
    ///     Why this row cannot change gear, or null when it can. Priced at the status the click
    ///     would move to, so a flare is charged as a flare rather than as an ordinary burn.
    /// </summary>
    private static string? BurnBlockedReason(Pawn pawn, InvestitureAbility ability, bool flare) {
        AllomancyAbility? a = FindAllomancyAbility(pawn, ability);
        if (a == null) return null;

        Status next = BurnToggle.Next(a.status, flare);
        AcceptanceReport report = a.Gene.CanBurn(a.GetDesiredBurnRateForStatus(next));

        return BurnAffordability.Allowed(a.status.power, next.power, report.Accepted) ? null : report.Reason;
    }

    // The wheel refuses an unaffordable burn through CanCast; the dock drove UpdateStatus straight past it.
    private static void ToggleAbility(Pawn pawn, InvestitureAbility ability, bool flare) {
        AllomancyAbility? a = FindAllomancyAbility(pawn, ability);
        if (a == null) return;

        Status next = BurnToggle.Next(a.status, flare);
        AcceptanceReport report = a.Gene.CanBurn(a.GetDesiredBurnRateForStatus(next));
        if (!BurnAffordability.Allowed(a.status.power, next.power, report.Accepted)) return;

        a.UpdateStatus(next);
    }

    private static void OpenThreshold(Pawn pawn, InvestitureCell cell) {
        if (pawn.genes == null) return;
        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is Allomancer a && a.metal.defName == cell.SubsystemId && !a.Overridden) {
                Find.WindowStack.Add(new Dialog_AllomancyRestockSlider(a));
                return;
            }
        }
    }
}
