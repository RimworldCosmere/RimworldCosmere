using Cosmere.Core.Listing;
using Cosmere.Core.UI;
using Cosmere.System.Roshar;
using Cosmere.System.Roshar.Comp.Game;
using Cosmere.System.Roshar.Comp.Map;
using Cosmere.System.Roshar.Def;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Roshar.Surgebinding.Hediff;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Dialog;

[StaticConstructorOnStartup]
public class Dialog_ChooseRadiantOrder : Dialog_RadiantOrderDialogBase {
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

        bool isBondsmithLocked = forcedBondsmithSpren != null;

        if (!isBondsmithLocked) {
            if (Widgets.ButtonText(firstButtonRect, "Previous")) {
                quotes = null;
                radiantOrderIndex = (radiantOrderIndex - 1 + availableOrders.Count) % availableOrders.Count;
                order = availableOrders[radiantOrderIndex];
            }
        }

        RadiantOrderDef currentOrder = availableOrders[radiantOrderIndex];
        TaggedString joinString = "CRO_Choose_Radiant_Order_Dialog_Join".Translate(
            currentOrder.LabelCap.Named("ORDER")
        );

        bool isBondsmith = currentOrder == RadiantOrderDefOf.Bondsmith;
        bool bondsmithBlocked = false;

        if (isBondsmith && !isBondsmithLocked) {
            RadiantTracker tracker = Current.Game.GetComponent<RadiantTracker>();
            if (tracker != null && !tracker.CanProgressBondsmith()) {
                bondsmithBlocked = true;
            }
        }

        if (bondsmithBlocked) {
            Color origColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.4f);
            CTAButtonText(secondButtonRect, joinString);
            GUI.color = origColor;

            TooltipHandler.TipRegion(secondButtonRect, "Maximum number of Bondsmiths (3) has been reached.");
        } else if (CTAButtonText(secondButtonRect, joinString)) {
            pawn!.AllComps.RemoveWhere(x => x is Comp.Thing.ChooseRadiantOrder);

            TrueSprenSpawner? spawner = pawn!.Map?.GetComponent<TrueSprenSpawner>();
            spawner?.DestroySprenForPawn(pawn!);

            string? sprenName = isBondsmithLocked ? forcedBondsmithSpren : null;
            Surgebinder? surgebinder = pawn!.genes.TryAddRadiantOrder(
                currentOrder.GetSurgebindingGene(),
                sprenName: sprenName,
                showNamingDialog: !isBondsmithLocked
            );

            if (isBondsmithLocked && surgebinder != null) {
                surgebinder.godsprenName = forcedBondsmithSpren!;

                BondsmithCalling? calling = null;
                List<Verse.Hediff> hediffs = pawn!.health?.hediffSet?.hediffs ?? [];
                for (int i = 0; i < hediffs.Count; i++) {
                    if (hediffs[i] is BondsmithCalling c) {
                        calling = c;
                        break;
                    }
                }

                if (calling != null) {
                    pawn!.health!.RemoveHediff(calling);
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
                Find.Selector.Select(pawn!);
            }
        }

        if (!isBondsmithLocked) {
            if (Widgets.ButtonText(thirdButtonRect, "Next")) {
                quotes = null;
                radiantOrderIndex = (radiantOrderIndex + 1) % availableOrders.Count;
                order = availableOrders[radiantOrderIndex];
            }
        }
    }
}
