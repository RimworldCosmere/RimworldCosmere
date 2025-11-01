using System;
using Cosmere.Core;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Allomancy.Hediff;
using Verse;

namespace Cosmere.System.Scadrial.Util;

public static class HediffUtility {
    public static HediffDef GetHediffDefForPawn(Pawn caster, Pawn target, IMultiTypeHediff hediff) {
        if (hediff.GetFriendlyHediff() != null && target.Faction == caster.Faction) {
            return hediff.GetFriendlyHediff()!;
        }

        if (hediff.GetHostileHediff() != null && target.Faction != caster.Faction) {
            return hediff.GetHostileHediff()!;
        }

        return hediff.GetHediff()!;
    }

    public static AllomanticHediff CreateHediff(HediffDef def, Pawn target, AllomancyAbility ability) {
        AllomanticHediff newHediff = (AllomanticHediff)Activator.CreateInstance(def.hediffClass, def, target, ability);
        newHediff.loadID = Find.UniqueIDsManager.GetNextHediffID();
        newHediff.PostMake();
        newHediff.AddSource(ability);

        return newHediff;
    }

    public static AllomanticHediff? GetOrAddHediff(
        Pawn caster,
        Pawn target,
        AllomancyAbility ability,
        HediffDef? hediffDef
    ) {
        if (hediffDef == null) return null;

        if (TryGetHediff(target, hediffDef, out AllomanticHediff hediff)) {
            hediff.AddSource(ability);
            return hediff;
        }

        AllomanticHediff? newHediff = CreateHediff(hediffDef, target, ability);

        target.health.AddHediff(newHediff);

        return newHediff;
    }

    public static AllomanticHediff? GetOrAddHediff(
        Pawn caster,
        Pawn target,
        AllomancyAbility ability,
        IMultiTypeHediff def
    ) {
        return GetOrAddHediff(caster, target, ability, GetHediffDefForPawn(caster, target, def));
    }

    public static void RemoveHediff(Pawn caster, Pawn target, AllomancyAbility ability, HediffDef? hediffDef) {
        if (hediffDef == null) return;

        if (!TryGetHediff(target, hediffDef, out AllomanticHediff hediff)) {
            return;
        }

        hediff.RemoveSource(ability);
    }

    public static void RemoveHediff(Pawn caster, Pawn target, AllomancyAbility ability, IMultiTypeHediff def) {
        RemoveHediff(caster, target, ability, GetHediffDefForPawn(caster, target, def));
    }

    private static bool TryGetHediff(Pawn target, HediffDef? def, out AllomanticHediff hediff) {
        hediff = null!;
        if (def == null) return false;

        Hediff? uncastHediff = null;

        target.health?.hediffSet?.TryGetHediff(
            def,
            out uncastHediff
        );
        if (uncastHediff == null) return false;

        hediff = (AllomanticHediff)uncastHediff;

        return true;
    }
}