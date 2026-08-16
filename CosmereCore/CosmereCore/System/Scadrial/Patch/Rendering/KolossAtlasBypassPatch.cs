using System;
using System.Linq.Expressions;
using System.Reflection;
using Concord;
using Cosmere.System.Scadrial.Util;
using Verse;

namespace Cosmere.System.Scadrial.Patch.Rendering;

/// <summary>
///     Keeps a grown koloss off the shared pawn texture atlas, so it stops being cropped when the
///     camera pulls back.
/// </summary>
/// <remarks>
///     Past <c>ZoomRootSize > 18</c> the game stops drawing a humanlike and blits a picture baked
///     into a shared atlas at a fixed 128px per pawn, with the bake camera pinned to zoom 1. A pawn
///     drawn larger than a person is cropped there and then drawn at normal size anyway, so the crop
///     only shows up zoomed out and reads as a camera bug. Nothing on the pawn resizes that frame -
///     <c>PawnTextureAtlas.FrameSize</c> is a constant.
///     <para>
///         A mechanoid never hits this. The same condition requires <c>RaceProps.Humanlike</c>,
///         which is the whole reason a centipede draws at full size at every zoom. Pushing the zoom
///         threshold out of reach puts a grown koloss on that same dynamic path.
///     </para>
///     <para>
///         The cost is the atlas cache, for koloss only. Every mechanoid and animal on the map
///         already renders this way.
///     </para>
/// </remarks>
[Patch]
public abstract class KolossAtlasBypassPatch : PawnRenderer {
    private static readonly Func<PawnRenderer, Pawn?>? ReadPawn = BuildPawnReader();

    protected KolossAtlasBypassPatch(Pawn pawn) : base(pawn) { }

    /// <summary>
    ///     Resolves the private <c>PawnRenderer.pawn</c> field into a compiled getter, or null when
    ///     the field is gone.
    /// </summary>
    /// <remarks>
    ///     Compiled once rather than read through <see cref="FieldInfo" /> every call, because the
    ///     injection this feeds runs once per pawn per frame. Not unit tested: the build references
    ///     the metadata-only RimWorld assembly, which carries no private members to look up, so this
    ///     can only be checked against the real assembly at runtime. Hence the error below.
    /// </remarks>
    /// <returns>A getter for the field, or null when it cannot be found.</returns>
    public static Func<PawnRenderer, Pawn?>? BuildPawnReader() {
        FieldInfo? field = typeof(PawnRenderer).GetField(
            "pawn",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        if (field == null) {
            Cosmere.Core.Logger.Error(
                "PawnRenderer.pawn not found, so a koloss cannot be told apart from a colonist here. "
                + "Koloss will be cropped past camera zoom 18."
            );
            return null;
        }

        ParameterExpression renderer = Expression.Parameter(typeof(PawnRenderer), "renderer");
        return Expression.Lambda<Func<PawnRenderer, Pawn?>>(Expression.Field(renderer, field), renderer)
            .Compile();
    }

    // Raising the threshold rather than clearing useCached outright: the flag is assigned inside a
    // private method whose return type is private too, so it cannot be reached from here.
    [Inject("ParallelGetPreRenderResults", 18f, At.Constant)]
    private float SkipAtlasForGrownPawn(float minZoom) {
        Pawn? pawn = ReadPawn?.Invoke(this);
        return KolossBulk.For(pawn) > 1f ? float.PositiveInfinity : minZoom;
    }
}
