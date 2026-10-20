using Concord;
using Cosmere.System.Scadrial.Util;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Patch.Rendering;

/// <summary>
///     Draws a koloss body instead of a big person's, without giving koloss a body type of their
///     own - a new BodyTypeDef would leave every piece of apparel with no texture to resolve to.
/// </summary>
[Patch]
public abstract class KolossBodyGraphicPatch : PawnRenderNode_Body {
    protected KolossBodyGraphicPatch(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
        : base(pawn, props, tree) { }

    [Inject(At.Return, nameof(GraphicFor))]
    private void AfterGraphicFor(Pawn pawn, ControlHandle<Graphic?> ch) {
        // null means vanilla already bailed - a stump, a dessicated corpse - and has its own answer.
        if (ch.ReturnValue == null) return;

        string? path = KolossAppearance.BodyGraphicPathFor(pawn);
        if (path == null) return;

        ch.ReturnValue = GraphicDatabase.Get<Graphic_Multi>(
            path,
            ch.ReturnValue.Shader,
            Vector2.one,
            ch.ReturnValue.color
        );
    }
}
