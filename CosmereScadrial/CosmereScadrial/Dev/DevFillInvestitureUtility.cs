using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Comps.Things;
using CosmereScadrial.Defs;
using LudeonTK;
using RimWorld;
using Verse;

namespace CosmereScadrial.Dev {
    [StaticConstructorOnStartup]
    public static class DevFillInvestitureUtility {
        [DebugAction("Cosmere/Scadrial", "Prepare Dev Character",
            actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void PrepareDevCharacter(Pawn pawn) {
            pawn.genes.AddGene(GeneDefOf.Cosmere_Mistborn, true);

            var comp = pawn.GetComp<MetalReserves>();
            foreach (var metal in DefDatabase<MetallicArtsMetalDef>.AllDefsListForReading.Where(x => !x.godMetal && x.allomancy != null)) {
                comp.SetReserve(metal, MetalReserves.MAX_AMOUNT);
                var vial = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed($"Cosmere_AllomanticVial_{metal.LabelCap}"));
                vial.stackCount = 20;
                pawn.inventory.innerContainer.TryAdd(vial);
            }

            Messages.Message($"Made {pawn.NameFullColored} a mistborn, filled reserves and gave 20 vials of each metal.", pawn, MessageTypeDefOf.PositiveEvent);
        }

        [DebugAction("Cosmere/Scadrial", "Fill allomantic reserves (all metals)",
            actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void FillAllReserves(Pawn pawn) {
            var comp = pawn.GetComp<MetalReserves>();
            foreach (var metal in DefDatabase<MetallicArtsMetalDef>.AllDefs) {
                comp.SetReserve(metal, MetalReserves.MAX_AMOUNT);
            }

            Messages.Message($"Filled all reserves for {pawn.LabelShort}", pawn, MessageTypeDefOf.PositiveEvent);
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
    }
}