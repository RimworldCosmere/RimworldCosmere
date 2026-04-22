using LudeonTK;
using Verse;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Playground;

[StaticConstructorOnStartup]
public static class PlaygroundDebugAction
{
    [DebugAction("Cosmere/Core", "Open Lightweave Playground", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Entry | AllowedGameStates.Playing)]
    public static void Open()
    {
        Find.WindowStack.Add(new LightweavePlayground());
    }

    [DebugAction("Cosmere/Core", "Toggle Playground Direction", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Entry | AllowedGameStates.Playing)]
    public static void Toggle()
    {
        LightweavePlayground.DirectionOverride = LightweavePlayground.DirectionOverride == Direction.Rtl ? Direction.Ltr : Direction.Rtl;
    }
}
