using Cosmere.Foundation.Listing;
using Cosmere.Foundation.UI;
using Cosmere.Foundation.Window;
using Cosmere.Roshar.Def;
using UnityEngine;
using Verse;

namespace Cosmere.Roshar.Dialog;

[StaticConstructorOnStartup]
public class ChooseRadiantOrder() : BaseWindow {
    private const float DefaultSurgeHeight = 175;

    private static readonly List<RadiantOrderDef> RadiantOrders = DefDatabase<RadiantOrderDef>.AllDefsListForReading
        .Where(x => !x.Equals(RadiantOrderDefOf.Bondsmith))
        .ToList();

    private readonly Pawn pawn;

    private string[]? quotes;
    private int radiantOrderIndex;
    private float? surgeHeight;

    public ChooseRadiantOrder(Pawn pawn) : this() {
        this.pawn = pawn;
    }

    protected override bool hasFooter => true;

    protected override Vector2 initialWindowSize => new Vector2(
        Spacing.Get(65),
        Mathf.Max(Spacing.Get(30), UI.screenHeight - Spacing.Get(10))
    );

    protected override TaggedString GetTitle() {
        return "CRO_Choose_Radiant_Order_Dialog_Title".Translate(pawn.LabelShortCap.Named("PAWN"));
    }

    protected override TaggedString? GetSubtitle() {
        return "CRO_Choose_Radiant_Order_Dialog_Subtitle".Translate();
    }

