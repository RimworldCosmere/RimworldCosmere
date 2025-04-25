using CosmereScadrial.Utils;
using RimWorld;
using Verse;

namespace CosmereScadrial.Abilities.Allomancy {
    public class ExternalPhysicalTargetAbility : AbilityOtherTarget {
        public ExternalPhysicalTargetAbility() { }

        public ExternalPhysicalTargetAbility(Pawn pawn) : base(pawn) { }

        public ExternalPhysicalTargetAbility(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }

        public ExternalPhysicalTargetAbility(Pawn pawn, AbilityDef def) : base(pawn, def) { }

        public ExternalPhysicalTargetAbility(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) { }

        protected override sealed bool toggleable => false;

        public override bool CanApplyOn(LocalTargetInfo target) {
            if (!base.CanApplyOn(target) || !target.HasThing || !MetalDetector.IsCapableOfHavingMetal(target.Thing.def)) return false;

            return MetalDetector.GetMetal(target.Thing) > 0f;
        }

        public override bool Activate(LocalTargetInfo targetInfo, LocalTargetInfo dest, bool flare) {
            target = targetInfo;

            var baseActivate = base.Activate(target, dest, flare);
            if (!baseActivate || !target.HasThing) {
                return false;
            }

            var job = JobMaker.MakeJob(JobDefOf.Cosmere_Job_CastAllomanticAbilityAtTarget, target);
            job.ability = this;
            job.verbToUse = new Verb_CastAbility();
            job.verbToUse.caster = pawn;
            job.playerForced = true;
            job.verbToUse.verbProps = def.verbProperties;
            job.count = (int)(flare ? BurningStatus.Flaring : BurningStatus.Burning);
            pawn.jobs.TryTakeOrderedJob(job);

            return true;
        }
    }
}