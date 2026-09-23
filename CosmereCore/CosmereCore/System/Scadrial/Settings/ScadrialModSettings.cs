using System;
using Cosmere.Core.Settings;
using Cosmere.Core.Settings.Model;
using Cosmere.System.Scadrial.Grid;
using Cosmere.System.Scadrial.Util;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Settings;

public enum MistsFrequency {
    Daily,
    Weekly,
    Monthly,
}

public class ScadrialModSettings : CosmereModSettings {
    public bool alwaysShowAllomanticAuras;
    public bool enableAshfall = true;
    public bool enableMists = true;

    /// <summary>
    ///     Years from the fourth spike going in to the skin giving out. Rewrites the growth
    ///     hediff's rate - see KolossGrowthTuning.
    /// </summary>
    public float kolossGrowthYears = 8f;

    /// <summary>
    ///     Seconds a koloss stands loose before it turns. Long enough to notice and put another
    ///     Allomancer on it; short enough that losing a hold still costs something.
    /// </summary>
    public float kolossGraceSeconds = 10f;

    /// <summary>
    ///     What holding one costs per interval, against what seizing it cost. The only limit on
    ///     how many a single Allomancer can carry - see KolossRoster.
    /// </summary>
    public float kolossHoldFraction = 0.05f;

    /// <summary>
    ///     Multiplies how hard a koloss or kandra is to seize. Below one they come easily; above
    ///     it, duralumin stops being optional.
    /// </summary>
    public float kolossResistance = 1f;
    public bool mistsArrivalLetter = true;
    public MistsFrequency mistsFrequency = MistsFrequency.Daily;
    public bool pawnsKeepMetalmindsWhenDowned;
    public bool pawnsKeepVialsWhenDowned;

    /// <summary>Cells a vent's fertile ground creeps out to before it stops.</summary>
    public float ventSoilReach = AshVentSoilSpread.DefaultReachCells;

    public override string Name => "Scadrial";

    public override string SkinId => "Allomancy";

