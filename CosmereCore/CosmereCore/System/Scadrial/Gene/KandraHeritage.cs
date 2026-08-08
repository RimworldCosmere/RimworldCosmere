using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Gene;

/// <summary>
///     Being a kandra, including which generation of them this one is.
/// </summary>
/// <remarks>
///     Generation is not biology and does not belong on a gene of its own. It is a record of
///     when the Contract made this kandra, and since every kandra has one it rides along with
///     being a kandra at all.
///     <para>
///         Kandra rank themselves by how far back they were made. First generation are the
///         oldest things walking on Scadrial and they know it; tenth generation take orders.
///         The number is rolled once and stored, because it is the one fact about a kandra
///         that cannot change.
///     </para>
/// </remarks>
public class KandraHeritage : Verse.Gene {
    public const int Earliest = 1;
    public const int Latest = 10;

    private int generation;

    /// <summary>First generation, down to tenth. Lower is older and harder to push around.</summary>
    public int Generation {
        get {
            if (generation == 0) Roll();

            return generation;
        }
    }

    /// <summary>1.0 for the first generation, tailing to 0.0 for the tenth.</summary>
    public float Seniority => 1f - ((Generation - Earliest) / (float)(Latest - Earliest));

    public override void PostAdd() {
        base.PostAdd();
        Roll();
        Apply();
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref generation, "kandraGeneration");
    }

    public override string LabelCap => base.LabelCap + " (" + Generation + ")";

    private void Roll() {
        if (generation != 0) return;

        // Highest of two rolls, which skews late. The Contract made far more tenth-generation
        // kandra than firsts, and an even roll would put an ancient in every colony.
        generation = Mathf.Max(
            Rand.RangeInclusive(Earliest, Latest),
            Rand.RangeInclusive(Earliest, Latest)
        );
    }

    /// <summary>
    ///     Older kandra carry more of everything they have had centuries to practise.
    /// </summary>
    private void Apply() {
        if (pawn.skills?.skills == null) return;

        int bonus = Mathf.RoundToInt(Seniority * 6f);
        if (bonus <= 0) return;

        for (int i = 0; i < pawn.skills.skills.Count; i++) {
            SkillRecord skill = pawn.skills.skills[i];
            if (skill.TotallyDisabled) continue;

            skill.Level = Mathf.Min(20, skill.Level + bonus);
        }
    }
}
