using CosmereCore.Utils;
using CosmereScadrial.Flags;
using RimWorld;
using Verse;
using Verse.AI;
using PawnUtility = CosmereFramework.Utils.PawnUtility;

namespace CosmereScadrial.Abilities.Allomancy {
    public abstract partial class AbstractAbility {
        public override bool CanApplyOn(LocalTargetInfo targetInfo) {
            var targetPawn = targetInfo.Pawn;
            if (targetFlags.Has(TargetFlags.Locations) && !targetInfo.HasThing &&
                !targetInfo.TryGetPawn(out _)) {
                return true;
            }

            if (targetFlags.Has(TargetFlags.Self) && targetPawn == pawn) return true;
            if (!targetFlags.Has(TargetFlags.Self) && targetPawn == pawn) return false;
            if (targetFlags.Has(TargetFlags.Pawns) && targetPawn != null) return true;
            if (targetFlags.Has(TargetFlags.Fires) && targetPawn != null && targetPawn.IsBurning()) return true;
            if (targetFlags.Has(TargetFlags.Fires) && targetInfo.HasThing && targetInfo.Thing.IsBurning()) return true;
            if (targetFlags.Has(TargetFlags.Fires) && !targetInfo.HasThing &&
                targetInfo.Cell.ContainsStaticFire(pawn.Map)) {
                return true;
            }

            if (targetFlags.Has(TargetFlags.Buildings) && targetPawn == null && targetInfo.HasThing &&
                targetInfo.Thing.def.category == ThingCategory.Building) {
                return true;
            }

            if (targetFlags.Has(TargetFlags.Items) && targetPawn == null && targetInfo.HasThing &&
                targetInfo.Thing.def.category == ThingCategory.Item) {
                return true;
            }

            if (targetFlags.Has(TargetFlags.Animals) && targetPawn != null &&
                targetInfo.Thing.def.race.Animal) {
                return true;
            }

            if (targetFlags.Has(TargetFlags.Humans) && targetPawn != null && targetInfo.Thing.def.race.Humanlike) {
                return true;
            }

            if (targetFlags.Has(TargetFlags.Mechs) && targetPawn != null && targetInfo.Thing.def.race.IsMechanoid) {
                return true;
            }

            if (targetFlags.Has(TargetFlags.Plants) && targetPawn == null && targetInfo.HasThing &&
                targetInfo.Thing.def.plant != null) {
                return true;
            }

            if (targetFlags.Has(TargetFlags.Mutants) && targetPawn is { IsMutant: true }) return true;
            if (targetFlags.Has(TargetFlags.WorldCell) && !targetInfo.HasThing &&
                !targetInfo.TryGetPawn(out _)) {
                return true;
            }

            return false;
        }

        public virtual AcceptanceReport CanActivate(LocalTargetInfo targetInfo, BurningStatus activationStatus,
            bool ignoreInvestiture = false) {
            if (!metalBurning.CanBurn(metal, def.beuPerTick)) {
                return "MenuNoReserves".Translate(metal.LabelCap);
            }

            if (activationStatus == BurningStatus.Flaring && PawnUtility.IsAsleep(pawn)) {
                return "MenuCannotFlareAsleep".Translate();
            }

            if (!ignoreInvestiture && targetInfo.Pawn != null && InvestitureDetector.IsShielded(targetInfo.Pawn)) {
                return "TargetShielded".Translate(targetInfo.Pawn.LabelShortCap);
            }

            return true;
        }

        public override void QueueCastingJob(LocalTargetInfo targetInfo, LocalTargetInfo dest) {
            QueueCastingJob(targetInfo, dest, false);
        }

        public void QueueCastingJob(LocalTargetInfo targetInfo, LocalTargetInfo destination, bool flare) {
            target = targetInfo;
            shouldFlare = def.canFlare && flare;
            base.QueueCastingJob(targetInfo, destination);
        }

        public override Job GetJob(LocalTargetInfo targetInfo, LocalTargetInfo destination) {
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