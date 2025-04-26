using RimWorld;
using Verse;
using Verse.AI;
using PawnUtility = CosmereFramework.Utils.PawnUtility;

namespace CosmereScadrial.Abilities.Allomancy {
    public class AbilityOtherTarget : AbstractAbility {
        private Job job;

        public AbilityOtherTarget() { }

        public AbilityOtherTarget(Pawn pawn) : base(pawn) { }

        public AbilityOtherTarget(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }

        public AbilityOtherTarget(Pawn pawn, AbilityDef def) : base(pawn, def) { }

        public AbilityOtherTarget(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) { }

        public override void AbilityTick() {
            base.AbilityTick();

            if (!atLeastPassive) {
                return;
            }

            if (PawnUtility.IsAsleep(pawn)) {
                UpdateStatus(BurningStatus.Off);
            }
        }

        public override bool Activate(LocalTargetInfo targetInfo, LocalTargetInfo dest) {
            target = targetInfo;

            var baseActivate = base.Activate(target, dest);
            if (!toggleable || !baseActivate) {
                return baseActivate;
            }

            if (def.maintenance == null) return true;

            job = JobMaker.MakeJob(def.maintenance.jobDef, target);
            job.followRadius = def.maintenance.followRadius;
            job.ability = this;
            job.count = (int)(status ?? BurningStatus.Burning);
            pawn.jobs.TryTakeOrderedJob(job);

            return true;
        }

        protected override void OnDisable() {
            RemoveHediff(target.Pawn);
            target = null;
            if (job != null) {
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                job = null;
            }

            ApplyDrag(def.applyDragOnTarget ? target.Pawn : pawn, flareDuration / 3000f);
            flareStartTick = -1;
        }

        protected override void OnFlare() {
            flareStartTick = Find.TickManager.TicksGame;
            RemoveDrag(def.applyDragOnTarget ? target.Pawn : pawn);
        }

        protected override void OnDeFlare() {
            ApplyDrag(def.applyDragOnTarget ? target.Pawn : pawn, flareDuration / 3000f / 2);
            flareStartTick = -1;
        }
    }
}