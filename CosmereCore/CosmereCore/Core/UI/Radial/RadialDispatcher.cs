using Cosmere.Core;
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
        if (pawn.abilities == null) {
            Logger.Verbose($"radial dispatch: allomancy metal {metalDefName} skipped - pawn has no abilities");
            return;
        }
        List<RimWorld.Ability> abilities = pawn.abilities.AllAbilitiesForReading;
        for (int i = 0; i < abilities.Count; i++) {
            if (abilities[i] is not AllomancyAbility a || a.metal.defName != metalDefName) continue;
            Status next;
            if (flareShift) {
                next = a.status == BurningStatus.Flaring ? BurningStatus.Off : BurningStatus.Flaring;
            } else {
                next = a.atLeastBurning ? BurningStatus.Off : BurningStatus.Burning;
            }
            a.UpdateStatus(next);
            return;
        }
        Logger.Verbose($"radial dispatch: allomancy metal {metalDefName} not on pawn");
    }

    private static void DispatchFeruchemyDirection(Pawn pawn, string metalDefName, FeruchemyDirection direction) {
        if (pawn.genes == null) {
            Logger.Verbose($"radial dispatch: feruchemy metal {metalDefName} skipped - pawn has no genes");
            return;
        }
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
        Logger.Verbose($"radial dispatch: feruchemy metal {metalDefName} not on pawn");
    }

    private static void DispatchAbility(Pawn pawn, AbilityDef? def) {
        if (def == null) {
            Logger.Verbose("radial dispatch: ability def is null");
            return;
        }
        if (pawn.abilities == null) {
            Logger.Verbose($"radial dispatch: ability {def.defName} skipped - pawn has no abilities");
            return;
        }
        RimWorld.Ability? ability = pawn.abilities.GetAbility(def);
        if (ability == null) {
            Logger.Verbose($"radial dispatch: ability {def.defName} not on pawn");
            return;
        }
        if (!ability.CanCast) {
            Logger.Verbose($"radial dispatch: ability {def.defName} cannot cast");
            return;
        }

        if (def.targetRequired) {
            Find.Targeter.BeginTargeting(ability.verb);
            return;
        }

        ability.QueueCastingJob(pawn, LocalTargetInfo.Invalid);
    }
}
