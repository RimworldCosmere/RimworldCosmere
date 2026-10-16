using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Radial;

public static class RadialController {
    private const float HoldThresholdSeconds = 0.25f;

    private static RadialWindow? window;
    private static bool wasHeld;
    private static float pressedAt;

    public static void OnHotkeyPoll() {
        if (Event.current?.type != EventType.Repaint) return;
        if (Current.ProgramState != ProgramState.Playing) return;
        if (Find.CurrentMap == null) return;

        if (window != null && !Find.WindowStack.IsOpen(window)) window = null;

        // gizmo hotkey opens the wheel; this just times the press and decides tap vs hold on release.
        bool isHeld = RadialKeyBindingDefOf.Cosmere_Keybind_RadialOpen.IsDown;

        if (isHeld && !wasHeld) pressedAt = Time.realtimeSinceStartup;
        else if (!isHeld && wasHeld) OnKeyUp();

        wasHeld = isHeld;
    }

    private static void OnKeyUp() {
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

    /// <summary>
    ///     Opens the wheel, or closes it if already open. A mouse click has no release to cast on, so
    ///     it lands in browse mode; a keypress starts in quick mode and the release decides if it stays open.
    /// </summary>
    public static void ToggleForPawn(Pawn pawn) {
        if (window != null) {
            window.Close(false);
            window = null;
            return;
        }

        RadialSnapshot? snap = RadialSnapshotBuilder.Build(pawn);
        if (snap == null) return;

        bool viaHotkey = RadialKeyBindingDefOf.Cosmere_Keybind_RadialOpen.IsDown;
        window = new RadialWindow(snap, anchorOnPawn: !viaHotkey) { BrowseMode = !viaHotkey };
        Find.WindowStack.Add(window);
    }

    /// <summary>
    ///     Building a snapshot walks every provider and allocates, and GetGizmos runs each frame, so
    ///     the answer is cached briefly per pawn.
    /// </summary>
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
