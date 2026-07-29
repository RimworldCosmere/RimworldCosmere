using Cosmere.Core.Settings;
using Cosmere.Core.Settings.Model;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Settings;

public class RosharModSettings : CosmereModSettings {
    private const float hour = 216000;
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

    // 28.8 in-game days.
    public float nahelSprenSpawnAverageIntervalTicks = baseTime;

    // 230.4 in-game days.
    public float nahelSprenSpawnMaxIntervalTicks = baseTime * 8;

    // 14.4 in-game days.
    public float nahelSprenSpawnMinIntervalTicks = baseTime / 2;

    public float progressionDifficulty = 1f;
    public bool showIdealRequirements = true;

    public override string Name => "Roshar";

    public override string SkinId => "Surgebinding";

    public override IReadOnlyList<SettingSection> BuildSections() {
        float minNahelSprenSpawnIntervalTicks = GenDate.TicksPerHour;
        float maxNahelSprenSpawnIntervalTicks = GenDate.DaysToTicks(600f);
        IReadOnlyList<SettingSection> sections = [
            new SettingSection(
                "highstorms",
                "CR_Settings_Category_Highstorm",
                [
                    new SettingDescriptor(
                        "enabled",
                        "CR_Settings_Highstorm_Enabled_Label",
                        "CR_Settings_Highstorm_Enabled_Tooltip",
                        new CheckboxControl(
                            () => enableHighstorms,
                            updated => enableHighstorms = updated,
                            true
                        )
                    ),
                    new SettingDescriptor(
                        "pushing",
                        "CR_Settings_Highstorm_Pushing_Label",
                        "CR_Settings_Highstorm_Pushing_Tooltip",
                        new CheckboxControl(
                            () => enableHighstormPushing,
                            updated => enableHighstormPushing = updated,
                            true
                        )
                    ),
                    new SettingDescriptor(
                        "interval",
                        "CR_Settings_Highstorm_Interval_Label",
                        "CR_Settings_Highstorm_Interval_Tooltip",
                        new IntRangeControl(
                            () => highstormMinIntervalDays,
                            updated => SettingValueMath.SetOrderedMinimum(
                                updated,
                                ref highstormMinIntervalDays,
                                ref highstormMaxIntervalDays,
                                1,
                                30
                            ),
                            () => highstormMaxIntervalDays,
                            updated => SettingValueMath.SetOrderedMaximum(
                                updated,
                                ref highstormMinIntervalDays,
                                ref highstormMaxIntervalDays,
                                1,
                                30
                            ),
                            5,
                            7,
                            1,
                            30
                        )
                    ),
                    new SettingDescriptor(
                        "duration",
                        "CR_Settings_Highstorm_Duration_Label",
                        "CR_Settings_Highstorm_Duration_Tooltip",
                        new TicksControl(
                            () => highstormDurationTicks,
                            updated => highstormDurationTicks = (int)updated,
                            20000,
                            1000,
                            60000,
                            TickUnit.Hours
                        )
                    ),
                    new SettingDescriptor(
                        "weeping",
                        "CR_Settings_Highstorm_EnableWeeping_Label",
                        "CR_Settings_Highstorm_EnableWeeping_Tooltip",
                        new CheckboxControl(
                            () => enableWeeping,
                            updated => enableWeeping = updated,
                            true
                        )
                    ),
                ]
            ),
            new SettingSection(
                "nahel-bonds",
                "CR_Settings_Category_NahelBond",
                [
                    new SettingDescriptor(
                        "average",
                        "CR_Settings_NahelBond_AverageTicks_Label",
                        "CR_Settings_NahelBond_AverageTicks_Tooltip",
                        new TicksControl(
                            () => nahelSprenSpawnAverageIntervalTicks,
                            updated => SettingValueMath.SetOrderedAverage(
                                updated,
                                ref nahelSprenSpawnMinIntervalTicks,
                                ref nahelSprenSpawnAverageIntervalTicks,
                                ref nahelSprenSpawnMaxIntervalTicks,
                                minNahelSprenSpawnIntervalTicks,
                                maxNahelSprenSpawnIntervalTicks
                            ),
                            baseTime,
                            minNahelSprenSpawnIntervalTicks,
                            maxNahelSprenSpawnIntervalTicks,
                            TickUnit.Days
                        )
                    ),
                    new SettingDescriptor(
                        "minimum",
                        "CR_Settings_NahelBond_MinimumTicks_Label",
                        "CR_Settings_NahelBond_MinimumTicks_Tooltip",
                        new TicksControl(
                            () => nahelSprenSpawnMinIntervalTicks,
                            updated => SettingValueMath.SetOrderedMinimum(
                                updated,
                                ref nahelSprenSpawnMinIntervalTicks,
                                ref nahelSprenSpawnAverageIntervalTicks,
                                ref nahelSprenSpawnMaxIntervalTicks,
                                minNahelSprenSpawnIntervalTicks,
                                maxNahelSprenSpawnIntervalTicks
                            ),
                            baseTime / 2,
                            minNahelSprenSpawnIntervalTicks,
                            maxNahelSprenSpawnIntervalTicks,
                            TickUnit.Days
                        )
                    ),
                    new SettingDescriptor(
                        "maximum",
                        "CR_Settings_NahelBond_MaximumTicks_Label",
                        "CR_Settings_NahelBond_MaximumTicks_Tooltip",
                        new TicksControl(
                            () => nahelSprenSpawnMaxIntervalTicks,
                            updated => SettingValueMath.SetOrderedMaximum(
                                updated,
                                ref nahelSprenSpawnMinIntervalTicks,
                                ref nahelSprenSpawnAverageIntervalTicks,
                                ref nahelSprenSpawnMaxIntervalTicks,
                                minNahelSprenSpawnIntervalTicks,
                                maxNahelSprenSpawnIntervalTicks
                            ),
                            baseTime * 8,
                            minNahelSprenSpawnIntervalTicks,
                            maxNahelSprenSpawnIntervalTicks,
                            TickUnit.Days
                        )
                    ),
                ]
            ),
            new SettingSection(
                "lesser-spren",
                "CR_Settings_Category_LesserSpren",
                [
                    new SettingDescriptor(
                        "enabled",
                        "CR_Settings_LesserSpren_Enabled_Label",
                        "CR_Settings_LesserSpren_Enabled_Tooltip",
                        new CheckboxControl(
                            () => lesserSprenSpawn,
                            updated => lesserSprenSpawn = updated,
                            true
                        )
                    ),
                    new SettingDescriptor(
                        "maximum-zoom",
                        "CR_Settings_LesserSpren_MaximumZoom_Label",
                        "CR_Settings_LesserSpren_MaximumZoom_Tooltip",
                        new SliderControl(
                            () => lesserSprenMaxZoomSpawn,
                            updated => lesserSprenMaxZoomSpawn = updated,
                            35f,
                            0f,
                            100f,
                            null,
                            value => value.ToString("0")
                        )
                    ),
                ]
            ),
            new SettingSection(
                "ideal-progression",
                "CR_Settings_Category_IdealProgression",
                [
                    new SettingDescriptor(
                        "difficulty",
                        "CR_Settings_IdealProgression_Difficulty_Label",
                        "CR_Settings_IdealProgression_Difficulty_Tooltip",
                        new SliderControl(
                            () => progressionDifficulty,
                            updated => progressionDifficulty = updated,
                            1f,
                            0.5f,
                            2f,
                            null,
                            value => value.ToString("0.0")
                        )
                    ),
                    new SettingDescriptor(
                        "show-requirements",
                        "CR_Settings_IdealProgression_ShowRequirements_Label",
                        "CR_Settings_IdealProgression_ShowRequirements_Tooltip",
                        new CheckboxControl(
                            () => showIdealRequirements,
                            updated => showIdealRequirements = updated,
                            true
                        )
                    ),
                ]
            ),
            new SettingSection(
                "stormlight",
                "CR_Settings_Category_Stormlight",
                [
                    new SettingDescriptor(
                        "enable-pawn-glow",
                        "CR_Settings_Stormlight_EnablePawnGlow_Label",
                        "CR_Settings_Stormlight_EnablePawnGlow_Tooltip",
                        new CheckboxControl(
                            () => enablePawnGlow,
                            updated => enablePawnGlow = updated,
                            false
                        ),
                        enabled: () => false,
                        disabledReasonKey: "CR_Settings_Stormlight_EnablePawnGlow_DisabledReason"
                    ),
                ]
            ),
        ];

        IReadOnlyList<string> errors = SettingsDescriptorValidator.Validate(Name, sections);
        foreach (string error in errors) {
            Cosmere.Core.Logger.Error($"Settings descriptor validation failed: {error}");
        }

        return sections;
    }

