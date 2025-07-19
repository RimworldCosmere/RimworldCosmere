using System;
using CosmereRoshar.Comp.Thing;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereRoshar.Patches.Highstorm;

[HarmonyPatch(typeof(Pawn))]
[HarmonyPatch("Tick")]
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

    private static void Postfix(Pawn instance) {
        if (Find.TickManager.TicksGame % 20 != 0) return;

        if (!IsPawnValidForStorm(instance)) {
            return;
        }

        if (IsHighstormActive(instance.Map)) {
            DamageAndMovePawn(instance);
            if (StormlightUtilities.IsRadiant(instance)) {
                return;
            }

            if (Find.TickManager.TicksGame % 100 == 0 && StormlightUtilities.IsPawnEligibleForDoctoring(instance)) {
                TryToBondPawn(instance, CosmereRosharDefs.Cosmere_Roshar_RadiantWindrunner);
            }

            return;
        }

        StormShelterManager.FirstTickOfHighstorm = true;
        if (StormlightUtilities.IsRadiant(instance)) {
            return;
        }

        if (Find.TickManager.TicksGame % 100 == 0) {
            if ((instance.Map.weatherManager.curWeather.defName == "Fog" ||
                 instance.Map.weatherManager.curWeather.defName == "FoggyRain") &&
                StormlightUtilities.IsPawnEligibleForDoctoring(instance)) {
                TryToBondPawn(instance, CosmereRosharDefs.Cosmere_Roshar_RadiantTruthwatcher);
            } else if ((instance.Map.weatherManager.curWeather.defName == "Rain" ||
                        instance.Map.weatherManager.curWeather.defName == "Clear") &&
                       StormlightUtilities.IsNearGrowingPlants(instance) &&
                       StormlightUtilities.IsPawnEligibleForDoctoring(instance)) {
                TryToBondPawn(instance, CosmereRosharDefs.Cosmere_Roshar_RadiantEdgedancer);
            } else if (instance.Map.weatherManager.curWeather.defName == "DryThunderstorm" ||
                       instance.Map.weatherManager.curWeather.defName == "RainyThunderstorm") {
                TryToBondPawn(instance, CosmereRosharDefs.Cosmere_Roshar_RadiantSkybreaker);
            }
        }
    }

    private static int GetUpperNumber(float crisisValue) {
        return (int)(InitialBondMaxNumber - crisisValue * crisisValue * 0.01f);
    }

    private static void TryToBondPawn(Pawn pawn, TraitDef traitDef) {
        if (pawn != null && pawn.RaceProps.Humanlike) {
            if (StormlightUtilities.GetRadiantTrait(pawn) != null) return;

            PawnStats pawnStats = pawn.GetComp<PawnStats>();
            if (pawnStats == null) return;

            int upperNumber = GetUpperNumber(pawnStats.GetRequirementsEntry().value);

            if (upperNumber <= 1) upperNumber = 2;
            int number = MRand.Next(1, upperNumber);
            if (number == 1) {
                if (traitDef == CosmereRosharDefs.Cosmere_Roshar_RadiantWindrunner) {
                    StormlightUtilities.SpeakOaths(
                        pawn,
                        pawnStats,
                        traitDef,
                        $"{pawn.NameShortColored} " + WindrunnerBondText,
                        "A Whisper in the Mind.."
                    );
                } else if (traitDef == CosmereRosharDefs.Cosmere_Roshar_RadiantTruthwatcher) {
                    StormlightUtilities.SpeakOaths(
                        pawn,
                        pawnStats,
                        traitDef,
                        $"{pawn.NameShortColored} " + TruthwatcherBondText,
                        "A Whisper in the Mind.."
                    );
                } else if (traitDef == CosmereRosharDefs.Cosmere_Roshar_RadiantEdgedancer) {
                    StormlightUtilities.SpeakOaths(
                        pawn,
                        pawnStats,
                        traitDef,
                        $"{pawn.NameShortColored} " + EdgedancerBondText,
                        "A Whisper in the Mind.."
                    );
                } else if (traitDef == CosmereRosharDefs.Cosmere_Roshar_RadiantSkybreaker) {
                    StormlightUtilities.SpeakOaths(
                        pawn,
                        pawnStats,
                        traitDef,
                        $"{pawn.NameShortColored} " + SkybreakerBondText,
                        "A Whisper in the Mind.."
                    );
                }
            }
            //else { Log.Message($"{pawn.NameShortColored} tried to bond, failed with number: {number} of {upperNumber}, crisis value: {pawnStats.GetRequirementsEntry().Value}"); }
        }
    }

    private static bool IsHighstormActive(Map map) {
        // Checks if our custom GameCondition is present
        GameCondition.Highstorm? condition = map.gameConditionManager.GetActiveCondition<GameCondition.Highstorm>();
        return condition != null;
    }

    private static bool IsPawnValidForStorm(Pawn pawn) {
        if (pawn == null) {
            return false;
        }

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

        if (pawn.Position.Roofed(pawn.Map)) {
            return false;
        }

        return true;
    }

    private static bool CheckIfSheltered(Pawn pawn) {
        return !pawn.ShouldBeMovedByStorm();
    }


    private static void DamageAndMovePawn(Pawn instance) {
        if (CheckIfSheltered(instance)) {
            return;
        }

        bool isRadiant = false;
        if (instance.story != null && instance.RaceProps.Humanlike) {
            foreach (Trait t in instance.story.traits.allTraits) {
                if (StormlightUtilities.IsRadiant(t)) {
                    isRadiant = true;
                }
            }
        }

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