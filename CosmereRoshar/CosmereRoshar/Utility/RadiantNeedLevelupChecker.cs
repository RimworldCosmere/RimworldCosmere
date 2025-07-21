using Cosmere.Roshar.Patches;

namespace Cosmere.Roshar.Utility;

public static class RadiantNeedLevelupChecker {
    public static void UpdateIsSatisfiedReq1_2(PawnStats pawnStats) {
        RadiantRequirements? windrunnerRequirement =
            pawnStats.requirementMap[Defs.Cosmere_Roshar_Trait_Radiant_Windrunner.defName][pawnStats.props
                .req12];
        if (windrunnerRequirement.count >= 1 && pawnStats.patientSaved) {
            windrunnerRequirement.isSatisfied = true;
        }

        RadiantRequirements? truthwatcherRequirement =
            pawnStats.requirementMap[Defs.Cosmere_Roshar_Trait_RadiantTruthwatcher.defName][
                pawnStats.props.req12];
        if (truthwatcherRequirement.count >= 1 && pawnStats.patientSaved) {
            truthwatcherRequirement.isSatisfied = true;
        }
    }

    public static void UpdateIsSatisfiedReq2_3(PawnStats pawnStats) {
        //helped enemy in need
        RadiantRequirements? windrunnerRequirement =
            pawnStats.requirementMap[Defs.Cosmere_Roshar_Trait_Radiant_Windrunner.defName][pawnStats.props
                .req23];
        if (windrunnerRequirement.count >= 1 && pawnStats.enemyPatientSaved) {
            windrunnerRequirement.isSatisfied = true;
        }
    }

    public static void UpdateIsSatisfiedReq3_4(PawnStats pawnStats) {
        //ally with bond died even tho tried to save
        RadiantRequirements? windrunnerRequirement =
            pawnStats.requirementMap[Defs.Cosmere_Roshar_Trait_Radiant_Windrunner.defName][pawnStats.props
                .req34];
        windrunnerRequirement.isSatisfied = true;
    }

    public static void UpdateIsSatisfiedReq4_5(PawnStats pawnStats) {
        //??
    }
}