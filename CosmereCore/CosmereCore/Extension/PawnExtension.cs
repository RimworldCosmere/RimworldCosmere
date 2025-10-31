using System;
using System.Reflection;
using Cosmere.Core.Util;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Cosmere.Extension;

[StaticConstructorOnStartup]
public static class PawnExtension {
    private static readonly Assembly? Scadrial = LoadedModManager.RunningMods
        .FirstOrDefault(m => m.PackageId.Equals("cosmere.scadrial", StringComparison.CurrentCultureIgnoreCase))
        ?.assemblies.loadedAssemblies.FirstOrDefault();

    public static bool IsShieldedAgainstInvestiture(this Pawn pawn) {
        return InvestitureDetector.IsShielded(pawn);
    }

    public static void MaintainProximityTo(
        this Pawn pawn,
        LocalTargetInfo target,
        float maxDistance,
        PathEndMode endMode
    ) {
        float distance = pawn.Position.DistanceTo(target.CenterVector3.ToIntVec3());

        if (distance > maxDistance && !pawn.pather.MovingNow) {
            pawn.pather.StartPath(target, endMode);
        } else if (distance <= maxDistance && pawn.pather.MovingNow) {
            pawn.pather.StopDead();
            pawn.jobs.curDriver.Notify_PatherArrived();
        }
    }

    public static bool IsAsleep(this Pawn pawn) {
        return pawn.CurJob?.def == RimWorld.JobDefOf.LayDown &&
               pawn.jobs.curDriver is JobDriver_LayDown { asleep: true };
    }

    public static float DistanceTo(this Pawn pawn, Verse.Thing thing) {
        return pawn.DistanceTo(thing.Position);
    }

    public static float DistanceTo(this Pawn pawn, IntVec3 position) {
        return pawn.Position.DistanceTo(position);
    }

    public static List<IntVec3> GetCellsAround(this Pawn pawn, float radius, bool useCenter = false) {
        return GenRadial.RadialCellsAround(
                pawn.Position,
                Mathf.Round(Math.Min(GenRadial.MaxRadialPatternRadius - .01f, radius)),
                useCenter
            )
            .Where(c => c.InBounds(pawn.Map))
            .ToList();
    }

    public static bool TryGetAbility<T>(this Pawn pawn, AbilityDef def, out T ability)
        where T : Ability {
        ability = (T)pawn.abilities.GetAbility(def);

        return ability != null;
    }

    public static bool TryGetAbility<T, TDef>(this Pawn pawn, AbilityDef def, out T ability)
        where T : Ability where TDef : AbilityDef {
        ability = (T)pawn.abilities.GetAbility((TDef)def);

        return ability != null;
    }

    public static T? GetAbility<T, TDef>(this Pawn pawn, AbilityDef def) where T : Ability where TDef : AbilityDef {
        return (T)pawn.abilities.GetAbility((TDef)def);
    }

    public static T? GetAbility<T, TDef>(this Pawn pawn, TDef def) where T : Ability where TDef : AbilityDef {
        return (T)pawn.abilities.GetAbility(def);
    }

    public static void BecomeMistborn(
        this Pawn pawn,
        bool canSnap = false,
        bool snapped = true,
        bool fillReserves = true,
        string? cause = null
    ) {
        if (!ModsConfig.IsActive("Cosmere.Scadrial") || Scadrial == null) return;

        Type? geneUtility = Scadrial.GetType("Cosmere.Scadrial.Utility.GeneUtility");
        MethodInfo? addMistborn = geneUtility?.GetMethod(
            "AddMistborn",
            BindingFlags.Public | BindingFlags.Static
        );

        addMistborn?.Invoke(null, [pawn, canSnap, snapped, cause]);
        pawn.SetAllomanticReserves(float.PositiveInfinity);
    }

    public static void BecomeFullFeruchemist(
        this Pawn pawn,
        bool canSnap = false,
        bool snapped = true,
        string? cause = null
    ) {
        if (!ModsConfig.IsActive("Cosmere.Scadrial") || Scadrial == null) return;

        Type? geneUtility = Scadrial.GetType("Cosmere.Scadrial.Utility.GeneUtility");
        MethodInfo? addFullFeruchemist = geneUtility?.GetMethod(
            "AddFullFeruchemist",
            BindingFlags.Public | BindingFlags.Static
        );

        addFullFeruchemist?.Invoke(null, [pawn, canSnap, snapped, cause]);
    }

    public static void SetAllomanticReserves(this Pawn pawn, float amount) {
        if (!ModsConfig.IsActive("Cosmere.Scadrial") || Scadrial == null) return;

        Type? extension = Scadrial.GetType("Cosmere.Scadrial.Extension.PawnExtension");
        MethodInfo? setAllAllomanticReserves = extension?.GetMethod(
            "SetAllAllomanticReserves",
            BindingFlags.Public | BindingFlags.Static
        );

        setAllAllomanticReserves?.Invoke(null, [pawn, amount]);
    }
}
