using System.Collections.Generic;
using Concord;
using Cosmere.System.Scadrial.Kandra;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Patch.Kandra;

/// <summary>
///     Takes a kandra's fists away while it is wearing something that has none.
/// </summary>
/// <remarks>
///     The borrowed animal tools arrive through a hediff comp, but the pawn keeps its own race
///     tools - left fist, right fist, teeth - and <c>VerbUtility</c> will not prune them, because a
///     human fist at power 8.2 is close enough to a wolf bite at power 12 to survive the
///     25%-of-best-DPS cut. So about half a shaped kandra's attacks read as punches while the thing
///     on screen is a dog, which is the exact tell the disguise exists to avoid.
///     <para>
///         Filtered on the way out rather than at the source. The pawn's tools live on its
///         <c>verbTracker</c>, which builds its verbs once and caches them, so emptying the list
///         afterwards does nothing and emptying it beforehand would need the tracker rebuilt on
///         every shape change. <c>IsUsableMeleeVerb</c> would be the natural seam but it is a local
///         function inside this method and cannot be patched.
///     </para>
/// </remarks>
[Patch]
public abstract class ShapedMeleeVerbsPatch : Pawn_MeleeVerbs {
    protected ShapedMeleeVerbsPatch(Pawn pawn)
        : base(pawn) { }

    [Inject(At.Return, nameof(GetUpdatedAvailableVerbsList))]
    private void AfterGetUpdatedAvailableVerbsList(bool terrainTools, ControlHandle<List<VerbEntry>> ch) {
        List<VerbEntry>? entries = ch.ReturnValue;
        if (entries == null || entries.Count == 0) return;

        // Pawn_MeleeVerbs.pawn is private, and every verb here is cast by the same pawn.
        if (entries[0].verb?.caster is not Verse.Pawn caster) return;
        if (KandraShapeGraphicUtility.WornKind(caster) == null) return;

        // hediff/weapon-lent verbs have a different owner; only the pawns own race tools hang directly off it
        int borrowed = 0;
        for (int i = 0; i < entries.Count; i++) {
            if (entries[i].verb?.DirectOwner is not Verse.Pawn) borrowed++;
        }

        // never strip the last verb: a pawn with none makes ChooseMeleeVerb log an error on every swing
        if (borrowed == 0) return;

        entries.RemoveAll(e => e.verb?.DirectOwner is Verse.Pawn);
    }
}
