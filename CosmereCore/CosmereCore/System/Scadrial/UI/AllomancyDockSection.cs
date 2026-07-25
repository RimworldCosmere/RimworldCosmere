using Cosmere.Core.Ability;
using Cosmere.Core.UI.Dock;
using Cosmere.Core.UI.Model;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI;

public sealed class AllomancyDockSection : DockSectionBase {
    private static readonly Color BurningTint = new Color(0.498f, 0.714f, 0.847f);
    private static readonly Color FlaringTint = new Color(0.878f, 0.416f, 0.271f);
    private static readonly Color QuadHeader = new Color(0.490f, 0.384f, 0.259f);

    private readonly Dictionary<string, string> labelCache = new Dictionary<string, string>();
    private IReadOnlyList<MetalGroup>? cachedGroups;
    private int cachedPawnId = -1;
    private int cachedCellCount = -1;

    public override string SystemId => "Allomancy";

    public override float GetHeaderHeight() {
        return 28f;
    }

    public override float GetExpandedBodyHeight(Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        return MetallicArtsTable.HeightFor(GroupsFor(pawn, snapshot), null, 0f);
    }

    public override void DrawBody(Rect rect, Pawn pawn, InvestitureSnapshot snapshot, DockRenderContext ctx) {
        MetallicArtsTable.Draw(
            rect,
            GroupsFor(pawn, snapshot),
            QuadHeader,
            null,
            0f,
            (tileRect, row) => DrawTile(tileRect, pawn, row),
            null
        );
    }

    private void DrawTile(Rect rect, Pawn pawn, MetalRow row) {
        InvestitureCell cell = row.Cell;
        MetalTileState state = cell.IsActive ? MetalTileState.Active : MetalTileState.Idle;

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

        Event current = Event.current;
        if (current != null && current.button == 1) {
            OpenThreshold(pawn, cell);
            current.Use();
            return;
        }

        ToggleBurn(pawn, cell.SubsystemId, current != null && current.shift);
        current?.Use();
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

            Status next = a.atLeastBurning
                ? BurningStatus.Off
                : flare
                    ? BurningStatus.Flaring
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
