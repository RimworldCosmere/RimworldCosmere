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

    /// <summary>How many spikes are still in it. Counting them is Hemalurgy's question.</summary>
    public static int SpikeCount(Pawn? pawn) {
        return Hemalurgy.HemalurgicSpikeUtility.SpikeCount(pawn);
    }

    /// <summary>
    ///     Checks whether there are still enough spikes to hold a koloss together.
    /// </summary>
    /// <remarks>
    ///     Four made it. Pulling one is a surgeon's decision with a consequence, so this runs on a
    ///     slow tick and after any spike surgery rather than watching for it between ticks.
    ///     <para>
    ///         What a part-spiked koloss becomes is not decided yet - for now this only keeps
    ///         Ruin's reading honest, so a koloss with two spikes left does not still read as a
    ///         thing with four.
    ///     </para>
    /// </remarks>
    public static void ReconcileSpikes(Pawn pawn) {
        if (pawn.health?.hediffSet == null) return;
        if (!IsKoloss(pawn)) return;

        Hemalurgy.HemalurgicImplantUtility.UpdateRuinsInfluence(pawn);
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

    /// <summary>
    ///     Consumes a person and stands a koloss up where they were.
    /// </summary>
    /// <remarks>
    ///     A new pawn rather than the old one rewritten. Mutating the subject meant unpicking every
    ///     gene of whatever xenotype it used to be, by hand, on a live colonist - and RimWorld has
    ///     no ClearEndogenes, so that was a reverse-index loop over RemoveGene firing trait,
    ///     passion and graphics side effects per gene. Generating instead is both safer and truer:
    ///     what gets up is not the person who lay down.
    ///     <para>
    ///         Nothing is left of the body. The flesh went into the thing standing over it, so
    ///         there is no corpse to bury and no spikes to take back out of one.
    ///     </para>
    /// </remarks>
    public static Pawn? Make(Pawn subject, XenotypeDef koloss) {
        Map? map = subject.Map;
        IntVec3 where = subject.Position;
        if (map == null) return null;

        // Gender is the one thing that carries: a koloss is built out of a body, and the body
        // had a sex.
        Pawn made = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
            KolossKind ?? RimWorld.PawnKindDefOf.Colonist,
            subject.Faction,
            PawnGenerationContext.NonPlayer,
            forceGenerateNewPawn: true,
            canGeneratePawnRelations: false,
            fixedGender: subject.gender,
            forcedXenotype: koloss
        ));

        Inherit(subject, made);

        // Before the subject goes, so the koloss is standing where they were rather than dropped
        // at the map edge.
        subject.Destroy(DestroyMode.Vanish);
        GenSpawn.Spawn(made, where, map);

        return made;
    }

    /// <summary>
    ///     What the subject's body was worth.
    /// </summary>
    /// <remarks>
    ///     Not the mind - a koloss has none to speak of, and the backstory says it does not
    ///     remember its name. What survives is what the muscle knew: a fighter makes a better
    ///     koloss than a clerk does, which is the only reason to care who goes on the table.
    /// </remarks>
    private static void Inherit(Pawn subject, Pawn made) {
        SkillRecord? theirMelee = subject.skills?.GetSkill(RimWorld.SkillDefOf.Melee);
        SkillRecord? ourMelee = made.skills?.GetSkill(RimWorld.SkillDefOf.Melee);

        // levelInt, not Level. The getter returns 0 for a skill the pawn is incapable of and adds
        // aptitude on top of what is stored.
        if (theirMelee != null && ourMelee != null) {
            ourMelee.levelInt = Mathf.Clamp(theirMelee.levelInt, ourMelee.levelInt, 20);
        }

        if (made.skills?.skills == null) return;

        // Everything that needed a mind to hold it did not survive the spikes.
        for (int i = 0; i < made.skills.skills.Count; i++) {
            SkillRecord skill = made.skills.skills[i];
            if (skill.def == RimWorld.SkillDefOf.Melee) continue;

            skill.levelInt = 0;
            skill.passion = Passion.None;
        }
    }

    private static PawnKindDef? KolossKind =>
        DefDatabase<PawnKindDef>.GetNamedSilentFail("Cosmere_Scadrial_PawnKind_Koloss");
}
