using UnityEngine;
using Verse;
using Cosmere.Core.Ability.Autocast;
using Cosmere.Core.Framework;
using Cosmere.Core.Savant;
using Cosmere.Core.ScenarioPart;
using Cosmere.Core.UI.Dock;
using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Radial;
using Cosmere.Core.UI.Skin;
using Cosmere.System.Scadrial.Hemalurgy;
using Cosmere.System.Scadrial.ScenarioPart;
using Cosmere.System.Scadrial.Savant;
using Cosmere.System.Scadrial.UI.Radial;

namespace Cosmere.System.Scadrial.UI;

[StaticConstructorOnStartup]
public static class ScadrialUIRegistration {
    static ScadrialUIRegistration() {
        InvestitureBlockingHediffRegistry.Register(HemalurgicDefOf.Cosmere_Scadrial_Hediff_Drab);
        InvestitureProviderRegistry.Register(new AllomancyInvestitureProvider());
        InvestitureProviderRegistry.Register(new FeruchemyInvestitureProvider());
        SavantCandidateRegistry.Register(new ScadrialSavantCandidateProvider());
        SystemSkinRegistry.Register(new DataSystemSkin(
            systemId: "Allomancy",
            headerLabelKey: "CC_System_Allomancy_Header",
            accentColor: new Color(0.78f, 0.55f, 0.18f),
            barFillColor: new Color(0.85f, 0.63f, 0.22f),
            barBackgroundColor: new Color(0.14f, 0.09f, 0.04f),
            headerTextColor: new Color(0.95f, 0.82f, 0.52f),
            panelBackgroundColor: new Color(0.09f, 0.05f, 0.02f, 0.85f),
            borderTintColor: new Color(0.78f, 0.55f, 0.18f)
        ));
        SystemSkinRegistry.Register(new DataSystemSkin(
            systemId: "Feruchemy",
            headerLabelKey: "CC_System_Feruchemy_Header",
            accentColor: new Color(0.65f, 0.38f, 0.24f),
            barFillColor: new Color(0.80f, 0.45f, 0.28f),
            barBackgroundColor: new Color(0.12f, 0.07f, 0.04f),
            headerTextColor: new Color(0.94f, 0.75f, 0.56f),
            panelBackgroundColor: new Color(0.08f, 0.05f, 0.03f, 0.85f),
            borderTintColor: new Color(0.65f, 0.38f, 0.24f)
        ));
        DockSectionRegistry.Register(new AllomancyDockSection());
        DockSectionRegistry.Register(new FeruchemyDockSection());
        NamedPawnApplierRegistry.Register(new ScadrialNamedPawnApplier());
        RadialActionRegistry.Register(new AllomancyRadialHandler());
        RadialActionRegistry.Register(new FeruchemyRadialHandler());
        AutocastDefaults.Register("Cosmere_Scadrial_Ability_Pewter", [
            new AutocastTrigger(AutocastTriggerKind.Drafted, AutocastComparison.EqualTo, 0f),
        ]);
        AutocastDefaults.Register("Cosmere_Scadrial_Ability_Tin", [
            new AutocastTrigger(AutocastTriggerKind.Drafted, AutocastComparison.EqualTo, 0f),
        ]);
        AutocastDefaults.Register("Cosmere_Scadrial_Ability_CompoundGold", [
            new AutocastTrigger(AutocastTriggerKind.HealthPercent, AutocastComparison.LessThan, 0.4f),
        ]);
    }
}
