using System.Collections.Generic;
using CosmereRoshar.Patches;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereRoshar.Need;

public class RadiantProgress : RimWorld.Need {
    private const float LevelNewSquire = 500f;
    private const float LevelExperiencedSquire = LevelNewSquire * 6f;
    private const float LevelKnightRadiant = LevelExperiencedSquire * 2f;
    private const float LevelKnightRadiantMaster = LevelKnightRadiant * 2f;
    private const float MaxXp = LevelKnightRadiantMaster + 10f;
    private int currentDegree;
    private float currentXp;


    public RadiantProgress(Pawn pawn) : base(pawn) {
        threshPercents = new List<float> { 0.0416f, 0.25f, 0.5f }; // Visual bar markers
    }

    public int idealLevel => currentDegree + 1;

    public override int GUIChangeArrow => 0; // No arrow (need doesn’t decay)


    // Called after loading or on spawn
    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref currentXp, "currentXp");
        Scribe_Values.Look(ref currentDegree, "CurrentDegree");

        // Save/load a reference to a Pawn with Scribe_References:
        //Scribe_References.Look(ref pawnOwnerOfComp, "pawnOwnerOfComp");
    }

    public override void NeedInterval() {
        // **No passive decay**, since XP should only increase when events happen
    }

    public void GainXp(float amount) {
        currentXp += amount * 5f;
        CurLevel = Mathf.Max(0f, Mathf.Min(1f, currentXp / MaxXp));

        //Log.Message($"CurrentXP: {currentXp}, CurLevel: {CurLevel}");
    }

    public void UpdateRadiantTrait(Pawn pawn) {
        Trait radiantTrait = StormlightUtilities.GetRadiantTrait(pawn);
        if (radiantTrait != null) {
            int currentDegree = radiantTrait.Degree;
            int newDegree = GetDegreeFromXp(currentXp);

            if (newDegree > currentDegree && IsEligibleForRankup(pawn, radiantTrait)) {
                // Remove old trait and add the upgraded one
                pawn.story.traits.RemoveTrait(radiantTrait);
                pawn.story.traits.GainTrait(new Trait(radiantTrait.def, newDegree));
                this.currentDegree = newDegree;
                Messages.Message($"{pawn.Name} has grown stronger as a Radiant!", pawn, MessageTypeDefOf.PositiveEvent);
            } else if (newDegree > currentDegree) {
                currentXp = GetXpFromDegree(currentDegree);
            }
        }
    }

    private bool IsEligibleForRankup(Pawn pawn, Trait trait) {
        bool eligible = false;
        PawnStats pawnStats = pawn.GetComp<PawnStats>();

        switch (idealLevel) {
            case 1:
                Cosmere_Roshar_Radiant_NeedLevelupChecker.UpdateIsSatisfiedReq1_2(pawnStats);
                eligible = pawnStats.GetRequirements(trait.def, pawnStats.props.req12).isSatisfied;
                break;
            case 2:
                Cosmere_Roshar_Radiant_NeedLevelupChecker.UpdateIsSatisfiedReq2_3(pawnStats);
                eligible = pawnStats.GetRequirements(trait.def, pawnStats.props.req23).isSatisfied;
                break;
            case 3:
                Cosmere_Roshar_Radiant_NeedLevelupChecker.UpdateIsSatisfiedReq3_4(pawnStats);
                eligible = pawnStats.GetRequirements(trait.def, pawnStats.props.req34).isSatisfied;
                break;
            //case 4:
            //    Cosmere_Roshar_Radiant_NeedLevelupChecker.UpdateIsSatisfiedReq4_5(pawnStats);
            //    eligible = pawnStats.GetRequirements(trait.def, pawnStats.Props.Req_4_5).IsSatisfied; 
            //    break;
            default:
                eligible = true;
                break;
        }

        return eligible;
    }

    private int GetDegreeFromXp(float xp) {
        if (xp >= LevelKnightRadiantMaster) return 4; // Knight Radiant Master
        if (xp >= LevelKnightRadiant) return 3; // Knight Radiant
        if (xp >= LevelExperiencedSquire) return 2; // Experienced Squire
        if (xp >= LevelNewSquire) return 1; // New Squire
        return 0; // Bonded (Base Level)
    }

    private float GetXpFromDegree(int degree) {
        if (degree == 4) return LevelKnightRadiantMaster; // Knight Radiant Master
        if (degree == 3) return LevelKnightRadiantMaster; // Knight Radiant Master
        if (degree == 2) return LevelKnightRadiant; // Knight Radiant
        if (degree == 1) return LevelExperiencedSquire; // Experienced Squire
        if (degree == 0) return LevelNewSquire; // New Squire
        return LevelNewSquire; // Bonded (Base Level)
    }
}

public static class RadiantUtility {
    public static void GiveRadiantXp(Pawn pawn, float amount) {
        if (pawn == null) {
            return;
        }

        RadiantProgress progress = pawn.needs?.TryGetNeed<RadiantProgress>();
        if (progress != null) {
            progress.GainXp(amount);
            progress.UpdateRadiantTrait(pawn);
        }
    }
}