using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.System.Roshar.Def;
using LudeonTK;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Dev;

public static class NightwatcherDebugActions {
    [DebugAction("Cosmere/Roshar", "Open Nightwatcher Dialog",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void OpenNightwatcherDialog(Pawn pawn) {
        CompNightwatcher? comp = pawn.TryGetComp<CompNightwatcher>();
        if (comp != null && comp.HasVisited) {
            Messages.Message($"{pawn.LabelShort} has already visited the Nightwatcher", MessageTypeDefOf.RejectInput);
            return;
        }

        NightwatcherSystem.InitiateSeek(pawn);
    }

    [DebugAction("Cosmere/Roshar", "Reset Nightwatcher Visit",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void ResetNightwatcherVisit(Pawn pawn) {
        CompNightwatcher? comp = pawn.TryGetComp<CompNightwatcher>();
        if (comp == null) {
            Messages.Message($"{pawn.LabelShort} has no CompNightwatcher", MessageTypeDefOf.RejectInput);
            return;
        }

        comp.ResetVisit();
        Messages.Message($"Reset Nightwatcher visit for {pawn.LabelShort}", MessageTypeDefOf.NeutralEvent);
    }

    [DebugAction("Cosmere/Roshar", "Give Nightwatcher Boon",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void GiveNightwatcherBoon(Pawn pawn) {
        List<DebugMenuOption> options = [];
        List<NightwatcherBoonDef> boons = DefDatabase<NightwatcherBoonDef>.AllDefsListForReading;
        for (int i = 0; i < boons.Count; i++) {
            NightwatcherBoonDef boon = boons[i];
            options.Add(new DebugMenuOption($"[T{boon.powerTier}] {boon.LabelCap}", DebugMenuOptionMode.Action, () => {
                NightwatcherSystem.ApplyBoon(pawn, boon);
                Messages.Message($"Applied boon '{boon.LabelCap}' to {pawn.LabelShort}", MessageTypeDefOf.PositiveEvent);
            }));
        }

        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
    }

    [DebugAction("Cosmere/Roshar", "Give Nightwatcher Curse",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap)]
    private static void GiveNightwatcherCurse(Pawn pawn) {
        List<DebugMenuOption> options = [];
        List<NightwatcherCurseDef> curses = DefDatabase<NightwatcherCurseDef>.AllDefsListForReading;
        for (int i = 0; i < curses.Count; i++) {
            NightwatcherCurseDef curse = curses[i];
            options.Add(new DebugMenuOption(curse.LabelCap, DebugMenuOptionMode.Action, () => {
                NightwatcherSystem.ApplyCurse(pawn, curse);
                Messages.Message($"Applied curse '{curse.LabelCap}' to {pawn.LabelShort}", MessageTypeDefOf.NegativeEvent);
            }));
        }

        Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
    }
}
