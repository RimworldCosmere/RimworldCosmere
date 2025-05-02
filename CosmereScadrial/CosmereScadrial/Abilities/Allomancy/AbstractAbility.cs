using System;
using CosmereScadrial.Comps.Things;
using CosmereScadrial.Defs;
using CosmereScadrial.Flags;
using RimWorld;
using Verse;
using Verse.AI;
using HediffUtility = CosmereScadrial.Utils.HediffUtility;
using Log = CosmereFramework.Log;
using PawnUtility = CosmereFramework.Utils.PawnUtility;

namespace CosmereScadrial.Abilities.Allomancy {
    public abstract class AbstractAbility : Ability {
        protected int flareStartTick = -1;
        protected bool shouldFlare;
        public BurningStatus? status;
        public LocalTargetInfo target;

        protected AbstractAbility() { }

        protected AbstractAbility(Pawn pawn) : base(pawn) { }

        protected AbstractAbility(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }

        protected AbstractAbility(Pawn pawn, AbilityDef def) : base(pawn, def) {
            if (toggleable) {
                status = BurningStatus.Off;
            }
        }

        protected AbstractAbility(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) {
            if (toggleable) {
                status = BurningStatus.Off;
            }
        }

        protected TargetFlags targetFlags => TargetFlagsExtensions.FromAbilityDef(def);

        public float flareDuration => flareStartTick < 0 ? 0 : Find.TickManager.TicksGame - flareStartTick;

        public new AllomanticAbilityDef def => (AllomanticAbilityDef)base.def;

        public bool atLeastPassive => status > BurningStatus.Passive;

        public MetallicArtsMetalDef metal => def.metal;

        protected virtual bool toggleable => def.toggleable;

        protected MetalBurning metalBurning => pawn.GetComp<MetalBurning>();

        public override bool CanCast => metalBurning.CanBurn(metal, def.beuPerTick);

        public TaggedString GetRightClickLabel(LocalTargetInfo target, BurningStatus burningStatus, string disableReason = null) {
            var hasDisableReason = !string.IsNullOrEmpty(disableReason);

            var label = def.LabelCap.Replace("Target", target.Pawn != null ? target.Pawn.LabelShort : target.Thing.LabelNoParenthesisCap);
            if (burningStatus.Equals(BurningStatus.Off)) {
                return def.label == metal.LabelCap ? $"Stop Burning {metal.LabelCap}" : $"Stop {label}";
            }

            if (def.LabelCap == metal.LabelCap) {
                return burningStatus.Equals(BurningStatus.Burning) ? $"Burn {metal.label}{(hasDisableReason ? $" ({disableReason.Trim()})" : "")}" : $"Flare {metal.label}{(hasDisableReason ? $" ({disableReason.Trim()})" : "")}";
            }

            if (status == BurningStatus.Flaring && burningStatus.Equals(BurningStatus.Burning)) label = $"De-Flare: {label}";
            return burningStatus.Equals(BurningStatus.Burning) ? $"{label}{(hasDisableReason ? $" ({disableReason.Trim()})" : "")}" : $"Flare {label}{(hasDisableReason ? $" ({disableReason.Trim()})" : "")}";
        }

