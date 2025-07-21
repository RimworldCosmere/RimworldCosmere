using LudeonTK;
using Verse;

namespace Cosmere.Core.Dev;

[StaticConstructorOnStartup]
public static class CoreUtility {
    [DebugAction(
        "Cosmere/Core",
        "Fill Investiture",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void FillPawnInvestiture(Pawn pawn) {
        if (pawn.needs.TryGetNeed<Need.Investiture>() is not { } investiture) return;
        investiture.CurLevel = investiture.MaxLevel;
    }

    [DebugAction(
        "Cosmere/Core",
        "Wipe Investiture",
        actionType = DebugActionType.ToolMapForPawns,
        allowedGameStates = AllowedGameStates.PlayingOnMap
    )]
    public static void WipePawnInvestiture(Pawn pawn) {
        if (pawn.needs.TryGetNeed<Need.Investiture>() is not { } investiture) return;
        investiture.CurLevel = 0;
    }
}