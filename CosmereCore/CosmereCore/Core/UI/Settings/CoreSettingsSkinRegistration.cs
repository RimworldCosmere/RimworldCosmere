using Cosmere.Core.UI.Skin;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI.Settings;

[StaticConstructorOnStartup]
public static class CoreSettingsSkinRegistration {
    static CoreSettingsSkinRegistration() {
        SystemSkinRegistry.Register(new DataSystemSkin(
            systemId: "Core",
            headerLabelKey: "CC_Settings_System_Core",
            accentColor: new Color(0.62f, 0.69f, 0.74f),
            barFillColor: new Color(0.72f, 0.78f, 0.82f),
            barBackgroundColor: new Color(0.06f, 0.08f, 0.10f),
            headerTextColor: new Color(0.78f, 0.84f, 0.88f),
            panelBackgroundColor: new Color(0.06f, 0.08f, 0.10f, 0.85f),
            borderTintColor: new Color(0.62f, 0.69f, 0.74f),
            sigil: () => SettingsTextures.CoreSigil
        ));
    }
}
