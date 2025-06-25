using CosmereScadrial.Util;

namespace CosmereScadrial.Gene;

public class Feruchemist : Metalborn {
    protected override void PostAddOrRemove() {
        MetalbornUtility.HandleFullFeruchemistTrait(pawn);
        MetalbornUtility.HandleFeruchemistTrait(pawn);
    }
}