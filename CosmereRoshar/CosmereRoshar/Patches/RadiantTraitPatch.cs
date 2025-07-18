using CosmereRoshar.Comp;
using CosmereRoshar.Comp.Thing;
using CosmereRoshar.Comps;
using CosmereRoshar.Comps.WeaponsAndArmor;
using CosmereRoshar.Need;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereRoshar.Patches;

[HarmonyPatch(typeof(TraitSet), "GainTrait")]
public static class RadiantGainTraitPatch {
    private static void Postfix(Pawn pawn, Trait trait) {
        if (StormlightUtilities.IsRadiant(trait)) {
            if (pawn != null && pawn.RaceProps.Humanlike) {
                Log.Message($"{pawn.Name} has become Radiant!");
                GivePawnStormlight(pawn);
                //givePawnGlow(___pawn);
                if (trait.Degree >= 2) {
                    GivePawnShardbladeComp(pawn);
                }

                if (trait.Degree == 0) {
                    pawn.needs.AddOrRemoveNeedsAsAppropriate();
                    NeedRadiantProgress progress = pawn.needs?.TryGetNeed<NeedRadiantProgress>();
                    if (progress != null) {
                        progress.GainXp(0);
                    }
                }
            }
        }
    }

    private static void GivePawnShardbladeComp(Pawn pawn) {
        ThingDef stuffDef = DefDatabase<ThingDef>.GetNamed("whtwl_ShardMaterial");
        ThingDef shardThing = DefDatabase<ThingDef>.GetNamed("whtwl_MeleeWeapon_Shardblade");
        ThingWithComps blade = (ThingWithComps)ThingMaker.MakeThing(shardThing, stuffDef);

        CompShardblade comp = blade.GetComp<CompShardblade>();
        if (comp != null) {
            comp.Initialize(new CompPropertiesShardblade());
            comp.BondWithPawn(pawn, false);
            Log.Message($"{pawn.Name} gained shardbalde storage!");
        }
    }

    private static void GivePawnStormlight(Pawn pawn) {
        Stormlight stormlight = pawn.GetComp<Stormlight>();
        if (stormlight != null && stormlight.isActivatedOnPawn == false) {
            stormlight.isActivatedOnPawn = true;
            stormlight.CompInspectStringExtra();
            Log.Message($"{pawn.Name} gained Stormlight storage!");
        }
    }

    private static void GivePawnGlow(Pawn pawn) {
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
public static class PatchPawnMovement {
    private static void Postfix(Pawn pawn, IntVec3 c, ref float result) {
        if (pawn == null || pawn.health?.hediffSet == null) return;
        if (pawn.health.hediffSet.HasHediff(CosmereRosharDefs.WhtwlSurgeAbrasion)) {
            result = c.x != pawn.Position.x && c.z != pawn.Position.z
                ? pawn.TicksPerMoveDiagonal
                : pawn.TicksPerMoveCardinal;
        }
    }
}