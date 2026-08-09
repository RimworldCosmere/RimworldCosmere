using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Kandra;

/// <summary>
///     Moves everything that belongs to the kandra rather than to the body it is wearing.
/// </summary>
/// <remarks>
///     The animal is a different pawn, freshly generated, so by default it arrives as a stranger
///     with its own skills, its own needs and whatever hediffs generation felt like. None of that
///     is right: the person walking around is the kandra, and should read that way in every tab.
///     <para>
///         What moves is what the kandra is. What stays is what the body is: injuries taken in a
///         shape belong to that shape, because the kandra was wearing it rather than living in it.
///     </para>
/// </remarks>
public static class KandraShapeTransfer {
    /// <summary>Stepping into a shape. The new body is a blank, so everything moves.</summary>
    public static void Into(Pawn kandra, Pawn shape) {
        Identity(kandra, shape);
        Skills(kandra, shape);
        Needs(kandra, shape);
        Hediffs(kandra, shape);
        Relations(kandra, shape);
        Records(kandra, shape);
    }

    /// <summary>
    ///     Stepping back out. Only what changed while wearing the shape comes back.
    /// </summary>
    /// <remarks>
    ///     Deliberately not the reverse of <see cref="Into" />. That one clears the target's
    ///     hediffs first, which is right for a freshly generated animal and catastrophic for the
    ///     kandra: its spikes are anchored to its torso and would be wiped, taking the Blessing
    ///     and the kandra's mind with them.
    /// </remarks>
    public static void OutOf(Pawn shape, Pawn kandra) {
        Skills(shape, kandra);
        Needs(shape, kandra);
        Records(shape, kandra);
    }

    private static void Identity(Pawn from, Pawn to) {
        if (from.Name != null) to.Name = from.Name;

        to.gender = from.gender;

        if (from.story != null && to.story != null) {
            to.story.Childhood = from.story.Childhood;
            to.story.Adulthood = from.story.Adulthood;
            to.story.traits = from.story.traits;
        }

        // Without this the shape rolls its own, and a kandra walks out as a Skaa wolfhound.
        if (from.genes?.Xenotype != null) to.genes?.SetXenotypeDirect(from.genes.Xenotype);

        if (from.Ideo != null) to.ideo?.SetIdeo(from.Ideo);
    }

    private static void Skills(Pawn from, Pawn to) {
        if (from.skills?.skills == null || to.skills?.skills == null) return;

        for (int i = 0; i < from.skills.skills.Count; i++) {
            SkillRecord source = from.skills.skills[i];
            SkillRecord? target = to.skills.GetSkill(source.def);
            if (target == null) continue;

            // levelInt, not Level. The getter returns 0 for a skill the pawn is currently
            // incapable of, and adds trait and gene aptitude on top of what is stored. Reading
            // through it zeroed every skill the animal shape disables, permanently, and baked
            // aptitude bonuses in again on every round trip.
            target.levelInt = source.levelInt;
            target.xpSinceLastLevel = source.xpSinceLastLevel;
            target.passion = source.passion;
        }
    }

    /// <summary>
    ///     Carries hunger, rest and mood across, for the needs both bodies happen to have.
    /// </summary>
    /// <remarks>
    ///     A dog has no need for beauty and a person has no need for taming. Only the overlap
    ///     moves, and stepping into a fresh shape does not reset how tired or hungry the kandra
    ///     was a moment ago.
    /// </remarks>
    private static void Needs(Pawn from, Pawn to) {
        if (from.needs?.AllNeeds == null || to.needs?.AllNeeds == null) return;

        List<Need> theirs = from.needs.AllNeeds;
        for (int i = 0; i < theirs.Count; i++) {
            Need? mine = to.needs.TryGetNeed(theirs[i].def);
            if (mine == null) continue;

            mine.CurLevel = theirs[i].CurLevel;
        }
    }

    /// <summary>
    ///     Moves what the kandra is, and leaves what happened to the body.
    /// </summary>
    /// <remarks>
    ///     Spikes, Blessings and the mistwraith state all travel: they are the kandra, and a
    ///     kandra in a dog is still spiked. Injuries and missing parts do not, because they were
    ///     done to a shape rather than to the thing wearing it, and a body part record from one
    ///     body means nothing in another.
    /// </remarks>
    private static void Hediffs(Pawn from, Pawn to) {
        if (from.health?.hediffSet == null || to.health == null) return;

        // A generated animal arrives with its own scars and ailments. None of them are ours.
        to.health.RemoveAllHediffs();

        List<Hediff> theirs = [.. from.health.hediffSet.hediffs];
        for (int i = 0; i < theirs.Count; i++) {
            Hediff hediff = theirs[i];
            if (hediff is Hediff_Injury or Hediff_MissingPart or Hediff_AddedPart) continue;

            // Whole-body only. A part-anchored hediff cannot point at anything in a new body.
            if (hediff.Part != null) continue;

            to.health.AddHediff(hediff.def);
        }
    }

    /// <summary>
    ///     Copies the kandra's direct relations so the colony still knows who this is.
    /// </summary>
    /// <remarks>
    ///     One-way on purpose. The other side of each relation still points at the kandra, which
    ///     is correct: the kandra is the one who has a brother, and it is only borrowing a body.
    ///     The copy exists so the social tab on the animal is not blank.
    /// </remarks>
    private static void Relations(Pawn from, Pawn to) {
        if (from.relations == null || to.relations == null) return;

        List<DirectPawnRelation> theirs = [.. from.relations.DirectRelations];
        for (int i = 0; i < theirs.Count; i++) {
            if (theirs[i].otherPawn == null) continue;
            if (to.relations.DirectRelationExists(theirs[i].def, theirs[i].otherPawn)) continue;

            to.relations.AddDirectRelation(theirs[i].def, theirs[i].otherPawn);
        }
    }

    /// <summary>Kills counted as a dog still belong on the kandra's record.</summary>
    private static void Records(Pawn from, Pawn to) {
        if (from.records == null || to.records == null) return;

        List<RecordDef> all = DefDatabase<RecordDef>.AllDefsListForReading;
        for (int i = 0; i < all.Count; i++) {
            // Time records are ticked up by the game and AddTo refuses them outright.
            if (all[i].type == RecordType.Time) continue;

            float value = from.records.GetValue(all[i]);
            if (value <= 0f) continue;

            to.records.AddTo(all[i], value);
        }
    }
}
