using Concord;
using Cosmere.System.Roshar.Comp.Map;
using RimWorld;

namespace Cosmere.System.Roshar.Patch.Highstorm;

[Patch]
public abstract class StormlightOverlayPatch : MainTabWindow_Architect {
    private const string StormlightCategoryDefName = "Cosmere_Roshar_DesignationStormlight";

    [Inject(At.Return, nameof(WindowUpdate))]
    private void AfterWindowUpdate() {
        ArchitectCategoryTab? openTab = selectedDesPanel;
        if (openTab != null && openTab.def.defName == StormlightCategoryDefName) {
            StormlightOverlayDrawHandler.DrawThisFrame();
        }
    }
}
