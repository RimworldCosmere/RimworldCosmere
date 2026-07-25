using Cosmere.Core.UI.Dock;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Radial;

public static class RadialController {
    private const float HoldThresholdSeconds = 0.25f;

    private static RadialWindow? window;
    private static bool wasHeld;
    private static float pressedAt;
    private static bool suppressReopen;

    public static void OnHotkeyPoll() {
        if (Event.current?.type != EventType.Repaint) return;
        if (Current.ProgramState != ProgramState.Playing) return;
        if (Find.CurrentMap == null) return;

        if (window != null && !Find.WindowStack.IsOpen(window)) window = null;

        bool isHeld = RadialKeyBindingDefOf.Cosmere_Keybind_RadialOpen.IsDown;

        if (isHeld && !wasHeld) OnKeyDown();
        else if (!isHeld && wasHeld) OnKeyUp();

        wasHeld = isHeld;
    }

    private static void OnKeyDown() {
        if (window != null) {
            window.Close(false);
            window = null;
            suppressReopen = true;
            return;
        }

        suppressReopen = false;
        Pawn? pawn = InvestitureDockWindow.GetSelectedPawn();
        if (pawn == null) return;
        RadialSnapshot? snap = RadialSnapshotBuilder.Build(pawn);
        if (snap == null) return;

        pressedAt = Time.realtimeSinceStartup;
        window = new RadialWindow(snap);
        Find.WindowStack.Add(window);
    }

    private static void OnKeyUp() {
        if (suppressReopen) {
            suppressReopen = false;
            return;
        }

        if (window == null) return;

        float heldFor = Time.realtimeSinceStartup - pressedAt;
        if (heldFor < HoldThresholdSeconds) {
            window.BrowseMode = true;
            return;
        }

        bool flareShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        window.TryCommitOnRelease(flareShift);
        window = null;
    }

    /// Opens the wheel for a pawn without the hotkey, so it starts in browse
    /// mode: a gizmo click has no release to cast on.
    public static void OpenForPawn(Pawn pawn) {
        if (window != null) {
            window.Close(false);
            window = null;
        }

        RadialSnapshot? snap = RadialSnapshotBuilder.Build(pawn);
        if (snap == null) return;

        window = new RadialWindow(snap) { BrowseMode = true };
        Find.WindowStack.Add(window);
    }

    /// Building a snapshot walks every provider and allocates, and GetGizmos
    /// runs each frame, so the answer is cached briefly per pawn.
    private const int RadialCheckIntervalTicks = 60;
    private static int cachedRadialPawnId = -1;
    private static int cachedRadialTick = -1;
    private static bool cachedHasRadial;

    public static bool HasRadialFor(Pawn pawn) {
        int tick = Find.TickManager?.TicksGame ?? 0;
        if (cachedRadialPawnId == pawn.thingIDNumber && tick - cachedRadialTick < RadialCheckIntervalTicks) {
            return cachedHasRadial;
        }

        cachedRadialPawnId = pawn.thingIDNumber;
        cachedRadialTick = tick;
        cachedHasRadial = RadialSnapshotBuilder.Build(pawn) != null;
        return cachedHasRadial;
    }

    public static void NotifyClosed(RadialWindow closing) {
        if (window == closing) window = null;
    }
}
