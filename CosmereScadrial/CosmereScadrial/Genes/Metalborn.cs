using System.Linq;
using CosmereCore.Utils;
using CosmereScadrial.Utils;
using Verse;

namespace CosmereScadrial.Genes {
    public class Metalborn : Gene {
        public override void PostAdd() {
            base.PostAdd();

            if (def.defName is "Cosmere_Mistborn" or "Cosmere_FullFeruchemist") {
                foreach (var gene in pawn.genes.GenesListForReading.Where(gene =>
                             gene is Metalborn && !gene.Equals(this))) {
                    pawn.genes.RemoveGene(gene);
                }
            }

            MetalbornUtility.HandleMetalbornTrait(pawn);
            InvestitureUtility.AssignHeighteningFromBEUs(pawn);
            MetalbornUtility.HandleBurningMetalHediff(pawn);
        }

        public override void PostRemove() {
            base.PostRemove();

            MetalbornUtility.HandleMetalbornTrait(pawn);
            MetalbornUtility.HandleBurningMetalHediff(pawn);
        }
    }
}