using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Kandra;

/// <summary>
///     Draws a borrowed animal body without the vetoes a human body carries.
/// </summary>
/// <remarks>
///     Deliberately not <c>PawnRenderNodeWorker_Body</c>. That one refuses to draw when
///     <c>PawnRenderFlags.NoBody</c> is set, which happens to any swimming pawn, and defers to
///     <c>bed_showSleeperBody</c> for a humanlike pawn in a bed. Vanilla survives both because the
///     head keeps drawing and real animals have swimming graphics; a shaped kandra has neither, so
///     inheriting from it would leave the pawn invisible in water and in some beds.
/// </remarks>
public class PawnRenderNodeWorker_KandraShape : PawnRenderNodeWorker {
    /// <summary>How much of the portrait window the largest edge of an animal may fill.</summary>
    private const float PortraitFill = 0.9f;

    /// <summary>
    ///     Shrinks big animals to fit the portrait, and leaves the map alone.
    /// </summary>
    /// <remarks>
    ///     The colonist bar and the character card draw from the same tree, cropped to roughly a
    ///     person. An elephant at drawSize 3 would show as a patch of grey hide, so the portrait
    ///     gets a scalar that fits the whole animal in the window. On the map it draws at its own
    ///     size, which is the entire point.
    /// </remarks>
    public override Vector3 ScaleFor(PawnRenderNode node, PawnDrawParms parms) {
        Vector3 scale = base.ScaleFor(node, parms);
        if (!parms.flags.FlagSet(PawnRenderFlags.Portrait)) return scale;

        Vector2 size = KandraShapeGraphicUtility.DrawSizeFor(parms.pawn);
        float largest = Mathf.Max(size.x, size.y);
        if (largest <= 0f) return scale;

        return scale * Mathf.Min(1f, PortraitFill / largest);
    }
}
