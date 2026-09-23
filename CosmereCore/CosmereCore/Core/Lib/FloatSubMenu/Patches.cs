using Concord;
using Verse;

namespace Cosmere.Core.Lib.FloatSubMenu;

internal static class DistanceOverride {
    internal static bool replaceDist;
    internal static float dist;
}

[Patch]
internal abstract class FloatMenuUpdateBaseColorPatch : FloatMenu {
    protected FloatMenuUpdateBaseColorPatch(List<FloatMenuOption> options) : base(options) { }

    [Inject(At.Head, "UpdateBaseColor")]
    private void BeforeUpdateBaseColor() {
        // Make sure we do not replace any values needed to calculate replacement.
        DistanceOverride.replaceDist = false;
        DistanceOverride.replaceDist = FloatSubMenu.ShouldReplaceDistanceFor(this, ref DistanceOverride.dist);
    }

    [Inject(At.Return, "UpdateBaseColor")]
    private void AfterUpdateBaseColor() {
        DistanceOverride.replaceDist = false;
    }
}

[Patch(typeof(GenUI))]
internal static class DistFromRectPatch {
    [Inject(At.Head, nameof(GenUI.DistFromRect))]
    private static Control BeforeDistFromRect(ControlHandle<float> ch) {
        if (!DistanceOverride.replaceDist) return Control.Continue;

        ch.ReturnValue = DistanceOverride.dist;
        return Control.Cancel;
    }
}
