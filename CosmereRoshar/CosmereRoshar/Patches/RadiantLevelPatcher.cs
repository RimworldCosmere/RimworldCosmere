using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereRoshar.Patches;

// @todo Move these to where they belong.
public class RadiantRequirements : IExposable {
    public int count;
    public bool isSatisfied;
    public float value;

    public void ExposeData() {
        Scribe_Values.Look(ref isSatisfied, "IsSatisfied");
        Scribe_Values.Look(ref value, "Value");
        Scribe_Values.Look(ref count, "Count");
    }
}

public class RequirementMapEntry : IExposable {
    public Dictionary<string, RadiantRequirements> innerDict;
    public string outerKey;

    public void ExposeData() {
        Scribe_Values.Look(ref outerKey, "outerKey");
        Scribe_Collections.Look(ref innerDict, "innerDict", LookMode.Value, LookMode.Deep);
    }
}

public class CompPropertiesPawnStats : CompProperties {
    public string req01;
    public string req12;
    public string req23;
    public string req34;
    public string req45;

    public CompPropertiesPawnStats() {
        compClass = typeof(PawnStats);
    }
}

public class PawnStats : ThingComp {
    public int doCheckWhenThisIsZero;
    public bool enemyPatientDied;
    public bool enemyPatientSaved;
    public bool hasFormedBond;
    public bool patientDied;
    public List<Pawn> patientList = [];
    public bool patientSaved;

    public Dictionary<string, Dictionary<string, RadiantRequirements>> requirementMap =
        new Dictionary<string, Dictionary<string, RadiantRequirements>>();

    private List<RequirementMapEntry> requirementMapSerialized = [];
    public new CompPropertiesPawnStats props => (CompPropertiesPawnStats)base.props;

    public override void PostExposeData() {
        Scribe_Collections.Look(ref patientList, "PatientList", LookMode.Reference);
        Scribe_Values.Look(ref patientDied, "PatientDied");
        Scribe_Values.Look(ref patientSaved, "PatientSaved");
        Scribe_Values.Look(ref enemyPatientDied, "EnemyPatientDied");
        Scribe_Values.Look(ref enemyPatientSaved, "EnemyPatientSaved");
        Scribe_Values.Look(ref hasFormedBond, "hasFormedBond");
        if (Scribe.mode == LoadSaveMode.Saving) {
            requirementMapSerialized.Clear();
            foreach (KeyValuePair<string, Dictionary<string, RadiantRequirements>> outerPair in requirementMap) {
                requirementMapSerialized.Add(
                    new RequirementMapEntry {
                        outerKey = outerPair.Key,
                        innerDict = outerPair.Value,
                    }
                );
            }
        }

        Scribe_Collections.Look(ref requirementMapSerialized, "requirementMapSerialized", LookMode.Deep);

        if (Scribe.mode == LoadSaveMode.LoadingVars) {
            requirementMap.Clear();
            if (requirementMapSerialized != null) {
                foreach (RequirementMapEntry? entry in requirementMapSerialized) {
                    if (entry != null && entry.outerKey != null) {
                        requirementMap[entry.outerKey] = entry.innerDict;
                    }
                }
            }
        }
    }

