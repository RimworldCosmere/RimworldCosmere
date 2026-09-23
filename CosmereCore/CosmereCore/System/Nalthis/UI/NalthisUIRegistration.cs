using Cosmere.Core.UI.Dock;
using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Skin;
using UnityEngine;
using Verse;

namespace Cosmere.System.Nalthis.UI;

[StaticConstructorOnStartup]
public static class NalthisUIRegistration {
    static NalthisUIRegistration() {
        InvestitureProviderRegistry.Register(new AwakeningInvestitureProvider());
        SystemSkinRegistry.Register(new DataSystemSkin(
            systemId: "Awakening",
            headerLabelKey: "CC_System_Awakening_Header",
            accentColor: new Color(0.82f, 0.22f, 0.38f),
            barFillColor: new Color(0.92f, 0.32f, 0.50f),
            barBackgroundColor: new Color(0.10f, 0.02f, 0.05f),
            headerTextColor: new Color(0.98f, 0.75f, 0.80f),
            panelBackgroundColor: new Color(0.10f, 0.02f, 0.05f, 0.85f),
            borderTintColor: new Color(0.82f, 0.22f, 0.38f)
        ));
        DockSectionRegistry.Register(new AwakeningDockSection());
    }
}
