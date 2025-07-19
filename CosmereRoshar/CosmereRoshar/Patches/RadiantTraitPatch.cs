using CosmereRoshar.Combat.Abilities.Implementations;
using CosmereRoshar.Comp.Thing;
using CosmereRoshar.Need;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CosmereRoshar.Patches;

[HarmonyPatch(typeof(TraitSet), "GainTrait")]
public static class RadiantGainTraitPatch {
    private static void Postfix(Pawn? pawn, Trait trait) {
        if (!StormlightUtilities.IsRadiant(trait)) return;
        if (pawn?.needs == null || !pawn.RaceProps.Humanlike) return;
        Log.Message($"{pawn.Name} has become Radiant!");
        GivePawnStormlight(pawn);
        //givePawnGlow(___pawn);
        if (trait.Degree >= 2) {
            GivePawnShardbladeComp(pawn);
        }

        if (trait.Degree != 0) return;

        pawn.needs.AddOrRemoveNeedsAsAppropriate();

        if (!pawn.needs.TryGetNeed(out NeedRadiantProgress progress)) return;
        progress.GainXp(0);
    }

    // @todo Can we somehow avoid programatically giving the comp to the pawn?
    private static void GivePawnShardbladeComp(Pawn pawn) {
        ThingDef stuffDef = DefDatabase<ThingDef>.GetNamed("whtwl_ShardMaterial");
        ThingDef shardThing = DefDatabase<ThingDef>.GetNamed("whtwl_MeleeWeapon_Shardblade");
        ThingWithComps blade = (ThingWithComps)ThingMaker.MakeThing(shardThing, stuffDef);

        if (!blade.TryGetComp(out ShardBlade comp)) return;
        comp.Initialize(new ShardbladeProperties());
        comp.BondWithPawn(pawn, false);
        Log.Message($"{pawn.Name} gained shardbalde storage!");
    }

    private static void GivePawnStormlight(Pawn pawn) {
        Stormlight stormlight = pawn.GetComp<Stormlight>();
        if (stormlight != null && !stormlight.isActivatedOnPawn) {
            stormlight.isActivatedOnPawn = true;
            stormlight.CompInspectStringExtra();
            Log.Message($"{pawn.Name} gained Stormlight storage!");
        }
    }

    // @todo Can we somehow avoid programatically giving the comp to the pawn?
    private static void GivePawnGlow(Pawn pawn) {
        if (!pawn.TryGetComp(out CompGlower glowerComp)) return;
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

// @TODO Move to CosmereRoshar/Patches/PawnPathFollowerPatches.cs
[HarmonyPatch(typeof(Pawn_PathFollower), "CostToMoveIntoCell")]
[HarmonyPatch([typeof(Pawn), typeof(IntVec3)])]
public static class PatchPawnMovement {
    private static void Postfix(Pawn? pawn, IntVec3 c, ref float result) {
        if (pawn?.health?.hediffSet == null) return;
        if (pawn.health.hediffSet.HasHediff(CosmereRosharDefs.WhtwlSurgeAbrasion)) {
            result = c.x != pawn.Position.x && c.z != pawn.Position.z
                ? pawn.TicksPerMoveDiagonal
                : pawn.TicksPerMoveCardinal;
        }
    }
}