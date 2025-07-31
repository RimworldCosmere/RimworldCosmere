using Cosmere.Framework.Listing;
using Cosmere.Framework.Settings;
using UnityEngine;
using Verse;

namespace Cosmere.Roshar.Settings;

public class RosharModSettings : CosmereModSettings {
    // Base chance should be about once every 2 hours
    private const float twoHours = 432000;
    public float baseNahelSprenSpawnChance = 1f / twoHours;
    public float bondChanceMultiplier = 0;
    public bool devOptionAutofillSpheres;
    public bool enableHighstormDamage;
    public bool enableHighstormPushing;

    public bool enablePawnGlow;

    // Should be at MOST every 8 hours
    public float nahelSprenSpawnMaxIntervalTicks = twoHours * 4;

    // Should at LEAST be every 30 minutes
    public float nahelSprenSpawnMinIntervalTicks = twoHours / 4;

    public override string Name => "Roshar";

    public override void ExposeData() {
        Scribe_Values.Look(ref enableHighstormPushing, "enableHighstormPushing", true);
        Scribe_Values.Look(ref enablePawnGlow, "enablePawnGlow");
        Scribe_Values.Look(ref devOptionAutofillSpheres, "devOptionAutofillSpheres");
        Scribe_Values.Look(ref enableHighstormDamage, "enableHighstormDamage", true);
        Scribe_Values.Look(ref baseNahelSprenSpawnChance, "baseNahelSprenSpawnChance", 1f / twoHours);
        Scribe_Values.Look(ref nahelSprenSpawnMinIntervalTicks, "nahelSprenSpawnMinIntervalTicks", twoHours / 4);
        Scribe_Values.Look(ref nahelSprenSpawnMaxIntervalTicks, "nahelSprenSpawnMaxIntervalTicks", twoHours * 4);
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
            "CR_Settings_Category_NahelBond".Translate(),
            fieldset => {
                fieldset.Field(
                    "CR_Settings_NahelBond_BaseChance_Label".Translate(),
                    "CR_Settings_NahelBond_BaseChance_Tooltip".Translate(),
                    sub => {
                        string chanceBuffer = baseNahelSprenSpawnChance.ToString();
                        sub.TextFieldNumeric(ref baseNahelSprenSpawnChance, ref chanceBuffer);
                    }
                );
                fieldset.Field(
                    "CR_Settings_NahelBond_MinimumTicks_Label".Translate(),
                    "CR_Settings_NahelBond_MinimumTicks_Tooltip".Translate(),
                    sub => {
                        string intervalBuffer = nahelSprenSpawnMinIntervalTicks.ToString();
                        sub.TextFieldNumeric(ref nahelSprenSpawnMinIntervalTicks, ref intervalBuffer);
                    }
                );
                fieldset.Field(
                    "CR_Settings_NahelBond_MaximumTicks_Label".Translate(),
                    "CR_Settings_NahelBond_MaximumTicks_Tooltip".Translate(),
                    sub => {
                        string intervalBuffer = nahelSprenSpawnMaxIntervalTicks.ToString();
                        sub.TextFieldNumeric(ref nahelSprenSpawnMaxIntervalTicks, ref intervalBuffer);
                    }
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