using Cosmere.Framework.UI;
using Cosmere.Roshar.Def;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Roshar.Dialog;

public class ChooseRadiantOrder() : Window {
    private const float HeaderHeight = 100;
    private const float FooterHeight = 68;
    private const float FooterButtonHeight = 32;
    private static readonly Padding Padding = new Padding(18);
    private static readonly Color HeaderColor = Widgets.MenuSectionBGFillColor;
    private static readonly Color FooterColor = Widgets.MenuSectionBGFillColor;
    private static readonly Color LineColor = new ColorInt(97, 108, 122).ToColor;
    private static readonly Color BorderColor = new ColorInt(135, 135, 135).ToColor;

    private static readonly List<RadiantOrderDef> RadiantOrders = DefDatabase<RadiantOrderDef>.AllDefsListForReading
        .Where(x => !x.Equals(RadiantOrderDefOf.Bondsmith))
        .ToList();

    private readonly Pawn pawn;

    private readonly ScrollViewStatus scrollViewStatus = new ScrollViewStatus();
    private float currentHeight;
    private string[]? quotes;
    private int radiantOrderIndex;
    private Vector2 scrollPos;

    private float? surgeHeight;

    public ChooseRadiantOrder(Pawn pawn) : this() {
        this.pawn = pawn;
        doCloseX = true;
        forcePause = true;
        draggable = true;
    }

    protected override float Margin => 1f;

    public override Vector2 InitialSize => new Vector2(750, Mathf.Max(500, UI.screenHeight - 250));

    private void DrawHeader(Rect rect) {
        Rect innerRect = rect.ContractedBy(Padding);
        Listing_Standard listing = new Listing_Standard { maxOneColumn = true };
        listing.Begin(innerRect);

        using (new TextBlock(TextAnchor.MiddleCenter, Color.white)) {
            using (new TextBlock(GameFont.Medium)) {
                listing.Label("CRO_Choose_Radiant_Order_Dialog_Title".Translate(pawn.LabelShortCap.Named("PAWN")));
            }

            using (new TextBlock(GameFont.Tiny)) {
                listing.Label("CRO_Choose_Radiant_Order_Dialog_Subtitle".Translate());
            }
        }

        listing.End();
    }

    private float DrawBody(Rect rect) {
        rect = rect.ContractedBy(scrollViewStatus.scrollVisibile ? 35 : 55, 0);
        if (scrollViewStatus.scrollVisibile) {
            rect = new Rect(rect.x + 20, rect.y, rect.width - 20, rect.height);
        }

        Listing_Standard listing = new Listing_Standard {
            maxOneColumn = true,
            verticalSpacing = 0,
        };

        listing.Begin(rect);
        DrawOrderBodyContent(listing, RadiantOrders[radiantOrderIndex]);
        listing.End();

        listing.Gap();

        return listing.CurHeight;
    }

