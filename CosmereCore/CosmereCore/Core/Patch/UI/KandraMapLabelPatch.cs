using Concord;
using Cosmere.System.Scadrial.Kandra;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Patch.UI;

/// <summary>
///     Adds the impersonated name to the label under a kandra on the map.
/// </summary>
/// <remarks>
///     The pawn keeps its own name, so the colonist bar and every list still read the kandra you
///     recruited. Only the map label picks up the second name, because that is where you are
///     looking when you want to know who the colony thinks that is.
///     <para>
///         <c>GetPawnLabel</c> is private, and both the label draw and its width measurement go
///         through it. Patching it rather than the draw keeps the background box the right size
///         for the longer string.
///     </para>
/// </remarks>
[Patch(typeof(GenMapUI))]
public static class KandraMapLabelPatch {
    [Inject(At.Return, "GetPawnLabel")]
    private static void AfterGetPawnLabel(Pawn pawn, ControlHandle<string> ch) {
        if (pawn == null) return;

        CompKandraForms? forms = pawn.TryGetComp<CompKandraForms>();

        // An animal form's "name" is its species, so the suffix would read "Jenny (wolf)". The
        // point of the suffix is telling two impersonated colonists apart; a wolf needs no help.
        if (forms?.Current?.IsAnimal != false) return;

        string? worn = forms.WornName;
        if (string.IsNullOrEmpty(worn)) return;
        if (ch.ReturnValue == null) return;

        ch.ReturnValue = ch.ReturnValue + " (" + worn + ")";
    }
}
