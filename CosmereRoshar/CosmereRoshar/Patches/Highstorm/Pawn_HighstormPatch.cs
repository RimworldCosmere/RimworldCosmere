using HarmonyLib;
using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using System;
using Verse.Noise;

namespace CosmereRoshar {
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("Tick")]
    public static class Pawn_HighstormPushPatch {
        private static Random m_Rand = new Random();
        private static string WindrunnerBondText = $"pauses. A presence lingers at the edge of their awareness—watching, waiting. It has seen their struggles, their moments of strength, their failures. And yet, it remains.\r\n\r\nA faint whisper echoes in the back of their mind, words unbidden yet undeniable:\r\n\"Life before death. Strength before weakness. Journey before destination.\"";
        private static string TruthwatcherBondText = $"pauses. The world seems to blur at the edges, as if reality itself hesitates to be fully known. In the shifting mist, a quiet presence lingers—watching, not with eyes, but with understanding. It has seen their doubts, their curiosity, their silent strength.\r\n\r\nA whisper, clear and certain, forms within their thoughts:\r\n\"Life before death. Strength before weakness. Journey before destination.\"";
        private static string EdgedancerBondText = $"pauses. The scent of blooming life drifts on the air, and something warm brushes against their soul—a presence both nurturing and ancient. It remembers who they are, even when the world does not. It has seen their compassion, their small kindnesses, their quiet resolve.\r\n\r\nFrom somewhere deep within, words rise like new growth:\r\n\"Life before death. Strength before weakness. Journey before destination.\"";
        private static string SkybreakerBondText = $"pauses. The air grows heavy, still with judgment. A presence watches from afar, cold and unyielding—but not without purpose. It has measured their choices, their discipline, their will to obey or defy.\r\n\r\nThen, without emotion or mercy, the words arrive—etched like a sentence passed:\r\n\"Life before death. Strength before weakness. Journey before destination.\"";
        private static float InitialBondMaxNumber = 100000f;
        static void Postfix(Pawn __instance) {
            if (Find.TickManager.TicksGame % 20 != 0) return;

            if (!IsPawnValidForStorm(__instance))
                return;
            if (IsHighstormActive(__instance.Map)) {

                damageAndMovePawn(__instance);
                if (StormlightUtilities.IsRadiant(__instance)) {
                    return;
                }
                if (Find.TickManager.TicksGame % 100 == 0 && StormlightUtilities.IsPawnEligibleForDoctoring(__instance)) {
                    tryToBondPawn(__instance, CosmereRosharDefs.whtwl_Radiant_Windrunner);
                }

                return;
            }
            else {
                StormShelterManager.FirstTickOfHighstorm = true;
            }
            if (StormlightUtilities.IsRadiant(__instance)) {
                return;
            }
            if (Find.TickManager.TicksGame % 100 == 0) {
                if ((__instance.Map.weatherManager.curWeather.defName == "Fog" || __instance.Map.weatherManager.curWeather.defName == "FoggyRain") && StormlightUtilities.IsPawnEligibleForDoctoring(__instance)) {
                    tryToBondPawn(__instance, CosmereRosharDefs.whtwl_Radiant_Truthwatcher);
                }
                else if ((__instance.Map.weatherManager.curWeather.defName == "Rain" || __instance.Map.weatherManager.curWeather.defName == "Clear") && StormlightUtilities.IsNearGrowingPlants(__instance) && StormlightUtilities.IsPawnEligibleForDoctoring(__instance)) {
                    tryToBondPawn(__instance, CosmereRosharDefs.whtwl_Radiant_Edgedancer);
                }
                else if (__instance.Map.weatherManager.curWeather.defName == "DryThunderstorm" || __instance.Map.weatherManager.curWeather.defName == "RainyThunderstorm") {
                    tryToBondPawn(__instance, CosmereRosharDefs.whtwl_Radiant_Skybreaker);
                }
            }
        }
        private static int getUpperNumber(float crisisValue) {
            return (int)(InitialBondMaxNumber - ((crisisValue * crisisValue) * 0.01f));
        }
        private static void tryToBondPawn(Pawn pawn, TraitDef traitDef) {
            if (pawn != null && pawn.RaceProps.Humanlike) {
                if (StormlightUtilities.GetRadiantTrait(pawn) != null) return;

                PawnStats pawnStats = pawn.GetComp<PawnStats>();
                if (pawnStats == null) return;

                int upperNumber = getUpperNumber(pawnStats.GetRequirementsEntry().Value);

                if (upperNumber <= 1) upperNumber = 2;
                int number = m_Rand.Next(1, upperNumber);
                if (number == 1) {
                    if (traitDef == CosmereRosharDefs.whtwl_Radiant_Windrunner) {
                        StormlightUtilities.SpeakOaths(pawn, pawnStats, traitDef, $"{pawn.NameShortColored} " + WindrunnerBondText, "A Whisper in the Mind..");
                    }
                    else if (traitDef == CosmereRosharDefs.whtwl_Radiant_Truthwatcher) {
                        StormlightUtilities.SpeakOaths(pawn, pawnStats, traitDef, $"{pawn.NameShortColored} " + TruthwatcherBondText, "A Whisper in the Mind..");
                    }
                    else if (traitDef == CosmereRosharDefs.whtwl_Radiant_Edgedancer) {
                        StormlightUtilities.SpeakOaths(pawn, pawnStats, traitDef, $"{pawn.NameShortColored} " + EdgedancerBondText, "A Whisper in the Mind..");
                    }
                    else if (traitDef == CosmereRosharDefs.whtwl_Radiant_Skybreaker) {
                        StormlightUtilities.SpeakOaths(pawn, pawnStats, traitDef, $"{pawn.NameShortColored} " + SkybreakerBondText, "A Whisper in the Mind..");
                    }

                }
                //else { Log.Message($"{pawn.NameShortColored} tried to bond, failed with number: {number} of {upperNumber}, crisis value: {pawnStats.GetRequirementsEntry().Value}"); }
            }
        }

        private static bool IsHighstormActive(Map map) {
            // Checks if our custom GameCondition is present
            var condition = map.gameConditionManager.GetActiveCondition<GameCondition_Highstorm>();
            return (condition != null);
        }
        private static bool IsPawnValidForStorm(Pawn pawn) {
            if (pawn == null)
                return false;
            if (pawn.Dead || pawn.Destroyed || !pawn.Spawned)
                return false;
            if (pawn.Map == null)
                return false;
            if (!pawn.Position.IsValid)
                return false;
            if (!pawn.Position.InBounds(pawn.Map))
                return false;
            if (pawn.Position.Roofed(pawn.Map))
                return false;

            return true;
        }

        private static bool checkIfSheltered(Pawn pawn) {
            return !StormlightUtilities.ShouldBeMovedByStorm(pawn);
        }



        private static void damageAndMovePawn(Pawn __instance) {
            if (checkIfSheltered(__instance))
                return;

            bool isRadiant = false;
            if (__instance.story != null && __instance.RaceProps.Humanlike) {
                foreach (Trait t in __instance.story.traits.allTraits) {
                    if (StormlightUtilities.IsRadiant(t)) {
                        isRadiant = true;
                        continue;
                    }
                }
            }

            if (CosmereRoshar.EnableHighstormDamage || (__instance.RaceProps.Humanlike && __instance.Faction.IsPlayer)) {
                __instance.TakeDamage(new DamageInfo(DamageDefOf.Blunt, 1));
            }

            // Storm always blows from east -> pushes pawns west
            IntVec3 newPos = __instance.Position + IntVec3.West;
            if (isRadiant == false && __instance.Map != null && newPos.Walkable(__instance.Map) && newPos.IsValid && newPos.InBounds(__instance.Map)) {
                __instance.Position = newPos;
            }
            else {
                var stormlightComp = __instance.TryGetComp<CompStormlight>();
                if (stormlightComp != null) {
                    stormlightComp.infuseStormlight(25f); // 25 units per check
                }
            }
        }
    }
}

