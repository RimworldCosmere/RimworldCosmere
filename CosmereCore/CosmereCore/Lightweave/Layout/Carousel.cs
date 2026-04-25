using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Hooks;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;
using Verse.Sound;
using static Cosmere.Lightweave.Hooks.Hooks;
using static Cosmere.Lightweave.Layout.Layout;
using static Cosmere.Lightweave.Typography.Typography;
using Text = Cosmere.Lightweave.Typography.Typography.Text;

namespace Cosmere.Lightweave.Layout;

[Doc(
    Id = "carousel",
    Summary = "Slide-paged carousel with optional arrows and dots.",
    WhenToUse = "Cycle through a small set of equally-weighted views.",
    SourcePath = "CosmereCore/CosmereCore/Lightweave/Layout/Carousel.cs",
    PreferredVariantHeight = 200f
)]
public static class Carousel {
    private const float SlideDurationSeconds = 0.28f;
    private const float ControlZone = 40f;
    private const float DotRadiusPx = 4f;
    private const float DotGapPx = 8f;
    private const float DotStripHeight = 18f;
    private static readonly Func<float, float> EaseOutCubic = t => 1f - Mathf.Pow(1f - t, 3f);

    public static LightweaveNode Create(
        [DocParam("Slide nodes shown one at a time.")]
        IReadOnlyList<LightweaveNode> slides,
        [DocParam("Index of the visible slide.")]
        int currentIndex,
        [DocParam("Callback invoked when the user changes slides.")]
        Action<int> onIndexChange,
        [DocParam("Show left/right arrow controls.")]
        bool showArrows = true,
        [DocParam("Show pagination dot strip.")]
        bool showDots = true,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New("Carousel", line, file);

        for (int i = 0; i < slides.Count; i++) {
            node.Children.Add(slides[i]);
        }

        node.Measure = availableWidth => {
            if (slides == null || slides.Count == 0) {
                return showDots ? DotStripHeight : 0f;
            }

            float maxSlideH = 0f;
            for (int i = 0; i < slides.Count; i++) {
                float h = slides[i].Measure?.Invoke(availableWidth) ?? slides[i].PreferredHeight ?? 0f;
                if (h > maxSlideH) {
                    maxSlideH = h;
                }
            }

            return maxSlideH + (showDots ? DotStripHeight : 0f);
        };

        node.Paint = (rect, _) => {
            if (slides == null || slides.Count == 0) {
                return;
            }

            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            bool rtl = dir == Direction.Rtl;

            int count = slides.Count;
            int clamped = Mathf.Clamp(currentIndex, 0, count - 1);

            float animIndex = UseAnim.Animate(
                clamped,
                SlideDurationSeconds,
                EaseOutCubic,
                line,
                file + "#carIndex"
            );

            Rect frameRect;
            Rect dotRect;
            if (showDots) {
                frameRect = new Rect(rect.x, rect.y, rect.width, Mathf.Max(0f, rect.height - DotStripHeight));
                dotRect = new Rect(rect.x, frameRect.yMax, rect.width, DotStripHeight);
            } else {
                frameRect = rect;
                dotRect = default;
            }

            BackgroundSpec frameBg = new BackgroundSpec.Solid(ThemeSlot.SurfaceSunken);
            BorderSpec frameBorder = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault);
            RadiusSpec frameRadius = RadiusSpec.All(new Rem(0.25f));
            PaintBox.Draw(frameRect, frameBg, frameBorder, frameRadius);

            GUI.BeginClip(frameRect);

            float slotWidth = frameRect.width;
            float baseX = -animIndex * slotWidth;
            if (rtl) {
                baseX = animIndex * slotWidth;
            }

            for (int i = 0; i < count; i++) {
                float slideX = rtl
                    ? baseX - i * slotWidth
                    : baseX + i * slotWidth;
                if (slideX + slotWidth < 0f || slideX > frameRect.width) {
                    continue;
                }

                Rect slideRect = new Rect(slideX, 0f, slotWidth, frameRect.height);
                slides[i].MeasuredRect = slideRect;
                LightweaveRoot.PaintSubtree(slides[i], slideRect);
            }

            GUI.EndClip();

            if (showArrows && count > 1) {
                Rect leftZone = new Rect(frameRect.x, frameRect.y, ControlZone, frameRect.height);
                Rect rightZone = new Rect(frameRect.xMax - ControlZone, frameRect.y, ControlZone, frameRect.height);
                DrawArrow(leftZone, theme, true, clamped == 0);
                DrawArrow(rightZone, theme, false, clamped == count - 1);

                Event e = Event.current;
                if (e.type == EventType.MouseUp && e.button == 0) {
                    if (leftZone.Contains(e.mousePosition)) {
                        int next = rtl ? Mathf.Min(clamped + 1, count - 1) : Mathf.Max(0, clamped - 1);
                        if (next != clamped) {
                            onIndexChange?.Invoke(next);
                            e.Use();
                        }
                    } else if (rightZone.Contains(e.mousePosition)) {
                        int next = rtl ? Mathf.Max(0, clamped - 1) : Mathf.Min(clamped + 1, count - 1);
                        if (next != clamped) {
                            onIndexChange?.Invoke(next);
                            e.Use();
                        }
                    }
                }
            }

            if (showDots && count > 1) {
                float totalDotsWidth = count * (DotRadiusPx * 2f) + (count - 1) * DotGapPx;
                float startX = dotRect.x + (dotRect.width - totalDotsWidth) / 2f;
                float dotY = dotRect.y + (dotRect.height - DotRadiusPx * 2f) / 2f;
                Color savedColor = GUI.color;
                Event e = Event.current;

                for (int i = 0; i < count; i++) {
                    int logical = rtl ? count - 1 - i : i;
                    float dotX = startX + i * (DotRadiusPx * 2f + DotGapPx);
                    Rect dot = new Rect(dotX, dotY, DotRadiusPx * 2f, DotRadiusPx * 2f);
                    Rect hitRect = new Rect(dot.x - DotGapPx / 2f, dotRect.y, dot.width + DotGapPx, dotRect.height);

                    bool active = logical == clamped;
                    Color col = active
                        ? theme.GetColor(ThemeSlot.SurfaceAccent)
                        : theme.GetColor(ThemeSlot.BorderSubtle);
                    GUI.color = col;
                    GUI.DrawTexture(RectSnap.Snap(dot), Texture2D.whiteTexture);
                    GUI.color = savedColor;

                    MouseoverSounds.DoRegion(hitRect);
                    if (e.type == EventType.MouseUp && e.button == 0 && hitRect.Contains(e.mousePosition)) {
                        if (logical != clamped) {
                            onIndexChange?.Invoke(logical);
                            e.Use();
                        }
                    }
                }
            }
        };

