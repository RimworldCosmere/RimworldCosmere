using Cosmere.System.Scadrial.Comp.Map;
using Cosmere.System.Scadrial.Grid;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Dev;

/// <summary>
///     Dev overlay for the ash grid: heat-ramped cell highlight plus the depth in millimetres on
///     each tile. Judging ash by eye is guesswork, and the grid already knows the answer.
/// </summary>
public class AshOverlayDrawer : MapComponent {
    private static readonly Color Shallow = new Color(0.35f, 0.75f, 1f);
    private static readonly Color Mid = new Color(1f, 0.85f, 0.3f);
    private static readonly Color Deep = new Color(1f, 0.3f, 0.25f);

    private CellBoolDrawer? drawer;

    public AshOverlayDrawer(Verse.Map map) : base(map) { }

    public static bool Enabled { get; set; }

    private CellBoolDrawer Drawer =>
        drawer ??= new CellBoolDrawer(
            HasAsh,
            () => Color.white,
            ColorForIndex,
            map.Size.x,
            map.Size.z,
            0.5f
        );

    public override void MapComponentUpdate() {
        if (!Enabled) return;

        Drawer.MarkForDraw();
        Drawer.CellBoolDrawerUpdate();
    }

    public override void MapComponentOnGUI() {
        if (!Enabled) return;
        if (Find.CameraDriver.CurrentZoom > CameraZoomRange.Close) return;

        AshDepthTracker? tracker = map.GetComponent<AshDepthTracker>();
        if (tracker == null) return;

        AshGrid grid = tracker.Grid;
        CellIndices indices = map.cellIndices;
        CellRect view = Find.CameraDriver.CurrentViewRect;
        view.ClipInsideMap(map);

        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter)) {
            foreach (IntVec3 cell in view) {
                int mm = grid.GetDepthMm(indices.CellToIndex(cell));
                if (mm <= 0) continue;

                GenMapUI.DrawThingLabel(GenMapUI.LabelDrawPosFor(cell), mm.ToString(), ColorForDepth(mm));
            }
        }
    }

    /// <summary>Called whenever the grid changes so the overlay does not lag behind the ash.</summary>
    public void SetDirty() {
        drawer?.SetDirty();
    }

    private bool HasAsh(int index) {
        return map.GetComponent<AshDepthTracker>()?.Grid.GetDepthMm(index) > 0;
    }

    private Color ColorForIndex(int index) {
        AshDepthTracker? tracker = map.GetComponent<AshDepthTracker>();
        return tracker == null ? Color.clear : ColorForDepth(tracker.Grid.GetDepthMm(index));
    }

    private static Color ColorForDepth(int millimetres) {
        float t = Mathf.Clamp01(millimetres / (float)AshGrid.MaxDepthMm);
        return t < 0.5f ? Color.Lerp(Shallow, Mid, t * 2f) : Color.Lerp(Mid, Deep, (t - 0.5f) * 2f);
    }
}

public static class AshOverlayDebugActions {
    [DebugAction("Cosmere", "Ash: toggle depth overlay", allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void ToggleOverlay() {
        AshOverlayDrawer.Enabled = !AshOverlayDrawer.Enabled;
        Messages.Message(
            AshOverlayDrawer.Enabled ? "Ash depth overlay ON" : "Ash depth overlay OFF",
            MessageTypeDefOf.SilentInput,
            false
        );
    }
}
