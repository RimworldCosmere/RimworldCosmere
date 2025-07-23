using Cosmere.Core.Comp.Thing;
using LudeonTK;
using Verse;

namespace Cosmere.Core.Dev;

[StaticConstructorOnStartup]
public static class CoreUtility {
    [DebugAction(
        "Cosmere/Core",
        "Fill Investiture",
        actionType = DebugActionType.ToolMap,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void FillInvestiture() {
        foreach (Thing item in Find.CurrentMap.thingGrid.ThingsListAt(UI.MouseCell())) {
            FillInvestiture(item);
        }
    }

    private static void FillInvestiture(Thing thing) {
        if (!thing.TryGetComp(out InvestitureHolder investiture)) return;
        investiture.currentInvestitureSelf = investiture.maxInvestitureSelf;
        foreach (Thing child in investiture.children) {
            FillInvestiture(child);
        }
    }

    [DebugAction(
        "Cosmere/Core",
        "Wipe Investiture",
        actionType = DebugActionType.ToolMap,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void WipePawnInvestiture() {
        foreach (Thing item in Find.CurrentMap.thingGrid.ThingsListAt(UI.MouseCell())) {
            WipeInvestiture(item);
        }
    }

    private static void WipeInvestiture(Thing thing) {
        if (!thing.TryGetComp(out InvestitureHolder investiture)) return;
        investiture.currentInvestitureSelf = 0;
        foreach (Thing child in investiture.children) {
            WipeInvestiture(child);
        }
    }
}