    private void DrawOrderBodyContent(Listing_Standard listing, RadiantOrderDef order) {
        listing.Gap();
        // Centered Image
        if (order.icon != null) {
            float imageSize = 128f;
            Rect imageRect = listing.GetRect(imageSize);
            Rect centered = new Rect(
                imageRect.x + (imageRect.width - imageSize) / 2f,
                imageRect.y,
                imageSize,
                imageSize
            );
            GUI.DrawTexture(centered, order.icon, ScaleMode.ScaleToFit);
            listing.Gap();
        }

        // Order Name
        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleCenter, Color.white))
            listing.Label(order.LabelCap);

        // Quote
        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleCenter, new Color(0.9f, 0.9f, 0.9f))) {
            quotes ??= [
                order.ideals[0].quotes.RandomElement(),
                order.ideals[1].quotes.RandomElement(),
            ];

            listing.Label($"\"{quotes[0]}\"");
            listing.Label($"\"{quotes[1]}\"");
        }

        listing.Gap();
        listing.GapLine();
        listing.Gap();

        // Description
        using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, Color.white))
            listing.Label(order.description);

        listing.Gap(293 - listing.CurHeight);
        listing.Gap(30f);

        float spaceRemaining =
            InitialSize.y -
            HeaderHeight -
            FooterHeight -
            listing.CurHeight -
            (scrollViewStatus.scrollVisibile ? 35 : 55);
        if (surgeHeight != null) {
            listing.Gap(Mathf.Abs(spaceRemaining - surgeHeight.Value));
        }

        //listing.Gap(Mathf.Abs(330 - listing.CurHeight + surgeHeight ?? 150));

        // Surge of Power Header
        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.7f)))
            listing.Label("CRO_Bond_Choose_Surges".Translate(order.LabelCap.Named("ORDER")));

        listing.Gap();

        // Two surge boxes side-by-side
        Rect surgeRow = listing.GetRect(surgeHeight ??= 150); // fixed height for surge blocks
        surgeRow.SplitVerticallyWithMargin(out Rect leftBox, out Rect rightBox, 12f);

        float leftHeight = DrawSurgeBox(leftBox, order.surges[0]);
        float rightHeight = DrawSurgeBox(rightBox, order.surges[1]);

        surgeHeight = Mathf.Max(150, leftHeight, rightHeight);
    }

    private float DrawSurgeBox(Rect rect, SurgeDef surge) {
        Widgets.DrawShadowAround(rect);
        Widgets.DrawMenuSection(rect);

        Rect inner = rect.ContractedBy(18f);
        Listing_Standard listing = new Listing_Standard { maxOneColumn = true, verticalSpacing = 0 };
        listing.Begin(inner);

        // Title
        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleCenter, Color.white))
            listing.Label(surge.LabelCap);

        listing.Gap(9);

        // Description
        using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, Color.white))
            listing.Label(surge.description);

        if (surge.abilities.Count > 0) {
            listing.Gap(9);

            // Abilities
            using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, new Color(0.85f, 0.85f, 0.85f))) {
                foreach (AbilityDef? ability in surge.abilities) {
                    listing.Label($"• {ability.LabelCap}");
                }
            }
        }

        listing.End();
        listing.Gap();

        return listing.CurHeight + 22;
    }

    private void DrawFooter(Rect rect) {
        Rect innerRect = rect.ContractedBy(Padding);
        float third = innerRect.width / 3f;

        if (Widgets.ButtonText(
                new Rect(innerRect.x, innerRect.y, third - 36, FooterButtonHeight),
                "Previous"
            )) {
            surgeHeight = null;
            quotes = null;
            radiantOrderIndex = (radiantOrderIndex - 1 + RadiantOrders.Count) % RadiantOrders.Count;
        }

        if (Widgets.ButtonText(
                new Rect(18 + innerRect.x + third, innerRect.y, third - 36, FooterButtonHeight),
                "Select"
            )) {
            pawn.AllComps.RemoveWhere(x => x is Comp.Thing.ChooseRadiantOrder);
            pawn.genes.TryAddRadiantOrder(RadiantOrders[radiantOrderIndex].GetSurgebindingGene());
            Find.WindowStack.TryRemove(this);
            Find.Selector.Select(pawn);
        }

        if (Widgets.ButtonText(
                new Rect(36 + innerRect.x + third * 2, innerRect.y, third - 36, FooterButtonHeight),
                "Next"
            )) {
            surgeHeight = null;
            quotes = null;
            radiantOrderIndex = (radiantOrderIndex + 1) % RadiantOrders.Count;
        }
    }

    public override void DoWindowContents(Rect inRect) {
        Rect headerRect = new Rect(inRect.x, inRect.y, inRect.width, HeaderHeight);
        Rect bodyRect = new Rect(
            inRect.x,
            inRect.y + HeaderHeight,
            inRect.width,
            inRect.height - HeaderHeight - FooterHeight
        );
        Rect footerRect = new Rect(inRect.x, inRect.y + inRect.height - FooterHeight, inRect.width, FooterHeight);

        using (new TextBlock(Widgets.MenuSectionBGFillColor)) GUI.DrawTexture(headerRect, BaseContent.WhiteTex);

        using (new TextBlock(Widgets.MenuSectionBGFillColor))
            Widgets.DrawLineHorizontal(headerRect.x, headerRect.yMax - 1, headerRect.width, BorderColor);
        //Widgets.DrawRectFast(headerRect, LineColor);
        //Widgets.DrawLineHorizontal(headerRect.x, headerRect.yMax - 1, headerRect.width, HeaderColor);
        DrawHeader(headerRect);

        // Widgets.DrawRectFast(bodyRect, Color.black);
        using (ScrollView sv = new ScrollView(bodyRect, scrollViewStatus)) {
            sv.height = DrawBody(sv.rect);
        }

        using (new TextBlock(Widgets.MenuSectionBGFillColor)) GUI.DrawTexture(footerRect, BaseContent.WhiteTex);

        using (new TextBlock(Widgets.MenuSectionBGFillColor))
            Widgets.DrawLineHorizontal(footerRect.x, footerRect.y, footerRect.width, BorderColor);
        DrawFooter(footerRect);
    }
}