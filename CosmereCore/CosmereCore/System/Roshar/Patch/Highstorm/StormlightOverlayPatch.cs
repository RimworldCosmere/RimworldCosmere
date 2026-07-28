using Concord;
using Cosmere.System.Roshar.Comp.Map;
using RimWorld;

namespace Cosmere.System.Roshar.Patch.Highstorm;

[Patch]
public abstract class StormlightOverlayPatch : MainTabWindow_Architect {
    private static string? stormlightCategoryDefName;

    [Inject(At.Return, nameof(WindowUpdate))]
    private void AfterWindowUpdate() {
        stormlightCategoryDefName ??= "Cosmere_Roshar_DesignationStormlight";

        ArchitectCategoryTab? openTab = selectedDesPanel;
        if (openTab != null && openTab.def.defName == stormlightCategoryDefName) {
            StormlightOverlayDrawHandler.DrawThisFrame();
        }
    }
}
