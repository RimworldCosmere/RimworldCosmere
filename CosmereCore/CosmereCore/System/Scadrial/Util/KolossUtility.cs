using System.Collections.Generic;
using System.Linq;
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

        // nothing it knew survives in a usable form - combat is muscle memory and stays, everything else does not.
        if (pawn.skills?.skills != null) {
            for (int i = 0; i < pawn.skills.skills.Count; i++) {
                SkillRecord skill = pawn.skills.skills[i];
                bool physical = skill.def == RimWorld.SkillDefOf.Melee || skill.def == RimWorld.SkillDefOf.Shooting;

                // levelInt, not Level - the getter returns 0 when incapable and adds aptitude on top.
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
        return MakeFrom(subject, koloss);
    }

    /// <summary>
    ///     The same, for a subject who may already be dead.
    /// </summary>
    /// <remarks>
    ///     The ritual path kills before the outcome fires, so by then the subject is a corpse
    ///     standing in for a person. Both paths end the same way - nothing of the body is left -
    ///     so both come through here.
    /// </remarks>
    public static Pawn? MakeFrom(Pawn subject, XenotypeDef koloss) {
        Cosmere.Core.Logger.Important($"Koloss: making one out of {subject.LabelShort}.");

        // must read before the subject goes - asking after always says no, losing the player's selection.
        bool watching = Find.Selector?.IsSelected(subject) == true
                        || (subject.Corpse != null && Find.Selector?.IsSelected(subject.Corpse) == true);

        Corpse? corpse = subject.Corpse;
        Map? map = subject.MapHeld ?? corpse?.Map;
        IntVec3 where = corpse?.Spawned == true ? corpse.Position : subject.Position;
        if (map == null) return null;

        // gender carries from the body, but biological age must stay adult - zero there makes a baby, not a koloss.
        Pawn made = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
            KolossKind ?? RimWorld.PawnKindDefOf.Colonist,
            subject.Faction,
            PawnGenerationContext.NonPlayer,
            forceGenerateNewPawn: true,
            canGeneratePawnRelations: false,
            fixedGender: subject.gender,
            forcedXenotype: koloss,
            fixedBiologicalAge: Rand.Range(MinAdultAge, MaxAdultAge),
            fixedChronologicalAge: 0f
        ));

        Inherit(subject, made);
        CarrySpikes(subject, made);
        StartGrowing(made);
        Disfigure(made);

        // one name, no family - whatever it was called belonged to somebody who is not here anymore.
        made.Name = new NameSingle(
            PawnBioAndNameGenerator.GeneratePawnName(made, NameStyle.Full).ToStringShort,
            true
        );

        // fixedChronologicalAge on the generation request does not survive - the tracker is set directly instead.
        if (made.ageTracker != null) made.ageTracker.AgeChronologicalTicks = 0;

        // destroys corpse and pawn both before spawning, or the body stays on the floor with nothing in it.
        if (corpse is { Destroyed: false }) corpse.Destroy(DestroyMode.Vanish);
        if (!subject.Destroyed) subject.Destroy(DestroyMode.Vanish);

        GenSpawn.Spawn(made, where, map);

        // keeps the camera on the koloss if it was on the person - losing it at the transformation is the worst time.
        if (watching) {
            Find.Selector?.ClearSelection();
            Find.Selector?.Select(made);
        }

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

        // levelInt, not Level - the getter returns 0 when incapable and adds aptitude on top of the stored value.
        if (theirMelee != null && ourMelee != null) {
            ourMelee.levelInt = Mathf.Clamp(theirMelee.levelInt, ourMelee.levelInt, 20);
        }

        if (made.skills?.skills == null) return;

        // a koloss is not an object - it can stamp out a fire or hold a bandage on, badly, and never gets better at it.
        for (int i = 0; i < made.skills.skills.Count; i++) {
            SkillRecord skill = made.skills.skills[i];
            if (skill.def == RimWorld.SkillDefOf.Melee) continue;

            skill.levelInt = Clumsy.Contains(skill.def) ? 1 : 0;
            skill.passion = Passion.None;
        }
    }

    /// <summary>
    ///     Whatever was already driven into the subject comes across with it.
    /// </summary>
    /// <remarks>
    ///     The four that make a koloss are added by <c>SpikeBound</c> during generation. These are
    ///     the ones the subject already had - a Misting made by hemalurgy, a kandra, an Inquisitor
    ///     part-way through. Those spikes are physically in the body that is being used, so they do
    ///     not fall out because the body changed shape, and Ruin's hold reads the total.
    /// </remarks>
    private static void CarrySpikes(Pawn subject, Pawn made) {
        Verse.Hediff? had = subject.health?.hediffSet?.GetFirstHediffOfDef(
            Hemalurgy.HemalurgicDefOf.Cosmere_Scadrial_Hediff_HemalurgicSpikes
        );
        if (had is not Hemalurgy.Hediff.HemalurgicSpikes theirs || theirs.spikes.Count == 0) return;

        BodyPartRecord? core = made.RaceProps?.body?.corePart == null
            ? null
            : made.health?.hediffSet?.GetNotMissingParts()
                .FirstOrDefault(p => p.def == made.RaceProps.body.corePart.def);

        List<Hemalurgy.ImplantedSpikeData> carried = [.. theirs.spikes];
        for (int i = 0; i < carried.Count; i++) {
            Hemalurgy.HemalurgicImplantUtility.AddToUnifiedHediff(made, carried[i], core);
        }

        // Ruin speaks through the total, so it must be recomputed after the carry, not left at the four alone.
        Hemalurgy.HemalurgicImplantUtility.UpdateRuinsInfluence(made);
    }

    /// <summary>The handful of things a koloss can still be pointed at, badly.</summary>
    private static readonly HashSet<SkillDef> Clumsy = [
        RimWorld.SkillDefOf.Cooking,
        RimWorld.SkillDefOf.Crafting,
        RimWorld.SkillDefOf.Medicine,
        RimWorld.SkillDefOf.Social,
        RimWorld.SkillDefOf.Animals,
        RimWorld.SkillDefOf.Mining,
        RimWorld.SkillDefOf.Construction,
    ];

    /// <summary>
    ///     Every koloss looks the same way, and none of them have a personality left to roll.
    /// </summary>
    /// <remarks>
    ///     Generation hands out whatever traits it likes, which produced koloss who were delicate
    ///     and pyromaniac. What a koloss actually is, is enormous and hard to look at - so the
    ///     rolled ones go and the one that describes the thing goes on. Anything a gene or a hediff
    ///     adds afterwards, such as Invested, is untouched by this.
    /// </remarks>
    private static void Disfigure(Pawn made) {
        if (made.story?.traits == null) return;

        List<Trait> rolled = [.. made.story.traits.allTraits];
        for (int i = 0; i < rolled.Count; i++) {
            made.story.traits.RemoveTrait(rolled[i]);
        }

        TraitDef? beauty = DefDatabase<TraitDef>.GetNamedSilentFail("Beauty");

        // Degree -2 is "staggeringly ugly". The spikes did not leave much of the face.
        if (beauty != null) made.story.traits.GainTrait(new Trait(beauty, -2, true));
    }

    /// <summary>
    ///     A body that has finished growing up, before it starts growing wrong.
    /// </summary>
    /// <remarks>
    ///     Rolled rather than fixed. Every koloss coming out of the slab at exactly twenty read as
    ///     a template rather than a person something was done to.
    /// </remarks>
    private const float MinAdultAge = 18f;

    private const float MaxAdultAge = 42f;

    /// <summary>
    ///     Makes sure the clock is running.
    /// </summary>
    /// <remarks>
    ///     <c>KolossHeritage.PostAdd</c> is meant to cover this, but gene PostAdd runs during
    ///     generation, before the health tracker is in a state worth writing to - so the hediff
    ///     silently never landed and the koloss had no growth row at all. Adding it here as well
    ///     costs nothing: both paths check for it first.
    /// </remarks>
    private static void StartGrowing(Pawn made) {
        if (made.health?.hediffSet == null) {
            Cosmere.Core.Logger.Warning($"Koloss: {made.LabelShort} has no health tracker, so no clock started.");

            return;
        }

        HediffDef? growth = HediffDefOf.Cosmere_Scadrial_Hediff_KolossGrowth;
        if (growth == null) {
            Cosmere.Core.Logger.Warning("Koloss: the growth hediff def is missing, so nothing starts the clock.");
            return;
        }

        if (made.health.hediffSet.GetFirstHediffOfDef(growth) != null) {
            Cosmere.Core.Logger.Important($"Koloss: {made.LabelShort} was already growing.");

            return;
        }

        made.health.AddHediff(growth);
        Cosmere.Core.Logger.Important($"Koloss: {made.LabelShort} started growing.");
    }

    private static PawnKindDef? KolossKind =>
        DefDatabase<PawnKindDef>.GetNamedSilentFail("Cosmere_Scadrial_PawnKind_Koloss");
}
