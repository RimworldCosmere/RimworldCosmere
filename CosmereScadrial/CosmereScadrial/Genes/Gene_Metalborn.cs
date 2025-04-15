using System.Linq;
using CosmereCore.Utils;
using CosmereScadrial.Comps;
using CosmereScadrial.DefModExtensions;
using CosmereScadrial.Registry;
using CosmereScadrial.Utils;
using RimWorld;
using Verse;

namespace CosmereScadrial.Genes {
    public class Gene_Metalborn : Gene {
        public override void PostAdd() {
            base.PostAdd();

            var trait = DefDatabase<TraitDef>.GetNamedSilentFail(def.defName);
            if (trait != null) {
                // pawn.story.traits.GainTrait(new Trait(trait));
            }

            MetalbornUtility.HandleMetalbornTrait(pawn);
            InvestitureUtility.AssignHeighteningFromBEUs(pawn);

            // Initialize metal reserves
            if (pawn.GetComp<ScadrialInvestiture>() is not { } comp) return;
            if (def.GetModExtension<MetalsLinked>() is not { } ext) return;

            foreach (var metal in ext.metals?.Where(metal => MetalRegistry.Metals.ContainsKey(metal))!) {
                comp.SetReserve(metal, 0);
            }
        }

        public override void PostRemove() {
            base.PostRemove();

            MetalbornUtility.HandleMetalbornTrait(pawn);

            // Initialize metal reserves
            if (pawn.GetComp<ScadrialInvestiture>() is not { } comp) return;
            if (def.GetModExtension<MetalsLinked>() is not { } ext) return;

            foreach (var metal in ext.metals?.Where(metal => MetalRegistry.Metals.ContainsKey(metal))!) {
                comp.RemoveReserve(metal, true);
            }
        }
    }
}