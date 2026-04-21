using Cosmere.Core.Ability;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using Verse;

namespace Cosmere.Core.UI.Radial;

public static class RadialDispatcher {
    public static void Dispatch(Pawn pawn, RadialLeaf leaf, string subsystemId, bool flareShift) {
        if (leaf.IsLocked) return;

        switch (leaf.Kind) {
            case RadialActionKind.StartAllomancyBurn:
                DispatchAllomancy(pawn, subsystemId, flareShift);
                return;
            case RadialActionKind.ToggleFeruchemyTap:
                DispatchFeruchemyDirection(pawn, subsystemId, FeruchemyDirection.Tap);
                return;
            case RadialActionKind.ToggleFeruchemyStore:
                DispatchFeruchemyDirection(pawn, subsystemId, FeruchemyDirection.Store);
                return;
            case RadialActionKind.ResetFeruchemyIdle:
                DispatchFeruchemyDirection(pawn, subsystemId, FeruchemyDirection.Idle);
                return;
            case RadialActionKind.InvokeCompound:
            case RadialActionKind.CastAbility:
                DispatchAbility(pawn, leaf.AbilityDef);
                return;
        }
    }

    private enum FeruchemyDirection { Tap, Store, Idle }

    private static void DispatchAllomancy(Pawn pawn, string metalDefName, bool flareShift) {
        if (pawn.abilities == null) return;
        List<RimWorld.Ability> abilities = pawn.abilities.AllAbilitiesForReading;
        for (int i = 0; i < abilities.Count; i++) {
            if (abilities[i] is not AllomancyAbility a || a.metal.defName != metalDefName) continue;
            Status next = a.atLeastBurning
                ? BurningStatus.Off
                : flareShift ? BurningStatus.Flaring : BurningStatus.Burning;
            a.UpdateStatus(next);
            return;
        }
    }

    private static void DispatchFeruchemyDirection(Pawn pawn, string metalDefName, FeruchemyDirection direction) {
        if (pawn.genes == null) return;
        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is not Feruchemist f || f.metal.defName != metalDefName) continue;
            switch (direction) {
                case FeruchemyDirection.Tap:
                    f.targetValue = 75f;
                    return;
                case FeruchemyDirection.Store:
                    f.targetValue = 25f;
                    return;
                case FeruchemyDirection.Idle:
                    f.Reset();
                    return;
            }
        }
    }

    private static void DispatchAbility(Pawn pawn, AbilityDef? def) {
        if (def == null || pawn.abilities == null) return;
        RimWorld.Ability? ability = pawn.abilities.GetAbility(def);
        if (ability == null || !ability.CanCast) return;

        if (def.targetRequired) {
            Find.Targeter.BeginTargeting(ability.verb);
            return;
        }

        ability.QueueCastingJob(pawn, LocalTargetInfo.Invalid);
    }
}