    protected override void DrawBodyContent(FoundationListing listing) {
        RadiantOrderDef? order = RadiantOrders[radiantOrderIndex];
        // Centered Image
        float imageSize = Spacing.Get(16);
        Rect imageRect = listing.GetRect(imageSize);

        GUI.DrawTexture(imageRect.CenteredOnX(imageSize, imageSize), order.bannerIcon, ScaleMode.ScaleToFit);
        listing.Gap();

        // Quote
        using (new TextBlock(GameFont.Small, TextAnchor.MiddleCenter, bodyTextColor)) {
            quotes ??= [
                //order.ideals[0].quotes.RandomElement(),
                order.ideals[1].quotes.RandomElement(),
            ];

            foreach (string quote in quotes) {
                listing.Label($"<i>\"{quote}\"</i>");
            }
        }

        listing.Gap();
        listing.GapLine(color: BorderColor);
        listing.Gap();

        // Description
        using (new TextBlock(GameFont.Medium, TextAnchor.UpperLeft, bodyTextColor)) {
            float width = listing.ListingRect.width - Spacing.Get(36);
            float height = Text.CalcHeight(order.description, width);
            Rect rect = listing.GetRect(height + 32).ContractedBy(36, 24);
            Widgets.Label(rect, order.description);
        }

        /*float spaceRemaining = bodyHeight - listing.CurHeight - bodyPadding - Spacing.Get();
        if (surgeHeight != null) {
            listing.Gap(Mathf.Max(0, spaceRemaining - surgeHeight.Value));
        }*/

        // Surge of Power Header
        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleCenter, bodyTextColor)) {
            TaggedString str = "CRO_Bond_Choose_Surges".Translate(order.LabelCap.Named("ORDER"));
            Rect labelRect = listing.Label($"<b>{str}</b>");

            DrawBorder(
                labelRect.With(y: labelRect.yMax, height: 1),
                1,
                bottom: true
            );
        }

        listing.Gap();

        // Two surge boxes side-by-side
        Rect surgeRow = listing.GetRect(surgeHeight ??= DefaultSurgeHeight); // fixed height for surge blocks
        surgeRow.SplitVerticallyWithMargin(out Rect leftBox, out Rect rightBox, Spacing.Get());

        float leftHeight = DrawSurgeBox(leftBox, order.surges[0]);
        float rightHeight = DrawSurgeBox(rightBox, order.surges[1]);

        listing.Gap();

        surgeHeight = Mathf.Max(DefaultSurgeHeight, leftHeight, rightHeight);
    }

    private float DrawSurgeBox(Rect rect, SurgeDef surge) {
        float imageSize = Spacing.Get(5);
        DrawDropShadow(rect);
        if (drawBorder) {
            //Widgets.DrawBoxSolid(rect, BorderColor);
        }

        GUI.DrawTexture(rect.ContractedBy(0), HeaderBackground, ScaleMode.StretchToFill);

        Vector2 titleSize;
        using (new TextBlock(GameFont.Medium)) titleSize = Text.CalcSize(surge.LabelCap);

        Rect inner = rect.ContractedBy(16);
        inner.SplitVerticallyWithMargin(
            out Rect leftBox,
            out Rect rightBox,
            out float overflow,
            Spacing.Get(),
            Mathf.Max(imageSize, titleSize.x)
        );

        float descriptionHeight;
        using (new TextBlock(GameFont.Small, null, true))
            descriptionHeight = Text.CalcHeight(surge.description, rightBox.width);
        float leftPadding = (surgeHeight ?? DefaultSurgeHeight) - imageSize - titleSize.y - Spacing.Get(2);
        float rightPadding = (surgeHeight ?? DefaultSurgeHeight) - descriptionHeight - Spacing.Get(2);

        // Left Side
        FoundationListing leftListing = new FoundationListing { maxOneColumn = true, verticalSpacing = 0 };
        leftListing.Begin(leftBox);
        leftListing.Gap(leftPadding / 2);

        Rect imageRect = leftListing.GetRect(imageSize);

        Rect centered = new Rect(
            imageRect.x + (imageRect.width - imageSize) / 2f,
            imageRect.y,
            imageSize,
            imageSize
        );
        GUI.DrawTexture(centered, surge.icon, ScaleMode.ScaleToFit);
        leftListing.Gap();

        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleCenter, headerTextColor))
            leftListing.Label(surge.LabelCap);
        leftListing.End();

        // Right side
        FoundationListing rightListing = new FoundationListing { maxOneColumn = true, verticalSpacing = 0 };
        rightListing.Begin(rightBox);
        rightListing.Gap(rightPadding / 2);

        // Description
        using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, headerTextColor))
            rightListing.Label(surge.description);

        rightListing.End();
        rightListing.Gap();

        return Mathf.Max(leftListing.CurHeight, rightListing.CurHeight);
    }

    protected override void DrawFooter(Rect rect) {
        const int divisor = 12;

        Rect innerRect = rect.ContractedBy(padding);
        float unit = innerRect.width / divisor;
        Rect firstButtonRect = new Rect(innerRect.x, innerRect.y, unit, footerButtonHeight);
        Rect secondButtonRect = new Rect(
            unit * 4 + Spacing.Get(1 + 1f / divisor),
            innerRect.y - 4,
            unit * 4,
            footerButtonHeight + 8
        );
        Rect thirdButtonRect = new Rect(
            innerRect.width - unit * 1f + Spacing.Get(1 + 1f / divisor),
            innerRect.y,
            unit,
            footerButtonHeight
        );

        if (Widgets.ButtonText(firstButtonRect, "Previous")) {
            surgeHeight = null;
            quotes = null;
            radiantOrderIndex = (radiantOrderIndex - 1 + RadiantOrders.Count) % RadiantOrders.Count;
        }

        TaggedString joinString = "CRO_Choose_Radiant_Order_Dialog_Join".Translate(
            RadiantOrders[radiantOrderIndex].LabelCap.Named("ORDER")
        );
        if (CTAButtonText(secondButtonRect, joinString)) {
            pawn.AllComps.RemoveWhere(x => x is Comp.Thing.ChooseRadiantOrder);
            pawn.genes.TryAddRadiantOrder(RadiantOrders[radiantOrderIndex].GetSurgebindingGene());
            Close();
            Find.Selector.Select(pawn);
        }

        if (Widgets.ButtonText(thirdButtonRect, "Next")) {
            surgeHeight = null;
            quotes = null;
            radiantOrderIndex = (radiantOrderIndex + 1) % RadiantOrders.Count;
        }
    }
}