using System;
using System.Collections.Generic;
using CosmereScadrial.Abilities.Gizmos;
using CosmereScadrial.Abilities.Hediffs;
using CosmereScadrial.Comps.Things;
using CosmereScadrial.Defs;
using RimWorld;
using UnityEngine;
using Verse;
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
        private int flareStartTick = -1;
        public BurningStatus? status;

        public AllomanticAbility(Pawn pawn, AbilityDef def) : base(pawn, def) {
            if (toggleable) {
                status = BurningStatus.Off;
            }
        }

        public AllomanticAbility(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) {
            if (toggleable) {
                status = BurningStatus.Off;
            }
        }

        public float flareDuration => flareStartTick < 0 ? 0 : Find.TickManager.TicksGame - flareStartTick;

        public new AllomanticAbilityDef def => (AllomanticAbilityDef)base.def;

        public bool atLeastPassive => status > BurningStatus.Passive;

        public MetallicArtsMetalDef metal => def.metal;

        private bool toggleable => def.toggleable;

        private MetalBurning metalBurning => pawn.GetComp<MetalBurning>();

        public override bool CanCast => metalBurning.CanBurn(metal, def.beuPerTick);

        public override bool GizmoDisabled(out string reason) {
            if (metalBurning.CanBurn(metal, def.beuPerTick)) return base.GizmoDisabled(out reason);

            reason = "CannotBurn".Translate((NamedArgument)pawn, (NamedArgument)metal);
            return true;
        }

        public override IEnumerable<Command> GetGizmos() {
            var command = new BurnGizmo(this);

            yield return command;
        }

        public override void AbilityTick() {
            base.AbilityTick();

            if (!pawn.IsHashIntervalTick(GenTicks.SecondsToTicks(5)) || !pawn.Spawned || pawn.Map == null) return;

            var mote = MoteMaker.MakeAttachedOverlay(
                pawn,
                DefDatabase<ThingDef>.GetNamed("Mote_ToxicDamage"),
                Vector3.zero
            );

            mote.instanceColor = metal.color;
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
            if (!toggleable) {
                return base.Activate(target, dest);
            }

            if (PawnUtility.IsAsleep(pawn)) {
                if (status == BurningStatus.Off && def.targetRequired) {
                    Messages.Message($"{pawn.NameFullColored} is not allowed to burn while they are asleep.", pawn, MessageTypeDefOf.NegativeEvent);
                    return false;
                }
            }

            if (!base.Activate(target, dest)) {
                return false;
            }

            UpdateStatus(PawnUtility.IsAsleep(pawn) ? BurningStatus.Passive : atLeastPassive ? BurningStatus.Off : BurningStatus.Burning);

            return true;
        }

        protected virtual BurningMetal GetOrAddHediff() {
            if (pawn.health.hediffSet.TryGetHediff(def.hediff, out var ret)) return ret as BurningMetal;

            var hediff = (BurningMetal)Activator.CreateInstance(def.hediff.hediffClass);
            hediff.def = def.hediff;
            hediff.pawn = pawn;
            hediff.loadID = Find.UniqueIDsManager.GetNextHediffID();
            hediff.sourceAbility = this;
            hediff.PostMake();

            pawn.health.AddHediff(hediff);

            return hediff;
        }

        protected virtual void OnEnable() {
            GetOrAddHediff();
        }

        protected virtual void OnDisable() {
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

            Verse.Log.Warning($"Applying {def.dragHediff.defName} drag to {pawn.NameFullColored} with Severity={severity}");
            var drag = pawn.health.GetOrAddHediff(def.dragHediff);
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
        }
    }
}