        return node;
    }

    private static void DrawArrow(Rect rect, Theme.Theme theme, bool pointLeft, bool dimmed) {
        Color saved = GUI.color;
        Color background = new Color(0f, 0f, 0f, dimmed ? 0.12f : 0.32f);
        GUI.color = background;
        GUI.DrawTexture(RectSnap.Snap(rect), Texture2D.whiteTexture);
        GUI.color = saved;

        Texture2D? arrowTex = pointLeft ? TexUI.ArrowTexLeft : TexUI.ArrowTexRight;
        if (arrowTex == null) {
            return;
        }

        float size = Mathf.Min(rect.width, rect.height) * 0.7f;
        Rect iconRect = new Rect(
            rect.x + (rect.width - size) * 0.5f,
            rect.y + (rect.height - size) * 0.5f,
            size,
            size
        );

        Color arrowColor = theme.GetColor(ThemeSlot.TextPrimary);
        if (dimmed) {
            arrowColor = new Color(arrowColor.r, arrowColor.g, arrowColor.b, 0.4f);
        }

        GUI.color = arrowColor;
        GUI.DrawTexture(RectSnap.Snap(iconRect), arrowTex);
        GUI.color = saved;
    }

    private static LightweaveNode DocsSlide(ThemeSlot bg, string labelKey) {
        return Box.Create(
            EdgeInsets.All(SpacingScale.Md),
            new BackgroundSpec.Solid(bg),
            null,
            null,
            c => c.Add(
                Text.Create(
                    (string)labelKey.Translate(),
                    FontRole.BodyBold,
                    new Rem(1f),
                    ThemeSlot.TextPrimary,
                    TextAlign.Center
                )
            )
        );
    }

    [DocVariant("CC_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        StateHandle<int> index = UseState(0);
        return new DocSample(
            Carousel.Create(
                new List<LightweaveNode> {
                    DocsSlide(ThemeSlot.SurfaceRaised, "CC_Playground_carousel_Slide_First"),
                    DocsSlide(ThemeSlot.SurfaceAccent, "CC_Playground_carousel_Slide_Second"),
                    DocsSlide(ThemeSlot.SurfacePrimary, "CC_Playground_carousel_Slide_Third"),
                    DocsSlide(ThemeSlot.SurfaceSunken, "CC_Playground_carousel_Slide_Fourth"),
                },
                index.Value,
                i => index.Set(i)
            )
        );
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        StateHandle<int> index = UseState(0);
        return new DocSample(
            Carousel.Create(
                new List<LightweaveNode> {
                    DocsSlide(ThemeSlot.SurfaceRaised, "CC_Playground_carousel_Slide_First"),
                    DocsSlide(ThemeSlot.SurfaceAccent, "CC_Playground_carousel_Slide_Second"),
                },
                index.Value,
                i => index.Set(i)
            )
        );
    }
}