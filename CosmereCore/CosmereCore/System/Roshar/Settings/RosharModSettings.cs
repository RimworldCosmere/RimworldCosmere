using Cosmere.Core.Listing;
using Cosmere.Core.Settings;
using Verse;

namespace Cosmere.System.Roshar.Settings;

public class RosharModSettings : CosmereModSettings {
    private const float hour = 216000;

    // Should probably be like.... 8 hours. Triple speed makes 30 hours happen in < 5 minutes
    private const float baseTime = hour * 8;
    public bool enableHighstormPushing = true;

    public bool enableHighstorms = true;

    public bool enablePawnGlow;
    public bool enableWeeping = true;
    public int highstormDurationTicks = 20000;
    public int highstormMaxIntervalDays = 7;
    public int highstormMinIntervalDays = 5;
    public float lesserSprenMaxZoomSpawn = 35f;

    public bool lesserSprenSpawn = true;

    // Defaulting to every 2 hours
    public float nahelSprenSpawnAverageIntervalTicks = baseTime;

    // Should be at MOST every 8 hours
    public float nahelSprenSpawnMaxIntervalTicks = baseTime * 8;

    // Should at LEAST be every 30 minutes
    public float nahelSprenSpawnMinIntervalTicks = baseTime / 2;

    public float progressionDifficulty = 1f;
    public bool showIdealRequirements = true;

    public override string Name => "Roshar";


    public override void ExposeData() {
        Scribe_Values.Look(ref enableHighstorms, "enableHighstorms", true);
        Scribe_Values.Look(ref enableHighstormPushing, "enableHighstormPushing", true);
        Scribe_Values.Look(ref enablePawnGlow, "enablePawnGlow");
        Scribe_Values.Look(ref highstormMinIntervalDays, "highstormMinIntervalDays", 5);
        Scribe_Values.Look(ref highstormMaxIntervalDays, "highstormMaxIntervalDays", 7);
        Scribe_Values.Look(ref enableWeeping, "enableWeeping", true);
        Scribe_Values.Look(ref highstormDurationTicks, "highstormDurationTicks", 20000);
        Scribe_Values.Look(
            ref nahelSprenSpawnAverageIntervalTicks,
            "nahelSprenSpawnAverageIntervalTicks",
            baseTime * 2
        );
        Scribe_Values.Look(ref nahelSprenSpawnMinIntervalTicks, "nahelSprenSpawnMinIntervalTicks", baseTime / 2);
        Scribe_Values.Look(ref nahelSprenSpawnMaxIntervalTicks, "nahelSprenSpawnMaxIntervalTicks", baseTime * 8);
        Scribe_Values.Look(ref lesserSprenSpawn, "lesserSprenSpawn", true);
        Scribe_Values.Look(ref lesserSprenMaxZoomSpawn, "lesserSprenMaxZoomSpawn", 35f);
        Scribe_Values.Look(ref progressionDifficulty, "progressionDifficulty", 1f);
        Scribe_Values.Look(ref showIdealRequirements, "showIdealRequirements", true);
    }


    public override void DoTabContents(Form listing) {
        listing.Fieldset(
            "CR_Settings_Category_Highstorm".Translate(),
            fieldset => {
                fieldset.Field(
                    "CR_Settings_Highstorm_Enabled_Label".Translate(),
                    "CR_Settings_Highstorm_Enabled_Tooltip".Translate(),
                    sub => sub.Checkbox(ref enableHighstorms)
                );

                fieldset.Field(
                    "CR_Settings_Highstorm_Pushing_Label".Translate(),
                    "CR_Settings_Highstorm_Pushing_Tooltip".Translate(),
                    sub => sub.Checkbox(ref enableHighstormPushing)
                );

                fieldset.Field(
                    "CR_Settings_Highstorm_MinInterval_Label".Translate(),
                    "CR_Settings_Highstorm_MinInterval_Tooltip".Translate(),
                    sub => {
                        string buffer = highstormMinIntervalDays.ToString();
                        sub.TextFieldNumeric(ref highstormMinIntervalDays, ref buffer, 1, 30);
                    }
                );

                fieldset.Field(
                    "CR_Settings_Highstorm_MaxInterval_Label".Translate(),
                    "CR_Settings_Highstorm_MaxInterval_Tooltip".Translate(),
                    sub => {
                        string buffer = highstormMaxIntervalDays.ToString();
                        sub.TextFieldNumeric(ref highstormMaxIntervalDays, ref buffer, 1, 30);
                    }
                );

                fieldset.Field(
                    "CR_Settings_Highstorm_EnableWeeping_Label".Translate(),
                    "CR_Settings_Highstorm_EnableWeeping_Tooltip".Translate(),
                    sub => sub.Checkbox(ref enableWeeping)
                );

                fieldset.Field(
                    "CR_Settings_Highstorm_Duration_Label".Translate(),
                    "CR_Settings_Highstorm_Duration_Tooltip".Translate(),
                    sub => {
                        string buffer = highstormDurationTicks.ToString();
                        sub.TextFieldNumeric(ref highstormDurationTicks, ref buffer, 1000, 60000);
                    }
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
            "CR_Settings_Category_LesserSpren".Translate(),
            fieldset => {
                fieldset.Field(
                    "CR_Settings_LesserSpren_Enabled_Label".Translate(),
                    "CR_Settings_LesserSpren_Enabled_Tooltip".Translate(),
                    sub => sub.Checkbox(ref lesserSprenSpawn)
                );
                fieldset.Field(
                    "CR_Settings_LesserSpren_MaximumZoom_Label".Translate(),
                    "CR_Settings_LesserSpren_MaximumZoom_Tooltip".Translate(),
                    sub => { lesserSprenMaxZoomSpawn = sub.Slider(lesserSprenMaxZoomSpawn, 0, 100); }
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
            "CR_Settings_Category_IdealProgression".Translate(),
            fieldset => {
                fieldset.Field(
                    "CR_Settings_IdealProgression_Difficulty_Label".Translate(),
                    "CR_Settings_IdealProgression_Difficulty_Tooltip".Translate(),
                    sub => { progressionDifficulty = sub.Slider(progressionDifficulty, 0.5f, 2f); }
                );
                fieldset.Field(
                    "CR_Settings_IdealProgression_ShowRequirements_Label".Translate(),
                    "CR_Settings_IdealProgression_ShowRequirements_Tooltip".Translate(),
                    sub => sub.Checkbox(ref showIdealRequirements)
                );
            }
        );
    }
}