    public override IReadOnlyList<SettingSection> BuildSections() {
        IReadOnlyList<SettingSection> sections = [
            new SettingSection(
                "incidents",
                "CS_Settings_Category_Incidents",
                [
                    new SettingDescriptor(
                        "enable-ashfall",
                        "CS_Settings_AshfallEnabled_Label",
                        "CS_Settings_AshfallEnabled_Tooltip",
                        new CheckboxControl(
                            () => enableAshfall,
                            updated => enableAshfall = updated,
                            true
                        )
                    ),
                    new SettingDescriptor(
                        "enable-mists",
                        "CS_Settings_MistsEnabled_Label",
                        "CS_Settings_MistsEnabled_Tooltip",
                        new CheckboxControl(
                            () => enableMists,
                            updated => enableMists = updated,
                            true
                        )
                    ),
                    new SettingDescriptor(
                        "mists-arrival-letter",
                        "CS_Settings_MistsLetter_Label",
                        "CS_Settings_MistsLetter_Tooltip",
                        new CheckboxControl(
                            () => mistsArrivalLetter,
                            updated => mistsArrivalLetter = updated,
                            true
                        )
                    ),
                    new SettingDescriptor(
                        "mists-frequency",
                        "CS_Settings_MistsFrequency_Label",
                        "CS_Settings_MistsFrequency_Tooltip",
                        new ChoiceControl(
                            () => mistsFrequency.ToString(),
                            updated => {
                                if (Enum.TryParse(updated, out MistsFrequency parsed)) mistsFrequency = parsed;
                            },
                            nameof(MistsFrequency.Daily),
                            () => [
                                Choice.Keyed(nameof(MistsFrequency.Daily), "CS_Settings_MistsFrequency_Daily"),
                                Choice.Keyed(nameof(MistsFrequency.Weekly), "CS_Settings_MistsFrequency_Weekly"),
                                Choice.Keyed(nameof(MistsFrequency.Monthly), "CS_Settings_MistsFrequency_Monthly"),
                            ],
                            false
                        )
                    ),
                ]
            ),
            new SettingSection(
                "pawns",
                "CS_Settings_Category_Pawns",
                [
                    new SettingDescriptor(
                        "keep-vials-on-down",
                        "CS_Settings_PawnsKeepVialOnDown_Label",
                        "CS_Settings_PawnsKeepVialOnDown_Tooltip",
                        new ChoiceControl(
                            () => pawnsKeepVialsWhenDowned.ToString(),
                            updated => {
                                if (bool.TryParse(updated, out bool parsed)) pawnsKeepVialsWhenDowned = parsed;
                            },
                            true.ToString(),
                            () => [
                                Choice.Keyed(true.ToString(), "CC_Settings_Choice_Yes"),
                                Choice.Keyed(false.ToString(), "CC_Settings_Choice_No"),
                            ],
                            false
                        )
                    ),
                    new SettingDescriptor(
                        "keep-metalminds-on-down",
                        "CS_Settings_PawnsKeepMetalmindsOnDown_Label",
                        "CS_Settings_PawnsKeepMetalmindsOnDown_Tooltip",
                        new ChoiceControl(
                            () => pawnsKeepMetalmindsWhenDowned.ToString(),
                            updated => {
                                if (bool.TryParse(updated, out bool parsed)) pawnsKeepMetalmindsWhenDowned = parsed;
                            },
                            true.ToString(),
                            () => [
                                Choice.Keyed(true.ToString(), "CC_Settings_Choice_Yes"),
                                Choice.Keyed(false.ToString(), "CC_Settings_Choice_No"),
                            ],
                            false
                        )
                    ),
                ]
            ),
            new SettingSection(
                "allomancy",
                "CS_Settings_Category_Allomancy",
                [
                    new SettingDescriptor(
                        "always-show-auras",
                        "CS_Settings_AlwaysShowAuras_Label",
                        "CS_Settings_AlwaysShowAuras_Tooltip",
                        new CheckboxControl(
                            () => alwaysShowAllomanticAuras,
                            updated => alwaysShowAllomanticAuras = updated,
                            false
                        )
                    ),
                ]
            ),
            new SettingSection(
                "ash",
                "CS_Settings_Category_Ash",
                [
                    new SettingDescriptor(
                        "vent-soil-reach",
                        "CS_Settings_VentSoilReach_Label",
                        "CS_Settings_VentSoilReach_Tooltip",
                        new SliderControl(
                            () => ventSoilReach,
                            updated => ventSoilReach = Mathf.Round(updated),
                            AshVentSoilSpread.DefaultReachCells,
                            AshVentSoilSpread.MinReachCells,
                            AshVentSoilSpread.MaxReachCells,
                            1f,
                            value => value.ToString("0")
                        )
                    ),
                ]
            ),
            new SettingSection(
                "koloss",
                "CS_Settings_Category_Koloss",
                [
                    new SettingDescriptor(
                        "koloss-growth-years",
                        "CS_Settings_KolossGrowthYears_Label",
                        "CS_Settings_KolossGrowthYears_Tooltip",
                        new SliderControl(
                            () => kolossGrowthYears,
                            updated => kolossGrowthYears = Mathf.Round(updated),
                            KolossGrowthTuning.DefaultYears,
                            KolossGrowthTuning.MinYears,
                            KolossGrowthTuning.MaxYears,
                            1f,
                            value => value.ToString("0")
                        )
                    ),
                    new SettingDescriptor(
                        "koloss-grace-seconds",
                        "CS_Settings_KolossGrace_Label",
                        "CS_Settings_KolossGrace_Tooltip",
                        new SliderControl(
                            () => kolossGraceSeconds,
                            updated => kolossGraceSeconds = Mathf.Round(updated),
                            10f,
                            0f,
                            60f,
                            1f,
                            value => value.ToString("0")
                        )
                    ),
                    new SettingDescriptor(
                        "koloss-hold-fraction",
                        "CS_Settings_KolossHold_Label",
                        "CS_Settings_KolossHold_Tooltip",
                        new SliderControl(
                            () => kolossHoldFraction,
                            updated => kolossHoldFraction = updated,
                            0.05f,
                            0f,
                            0.5f,
                            0.01f,
                            value => value.ToStringPercent("0.#")
                        )
                    ),
                    new SettingDescriptor(
                        "koloss-resistance",
                        "CS_Settings_KolossResistance_Label",
                        "CS_Settings_KolossResistance_Tooltip",
                        new SliderControl(
                            () => kolossResistance,
                            updated => kolossResistance = updated,
                            1f,
                            0.25f,
                            3f,
                            0.05f,
                            value => value.ToStringPercent("0")
                        )
                    ),
                ]
            ),
        ];

        IReadOnlyList<string> errors = SettingsDescriptorValidator.Validate(Name, sections);
        foreach (string error in errors) {
            Log.Error($"Settings descriptor validation failed: {error}");
        }

        return sections;
    }

    public override void ExposeData() {
        Scribe_Values.Look(ref kolossGrowthYears, "kolossGrowthYears", 8f);
        Scribe_Values.Look(ref kolossGraceSeconds, "kolossGraceSeconds", 10f);
        Scribe_Values.Look(ref kolossHoldFraction, "kolossHoldFraction", 0.05f);
        Scribe_Values.Look(ref kolossResistance, "kolossResistance", 1f);
        Scribe_Values.Look(ref enableMists, "enableMists", true);
        Scribe_Values.Look(ref mistsFrequency, "mistsFrequency");
        Scribe_Values.Look(ref pawnsKeepMetalmindsWhenDowned, "pawnsKeepMetalmindsWhenDowned", true);
        Scribe_Values.Look(ref pawnsKeepVialsWhenDowned, "pawnsKeepVialsWhenDowned", true);
        Scribe_Values.Look(ref alwaysShowAllomanticAuras, "alwaysShowAllomanticAuras");
        Scribe_Values.Look(ref ventSoilReach, "ventSoilReach", AshVentSoilSpread.DefaultReachCells);
    }
}
