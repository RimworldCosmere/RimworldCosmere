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

        //RAW GEMS
        public static ThingDef whtwl_RawDiamond;
        public static ThingDef whtwl_RawGarnet;
        public static ThingDef whtwl_RawRuby;
        public static ThingDef whtwl_RawSapphire;
        public static ThingDef whtwl_RawEmerald;

        //CUT GEMS
        public static ThingDef whtwl_CutDiamond;
        public static ThingDef whtwl_CutGarnet;
        public static ThingDef whtwl_CutRuby;
        public static ThingDef whtwl_CutSapphire;
        public static ThingDef whtwl_CutEmerald;

        //SPHERED GEMS
        public static ThingDef whtwl_Sphere_Diamond;
        public static ThingDef whtwl_Sphere_Garnet;
        public static ThingDef whtwl_Sphere_Ruby;
        public static ThingDef whtwl_Sphere_Sapphire;
        public static ThingDef whtwl_Sphere_Emerald;

        // METALS
        public static ThingDef whtwl_AlloyPewter;
        public static ThingDef whtwl_AlloyBrass;
        public static ThingDef whtwl_AlloyBronze;
        public static ThingDef whtwl_RawTin;
        public static ThingDef whtwl_RawZinc;
        public static ThingDef whtwl_RawCopper;
        public static ThingDef whtwl_RawAntimony;

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
                CosmereRosharDefs.whtwl_RawDiamond,
                CosmereRosharDefs.whtwl_RawGarnet,
                CosmereRosharDefs.whtwl_RawRuby,
                CosmereRosharDefs.whtwl_RawSapphire,
                CosmereRosharDefs.whtwl_RawEmerald
            };
            }
        }
        public static List<ThingDef> CutGems {
            get {
                return new List<ThingDef>
                {
                CosmereRosharDefs.whtwl_CutDiamond,
                CosmereRosharDefs.whtwl_CutGarnet,
                CosmereRosharDefs.whtwl_CutRuby,
                CosmereRosharDefs.whtwl_CutSapphire,
                CosmereRosharDefs.whtwl_CutEmerald
            };
            }
        }
    }

}
