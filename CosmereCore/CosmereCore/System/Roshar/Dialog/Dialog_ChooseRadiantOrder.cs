using Cosmere.Core.Listing;
using Cosmere.Core.UI;
using Cosmere.Core.UI.Dock;
using Cosmere.System.Roshar;
using Cosmere.System.Roshar.Comp.Game;
using Cosmere.System.Roshar.Comp.Map;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Roshar.Surgebinding.Hediff;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.System.Roshar.Dialog;

[StaticConstructorOnStartup]
public class Dialog_ChooseRadiantOrder : Dialog_RadiantOrderDialogBase {
    private const float RestDotAlpha = 0.4f;
    private const float HoverDotAlpha = 0.85f;

    private static readonly Color UnavailableDotColor = new Color(0.45f, 0.43f, 0.40f);
    private static readonly Color ArrowRestColor = new Color(0.79f, 0.65f, 0.37f, 0.3f);
    private static readonly Color ArrowHoverColor = new Color(0.79f, 0.65f, 0.37f, 0.9f);

    private static readonly List<RadiantOrderDef> RadiantOrders = DefDatabase<RadiantOrderDef>.AllDefsListForReading;
    private static List<RadiantOrderDef>? normalOrders;
    private readonly List<RadiantOrderDef> availableOrders;

    private readonly string? forcedBondsmithSpren;
    private string[]? quotes;
    private int radiantOrderIndex;

    public Dialog_ChooseRadiantOrder(Pawn pawn, string? bondsmithSpren = null) : base(RadiantOrders[0], pawn) {
        forcedBondsmithSpren = bondsmithSpren;

        if (forcedBondsmithSpren != null) {
            availableOrders = [];
            for (int i = 0; i < RadiantOrders.Count; i++) {
                if (RadiantOrders[i] == RadiantOrderDefOf.Bondsmith) {
                    availableOrders.Add(RadiantOrders[i]);
                    break;
                }
            }

            if (availableOrders.Count == 0) availableOrders = RadiantOrders;
        } else {
            normalOrders ??= BuildNormalOrders();
            availableOrders = normalOrders;
        }

        radiantOrderIndex = 0;
        if (availableOrders.Count > 0) {
            order = availableOrders[0];
        }
    }

    protected override bool hasFooter => true;

    private static List<RadiantOrderDef> BuildNormalOrders() {
        List<RadiantOrderDef> result = [];
        for (int i = 0; i < RadiantOrders.Count; i++) {
            if (RadiantOrders[i] != RadiantOrderDefOf.Bondsmith) {
                result.Add(RadiantOrders[i]);
            }
        }

        return result;
    }

    protected override TaggedString GetTitle() {
        if (forcedBondsmithSpren != null) {
            return "CRO_Bondsmith_Dialog_Title".Translate(
                pawn!.LabelShortCap.Named("PAWN"),
                forcedBondsmithSpren.Named("SPREN")
            );
        }

        return "CRO_Choose_Radiant_Order_Dialog_Title".Translate(pawn!.LabelShortCap.Named("PAWN"));
    }

    protected override TaggedString? GetSubtitle() {
        if (forcedBondsmithSpren != null) {
            return "CRO_Bondsmith_Dialog_Subtitle".Translate(forcedBondsmithSpren.Named("SPREN"));
        }

        return "CRO_Choose_Radiant_Order_Dialog_Subtitle".Translate();
    }

    protected override void DrawOverviewTab(FoundationListing listing) {
        if (order.ideals.Count > 1 && order.ideals[1].quotes.Count > 0) {
            quotes ??= [order.ideals[1].quotes.RandomElement()];

            using (new TextBlock(GameFont.Small, TextAnchor.MiddleCenter, bodyTextColor)) {
                for (int i = 0; i < quotes.Length; i++) {
                    listing.Label($"<i>\"{quotes[i]}\"</i>");
                }
            }

            listing.Gap();
            listing.GapLine(color: BorderColor);
            listing.Gap();
        }

        base.DrawOverviewTab(listing);
    }

