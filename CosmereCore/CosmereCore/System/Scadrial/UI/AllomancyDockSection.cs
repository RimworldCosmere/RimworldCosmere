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

    private static float StripHeight =>
        StripPadding * 2f + Text.LineHeightOf(GameFont.Tiny) + ReserveBarHeight + StripButtonHeight + 14f;

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
        return MetallicArtsTable.HeightFor(GroupsFor(pawn, snapshot), expandedMetal, StripHeight);
    }

    public override void DrawBody(Rect rect, Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        MetallicArtsTable.Draw(
            rect,
            GroupsFor(pawn, snapshot),
            QuadHeader,
            expandedMetal,
            StripHeight,
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
        Allomancer? gene = FindGene(pawn, cell.SubsystemId);
        if (gene == null) return;

        Widgets.DrawBoxSolid(rect, new Color(0.098f, 0.086f, 0.063f));
        Widgets.DrawBoxSolidWithOutline(rect, Color.clear, new Color(0.259f, 0.227f, 0.169f));

        Rect inner = rect.ContractedBy(StripPadding);
        float tinyH = Text.LineHeightOf(GameFont.Tiny);

        string state = cell.IsFlaring
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
            $"{gene.Value:0.00} / {gene.Max:0.00}",
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

        float buttonY = bar.yMax + 8f;
        AllomancyAbility? compound = CompoundingAccess.AbilityFor(pawn, cell.SubsystemId);
        Feruchemist? feruchemist = CompoundingAccess.FeruchemistFor(pawn, cell.SubsystemId);
        bool showCompound = compound != null && feruchemist != null && CompoundingAccess.Discovered(pawn);

        int buttons = showCompound ? 3 : 2;
        float buttonWidth = (inner.width - 5f * (buttons - 1)) / buttons;

        string burnLabel = cell.IsActive
            ? "CC_Dock_Allomancy_StopBurn".Translate()
            : "CC_Dock_Allomancy_Burn".Translate();
        if (DockChrome.Button(new Rect(inner.x, buttonY, buttonWidth, StripButtonHeight), burnLabel, true, Accent)) {
            ToggleBurn(pawn, cell.SubsystemId, false);
            Event.current?.Use();
        }

        Rect flareRect = new Rect(inner.x + buttonWidth + 5f, buttonY, buttonWidth, StripButtonHeight);
        string flareLabel = cell.IsFlaring
            ? "CC_Dock_Allomancy_StopFlare".Translate()
            : "CC_Dock_Allomancy_Flare".Translate();
        if (DockChrome.Button(flareRect, flareLabel, true, Accent)) {
            ToggleBurn(pawn, cell.SubsystemId, true);
            Event.current?.Use();
        }

        if (!showCompound) return;

        AcceptanceReport report = CompoundingAccess.Gate(pawn, feruchemist!, compound!);
        bool compounding = feruchemist!.isCompounding;
        bool canCompound = report.Accepted || compounding;

        Rect compoundRect = new Rect(inner.x + (buttonWidth + 5f) * 2f, buttonY, buttonWidth, StripButtonHeight);
        if (!canCompound) {
            TooltipHandler.TipRegion(
                compoundRect,
                "CC_Dock_Feruchemy_CompoundBlocked".Translate(
                    (report.Reason.NullOrEmpty()
                        ? "CC_Dock_Feruchemy_CompoundUnavailable".Translate().Resolve()
                        : report.Reason).Named("REASON")
                )
            );
        }

        string compoundLabel = compounding
            ? "CC_Dock_Feruchemy_StopStoreCompounded".Translate()
            : "CC_Dock_Feruchemy_StoreCompounded".Translate();
        if (!DockChrome.Button(compoundRect, compoundLabel, canCompound, Accent)) return;

        if (compounding) compound!.UpdateStatus(BurningStatus.Off);
        else compound!.QueueCastingJob(pawn, LocalTargetInfo.Invalid);
        Event.current?.Use();
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
        string state = cell.IsFlaring
            ? "CC_Dock_State_Flaring".Translate()
            : cell.IsActive
                ? "CC_Dock_State_Burning".Translate()
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