        public override bool CanApplyOn(LocalTargetInfo target) {
            var targetPawn = target.Pawn;
            if (targetFlags.Has(TargetFlags.Locations) && !target.HasThing && !target.TryGetPawn(out _)) return true;
            if (targetFlags.Has(TargetFlags.Self) && targetPawn == pawn) return true;
            if (targetFlags.Has(TargetFlags.Pawns) && targetPawn != null) return true;
            if (targetFlags.Has(TargetFlags.Fires) && targetPawn != null && targetPawn.IsBurning()) return true;
            if (targetFlags.Has(TargetFlags.Fires) && target.HasThing && target.Thing.IsBurning()) return true;
            if (targetFlags.Has(TargetFlags.Fires) && !target.HasThing && target.Cell.ContainsStaticFire(pawn.Map)) return true;
            if (targetFlags.Has(TargetFlags.Buildings) && targetPawn == null && target.HasThing && target.Thing.def.category == ThingCategory.Building) return true;
            if (targetFlags.Has(TargetFlags.Items) && targetPawn == null && target.HasThing && target.Thing.def.category == ThingCategory.Item) return true;
            if (targetFlags.Has(TargetFlags.Animals) && targetPawn != null && target.Thing.def.race.Animal) return true;
            if (targetFlags.Has(TargetFlags.Humans) && targetPawn != null && target.Thing.def.race.Humanlike) return true;
            if (targetFlags.Has(TargetFlags.Mechs) && targetPawn != null && target.Thing.def.race.IsMechanoid) return true;
            if (targetFlags.Has(TargetFlags.Plants) && targetPawn == null && target.HasThing && target.Thing.def.plant != null) return true;
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

        public float GetDesiredBurnRateForStatus() {
            return GetDesiredBurnRateForStatus(status ?? BurningStatus.Off);
        }

        public float GetDesiredBurnRateForStatus(BurningStatus burningStatus) {
            return burningStatus switch {
                BurningStatus.Off => 0,
                BurningStatus.Passive => def.beuPerTick / 2,
                BurningStatus.Burning => def.beuPerTick,
                BurningStatus.Flaring => def.beuPerTick * 2,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        public void UpdateStatus(BurningStatus newStatus) {
            if (status == newStatus) return;

            Log.Verbose($"Updating {pawn.NameFullColored}'s {def.defName} from {status} -> {newStatus}");
            var oldStatus = status;
            status = newStatus;
            metalBurning.UpdateBurnSource(metal, GetDesiredBurnRateForStatus(newStatus), def);

            switch (status) {
                // When Disabling (and possibly deflaring)
                case BurningStatus.Off when oldStatus > BurningStatus.Off:
                    OnDisable();
                    return;
                // When enabling 
                case > BurningStatus.Off when oldStatus == BurningStatus.Off:
                    OnEnable();
                    return;
                // When Flaring
                case BurningStatus.Flaring when oldStatus < BurningStatus.Flaring:
                    OnFlare();
                    return;
                // When de-flaring to burning
                case < BurningStatus.Burning when oldStatus == BurningStatus.Flaring:
                    OnDeFlare();
                    return;
            }
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
                UpdateStatus(PawnUtility.IsAsleep(pawn) ? BurningStatus.Passive : atLeastPassive ? BurningStatus.Off : BurningStatus.Burning);
            }

            base.PreActivate(target);
        }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest) {
            var result = base.Activate(target, dest);
            shouldFlare = false;

            return result;
        }

        protected void GetOrAddHediff(Pawn targetPawn) {
            HediffUtility.GetOrAddHediff(pawn, targetPawn, this, def);
        }

        protected void RemoveHediff(Pawn targetPawn) {
            HediffUtility.RemoveHediff(pawn, targetPawn, this);
        }

        protected void ApplyDrag(Pawn targetPawn, float severity) {
            if (def.dragHediff == null || severity < def.minSeverityForDrag) return;

            Log.Warning($"Applying {def.dragHediff.defName} drag to {targetPawn.NameFullColored} with Severity={severity}");
            var drag = targetPawn.health.GetOrAddHediff(def.dragHediff);
            drag.Severity = severity;
        }

        protected void RemoveDrag(Pawn targetPawn) {
            var drag = targetPawn.health.hediffSet.GetFirstHediffOfDef(def.dragHediff);
            if (drag == null) return;

            var existingSeverity = drag.Severity;
            targetPawn.health.RemoveHediff(drag);
            flareStartTick -= (int)(existingSeverity * 3000f);
        }

        protected virtual void OnEnable() { }

        protected virtual void OnDisable() { }

        protected virtual void OnFlare() { }

        protected virtual void OnDeFlare() { }

        public override void ExposeData() {
            base.ExposeData();

            Scribe_Values.Look(ref flareStartTick, "flareStartTick", -1);
            Scribe_Values.Look(ref status, "status");
            Scribe_TargetInfo.Look(ref target, "target");
        }
    }
}