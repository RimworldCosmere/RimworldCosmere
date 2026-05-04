using Cosmere.Core.Comp.Game;
using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.System.Roshar.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Hediff;

public class StrainedBond : HediffWithComps {
    private const float BaseRecoveryPerDay = 0.02f;
    private bool hasRegressedThisCycle;
    private bool sentBreakingWarning;
    private bool sentFracturedWarning;

    private bool sentStrainedWarning;

    public override void Tick() {
        base.Tick();

        if (!pawn.IsHashIntervalTick(GenTicks.TickLongInterval)) return;

        Surgebinder? surgebinder = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) {
            pawn.health.RemoveHediff(this);
            return;
        }

        ILoadReferenceable? bondTarget = surgebinder.GetBondTarget();
        if (bondTarget == null) {
            pawn.health.RemoveHediff(this);
            return;
        }

        float connection = SpiritWeb.Instance?.GetConnectionValue(pawn, bondTarget) ?? 0f;

        if (connection >= 1.0f) {
            pawn.health.RemoveHediff(this);
            return;
        }

        float idealMultiplier = 1f + surgebinder.CurrentIdeal * 0.25f;
        float personalityMultiplier = 1f;
        if (bondTarget is Pawn sprenPawn) {
            SprenBond? sprenBond = sprenPawn.TryGetComp<SprenBond>();
            if (sprenBond != null) {
                personalityMultiplier = sprenBond.GetRecoveryMultiplier();
            }
        }

        float recoveryPerTick = BaseRecoveryPerDay / GenDate.TicksPerDay * GenTicks.TickLongInterval;
        float recovery = recoveryPerTick * idealMultiplier * personalityMultiplier;
        SpiritWeb.Instance?.AdjustConnection(pawn, bondTarget, recovery);

        connection = SpiritWeb.Instance?.GetConnectionValue(pawn, bondTarget) ?? 0f;

        if (connection >= 0.7f) {
            pawn.health.RemoveHediff(this);
            return;
        }

        Severity = 1f - connection;

        string orderLabel = surgebinder.radiantOrderDef.LabelCap;

        if (connection < 0.15f && !sentBreakingWarning) {
            sentBreakingWarning = true;
            Find.LetterStack.ReceiveLetter(
                "CRO_BondBreaking_Title".Translate(pawn.NameShortColored.Named("PAWN")),
                "CRO_BondBreaking_Text".Translate(
                    pawn.NameFullColored.Named("PAWN"),
                    orderLabel.Named("ORDER")
                ),
                RimWorld.LetterDefOf.ThreatBig,
                pawn
            );
        }

        if (connection < 0.4f && !sentFracturedWarning) {
            sentFracturedWarning = true;
            Find.LetterStack.ReceiveLetter(
                "CRO_BondFractured_Title".Translate(pawn.NameShortColored.Named("PAWN")),
                "CRO_BondFractured_Text".Translate(
                    pawn.NameFullColored.Named("PAWN"),
                    orderLabel.Named("ORDER")
                ),
                RimWorld.LetterDefOf.NegativeEvent,
                pawn
            );
        }

        if (connection < 0.7f && !sentStrainedWarning) {
            sentStrainedWarning = true;
            Messages.Message(
                "CRO_BondStrained_Text".Translate(
                    pawn.NameShortColored.Named("PAWN"),
                    orderLabel.Named("ORDER")
                ),
                pawn,
                MessageTypeDefOf.ThreatSmall
            );
        }

        if (connection >= 0.7f) sentStrainedWarning = false;
        if (connection >= 0.4f) sentFracturedWarning = false;
        if (connection >= 0.15f) {
            sentBreakingWarning = false;
            hasRegressedThisCycle = false;
        }

        if (connection <= 0.15f && !hasRegressedThisCycle) {
            hasRegressedThisCycle = true;
            surgebinder.RegressIdeal();
        }

        if (connection <= 0.0f) {
            surgebinder.CatastrophicBondDeath();
        }
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref sentStrainedWarning, "sentStrainedWarning");
        Scribe_Values.Look(ref sentFracturedWarning, "sentFracturedWarning");
        Scribe_Values.Look(ref sentBreakingWarning, "sentBreakingWarning");
        Scribe_Values.Look(ref hasRegressedThisCycle, "hasRegressedThisCycle");
    }
}