    protected override float footerHeight => hasFooter ? Spacing.Get(8) : 0;

    protected override TaggedString? GetPositionReadout() {
        if (forcedBondsmithSpren != null || availableOrders.Count <= 1) return null;

        return "CRO_RadiantOrder_Position".Translate(
            (radiantOrderIndex + 1).Named("INDEX"),
            availableOrders.Count.Named("TOTAL")
        );
    }

    public override void DoWindowContents(Rect inRect) {
        base.DoWindowContents(inRect);

        if (forcedBondsmithSpren != null || availableOrders.Count <= 1) return;

        float contentTop = inRect.y + headerHeight;
        float railTop = inRect.yMax - footerHeight + padding.top;
        float height = RadiantFilmstripLayout.ArrowColumnHeight(contentTop, railTop);
        if (height <= 0f) return;

        Rect previousRect = new Rect(inRect.x, contentTop, RadiantFilmstripLayout.ArrowColumnWidth, height);
        Rect nextRect = new Rect(
            RadiantFilmstripLayout.RightArrowLeft(inRect.x, inRect.width),
            contentTop,
            RadiantFilmstripLayout.ArrowColumnWidth,
            height
        );

        if (DrawEdgeArrow(previousRect, true)) Step(-1);
        if (DrawEdgeArrow(nextRect, false)) Step(1);
    }

    /// <summary>Chevron points the way it moves, generated pre-turned so nothing rotates at draw time.</summary>
    private bool DrawEdgeArrow(Rect rect, bool pointsLeft) {
        float glyph = Spacing.Get(2);
        Rect chevronRect = new Rect(
            rect.center.x - glyph / 2f,
            rect.center.y - glyph / 2f,
            glyph,
            glyph
        );

        using (new TextBlock(Mouse.IsOver(rect) ? ArrowHoverColor : ArrowRestColor))
            GUI.DrawTexture(chevronRect, pointsLeft ? DockTex.ChevronLeft : DockTex.ChevronRight);

        // no fill: a 72px column of pale wash reads as a slab, and the chevron already brightens
        MouseoverSounds.DoRegion(rect);
        TooltipHandler.TipRegion(
            rect,
            pointsLeft
                ? "CRO_RadiantOrder_Nav_Previous".Translate()
                : "CRO_RadiantOrder_Nav_Next".Translate()
        );

        return Widgets.ButtonInvisible(rect);
    }

    private void Step(int delta) {
        SelectOrder((radiantOrderIndex + delta + availableOrders.Count) % availableOrders.Count);
    }

    private void SelectOrder(int index) {
        if (index == radiantOrderIndex) return;

        quotes = null;
        radiantOrderIndex = index;
        order = availableOrders[index];
    }

    private bool IsBondsmithCapped() {
        if (forcedBondsmithSpren != null) return false;

        RadiantTracker tracker = Current.Game.GetComponent<RadiantTracker>();

        return tracker != null && !tracker.CanProgressBondsmith();
    }

    private void DrawSigilRail(Rect rect) {
        int count = availableOrders.Count;
        bool bondsmithCapped = IsBondsmithCapped();
        TaggedString cappedReason = "CRO_RadiantOrder_BondsmithCapReached".Translate(
            RadiantTracker.MaxBondsmiths.Named("MAX")
        );

        for (int i = 0; i < count; i++) {
            RadiantOrderDef sigilOrder = availableOrders[i];
            bool selected = i == radiantOrderIndex;
            bool blocked = bondsmithCapped && sigilOrder == RadiantOrderDefOf.Bondsmith;

            float size = RadiantFilmstripLayout.DotSizeAt(i, radiantOrderIndex);
            float centerX = RadiantFilmstripLayout.DotCenterX(rect.center.x, count, i);
            Rect dotRect = new Rect(centerX - size / 2f, rect.center.y - size / 2f, size, size);

            if (selected) {
                Rect ringRect = dotRect.ExpandedBy(RadiantFilmstripLayout.SelectedRingWidth);
                using (new TextBlock(BorderColor)) GUI.DrawTexture(ringRect, CircleTex);
            }

            Color dotColor = blocked ? UnavailableDotColor : sigilOrder.color;
            float alpha = selected ? 1f : Mouse.IsOver(dotRect) ? HoverDotAlpha : RestDotAlpha;

            using (new TextBlock(new Color(dotColor.r, dotColor.g, dotColor.b, alpha)))
                GUI.DrawTexture(dotRect, CircleTex);

            Widgets.DrawHighlightIfMouseover(dotRect);
            MouseoverSounds.DoRegion(dotRect);
            TooltipHandler.TipRegion(
                dotRect,
                blocked
                    ? "CRO_RadiantOrder_Sigil_Blocked".Translate(
                        sigilOrder.LabelCap.Named("ORDER"),
                        cappedReason.Named("REASON")
                    )
                    : sigilOrder.LabelCap
            );

            if (Widgets.ButtonInvisible(dotRect)) {
                SelectOrder(i);
            }
        }
    }

