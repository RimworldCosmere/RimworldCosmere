using Cosmere.Core.UI.Dock;
using Cosmere.Core.UI.Skin;
using Cosmere.System.Nalthis.UI;
using Cosmere.System.Roshar.UI;
using Cosmere.System.Scadrial.UI;
using Verse;

namespace Cosmere.Core.UI.Model;

[StaticConstructorOnStartup]
public static class InvestitureProviderRegistrar {
    static InvestitureProviderRegistrar() {
        PawnInvestitureProviders.Register(new AllomancyInvestitureProvider());
        PawnInvestitureProviders.Register(new FeruchemyInvestitureProvider());
        PawnInvestitureProviders.Register(new SurgebindingInvestitureProvider());
        PawnInvestitureProviders.Register(new AwakeningInvestitureProvider());

        SystemSkinRegistry.Register(new AllomancySkin());
        SystemSkinRegistry.Register(new FeruchemySkin());
        SystemSkinRegistry.Register(new SurgebindingSkin());
        SystemSkinRegistry.Register(new AwakeningSkin());

        DockSectionRegistry.Register(new AllomancyDockSection());
        DockSectionRegistry.Register(new FeruchemyDockSection());
        DockSectionRegistry.Register(new SurgebindingDockSection());
        DockSectionRegistry.Register(new PlaceholderDockSection("Awakening"));
    }
}
