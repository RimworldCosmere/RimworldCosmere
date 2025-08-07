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
    protected bool drawBorderInt;

    private string[]? quotes;
    private int radiantOrderIndex;
    private float? surgeHeight;

    public ChooseRadiantOrder(Pawn pawn) : this() {
        this.pawn = pawn;
    }

    protected override bool hasFooter => true;

    protected override bool drawBorder {
        get => drawBorderInt;
        set => drawBorderInt = value;
    }

    protected override Vector2 initialWindowSize => new Vector2(
        Spacing.Get(65),
        Mathf.Max(Spacing.Get(31), UI.screenHeight - Spacing.Get(16))
    );

    protected override TaggedString GetTitle() {
        return "CRO_Choose_Radiant_Order_Dialog_Title".Translate(pawn.LabelShortCap.Named("PAWN"));
    }

    protected override TaggedString? GetSubtitle() {
        return "CRO_Choose_Radiant_Order_Dialog_Subtitle".Translate();
    }

    protected override void DrawBodyContent(Listing_Standard listing) {
        RadiantOrderDef? order = RadiantOrders[radiantOrderIndex];
        // Centered Image
        float imageSize = Spacing.Get(16);
        Rect imageRect = listing.GetRect(imageSize);
        Rect centered = new Rect(
            imageRect.x + (imageRect.width - imageSize) / 2f,
            imageRect.y,
            imageSize,
            imageSize
        );

        GUI.DrawTexture(centered, order.bannerIcon, ScaleMode.ScaleToFit);
        listing.Gap(Spacing.Get());

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

        listing.Gap(Spacing.Get());
        listing.GapLine(Spacing.Get());
        listing.Gap(Spacing.Get());

        // Description
        using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, bodyTextColor))
            listing.Label(order.description);

        float spaceRemaining = bodyHeight - listing.CurHeight - bodyPadding - Spacing.Get();
        if (surgeHeight != null) {
            listing.Gap(Mathf.Max(0, spaceRemaining - surgeHeight.Value));
        }

        // Surge of Power Header
        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleCenter, bodyTextColor)) {
            TaggedString str = "CRO_Bond_Choose_Surges".Translate(order.LabelCap.Named("ORDER"));
            listing.Label($"<b>{str}</b>");
        }

        listing.Gap(Spacing.Get());

        // Two surge boxes side-by-side
        Rect surgeRow = listing.GetRect(surgeHeight ??= DefaultSurgeHeight); // fixed height for surge blocks
        surgeRow.SplitVerticallyWithMargin(out Rect leftBox, out Rect rightBox, Spacing.Get());

        float leftHeight = DrawSurgeBox(leftBox, order.surges[0]);
        float rightHeight = DrawSurgeBox(rightBox, order.surges[1]);

        listing.Gap(Spacing.Get());

        surgeHeight = Mathf.Max(DefaultSurgeHeight, leftHeight, rightHeight);
    }

    private float DrawSurgeBox(Rect rect, SurgeDef surge) {
        float imageSize = Spacing.Get(5);
        DrawDropShadow(rect);
        if (drawBorder) {
            Widgets.DrawBoxSolid(rect, BorderColor);
        }

        GUI.DrawTexture(rect.ContractedBy(Margin), HeaderBackground, ScaleMode.StretchToFill);

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
        Listing_Standard leftListing = new Listing_Standard { maxOneColumn = true, verticalSpacing = 0 };
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
        leftListing.Gap(Spacing.Get());

        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleCenter, headerTextColor))
            leftListing.Label(surge.LabelCap);
        leftListing.End();

        // Right side
        Listing_Standard rightListing = new Listing_Standard { maxOneColumn = true, verticalSpacing = 0 };
        rightListing.Begin(rightBox);
        rightListing.Gap(rightPadding / 2);

        // Description
        using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, headerTextColor))
            rightListing.Label(surge.description);

        rightListing.End();
        rightListing.Gap(Spacing.Get());

        return Mathf.Max(leftListing.CurHeight, rightListing.CurHeight);
    }

    protected override void DrawFooter(Rect rect) {
        const int divisor = 12;

        Rect innerRect = rect.ContractedBy(padding);
        float unit = innerRect.width / divisor;
        Rect firstButtonRect = new Rect(innerRect.x, innerRect.y + 4, unit, footerButtonHeight - 8);
        Rect secondButtonRect = new Rect(
            unit * 4 + Spacing.Get(1 + 1f / divisor),
            innerRect.y - 4,
            unit * 4,
            footerButtonHeight + 8
        );
        Rect thirdButtonRect = new Rect(
            innerRect.width - unit * 1f + Spacing.Get(1 + 1f / divisor),
            innerRect.y + 4,
            unit,
            footerButtonHeight - 8
        );

        if (Widgets.ButtonText(firstButtonRect, "Previous")) {
            surgeHeight = null;
            quotes = null;
            radiantOrderIndex = (radiantOrderIndex - 1 + RadiantOrders.Count) % RadiantOrders.Count;
        }

        TaggedString joinString = "CRO_Choose_Radiant_Order_Dialog_Join".Translate(
            RadiantOrders[radiantOrderIndex].LabelCap.Named("ORDER")
        );
        if (Widgets.ButtonText(secondButtonRect, joinString)) {
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

    public override void DoWindowContents(Rect inRect) {
        base.DoWindowContents(inRect);

        drawBorder = Event.current.keyCode switch {
            KeyCode.Z => false,
            KeyCode.X => true,
            _ => drawBorder,
        };
    }
}