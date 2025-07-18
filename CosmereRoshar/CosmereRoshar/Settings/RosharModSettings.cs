using CosmereFramework.Extension;
using CosmereFramework.Listing;
using CosmereFramework.Settings;
using UnityEngine;
using Verse;

namespace CosmereRoshar.Settings;

public class RosharModSettings : CosmereModSettings {
    public float bondChanceMultiplier;
    public string bondChanceMultiplierBuffer;
    public bool devOptionAutofillSpheres;
    public bool enableHighstormDamage;
    public bool enableHighstormPushing;
    public bool enablePawnGlow;

    public override string Name => "Roshar";

    public override void ExposeData() {
        Scribe_Values.Look(ref enableHighstormPushing, "enableHighstormPushing", true);
        Scribe_Values.Look(ref enablePawnGlow, "enablePawnGlow");
        Scribe_Values.Look(ref devOptionAutofillSpheres, "devOptionAutofillSpheres");
        Scribe_Values.Look(ref enableHighstormDamage, "enableHighstormDamage", true);
        Scribe_Values.Look(ref bondChanceMultiplier, "bondChanceMultiplier", 1);
    }


    public override void DoTabContents(Rect inRect, ListingForm listing) {
        listing.Fieldset(
            "CR_Settings_Category_Highstorm".Translate(),
            fieldset => {
                fieldset.Field(
                    "CR_Settings_Highstorm_Pushing_Label".Translate(),
                    "CR_Settings_Highstorm_Pushing_Tooltip".Translate(),
                    sub => sub.Checkbox(ref enableHighstormPushing)
                );

                fieldset.Field(
                    "CR_Settings_Highstorm_Damage_Label".Translate(),
                    "CR_Settings_Highstorm_Damage_Tooltip".Translate(),
                    sub => sub.Checkbox(ref enableHighstormDamage)
                );
            }
        );

        listing.Fieldset(
            "CR_Settings_Category_Stormlight".Translate(),
            fieldset => {
                fieldset.Field(
                    "CR_Settings_Stormlight_EnablePawnGlow_Label".Translate(),
                    "CR_Settings_Stormlight_EnablePawnGlow_Tooltip".Translate(),
                    sub => sub.Checkbox(
                        ref enablePawnGlow,
                        disabled: true
                    )
                );
            }
        );

        listing.Fieldset(
            "CR_Settings_Category_Surgebinding".Translate(),
            fieldset => {
                fieldset.Field(
                    "CR_Settings_Stormlight_BondChanceMultiplier_Label".Translate(),
                    "CR_Settings_Stormlight_BondChanceMultiplier_Tooltip".Translate(),
                    sub => sub.TextFieldNumeric(ref bondChanceMultiplier, ref bondChanceMultiplierBuffer, 1, 10000)
                );
            }
        );

        if (!Prefs.DevMode) return;
        listing.Fieldset(
            "CR_Settings_Category_Development".Translate(),
            fieldset => {
                fieldset.Field(
                    "CR_Settings_Development_AutofillSpheres_Label".Translate(),
                    "CR_Settings_Development_AutofillSpheres_Tooltip".Translate(),
                    sub => sub.Checkbox(ref devOptionAutofillSpheres)
                );
            }
        );
    }
}