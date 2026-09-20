using Cosmere.System.Scadrial.Util;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Kandra;

/// <summary>
///     The eyes a kandra built into its own face.
/// </summary>
/// <remarks>
///     Their own node rather than part of the head texture, the same as the koloss eyes: the head
///     is multiplied by the body colour, so an iris baked into it comes out whatever the kandra
///     was carved from. The mask puts the left eye on red and the right on green, so one graphic
///     carries two stones.
/// </remarks>
public class PawnRenderNode_KandraEyes : PawnRenderNode_AttachmentHead {
    public PawnRenderNode_KandraEyes(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
        : base(pawn, props, tree) { }

    /// <summary>
    ///     CutoutComplex is the cutout shader that reports a _MaskTex. Under plain Cutout the mask
    ///     is ignored and both eyes silently take one colour.
    /// </summary>
    protected override UnityEngine.Shader DefaultShader => ShaderDatabase.CutoutComplex;

    /// <summary>
    ///     Builds the graphic off the form rather than the def's texPath, so the two irises can
    ///     take different stones. An undesigned kandra has no eye colour and draws nothing.
    /// </summary>
    /// <remarks>
    ///     Iris size is three sets of art, not a scale: a node takes its size from the def's props,
    ///     which every kandra shares, and scaling the quad walks the irises out of their sockets.
    /// </remarks>
    /// <summary>
    ///     The head's quad, not the hair's. <c>PawnRenderNode_AttachmentHead</c> hands back the hair
    ///     mesh, which narrows to 1.3 on the six vanilla Narrow head types, while the kandra head is
    ///     swapped onto <c>PawnRenderNode_Head</c> and stays 1.5 - the irises would slide inward off
    ///     their painted sockets on any narrow-crowned kandra.
    /// </summary>
    public override GraphicMeshSet MeshSetFor(Pawn pawn) {
        return HumanlikeMeshPoolUtility.GetHumanlikeHeadSetForPawn(pawn);
    }

    public override Graphic? GraphicFor(Pawn pawn) {
        KandraForm? form = FormFor(pawn);
        if (form?.eyeColourName == null) return null;

        UnityEngine.Shader shader = ShaderFor(pawn);
        if (shader == null) return null;

        Color left = IrisColorFor(form, form.eyeColourName);
        string? right = form.eyeColourTwoName;

        // The empty maskPath is the point: Graphic_Multi.Init then finds <path>_<dir>m itself.
        return GraphicDatabase.Get<Graphic_Multi>(
            KandraAppearance.EyeGraphicPathFor(pawn, form.irisSizeName),
            shader,
            Vector2.one,
            left,
            right == null || right == KandraAppearance.EyeColourNone ? left : IrisColorFor(form, right),
            null,
            string.Empty
        );
    }

    /// <summary>What one iris draws with. The glow node lifts this; the cutout takes it as it is.</summary>
    protected virtual Color IrisColorFor(KandraForm form, string? colourName) {
        return KandraAppearance.EyeDrawColorFor(colourName, form.eyeLightName);
    }

    /// <summary>The form the kandra is showing. A worn shape wins, the true body answers otherwise.</summary>
    protected static KandraForm? FormFor(Pawn? pawn) {
        CompKandraForms? forms = pawn?.TryGetComp<CompKandraForms>();
        if (forms == null) return null;

        return forms.Current ?? forms.TrueBody;
    }
}

/// <summary>
///     The bloom over the irises, on the same art one layer up.
/// </summary>
/// <remarks>
///     Two nodes rather than one because the iris colour cannot separate the three light steps on
///     its own: the lift clamps, and a pale stone like moonstone has 17 of 255 left to give. The
///     bloom carries that separation instead.
/// </remarks>
public class PawnRenderNode_KandraEyeGlow : PawnRenderNode_KandraEyes {
    public PawnRenderNode_KandraEyeGlow(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
        : base(pawn, props, tree) { }

    /// <summary>
    ///     MoteGlow adds light rather than cutting out, and declares no _MaskTex, so
    ///     <c>Graphic_Multi</c> never reads the _m files here and odd eyes bloom in the left eye's
    ///     colour. Ka accepted that on 2026-09-19.
    /// </summary>
    protected override UnityEngine.Shader DefaultShader => ShaderDatabase.MoteGlow;

    /// <summary>Nothing glows with the light out.</summary>
    public override Graphic? GraphicFor(Pawn pawn) {
        KandraForm? form = FormFor(pawn);
        if (form?.eyeLightName == null || form.eyeLightName == KandraAppearance.EyeLightOff) return null;

        return base.GraphicFor(pawn);
    }

    /// <summary>
    ///     The iris colour lifted by the light. Additive blending takes the lift out of alpha, not
    ///     the channels - dim, steady and burning come out 0.15, 0.4 and 0.75 apart, where the
    ///     channels would have clamped together on a pale stone.
    /// </summary>
    protected override Color IrisColorFor(KandraForm form, string? colourName) {
        Color iris = base.IrisColorFor(form, colourName);
        iris.a = KandraAppearance.EyeLightStrengthFor(form.eyeLightName) - 1f;

        return iris;
    }
}
