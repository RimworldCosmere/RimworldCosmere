using Concord;
using Cosmere.System.Scadrial.Util;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Patch.Rendering;

/// <summary>
///     Gives a koloss a body its own size instead of a person's scaled up.
/// </summary>
/// <remarks>
///     Past a camera zoom of 18 RimWorld stops drawing humanlikes and blits a picture baked into a
///     shared texture atlas, and that tile is cut for a person. Scaling the transform therefore
///     worked at close range and cropped the koloss at the knees the moment the player zoomed out,
///     which reads as a camera bug rather than a rendering one. Forcing the dynamic path is not
///     available: the decision lives in a private method whose return type is inaccessible.
///     <para>
///         bodyWidth is what HumanlikeMeshPoolUtility reads to size the mesh, and therefore what
///         decides the tile, so a bigger life stage fixes the crop at its source. The same def's
///         bodySizeFactor feeds Pawn.BodySize, which is why this also carries hunger, health scale
///         and how much cover the thing needs.
///     </para>
///     <para>
///         CurLifeStage is read on every render frame and by half the simulation besides, so the
///         answer is cached and the non-koloss case costs one dictionary miss and a null.
///     </para>
/// </remarks>
[Patch]
public abstract class KolossBodyPatch : Pawn_AgeTracker {
    [InjectField("pawn")]
    private readonly Pawn trackedPawn = null!;

    protected KolossBodyPatch(Pawn newPawn) : base(newPawn) { }

    [Inject(At.Return, nameof(CurLifeStage))]
    private void AfterCurLifeStage(ControlHandle<LifeStageDef> ch) {
        LifeStageDef? body = KolossBulk.CachedBodyFor(trackedPawn);
        if (body != null) ch.ReturnValue = body;
    }
}
