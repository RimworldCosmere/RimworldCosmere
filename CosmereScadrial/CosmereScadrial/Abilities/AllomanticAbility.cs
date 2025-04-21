using System;
using System.Collections.Generic;
using CosmereScadrial.Abilities.Hediffs;
using CosmereScadrial.Comps.Things;
using CosmereScadrial.Defs;
using CosmereScadrial.Flags;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Log = CosmereFramework.Log;
using PawnUtility = CosmereFramework.Utils.PawnUtility;

namespace CosmereScadrial.Abilities {
    public enum BurningStatus {
        Off,
        Passive,
        Burning,
        Flaring,
    }

    public class AllomanticAbility : Ability {
        private readonly TargetFlags targetFlags;
        private int flareStartTick = -1;
        public BurningStatus? status;

        private Pawn targetPawn;

        public AllomanticAbility(Pawn pawn, AbilityDef def) : base(pawn, def) {
            if (toggleable) {
                status = BurningStatus.Off;
            }

            targetFlags = targetFlags.FromAbilityDef(def);
            Log.Verbose($"{def.defName} flags={targetFlags}");
        }

        public AllomanticAbility(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) {
            if (toggleable) {
                status = BurningStatus.Off;
            }

            targetFlags = targetFlags.FromAbilityDef(def);
            Log.Verbose($"{def.defName} flags={targetFlags}");
        }

        public float flareDuration => flareStartTick < 0 ? 0 : Find.TickManager.TicksGame - flareStartTick;

        public new AllomanticAbilityDef def => (AllomanticAbilityDef)base.def;

        public bool atLeastPassive => status > BurningStatus.Passive;

        public MetallicArtsMetalDef metal => def.metal;

        private bool toggleable => def.toggleable;

        private MetalBurning metalBurning => pawn.GetComp<MetalBurning>();

        public override bool CanCast => metalBurning.CanBurn(metal, def.beuPerTick);