    public override void Initialize(CompProperties? props) {
        base.Initialize(props);

        //WINDRUNNER
        string? windrunnerDefName = CosmereRosharDefs.Cosmere_Roshar_Trait_RadiantWindrunner.defName;
        requirementMap.Add(windrunnerDefName, new Dictionary<string, RadiantRequirements>());
        requirementMap[windrunnerDefName].Add(this.props.req01, new RadiantRequirements());
        requirementMap[windrunnerDefName].Add(this.props.req12, new RadiantRequirements());
        requirementMap[windrunnerDefName].Add(this.props.req23, new RadiantRequirements());
        requirementMap[windrunnerDefName].Add(this.props.req34, new RadiantRequirements());


        //TRUTHWATCHER
        string? truthwatcherDefName = CosmereRosharDefs.Cosmere_Roshar_Trait_RadiantTruthwatcher.defName;
        requirementMap.Add(truthwatcherDefName, new Dictionary<string, RadiantRequirements>());
        requirementMap[truthwatcherDefName].Add(this.props.req01, new RadiantRequirements());
        requirementMap[truthwatcherDefName].Add(this.props.req12, new RadiantRequirements());
        requirementMap[truthwatcherDefName].Add(this.props.req23, new RadiantRequirements());
        requirementMap[truthwatcherDefName].Add(this.props.req34, new RadiantRequirements());
        requirementMap[truthwatcherDefName][this.props.req23].isSatisfied = true; // for now it is true default
        requirementMap[truthwatcherDefName][this.props.req34].isSatisfied = true; // for now it is true default


        //EDGEDANCER
        string? edgedancerDefName = CosmereRosharDefs.Cosmere_Roshar_Trait_RadiantEdgedancer.defName;
        requirementMap.Add(edgedancerDefName, new Dictionary<string, RadiantRequirements>());
        requirementMap[edgedancerDefName].Add(this.props.req01, new RadiantRequirements());
        requirementMap[edgedancerDefName].Add(this.props.req12, new RadiantRequirements());
        requirementMap[edgedancerDefName][this.props.req12].isSatisfied = true; // for now it is true default

        //SKYBREAKER
        string? skybreakerDefName = CosmereRosharDefs.Cosmere_Roshar_Trait_RadiantSkybreaker.defName;
        requirementMap.Add(skybreakerDefName, new Dictionary<string, RadiantRequirements>());
        requirementMap[skybreakerDefName].Add(this.props.req01, new RadiantRequirements());
        requirementMap[skybreakerDefName].Add(this.props.req12, new RadiantRequirements());
        requirementMap[skybreakerDefName].Add(this.props.req23, new RadiantRequirements());
        requirementMap[skybreakerDefName].Add(this.props.req34, new RadiantRequirements());
        requirementMap[skybreakerDefName][this.props.req01].isSatisfied = true; // for now it is true default
        requirementMap[skybreakerDefName][this.props.req12].isSatisfied = true; // for now it is true default
        requirementMap[skybreakerDefName][this.props.req23].isSatisfied = true; // for now it is true default
        requirementMap[skybreakerDefName][this.props.req34].isSatisfied = true; // for now it is true default
    }

    public RadiantRequirements GetRequirements(TraitDef trait, string req) {
        if (requirementMap[trait.defName].ContainsKey(req)) {
            return requirementMap?[trait.defName]?[req];
        }

        return requirementMap[trait.defName][props.req01];
    }

    public RadiantRequirements GetRequirementsEntry() {
        return requirementMap[CosmereRosharDefs.Cosmere_Roshar_Trait_RadiantWindrunner.defName][props.req01];
    }
}

public static class Cosmere_Roshar_Radiant_NeedLevelupChecker {
    public static void UpdateIsSatisfiedReq1_2(PawnStats pawnStats) {
        RadiantRequirements? windrunnerRequirement =
            pawnStats.requirementMap[CosmereRosharDefs.Cosmere_Roshar_Trait_RadiantWindrunner.defName][pawnStats.props
                .req12];
        if (windrunnerRequirement.count >= 1 && pawnStats.patientSaved) {
            windrunnerRequirement.isSatisfied = true;
        }

        RadiantRequirements? truthwatcherRequirement =
            pawnStats.requirementMap[CosmereRosharDefs.Cosmere_Roshar_Trait_RadiantTruthwatcher.defName][
                pawnStats.props.req12];
        if (truthwatcherRequirement.count >= 1 && pawnStats.patientSaved) {
            truthwatcherRequirement.isSatisfied = true;
        }
    }

    public static void UpdateIsSatisfiedReq2_3(PawnStats pawnStats) {
        //helped enemy in need
        RadiantRequirements? windrunnerRequirement =
            pawnStats.requirementMap[CosmereRosharDefs.Cosmere_Roshar_Trait_RadiantWindrunner.defName][pawnStats.props
                .req23];
        if (windrunnerRequirement.count >= 1 && pawnStats.enemyPatientSaved) {
            windrunnerRequirement.isSatisfied = true;
        }
    }

    public static void UpdateIsSatisfiedReq3_4(PawnStats pawnStats) {
        //ally with bond died even tho tried to save
        RadiantRequirements? windrunnerRequirement =
            pawnStats.requirementMap[CosmereRosharDefs.Cosmere_Roshar_Trait_RadiantWindrunner.defName][pawnStats.props
                .req34];
        windrunnerRequirement.isSatisfied = true;
    }

    public static void UpdateIsSatisfiedReq4_5(PawnStats pawnStats) {
        //??
    }
}

//Initial requirements, must have suffered crisis
[HarmonyPatch(typeof(MentalBreaker), nameof(MentalBreaker.MentalBreakerTickInterval))]
public static class Cosmere_Roshar_MentalBreakExperiences {
    private static bool ColonistFound = false;

    private static readonly AccessTools.FieldRef<MentalBreaker, Pawn> pawnRef =
        AccessTools.FieldRefAccess<Pawn>(typeof(MentalBreaker), "pawn");

