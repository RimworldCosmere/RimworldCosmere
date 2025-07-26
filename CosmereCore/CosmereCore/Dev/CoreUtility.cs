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
        foreach (Thing thing in Find.CurrentMap.thingGrid.ThingsListAt(UI.MouseCell())) {
            thing.TryGetComp<InvestitureHolder>()?.FillInvestiture();
        }
    }

    [DebugAction(
        "Cosmere/Core",
        "Wipe Investiture",
        actionType = DebugActionType.ToolMap,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void WipePawnInvestiture() {
        foreach (Thing thing in Find.CurrentMap.thingGrid.ThingsListAt(UI.MouseCell())) {
            thing.TryGetComp<InvestitureHolder>()?.WipeInvestiture();
        }
    }
}