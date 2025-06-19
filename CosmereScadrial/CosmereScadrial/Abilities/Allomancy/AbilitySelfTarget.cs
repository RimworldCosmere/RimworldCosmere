using RimWorld;
using UnityEngine;
using Verse;
using PawnUtility = CosmereFramework.Utils.PawnUtility;

namespace CosmereScadrial.Abilities.Allomancy;

public class AbilitySelfTarget : AbstractAbility {
    protected bool hasMote = true;
    public AbilitySelfTarget() { }

    public AbilitySelfTarget(Pawn pawn) : base(pawn) { }

    public AbilitySelfTarget(Pawn pawn, Precept sourcePrecept) : base(pawn, sourcePrecept) { }

    public AbilitySelfTarget(Pawn pawn, AbilityDef def) : base(pawn, def) {
        localTarget = pawn;
    }

    public AbilitySelfTarget(Pawn pawn, Precept sourcePrecept, AbilityDef def) : base(pawn, sourcePrecept, def) {
        localTarget = pawn;
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
        if (!hasMote) return;
        Mote? mote = MoteMaker.MakeAttachedOverlay(
            pawn,
            DefDatabase<ThingDef>.GetNamed("Mote_ToxicDamage"),
            Vector3.zero
        );

        mote.instanceColor = metal.color;
    }

    protected override void OnEnable() {
        GetOrAddHediff(pawn);
    }

    protected override void OnDisable() {
        ApplyDrag(pawn, flareDuration / 3000f);
        RemoveHediff(pawn);
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