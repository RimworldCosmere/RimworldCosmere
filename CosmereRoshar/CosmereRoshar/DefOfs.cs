using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CosmereRoshar {
    [DefOf]
    public static class CosmereRosharDefs {
        public static NeedDef whtwl_RadiantProgress;

        public static TraitDef whtwl_Radiant_Windrunner;
        public static TraitDef whtwl_Radiant_Truthwatcher;
        public static TraitDef whtwl_Radiant_Edgedancer;
        public static TraitDef whtwl_Radiant_Skybreaker;

        public static AbilityDef whtwl_SummonShardblade;
        public static AbilityDef whtwl_UnbondBlade;
        public static AbilityDef whtwl_BreathStormlight;
        public static AbilityDef whtwl_SurgeOfHealing;
        public static AbilityDef whtwl_SurgeOfGrowth;
        public static AbilityDef whtwl_SurgeOfAbrasion;
        public static AbilityDef whtwl_SurgeOfDivision;
        public static AbilityDef whtwl_LashingUpward;
        public static AbilityDef whtwl_WindRunnerFlight;

        //JobDef
        public static JobDef whtwl_RefuelSphereLamp;
        public static JobDef whtwl_RefuelFabrial;
        public static JobDef whtwl_RemoveFromFabrial;
        public static JobDef whtwl_CastAbilityOnTarget;

        //ThingDef
        public static ThingDef whtwl_Apparel_SpherePouch;
        public static ThingDef whtwl_SphereLamp_Wall;
        public static ThingDef whtwl_FabrialCage_Pewter;
        public static ThingDef whtwl_BasicFabrial_Augmenter; 

        // FABRIALS
        public static ThingDef whtwl_Apparel_Fabrial_Painrial_Diminisher;

        // WEAPONS
        public static ThingDef whtwl_MeleeWeapon_Shardblade;



        // HEDIFF
        public static HediffDef whtwl_painrial_agument;
        public static HediffDef whtwl_painrial_diminisher;
        public static HediffDef whtwl_apparel_painrial_diminisher_hediff;

        public static HediffDef whtwl_logirial_agument;
        public static HediffDef whtwl_logirial_diminisher;

        public static HediffDef whtwl_surge_abrasion;


        static CosmereRosharDefs() {
            DefOfHelper.EnsureInitializedInCtor(typeof(CosmereRosharDefs));
        }
    }

    public static class CosmereRosharUtilities {
        public static List<TraitDef> RadiantTraits {
            get {
                return new List<TraitDef>
                {
                CosmereRosharDefs.whtwl_Radiant_Windrunner,
                CosmereRosharDefs.whtwl_Radiant_Truthwatcher,
                CosmereRosharDefs.whtwl_Radiant_Edgedancer,
                CosmereRosharDefs.whtwl_Radiant_Skybreaker

            };
            }
        }
        public static List<ThingDef> RawGems {
            get {
                return new List<ThingDef>
                {
                CosmereResources.ThingDefOf.RawDiamond,
                CosmereResources.ThingDefOf.RawGarnet,
                CosmereResources.ThingDefOf.RawRuby,
                CosmereResources.ThingDefOf.RawSapphire,
                CosmereResources.ThingDefOf.RawEmerald
            };
            }
        }
        public static List<ThingDef> CutGems {
            get {
                return new List<ThingDef>
                {
                CosmereResources.ThingDefOf.CutDiamond,
                CosmereResources.ThingDefOf.CutGarnet,
                CosmereResources.ThingDefOf.CutRuby,
                CosmereResources.ThingDefOf.CutSapphire,
                CosmereResources.ThingDefOf.CutEmerald
            };
            }
        }
    }

}
