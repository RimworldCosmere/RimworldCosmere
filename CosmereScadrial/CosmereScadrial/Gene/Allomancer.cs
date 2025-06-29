using CosmereScadrial.Allomancy.Comp.Thing;
using CosmereScadrial.Util;
using Verse;

namespace CosmereScadrial.Gene;

public class Allomancer : Metalborn {
    public int RequestedVialStock = 3;
    public override float Max => MetalReserves.MaxAmount;
    public override float Value => reserves.GetReserve(metal);

    private MetalReserves reserves => pawn.GetComp<MetalReserves>();

    public override void Reset() {
        targetValue = 0.15f;
    }

    protected override void PostAddOrRemove() {
        MetalbornUtility.HandleMistbornTrait(pawn);
        MetalbornUtility.HandleAllomancerTrait(pawn);
    }

    public bool ShouldConsumeVialNow() {
        return Value < (double)targetValue;
    }

    public override void ExposeData() {
        base.ExposeData();

        Scribe_Values.Look(ref RequestedVialStock, "RequestedVialStock", 3);
    }
}