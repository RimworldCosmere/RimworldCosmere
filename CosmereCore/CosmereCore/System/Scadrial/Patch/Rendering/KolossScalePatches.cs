using System.Text;
using Concord;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Patch.Rendering;

/// <summary>
///     Lets a koloss carry what its shoulders say it should.
/// </summary>
/// <remarks>
///     Carried mass is <c>BodySize * 35</c> and nothing else - no stat, no gene, no hediff enters
///     into it. The gene's eightfold CarryingCapacity governs how big a stack a pawn hauls, which
///     is a different number, so a koloss still filled up at a colonist's 35kg.
///     <para>
///         Growth is not applied here. The koloss life stages carry it through bodySizeFactor, so
///         multiplying by it again would count the same fact twice. What is left is the flat
///         difference between a body this size and a person's, which no amount of BodySize
///         expresses: a koloss is built to carry, not merely large.
///     </para>
/// </remarks>
[Patch(typeof(MassUtility))]
public static class KolossMassPatch {
    [Inject(At.Return, nameof(MassUtility.Capacity))]
    private static void AfterCapacity(Pawn p, StringBuilder? explanation, ControlHandle<float> ch) {
        if (ch.ReturnValue <= 0f) return;

        if (KolossBulk.GrowthOf(p) < 0f) return;

        ch.ReturnValue *= KolossBulk.HaulingBuild;
    }
}
