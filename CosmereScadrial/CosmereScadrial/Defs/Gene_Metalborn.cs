using System.Linq;
using CosmereCore.Utils;
using CosmereScadrial.Comps;
using CosmereScadrial.DefModExtensions;
using CosmereScadrial.Registry;
using CosmereScadrial.Utils;
using RimWorld;
using Verse;

namespace CosmereScadrial.Defs {
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
            if (pawn.GetComp<CompScadrialInvestiture>() is not { } comp) return;
            if (def.GetModExtension<MetalLinked>() is not { } ext) return;

            foreach (var metal in ext.metals?.Where(metal => MetalRegistry.Metals.ContainsKey(metal))!) {
                Log.Message($"[Cosmere] {pawn.Name} has access to metal: {metal}");
                comp.SetReserve(metal, 0);
            }
        }

        public override void PostRemove() {
            base.PostRemove();

            MetalbornUtility.HandleMetalbornTrait(pawn);
            var trait = DefDatabase<TraitDef>.GetNamedSilentFail(def.defName);
            if (trait != null) {
                // pawn.story.traits.RemoveTrait(new Trait(trait));
            }

            // Initialize metal reserves
            if (pawn.GetComp<CompScadrialInvestiture>() is not { } comp) return;
            if (def.GetModExtension<MetalLinked>() is not { } ext) return;

            foreach (var metal in ext.metals?.Where(metal => MetalRegistry.Metals.ContainsKey(metal))!) {
                Log.Message($"[Cosmere] {pawn.Name} no longer has access to metal: {metal}");
                comp.RemoveReserve(metal, true);
            }
        }
    }
}