    private static void Postfix(MentalBreaker __instance, int delta) {
        if (!GenTicks.IsTickIntervalDelta(GenTicks.TicksPerRealSecond, delta)) return;
        Pawn? pawn = pawnRef(__instance);
        if (pawn.NonHumanlikeOrWildMan() || !pawn.IsColonist) return;
        PawnStats pawnStats = pawn.GetComp<PawnStats>();
        if (pawnStats == null || pawnStats.hasFormedBond) return;

        float increment = 0f;
        if (__instance.BreakExtremeIsImminent) {
            increment = 2.5f * CosmereRoshar.bondChanceMultiplier;
        } else if (__instance.BreakMajorIsImminent) {
            increment = 0.9f * CosmereRoshar.bondChanceMultiplier;
        } else if (__instance.BreakMinorIsImminent) {
            increment = 0.5f * CosmereRoshar.bondChanceMultiplier;
        }

        if (increment > 0f) {
            pawnStats.requirementMap[CosmereRosharDefs.Cosmere_Roshar_Trait_RadiantWindrunner.defName][pawnStats.props
                .req01].value += increment;
        }

        pawnStats.doCheckWhenThisIsZero = (pawnStats.doCheckWhenThisIsZero + 1) % 100;
    }
}

////second requirements, must help people in need
[HarmonyPatch(typeof(TendUtility), nameof(TendUtility.DoTend))]
public static class Cosmere_Roshar_HelpSomeoneInNeed {
    private static void Postfix(Pawn doctor, Pawn patient) {
        if (patient.NonHumanlikeOrWildMan() || doctor == patient) {
            return;
        }

        if (doctor?.TryGetComp(out PawnStats pawnStats) != true) return;

        // WINDRUNNER
        RadiantRequirements? windrunnerRequirement12 =
            pawnStats.requirementMap[CosmereRosharDefs.Cosmere_Roshar_Trait_RadiantWindrunner.defName][
                pawnStats.props.req12];
        if (!pawnStats.patientList.Contains(patient)) {
            pawnStats.patientList.Add(patient);
        }

        windrunnerRequirement12.count += 1;

        //2_3
        if (patient.IsPrisoner) {
            RadiantRequirements? windrunnerRequirement23 =
                pawnStats.requirementMap[CosmereRosharDefs.Cosmere_Roshar_Trait_RadiantWindrunner.defName][pawnStats
                    .props
                    .req23];
            windrunnerRequirement23.count += 1;
        }

        // TRUTHWATCHER
        RadiantRequirements? truthwatcherRequirement =
            pawnStats.requirementMap[CosmereRosharDefs.Cosmere_Roshar_Trait_RadiantTruthwatcher.defName][pawnStats.props
                .req12];
        truthwatcherRequirement.count += 1;
        if (truthwatcherRequirement.count >= 25 && pawnStats.patientSaved) {
            truthwatcherRequirement.isSatisfied = true;
        }
    }
}

//add class to check for eligible that both can use, and call from both!

[HarmonyPatch(typeof(Pawn_HealthTracker))]
public static class PatchPawnHealthTrackerHealthTick {
    private static readonly AccessTools.FieldRef<Pawn_HealthTracker, Pawn> pawnRef =
        AccessTools.FieldRefAccess<Pawn>(typeof(Pawn_HealthTracker), "pawn");

    [HarmonyPatch(nameof(Pawn_HealthTracker.HealthTickInterval))]
    private static void Postfix(Pawn_HealthTracker __instance, int delta) {
        if (!GenTicks.IsTickIntervalDelta(GenTicks.TicksPerRealSecond, delta)) return;
        Pawn? pawn = pawnRef(__instance);
        if (pawn == null || pawn.NonHumanlikeOrWildMan()) {
            return;
        }

        PawnStats pawnStats = pawn.GetComp<PawnStats>();

        List<Pawn> patientsToRemove = [];
        foreach (Pawn patient in pawnStats.patientList) {
            if (patient == null) {
                patientsToRemove.Add(patient);
                continue;
            }

            if (patient.health.Dead && !patient.IsPrisoner) {
                pawnStats.patientDied = true;
                pawnStats.requirementMap[CosmereRosharDefs.Cosmere_Roshar_Trait_RadiantWindrunner.defName][pawnStats
                    .props
                    .req34].isSatisfied = true;
                patientsToRemove.Add(patient);
            } else if (NeedsNoTending(patient)) {
                pawnStats.patientSaved = true;
                if (patient.IsPrisoner) {
                    pawnStats.enemyPatientSaved = true;
                }

                patientsToRemove.Add(patient);
            }
        }

        foreach (Pawn? patient in patientsToRemove) {
            pawnStats.patientList.Remove(patient);
        }
    }

    private static bool NeedsNoTending(Pawn pawn) {
        return !pawn.health.HasHediffsNeedingTendByPlayer() && !HealthAIUtility.ShouldSeekMedicalRest(pawn);
    }
}