    protected override void DrawFooter(Rect rect) {
        Rect innerRect = rect.ContractedBy(padding);
        bool isBondsmithLocked = forcedBondsmithSpren != null;

        Rect railRect = new Rect(
            innerRect.x,
            innerRect.y,
            innerRect.width,
            RadiantFilmstripLayout.RailHeight()
        );
        Rect secondButtonRect = new Rect(
            innerRect.x,
            railRect.yMax + Spacing.Get(0.5f),
            innerRect.width,
            footerButtonHeight
        );

        if (!isBondsmithLocked && availableOrders.Count > 1) {
            DrawSigilRail(railRect);
        }

        RadiantOrderDef currentOrder = availableOrders[radiantOrderIndex];
        TaggedString joinString = "CRO_Choose_Radiant_Order_Dialog_Join".Translate(
            currentOrder.LabelCap.Named("ORDER")
        );

        bool bondsmithBlocked = currentOrder == RadiantOrderDefOf.Bondsmith && IsBondsmithCapped();

        if (bondsmithBlocked) {
            CTAButtonText(secondButtonRect, joinString, false);
            TooltipHandler.TipRegion(
                secondButtonRect,
                "CRO_RadiantOrder_BondsmithCapReached".Translate(RadiantTracker.MaxBondsmiths.Named("MAX"))
            );
        } else if (CTAButtonText(secondButtonRect, joinString)) {
            pawn!.AllComps.RemoveWhere(x => x is Comp.Thing.ChooseRadiantOrder);

            TrueSprenSpawner? spawner = pawn.Map?.GetComponent<TrueSprenSpawner>();
            spawner?.DestroySprenForPawn(pawn);

            string? sprenName = isBondsmithLocked ? forcedBondsmithSpren : null;
            Surgebinder? surgebinder = pawn.genes.TryAddRadiantOrder(
                currentOrder.GetSurgebindingGene(),
                sprenName: sprenName,
                showNamingDialog: !isBondsmithLocked
            );

            if (isBondsmithLocked && surgebinder != null) {
                surgebinder.godsprenName = forcedBondsmithSpren!;

                BondsmithCalling? calling = null;
                List<Verse.Hediff> hediffs = pawn.health?.hediffSet?.hediffs ?? [];
                for (int i = 0; i < hediffs.Count; i++) {
                    if (hediffs[i] is BondsmithCalling c) {
                        calling = c;
                        break;
                    }
                }

                if (calling != null) {
                    pawn.health!.RemoveHediff(calling);
                }

                RadiantTracker tracker = Current.Game.GetComponent<RadiantTracker>();
                tracker?.RegisterBondsmith();

                BondsmithCallingChecker? checker = Current.Game.GetComponent<BondsmithCallingChecker>();
                checker?.RecordBondedGodspren(forcedBondsmithSpren!);
            }

            Close();

            if (surgebinder?.bondedSpren != null) {
                Find.Selector.Select(surgebinder.bondedSpren);
            } else {
                Find.Selector.Select(pawn);
            }
        }
    }
}
