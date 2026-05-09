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
using static Cosmere.Lightweave.Typography.Typography;
using Text = Cosmere.Lightweave.Typography.Typography.Text;

namespace Cosmere.Lightweave.Layout;

[Doc(
    Id = "carousel",
    Summary = "Slide-paged carousel with optional arrows and dots.",
    WhenToUse = "Cycle through a small set of equally-weighted views.",
    SourcePath = "Lightweave/Lightweave/Layout/Carousel.cs",
    PreferredVariantHeight = 200f
)]
public static class Carousel {
    private const float SlideDurationSeconds = 0.28f;
    private static readonly float ControlZone = new Rem(2.5f).ToPixels();
    private static readonly float DotRadiusPx = new Rem(0.25f).ToPixels();
    private static readonly float DotGapPx = new Rem(0.5f).ToPixels();
    private static readonly float DotStripHeight = new Rem(1.125f).ToPixels();
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
        [DocParam("Enable arrow-key navigation while the carousel is hovered.")]
    bool keyboardEnabled = true,
        [DocParam("Override hover sound on arrows/dots. Null = component default (false).")]
    bool? playHoverSound = null,
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
            }
            else {
                frameRect = rect;
                dotRect = default;
            }

            BackgroundSpec frameBg = BackgroundSpec.Of(ThemeSlot.SurfaceSunken);
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

            bool soundEnabled = playHoverSound ?? false;

            if (showArrows && count > 1) {
                Rect leftZone = new Rect(frameRect.x, frameRect.y, ControlZone, frameRect.height);
                Rect rightZone = new Rect(frameRect.xMax - ControlZone, frameRect.y, ControlZone, frameRect.height);
                DrawArrow(leftZone, theme, true, clamped == 0);
                DrawArrow(rightZone, theme, false, clamped == count - 1);

                Cosmere.Lightweave.Input.InteractionFeedback.Apply(leftZone, clamped > 0, soundEnabled);
                Cosmere.Lightweave.Input.InteractionFeedback.Apply(rightZone, clamped < count - 1, soundEnabled);

                Event e = Event.current;
                if (e.type == EventType.MouseUp && e.button == 0) {
                    if (leftZone.Contains(e.mousePosition)) {
                        int next = rtl ? Mathf.Min(clamped + 1, count - 1) : Mathf.Max(0, clamped - 1);
                        if (next != clamped) {
                            onIndexChange?.Invoke(next);
                            e.Use();
                        }
                    }
                    else if (rightZone.Contains(e.mousePosition)) {
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

                    Cosmere.Lightweave.Input.InteractionFeedback.Apply(hitRect, !active, soundEnabled);
                    if (e.type == EventType.MouseUp && e.button == 0 && hitRect.Contains(e.mousePosition)) {
                        if (logical != clamped) {
                            onIndexChange?.Invoke(logical);
                            e.Use();
                        }
                    }
                }
            }

            if (keyboardEnabled && count > 1 && rect.Contains(Event.current.mousePosition)) {
                Event ke = Event.current;
                if (ke.type == EventType.KeyDown) {
                    if (ke.keyCode == KeyCode.LeftArrow) {
                        int next = rtl ? Mathf.Min(clamped + 1, count - 1) : Mathf.Max(0, clamped - 1);
                        if (next != clamped) {
                            onIndexChange?.Invoke(next);
                            ke.Use();
                        }
                    }
                    else if (ke.keyCode == KeyCode.RightArrow) {
                        int next = rtl ? Mathf.Max(0, clamped - 1) : Mathf.Min(clamped + 1, count - 1);
                        if (next != clamped) {
                            onIndexChange?.Invoke(next);
                            ke.Use();
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
            BackgroundSpec.Of(bg),
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

    [DocVariant("CL_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        StateHandle<int> index = UseState(0);
        return new DocSample(() => 
            Carousel.Create(
                new List<LightweaveNode> {
                DocsSlide(ThemeSlot.SurfaceRaised, "CL_Playground_carousel_Slide_First"),
                DocsSlide(ThemeSlot.SurfaceAccent, "CL_Playground_carousel_Slide_Second"),
                DocsSlide(ThemeSlot.SurfacePrimary, "CL_Playground_carousel_Slide_Third"),
                DocsSlide(ThemeSlot.SurfaceSunken, "CL_Playground_carousel_Slide_Fourth"),
                },
                index.Value,
                i => index.Set(i)
            )
        );
    }


    [DocVariant("CL_Playground_Label_Autoplay")]
    public static DocSample DocsAutoplay() {
        const float autoplaySeconds = 3f;
        StateHandle<int> index = UseState(0);
        RefHandle<float> lastTick = UseRef(Time.realtimeSinceStartup);

        List<LightweaveNode> slides = new List<LightweaveNode> {
            DocsSlide(ThemeSlot.SurfaceRaised, "CL_Playground_carousel_Slide_First"),
            DocsSlide(ThemeSlot.SurfaceAccent, "CL_Playground_carousel_Slide_Second"),
            DocsSlide(ThemeSlot.SurfacePrimary, "CL_Playground_carousel_Slide_Third"),
        };

        LightweaveNode carousel = Carousel.Create(slides, index.Value, i => index.Set(i));

        LightweaveNode wrapper = NodeBuilder.New("CarouselAutoplay");
        wrapper.Children.Add(carousel);
        wrapper.Measure = carousel.Measure;
        wrapper.PreferredHeight = carousel.PreferredHeight;
        wrapper.Paint = (rect, _) => {
            float now = Time.realtimeSinceStartup;
            if (Mouse.IsOver(rect)) {
                lastTick.Current = now;
            }
            else if (now - lastTick.Current >= autoplaySeconds) {
                int next = (index.Value + 1) % slides.Count;
                index.Set(next);
                lastTick.Current = now;
            }

            carousel.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(carousel, rect);
        };

        return new DocSample(() => wrapper, useFullSource: true);
    }

    [DocVariant("CL_Playground_Label_Keyboard")]
    public static DocSample DocsKeyboard() {
        StateHandle<int> index = UseState(0);

        List<LightweaveNode> slides = new List<LightweaveNode> {
            DocsSlide(ThemeSlot.SurfaceRaised, "CL_Playground_carousel_Slide_First"),
            DocsSlide(ThemeSlot.SurfaceAccent, "CL_Playground_carousel_Slide_Second"),
            DocsSlide(ThemeSlot.SurfacePrimary, "CL_Playground_carousel_Slide_Third"),
        };

        LightweaveNode carousel = Carousel.Create(
            slides,
            index.Value,
            i => index.Set(i),
            keyboardEnabled: false
        );

        LightweaveNode wrapper = NodeBuilder.New("CarouselKeyboard");
        wrapper.Children.Add(carousel);
        wrapper.Measure = carousel.Measure;
        wrapper.PreferredHeight = carousel.PreferredHeight;
        wrapper.Paint = (rect, _) => {
            UseHotkey.Use(KeyCode.LeftArrow, () => index.Set(Mathf.Max(0, index.Value - 1)));
            UseHotkey.Use(KeyCode.RightArrow, () => index.Set(Mathf.Min(slides.Count - 1, index.Value + 1)));

            carousel.MeasuredRect = rect;
            LightweaveRoot.PaintSubtree(carousel, rect);
        };

        return new DocSample(() => wrapper, useFullSource: true);
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        StateHandle<int> index = UseState(0);
        return new DocSample(() => 
            Carousel.Create(
                new List<LightweaveNode> {
                DocsSlide(ThemeSlot.SurfaceRaised, "CL_Playground_carousel_Slide_First"),
                DocsSlide(ThemeSlot.SurfaceAccent, "CL_Playground_carousel_Slide_Second"),
                },
                index.Value,
                i => index.Set(i)
            )
        );
    }
}
