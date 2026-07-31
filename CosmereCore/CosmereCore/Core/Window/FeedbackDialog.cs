using Cosmere.Core.BetaHub;
using Cosmere.Core.Listing;
using Cosmere.Core.Settings;
using Cosmere.Core.UI;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.Window;

public sealed class FeedbackDialog : BaseWindow {
    private static readonly Color CounterColor = new Color(0.80f, 0.62f, 0.35f);
    private static readonly Color SuccessColor = new Color(0.55f, 0.75f, 0.52f);
    private static readonly Color FailureColor = new Color(0.82f, 0.47f, 0.42f);
    private static readonly Color SegmentOnColor = new Color(0.29f, 0.24f, 0.14f);
    private static readonly Color SegmentOffColor = new Color(0.11f, 0.11f, 0.10f);
    private static readonly Color SegmentOnText = new Color(0.94f, 0.90f, 0.82f);
    private static readonly Color ErrorColor = new Color(0.85f, 0.35f, 0.32f);
    private static readonly Texture2D ErrorBorderTexture = new Color(0.85f, 0.35f, 0.32f).ToSolidColorTexture();
    private static readonly Texture2D SegmentEdgeTexture = new Color(0.30f, 0.26f, 0.20f).ToSolidColorTexture();

    private readonly FeedbackReport report;
    private readonly byte[]? screenshot;

    private bool sending;
    private bool validationAttempted;
    private SubmitResult? result;
    private Texture2D? thumbnail;

    private FeedbackDialog(FeedbackKind kind, byte[]? screenshot)
        : base(new Vector2(620f, kind == FeedbackKind.Bug ? 760f : 540f)) {
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

    protected override bool drawHeaderSeparator => false;

    protected override bool drawFooterSeparator => false;

    protected override float bodyInset => Spacing.Get(0.375);

    protected override float bodyPadding => Spacing.Get(2.5);

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
        // bodyPadding only indents horizontally, so the first label would otherwise sit flush
        // against the header.
        listing.Gap(Spacing.Get(1));

        if (result != null) {
            DrawResultBody(listing);

            return;
        }

        // The client holds the report by reference while a submit is in flight, so editing
        // during a send would mutate the payload already on its way out.
        bool locked = sending;

        listing.Label("CC_BetaHub_Field_Title".Translate());
        Rect titleRect = listing.GetRect(Spacing.Get(1.75));
        if (locked) {
            Widgets.Label(titleRect, report.Title);
        } else {
            report.Title = Widgets.TextField(titleRect, report.Title);
        }

        MarkIfMissing(listing, titleRect, report.Title);
        listing.Gap(Spacing.Get(0.5));

        listing.Label(
            report.Kind == FeedbackKind.Bug
                ? "CC_BetaHub_Field_Description".Translate()
                : "CC_BetaHub_Field_Idea".Translate()
        );

        Rect descriptionRect = listing.GetRect(Spacing.Get(7));
        if (locked) {
            Widgets.Label(descriptionRect, report.Description);
        } else {
            report.Description = Widgets.TextArea(descriptionRect, report.Description);
        }

        int needed = FeedbackValidator.RemainingCharacters(report.Kind, report.Description);
        bool descriptionInvalid = validationAttempted && needed > 0;

        if (descriptionInvalid) Widgets.DrawBox(descriptionRect, 1, ErrorBorderTexture);

        if (descriptionInvalid) {
            using (new TextBlock(GameFont.Tiny, ErrorColor)) {
                listing.Label(
                    string.IsNullOrWhiteSpace(report.Description)
                        ? "CC_BetaHub_Field_Required".Translate()
                        : "CC_BetaHub_CharsNeeded".Translate(needed.Named("COUNT"))
                );
            }
        } else if (needed > 0) {
            using (new TextBlock(GameFont.Tiny, CounterColor)) {
                listing.Label("CC_BetaHub_CharsNeeded".Translate(needed.Named("COUNT")));
            }
        }

        listing.Gap(Spacing.Get(0.5));

        if (report.Kind == FeedbackKind.Bug) {
            listing.Label("CC_BetaHub_Field_Steps".Translate());
            Rect stepsRect = listing.GetRect(Spacing.Get(4));
            if (locked) {
                Widgets.Label(stepsRect, report.StepsToReproduce);
            } else {
                report.StepsToReproduce = Widgets.TextArea(stepsRect, report.StepsToReproduce);
            }

            MarkIfMissing(listing, stepsRect, report.StepsToReproduce);
            listing.Gap(Spacing.Get(0.5));
        }

        listing.Label("CC_BetaHub_Field_Mod".Translate());
        DrawTargetSegments(listing, locked);
        listing.Gap(Spacing.Get(0.5));

        listing.Label("CC_BetaHub_Field_Discord".Translate());
        Rect discordRect = listing.GetRect(Spacing.Get(1.75));
        TooltipHandler.TipRegion(discordRect, "CC_BetaHub_Field_DiscordTip".Translate());
        report.DiscordUsername = locked
            ? report.DiscordUsername
            : Widgets.TextField(discordRect, report.DiscordUsername ?? string.Empty);

        if (locked) Widgets.Label(discordRect, report.DiscordUsername ?? string.Empty);

        listing.Gap(Spacing.Get(0.5));
        listing.Label("CC_BetaHub_Field_Video".Translate());
        Rect videoRect = listing.GetRect(Spacing.Get(1.75));
        TooltipHandler.TipRegion(videoRect, "CC_BetaHub_Field_VideoTip".Translate());
        report.VideoUrl = locked
            ? report.VideoUrl
            : Widgets.TextField(videoRect, report.VideoUrl ?? string.Empty);

        if (locked) Widgets.Label(videoRect, report.VideoUrl ?? string.Empty);

        if (report.Kind != FeedbackKind.Bug || screenshot == null) return;

        listing.Gap(Spacing.Get(0.5));
        Rect shotRect = listing.GetRect(Spacing.Get(1.75));
        bool include = report.IncludeScreenshot;
        Widgets.CheckboxLabeled(shotRect, "CC_BetaHub_Field_IncludeScreenshot".Translate(), ref include, locked);
        report.IncludeScreenshot = include;

        thumbnail ??= BuildThumbnail();
        if (thumbnail != null) {
            GUI.DrawTexture(listing.GetRect(Spacing.Get(5)), thumbnail, ScaleMode.ScaleToFit);
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

        listing.Gap(Spacing.Get(0.5));

        if (ok) {
            if (string.IsNullOrEmpty(finished.IssueUrl)) return;

            // Printed rather than left behind the button: OpenURL reaches no browser in a
            // container or on a locked-down machine, and the player still needs the address.
            using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter, BodyTextColor)) {
                listing.Label(finished.IssueUrl!);
            }

            return;
        }

