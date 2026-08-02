using Cosmere.Core.Ability;
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
    private static readonly Color BurningTint = new Color(0.498f, 0.714f, 0.847f);
    private static readonly Color FlaringTint = new Color(0.878f, 0.416f, 0.271f);
    private static readonly Color QuadHeader = new Color(0.490f, 0.384f, 0.259f);

    private const float StripPadding = 7f;
    private const float ReserveBarHeight = 10f;
    private const float StripButtonHeight = 24f;

    // BurnRate is charged once per rare tick, so it has to be divided back down to
    // read as a rate rather than as a number four seconds wide.
    private const float RareTicksPerSecond = GenTicks.TickRareInterval / 60f;
    private static readonly Color Accent = new Color(0.478f, 0.400f, 0.263f);

    private static float ChromeHeight =>
        StripPadding * 2f + Text.LineHeightOf(GameFont.Tiny) + ReserveBarHeight + 22f;

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

    // Which strip is on screen, which is not the same as which one the player has open:
    // a closing strip has to keep drawing until it has finished sliding away.
    private string? revealedMetal;
    private float revealedHeight;

    // Picked while another metal is still open, and held until that one has finished
    // sliding away.
    private string? pendingMetal;
    private IReadOnlyList<MetalGroup>? cachedGroups;
    private int cachedPawnId = -1;
    private int cachedCellCount = -1;

    // Idempotent, because height is asked for several times a frame. Reveal itself only
    // advances once per frame; this just re-reads where it got to.
    private void StepReveal(float openHeight) {
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

    // Clicking the open metal closes it; clicking a different one closes it first and
    // queues the new one behind it.
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

    public override string SystemId => "Allomancy";

    public override float GetHeaderHeight() {
        return 28f;
    }

    public override float GetExpandedBodyHeight(Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        crest.Refresh(pawn, snapshot);
        float openHeight = StripHeightFor(CellFor(snapshot, expandedMetal ?? pendingMetal));
        StepReveal(openHeight);
        return crest.Height + MetallicArtsTable.HeightFor(GroupsFor(pawn, snapshot), revealedMetal, revealedHeight);
    }

    public override void DrawBody(Rect rect, Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        crest.Refresh(pawn, snapshot);
        crest.Draw(new Rect(rect.x, rect.y, rect.width, crest.Height - ScadrialCrest.Gap), Skin);
        float openHeight = StripHeightFor(CellFor(snapshot, expandedMetal ?? pendingMetal));
        StepReveal(openHeight);

        Rect table = new Rect(rect.x, rect.y + crest.Height, rect.width, rect.height - crest.Height);
        MetallicArtsTable.Draw(
            table,
            GroupsFor(pawn, snapshot),
            QuadHeader,
            revealedMetal,
            revealedHeight,
            openHeight,
            (tileRect, row) => DrawTile(tileRect, pawn, row),
            (stripRect, row, tileRect) => DrawStrip(stripRect, pawn, row, tileRect)
        );
    }

    private void DrawTile(Rect rect, Pawn pawn, MetalRow row) {
        InvestitureCell cell = row.Cell;
        MetalTileState state = cell.IsFlaring
            ? MetalTileState.Flaring
            : cell.IsActive
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
            cell.IsFlaring ? FlaringTint : BurningTint,
            savantStage: savant,
            joinedBelow: revealedMetal == cell.SubsystemId
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

        // Same fill and stroke as the tile, opened along the tile's span: the two are
        // one merged surface. A lit metal carries its accent down through the join as
        // well, so the pair reads as burning rather than as a lit tile on a dead panel.
        bool hot = cell.IsActive || cell.IsFlaring;
        Color tint = cell.IsFlaring ? FlaringTint : BurningTint;
        Panel.DrawNotchedTop(
            rect,
            MetalTile.Fill,
            hot ? tint : MetalTile.Border,
            openTile.x,
            openTile.xMax,
            hot ? tint : null,
            Panel.LitWash(cell.IsFlaring)
        );

        Rect inner = rect.ContractedBy(StripPadding);
        float tinyH = Text.LineHeightOf(GameFont.Tiny);

        string state = cell.IsFlaring
            ? "CC_Dock_State_Flaring".Translate()
            : cell.IsActive
                ? "CC_Dock_State_Burning".Translate()
                : "CC_Dock_Feruchemy_Idle".Translate();

        // The metal's own name is already on the tile this strip opened from, so
        // repeating it here spends the row on something the player just clicked.
        UIText.EllipsisLabel(
            new Rect(inner.x, inner.y, inner.width * 0.6f, tinyH),
            state,
            GameFont.Tiny,
            TextAnchor.MiddleLeft,
            new Color(0.780f, 0.718f, 0.596f)
        );

        // Always shown, zero included. A readout that vanishes when idle makes the
        // row change shape every time a metal lights, which reads as a glitch.
        UIText.EllipsisLabel(
            new Rect(inner.x + inner.width * 0.6f, inner.y, inner.width * 0.4f, tinyH),
            "CC_Dock_Feruchemy_Rate".Translate($"{BurnRatePerSecond(pawn, cell):+0.00;-0.00;0.00}".Named("RATE")),
            GameFont.Tiny,
            TextAnchor.MiddleRight,
            cell.IsActive ? new Color(0.851f, 0.643f, 0.255f) : new Color(0.478f, 0.443f, 0.376f)
        );

        Rect bar = new Rect(inner.x, inner.y + tinyH + 6f, inner.width, ReserveBarHeight);
        Panel.Draw(bar, new Color(0.047f, 0.043f, 0.035f), new Color(0.259f, 0.227f, 0.169f));
        Widgets.DrawBoxSolid(
            new Rect(bar.x + 2f, bar.y + 2f, (bar.width - 4f) * Mathf.Clamp01(cell.Bar.Fraction), bar.height - 4f),
            MetalPalette.For(cell.SubsystemId)
        );

        List<AbilityRow> rows = RowsFor(cell);
        bool headers = AbilityRowLayout.ShowGroupHeaders(rows);
        float y = bar.yMax + 10f;
        bool drawnSustained = false;
        bool drawnTargeted = false;

        for (int i = 0; i < rows.Count; i++) {
            AbilityRow row2 = rows[i];
            if (headers && !row2.IsTargeted && !drawnSustained) {
                DrawGroupHeader(
                    new Rect(inner.x, y, inner.width, AbilityRowLayout.GroupHeaderHeight),
                    "CC_Dock_Allomancy_GroupSustained".Translate()
                );
                y += AbilityRowLayout.GroupHeaderHeight;
                drawnSustained = true;
            }

            if (headers && row2.IsTargeted && !drawnTargeted) {
                DrawGroupHeader(
                    new Rect(inner.x, y, inner.width, AbilityRowLayout.GroupHeaderHeight),
                    "CC_Dock_Allomancy_GroupTargeted".Translate()
                );
                y += AbilityRowLayout.GroupHeaderHeight;
                drawnTargeted = true;
            }

            InvestitureAbility ability = FindAbility(cell, row2.DefName);
            DrawAbilityRow(new Rect(inner.x, y, inner.width, AbilityRowLayout.RowHeight), pawn, ability);
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

    private void DrawAbilityRow(Rect rect, Pawn pawn, InvestitureAbility ability) {
        float chipWidth = ability.CanFlare ? 30f : 0f;
        float mainWidth = rect.width - (ability.CanFlare ? chipWidth + 4f : 0f);

        string label = ability.IsActive
            ? "CC_Dock_Allomancy_StopBurn".Translate() + " " + ability.Label
            : ability.IsTargeted
                ? "CC_Dock_Allomancy_Target".Translate() + " " + ability.Label
                : "CC_Dock_Allomancy_Burn".Translate() + " " + ability.Label;

        Rect mainRect = new Rect(rect.x, rect.y, mainWidth, rect.height);
        TooltipHandler.TipRegion(
            mainRect,
            "CC_Dock_Allomancy_AbilityTip".Translate(
                ability.Label.Named("ABILITY"),
                (ability.Def.description ?? string.Empty).Named("DESC")
            )
        );

        if (DockButton.Draw(
                mainRect,
                label,
                ability.IsFlaring ? FlaringTint : Accent,
                kind: ability.IsActive ? DockButtonKind.Active : DockButtonKind.Primary
            )) {
            if (ability.IsTargeted) RadialDispatcher.CastOrToggle(pawn, ability.Def);
            else ToggleAbility(pawn, ability, false);
            Event.current?.Use();
        }

        if (!ability.CanFlare) return;

        Rect chipRect = new Rect(rect.xMax - chipWidth, rect.y, chipWidth, rect.height);
        TooltipHandler.TipRegion(chipRect, "CC_Dock_Allomancy_FlareTip".Translate(ability.Label.Named("METAL")));
        if (DockButton.Draw(
                chipRect,
                ability.IsFlaring ? "CC_Dock_Allomancy_StopFlare".Translate() : "CC_Dock_Allomancy_Flare".Translate(),
                ability.IsFlaring ? FlaringTint : Accent,
                kind: ability.IsFlaring ? DockButtonKind.Active : DockButtonKind.Ghost
            )) {
            ToggleAbility(pawn, ability, true);
            Event.current?.Use();
        }
    }

    // Charged once per rare tick, so the raw figure is four seconds of burn. Shown
    // negative because a reserve going down should read as going down.
    private static float BurnRatePerSecond(Pawn pawn, InvestitureCell cell) {
        Allomancer? gene = FindGene(pawn, cell.SubsystemId);
        if (gene == null) return 0f;

        return -gene.BurnRate / RareTicksPerSecond;
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

    // What burning this metal actually does, in the metal def's own words. The
    // tooltip described the click rather than the power before this.
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
    private static void ToggleAbility(Pawn pawn, InvestitureAbility ability, bool flare) {
        if (pawn.abilities == null) return;

        List<Ability> all = pawn.abilities.AllAbilitiesForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is not AllomancyAbility a || a.def != ability.Def) continue;

            a.UpdateStatus(BurnToggle.Next(a.status, flare));
            return;
        }
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
