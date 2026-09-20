using Cosmere.System.Scadrial.Util;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Kandra;

/// <summary>Which of the two eyes a kandra eye node draws. The def picks it per entry with a li Class.</summary>
public class PawnRenderNodeProperties_KandraEye : PawnRenderNodeProperties {
    /// <summary>Left unless an entry says otherwise, so a plain li still draws a whole eye.</summary>
    public bool rightEye;
}

/// <summary>One of the eyes a kandra built into its own face.</summary>
/// <remarks>Its own node, like the koloss eyes: the head is multiplied by the body colour.</remarks>
public class PawnRenderNode_KandraEyes : PawnRenderNode_AttachmentHead {
    public PawnRenderNode_KandraEyes(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
        : base(pawn, props, tree) { }

    /// <summary>Which eye this node is. A base-typed properties entry answers left.</summary>
    protected bool RightEye => Props is PawnRenderNodeProperties_KandraEye { rightEye: true };

    /// <summary>The head's quad. PawnRenderNode_AttachmentHead's hair mesh narrows to 1.3 on Narrow heads.</summary>
    /// <remarks>The kandra head stays 1.5, so the iris would slide inward off its painted socket.</remarks>
    public override GraphicMeshSet MeshSetFor(Pawn pawn) {
        return HumanlikeMeshPoolUtility.GetHumanlikeHeadSetForPawn(pawn);
    }

    /// <summary>The west eye draws on the east quad, because the west art is already mirrored.</summary>
    /// <remarks>The head set flips its west quad, which would mirror the iris back off its socket.</remarks>
    public override Mesh GetMesh(PawnDrawParms parms) {
        if (parms.facing == Rot4.West) parms.facing = Rot4.East;

        return base.GetMesh(parms);
    }

    /// <summary>The art and colour for this node's eye. An undesigned kandra draws nothing.</summary>
    /// <remarks>Iris size is two sets of art, not a scale - scaling walks the iris out of its socket.</remarks>
    public override Graphic? GraphicFor(Pawn pawn) {
        KandraForm? form = FormFor(pawn);
        if (form?.eyeColourName == null) return null;

        UnityEngine.Shader shader = ShaderFor(pawn);
        if (shader == null) return null;

        return GraphicDatabase.Get<Graphic_Multi>(
            KandraAppearance.EyeGraphicPathFor(pawn, form.irisSizeName, RightEye),
            shader,
            Vector2.one,
            IrisColorFor(form, StoneFor(form))
        );
    }

    /// <summary>The stone this node's eye is cut from.</summary>
    /// <remarks>A second colour never picked, or picked as none, means the eyes match.</remarks>
    protected string? StoneFor(KandraForm form) {
        if (!RightEye) return form.eyeColourName;

        string? second = form.eyeColourTwoName;

        return second == null || second == KandraAppearance.EyeColourNone ? form.eyeColourName : second;
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

/// <summary>The bloom over one iris, on the same art one layer up.</summary>
/// <remarks>The lift clamps on a pale stone like moonstone, so the bloom carries the separation.</remarks>
public class PawnRenderNode_KandraEyeGlow : PawnRenderNode_KandraEyes {
    public PawnRenderNode_KandraEyeGlow(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
        : base(pawn, props, tree) { }

    /// <summary>MoteGlow adds light rather than cutting out, which is what makes a bloom a bloom.</summary>
    protected override UnityEngine.Shader DefaultShader => ShaderDatabase.MoteGlow;

    /// <summary>Nothing glows with the light out.</summary>
    public override Graphic? GraphicFor(Pawn pawn) {
        KandraForm? form = FormFor(pawn);
        if (form?.eyeLightName == null || form.eyeLightName == KandraAppearance.EyeLightOff) return null;

        return base.GraphicFor(pawn);
    }

    /// <summary>The iris colour lifted by the light, out of alpha rather than the channels.</summary>
    /// <remarks>Dim, steady and burning come out 0.15, 0.4 and 0.75 apart instead of clamping together.</remarks>
    protected override Color IrisColorFor(KandraForm form, string? colourName) {
        Color iris = base.IrisColorFor(form, colourName);
        iris.a = KandraAppearance.EyeLightStrengthFor(form.eyeLightName) - 1f;

        return iris;
    }
}
