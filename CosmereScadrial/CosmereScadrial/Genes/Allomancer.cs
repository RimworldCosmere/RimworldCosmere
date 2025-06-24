using CosmereScadrial.Comps.Things;
using CosmereScadrial.Utils;
using Verse;

namespace CosmereScadrial.Genes;

public class Allomancer : Metalborn {
    public int RequestedVialStock = 3;
    public override float Max => MetalReserves.MaxAmount;
    public override float Value => reserves.GetReserve(metal);
    public override float ValuePercent => AllomancyUtility.GetReservePercent(pawn, metal);

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