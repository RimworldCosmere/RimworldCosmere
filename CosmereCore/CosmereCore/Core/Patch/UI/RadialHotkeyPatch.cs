using System;
using Concord;
using Cosmere.Core.UI.Radial;
using RimWorld;

namespace Cosmere.Core.Patch;

[Patch]
public abstract class RadialHotkeyPatch : UIRoot_Play {
    [Inject(At.Return, nameof(UIRootOnGUI))]
    private void AfterUIRootOnGUI() {
        try {
            RadialController.OnHotkeyPoll();
        } catch (Exception ex) {
            Log.Error($"radial hotkey poll failed: {ex}");
        }
    }
}
