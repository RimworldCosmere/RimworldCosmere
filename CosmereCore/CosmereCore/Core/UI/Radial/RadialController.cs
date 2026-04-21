using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Radial;

public static class RadialController {
    private static RadialWindow? window;
    private static bool wasHeld;

    public static void OnHotkeyPoll() {
        if (Current.ProgramState != ProgramState.Playing) return;
        if (Find.CurrentMap == null) return;

        bool isHeld = RadialKeyBindingDefOf.Cosmere_Keybind_RadialOpen.IsDown;

        if (isHeld && !wasHeld) {
            TryOpen();
        } else if (!isHeld && wasHeld) {
            TryRelease();
        }

        wasHeld = isHeld;
    }

    private static void TryOpen() {
        Pawn? pawn = Dock.InvestitureDockWindow.GetSelectedPawn();
        if (pawn == null) return;
        RadialSnapshot? snap = RadialSnapshotBuilder.Build(pawn);
        if (snap == null) return;

        window = new RadialWindow(snap);
        Find.WindowStack.Add(window);
    }

    private static void TryRelease() {
        if (window == null) return;
        bool flareShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        window.TryCommitOnRelease(flareShift);
        window = null;
    }
}
