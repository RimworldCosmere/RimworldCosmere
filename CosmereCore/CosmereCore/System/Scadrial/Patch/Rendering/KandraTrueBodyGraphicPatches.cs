using Concord;
using Cosmere.System.Scadrial.Util;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Patch.Rendering;

/// <summary>
///     Draws the kandra's own shape instead of the body it last wore.
/// </summary>
[Patch]
public abstract class KandraTrueBodyGraphicPatch : PawnRenderNode_Body {
    protected KandraTrueBodyGraphicPatch(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
        : base(pawn, props, tree) { }

    [Inject(At.Return, nameof(GraphicFor))]
    private void AfterGraphicFor(Pawn pawn, ControlHandle<Graphic?> ch) {
        // null means vanilla already bailed - a stump, a dessicated corpse - and has its own answer.
        if (ch.ReturnValue == null) return;

        string? path = KandraAppearance.BodyGraphicPathFor(pawn);
        if (path == null) return;

        // Shader comes off vanilla's graphic; the colour deliberately does not.
        ch.ReturnValue = GraphicDatabase.Get<Graphic_Multi>(
            path,
            ch.ReturnValue.Shader,
            Vector2.one,
            KandraAppearance.TrueBodyColorFor(pawn)
        );
    }
}

/// <summary>
///     The same swap for the head, so the face is not the one it stole either.
/// </summary>
[Patch]
public abstract class KandraTrueHeadGraphicPatch : PawnRenderNode_Head {
    protected KandraTrueHeadGraphicPatch(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
        : base(pawn, props, tree) { }

    [Inject(At.Return, nameof(GraphicFor))]
    private void AfterGraphicFor(Pawn pawn, ControlHandle<Graphic?> ch) {
        if (ch.ReturnValue == null) return;

        string? path = KandraAppearance.HeadGraphicPathFor(pawn);
        if (path == null) return;

        ch.ReturnValue = GraphicDatabase.Get<Graphic_Multi>(
            path,
            ch.ReturnValue.Shader,
            Vector2.one,
            KandraAppearance.TrueBodyColorFor(pawn)
        );
    }
}
