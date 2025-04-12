using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Comps;
using CosmereScadrial.DefModExtensions;
using CosmereScadrial.Registry;
using LudeonTK;
using RimWorld;
using Verse;

namespace CosmereScadrial.Dev {
    [StaticConstructorOnStartup]
    public static class DevFillInvestitureUtility {
        [DebugAction("Cosmere/Scadrial", "Fill allomantic reserves (all metals)",
            actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void FillAllReserves(Pawn pawn) {
            var comp = pawn.GetComp<CompScadrialInvestiture>();
            if (comp == null) return;

            foreach (var metal in MetalRegistry.Metals.Values) comp.SetReserve(metal.Name, metal.MaxAmount);

            Messages.Message($"Filled all reserves for {pawn.LabelShort}", pawn, MessageTypeDefOf.PositiveEvent);
        }

        [DebugAction("Cosmere/Scadrial", "Wipe allomantic reserves (all metals)",
            actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void WipeAllReserves(Pawn pawn) {
            var comp = pawn.GetComp<CompScadrialInvestiture>();
            if (comp == null) return;

            foreach (var metal in MetalRegistry.Metals.Values) comp.SetReserve(metal.Name, 0);

            Messages.Message($"Wiped all reserves for {pawn.LabelShort}", pawn, MessageTypeDefOf.PositiveEvent);
        }

        [DebugAction("Cosmere/Scadrial", "Fill reserves based on genes",
            actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void FillReservesFromGenes(Pawn pawn) {
            var comp = pawn.GetComp<CompScadrialInvestiture>();
            if (comp == null) return;

            var metals = pawn.genes?.GenesListForReading
                .SelectMany(g => g.def?.GetModExtension<MetalLinked>()?.metals ?? Enumerable.Empty<string>())
                .Distinct()
                .ToList();

            foreach (var metalName in metals) {
                if (MetalRegistry.Metals.TryGetValue(metalName, out var metal)) {
                    comp.SetReserve(metal.Name, metal.MaxAmount);
                }
            }

            Messages.Message($"Filled {metals.Count} reserves for {pawn.LabelShort}", pawn,
                MessageTypeDefOf.PositiveEvent);
        }

        [DebugAction("Cosmere/Scadrial", "Fill specific metal reserve", actionType = DebugActionType.ToolMapForPawns,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void FillSpecificMetal(Pawn pawn) {
            List<DebugMenuOption> options = new List<DebugMenuOption>();

            foreach (var metal in MetalRegistry.Metals.Values.OrderBy(m => m.Name)) {
                var label = metal.Name.CapitalizeFirst();
                options.Add(new DebugMenuOption(label, DebugMenuOptionMode.Action, () => {
                    var comp = pawn.GetComp<CompScadrialInvestiture>();
                    if (comp != null) {
                        comp.SetReserve(metal.Name, metal.MaxAmount);
                        Messages.Message($"Filled {label} for {pawn.LabelShort}", pawn, MessageTypeDefOf.PositiveEvent);
                    }
                }));
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
        }

        [DebugAction("Cosmere/Scadrial", "Wipe specific metal reserve", actionType = DebugActionType.ToolMapForPawns,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void WipeSpecificMetal(Pawn pawn) {
            List<DebugMenuOption> options = new List<DebugMenuOption>();

            foreach (var metal in MetalRegistry.Metals.Values.OrderBy(m => m.Name)) {
                var label = metal.Name.CapitalizeFirst();
                options.Add(new DebugMenuOption(label, DebugMenuOptionMode.Action, () => {
                    var comp = pawn.GetComp<CompScadrialInvestiture>();
                    if (comp != null) {
                        comp.SetReserve(metal.Name, 0);
                        Messages.Message($"Wiped {label} for {pawn.LabelShort}", pawn, MessageTypeDefOf.PositiveEvent);
                    }
                }));
            }

            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
        }
    }
}