using System;
using RimWorld;

namespace CosmereScadrial.Flags {
    [Flags]
    public enum TargetFlags : ushort {
        None = 0,
        Locations = 1 << 0,
        Self = 1 << 1,
        Pawns = 1 << 2,
        Fires = 1 << 3,
        Buildings = 1 << 4,
        Items = 1 << 5,
        Animals = 1 << 6,
        Humans = 1 << 7,
        Mechs = 1 << 8,
        Plants = 1 << 9,
        Mutants = 1 << 10,
        WorldCell = 1 << 11,
        Humanlike = Humans | Mechs | Mutants,
        HumanlikeOrAnimal = Humanlike | Animals,
        Objects = Buildings | Items | Plants,
        OtherTargets = Fires | Buildings | Items | Plants | WorldCell,
    }

    public static class TargetFlagsExtensions {
        public static bool Has(this TargetFlags flags, TargetFlags mask) {
            return (flags & mask) != 0;
        }

        public static bool IsOnlySelf(this TargetFlags flags) {
            return flags == TargetFlags.Self;
        }

        public static bool IsOnlyOther(this TargetFlags flags) {
            return !flags.HasFlag(TargetFlags.Self) && flags.Has(TargetFlags.OtherTargets);
        }

        public static bool IsOnlyOtherHumanlike(this TargetFlags flags) {
            return !flags.HasFlag(TargetFlags.Self) && flags.Has(TargetFlags.Humanlike);
        }

        public static bool IsOnlyOtherAnimal(this TargetFlags flags) {
            return !flags.HasFlag(TargetFlags.Self) && flags.Has(TargetFlags.Animals);
        }

        public static bool IsOnlyOtherHumanlikeOrAnimal(this TargetFlags flags) {
            return !flags.HasFlag(TargetFlags.Self) && flags.Has(TargetFlags.HumanlikeOrAnimal);
        }

        public static bool IsOnlyObject(this TargetFlags flags) {
            return !flags.HasFlag(TargetFlags.Self) && flags.Has(TargetFlags.Objects) && !flags.CanTargetPawns();
        }

        public static bool IsSelfOrOther(this TargetFlags flags) {
            return flags.HasFlag(TargetFlags.Self) && flags.Has(TargetFlags.OtherTargets);
        }

        public static bool IsSelfOrOtherPawn(this TargetFlags flags) {
            return flags.HasFlag(TargetFlags.Self) && flags.Has(TargetFlags.Pawns);
        }

        public static bool IsSelfOrOtherHumanlike(this TargetFlags flags) {
            return flags.HasFlag(TargetFlags.Self) && flags.Has(TargetFlags.Humanlike);
        }

        public static bool IsSelfOrOtherHumanlikeOrAnimal(this TargetFlags flags) {
            return flags.HasFlag(TargetFlags.Self) && flags.Has(TargetFlags.HumanlikeOrAnimal);
        }

        public static bool IsEmpty(this TargetFlags flags) {
            return flags == TargetFlags.None;
        }

        public static TargetFlags OnlySelf() {
            return TargetFlags.Self;
        }

        public static TargetFlags OnlyOther() {
            return TargetFlags.OtherTargets;
        }

        public static TargetFlags OnlyOtherHumanlike() {
            return TargetFlags.Humanlike;
        }

        public static TargetFlags SelfAndOther() {
            return TargetFlags.Self | TargetFlags.OtherTargets;
        }

        public static TargetFlags SelfAndOtherHumanlike() {
            return TargetFlags.Self | TargetFlags.Humanlike;
        }

        public static bool CanTargetPawns(this TargetFlags flags) {
            return flags.HasFlag(TargetFlags.Pawns);
        }

        public static bool CanTargetWorldCell(this TargetFlags flags) {
            return flags.HasFlag(TargetFlags.WorldCell);
        }

        public static bool CanTargetAnimals(this TargetFlags flags) {
            return flags.HasFlag(TargetFlags.Animals);
        }

        public static bool CanTargetAnything(this TargetFlags flags) {
            return !flags.IsEmpty();
        }

        public static TargetFlags FromAbilityDef(AbilityDef def) {
            var flags = TargetFlags.None;
            var verbProps = def.verbProperties;
            var param = verbProps.targetParams;
            if (!verbProps.targetable) {
                return TargetFlags.Self;
            }

            if (def.targetWorldCell) flags |= TargetFlags.WorldCell;
            if (param.canTargetLocations) flags |= TargetFlags.Locations;
            if (param.canTargetSelf) flags |= TargetFlags.Self;
            if (param.canTargetPawns) flags |= TargetFlags.Pawns;
            if (param.canTargetFires) flags |= TargetFlags.Fires;
            if (param.canTargetBuildings) flags |= TargetFlags.Buildings;
            if (param.canTargetItems) flags |= TargetFlags.Items;
            if (param.canTargetAnimals) flags |= TargetFlags.Animals | TargetFlags.Pawns;
            if (param.canTargetHumans) flags |= TargetFlags.Humans | TargetFlags.Pawns;
            if (param.canTargetMechs) flags |= TargetFlags.Mechs | TargetFlags.Pawns;
            if (param.canTargetPlants) flags |= TargetFlags.Plants;
            if (param.canTargetMutants) flags |= TargetFlags.Mutants | TargetFlags.Pawns;

            return flags;
        }
    }
}