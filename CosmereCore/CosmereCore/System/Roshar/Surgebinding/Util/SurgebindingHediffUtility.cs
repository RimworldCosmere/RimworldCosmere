using System;
using Cosmere.System.Roshar.Surgebinding.Ability;
using Cosmere.System.Roshar.Surgebinding.Hediff;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Util;

public static class SurgebindingHediffUtility {
    public static SurgebindingHediff? GetOrAddHediff(
        Pawn target,
        SurgebindingAbility ability,
        HediffDef? hediffDef
    ) {
        if (hediffDef == null) return null;

        if (TryGetHediff(target, hediffDef, out SurgebindingHediff hediff)) {
            hediff.AddSource(ability);
            return hediff;
        }

        SurgebindingHediff newHediff =
            (SurgebindingHediff)Activator.CreateInstance(hediffDef.hediffClass, hediffDef, target, ability);
        newHediff.loadID = Find.UniqueIDsManager.GetNextHediffID();
        newHediff.PostMake();
        newHediff.AddSource(ability);

        target.health.AddHediff(newHediff);

        return newHediff;
    }

    public static void RemoveHediff(
        Pawn target,
        SurgebindingAbility ability,
        HediffDef? hediffDef
    ) {
        if (hediffDef == null) return;

        if (!TryGetHediff(target, hediffDef, out SurgebindingHediff hediff)) return;

        hediff.RemoveSource(ability);

        if (hediff.SourceAbilities.Count == 0) {
            target.health.RemoveHediff(hediff);
        }
    }

    private static bool TryGetHediff(Pawn target, HediffDef def, out SurgebindingHediff hediff) {
        hediff = null!;

        Verse.HediffSet? hediffSet = target.health?.hediffSet;
        if (hediffSet == null) return false;

        hediffSet.TryGetHediff(def, out Verse.Hediff? uncastHediff);
        if (uncastHediff is not SurgebindingHediff surgebinding) return false;

        hediff = surgebinding;
        return true;
    }
}