        if (string.IsNullOrEmpty(finished.ServerMessage)) return;

        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter, BodyTextColor)) {
            listing.Label(finished.ServerMessage!);
        }
    }

    /// <summary>
    ///     Red border plus a red note, in the same slot the character counter uses.
    /// </summary>
    private void MarkIfMissing(FoundationListing listing, Rect rect, string value) {
        if (!validationAttempted || !string.IsNullOrWhiteSpace(value)) return;

        Widgets.DrawBox(rect, 1, ErrorBorderTexture);

        using (new TextBlock(GameFont.Tiny, ErrorColor)) {
            listing.Label("CC_BetaHub_Field_Required".Translate());
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

        Rect sendRect = inner.RightPartPixels(buttonWidth);
        string sendLabel = sending ? "CC_BetaHub_Sending".Translate() : "CC_BetaHub_Submit".Translate();

        // Send stays enabled when the form is incomplete so the click can say what is missing.
        // A disabled button gives the player nothing to act on.
        if (Widgets.ButtonText(sendRect, sendLabel, active: !sending)) {
            if (FeedbackValidator.IsComplete(report.Kind, report.Title, report.Description, report.StepsToReproduce)) {
                Submit();
            } else {
                validationAttempted = true;
                RimWorld.SoundDefOf.ClickReject.PlayOneShotOnCamera();
            }
        }
    }

    /// <summary>
///     Four options in one row rather than four stacked rows.
    /// </summary>
    /// <remarks>
    ///     RadioButtonLabeled spans the whole listing width, which stranded each circle about
    ///     700px from its own label. A segment row also buys back three rows of height, which
    ///     is what was pushing the screenshot preview below the fold.
    /// </remarks>
    private void DrawTargetSegments(FoundationListing listing, bool locked) {
        Rect row = listing.GetRect(Spacing.Get(1.75));
        const float gap = 4f;
        float width = (row.width - gap * 3f) / 4f;

        DrawSegment(new Rect(row.x, row.y, width, row.height), FeedbackTarget.Unknown, "CC_BetaHub_Mod_Unknown", locked);
        DrawSegment(new Rect(row.x + width + gap, row.y, width, row.height), FeedbackTarget.Core, "CC_BetaHub_Mod_Core", locked);
        DrawSegment(new Rect(row.x + (width + gap) * 2f, row.y, width, row.height), FeedbackTarget.Scadrial, "CC_BetaHub_Mod_Scadrial", locked);
        DrawSegment(new Rect(row.x + (width + gap) * 3f, row.y, width, row.height), FeedbackTarget.Roshar, "CC_BetaHub_Mod_Roshar", locked);
    }

    private void DrawSegment(Rect rect, FeedbackTarget target, string labelKey, bool locked) {
        bool selected = report.Target == target;

        Widgets.DrawBoxSolid(rect, selected ? SegmentOnColor : SegmentOffColor);
        Widgets.DrawBox(rect, 1, selected ? BorderTexture : SegmentEdgeTexture);

        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter, selected ? SegmentOnText : BodyTextColor)) {
            Widgets.Label(rect, labelKey.Translate());
        }

        if (locked) return;

        Widgets.DrawHighlightIfMouseover(rect);

        if (Widgets.ButtonInvisible(rect)) report.Target = target;
    }

    private Texture2D? BuildThumbnail() {
        if (screenshot == null) return null;

        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGB24, false);

        return texture.LoadImage(screenshot) ? texture : null;
    }

    private void DrawResultFooter(Rect inner, float buttonWidth) {
        SubmitResult finished = result!;
        Rect actionRect = inner.RightPartPixels(buttonWidth);

        if (Widgets.ButtonText(inner.LeftPartPixels(buttonWidth), "CC_BetaHub_Close".Translate())) {
            Close();
        }

        if (finished.Outcome == SubmitOutcome.Success) {
            if (string.IsNullOrEmpty(finished.IssueUrl)) return;

            if (Widgets.ButtonText(actionRect, "CC_BetaHub_ViewOnline".Translate())) {
                // Copied as well as opened. OpenURL fails silently where no browser is
                // reachable, and the window stays open so the address is still readable.
                GUIUtility.systemCopyBuffer = finished.IssueUrl;
                Application.OpenURL(finished.IssueUrl!);
                Messages.Message("CC_BetaHub_LinkCopied".Translate(), RimWorld.MessageTypeDefOf.TaskCompletion, false);
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