    public override void ExposeData() {
        Scribe_Values.Look(ref enableHighstorms, "enableHighstorms", true);
        Scribe_Values.Look(ref enableHighstormPushing, "enableHighstormPushing", true);
        Scribe_Values.Look(ref enablePawnGlow, "enablePawnGlow");
        Scribe_Values.Look(ref highstormMinIntervalDays, "highstormMinIntervalDays", 5);
        Scribe_Values.Look(ref highstormMaxIntervalDays, "highstormMaxIntervalDays", 7);
        Scribe_Values.Look(ref enableWeeping, "enableWeeping", true);
        Scribe_Values.Look(ref highstormDurationTicks, "highstormDurationTicks", 20000);
        Scribe_Values.Look(ref nahelSprenSpawnAverageIntervalTicks, "nahelSprenSpawnAverageIntervalTicks", baseTime);
        Scribe_Values.Look(ref nahelSprenSpawnMinIntervalTicks, "nahelSprenSpawnMinIntervalTicks", baseTime / 2);
        Scribe_Values.Look(ref nahelSprenSpawnMaxIntervalTicks, "nahelSprenSpawnMaxIntervalTicks", baseTime * 8);
        Scribe_Values.Look(ref lesserSprenSpawn, "lesserSprenSpawn", true);
        Scribe_Values.Look(ref lesserSprenMaxZoomSpawn, "lesserSprenMaxZoomSpawn", 35f);
        Scribe_Values.Look(ref progressionDifficulty, "progressionDifficulty", 1f);
        Scribe_Values.Look(ref showIdealRequirements, "showIdealRequirements", true);
    }
}
