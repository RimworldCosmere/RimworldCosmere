using Cosmere.Core.Ability;
using Cosmere.Core.UI;
using Cosmere.Core.UI.Dock;
using Cosmere.Core.UI.Model;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Feruchemy;
using Cosmere.System.Scadrial.Gene;
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
    private const float StripButtonHeight = 22f;
    private static readonly Color Accent = new Color(0.478f, 0.400f, 0.263f);
    private static readonly Color CompoundTint = new Color(0.851f, 0.667f, 0.286f);

    private const float ProgressBarHeight = 6f;

    private static float BaseStripHeight =>
        StripPadding * 2f + Text.LineHeightOf(GameFont.Tiny) + ReserveBarHeight + StripButtonHeight + 14f;

    /// Only the open metal draws a strip, so the compounding readout can claim
    /// extra room without every other row paying for it.
    private float StripHeightFor(Pawn pawn) {
        if (expandedMetal == null) return BaseStripHeight;
        if (CompoundingAccess.FeruchemistFor(pawn, expandedMetal) is not { isCompounding: true }) {
            return BaseStripHeight;
        }

        return BaseStripHeight + ProgressBarHeight + Text.LineHeightOf(GameFont.Tiny) + 7f;
    }

    private readonly Dictionary<string, string> labelCache = new Dictionary<string, string>();
    private string? expandedMetal;
    private IReadOnlyList<MetalGroup>? cachedGroups;
    private int cachedPawnId = -1;
    private int cachedCellCount = -1;

    public override string SystemId => "Allomancy";

    public override float GetHeaderHeight() {
        return 28f;
    }

    public override float GetExpandedBodyHeight(Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        return MetallicArtsTable.HeightFor(GroupsFor(pawn, snapshot), expandedMetal, StripHeightFor(pawn));
    }

    public override void DrawBody(Rect rect, Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        MetallicArtsTable.Draw(
            rect,
            GroupsFor(pawn, snapshot),
            QuadHeader,
            expandedMetal,
            StripHeightFor(pawn),
            (tileRect, row) => DrawTile(tileRect, pawn, row),
            (stripRect, row) => DrawStrip(stripRect, pawn, row)
        );
    }

    private void DrawTile(Rect rect, Pawn pawn, MetalRow row) {
        InvestitureCell cell = row.Cell;
        MetalTileState state = cell.IsFlaring
            ? MetalTileState.Flaring
            : cell.IsActive
                ? MetalTileState.Active
                : MetalTileState.Idle;

        MetalTile.Draw(
            rect,
            cell.Icon,
            MetalLabel(cell),
            $"{cell.Bar.Fraction * 100f:0}%",
            cell.Bar.Fraction,
            MetalPalette.For(cell.SubsystemId),
            state,
            cell.IsFlaring ? FlaringTint : BurningTint
        );

        TooltipHandler.TipRegion(rect, () => Tooltip(cell), cell.SubsystemId.GetHashCode());

        if (!Widgets.ButtonInvisible(rect)) return;

        Event? current = Event.current;
        if (current is { button: 1 }) {
            OpenThreshold(pawn, cell);
            current.Use();
            return;
        }

        expandedMetal = expandedMetal == cell.SubsystemId ? null : cell.SubsystemId;
        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
        current?.Use();
    }

    private void DrawStrip(Rect rect, Pawn pawn, MetalRow row) {
        InvestitureCell cell = row.Cell;
        // Nothing to draw for a metal this pawn cannot burn.
        if (FindGene(pawn, cell.SubsystemId) == null) return;

        Widgets.DrawBoxSolid(rect, new Color(0.098f, 0.086f, 0.063f));
        Widgets.DrawBoxSolidWithOutline(rect, Color.clear, new Color(0.259f, 0.227f, 0.169f));

        Rect inner = rect.ContractedBy(StripPadding);
        float tinyH = Text.LineHeightOf(GameFont.Tiny);

        Feruchemist? feruchemist = CompoundingAccess.FeruchemistFor(pawn, cell.SubsystemId);
        bool compounding = feruchemist is { isCompounding: true };

        string state = compounding
            ? "CC_Dock_Feruchemy_Compounding".Translate()
            : cell.IsFlaring
                ? "CC_Dock_State_Flaring".Translate()
                : cell.IsActive
                    ? "CC_Dock_State_Burning".Translate()
                    : "CC_Dock_Feruchemy_Idle".Translate();

        UIText.EllipsisLabel(
            new Rect(inner.x, inner.y, inner.width * 0.6f, tinyH),
            MetalLabel(cell) + " - " + state,
            GameFont.Tiny,
            TextAnchor.MiddleLeft,
            new Color(0.780f, 0.718f, 0.596f)
        );
        UIText.EllipsisLabel(
            new Rect(inner.x + inner.width * 0.6f, inner.y, inner.width * 0.4f, tinyH),
            $"{cell.Bar.Fraction * 100f:0}%",
            GameFont.Tiny,
            TextAnchor.MiddleRight,
            new Color(0.478f, 0.443f, 0.376f)
        );

        Rect bar = new Rect(inner.x, inner.y + tinyH + 6f, inner.width, ReserveBarHeight);
        Widgets.DrawBoxSolid(bar, new Color(0.047f, 0.043f, 0.035f));
        Widgets.DrawBoxSolid(
            new Rect(bar.x, bar.y, bar.width * Mathf.Clamp01(cell.Bar.Fraction), bar.height),
            MetalPalette.For(cell.SubsystemId)
        );

        float nextY = bar.yMax;
        if (compounding) nextY = DrawCompoundProgress(inner, bar.yMax, feruchemist!, cell);

        float buttonY = nextY + 8f;
        AllomancyAbility? compound = CompoundingAccess.AbilityFor(pawn, cell.SubsystemId);
        bool showCompound = compound != null && feruchemist != null && CompoundingAccess.Discovered(pawn);

        int buttons = showCompound ? 3 : 2;
        float buttonWidth = (inner.width - 5f * (buttons - 1)) / buttons;

        string burnLabel = cell.IsActive
            ? "CC_Dock_Allomancy_StopBurn".Translate()
            : "CC_Dock_Allomancy_Burn".Translate();
        Rect burnRect = new Rect(inner.x, buttonY, buttonWidth, StripButtonHeight);
        TooltipHandler.TipRegion(
            burnRect,
            compounding
                ? "CC_Dock_Allomancy_BusyCompounding".Translate()
                : "CC_Dock_Allomancy_BurnTip".Translate(MetalLabel(cell).Named("METAL"))
        );
        if (DockChrome.Button(burnRect, burnLabel, !compounding, Accent)) {
            ToggleBurn(pawn, cell.SubsystemId, false);
            Event.current?.Use();
        }

        Rect flareRect = new Rect(inner.x + buttonWidth + 5f, buttonY, buttonWidth, StripButtonHeight);
        string flareLabel = cell.IsFlaring
            ? "CC_Dock_Allomancy_StopFlare".Translate()
            : "CC_Dock_Allomancy_Flare".Translate();
        TooltipHandler.TipRegion(
            flareRect,
            compounding
                ? "CC_Dock_Allomancy_BusyCompounding".Translate()
                : "CC_Dock_Allomancy_FlareTip".Translate(MetalLabel(cell).Named("METAL"))
        );
        if (DockChrome.Button(flareRect, flareLabel, !compounding, Accent)) {
            ToggleBurn(pawn, cell.SubsystemId, true);
            Event.current?.Use();
        }

        if (!showCompound) return;

        AcceptanceReport report = CompoundingAccess.Gate(pawn, feruchemist!, compound!);
        bool canCompound = report.Accepted || compounding;

        Rect compoundRect = new Rect(inner.x + (buttonWidth + 5f) * 2f, buttonY, buttonWidth, StripButtonHeight);
        TooltipHandler.TipRegion(
            compoundRect,
            canCompound
                ? "CC_Dock_Allomancy_CompoundTip".Translate(MetalLabel(cell).Named("METAL"))
                : "CC_Dock_Feruchemy_CompoundBlocked".Translate(
                    (report.Reason.NullOrEmpty()
                        ? "CC_Dock_Feruchemy_CompoundUnavailable".Translate().Resolve()
                        : report.Reason).Named("REASON")
                )
        );

        string compoundLabel = compounding
            ? "CC_Dock_Feruchemy_StopStoreCompounded".Translate()
            : "CC_Dock_Feruchemy_StoreCompounded".Translate();
        if (!DockChrome.Button(compoundRect, compoundLabel, canCompound, Accent)) return;

        if (compounding) compound!.UpdateStatus(BurningStatus.Off);
        else compound!.QueueCastingJob(pawn, LocalTargetInfo.Invalid);
        Event.current?.Use();
    }

    /// Progress toward a full metalmind, with what it is costing and how long the
    /// reserve or the remaining room will let it run - whichever runs out first.
    private float DrawCompoundProgress(Rect inner, float y, Feruchemist feruchemist, InvestitureCell cell) {
        float free = feruchemist.CompoundedFreeSpace;
        float compounded = feruchemist.CompoundedAmount;
        float capacity = compounded + free;

        Rect progress = new Rect(inner.x, y + 3f, inner.width, ProgressBarHeight);
        Widgets.DrawBoxSolid(progress, new Color(0.047f, 0.043f, 0.035f));
        if (capacity > 0f) {
            Widgets.DrawBoxSolid(
                new Rect(progress.x, progress.y, progress.width * Mathf.Clamp01(compounded / capacity), progress.height),
                CompoundTint
            );
        }

        float tinyH = Text.LineHeightOf(GameFont.Tiny);
        Rect line = new Rect(inner.x, progress.yMax + 3f, inner.width, tinyH);

        float rate = feruchemist.CompoundStorePerSecond;
        UIText.EllipsisLabel(
            line,
            "CC_Dock_Allomancy_CompoundRate".Translate(rate.ToString("0.00").Named("RATE")),
            GameFont.Tiny,
            TextAnchor.MiddleLeft,
            CompoundTint
        );

        UIText.EllipsisLabel(
            line,
            RemainingLabel(feruchemist, cell, free, rate),
            GameFont.Tiny,
            TextAnchor.MiddleRight,
            new Color(0.545f, 0.502f, 0.427f)
        );

        return line.yMax;
    }

    /// Compounding ends when the metalmind fills or the reserve runs dry, so the
    /// estimate reports whichever arrives first rather than assuming it fills.
    private static string RemainingLabel(Feruchemist feruchemist, InvestitureCell cell, float free, float rate) {
        float drain = feruchemist.CompoundMetalDrainPerSecond;
        if (rate <= 0f || drain <= 0f) return "";

        float secondsToFull = free / rate;
        float reserve = cell.Bar.Fraction * cell.Bar.Max;
        float secondsToDry = drain > 0f ? reserve / drain : float.MaxValue;

        bool fills = secondsToFull <= secondsToDry;
        float seconds = Mathf.Min(secondsToFull, secondsToDry);
        string period = ((int)(seconds * GenTicks.TicksPerRealSecond)).ToStringTicksToPeriod();

        return (fills ? "CC_Dock_Allomancy_CompoundFull" : "CC_Dock_Allomancy_CompoundDry")
            .Translate(period.Named("TIME"));
    }

    private static Allomancer? FindGene(Pawn pawn, string metalDefName) {
        if (pawn.genes == null) return null;

        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is Allomancer a && a.metal.defName == metalDefName && !a.Overridden) return a;
        }

        return null;
    }

    private string Tooltip(InvestitureCell cell) {
        // Carries its own newline so an idle metal does not leave a blank line.
        string state = cell.IsFlaring
            ? "\n" + "CC_Dock_State_Flaring".Translate()
            : cell.IsActive
                ? "\n" + "CC_Dock_State_Burning".Translate()
                : "";
        return "CC_Dock_Allomancy_Tip".Translate(
            MetalLabel(cell).Named("METAL"),
            Mathf.RoundToInt(cell.Bar.Fraction * 100f).Named("PERCENT"),
            state.Named("STATE")
        );
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

    private static void ToggleBurn(Pawn pawn, string metalDefName, bool flare) {
        if (pawn.abilities == null) return;
        List<Ability> all = pawn.abilities.AllAbilitiesForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is not AllomancyAbility a || a.metal.defName != metalDefName) continue;

            Status next = flare
                ? a.status.power > 1 ? BurningStatus.Burning : BurningStatus.Flaring
                : a.atLeastBurning
                    ? BurningStatus.Off
                    : BurningStatus.Burning;
            a.UpdateStatus(next);
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
