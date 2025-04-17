using CosmereCore.Utils;
using CosmereScadrial.Utils;
using RimWorld;
using Verse;

namespace CosmereScadrial.Genes {
    public class Metalborn : Gene {
        public override void PostAdd() {
            base.PostAdd();

            var trait = DefDatabase<TraitDef>.GetNamedSilentFail(def.defName);
            if (trait != null) {
                // pawn.story.traits.GainTrait(new Trait(trait));
            }

            MetalbornUtility.HandleMetalbornTrait(pawn);
            InvestitureUtility.AssignHeighteningFromBeUs(pawn);
        }

        public override void PostRemove() {
            base.PostRemove();

            MetalbornUtility.HandleMetalbornTrait(pawn);
        }
    }
}