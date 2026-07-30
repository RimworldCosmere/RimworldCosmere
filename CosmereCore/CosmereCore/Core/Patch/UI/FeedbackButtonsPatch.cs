using System;
using Concord;
using Cosmere.Core.BetaHub;
using Cosmere.Core.Window;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.Patch;

[Patch]
public abstract class FeedbackButtonsPatch : UIRoot_Play {
    private static readonly Color PlateTop = new Color(0.23f, 0.20f, 0.16f);
    private static readonly Color PlateBottom = new Color(0.17f, 0.15f, 0.13f);
    private static readonly Color Accent = new Color(0.85f, 0.75f, 0.48f);
    private static readonly Color LabelColor = new Color(0.87f, 0.84f, 0.75f);

    [Inject(At.Return, nameof(UIRootOnGUI))]
    private void DrawFeedbackButtons() {
        if (Current.ProgramState != ProgramState.Playing) return;
        if (Find.CurrentMap == null) return;

        ScreenshotCapture.TryCaptureNow();

        if (!BetaHubConfig.ShouldShowFeedbackUi) return;
        if (Find.WindowStack.IsOpen<FeedbackDialog>()) return;

        float x = FeedbackButtonLayout.ButtonX(Verse.UI.screenWidth, LearningReadoutVisible());

        if (DrawPlate(x, FeedbackButtonLayout.ButtonY(0), "CC_BetaHub_ReportBug", "CC_BetaHub_ReportBug_Tip")) {
            FeedbackDialog.Open(FeedbackKind.Bug);
        }

        if (DrawPlate(x, FeedbackButtonLayout.ButtonY(1), "CC_BetaHub_Suggest", "CC_BetaHub_Suggest_Tip")) {
            FeedbackDialog.Open(FeedbackKind.Suggestion);
        }
    }

    private static bool DrawPlate(float x, float y, string labelKey, string tipKey) {
        Rect rect = new Rect(x, y, FeedbackButtonLayout.ButtonWidth, FeedbackButtonLayout.ButtonHeight);

        Widgets.DrawBoxSolid(rect, Mouse.IsOver(rect) ? PlateTop : PlateBottom);
        Widgets.DrawBoxSolid(new Rect(rect.x, rect.y, 3f, rect.height), Accent);
        Widgets.DrawHighlightIfMouseover(rect);
        TooltipHandler.TipRegion(rect, tipKey.Translate());
        MouseoverSounds.DoRegion(rect, RimWorld.SoundDefOf.Mouseover_Standard);

        Rect labelRect = new Rect(rect.x + 12f, rect.y, rect.width - 16f, rect.height);
        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, LabelColor)) {
            Widgets.Label(labelRect, labelKey.Translate());
        }

        return Widgets.ButtonInvisible(rect);
    }

    // Mirrors the condition in LearningReadout.LearningReadoutOnGUI.
    private static bool LearningReadoutVisible() {
        try {
            if (TutorSystem.TutorialMode || !TutorSystem.AdaptiveTrainingEnabled) return false;
            if (Find.PlaySettings.showLearningHelper) return true;

            LearningReadout? readout = Find.Tutor?.learningReadout;

            return readout != null && readout.ActiveConceptsCount != 0;
        } catch (Exception) {
            return false;
        }
    }
}
