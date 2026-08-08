using System.Collections.Generic;
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

    /// <summary>How often the body closes itself up. Six times a second is not the point; visible is.</summary>
    private const int HealInterval = 60;

    /// <summary>Severity healed across all wounds per interval.</summary>
    private const float HealPerInterval = 0.6f;

    /// <summary>Ticks of being whole before a lost part is rebuilt.</summary>
    private const int RegrowInterval = 2500;

    /// <summary>
    ///     Closes every wound at once, and eventually rebuilds what was cut off.
    /// </summary>
    /// <remarks>
    ///     Vanilla's own healing runs once every 600 ticks and picks a single wound at random, so
    ///     a stat multiplier alone is nearly invisible on a badly hurt pawn. The fast path that
    ///     does what this needs is <c>HediffStage.regeneration</c>, and that is wrapped in
    ///     <c>ModsConfig.AnomalyActive</c>, so it would quietly do nothing for most players.
    ///     Hence doing it here.
    /// </remarks>
    public override void TickInterval(int delta) {
        base.TickInterval(delta);

        if (pawn.Dead) return;

        if (pawn.IsHashIntervalTick(HealInterval, delta)) HealWounds();
        if (pawn.IsHashIntervalTick(RegrowInterval, delta)) RegrowOnePart();
    }

    private void HealWounds() {
        List<Hediff_Injury> injuries = [];
        pawn.health?.hediffSet?.GetHediffs(ref injuries, injury => injury.CanHealNaturally());
        if (injuries.Count == 0) return;

        // Spread the budget over everything rather than picking one, which is the whole
        // difference between this and the vanilla trickle.
        float each = HealPerInterval / injuries.Count;
        for (int i = 0; i < injuries.Count; i++) injuries[i].Heal(each);
    }

    /// <summary>
    ///     Rebuilds one missing part, worst first, as a nearly destroyed one rather than a fresh
    ///     one. It grows back over the following ticks like any other wound.
    /// </summary>
    private void RegrowOnePart() {
        HediffSet? set = pawn.health?.hediffSet;
        if (set == null) return;

        List<Hediff_MissingPart> missing = [];
        set.GetHediffs(
            ref missing,
            part => part.Part?.parent != null
                    && set.GetFirstHediffMatchingPart<Hediff_MissingPart>(part.Part.parent) == null
                    && set.GetFirstHediffMatchingPart<Hediff_AddedPart>(part.Part.parent) == null
        );
        if (missing.Count == 0) return;

        Hediff_MissingPart lost = missing[0];
        BodyPartRecord part = lost.Part;

        pawn.health!.RemoveHediff(lost);

        Hediff rebuilt = pawn.health.AddHediff(RimWorld.HediffDefOf.Misc, part);
        float health = set.GetPartHealth(part);
        rebuilt.Severity = Mathf.Max(health - 1f, health * 0.9f);

        Messages.Message(
            "CS_Kandra_Regrew".Translate(
                pawn.LabelShortCap.Named("PAWN"),
                part.Label.Named("PART")
            ),
            pawn,
            MessageTypeDefOf.PositiveEvent,
            false
        );
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
