using System.Collections.Generic;
using System.Linq;
using CosmereCore.Utils;
using CosmereResources.ModExtensions;
using CosmereScadrial.Defs;
using CosmereScadrial.Gizmo;
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

        public override IEnumerable<Verse.Gizmo> GetGizmos() {
            foreach (var gizmo in base.GetGizmos() ?? []) {
                yield return gizmo;
            }

            var metals = def.GetModExtension<MetalsLinked>().Metals;
            foreach (var metal in metals) {
                yield return new InvestitureAllomancyGizmo(this, MetallicArtsMetalDef.GetFromMetalDef(metal));
            }
        }
    }
}