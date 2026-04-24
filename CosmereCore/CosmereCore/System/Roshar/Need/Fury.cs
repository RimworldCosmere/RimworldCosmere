using Cosmere.System.Roshar.Gene;
using RimWorld;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Roshar.Need;

public class Fury : RimWorld.Need {
    private const float BuildupPerKill = 0.05f;
    private const float BuildupPerHeavyDamage = 0.1f;
    private const float PyromaniacMultiplier = 1.5f;
    private const float BaseDecayPerHour = 0.01f;
    private const float MiningDecayPerHour = 0.03f;
    private const float SmithingDecayPerHour = 0.04f;
    private const float MeditatingDecayPerHour = 0.05f;
    private const float PostBreakReset = 0.5f;
    private const float ChanceThreshold = 0.95f;
    private const float ChancePerInterval = 0.25f;
    private const int NeedIntervalTicks = 150;

    private bool furyMastered;

    public Fury(Pawn pawn) : base(pawn) {
        threshPercents = [0.3f, 0.6f, 0.85f];
    }

    public override bool ShowOnNeedList => IsDustbringer();

    public override float MaxLevel => 1.2f;

    public override void SetInitialLevel() {
        CurLevel = 0f;
    }

    public override void NeedInterval() {
        if (!IsDustbringer()) return;

        Surgebinder? surgebinder = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return;

        if (CurLevel >= 1.0f) {
            TriggerFuryBreak();
            return;
        }

        if (CurLevel >= ChanceThreshold && Rand.Chance(ChancePerInterval)) {
            TriggerFuryBreak();
            return;
        }

        float idealMultiplier = 1f + surgebinder.currentIdeal * 0.25f;
        float hoursPerInterval = NeedIntervalTicks / 2500f;
        float decay = BaseDecayPerHour;

        Verse.AI.Job curJob = pawn.CurJob;
        if (curJob != null) {
            JobDef jobDef = curJob.def;
            if (jobDef == RimWorld.JobDefOf.Mine || jobDef == RimWorld.JobDefOf.FinishFrame) {
                decay = MiningDecayPerHour;
            } else if (jobDef == RimWorld.JobDefOf.DoBill) {
                if (curJob.workGiverDef?.workType == WorkTypeDefOf.Smithing) {
                    decay = SmithingDecayPerHour;
                }
            } else if (jobDef == RimWorld.JobDefOf.Meditate) {
                decay = MeditatingDecayPerHour;
            }
        }

        CurLevel -= decay * idealMultiplier * hoursPerInterval;
    }

    public void CheckFuryBreak() {
        if (CurLevel >= 1.0f) {
            TriggerFuryBreak();
        }
    }

    public void OnEnemyKilled() {
        float multiplier = IsPyromaniac() ? PyromaniacMultiplier : 1f;
        CurLevel += BuildupPerKill * multiplier;
        CheckFuryBreak();
    }

    public void OnHeavyDamageTaken() {
        float multiplier = IsPyromaniac() ? PyromaniacMultiplier : 1f;
        CurLevel += BuildupPerHeavyDamage * multiplier;
        CheckFuryBreak();
    }

    public void OnFuryMastered() {
        furyMastered = true;
    }

    private void TriggerFuryBreak() {
        if (pawn.mindState == null) return;
        if (furyMastered) return;

        bool started = pawn.mindState.mentalStateHandler.TryStartMentalState(
            MentalStateDefOf.Berserk,
            "Fury Break",
            true,
            false,
            false,
            null,
            true
        );

        if (started) {
            CurLevel = PostBreakReset;
            Messages.Message(
                "CRO_FuryBreak".Translate(pawn.NameShortColored.Named("PAWN")),
                pawn,
                MessageTypeDefOf.ThreatSmall
            );
        } else {
            Logger.Verbose($"Fury break failed for {pawn.NameShortColored} - TryStartMentalState returned false");
        }
    }

    private bool IsDustbringer() {
        Surgebinder? surgebinder = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return false;
        return surgebinder.radiantOrderDef.defName == "Dustbringer";
    }

    private bool IsPyromaniac() {
        return pawn.story?.traits?.HasTrait(RimWorld.TraitDefOf.Pyromaniac) ?? false;
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref furyMastered, "furyMastered");
    }
}