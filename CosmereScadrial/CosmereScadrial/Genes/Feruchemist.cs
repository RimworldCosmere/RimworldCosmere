using CosmereScadrial.Utils;

namespace CosmereScadrial.Genes;

public class Feruchemist : Metalborn {
    protected override void PostAddOrRemove() {
        MetalbornUtility.HandleFullFeruchemistTrait(pawn);
        MetalbornUtility.HandleFeruchemistTrait(pawn);
    }
}