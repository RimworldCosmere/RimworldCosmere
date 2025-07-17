using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CosmereRoshar {


    public class Need_RadiantProgress : Need {
        private const float LEVEL_NEW_SQUIRE = 500f;
        private const float LEVEL_EXPERIENCED_SQUIRE = LEVEL_NEW_SQUIRE * 6f;
        private const float LEVEL_KNIGHT_RADIANT = LEVEL_EXPERIENCED_SQUIRE * 2f;
        private const float LEVEL_KNIGHT_RADIANT_MASTER = LEVEL_KNIGHT_RADIANT * 2f;
        private const float MAX_XP = LEVEL_KNIGHT_RADIANT_MASTER + 10f;
        private float currentXp = 0f;
        private int CurrentDegree = 0;

        public int IdealLevel { get { return CurrentDegree + 1; } }


        // Called after loading or on spawn
        public override void ExposeData() {
            base.ExposeData();
            Scribe_Values.Look(ref currentXp, "currentXp", 0f);
            Scribe_Values.Look(ref CurrentDegree, "CurrentDegree", 0);

            // Save/load a reference to a Pawn with Scribe_References:
            //Scribe_References.Look(ref pawnOwnerOfComp, "pawnOwnerOfComp");
        }


        public Need_RadiantProgress(Pawn pawn) : base(pawn) {
            this.threshPercents = new List<float> { 0.0416f, 0.25f, 0.5f }; // Visual bar markers
        }

        public override void NeedInterval() {
            // **No passive decay**, since XP should only increase when events happen
        }

        public void GainXP(float amount) {
            currentXp += amount*5f;
            CurLevel = Mathf.Max(0f, Mathf.Min(1f, (currentXp / MAX_XP)));

            //Log.Message($"CurrentXP: {currentXp}, CurLevel: {CurLevel}");
        }

        public void UpdateRadiantTrait(Pawn pawn) {
            Trait radiantTrait = StormlightUtilities.GetRadiantTrait(pawn);
            if (radiantTrait != null) {
                int currentDegree = radiantTrait.Degree;
                int newDegree = GetDegreeFromXP(currentXp);

                if (newDegree > currentDegree && isEligibleForRankup(pawn, radiantTrait)) {
                    // Remove old trait and add the upgraded one
                    pawn.story.traits.RemoveTrait(radiantTrait);
                    pawn.story.traits.GainTrait(new Trait(radiantTrait.def, newDegree));
                    CurrentDegree = newDegree;
                    Messages.Message($"{pawn.Name} has grown stronger as a Radiant!", pawn, MessageTypeDefOf.PositiveEvent);
                }
                else if (newDegree > currentDegree) {
                    currentXp = GetXpFromDegree(currentDegree);
                }
            }
        }

        private bool isEligibleForRankup(Pawn pawn, Trait trait) {
            bool eligible = false;
            PawnStats pawnStats = pawn.GetComp<PawnStats>();

            switch (IdealLevel) {
                case 1:
                    Whtwl_RadiantNeedLevelupChecker.UpdateIsSatisfiedReq1_2(pawnStats);
                    eligible = pawnStats.GetRequirements(trait.def, pawnStats.Props.Req_1_2).IsSatisfied;
                    break;
                case 2:
                    Whtwl_RadiantNeedLevelupChecker.UpdateIsSatisfiedReq2_3(pawnStats);
                    eligible = pawnStats.GetRequirements(trait.def, pawnStats.Props.Req_2_3).IsSatisfied;
                    break;
                case 3:
                    Whtwl_RadiantNeedLevelupChecker.UpdateIsSatisfiedReq3_4(pawnStats);
                    eligible = pawnStats.GetRequirements(trait.def, pawnStats.Props.Req_3_4).IsSatisfied;
                    break;
                //case 4:
                //    Whtwl_RadiantNeedLevelupChecker.UpdateIsSatisfiedReq4_5(pawnStats);
                //    eligible = pawnStats.GetRequirements(trait.def, pawnStats.Props.Req_4_5).IsSatisfied; 
                //    break;
                default:
                    eligible = true;
                    break;
            }

            return eligible;
        }

        private int GetDegreeFromXP(float xp) {
            if (xp >= LEVEL_KNIGHT_RADIANT_MASTER) return 4;     // Knight Radiant Master
            if (xp >= LEVEL_KNIGHT_RADIANT) return 3;            // Knight Radiant
            if (xp >= LEVEL_EXPERIENCED_SQUIRE) return 2;        // Experienced Squire
            if (xp >= LEVEL_NEW_SQUIRE) return 1;                // New Squire
            return 0;                                            // Bonded (Base Level)
        }

        private float GetXpFromDegree(int degree) {
            if (degree == 4) return LEVEL_KNIGHT_RADIANT_MASTER;     // Knight Radiant Master
            if (degree == 3) return LEVEL_KNIGHT_RADIANT_MASTER;     // Knight Radiant Master
            if (degree == 2) return LEVEL_KNIGHT_RADIANT;            // Knight Radiant
            if (degree == 1) return LEVEL_EXPERIENCED_SQUIRE;        // Experienced Squire
            if (degree == 0) return LEVEL_NEW_SQUIRE;                // New Squire
            return LEVEL_NEW_SQUIRE;                                            // Bonded (Base Level)
        }

        public override int GUIChangeArrow => 0; // No arrow (need doesn’t decay)
    }



    [HarmonyPatch(typeof(Pawn_NeedsTracker), "ShouldHaveNeed")]
    public static class Patch_RadiantProgress_Need {
        public static void Postfix(Pawn_NeedsTracker __instance, NeedDef nd, ref bool __result, Pawn ___pawn) {

            //if (nd == CosmereRosharDefs.whtwl_RadiantProgress) {
            //    __result = ___pawn.story?.traits?.allTraits.FirstOrDefault(t => CosmereRosharUtilities.RadiantTraits.Contains(t.def)) != null;
            //}

            if (nd == CosmereRosharDefs.whtwl_RadiantProgress && ___pawn.story?.traits?.HasTrait(CosmereRosharDefs.whtwl_Radiant_Windrunner) == true) {
                __result = true;
            }
            else if (nd == CosmereRosharDefs.whtwl_RadiantProgress && ___pawn.story?.traits?.HasTrait(CosmereRosharDefs.whtwl_Radiant_Truthwatcher) == true) {
                __result = true;
            }
            else if (nd == CosmereRosharDefs.whtwl_RadiantProgress && ___pawn.story?.traits?.HasTrait(CosmereRosharDefs.whtwl_Radiant_Edgedancer) == true) {
                __result = true;
            }   
            else if (nd == CosmereRosharDefs.whtwl_RadiantProgress && ___pawn.story?.traits?.HasTrait(CosmereRosharDefs.whtwl_Radiant_Skybreaker) == true) {
                __result = true;
            }
            else if (nd == CosmereRosharDefs.whtwl_RadiantProgress && ___pawn.story?.traits?.HasTrait(CosmereRosharDefs.whtwl_Radiant_Windrunner) == false) {
                __result = false;
            }
            else if (nd == CosmereRosharDefs.whtwl_RadiantProgress && ___pawn.story?.traits?.HasTrait(CosmereRosharDefs.whtwl_Radiant_Truthwatcher) == false) {
                __result = false;
            }
            else if (nd == CosmereRosharDefs.whtwl_RadiantProgress && ___pawn.story?.traits?.HasTrait(CosmereRosharDefs.whtwl_Radiant_Edgedancer) == false) {
                __result = false;
            }
            else if (nd == CosmereRosharDefs.whtwl_RadiantProgress && ___pawn.story?.traits?.HasTrait(CosmereRosharDefs.whtwl_Radiant_Skybreaker) == false) {
                __result = false;
            }
        }
    }


    public static class RadiantUtility {
        public static void GiveRadiantXP(Pawn pawn, float amount) {
            if (pawn == null) {
                return;
            }

            Need_RadiantProgress progress = pawn.needs?.TryGetNeed<Need_RadiantProgress>();
            if (progress != null) {
                progress.GainXP(amount);
                progress.UpdateRadiantTrait(pawn);
            }
        }
    }
}