        public TaggedString GetRightClickLabel(LocalTargetInfo target, BurningStatus burningStatus, string disableReason = null) {
            var hasDisableReason = !string.IsNullOrEmpty(disableReason);

            if (def.label == metal.label.CapitalizeFirst()) {
                return burningStatus.Equals(BurningStatus.Burning) ? $"Burn {metal.label}{(hasDisableReason ? $" ({disableReason.Trim()})" : "")}" : $"Flare {metal.label}{(hasDisableReason ? $" ({disableReason.Trim()})" : "")}";
            }

            var label = def.label.Replace("Target", target.Label);
            return burningStatus.Equals(BurningStatus.Burning) ? $"{label}{(hasDisableReason ? $" ({disableReason.Trim()})" : "")}" : $"Flare {label}{(hasDisableReason ? $" ({disableReason.Trim()})" : "")}";
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

            if (def.verbProperties.range > 0f) {
                if (pawn.Position.DistanceTo(target.CenterVector3.ToIntVec3()) > def.verbProperties.range) return false;
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

        public override IEnumerable<Command> GetGizmos() {
            yield break;
        }

        public override void AbilityTick() {
            base.AbilityTick();

            if (!atLeastPassive) {
                return;
            }

            if (targetPawn == null || targetPawn.Equals(pawn)) {
                if (PawnUtility.IsAsleep(pawn) && status > BurningStatus.Passive) {
                    UpdateStatus(BurningStatus.Passive);
                } else if (!PawnUtility.IsAsleep(pawn) && status == BurningStatus.Passive) {
                    UpdateStatus(BurningStatus.Burning);
                }

                if (!pawn.IsHashIntervalTick(GenTicks.SecondsToTicks(5)) || !pawn.Spawned || pawn.Map == null) return;
                var mote = MoteMaker.MakeAttachedOverlay(
                    pawn,
                    DefDatabase<ThingDef>.GetNamed("Mote_ToxicDamage"),
                    Vector3.zero
                );

                mote.instanceColor = metal.color;
                return;
            }

            TargetTick();
        }

        protected virtual void TargetTick() {
            var distanceToTarget = pawn.Position.DistanceTo(targetPawn.Position);
            if (PawnUtility.IsAsleep(pawn) || distanceToTarget > def.verbProperties.range || !targetPawn.Spawned || targetPawn.Dead) {
                UpdateStatus(BurningStatus.Off);
                return;
            }

            if (def.followTarget && pawn.IsHashIntervalTick(GenTicks.TicksPerRealSecond) && !TryFollowTarget(targetPawn, def.minFollowDistance)) {
                UpdateStatus(BurningStatus.Off);
            }
        }

        private bool TryFollowTarget(Pawn target, float minFollowDistance) {
            if (!pawn.Spawned || !target.Spawned) {
                return false;
            }

            if (!pawn.CanReach(targetPawn, PathEndMode.InteractionCell, Danger.None)) {
                return false;
            }

            var curJob = pawn.CurJob;

            if (curJob?.def == JobDefOf.Follow &&
                curJob.targetA.Thing == target &&
                Math.Abs(curJob.followRadius - minFollowDistance) < 0.01f) {
                // Already following the right target at the right distance
                return true;
            }

            // Too far? Assign new follow job
            var currentDistance = pawn.Position.DistanceTo(target.Position);

            if (!(currentDistance > minFollowDistance)) return true;
            var followJob = JobMaker.MakeJob(JobDefOf.Follow, target);
            followJob.followRadius = minFollowDistance;
            followJob.playerForced = true;

            pawn.jobs.TryTakeOrderedJob(followJob);
            return true;
        }

        public void UpdateStatus(BurningStatus status) {
            Log.Verbose($"Updating {pawn.NameFullColored}'s {def.defName} to {status} from {this.status}");
            var oldStatus = this.status;
            this.status = status;
            metalBurning.UpdateBurnSource(metal, this.status switch {
                BurningStatus.Off => 0,
                BurningStatus.Passive => def.beuPerTick / 2,
                BurningStatus.Burning => def.beuPerTick,
                BurningStatus.Flaring => def.beuPerTick * 2,
                _ => throw new ArgumentOutOfRangeException(),
            }, def);

            switch (this.status) {
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

        public bool Activate(LocalTargetInfo target, LocalTargetInfo dest, bool flare) {
            if (target.Pawn is { } pawnTarget) {
                targetPawn = pawnTarget;
            }

            var baseActivate = base.Activate(target, dest);
            if (!toggleable || !baseActivate) {
                return baseActivate;
            }

            if (flare) {
                UpdateStatus(status == BurningStatus.Flaring ? BurningStatus.Off : BurningStatus.Flaring);
            } else {
                UpdateStatus(PawnUtility.IsAsleep(pawn) ? BurningStatus.Passive : atLeastPassive ? BurningStatus.Off : BurningStatus.Burning);
            }

            return true;
        }

        protected virtual AllomanticHediff GetOrAddHediff() {
            if (targetPawn.health.hediffSet.TryGetHediff(def.hediff, out var ret)) return ret as AllomanticHediff;

            var hediff = (AllomanticHediff)Activator.CreateInstance(def.hediff.hediffClass);
            hediff.def = def.hediff;
            hediff.pawn = pawn;
            hediff.loadID = Find.UniqueIDsManager.GetNextHediffID();
            hediff.sourceAbility = this;
            hediff.PostMake();

            Log.Warning($"Applying {def.hediff.defName} drag to {targetPawn.NameFullColored} with Severity={hediff.Severity}");
            targetPawn.health.AddHediff(hediff);

            return hediff;
        }

        protected virtual void OnEnable() {
            GetOrAddHediff();
        }

        protected virtual void OnDisable() {
            targetPawn = null;
            ApplyDrag(flareDuration / 3000f);
            flareStartTick = -1;
        }

        protected virtual void OnFlare() {
            flareStartTick = Find.TickManager.TicksGame;
            GetOrAddHediff();
            RemoveDrag();
        }

        protected virtual void OnDeFlare() {
            ApplyDrag(flareDuration / 3000f / 2);
            flareStartTick = -1;
        }

        protected virtual void ApplyDrag(float severity) {
            if (def.dragHediff == null || severity < def.minSeverityForDrag) return;

            Log.Warning($"Applying {def.dragHediff.defName} drag to {targetPawn.NameFullColored} with Severity={severity}");
            var drag = targetPawn.health.GetOrAddHediff(def.dragHediff);
            drag.Severity = severity;
        }

        protected virtual void RemoveDrag() {
            var drag = pawn.health.hediffSet.GetFirstHediffOfDef(def.dragHediff);
            if (drag == null) return;

            var existingSeverity = drag.Severity;
            pawn.health.RemoveHediff(drag);
            flareStartTick -= (int)(existingSeverity * 3000f);
        }

        public override void ExposeData() {
            base.ExposeData();

            Scribe_Values.Look(ref flareStartTick, "flareStartTick", -1);
            Scribe_Values.Look(ref status, "status");
            Scribe_References.Look(ref targetPawn, "targetPawn");
        }
    }
}