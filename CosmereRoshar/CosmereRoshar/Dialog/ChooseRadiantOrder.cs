using UnityEngine;
using Verse;

namespace Cosmere.Roshar.Dialog;

public class ChooseRadiantOrder() : Window {
    private const float headerHeight = 100;
    private const float footerHeight = 100;
    private readonly Pawn pawn;
    private Vector2 scrollPos;

    public ChooseRadiantOrder(Pawn pawn) : this() {
        this.pawn = pawn;
        doCloseX = true;
        forcePause = true;
        draggable = true;
    }

    protected override float Margin => 1f;

    public override Vector2 InitialSize => new Vector2(750, 750);

    private void DrawHeader(Rect rect) {
        Widgets.DrawRectFast(rect, new Color(0.55f, 0.58f, 0.62f, .3f));
        Widgets.DrawLineHorizontal(rect.x, rect.yMax - 1, rect.width, new Color(1f, 1f, 1f, 0.4f));

        Rect innerRect = rect.ContractedBy(18);
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

    private void DrawBody(Rect rect) {
        Widgets.DrawRectFast(rect, Color.black);

        float viewHeight = 600f; // Estimate, or dynamically measure later
        Rect viewRect = new Rect(0, 0, rect.width - 50f, viewHeight);

        Widgets.BeginScrollView(rect.ContractedBy(12f), ref scrollPos, viewRect);

        Listing_Standard listing = new Listing_Standard();
        listing.Begin(viewRect);

        for (int i = 0; i < 20; i++) // Placeholder content
        {
            listing.Label($"Radiant Order #{i + 1}");
            listing.GapLine();
        }

        listing.End();
        Widgets.EndScrollView();
    }

    private void DrawFooter(Rect rect) {
        Widgets.DrawLineHorizontal(rect.x, rect.y, rect.width, new Color(1f, 1f, 1f, 0.4f));
        Widgets.DrawRectFast(rect, new Color(0.55f, 0.58f, 0.62f, .3f));

        Rect innerRect = rect.ContractedBy(18f);
        float third = innerRect.width / 3f;

        if (Widgets.ButtonText(new Rect(innerRect.x, innerRect.y, third, innerRect.height), "Previous")) {
            // TODO
        }

        if (Widgets.ButtonText(new Rect(innerRect.x + third, innerRect.y, third, innerRect.height), "Select")) {
            // TODO
        }

        if (Widgets.ButtonText(new Rect(innerRect.x + third * 2, innerRect.y, third, innerRect.height), "Next")) {
            // TODO
        }
    }

    public override void DoWindowContents(Rect inRect) {
        Rect headerRect = new Rect(inRect.x, inRect.y, inRect.width, headerHeight);
        Rect bodyRect = new Rect(
            inRect.x,
            inRect.y + headerHeight,
            inRect.width,
            inRect.height - headerHeight - footerHeight
        );
        Rect footerRect = new Rect(inRect.x, inRect.y + inRect.height - footerHeight, inRect.width, footerHeight);


        DrawHeader(headerRect);
        DrawBody(bodyRect);
        DrawFooter(footerRect);
    }
}