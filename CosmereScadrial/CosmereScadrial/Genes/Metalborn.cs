using CosmereCore.Utils;
using CosmereScadrial.Utils;
using Verse;

namespace CosmereScadrial.Genes {
    public class Metalborn : Gene {
        public override void PostAdd() {
            base.PostAdd();

            MetalbornUtility.HandleMetalbornTrait(pawn);
            MetalbornUtility.HandleAbilities(pawn, def);
            InvestitureUtility.AssignHeighteningFromBeUs(pawn);
        }

        public override void PostRemove() {
            base.PostRemove();

            MetalbornUtility.HandleMetalbornTrait(pawn);
            MetalbornUtility.HandleAbilities(pawn, def);
        }
    }
}