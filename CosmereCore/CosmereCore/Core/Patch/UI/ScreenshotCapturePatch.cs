using Concord;
using Cosmere.Core.BetaHub;
using RimWorld;
using Verse;

namespace Cosmere.Core.Patch;

/// <summary>
///     Services a pending feedback screenshot at the end of the GUI pass.
/// </summary>
/// <remarks>
///     Deliberately not folded into FeedbackButtonsPatch. The buttons have to be hit-tested
///     early, before the mouse event is consumed, but the frame grab has to happen last or it
///     captures a half-composited UI.
/// </remarks>
[Patch]
public abstract class ScreenshotCapturePatch : UIRoot_Play {
    [Inject(At.Return, nameof(UIRootOnGUI))]
    private void CapturePendingScreenshot() {
        if (Current.ProgramState != ProgramState.Playing) return;
        if (Find.CurrentMap == null) return;

        ScreenshotCapture.TryCaptureNow();
    }
}
