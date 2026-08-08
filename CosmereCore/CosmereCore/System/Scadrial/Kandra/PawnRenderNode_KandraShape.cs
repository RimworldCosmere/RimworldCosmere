using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Kandra;

/// <summary>Where a borrowed shape gets its graphic from.</summary>
public class KandraShapeGraphic : DefModExtension {
    public string texPath = string.Empty;
    public float drawSize = 1.4f;
    public ShaderTypeDef? shader;
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

    public override Graphic? GraphicFor(Pawn pawn) {
        KandraShapeGraphic? shape = pawn.def.GetModExtension<KandraShapeGraphic>();
        if (shape == null || string.IsNullOrEmpty(shape.texPath)) return null;

        UnityEngine.Shader shader = shape.shader?.Shader ?? ShaderDatabase.Cutout;
        Vector2 size = new Vector2(shape.drawSize, shape.drawSize);

        Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(
            shape.texPath,
            shader,
            size,
            Color.white
        );

        // Wounds and rot still read through the hediff set, the same as any other body.
        Color colour = pawn.health.hediffSet.GetSkinColor(graphic.Color);
        return graphic.GetColoredVersion(graphic.Shader, colour, colour);
    }
}
