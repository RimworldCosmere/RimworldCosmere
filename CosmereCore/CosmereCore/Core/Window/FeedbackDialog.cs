using Cosmere.Core.BetaHub;
using Cosmere.Core.Listing;
using Cosmere.Core.Settings;
using Cosmere.Core.UI;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Window;

public sealed class FeedbackDialog : BaseWindow {
    private static readonly Color CounterColor = new Color(0.80f, 0.62f, 0.35f);
    private static readonly Color SuccessColor = new Color(0.55f, 0.75f, 0.52f);
    private static readonly Color FailureColor = new Color(0.82f, 0.47f, 0.42f);

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
        if (result != null) {
            DrawResultBody(listing);

            return;
        }

        // The client holds the report by reference while a submit is in flight, so editing
        // during a send would mutate the payload already on its way out.
        bool locked = sending;

        listing.Label("CC_BetaHub_Field_Title".Translate());
        report.Title = DrawField(listing, report.Title, locked);
        listing.Gap(Spacing.Get(0.5));

        listing.Label(
            report.Kind == FeedbackKind.Bug
                ? "CC_BetaHub_Field_Description".Translate()
                : "CC_BetaHub_Field_Idea".Translate()
        );
        report.Description = DrawArea(listing, report.Description, Spacing.Get(7), locked);

        int needed = FeedbackValidator.RemainingCharacters(report.Kind, report.Description);
        if (needed > 0) {
            using (new TextBlock(GameFont.Tiny, CounterColor)) {
                listing.Label("CC_BetaHub_CharsNeeded".Translate(needed.Named("COUNT")));
            }
        }

        listing.Gap(Spacing.Get(0.5));

        if (report.Kind == FeedbackKind.Bug) {
            listing.Label("CC_BetaHub_Field_Steps".Translate());
            report.StepsToReproduce = DrawArea(listing, report.StepsToReproduce, Spacing.Get(4), locked);
            listing.Gap(Spacing.Get(0.5));
        }

        listing.Label("CC_BetaHub_Field_Mod".Translate());
        DrawTargetRow(listing, FeedbackTarget.Unknown, "CC_BetaHub_Mod_Unknown", locked);
        DrawTargetRow(listing, FeedbackTarget.Core, "CC_BetaHub_Mod_Core", locked);
        DrawTargetRow(listing, FeedbackTarget.Scadrial, "CC_BetaHub_Mod_Scadrial", locked);
        DrawTargetRow(listing, FeedbackTarget.Roshar, "CC_BetaHub_Mod_Roshar", locked);
        listing.Gap(Spacing.Get(0.5));

        listing.Label("CC_BetaHub_Field_Discord".Translate());
        Rect discordRect = listing.GetRect(Spacing.Get(1.75));
        TooltipHandler.TipRegion(discordRect, "CC_BetaHub_Field_DiscordTip".Translate());
        report.DiscordUsername = locked
            ? report.DiscordUsername
            : Widgets.TextField(discordRect, report.DiscordUsername ?? string.Empty);

        if (locked) Widgets.Label(discordRect, report.DiscordUsername ?? string.Empty);

        if (report.Kind != FeedbackKind.Bug || screenshot == null) return;

        listing.Gap(Spacing.Get(0.5));
        Rect shotRect = listing.GetRect(Spacing.Get(1.75));
        bool include = report.IncludeScreenshot;
        Widgets.CheckboxLabeled(shotRect, "CC_BetaHub_Field_IncludeScreenshot".Translate(), ref include, locked);
        report.IncludeScreenshot = include;

        thumbnail ??= BuildThumbnail();
        if (thumbnail != null) {
            GUI.DrawTexture(listing.GetRect(Spacing.Get(7)), thumbnail, ScaleMode.ScaleToFit);
        }
    }

    /// <summary>
    ///     Replaces the form once a submit has landed, so the window is never a tall empty box
    ///     with one line of footer text.
    /// </summary>
    private void DrawResultBody(FoundationListing listing) {
        SubmitResult finished = result!;
        bool ok = finished.Outcome == SubmitOutcome.Success;

        listing.Gap(Spacing.Get(2));

        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleCenter, ok ? SuccessColor : FailureColor)) {
            listing.Label(BetaHubStatusMapper.MessageKey(finished.Outcome)
                .Translate((finished.ServerMessage ?? string.Empty).Named("REASON")));
        }

        if (ok || string.IsNullOrEmpty(finished.ServerMessage)) return;

        listing.Gap(Spacing.Get(0.5));

        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter, BodyTextColor)) {
            listing.Label(finished.ServerMessage!);
        }
    }

    private static string DrawField(FoundationListing listing, string value, bool locked) {
        Rect rect = listing.GetRect(Spacing.Get(1.75));
        if (!locked) return Widgets.TextField(rect, value);

        Widgets.Label(rect, value);

        return value;
    }

    private static string DrawArea(FoundationListing listing, string value, float height, bool locked) {
        Rect rect = listing.GetRect(height);
        if (!locked) return Widgets.TextArea(rect, value);

        Widgets.Label(rect, value);

        return value;
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

    private void DrawTargetRow(FoundationListing listing, FeedbackTarget target, string labelKey, bool locked) {
        Rect rect = listing.GetRect(Spacing.Get(1.5));
        if (!locked) Widgets.DrawHighlightIfMouseover(rect);

        bool selected = report.Target == target;
        if (Widgets.RadioButtonLabeled(rect, labelKey.Translate(), selected) && !locked) {
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
        Rect actionRect = inner.RightPartPixels(buttonWidth);

        if (Widgets.ButtonText(inner.LeftPartPixels(buttonWidth), "CC_BetaHub_Cancel".Translate())) {
            Close();
        }

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
