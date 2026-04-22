using LudeonTK;
using Verse;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Playground;

public static class PlaygroundDebugAction
{
    [DebugAction("Cosmere", "Open Lightweave Playground", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Entry | AllowedGameStates.Playing)]
    private static void Open()
    {
        Find.WindowStack.Add(new LightweavePlayground());
    }

    [DebugAction("Cosmere", "Toggle Playground Direction", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Entry | AllowedGameStates.Playing)]
    private static void Toggle()
    {
        LightweavePlayground.DirectionOverride = LightweavePlayground.DirectionOverride == Direction.Rtl ? Direction.Ltr : Direction.Rtl;
    }
}
