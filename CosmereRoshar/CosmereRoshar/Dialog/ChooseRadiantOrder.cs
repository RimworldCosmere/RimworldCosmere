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

    private static readonly List<RadiantOrderDef> RadiantOrders = DefDatabase<RadiantOrderDef>.AllDefsListForReading
        .Where(x => !x.Equals(RadiantOrderDefOf.Bondsmith))
        .ToList();

    private readonly Pawn pawn;

    private readonly ScrollViewStatus scrollViewStatus = new ScrollViewStatus();
    private float currentHeight;
    private int radiantOrderIndex;
    private Vector2 scrollPos;

    public ChooseRadiantOrder(Pawn pawn) : this() {
        this.pawn = pawn;
        doCloseX = true;
        forcePause = true;
        draggable = true;
    }

    protected override float Margin => 1f;

    public override Vector2 InitialSize => new Vector2(750, 800);

    private void DrawHeader(Rect rect) {
        Widgets.DrawRectFast(rect, new Color(0.55f, 0.58f, 0.62f, .3f));
        Widgets.DrawLineHorizontal(rect.x, rect.yMax - 1, rect.width, new Color(1f, 1f, 1f, 0.4f));

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
        rect = rect.ContractedBy(75, 0);

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
        using (new TextBlock(GameFont.Small, TextAnchor.MiddleCenter, new Color(0.9f, 0.9f, 0.9f)))
            listing.Label($"\"{order.ideals[0].quotes.RandomElement()}\"");

        listing.Gap();
        listing.GapLine();
        listing.Gap();

        // Description
        using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, Color.white))
            listing.Label(order.description);

        listing.Gap(36f);

        // Surge of Power Header
        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleCenter, new Color(1f, 1f, 1f, 0.7f)))
            listing.Label("CRO_Bond_Choose_Surges".Translate(order.LabelCap.Named("ORDER")));

        listing.Gap();

        // Two surge boxes side-by-side
        Rect surgeRow = listing.GetRect(240f); // fixed height for surge blocks
        surgeRow.SplitVerticallyWithMargin(out Rect leftBox, out Rect rightBox, 12f);

        DrawSurgeBox(leftBox, order.surges[0]);
        DrawSurgeBox(rightBox, order.surges[1]);
    }

    private void DrawSurgeBox(Rect rect, SurgeDef surge) {
        Widgets.DrawMenuSection(rect);

        Rect inner = rect.ContractedBy(18f);
        Listing_Standard listing = new Listing_Standard();
        listing.Begin(inner);

        // Title
        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleCenter, Color.white))
            listing.Label(surge.LabelCap);

        listing.Gap();

        // Description
        using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, Color.white))
            listing.Label(surge.description);

        listing.Gap();

        // Abilities
        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, new Color(0.85f, 0.85f, 0.85f))) {
            foreach (AbilityDef? ability in surge.abilities) {
                listing.Label($"• {ability.LabelCap}");
            }
        }

        listing.End();
    }

    private void DrawFooter(Rect rect) {
        Widgets.DrawLineHorizontal(rect.x, rect.y, rect.width, new Color(1f, 1f, 1f, 0.4f));
        Widgets.DrawRectFast(rect, new Color(0.55f, 0.58f, 0.62f, .3f));

        Rect innerRect = rect.ContractedBy(Padding);
        float third = innerRect.width / 3f;

        if (Widgets.ButtonText(
                new Rect(innerRect.x, innerRect.y, third - 36, FooterButtonHeight),
                "Previous"
            )) {
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

        DrawHeader(headerRect);
        Widgets.DrawRectFast(bodyRect, Color.black);
        using (ScrollView sv = new ScrollView(bodyRect, scrollViewStatus)) {
            sv.height = DrawBody(sv.rect);
        }

        DrawFooter(footerRect);
    }
}