using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Kandra;

/// <summary>
///     Must derive from the verb giver's own properties, not from HediffCompProperties.
/// </summary>
/// <remarks>
///     <c>HediffComp_VerbGiver.Props</c> is a hard cast to
///     <see cref="HediffCompProperties_VerbGiver" />, and <c>VerbProperties</c> reads through it.
///     Deriving from the wrong base throws <c>InvalidCastException</c> the first time anything asks
///     the pawn for a melee verb, which is every time it is attacked or told to attack.
/// </remarks>
public class HediffCompProperties_KandraShapeVerbs : HediffCompProperties_VerbGiver {
    public HediffCompProperties_KandraShapeVerbs() {
        compClass = typeof(HediffComp_KandraShapeVerbs);
    }
}

/// <summary>
///     Lends a kandra the teeth of whatever it is wearing.
/// </summary>
/// <remarks>
///     <c>HediffComp_VerbGiver</c> reads its tools from the def, which is no good here: there is
///     one hediff and a hundred and seventeen animals. Re-declaring <see cref="IVerbOwner" /> on
///     the subclass re-maps the interface to this <c>Tools</c> instead of the base's, so vanilla's
///     own <c>VerbTracker</c> builds verbs from the worn animal.
///     <para>
///         Every tool is re-linked to <c>Teeth</c> on the way through.
///         <c>VerbProperties.GetDamageFactorFor</c> returns 0 for a body part group the body does
///         not have, and <c>Verb.IsStillUsableBy</c> then drops the verb without a word - so a
///         wolf's paw attack on a human body would simply never happen.
///     </para>
/// </remarks>
public class HediffComp_KandraShapeVerbs : HediffComp_VerbGiver, IVerbOwner {
    private List<Tool>? borrowed;
    private PawnKindDef? borrowedFrom;

    /// <summary>The animal's attacks, re-anchored to a part the wearer actually has.</summary>
    public new List<Tool>? Tools {
        get {
            PawnKindDef? worn = KandraShapeGraphicUtility.WornKind(Pawn);
            if (worn == null) return base.Tools;

            if (borrowed != null && borrowedFrom == worn) return borrowed;

            borrowedFrom = worn;
            borrowed = Borrow(worn);

            return borrowed;
        }
    }

    /// <summary>
    ///     Throws away verbs built for the last shape.
    /// </summary>
    /// <remarks>
    ///     <c>VerbTracker</c> builds its list once and caches it, so without this a kandra keeps
    ///     biting like whatever it wore first.
    /// </remarks>
    public override void CompPostTick(ref float severityAdjustment) {
        PawnKindDef? worn = KandraShapeGraphicUtility.WornKind(Pawn);
        if (worn != borrowedFrom) {
            borrowedFrom = worn;
            borrowed = worn == null ? null : Borrow(worn);
            verbTracker = new VerbTracker(this);
        }

        base.CompPostTick(ref severityAdjustment);
    }

    /// <summary>Teeth is the one attack group both a human and a quadruped body carry.</summary>
    private static BodyPartGroupDef? Bite =>
        bite ??= DefDatabase<BodyPartGroupDef>.GetNamedSilentFail("Teeth");

    private static BodyPartGroupDef? bite;

    private static List<Tool> Borrow(PawnKindDef worn) {
        List<Tool> theirs = worn.race?.tools ?? [];
        List<Tool> ours = [];

        for (int i = 0; i < theirs.Count; i++) {
            Tool source = theirs[i];
            ours.Add(new Tool {
                label = source.label,
                capacities = source.capacities,
                power = source.power,
                cooldownTime = source.cooldownTime,
                armorPenetration = source.armorPenetration,
                chanceFactor = source.chanceFactor,
                alwaysTreatAsWeapon = source.alwaysTreatAsWeapon,

                // The one field that must not be copied. A human body has no paws.
                linkedBodyPartsGroup = Bite,
                ensureLinkedBodyPartsGroupAlwaysUsable = true,
                id = "KandraShape" + i,
            });
        }

        return ours;
    }
}
