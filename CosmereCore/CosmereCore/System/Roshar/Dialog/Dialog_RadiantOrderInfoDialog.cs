using System.Reflection;
using Cosmere.Core.Listing;
using Cosmere.Core.UI;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Gene;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Dialog;

public enum RadiantOrderInfoMode {
    SpeakOath,
    View,
}

[StaticConstructorOnStartup]
public class Dialog_RadiantOrderInfoDialog : Dialog_RadiantOrderDialogBase {
    private static readonly Color GreenTint = new Color(0.2f, 0.4f, 0.2f, 0.3f);
    private static readonly Color YellowTint = new Color(0.4f, 0.35f, 0.15f, 0.3f);
    private static readonly Color DangerColor = new Color(0.85f, 0.2f, 0.2f);

    private readonly RadiantOrderInfoMode mode;

    public Dialog_RadiantOrderInfoDialog(Pawn pawn, Surgebinder surgebinder, RadiantOrderInfoMode mode)
        : base(surgebinder.radiantOrderDef, pawn, surgebinder) {
        this.mode = mode;
    }

    protected override bool hasFooter => true;

    protected override TaggedString GetTitle() {
        return "CRO_RadiantOrder_Info_Title".Translate(
            pawn!.NameShortColored.Named("PAWN"),
            order.LabelCap.Named("ORDER")
        );
    }

    protected override void DrawIdealsTab(FoundationListing listing) {
        bool showOathContent = mode == RadiantOrderInfoMode.SpeakOath && surgebinder!.PendingOath;

        if (showOathContent) {
            int nextIdealIndex = surgebinder!.CurrentIdeal + 1;
            if (nextIdealIndex < order.ideals.Count) {
                Ideal nextIdeal = order.ideals[nextIdealIndex];

                DrawOathDisplay(listing, nextIdeal, nextIdealIndex);
                listing.Gap();

                DrawProgressRecap(listing);
                listing.Gap();

                if (nextIdealIndex < order.ideals.Count - 1) {
                    int futureIdealIndex = nextIdealIndex + 1;
                    Ideal futureIdeal = order.ideals[futureIdealIndex];
                    DrawNextPreview(listing, futureIdeal);
                    listing.Gap();
                }

                listing.GapLine(color: BorderColor);
                listing.Gap();
            }
        }

        base.DrawIdealsTab(listing);
    }

