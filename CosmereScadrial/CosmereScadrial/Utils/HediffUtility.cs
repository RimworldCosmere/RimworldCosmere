using CosmereScadrial.Abilities.Allomancy;
using CosmereScadrial.Abilities.Allomancy.Hediffs;
using Verse;

namespace CosmereScadrial.Utils;

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

    public static AllomanticHediff CreateHediff(HediffDef def, Pawn target, AbstractAbility ability) {
        AllomanticHediff? newHediff = new AllomanticHediff(def, target, ability) {
            loadID = Find.UniqueIDsManager.GetNextHediffID(),
        };
        newHediff.PostMake();
        newHediff.AddSource(ability);

        return newHediff;
    }

    public static AllomanticHediff? GetOrAddHediff(Pawn target, AbstractAbility ability, HediffDef? hediffDef) {
        if (hediffDef == null) return null;

        if (TryGetHediff(target, hediffDef, out AllomanticHediff hediff)) {
            hediff.AddSource(ability);
            return hediff;
        }

        AllomanticHediff? newHediff = CreateHediff(hediffDef, target, ability);

        target.health.AddHediff(newHediff);

        return newHediff;
    }

    public static AllomanticHediff? GetOrAddHediff(Pawn caster, Pawn target, AbstractAbility ability,
        IMultiTypeHediff def) {
        return GetOrAddHediff(target, ability, GetHediffDefForPawn(caster, target, def));
    }

    public static void RemoveHediff(Pawn target, AbstractAbility ability, HediffDef? hediffDef) {
        if (hediffDef == null) return;

        if (!TryGetHediff(target, hediffDef, out AllomanticHediff hediff)) {
            return;
        }

        hediff.RemoveSource(ability);
    }

    public static void RemoveHediff(Pawn caster, Pawn target, AbstractAbility ability, IMultiTypeHediff def) {
        RemoveHediff(target, ability, GetHediffDefForPawn(caster, target, def));
    }

    private static bool TryGetHediff(Pawn target, HediffDef def, out AllomanticHediff hediff) {
        hediff = null;
        if (def == null) return false;

        Hediff uncastHediff = null;

        target.health?.hediffSet?.TryGetHediff(
            def,
            out uncastHediff
        );
        hediff = (AllomanticHediff)uncastHediff;

        return hediff != null;
    }
}