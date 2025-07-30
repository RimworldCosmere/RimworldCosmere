using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Thing;

public class Splinter : Pawn, IThingGlower {
    private readonly ThingDef moteDef = DefDatabase<ThingDef>.GetNamed("InvestitureGlow");
    private Mote? mote;
    private IntVec3 previousPos;

    public bool ShouldBeLitNow() {
        return true;
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad) {
        base.SpawnSetup(map, respawningAfterLoad);
        flight.StartFlying();
    }

    protected override void Tick() {
        base.Tick();

        //if (previousPos != Position) GetComp<CompGlower>().ForceRegister(Map);
        //previousPos = Position;

        if (mote?.Destroyed ?? false) {
            mote = null;
        }

        mote ??= MoteMaker.MakeAttachedOverlay(
            this,
            moteDef,
            Vector3.zero
        );

        mote?.Maintain();
    }
}