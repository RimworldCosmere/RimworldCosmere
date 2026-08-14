using System.Text;
using Concord;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Patch.Rendering;

/// <summary>
///     Draws a koloss at the size it has grown to, clothes and all.
/// </summary>
/// <remarks>
///     The gene used to hang a render node off the Body tag and give it this scaling. That node
///     had no graphic of its own, so scaling it scaled nothing, and a koloss was drawn at exactly
///     a colonist's size the whole time.
///     <para>
///         Patching the base worker instead catches every node on the pawn - body, head, hair and
///         each piece of apparel - because <c>PawnRenderNodeWorker_Apparel_Body</c> is the only
///         subclass that overrides <c>ScaleFor</c> and it calls base first. Scaling only the body
///         would have dressed a giant in a colonist's coat.
///     </para>
/// </remarks>
[Patch]
public abstract class KolossScalePatch : PawnRenderNodeWorker {
    [Inject(At.Return, nameof(ScaleFor))]
    private void AfterScaleFor(PawnRenderNode node, PawnDrawParms parms, ControlHandle<Vector3> ch) {
        float grown = KolossBulk.For(parms.pawn);
        if (grown != 1f) ch.ReturnValue *= grown;
    }
}

/// <summary>
///     Lets a koloss carry what its shoulders say it should.
/// </summary>
/// <remarks>
///     Carried mass is <c>BodySize * 35</c> and nothing else - no stat, no gene, no hediff enters
///     into it. The gene's eightfold CarryingCapacity governs how big a stack a pawn hauls, which
///     is a different number, so a koloss still filled up at a colonist's 35kg.
/// </remarks>
[Patch(typeof(MassUtility))]
public static class KolossMassPatch {
    [Inject(At.Return, nameof(MassUtility.Capacity))]
    private static void AfterCapacity(Pawn p, StringBuilder? explanation, ControlHandle<float> ch) {
        if (ch.ReturnValue <= 0f) return;

        float grown = KolossBulk.For(p);
        if (grown == 1f) return;

        ch.ReturnValue *= grown * KolossBulk.HaulingBuild;
    }
}
