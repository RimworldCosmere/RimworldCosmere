using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Comps.Things;
using CosmereScadrial.Defs;
using CosmereScadrial.Utils;
using LudeonTK;
using RimWorld;
using Verse;

namespace CosmereScadrial.Dev {
    [StaticConstructorOnStartup]
    public static class ScadrianUtility {
        [DebugAction("Cosmere/Scadrial", "Prepare Dev Pawn",
            actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void PrepareDevPawn(Pawn pawn) {
            if (pawn.genes == null) return;
            pawn.genes?.AddGene(GeneDefOf.Cosmere_Mistborn, true);
            Messages.Message($"Made {pawn.NameFullColored} a mistborn", pawn, MessageTypeDefOf.PositiveEvent);
            FillAllReserves(pawn);
            GiveAllAllomanticVials(pawn);
        }

        [DebugAction("Cosmere/Scadrial", "Give All Allomantic Vials",
            actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void GiveAllAllomanticVials(Pawn pawn) {
            foreach (var metal in DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading.Where(x =>
                         !x.godMetal && x.allomancy != null)) {
                var vial = ThingMaker.MakeThing(
                    DefDatabase<ThingDef>.GetNamed($"Cosmere_AllomanticVial_{metal.LabelCap}"));
                vial.stackCount = 20;
                pawn.inventory.innerContainer.TryAdd(vial);
            }

            Messages.Message($"Gave {pawn.NameFullColored} 20 vials of each metal.", pawn,
                MessageTypeDefOf.PositiveEvent);
        }

        [DebugAction("Cosmere/Scadrial", "Prepare All Dev Pawns", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void PrepareAllDevPawn() {
            foreach (var pawn in Find.CurrentMap.mapPawns.AllPawnsSpawned) {
                PrepareDevPawn(pawn);
            }
        }

        [DebugAction("Cosmere/Scadrial", "Fill allomantic reserves (all metals)",
            actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void FillAllReserves(Pawn pawn) {
            var comp = pawn.GetComp<MetalReserves>();
            foreach (var metal in DefDatabase<MetallicArtsMetalDef>.AllDefs) {
                comp.SetReserve(metal, MetalReserves.MAX_AMOUNT);
            }

            Messages.Message($"Gave {pawn.NameFullColored} full reserves reserves for all metals", pawn,
                MessageTypeDefOf.PositiveEvent);
        }

        [DebugAction("Cosmere/Scadrial", "Wipe allomantic reserves (all metals)",
            actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void WipeAllReserves(Pawn pawn) {
            var comp = pawn.GetComp<MetalReserves>();
            foreach (var metal in DefDatabase<MetallicArtsMetalDef>.AllDefs) comp.RemoveReserve(metal);

            Messages.Message($"Wiped all reserves for {pawn.LabelShort}", pawn, MessageTypeDefOf.PositiveEvent);
        }

        [DebugAction("Cosmere/Scadrial", "Fill specific metal reserve", actionType = DebugActionType.ToolMapForPawns,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void FillSpecificMetal(Pawn pawn) {
            var options = new List<DebugMenuOption>();

            var comp = pawn.GetComp<MetalReserves>();
            foreach (var metal in DefDatabase<MetallicArtsMetalDef>.AllDefs) {
                var label = metal.label.CapitalizeFirst();
                options.Add(new DebugMenuOption(label, DebugMenuOptionMode.Action, () => {
                    comp.SetReserve(metal, MetalReserves.MAX_AMOUNT);
                    Messages.Message($"Filled {label} for {pawn.LabelShort}", pawn, MessageTypeDefOf.PositiveEvent);
                }));
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
        }

        [DebugAction("Cosmere/Scadrial", "Wipe specific metal reserve", actionType = DebugActionType.ToolMapForPawns,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void WipeSpecificMetal(Pawn pawn) {
            var options = new List<DebugMenuOption>();

            var comp = pawn.GetComp<MetalReserves>();
            foreach (var metal in DefDatabase<MetallicArtsMetalDef>.AllDefs) {
                var label = metal.label.CapitalizeFirst();
                options.Add(new DebugMenuOption(label, DebugMenuOptionMode.Action, () => {
                    comp.SetReserve(metal, 0);
                    Messages.Message($"Wiped {label} for {pawn.LabelShort}", pawn, MessageTypeDefOf.PositiveEvent);
                }));
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
        }

        [DebugAction("Cosmere/Scadrial", "Snap Pawn",
            actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SnapPawn(Pawn pawn) {
            SnapUtility.TrySnap(pawn);
        }

        [DebugAction("Cosmere/Scadrial", "Try Give Random Allomantic Ability",
            actionType = DebugActionType.ToolMapForPawns,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void TryGiveRandomAllomanticAbility(Pawn pawn) {
            if (Rand.Chance(1f / 16f)) {
                GeneInheritanceUtility.AddGene(pawn, "Cosmere_Mistborn", false);
            } else {
                GeneInheritanceUtility.TryAddRandomAllomanticGene(pawn, false);
            }
        }

        [DebugAction("Cosmere/Scadrial", "Try Give Random Feruchemical Ability",
            actionType = DebugActionType.ToolMapForPawns,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void TryGiveRandomFeruchemicalAbility(Pawn pawn) {
            if (Rand.Chance(1f / 16f)) {
                GeneInheritanceUtility.AddGene(pawn, "Cosmere_FullFeruchemist", false);
            } else {
                GeneInheritanceUtility.TryAddRandomFeruchemicalGene(pawn, false);
            }
        }
    }
}