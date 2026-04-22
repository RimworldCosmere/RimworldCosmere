using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Quickstart;
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
        foreach (Verse.Thing thing in Find.CurrentMap.thingGrid.ThingsListAt(Verse.UI.MouseCell())) {
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
        foreach (Verse.Thing thing in Find.CurrentMap.thingGrid.ThingsListAt(Verse.UI.MouseCell())) {
            thing.TryGetComp<InvestitureHolder>()?.WipeInvestiture();
        }
    }

    [DebugAction(
        "Cosmere/Core",
        "Reload Quickstart",
        allowedGameStates = AllowedGameStates.Entry
    )]
    public static void ReloadQuickstart() {
        Quickstarter.ReloadQuickstart();
    }
}