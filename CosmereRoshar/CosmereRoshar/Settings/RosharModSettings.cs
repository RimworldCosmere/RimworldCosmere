using Cosmere.Framework.Listing;
using Cosmere.Framework.Settings;
using Verse;

namespace Cosmere.Roshar.Settings;

public class RosharModSettings : CosmereModSettings {
    private const float hour = 216000;

    // Should probably be like.... 8 hours. Triple speed makes 30 hours happen in < 5 minutes
    private const float baseTime = hour * 8;

    public bool enableHighstormDamage;
    public bool enableHighstormPushing;

    public bool enablePawnGlow;

    // Defaulting to every 2 hours
    public float nahelSprenSpawnAverageIntervalTicks = baseTime;

    // Should be at MOST every 8 hours
    public float nahelSprenSpawnMaxIntervalTicks = baseTime * 8;

    // Should at LEAST be every 30 minutes
    public float nahelSprenSpawnMinIntervalTicks = baseTime / 2;

    public override string Name => "Roshar";

    public override void ExposeData() {
        Scribe_Values.Look(ref enableHighstormPushing, "enableHighstormPushing", true);
        Scribe_Values.Look(ref enablePawnGlow, "enablePawnGlow");
        Scribe_Values.Look(ref enableHighstormDamage, "enableHighstormDamage", true);
        Scribe_Values.Look(
            ref nahelSprenSpawnAverageIntervalTicks,
            "nahelSprenSpawnAverageIntervalTicks",
            baseTime * 2
        );
        Scribe_Values.Look(ref nahelSprenSpawnMinIntervalTicks, "nahelSprenSpawnMinIntervalTicks", baseTime / 2);
        Scribe_Values.Look(ref nahelSprenSpawnMaxIntervalTicks, "nahelSprenSpawnMaxIntervalTicks", baseTime * 8);
    }


    public override void DoTabContents(ListingForm listing) {
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
            "CR_Settings_Category_NahelBond".Translate(),
            fieldset => {
                fieldset.Field(
                    "CR_Settings_NahelBond_AverageTicks_Label".Translate(),
                    "CR_Settings_NahelBond_AverageTicks_Tooltip".Translate(),
                    sub => {
                        string chanceBuffer = nahelSprenSpawnAverageIntervalTicks.ToString();
                        sub.TextFieldNumeric(ref nahelSprenSpawnAverageIntervalTicks, ref chanceBuffer);
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
    }
}