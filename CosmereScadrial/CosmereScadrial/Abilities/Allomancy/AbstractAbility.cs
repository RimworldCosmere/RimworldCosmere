using System;
using System.Collections.Generic;
using CosmereScadrial.Abilities.Allomancy.Hediffs;
using CosmereScadrial.Comps.Things;
using CosmereScadrial.Defs;
using CosmereScadrial.Flags;
using RimWorld;
using Verse;
using HediffUtility = CosmereScadrial.Utils.HediffUtility;
using Log = CosmereFramework.Log;
using PawnUtility = CosmereFramework.Utils.PawnUtility;

namespace CosmereScadrial.Abilities.Allomancy {
    public abstract class AbstractAbility : Ability {
        protected int flareStartTick = -1;
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

            var label = def.LabelCap.Replace("Target", target.Label);
            if (burningStatus.Equals(BurningStatus.Off)) {
                return def.label == metal.LabelCap ? $"Stop Burning {metal.LabelCap}" : $"Stop {label}";
            }

            if (def.label == metal.label.CapitalizeFirst()) {
                return burningStatus.Equals(BurningStatus.Burning) ? $"Burn {metal.label}{(hasDisableReason ? $" ({disableReason.Trim()})" : "")}" : $"Flare {metal.label}{(hasDisableReason ? $" ({disableReason.Trim()})" : "")}";
            }

            if (status == BurningStatus.Flaring && burningStatus.Equals(BurningStatus.Burning)) label = $"De-Flare: {label}";
            return burningStatus.Equals(BurningStatus.Burning) ? $"{label}{(hasDisableReason ? $" ({disableReason.Trim()})" : "")}" : $"Flare {label}{(hasDisableReason ? $" ({disableReason.Trim()})" : "")}";
        }

        public override bool CanApplyOn(LocalTargetInfo target) {
            // 1. Handle null EffectComps or targetable fallback
            if (EffectComps == null) {
                return def.verbProperties.targetable || !def.verbProperties.targetParams.canTargetSelf || target.Thing == pawn;
            }

            // 2. Verify all EffectComps agree this can apply
            if (EffectComps.Any(comp => !comp.CanApplyOn(target, null))) {
                return false;
            }

            // Null check up front for any target.Pawn use
            var targetPawn = target.Pawn;

            // 3. Self targeting
            if (targetFlags.IsOnlySelf()) {
                return targetPawn != null && targetPawn == pawn;
            }

            // 4. Object-only logic (non-pawn, has a thing)
            if (targetFlags.IsOnlyObject()) {
                return targetPawn == null && target.HasThing;
            }

            // 5. Other-only targeting
            if (targetFlags.IsOnlyOther()) {
                return targetPawn != null && targetPawn != pawn;
            }

            // 6. Self or other pawn
            if (targetFlags.IsSelfOrOtherPawn()) {
                return targetPawn != null;
            }

            // 7. World cell targeting
            if (targetFlags.CanTargetWorldCell()) {
                return targetPawn == null;
            }

            // 8. Target is not world cell — pawn must not be null now
            if (targetPawn == null) {
                return false;
            }

            // 9. Animal targeting
            if (targetFlags.CanTargetAnimals()) {
                if (targetPawn.IsNonMutantAnimal) {
                    return true;
                }
            } else if (targetPawn.IsNonMutantAnimal) {
                return false;
            }

            // 10. Humanlike or self
            if (targetFlags.IsSelfOrOtherHumanlike()) {
                return targetPawn == pawn || targetPawn.RaceProps?.Humanlike == true;
            }

            // 11. Humanlike or animal or self
            if (targetFlags.IsSelfOrOtherHumanlikeOrAnimal()) {
                return targetPawn == pawn || targetPawn.RaceProps?.Humanlike == true || targetPawn.IsNonMutantAnimal;
            }

            // 12. General self-or-other fallback
            if (targetFlags.IsSelfOrOther()) {
                return true;
            }

            return false;
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

        public override IEnumerable<Command> GetGizmos() {
            yield break;
        }

        public virtual float GetDesiredBurnRateForStatus() {
            return GetDesiredBurnRateForStatus(status ?? BurningStatus.Off);
        }

        public virtual float GetDesiredBurnRateForStatus(BurningStatus burningStatus) {
            return burningStatus switch {
                BurningStatus.Off => 0,
                BurningStatus.Passive => def.beuPerTick / 2,
                BurningStatus.Burning => def.beuPerTick,
                BurningStatus.Flaring => def.beuPerTick * 2,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        public virtual void UpdateStatus(BurningStatus newStatus) {
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

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest) {
            return Activate(target, dest, false);
        }

        public virtual bool Activate(LocalTargetInfo target, LocalTargetInfo dest, bool flare) {
            if (!pawn.Spawned || pawn.Map == null) return false;

            return base.Activate(target, dest);
        }

        protected virtual AllomanticHediff GetOrAddHediff(Pawn targetPawn) {
            return HediffUtility.GetOrAddHediff(pawn, targetPawn, this, def);
        }

        protected virtual void RemoveHediff(Pawn targetPawn) {
            HediffUtility.RemoveHediff(pawn, targetPawn, this);
        }

        protected virtual void ApplyDrag(Pawn targetPawn, float severity) {
            if (def.dragHediff == null || severity < def.minSeverityForDrag) return;

            Log.Warning($"Applying {def.dragHediff.defName} drag to {targetPawn.NameFullColored} with Severity={severity}");
            var drag = targetPawn.health.GetOrAddHediff(def.dragHediff);
            drag.Severity = severity;
        }

        protected virtual void RemoveDrag(Pawn targetPawn) {
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