using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Kandra;

/// <summary>Where a borrowed shape gets its graphic from.</summary>
/// <remarks>
///     The animal's own <see cref="GraphicData" /> is carried whole rather than picked apart.
///     Rebuilding one from a texture path alone threw away the colour, the mask and the shader,
///     which is how a cougar ended up white.
/// </remarks>
public class KandraShapeGraphic : DefModExtension {
    public GraphicData? body;
    public GraphicData? female;
}

/// <summary>
///     Draws a humanlike-intelligence pawn as whatever animal it is wearing.
/// </summary>
/// <remarks>
///     The Animal render tree cannot do this. Every node in it reads
///     <c>Pawn_AgeTracker.CurKindLifeStage</c>, which returns null for a humanlike pawn on
///     purpose and logs an error saying so, and the node then dereferences it. That guard is
///     deliberate, so the tree is closed to us.
///     <para>
///         A kandra has to be humanlike to be drafted and given orders, so the graphic comes from
///         a def extension instead of from the pawnkind's life stages. Nothing here touches the
///         age tracker.
///     </para>
/// </remarks>
public class PawnRenderNode_KandraShape : PawnRenderNode {
    public PawnRenderNode_KandraShape(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
        : base(pawn, props, tree) { }

    /// <summary>
    ///     Draws on a quad the size of the animal, not the size of a person.
    /// </summary>
    /// <remarks>
    ///     The base returns <c>HumanlikeMeshPoolUtility.GetHumanlikeBodySetForPawn</c>, a fixed
    ///     1.5 by 1.5 human body quad, and the graphic's own drawSize is never consulted - the
    ///     mesh comes from the pool rather than from <c>Graphic.MeshAt</c>. So a bluebird authored
    ///     at 0.6 was being stretched over a human.
    /// </remarks>
    public override GraphicMeshSet MeshSetFor(Pawn pawn) {
        GraphicData? data = DataFor(pawn);
        if (data == null) return base.MeshSetFor(pawn);

        Vector2 size = data.drawSize;
        if (size.x <= 0f || size.y <= 0f) return base.MeshSetFor(pawn);

        return MeshPool.GetMeshSetForSize(size.x, size.y);
    }

    public override Graphic? GraphicFor(Pawn pawn) {
        GraphicData? data = DataFor(pawn);
        if (data == null || string.IsNullOrEmpty(data.texPath)) return null;

        // GraphicData.Graphic applies the colour, mask and shader the animal was authored with.
        Graphic graphic = data.Graphic;

        // Wounds and rot still read through the hediff set, the same as any other body.
        Color colour = pawn.health.hediffSet.GetSkinColor(graphic.Color);
        Color colourTwo = pawn.health.hediffSet.GetSkinColor(graphic.ColorTwo);

        return graphic.GetColoredVersion(graphic.Shader, colour, colourTwo);
    }

    private static GraphicData? DataFor(Pawn pawn) {
        KandraShapeGraphic? shape = pawn.def.GetModExtension<KandraShapeGraphic>();
        if (shape == null) return null;

        return pawn.gender == Gender.Female && shape.female != null ? shape.female : shape.body;
    }
}
