using UnityEngine;
using Cosmere.Core.UI.Lightweave.Runtime;
using Cosmere.Core.UI.Lightweave.Types;

namespace Cosmere.Core.UI.Lightweave.Rendering;

public static class PaintBox
{
    private static Texture2D? whiteTex;
    private static Texture2D WhiteTex => whiteTex ??= Texture2D.whiteTexture;

    public static void Draw(Rect rect, BackgroundSpec? bg, BorderSpec? border, RadiusSpec? radius)
    {
        Rect r = RectSnap.Snap(rect);
        Direction dir = RenderContext.Current.Direction;

        Vector4 rad = radius?.ResolveVector(dir) ?? Vector4.zero;
        Vector4 bw = border?.ResolveVector(dir) ?? Vector4.zero;

        if (bg is BackgroundSpec.Solid solid)
        {
            Color c = ResolveColor(solid.Color);
            GUI.DrawTexture(r, WhiteTex, ScaleMode.StretchToFill, alphaBlend: true, imageAspect: 0, color: c, borderWidths: bw, borderRadiuses: rad);
        }
        else if (bg is BackgroundSpec.Textured tex)
        {
            Color c = tex.Tint != null ? ResolveColor(tex.Tint) : Color.white;
            GUI.DrawTexture(r, tex.Texture, tex.Mode, alphaBlend: true, imageAspect: 0, color: c, borderWidths: bw, borderRadiuses: rad);
        }
        else if (bg is BackgroundSpec.Gradient grad)
        {
            Color c = grad.Tint != null ? ResolveColor(grad.Tint) : Color.white;
            GUI.DrawTexture(r, grad.GradientTex, ScaleMode.StretchToFill, alphaBlend: true, imageAspect: 0, color: c, borderWidths: bw, borderRadiuses: rad);
        }
        else if (border != null)
        {
            Color bc = border.Value.Color != null ? ResolveColor(border.Value.Color) : Color.white;
            GUI.DrawTexture(r, WhiteTex, ScaleMode.StretchToFill, alphaBlend: true, imageAspect: 0, color: new Color(0, 0, 0, 0), borderWidths: bw, borderRadiuses: rad);
            GUI.DrawTexture(r, WhiteTex, ScaleMode.StretchToFill, alphaBlend: true, imageAspect: 0, color: bc, borderWidths: bw, borderRadiuses: rad);
        }
    }

    private static Color ResolveColor(ColorRef cref) => cref switch
    {
        ColorRef.Literal l => l.Value,
        ColorRef.Token t => RenderContext.Current.Theme.GetColor(t.Slot),
        _ => Color.magenta,
    };
}
