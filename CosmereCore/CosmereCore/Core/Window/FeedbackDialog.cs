using Cosmere.Core.BetaHub;
using Cosmere.Core.Listing;
using Cosmere.Core.Settings;
using Cosmere.Core.UI;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Window;

public sealed class FeedbackDialog : BaseWindow {
    private static readonly Color CounterColor = new Color(0.80f, 0.62f, 0.35f);

    private readonly FeedbackReport report;
    private readonly byte[]? screenshot;

    private bool sending;
    private SubmitResult? result;
    private Texture2D? thumbnail;

    private FeedbackDialog(FeedbackKind kind, byte[]? screenshot)
        : base(new Vector2(620f, 640f)) {
        this.screenshot = screenshot;
        report = new FeedbackReport {
            Kind = kind,
            IncludeScreenshot = kind == FeedbackKind.Bug && screenshot != null,
            DiscordUsername = Mod.GetModSettings<CoreModSettings>().feedbackDiscordUsername,
        };
        hasFooter = true;
        drawBorder = true;
    }

    public static void Open(FeedbackKind kind) {
        if (kind == FeedbackKind.Suggestion) {
            Find.WindowStack.Add(new FeedbackDialog(kind, null));
            return;
        }

        ScreenshotCapture.RequestCapture(jpeg => Find.WindowStack.Add(new FeedbackDialog(kind, jpeg)));
    }

    public override void PreClose() {
        base.PreClose();

        if (thumbnail == null) return;

        Object.Destroy(thumbnail);
        thumbnail = null;
    }

    protected override TaggedString GetTitle() {
        return report.Kind == FeedbackKind.Bug
            ? "CC_BetaHub_Dialog_BugTitle".Translate()
            : "CC_BetaHub_Dialog_SuggestTitle".Translate();
    }

    protected override TaggedString? GetSubtitle() {
        return report.Kind == FeedbackKind.Bug
            ? "CC_BetaHub_Dialog_BugSubtitle".Translate()
            : "CC_BetaHub_Dialog_SuggestSubtitle".Translate();
    }

    protected override void DrawBodyContent(FoundationListing listing) {
        if (result != null) return;

        listing.Label("CC_BetaHub_Field_Title".Translate());
        report.Title = Widgets.TextField(listing.GetRect(Spacing.Get(1.75)), report.Title);
        listing.Gap(Spacing.Get(0.5));

        listing.Label(
            report.Kind == FeedbackKind.Bug
                ? "CC_BetaHub_Field_Description".Translate()
                : "CC_BetaHub_Field_Idea".Translate()
        );
        report.Description = Widgets.TextArea(listing.GetRect(Spacing.Get(7)), report.Description);

        int needed = FeedbackValidator.RemainingCharacters(report.Kind, report.Description);
        if (needed > 0) {
            using (new TextBlock(GameFont.Tiny, CounterColor)) {
                listing.Label("CC_BetaHub_CharsNeeded".Translate(needed.Named("COUNT")));
            }
        }

        listing.Gap(Spacing.Get(0.5));

        if (report.Kind == FeedbackKind.Bug) {
            listing.Label("CC_BetaHub_Field_Steps".Translate());
            report.StepsToReproduce = Widgets.TextArea(listing.GetRect(Spacing.Get(4)), report.StepsToReproduce);
            listing.Gap(Spacing.Get(0.5));
        }

        listing.Label("CC_BetaHub_Field_Mod".Translate());
        DrawTargetRow(listing, FeedbackTarget.Unknown, "CC_BetaHub_Mod_Unknown");
        DrawTargetRow(listing, FeedbackTarget.Core, "CC_BetaHub_Mod_Core");
        DrawTargetRow(listing, FeedbackTarget.Scadrial, "CC_BetaHub_Mod_Scadrial");
        DrawTargetRow(listing, FeedbackTarget.Roshar, "CC_BetaHub_Mod_Roshar");
        listing.Gap(Spacing.Get(0.5));

        listing.Label("CC_BetaHub_Field_Discord".Translate());
        Rect discordRect = listing.GetRect(Spacing.Get(1.75));
        TooltipHandler.TipRegion(discordRect, "CC_BetaHub_Field_DiscordTip".Translate());
        report.DiscordUsername = Widgets.TextField(discordRect, report.DiscordUsername ?? string.Empty);

        if (report.Kind != FeedbackKind.Bug || screenshot == null) return;

        listing.Gap(Spacing.Get(0.5));
        Rect shotRect = listing.GetRect(Spacing.Get(1.75));
        bool include = report.IncludeScreenshot;
        Widgets.CheckboxLabeled(shotRect, "CC_BetaHub_Field_IncludeScreenshot".Translate(), ref include);
        report.IncludeScreenshot = include;

        thumbnail ??= BuildThumbnail();
        if (thumbnail != null) {
            GUI.DrawTexture(listing.GetRect(Spacing.Get(7)), thumbnail, ScaleMode.ScaleToFit);
        }
    }

    protected override void DrawFooter(Rect rect) {
        Rect inner = rect.ContractedBy(Spacing.Get(0.5));
        float buttonWidth = Spacing.Get(8);

        if (result != null) {
            DrawResultFooter(inner, buttonWidth);
            return;
        }

        if (Widgets.ButtonText(inner.LeftPartPixels(buttonWidth), "CC_BetaHub_Cancel".Translate())) {
            Close();
        }

        bool ready = !sending && FeedbackValidator.IsSubmittable(report.Kind, report.Description);
        Rect sendRect = inner.RightPartPixels(buttonWidth);
        string sendLabel = sending ? "CC_BetaHub_Sending".Translate() : "CC_BetaHub_Submit".Translate();

        if (Widgets.ButtonText(sendRect, sendLabel, active: ready)) {
            Submit();
        }
    }

    private void DrawTargetRow(FoundationListing listing, FeedbackTarget target, string labelKey) {
        Rect rect = listing.GetRect(Spacing.Get(1.5));
        Widgets.DrawHighlightIfMouseover(rect);

        if (Widgets.RadioButtonLabeled(rect, labelKey.Translate(), report.Target == target)) {
            report.Target = target;
        }
    }

    private Texture2D? BuildThumbnail() {
        if (screenshot == null) return null;

        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGB24, false);

        return texture.LoadImage(screenshot) ? texture : null;
    }

    private void DrawResultFooter(Rect inner, float buttonWidth) {
        SubmitResult finished = result!;
        string message = BetaHubStatusMapper.MessageKey(finished.Outcome)
            .Translate((finished.ServerMessage ?? string.Empty).Named("REASON"));

        using (new TextBlock(TextAnchor.MiddleLeft, BodyTextColor)) {
            Widgets.Label(inner.LeftPartPixels(inner.width - buttonWidth - Spacing.Get(0.5)), message);
        }

        Rect actionRect = inner.RightPartPixels(buttonWidth);

        if (finished.Outcome == SubmitOutcome.Success) {
            if (Widgets.ButtonText(actionRect, "CC_BetaHub_ViewOnline".Translate())) {
                if (!string.IsNullOrEmpty(finished.IssueUrl)) Application.OpenURL(finished.IssueUrl!);
                Close();
            }

            return;
        }

        if (Widgets.ButtonText(actionRect, "CC_BetaHub_Retry".Translate())) {
            result = null;
        }
    }

    private void Submit() {
        sending = true;
        Mod.GetModSettings<CoreModSettings>().feedbackDiscordUsername = report.DiscordUsername;

        BetaHubClient.Submit(
            report,
            report.IncludeScreenshot ? screenshot : null,
            outcome => {
                sending = false;
                result = outcome;
            }
        );
    }
}
