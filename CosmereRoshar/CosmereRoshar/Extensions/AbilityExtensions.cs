using System.Linq;
using RimWorld;
using Verse;

namespace CosmereRoshar.Extensions;

public static class AbilityExtensions {
    public static T? GetAbilityComp<T>(this Pawn pawn, string abilityDefName) where T : CompAbilityEffect {
        if (pawn.abilities == null) return null;

        Ability ability = pawn.abilities.GetAbility(DefDatabase<AbilityDef>.GetNamed(abilityDefName));
        return ability?.comps?.OfType<T>().FirstOrDefault();
    }
}