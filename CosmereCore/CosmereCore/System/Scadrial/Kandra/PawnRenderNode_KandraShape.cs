using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Kandra;

/// <summary>Where a generated shape race got its graphic from. Dies with the generator.</summary>
/// <remarks>
///     Only the two-pawn path reads this. The one-pawn node takes its graphic from
///     <see cref="CompKandraForms" /> instead, because a shape is no longer its own ThingDef.
/// </remarks>
public class KandraShapeGraphic : DefModExtension {
    public GraphicData? body;
    public GraphicData? female;
}

/// <summary>
///     The picture a kandra is currently wearing, and how big it is.
/// </summary>
/// <remarks>
///     Read from <see cref="CompKandraForms" /> rather than from a def extension or the node's own
///     <c>hediff</c> back-reference. The node runs <c>meshSet = MeshSetFor(pawn)</c> in its
///     constructor, and <c>DynamicPawnRenderNodeSetup_Hediffs</c> only assigns
///     <c>node.hediff</c> after <c>Activator.CreateInstance</c> returns, so the hediff is null
///     exactly when the size is needed. The comp is not.
///     <para>
///         The consequence is an ordering rule everywhere else: the worn form must be set on the
///         comp <em>before</em> the hediff is added, never after.
///     </para>
/// </remarks>
public static class KandraShapeGraphicUtility {
    /// <summary>The animal a pawn is wearing, or null when it is being itself.</summary>
    public static PawnKindDef? WornKind(Pawn? pawn) {
        return pawn?.TryGetComp<CompKandraForms>()?.Current?.animalKind;
    }

    /// <summary>
    ///     The graphic data for the worn animal, picking the female variant when there is one.
    /// </summary>
    /// <remarks>
    ///     The whole <see cref="GraphicData" /> travels rather than a texture path. Rebuilding one
    ///     from a path alone drops the colour, the mask and the shader, which is how a cougar came
    ///     out white.
    ///     <para>
    ///         The comp is asked first and the def extension second, so both designs work off the
    ///         same node while the one-pawn spike is being proven. A generated shape race has no
    ///         <see cref="CompKandraForms" /> - that lives on the kandra held inside it - so it
    ///         falls through to the extension, and the spike is additive rather than a cutover.
    ///     </para>
    /// </remarks>
    public static GraphicData? DataFor(Pawn? pawn) {
        PawnKindDef? kind = WornKind(pawn);
        if (kind?.lifeStages is { Count: > 0 }) {
            PawnKindLifeStage stage = kind.lifeStages[^1];

            return pawn!.gender == Gender.Female && stage.femaleGraphicData != null
                ? stage.femaleGraphicData
                : stage.bodyGraphicData;
        }

        KandraShapeGraphic? legacy = pawn?.def.GetModExtension<KandraShapeGraphic>();
        if (legacy == null) return null;

        return pawn!.gender == Gender.Female && legacy.female != null ? legacy.female : legacy.body;
    }

    public static Vector2 DrawSizeFor(Pawn? pawn) {
        return DataFor(pawn)?.drawSize ?? Vector2.one;
    }
}

/// <summary>
///     Draws a kandra as whatever animal it is wearing, without changing what it is.
/// </summary>
/// <remarks>
///     The pawn keeps its own race, so it stays a colonist that can be drafted, keeps its skills,
///     its spikes, its Connection and its place in the colonist bar. Only the picture changes.
/// </remarks>
public class PawnRenderNode_KandraShape : PawnRenderNode {
    public PawnRenderNode_KandraShape(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
        : base(pawn, props, tree) { }

    /// <summary>
    ///     Draws on a quad the size of the animal, not the size of a person.
    /// </summary>
    /// <remarks>
    ///     The base returns <c>HumanlikeMeshPoolUtility.GetHumanlikeBodySetForPawn</c>, a fixed
    ///     1.5 by 1.5 human quad. The mesh comes from the pool rather than from
    ///     <c>Graphic.MeshAt</c>, so a graphic's own drawSize is never consulted and a bluebird
    ///     authored at 0.6 was stretched over a human.
    /// </remarks>
    public override GraphicMeshSet MeshSetFor(Pawn pawn) {
        Vector2 size = KandraShapeGraphicUtility.DrawSizeFor(pawn);
        if (size.x <= 0f || size.y <= 0f) return base.MeshSetFor(pawn);

        return MeshPool.GetMeshSetForSize(size.x, size.y);
    }

    public override Graphic? GraphicFor(Pawn pawn) {
        GraphicData? data = KandraShapeGraphicUtility.DataFor(pawn);
        if (data == null || string.IsNullOrEmpty(data.texPath)) return null;

        // GraphicData.Graphic applies the colour, mask and shader the animal was authored with.
        Graphic graphic = data.Graphic;

        // Wounds and rot still read through the hediff set, the same as any other body.
        Color colour = pawn.health.hediffSet.GetSkinColor(graphic.Color);
        Color colourTwo = pawn.health.hediffSet.GetSkinColor(graphic.ColorTwo);

        return graphic.GetColoredVersion(graphic.Shader, colour, colourTwo);
    }
}
