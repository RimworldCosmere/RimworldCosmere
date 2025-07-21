using System;
using Cosmere.Core.Ability;
using Cosmere.Core.Gene;
using Cosmere.Core.Hediff;
using Cosmere.Core.Util;
using Verse;

namespace Cosmere.Core.Extension;

public static class PawnExtension {
    public static bool IsShieldedAgainstInvestiture(this Pawn pawn) {
        return InvestitureDetector.IsShielded(pawn);
    }

    public static IHediff<TGene>? GetOrAddHediff<TGene>(
        this Pawn pawn,
        IAbility<TGene, IHediff<TGene>> ability,
        HediffDef? hediffDef
    ) where TGene : Invested {
        if (hediffDef == null || ability == null) return null;

        if (pawn.TryGetHediff<TGene, IHediff<TGene>>(hediffDef, out IHediff<TGene> hediff)) {
            hediff.AddSource(ability);
            return hediff;
        }

        IHediff<TGene> newHediff = pawn.CreateHediff(hediffDef, ability);

        pawn.health.AddHediff(newHediff as HediffWithComps);

        return newHediff;
    }

    public static AbstractHediff<TGene>? GetOrAddHediff<TGene>(
        this Pawn pawn,
        Pawn caster,
        IAbility<TGene, IHediff<TGene>>? ability,
        IMultiTypeHediff def
    ) where TGene : Invested {
        return (AbstractHediff<TGene>)pawn.GetOrAddHediff(ability, pawn.GetHediffDefForPawn(caster, def));
    }

    public static void RemoveHediff<TGene>(
        this Pawn pawn,
        Pawn caster,
        IAbility<TGene, IHediff<TGene>>? ability,
        IMultiTypeHediff def
    ) where TGene : Invested {
        pawn.RemoveHediff(ability, pawn.GetHediffDefForPawn(caster, def));
    }

    public static void RemoveHediff<TGene, THediff>(
        this Pawn pawn,
        IAbility<TGene, THediff>? ability,
        HediffDef? hediffDef
    ) where TGene : Invested where THediff : IHediff<TGene> {
        if (hediffDef == null || ability == null) return;

        if (!pawn.TryGetHediff<TGene, IHediff<TGene>>(hediffDef, out IHediff<TGene> hediff)) {
            return;
        }

        hediff.RemoveSource((IAbility<TGene, IHediff<TGene>>)ability);
    }

    private static IHediff<TGene> CreateHediff<TGene>(
        this Pawn pawn,
        HediffDef def,
        IAbility<TGene, IHediff<TGene>> ability
    ) where TGene : Invested {
        IHediff<TGene> newHediff =
            (IHediff<TGene>)Activator.CreateInstance(def.hediffClass, def, pawn, ability);
        ((HediffWithComps)newHediff).loadID = Find.UniqueIDsManager.GetNextHediffID();
        newHediff.PostMake();
        newHediff.AddSource(ability);

        return newHediff;
    }

    private static bool TryGetHediff<TGene, THediff>(this Pawn pawn, HediffDef? def, out THediff hediff)
        where TGene : Invested where THediff : class, IHediff<TGene> {
        hediff = null!;
        if (def == null) return false;

        Verse.Hediff? uncastHediff = null;

        pawn.health?.hediffSet?.TryGetHediff(
            def,
            out uncastHediff
        );
        if (uncastHediff == null) return false;

        hediff = uncastHediff as THediff;

        return true;
    }

    private static HediffDef GetHediffDefForPawn(this Pawn pawn, Pawn caster, IMultiTypeHediff hediff) {
        if (hediff.GetFriendlyHediff() != null && pawn.Faction == caster.Faction) {
            return hediff.GetFriendlyHediff()!;
        }

        if (hediff.GetHostileHediff() != null && pawn.Faction != caster.Faction) {
            return hediff.GetHostileHediff()!;
        }

        return hediff.GetHediff()!;
    }
}