using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Transformation;

public class PausingFloatMenu : FloatMenu {
    private static TimeSpeed? savedSpeed;
    private static int openCount;

    public PausingFloatMenu(List<FloatMenuOption> options) : base(options) {
        PauseAndCount();
    }

    public override void PostClose() {
        base.PostClose();
        ReleasePause();
    }

    public static void KeepPaused() {
        openCount++;
    }

    private static void PauseAndCount() {
        savedSpeed ??= Find.TickManager.CurTimeSpeed;
        Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
        openCount++;
    }

    public static void ReleasePause() {
        openCount--;
        if (openCount <= 0) {
            openCount = 0;
            RestoreSpeed();
        }
    }

    private static void RestoreSpeed() {
        if (savedSpeed.HasValue) {
            Find.TickManager.CurTimeSpeed = savedSpeed.Value;
            savedSpeed = null;
        }
    }
}
