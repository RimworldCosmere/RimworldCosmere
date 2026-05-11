using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Theme;
using Cosmere.Lightweave.Types;
using UnityEngine;
using static Cosmere.Lightweave.Typography.Typography;
using Verse;
using Text = Cosmere.Lightweave.Typography.Typography.Text;

namespace Cosmere.Lightweave.Tokens;

[Doc(
    Id = "breakpoints",
    Summary = "Tailwind-style viewport breakpoints (Xs through Xxl) resolved from the root paint width.",
    WhenToUse = "Read RenderContext.Current.Breakpoint or compose Responsive<T> values when layout should adapt across window widths.",
    SourcePath = "Lightweave/Lightweave/Tokens/Breakpoint.cs",
    Category = "Tokens",
    PreferredVariantHeight = 96f
)]
public static class BreakpointsDoc {
    private static readonly Breakpoint[] AllBreakpoints = {
        Breakpoint.Xs,
        Breakpoint.Sm,
        Breakpoint.Md,
        Breakpoint.Lg,
        Breakpoint.Xl,
        Breakpoint.Xxl,
    };

    [DocVariant("CL_Playground_breakpoints_Readout")]
    public static DocSample DocsReadout() {
        return new DocSample(() => 
            Box.Create(
                outer => outer.Add(
                    Box.Create(
                        inner => inner.Add(BreakpointReadoutNode()),
                        style: new Style {
                            Padding = EdgeInsets.Vertical(SpacingScale.Sm),
                            Background = BackgroundSpec.Of(ThemeSlot.SurfaceAccent),
                            Radius = RadiusSpec.All(RadiusScale.Sm),
                        }
                    )
                ),
                style: new Style {
                    Padding = EdgeInsets.All(new Rem(0.25f)),
                    Background = BackgroundSpec.Of(ThemeSlot.SurfaceSunken),
                    Border = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderSubtle),
                    Radius = RadiusSpec.All(RadiusScale.Sm),
                }
            )
        );
    }

    [DocVariant("CL_Playground_breakpoints_Ladder")]
    public static DocSample DocsLadder() {
        return new DocSample(() => 
            Box.Create(
                outer => outer.Add(BreakpointLadderNode()),
                style: new Style {
                    Padding = EdgeInsets.All(new Rem(0.25f)),
                    Background = BackgroundSpec.Of(ThemeSlot.SurfaceSunken),
                    Border = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderSubtle),
                    Radius = RadiusSpec.All(RadiusScale.Sm),
                }
            )
        );
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => 
            Box.Create(
                outer => outer.Add(
                    Box.Create(
                        inner => inner.Add(BreakpointReadoutNode()),
                        style: new Style {
                            Padding = EdgeInsets.Vertical(SpacingScale.Sm),
                            Background = BackgroundSpec.Of(ThemeSlot.SurfaceAccent),
                            Radius = RadiusSpec.All(RadiusScale.Sm),
                        }
                    )
                ),
                style: new Style {
                    Padding = EdgeInsets.All(new Rem(0.25f)),
                    Background = BackgroundSpec.Of(ThemeSlot.SurfaceSunken),
                    Border = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderSubtle),
                    Radius = RadiusSpec.All(RadiusScale.Sm),
                }
            )
        );
    }

    private static LightweaveNode BreakpointReadoutNode() {
        LightweaveNode node = NodeBuilder.New("BreakpointsReadout");
        node.PreferredHeight = new Rem(2f).ToPixels();
        node.Measure = _ => new Rem(2f).ToPixels();
        node.Paint = (rect, _) => {
            Breakpoint bp = RenderContext.Current.Breakpoint;
            float minPx = MinWidthForBreakpoint(bp);
            string key = BreakpointLabelKey(bp);
            string label = $"{(string)key.Translate()} ({Mathf.RoundToInt(minPx)}px+)";
            Theme.Theme theme = RenderContext.Current.Theme;
            int pixelSize = Mathf.RoundToInt(new Rem(1f).ToFontPx());
            GUIStyle style = GuiStyleCache.GetOrCreate(theme.GetFont(FontRole.BodyBold), pixelSize, FontStyle.Bold);
            style.alignment = TextAnchor.MiddleCenter;
            Color saved = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.TextPrimary);
            GUI.Label(RectSnap.Snap(rect), label, style);
            GUI.color = saved;
        };
        return node;
    }

    private static LightweaveNode BreakpointLadderNode() {
        LightweaveNode node = NodeBuilder.New("BreakpointsLadder");
        node.PreferredHeight = new Rem(1.5f).ToPixels();
        node.Measure = _ => new Rem(1.5f).ToPixels();
        node.Paint = (rect, _) => {
            Breakpoint current = RenderContext.Current.Breakpoint;
            Theme.Theme theme = RenderContext.Current.Theme;
            int count = AllBreakpoints.Length;
            float gap = new Rem(0.25f).ToPixels();
            float cellWidth = (rect.width - gap * (count - 1)) / count;
            int pixelSize = Mathf.RoundToInt(new Rem(0.8125f).ToFontPx());
            GUIStyle style = GuiStyleCache.GetOrCreate(theme.GetFont(FontRole.BodyBold), pixelSize, FontStyle.Bold);
            style.alignment = TextAnchor.MiddleCenter;
            RadiusSpec radius = RadiusSpec.All(RadiusScale.Sm);
            for (int i = 0; i < count; i++) {
                Breakpoint bp = AllBreakpoints[i];
                bool isActive = bp == current;
                ThemeSlot bgSlot = isActive ? ThemeSlot.SurfaceAccent : ThemeSlot.SurfaceRaised;
                ThemeSlot fgSlot = isActive ? ThemeSlot.TextOnAccent : ThemeSlot.TextMuted;
                Rect cell = new Rect(rect.x + i * (cellWidth + gap), rect.y, cellWidth, rect.height);
                PaintBox.Draw(cell, BackgroundSpec.Of(bgSlot), null, radius);
                Color saved = GUI.color;
                GUI.color = theme.GetColor(fgSlot);
                GUI.Label(RectSnap.Snap(cell), bp.ToString(), style);
                GUI.color = saved;
            }
        };
        return node;
    }

    private static float MinWidthForBreakpoint(Breakpoint bp) {
        return bp switch {
            Breakpoint.Sm => Breakpoints.SmMinPx,
            Breakpoint.Md => Breakpoints.MdMinPx,
            Breakpoint.Lg => Breakpoints.LgMinPx,
            Breakpoint.Xl => Breakpoints.XlMinPx,
            Breakpoint.Xxl => Breakpoints.XxlMinPx,
            _ => 0f,
        };
    }

    private static string BreakpointLabelKey(Breakpoint bp) {
        return bp switch {
            Breakpoint.Sm => "CL_Playground_Breakpoint_Sm",
            Breakpoint.Md => "CL_Playground_Breakpoint_Md",
            Breakpoint.Lg => "CL_Playground_Breakpoint_Lg",
            Breakpoint.Xl => "CL_Playground_Breakpoint_Xl",
            Breakpoint.Xxl => "CL_Playground_Breakpoint_Xxl",
            _ => "CL_Playground_Breakpoint_Xs",
        };
    }
}
