using RimWorld;
using UnityEngine;
using Verse;
using PawnUtility = CosmereFramework.Utils.PawnUtility;

namespace CosmereScadrial.Abilities {
    public class AllomanticAbilitySelfTarget : AbstractAllomanticAbility {
        public AllomanticAbilitySelfTarget(Pawn pawn, AbilityDef def) : base(pawn, def) {
            target = pawn;
        }

        public AllomanticAbilitySelfTarget(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) {
            target = pawn;
        }

        public override void AbilityTick() {
            base.AbilityTick();

            if (!atLeastPassive) {
                return;
            }

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
        }

        public override bool Activate(LocalTargetInfo target, LocalTargetInfo dest, bool flare = false) {
            var baseActivate = base.Activate(target, dest, flare);
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

        protected override void OnEnable() {
            GetOrAddHediff(pawn);
        }

        protected override void OnDisable() {
            ApplyDrag(pawn, flareDuration / 3000f);
            flareStartTick = -1;
        }

        protected override void OnFlare() {
            flareStartTick = Find.TickManager.TicksGame;
            GetOrAddHediff(pawn);
            RemoveDrag(pawn);
        }

        protected override void OnDeFlare() {
            ApplyDrag(pawn, flareDuration / 3000f / 2);
            flareStartTick = -1;
        }
    }
}