    private void DrawOathDisplay(FoundationListing listing, Ideal ideal, int idealIndex) {
        string quote = ideal.quotes.Count > 0 ? ideal.quotes[0] : string.Empty;
        TaggedString idealLabel = "CRO_RadiantOrder_Info_IdealOfOrder".Translate(
            GetOrdinal(idealIndex + 1).Named("ORDINAL"),
            order.LabelCap.Named("ORDER")
        );

        float quoteHeight;
        using (new TextBlock(GameFont.Medium))
            quoteHeight = Text.CalcHeight($"<i>\"{quote}\"</i>", listing.ColumnWidth - Spacing.Get(4));

        float totalHeight = quoteHeight + Spacing.Get(3);
        Rect boxRect = listing.GetRect(totalHeight);

        Texture2D borderTex = accentColor.ToSolidColorTexture();
        Widgets.DrawBox(boxRect, 2, borderTex);

        Rect inner = boxRect.ContractedBy(Spacing.Get());

        Rect quoteRect = new Rect(inner.x, inner.y, inner.width, quoteHeight);
        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleCenter, headerTextColor))
            Widgets.Label(quoteRect, $"<i>\"{quote}\"</i>");

        Rect labelRect = new Rect(inner.x, quoteRect.yMax + Spacing.Get(0.5f), inner.width, Spacing.Get(1.5f));
        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter, bodyTextColor))
            Widgets.Label(labelRect, idealLabel);
    }

    private void DrawProgressRecap(FoundationListing listing) {
        float headerHeight = Spacing.Get(2);

        FieldInfo[] fields = typeof(RecordDefOf).GetFields(BindingFlags.Public | BindingFlags.Static);

        int lineCount = 0;
        for (int i = 0; i < fields.Length; i++) {
            RecordDef? recordDef = fields[i].GetValue(null) as RecordDef;
            if (recordDef == null) continue;

            float value = pawn!.records.GetValue(recordDef);
            if (value > 0f) lineCount++;
        }

        if (lineCount == 0) return;

        float lineHeight = Spacing.Get(1.5f);
        float totalHeight = headerHeight + lineCount * lineHeight + Spacing.Get(2);
        Rect boxRect = listing.GetRect(totalHeight);

        Widgets.DrawBoxSolid(boxRect, GreenTint);

        Rect inner = boxRect.ContractedBy(Spacing.Get(0.5f));
        float yPos = inner.y;

        Rect headerRect = new Rect(inner.x, yPos, inner.width, headerHeight);
        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, headerTextColor)) {
            Widgets.Label(
                headerRect,
                $"<b>{"CRO_RadiantOrder_ProgressRecap".Translate(pawn!.LabelShortCap.Named("PAWN"))}</b>"
            );
        }

        yPos += headerHeight + Spacing.Get(0.25f);

        for (int i = 0; i < fields.Length; i++) {
            RecordDef? recordDef = fields[i].GetValue(null) as RecordDef;
            if (recordDef == null) continue;

            float value = pawn.records.GetValue(recordDef);
            if (value <= 0f) continue;

            Rect lineRect = new Rect(inner.x + Spacing.Get(), yPos, inner.width - Spacing.Get(), lineHeight);
            string displayValue = recordDef.type == RecordType.Time
                ? ((int)value).ToStringTicksToPeriod()
                : ((int)value).ToString();

            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, bodyTextColor)) {
                Widgets.Label(
                    lineRect,
                    "CRO_RadiantOrder_Info_ProgressLine".Translate(
                        recordDef.LabelCap.Named("RECORD"),
                        displayValue.Named("VALUE")
                    )
                );
            }

            yPos += lineHeight;
        }
    }

    private void DrawNextPreview(FoundationListing listing, Ideal futureIdeal) {
        float headerHeight = Spacing.Get(2);

        string? requirements = GetIdealRequirements(surgebinder!.CurrentIdeal + 2);
        float reqHeight = 0f;
        if (requirements != null) {
            using (new TextBlock(GameFont.Small))
                reqHeight = Text.CalcHeight(requirements, listing.ColumnWidth - Spacing.Get(3));
        }

        float totalHeight = headerHeight + reqHeight + Spacing.Get(2);
        Rect boxRect = listing.GetRect(totalHeight);

        Widgets.DrawBoxSolid(boxRect, YellowTint);

        Rect inner = boxRect.ContractedBy(Spacing.Get(0.5f));
        float yPos = inner.y;

        Rect headerRect = new Rect(inner.x, yPos, inner.width, headerHeight);
        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, headerTextColor)) {
            Widgets.Label(
                headerRect,
                $"<b>{"CRO_RadiantOrder_NextIdeal".Translate(futureIdeal.label.Named("IDEAL"))}</b>"
            );
        }

        yPos += headerHeight + Spacing.Get(0.25f);

        if (requirements != null) {
            Rect reqRect = new Rect(inner.x + Spacing.Get(0.5f), yPos, inner.width - Spacing.Get(), reqHeight);
            using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, bodyTextColor))
                Widgets.Label(reqRect, requirements);
        }
    }

    protected override void DrawFooter(Rect rect) {
        const int divisor = 12;

        Rect innerRect = rect.ContractedBy(padding);
        float unit = innerRect.width / divisor;

        Rect leftButtonRect = new Rect(innerRect.x, innerRect.y, unit * 2f, footerButtonHeight);
        Rect centerButtonRect = new Rect(
            innerRect.x + unit * 4f,
            innerRect.y - 4f,
            unit * 4f,
            footerButtonHeight + 8f
        );
        Rect rightButtonRect = new Rect(
            innerRect.xMax - unit * 2f,
            innerRect.y,
            unit * 2f,
            footerButtonHeight
        );

        TaggedString leftLabel = mode == RadiantOrderInfoMode.SpeakOath
            ? "CRO_SpeakOath_NotYet".Translate()
            : "CRO_RadiantOrder_Close".Translate();
        if (Widgets.ButtonText(leftButtonRect, leftLabel)) {
            Close();
            return;
        }

        bool showSpeakWords = mode == RadiantOrderInfoMode.SpeakOath || surgebinder!.PendingOath;
        if (showSpeakWords) {
            if (CTAButtonText(centerButtonRect, "CRO_SpeakOath_SpeakTheWords".Translate())) {
                surgebinder!.SpeakOath();
                Close();
                return;
            }
        }

        TooltipHandler.TipRegion(rightButtonRect, "CRO_BreakBond_SeverTooltip".Translate());

        using (new TextBlock(GUI.color * DangerColor)) {
            if (Widgets.ButtonText(rightButtonRect, "CRO_BreakBond_Sever".Translate())) {
                ShowSeverBondDialog();
            }
        }
    }

    private void ShowSeverBondDialog() {
        DiaNode root = new DiaNode(
            "CRO_BreakBond_DialogText".Translate(pawn!.NameShortColored.Named("PAWN"))
        );

        DiaOption severOption = new DiaOption("CRO_BreakBond_Sever".Translate()) {
            action = () => {
                surgebinder!.CatastrophicBondDeath();
                Close();
            },
            resolveTree = true,
        };
        root.options.Add(severOption);

        DiaOption cancelOption = new DiaOption("CRO_BreakBond_Cancel".Translate()) {
            resolveTree = true,
        };
        root.options.Add(cancelOption);

        Find.WindowStack.Add(
            new Dialog_NodeTree(
                root,
                title: "CRO_BreakBond_DialogTitle".Translate()
            )
        );
    }

    private static string GetOrdinal(int number) {
        if (number <= 0) return number.ToString();

        int remainder = number % 100;
        if (remainder >= 11 && remainder <= 13) return number + "th";

        return (number % 10) switch {
            1 => number + "st",
            2 => number + "nd",
            3 => number + "rd",
            _ => number + "th",
        };
    }
}
