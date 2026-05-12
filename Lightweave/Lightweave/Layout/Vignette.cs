using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;

namespace Cosmere.Lightweave.Layout;

[Doc(
    Id = "vignette",
    Summary = "Decorative vignette overlay - darkens corners or edges to create mood and focus.",
    WhenToUse = "Frame a hero panel, dim the edges of a card, fade out content along one edge.",
    SourcePath = "Lightweave/Lightweave/Layout/Vignette.cs"
)]
public static class Vignette {
    public static LightweaveNode Create(
        [DocParam("Child painted underneath the vignette.")]
        LightweaveNode child,
        [DocParam("Falloff shape: Radial corners-dark, Frame inner-glow, Linear edge fade.")]
        VignetteShape shape = VignetteShape.Radial,
        [DocParam("Edge to fade from when shape is Linear.")]
        VignetteEdge edge = VignetteEdge.Bottom,
        [DocParam("0-1 alpha multiplier applied to the vignette texture.")]
        float intensity = 0.6f,
        [DocParam("Coverage multiplier. 1 = default, >1 = darker (wider dark band), <1 = lighter (thinner dark band).")]
        float scale = 1f,
        [DocParam("Vignette color. Defaults to SurfaceSunken slot.", TypeOverride = "ColorRef?", DefaultOverride = "null")]
        ColorRef? color = null,
        [DocParam("Inline style override.", TypeOverride = "Style?", DefaultOverride = "null")]
        Style? style = null,
        [DocParam("Additional class names merged after the base 'vignette' class.", TypeOverride = "string[]?", DefaultOverride = "null")]
        string[]? classes = null,
        [DocParam("Stable id for state-style lookup.", TypeOverride = "string?", DefaultOverride = "null")]
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New("Vignette", line, file);
        node.ApplyStyling("vignette", style, classes, id);
        node.Children.Add(child);

        node.MeasureWidth = () => child.MeasureWidth?.Invoke() ?? 0f;
        node.Measure = availableWidth => child.Measure?.Invoke(availableWidth) ?? child.PreferredHeight ?? 0f;

        node.Paint = (rect, paintChildren) => {
            if (Event.current.type == EventType.Repaint) {
                Color tint = ResolveColor(color, node);
                tint.a *= Mathf.Clamp01(intensity);
                Draw(rect, shape, edge, tint, scale);
            }
            if (child.IsInFlow()) {
                child.MeasuredRect = rect;
            }
            paintChildren();
        };

        return node;
    }

    public static LightweaveNode Overlay(
        [DocParam("Falloff shape: Radial corners-dark, Frame inner-glow, Linear edge fade.")]
        VignetteShape shape = VignetteShape.Radial,
        [DocParam("Edge to fade from when shape is Linear.")]
        VignetteEdge edge = VignetteEdge.Bottom,
        [DocParam("0-1 alpha multiplier applied to the vignette texture.")]
        float intensity = 0.6f,
        [DocParam("Coverage multiplier. 1 = default, >1 = darker (wider dark band), <1 = lighter (thinner dark band).")]
        float scale = 1f,
        [DocParam("Vignette color. Defaults to SurfaceSunken slot.", TypeOverride = "ColorRef?", DefaultOverride = "null")]
        ColorRef? color = null,
        [DocParam("Inline style override.", TypeOverride = "Style?", DefaultOverride = "null")]
        Style? style = null,
        [DocParam("Additional class names merged after the base 'vignette' class.", TypeOverride = "string[]?", DefaultOverride = "null")]
        string[]? classes = null,
        [DocParam("Stable id for state-style lookup.", TypeOverride = "string?", DefaultOverride = "null")]
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New("Vignette.Overlay", line, file);
        node.ApplyStyling("vignette", style, classes, id);

        node.Paint = (rect, _) => {
            if (Event.current.type != EventType.Repaint) {
                return;
            }
            Color tint = ResolveColor(color, node);
            tint.a *= Mathf.Clamp01(intensity);
            Draw(rect, shape, edge, tint, scale);
        };

        return node;
    }

    private static Color ResolveColor(ColorRef? cr, LightweaveNode node) {
        Theme.Theme theme = RenderContext.Current.Theme;
        return cr switch {
            ColorRef.Literal lit => lit.Value,
            ColorRef.Token tok => theme.GetColor(tok.Slot),
            _ => theme.GetColor(ThemeSlot.SurfaceSunken),
        };
    }

    private static void Draw(Rect rect, VignetteShape shape, VignetteEdge edge, Color tint, float scale = 1f) {
        float s = Mathf.Max(0.1f, scale);
        Texture2D tex = shape switch {
            VignetteShape.Frame => VignetteTextureCache.Frame(innerSize: Mathf.Clamp01(0.62f / s), softness: 0.18f),
            VignetteShape.Linear => VignetteTextureCache.Linear(edge, falloff: 1.2f / s),
            _ => VignetteTextureCache.Radial(falloff: 1.6f / s),
        };
        Color saved = GUI.color;
        GUI.color = tint;
        GUI.DrawTexture(RectSnap.Snap(rect), tex, ScaleMode.StretchToFill, true);
        GUI.color = saved;
    }

    [DocVariant("CL_Playground_Vignette_Radial")]
    public static DocSample DocsRadial() {
        return new DocSample(() => DocsViewport(VignetteShape.Radial, VignetteEdge.Bottom));
    }

    [DocVariant("CL_Playground_Vignette_Frame")]
    public static DocSample DocsFrame() {
        return new DocSample(() => DocsViewport(VignetteShape.Frame, VignetteEdge.Bottom));
    }

    [DocVariant("CL_Playground_Vignette_LinearBottom")]
    public static DocSample DocsLinearBottom() {
        return new DocSample(() => DocsViewport(VignetteShape.Linear, VignetteEdge.Bottom));
    }

    [DocVariant("CL_Playground_Vignette_LinearTop")]
    public static DocSample DocsLinearTop() {
        return new DocSample(() => DocsViewport(VignetteShape.Linear, VignetteEdge.Top));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => DocsViewport(VignetteShape.Radial, VignetteEdge.Bottom));
    }

    private static LightweaveNode DocsViewport(VignetteShape shape, VignetteEdge edge) {
        return Box.Create(
            c => c.Add(
                Vignette.Create(
                    Spacer.Fixed(new Rem(6f)),
                    shape: shape,
                    edge: edge,
                    intensity: 0.85f,
                    color: ThemeSlot.SurfaceSunken
                )
            ),
            style: new Style {
                Padding = EdgeInsets.All(SpacingScale.Xs),
                Background = BackgroundSpec.Of(ThemeSlot.SurfaceAccent),
                Radius = RadiusSpec.All(RadiusScale.Sm),
            }
        );
    }
}
