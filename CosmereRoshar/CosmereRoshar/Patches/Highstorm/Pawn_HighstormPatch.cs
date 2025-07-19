using System;
using CosmereRoshar.Comp.Thing;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereRoshar.Patches.Highstorm;

[HarmonyPatch(typeof(Pawn))]
[HarmonyPatch("TickInterval")]
public static class PawnHighstormPushPatch {
    private static readonly Random MRand = new Random();

    private static readonly string WindrunnerBondText =
        "pauses. A presence lingers at the edge of their awareness—watching, waiting. It has seen their struggles, their moments of strength, their failures. And yet, it remains.\r\n\r\nA faint whisper echoes in the back of their mind, words unbidden yet undeniable:\r\n\"Life before death. Strength before weakness. Journey before destination.\"";

    private static readonly string TruthwatcherBondText =
        "pauses. The world seems to blur at the edges, as if reality itself hesitates to be fully known. In the shifting mist, a quiet presence lingers—watching, not with eyes, but with understanding. It has seen their doubts, their curiosity, their silent strength.\r\n\r\nA whisper, clear and certain, forms within their thoughts:\r\n\"Life before death. Strength before weakness. Journey before destination.\"";

    private static readonly string EdgedancerBondText =
        "pauses. The scent of blooming life drifts on the air, and something warm brushes against their soul—a presence both nurturing and ancient. It remembers who they are, even when the world does not. It has seen their compassion, their small kindnesses, their quiet resolve.\r\n\r\nFrom somewhere deep within, words rise like new growth:\r\n\"Life before death. Strength before weakness. Journey before destination.\"";

    private static readonly string SkybreakerBondText =
        "pauses. The air grows heavy, still with judgment. A presence watches from afar, cold and unyielding—but not without purpose. It has measured their choices, their discipline, their will to obey or defy.\r\n\r\nThen, without emotion or mercy, the words arrive—etched like a sentence passed:\r\n\"Life before death. Strength before weakness. Journey before destination.\"";

    private static readonly float InitialBondMaxNumber = 100000f;

    private static void Postfix(Pawn __instance, int delta) {
        if (__instance.IsHashIntervalTick(GenTicks.TicksPerRealSecond / 3, delta)) return;

        if (!IsPawnValidForStorm(__instance)) {
            return;
        }

        if (IsHighstormActive(__instance.Map)) {
            DamageAndMovePawn(__instance);
            if (StormlightUtilities.IsRadiant(__instance)) {
                return;
            }

            if (Find.TickManager.TicksGame % 100 == 0 && StormlightUtilities.IsPawnEligibleForDoctoring(__instance)) {
                TryToBondPawn(__instance, CosmereRosharDefs.Cosmere_Roshar_Trait_RadiantWindrunner);
            }

            return;
        }

        StormShelterManager.FirstTickOfHighstorm = true;
        if (StormlightUtilities.IsRadiant(__instance)) {
            return;
        }

        if (__instance.IsHashIntervalTick(GenTicks.TicksPerRealSecond)) return;
        switch (__instance.Map.weatherManager.curWeather.defName) {
            case "Fog" or "FoggyRain" when
                StormlightUtilities.IsPawnEligibleForDoctoring(__instance):
                TryToBondPawn(__instance, CosmereRosharDefs.Cosmere_Roshar_Trait_RadiantTruthwatcher);
                break;
            case "Rain" or "Clear" when
                StormlightUtilities.IsNearGrowingPlants(__instance) &&
                StormlightUtilities.IsPawnEligibleForDoctoring(__instance):
                TryToBondPawn(__instance, CosmereRosharDefs.Cosmere_Roshar_Trait_RadiantEdgedancer);
                break;
            case "DryThunderstorm" or "RainyThunderstorm":
                TryToBondPawn(__instance, CosmereRosharDefs.Cosmere_Roshar_Trait_RadiantSkybreaker); break;
        }
    }

    private static int GetUpperNumber(float crisisValue) {
        return (int)(InitialBondMaxNumber - crisisValue * crisisValue * 0.01f);
    }

