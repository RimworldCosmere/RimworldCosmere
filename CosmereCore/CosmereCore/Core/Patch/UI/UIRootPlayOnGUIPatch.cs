using Concord;
using Cosmere.Core.UI.Dock;
using RimWorld;
using Verse;

namespace Cosmere.Core.Patch;

[Patch]
public abstract class UIRootPlayOnGUIPatch : UIRoot_Play {
    [Inject(At.Return, nameof(UIRootOnGUI))]
    private void AfterUIRootOnGUI() {
        if (Current.ProgramState != ProgramState.Playing) return;
        if (Find.CurrentMap == null) return;
        InvestitureDockController.UpdateVisibility();
    }
}
