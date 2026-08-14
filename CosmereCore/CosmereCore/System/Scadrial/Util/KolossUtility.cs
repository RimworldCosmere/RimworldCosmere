using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Util;

/// <summary>
///     Turning a person into a koloss, and the rules about who can become one.
/// </summary>
public static class KolossUtility {
    public const string KolossXenotype = "Cosmere_Scadrial_Xenotype_Koloss";
    public const string KolossBloodedXenotype = "Cosmere_Scadrial_Xenotype_KolossBlooded";

    public static bool IsKoloss(Pawn? pawn) {
        return pawn?.genes?.Xenotype?.defName == KolossXenotype;
    }

    public static bool IsKolossBlooded(Pawn? pawn) {
        return pawn?.genes?.Xenotype?.defName == KolossBloodedXenotype;
    }

    /// <summary>
    ///     Replaces everything the pawn was with the koloss xenotype, and starts the clock.
    /// </summary>
    /// <remarks>
    ///     The old xenotype's genes go rather than stack. A koloss made out of a Terris woman is
    ///     a koloss, not a Terris koloss - the spikes take what was there and leave something
    ///     that has no room for it.
    ///     <para>
    ///         Growth is added at severity zero however old the subject was. The clock starts at
    ///         the spikes, not at birth.
    ///     </para>
    /// </remarks>
    public static void Become(Pawn pawn, XenotypeDef koloss) {
        if (pawn.genes == null) return;

        pawn.genes.SetXenotype(koloss);

        if (pawn.health?.hediffSet?.GetFirstHediffOfDef(
                HediffDefOf.Cosmere_Scadrial_Hediff_KolossGrowth
            ) == null) {
            pawn.health?.AddHediff(HediffDefOf.Cosmere_Scadrial_Hediff_KolossGrowth);
        }

        // Nothing it knew survives in a form it can still use. Combat is muscle memory and
        // stays; everything that needed a mind to hold it does not.
        if (pawn.skills?.skills != null) {
            for (int i = 0; i < pawn.skills.skills.Count; i++) {
                SkillRecord skill = pawn.skills.skills[i];
                bool physical = skill.def == RimWorld.SkillDefOf.Melee || skill.def == RimWorld.SkillDefOf.Shooting;

                // levelInt, not Level. The getter returns 0 for a skill the pawn is currently
                // incapable of and adds trait and gene aptitude on top of what is stored, so
                // reading through it would take two off a number that was never there.
                skill.levelInt = physical ? Mathf.Max(0, skill.levelInt - 2) : 0;
                skill.passion = Passion.None;
            }
        }

        Forget(pawn);
    }

    /// <summary>
    ///     Takes the person away and leaves the thing.
    /// </summary>
    /// <remarks>
    ///     Become's own message says whoever they were did not come back, and the koloss childhood
    ///     backstory says it does not remember the name it had. Until now the code disagreed with
    ///     both: the name, the ideoligion, the traits and every relationship survived intact, so a
    ///     colonist's husband could be made into a koloss and stay her husband.
    ///     <para>
    ///         The faction is deliberately left alone. Whether a made koloss is still yours is a
    ///         question about the colony, not about what it is.
    ///     </para>
    /// </remarks>
    private static void Forget(Pawn pawn) {
        pawn.Name = new NameSingle("CS_Koloss_Name".Translate(), true);

        pawn.ideo?.SetIdeo(null);

        if (pawn.story?.traits != null) {
            List<Trait> had = [.. pawn.story.traits.allTraits];
            for (int i = 0; i < had.Count; i++) {
                pawn.story.traits.RemoveTrait(had[i]);
            }
        }

        if (pawn.relations != null) {
            pawn.relations.ClearAllRelations();
        }
    }
}
