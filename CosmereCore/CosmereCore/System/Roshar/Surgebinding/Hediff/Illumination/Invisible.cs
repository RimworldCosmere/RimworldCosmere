using Cosmere.Core.Ability;
using Cosmere.Core.Hediff;
using Cosmere.System.Roshar.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Hediff.Illumination;

public class Invisible : SurgebindingHediff {
    public Invisible() { }

    public Invisible(HediffDef hediffDef, Pawn pawn, IAbility<Surgebinder, IHediff<Surgebinder>> ability) :
        base(hediffDef, pawn, ability) { }

    public override void PostAdd(DamageInfo? dinfo) {
        base.PostAdd(dinfo);
        if (pawn.Spawned) {
            pawn.Map.attackTargetsCache.UpdateTarget(pawn);
            PortraitsCache.SetDirty(pawn);
            GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
        }
    }

    public override void PostRemoved() {
        base.PostRemoved();
        if (pawn.Spawned) {
            pawn.Map.attackTargetsCache.UpdateTarget(pawn);
            PortraitsCache.SetDirty(pawn);
            GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
        }
    }
}