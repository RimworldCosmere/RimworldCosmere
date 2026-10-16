using Concord;
using Cosmere.Core.Util;
using Verse;

namespace Cosmere.Core.Patch;

/// <summary>
///     Takes the per-pawn world draw before any shard's xenotype patch answers.
/// </summary>
/// <remarks>
///     At.Head, so it runs ahead of every At.Return injection on the same method no matter what
///     order Concord composes those in. That ordering is the whole point: the shards read one
///     answer instead of each rolling their own.
/// </remarks>
[Patch(typeof(PawnGenerator))]
public static class XenotypeArbiterPatch {
    [Inject(At.Head, nameof(PawnGenerator.GetXenotypeForGeneratedPawn))]
    private static void BeforeGetXenotypeForGeneratedPawn() {
        XenotypeArbiter.Draw();
    }
}
