using Cosmere.Core.Ability.Autocast;
using Cosmere.Core.Framework;
using Cosmere.Core.Savant;
using Cosmere.Core.ScenarioPart;
using Cosmere.Core.UI.Dock;
using Cosmere.Core.UI.Model;
using Cosmere.Core.UI.Skin;
using Cosmere.System.Roshar.LesserSpren.SprenController;
using Cosmere.System.Roshar.Savant;
using Cosmere.System.Roshar.ScenarioPart;
using Cosmere.System.Roshar.Surgebinding;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.UI;

[StaticConstructorOnStartup]
public static class RosharUIRegistration {
    static RosharUIRegistration() {
        SprenControllerRegistry.Register(new DeathsprenController());
        SprenControllerRegistry.Register(new FearsprenController());
        SprenControllerRegistry.Register(new FlamesprenController());
        SprenControllerRegistry.Register(new GrasssprenController());
        SprenControllerRegistry.Register(new JoysprenController());
        SprenControllerRegistry.Register(new LifesprenController());
        SprenControllerRegistry.Register(new RainsprenController());
        SprenControllerRegistry.Register(new RiversprenController());
        SprenControllerRegistry.Register(new RocksprenController());
        SprenControllerRegistry.Register(new SandsprenController());
        SprenControllerRegistry.Register(new WavesprenController());
        SprenControllerRegistry.Register(new WindsprenController());

        InvestitureProviderRegistry.Register(new SurgebindingInvestitureProvider());

        // The Radiant plate is stored white on transparent, so GUI.color tints it
        // straight to the rail's ink. Without a sigil the ribbon fell back to
        // printing the first letter of the header.
        SystemSkinRegistry.Register(new DataSystemSkin(
            systemId: "Surgebinding",
            headerLabelKey: "CC_System_Surgebinding_Header",
            accentColor: new Color(0.55f, 0.78f, 1.00f),
            barFillColor: new Color(0.70f, 0.88f, 1.00f),
            barBackgroundColor: new Color(0.03f, 0.06f, 0.12f),
            headerTextColor: new Color(0.90f, 0.95f, 1.00f),
            panelBackgroundColor: new Color(0.03f, 0.06f, 0.12f, 0.85f),
            borderTintColor: new Color(0.55f, 0.78f, 1.00f),
            sigil: () => ContentFinder<Texture2D>.Get("UI/Icons/KnightsRadiant", false)
        ));
        DockSectionRegistry.Register(new SurgebindingDockSection());
        ConnectionStealRegistry.Register(new SurgebinderConnectionStealHandler());
        SavantCandidateRegistry.Register(new RosharSavantCandidateProvider());
        InvestitureHealExclusionRegistry.Register("Cosmere_Roshar_Hediff_NW_");
        NamedPawnApplierRegistry.Register(new RosharNamedPawnApplier());
        AutocastDefaults.Register("Cosmere_Roshar_Ability_Heal", [
            new AutocastTrigger(AutocastTriggerKind.HealthPercent, AutocastComparison.LessThan, 0.6f),
            new AutocastTrigger(AutocastTriggerKind.ReservePercent, AutocastComparison.GreaterThan, 0.1f),
        ]);

        // Draw a breath before the Light runs out rather than after.
        AutocastDefaults.Register("Cosmere_Roshar_Ability_BreatheStormlight", [
            new AutocastTrigger(AutocastTriggerKind.ReservePercent, AutocastComparison.LessThan, 0.2f),
        ]);

        // Drafted is the closest thing to "in combat" the trigger set has: it is the moment the
        // player has decided this pawn is fighting, which is when Blade and Plate should be up.
        AutocastDefaults.Register(
            "Cosmere_Roshar_Ability_ToggleShardblade",
            [new AutocastTrigger(AutocastTriggerKind.Drafted, AutocastComparison.EqualTo, 1f)],
            toggleOffWhenInactive: true
        );
        AutocastDefaults.Register(
            "Cosmere_Roshar_Ability_ToggleShardplate",
            [new AutocastTrigger(AutocastTriggerKind.Drafted, AutocastComparison.EqualTo, 1f)],
            toggleOffWhenInactive: true
        );
    }
}
