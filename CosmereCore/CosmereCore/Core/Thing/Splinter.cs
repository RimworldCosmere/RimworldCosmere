using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Thing;

public class Splinter : Pawn, IThingGlower {
    private readonly ThingDef moteDef = ThingDefOf.Cosmere_Core_Mote_InvestitureGlow;
    private Mote? mote;

    public bool ShouldBeLitNow() {
        return true;
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
        base.SpawnSetup(map, respawningAfterLoad);
        flight.StartFlying();
    }

    protected override void Tick() {
        base.Tick();

        if (mote?.Destroyed ?? false) {
            mote = null;
        }

        if (Map == null) return;

        mote ??= MoteMaker.MakeAttachedOverlay(
            this,
            moteDef,
            Vector3.zero
        );

        mote?.Maintain();
    }
}
