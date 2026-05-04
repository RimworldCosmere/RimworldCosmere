using Cosmere.Core.UI.Dock;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Radial;

public static class RadialController {
    private static RadialWindow? window;
    private static bool wasHeld;

    public static void OnHotkeyPoll() {
        if (Event.current?.type != EventType.Repaint) return;
        if (Current.ProgramState != ProgramState.Playing) return;
        if (Find.CurrentMap == null) return;

        bool isHeld = RadialKeyBindingDefOf.Cosmere_Keybind_RadialOpen.IsDown;

        if (isHeld && !wasHeld) {
            TryOpen();
        }
        else if (!isHeld && wasHeld) {
            TryRelease();
        }

        wasHeld = isHeld;
    }

    private static void TryOpen() {
        if (window != null) {
            window.Close(false);
            window = null;
        }

        Pawn? pawn = GetRadialTargetPawn();
        if (pawn == null) return;
        RadialSnapshot? snap = RadialSnapshotBuilder.Build(pawn);
        if (snap == null) return;

        window = new RadialWindow(snap);
        Find.WindowStack.Add(window);
    }

    private static Pawn? GetRadialTargetPawn() => InvestitureDockWindow.GetSelectedPawn();

    private static void TryRelease() {
        if (window == null) return;
        bool flareShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        window.TryCommitOnRelease(flareShift);
        window = null;
    }
}