    private static void TryToBondPawn(Pawn pawn, TraitDef traitDef) {
        if (!pawn.RaceProps.Humanlike) return;
        if (StormlightUtilities.GetRadiantTrait(pawn) != null) return;

        PawnStats pawnStats = pawn.GetComp<PawnStats>();
        if (pawnStats == null) return;

        int upperNumber = GetUpperNumber(pawnStats.GetRequirementsEntry().value);

        if (upperNumber <= 1) upperNumber = 2;
        int number = MRand.Next(1, upperNumber);
        if (number != 1) return;

        if (traitDef == CosmereRosharDefs.Cosmere_Roshar_Trait_RadiantWindrunner) {
            StormlightUtilities.SpeakOaths(
                pawn,
                pawnStats,
                traitDef,
                $"{pawn.NameShortColored} " + WindrunnerBondText,
                "A Whisper in the Mind.."
            );
        } else if (traitDef == CosmereRosharDefs.Cosmere_Roshar_Trait_RadiantTruthwatcher) {
            StormlightUtilities.SpeakOaths(
                pawn,
                pawnStats,
                traitDef,
                $"{pawn.NameShortColored} " + TruthwatcherBondText,
                "A Whisper in the Mind.."
            );
        } else if (traitDef == CosmereRosharDefs.Cosmere_Roshar_Trait_RadiantEdgedancer) {
            StormlightUtilities.SpeakOaths(
                pawn,
                pawnStats,
                traitDef,
                $"{pawn.NameShortColored} " + EdgedancerBondText,
                "A Whisper in the Mind.."
            );
        } else if (traitDef == CosmereRosharDefs.Cosmere_Roshar_Trait_RadiantSkybreaker) {
            StormlightUtilities.SpeakOaths(
                pawn,
                pawnStats,
                traitDef,
                $"{pawn.NameShortColored} " + SkybreakerBondText,
                "A Whisper in the Mind.."
            );
        }
        //else { Log.Message($"{pawn.NameShortColored} tried to bond, failed with number: {number} of {upperNumber}, crisis value: {pawnStats.GetRequirementsEntry().Value}"); }
    }

    private static bool IsHighstormActive(Map map) {
        // Checks if our custom GameCondition is present
        GameCondition.Highstorm? condition = map.gameConditionManager.GetActiveCondition<GameCondition.Highstorm>();
        return condition != null;
    }

    private static bool IsPawnValidForStorm(Pawn pawn) {
        if (pawn.Dead || pawn.Destroyed || !pawn.Spawned) {
            return false;
        }

        if (pawn.Map == null) {
            return false;
        }

        if (!pawn.Position.IsValid) {
            return false;
        }

        if (!pawn.Position.InBounds(pawn.Map)) {
            return false;
        }

        return !pawn.Position.Roofed(pawn.Map);
    }

    private static bool CheckIfSheltered(Pawn pawn) {
        return !pawn.ShouldBeMovedByStorm();
    }


    private static void DamageAndMovePawn(Pawn instance) {
        if (CheckIfSheltered(instance)) {
            return;
        }

        bool isRadiant = instance.story?.traits.allTraits.Any(StormlightUtilities.IsRadiant) ?? false;
        if (CosmereRoshar.enableHighstormDamage || instance.RaceProps.Humanlike && instance.Faction.IsPlayer) {
            instance.TakeDamage(new DamageInfo(DamageDefOf.Blunt, 1));
        }

        // Storm always blows from east -> pushes pawns west
        IntVec3 newPos = instance.Position + IntVec3.West;
        if (!isRadiant &&
            instance.Map != null &&
            newPos.Walkable(instance.Map) &&
            newPos.IsValid &&
            newPos.InBounds(instance.Map)) {
            instance.Position = newPos;
        } else {
            Stormlight? stormlightComp = instance.TryGetComp<Stormlight>();
            if (stormlightComp != null) {
                stormlightComp.InfuseStormlight(25f); // 25 units per check
            }
        }
    }
}