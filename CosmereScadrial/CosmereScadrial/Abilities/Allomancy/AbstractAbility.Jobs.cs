using CosmereScadrial.Flags;
using RimWorld;
using Verse;
using Verse.AI;
using PawnUtility = CosmereFramework.Utils.PawnUtility;

namespace CosmereScadrial.Abilities.Allomancy {
    public abstract partial class AbstractAbility {
        public override bool CanApplyOn(LocalTargetInfo target) {
            var targetPawn = target.Pawn;
            if (targetFlags.Has(TargetFlags.Locations) && !target.HasThing && !target.TryGetPawn(out _)) return true;
            if (targetFlags.Has(TargetFlags.Self) && targetPawn == pawn) return true;
            if (targetFlags.Has(TargetFlags.Pawns) && targetPawn != null) return true;
            if (targetFlags.Has(TargetFlags.Fires) && targetPawn != null && targetPawn.IsBurning()) return true;
            if (targetFlags.Has(TargetFlags.Fires) && target.HasThing && target.Thing.IsBurning()) return true;
            if (targetFlags.Has(TargetFlags.Fires) && !target.HasThing &&
                target.Cell.ContainsStaticFire(pawn.Map)) {
                return true;
            }

            if (targetFlags.Has(TargetFlags.Buildings) && targetPawn == null && target.HasThing &&
                target.Thing.def.category == ThingCategory.Building) {
                return true;
            }

            if (targetFlags.Has(TargetFlags.Items) && targetPawn == null && target.HasThing &&
                target.Thing.def.category == ThingCategory.Item) {
                return true;
            }

            if (targetFlags.Has(TargetFlags.Animals) && targetPawn != null && target.Thing.def.race.Animal) return true;
            if (targetFlags.Has(TargetFlags.Humans) && targetPawn != null && target.Thing.def.race.Humanlike) {
                return true;
            }

            if (targetFlags.Has(TargetFlags.Mechs) && targetPawn != null && target.Thing.def.race.IsMechanoid) {
                return true;
            }

            if (targetFlags.Has(TargetFlags.Plants) && targetPawn == null && target.HasThing &&
                target.Thing.def.plant != null) {
                return true;
            }

            if (targetFlags.Has(TargetFlags.Mutants) && targetPawn is { IsMutant: true }) return true;
            if (targetFlags.Has(TargetFlags.WorldCell) && !target.HasThing && !target.TryGetPawn(out _)) return true;

            return base.CanApplyOn(target);
        }

        public virtual bool CanActivate(BurningStatus activationStatus, out string reason) {
            if (!metalBurning.CanBurn(metal, def.beuPerTick)) {
                reason = "MenuNoReserves".Translate(metal.LabelCap);
                return false;
            }

            if (activationStatus == BurningStatus.Flaring && PawnUtility.IsAsleep(pawn)) {
                reason = "MenuCannotFlareAsleep".Translate();
                return false;
            }

            reason = null;
            return true;
        }

        public override void QueueCastingJob(LocalTargetInfo target, LocalTargetInfo dest) {
            QueueCastingJob(target, dest, false);
        }

        public void QueueCastingJob(LocalTargetInfo target, LocalTargetInfo destination, bool flare) {
            shouldFlare = flare;
            base.QueueCastingJob(target, destination);
        }

        public override Job GetJob(LocalTargetInfo target, LocalTargetInfo destination) {
            var job = JobMaker.MakeJob(def.jobDef ?? RimWorld.JobDefOf.CastAbilityOnThing, target);
            job.ability = this;
            job.verbToUse = verb;
            job.playerForced = true;
            job.targetA = target;
            job.targetB = destination;
            job.count = (int)(shouldFlare ? BurningStatus.Flaring : BurningStatus.Burning);

            return job;
        }

        protected override void PreActivate(LocalTargetInfo? target) {
            if (shouldFlare) {
                UpdateStatus(status == BurningStatus.Flaring ? BurningStatus.Off : BurningStatus.Flaring);
            } else {
                UpdateStatus(PawnUtility.IsAsleep(pawn) ? BurningStatus.Passive :
                    atLeastPassive ? BurningStatus.Off : BurningStatus.Burning);
            }

            base.PreActivate(target);
        }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest) {
            var result = base.Activate(target, dest);
            shouldFlare = false;

            return result;
        }
    }
}