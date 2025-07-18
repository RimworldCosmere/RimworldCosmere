using CosmereRoshar.Comp.Thing;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereRoshar;

[HarmonyPatch(typeof(TraitSet), "GainTrait")]
public static class RadiantGainTraitPatch {
    private static void Postfix(Pawn ___pawn, Trait trait) {
        if (StormlightUtilities.IsRadiant(trait)) {
            if (___pawn != null && ___pawn.RaceProps.Humanlike) {
                Log.Message($"{___pawn.Name} has become Radiant!");
                givePawnStormlight(___pawn);
                //givePawnGlow(___pawn);
                if (trait.Degree >= 2) {
                    givePawnShardbladeComp(___pawn);
                }

                if (trait.Degree == 0) {
                    ___pawn.needs.AddOrRemoveNeedsAsAppropriate();
                    Need_RadiantProgress progress = ___pawn.needs?.TryGetNeed<Need_RadiantProgress>();
                    if (progress != null) {
                        progress.GainXP(0);
                    }
                }
            }
        }
    }

    private static void givePawnShardbladeComp(Pawn pawn) {
        ThingDef stuffDef = DefDatabase<ThingDef>.GetNamed("whtwl_ShardMaterial");
        ThingDef shardThing = DefDatabase<ThingDef>.GetNamed("whtwl_MeleeWeapon_Shardblade");
        ThingWithComps blade = (ThingWithComps)ThingMaker.MakeThing(shardThing, stuffDef);

        CompShardblade comp = blade.GetComp<CompShardblade>();
        if (comp != null) {
            comp.Initialize(new CompProperties_Shardblade());
            comp.bondWithPawn(pawn, false);
            Log.Message($"{pawn.Name} gained shardbalde storage!");
        }
    }

    private static void givePawnStormlight(Pawn pawn) {
        CompStormlight stormlightComp = pawn.GetComp<CompStormlight>();
        if (stormlightComp != null && stormlightComp.isActivatedOnPawn == false) {
            stormlightComp.isActivatedOnPawn = true;
            stormlightComp.CompInspectStringExtra();
            Log.Message($"{pawn.Name} gained Stormlight storage!");
        }
    }

    private static void givePawnGlow(Pawn pawn) {
        if (pawn.GetComp<CompGlower>() == null) {
            CompGlower glowerComp = new CompGlower();
            pawn.AllComps.Add(glowerComp);
            glowerComp.parent = pawn;

            CompProperties_Glower glowProps = new CompProperties_Glower {
                glowRadius = 0, // Start with no glow
                overlightRadius = 2.0f, // Overlight radius (if needed)
                glowColor = new ColorInt(66, 245, 245, 0),
            };

            glowerComp.Initialize(glowProps);
            Log.Message($"{pawn.Name} now glows when infused with Stormlight!");
        }
    }
}

[HarmonyPatch(typeof(Pawn_PathFollower), "CostToMoveIntoCell")]
[HarmonyPatch(new[] { typeof(Pawn), typeof(IntVec3) })]
public static class Patch_Pawn_Movement {
    private static void Postfix(Pawn pawn, IntVec3 c, ref float __result) {
        if (pawn == null || pawn.health?.hediffSet == null) return;
        if (pawn.health.hediffSet.HasHediff(CosmereRosharDefs.whtwl_surge_abrasion)) {
            __result = c.x != pawn.Position.x && c.z != pawn.Position.z
                ? pawn.TicksPerMoveDiagonal
                : pawn.TicksPerMoveCardinal;
        }
    }
}