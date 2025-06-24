using CosmereScadrial.Comps.Things;
using CosmereScadrial.Utils;

namespace CosmereScadrial.Genes;

public class Allomancer : Metalborn {
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
}