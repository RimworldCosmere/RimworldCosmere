using System;
using System.Collections.Generic;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Hooks;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;
using static Cosmere.Lightweave.Hooks.Hooks;
using Cosmere.Lightweave.Layout;
using static Cosmere.Lightweave.Doc.DocChips;
using Caption = Cosmere.Lightweave.Typography.Typography.Caption;
using Code = Cosmere.Lightweave.Typography.Typography.Code;
using Heading = Cosmere.Lightweave.Typography.Typography.Heading;
using Icon = Cosmere.Lightweave.Typography.Typography.Icon;
using Label = Cosmere.Lightweave.Typography.Typography.Label;
using RichText = Cosmere.Lightweave.Typography.Typography.RichText;
using Text = Cosmere.Lightweave.Typography.Typography.Text;

namespace Cosmere.Lightweave.Playground;

[Doc(
    Id = "usestate",
    Summary = "Hook that preserves a value across renders for the current node.",
    WhenToUse = "Track local UI state - draft inputs, toggles, charge meters - inside a render function.",
    SourcePath = "Lightweave/Lightweave/Hooks/Hooks.cs",
    Target = typeof(Cosmere.Lightweave.Hooks.Hooks),
    TargetMember = nameof(Cosmere.Lightweave.Hooks.Hooks.UseState),
    ShowRtl = false
)]
public static class UseStateDoc {
    [DocVariant("CC_Playground_Hook_UseState_Stormlight")]
    public static DocSample DocsStormlight() {
        return new DocSample(BuildStormlightDoc, useFullSource: true);
    }

    [DocVariant("CC_Playground_Hook_UseState_Toggle")]
    public static DocSample DocsToggle() {
        return new DocSample(BuildToggleDoc, useFullSource: true);
    }

    [DocVariant("CC_Playground_Hook_UseState_Draft")]
    public static DocSample DocsDraft() {
        return new DocSample(BuildDraftDoc, useFullSource: true);
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(BuildStormlightDoc, useFullSource: true);
    }

    private static LightweaveNode BuildStormlightDoc() {
        StateHandle<int> charge = UseState(3);

        LightweaveNode label = Text.Create(
            ((string)"CC_Playground_Hook_UseState_StormlightLabel".Translate())
                .Replace("{COUNT}", charge.Value.ToString()),
            FontRole.BodyBold,
            new Rem(1f),
            ThemeSlot.TextPrimary,
            TextAlign.Center,
            FontStyle.Bold
        );

        return HStack.Create(
            SpacingScale.Xs,
            r => {
                r.Add(
                    Button.Create(
                        (string)"CC_Playground_Hook_UseState_Drain".Translate(),
                        () => charge.Set(Mathf.Max(0, charge.Value - 1)),
                        ButtonVariant.Secondary
                    ),
                    72f
                );
                r.AddFlex(label);
                r.Add(
                    Button.Create(
                        (string)"CC_Playground_Hook_UseState_Infuse".Translate(),
                        () => charge.Set(charge.Value + 1)
                    ),
                    72f
                );
            }
        );
    }

    private static LightweaveNode BuildToggleDoc() {
        StateHandle<bool> drawn = UseState(false);

        return HStack.Create(
            SpacingScale.Xs,
            r => {
                r.AddFlex(Text.Create(
                    drawn.Value
                        ? (string)"CC_Playground_Hook_UseState_BladeDrawn".Translate()
                        : (string)"CC_Playground_Hook_UseState_BladeDismissed".Translate(),
                    FontRole.Body,
                    new Rem(1f),
                    drawn.Value ? ThemeSlot.SurfaceAccent : ThemeSlot.TextSecondary,
                    TextAlign.Left
                ));
                r.Add(
                    Button.Create(
                        drawn.Value
                            ? (string)"CC_Playground_Hook_UseState_Dismiss".Translate()
                            : (string)"CC_Playground_Hook_UseState_Summon".Translate(),
                        () => drawn.Set(!drawn.Value)
                    ),
                    96f
                );
            }
        );
    }

    private static LightweaveNode BuildDraftDoc() {
        StateHandle<string> oath = UseState(string.Empty);

        return Stack.Create(
            SpacingScale.Xxs,
            s => {
                s.Add(Caption.Create(
                    (string)"CC_Playground_Hook_UseState_OathHint".Translate()
                ), 14f);
                s.Add(TextField.Create(
                    oath.Value,
                    v => oath.Set(v),
                    (string)"CC_Playground_Hook_UseState_OathPlaceholder".Translate()
                ), 28f);
            }
        );
    }
}

[Doc(
    Id = "useanim",
    Summary = "Hook that smoothly animates a float toward a target value each frame.",
    WhenToUse = "Drive transitions for opacity, scale, or color blends without manual tweening.",
    SourcePath = "Lightweave/Lightweave/Hooks/Hooks.cs",
    ShowRtl = false,
    PreferredVariantHeight = 120f
)]
public static class UseAnimDoc {
    public static DocSample DocsFade() {
        return new DocSample(BuildFadeDoc, useFullSource: true);
    }

    [DocVariant("CC_Playground_Hook_UseAnim_Scale", Order = 1)]
    public static DocSample DocsScale() {
        return new DocSample(() => {
            StateHandle<bool> big = UseState(false);
            float scale = UseAnim.Animate(big.Value ? 1.5f : 1f, 0.3f, t => 1f - Mathf.Pow(1f - t, 3f));

            LightweaveNode demo = NodeBuilder.New("UseAnimScale", 0, nameof(UseAnimDoc));
            demo.Paint = (rect, _) => {
                Color saved = GUI.color;
                Color accent = RenderContext.Current.Theme.GetColor(ThemeSlot.SurfaceAccent);
                float side = Mathf.Min(rect.width, rect.height) * 0.4f * scale;
                Rect square = new Rect(
                    rect.x + (rect.width - side) * 0.5f,
                    rect.y + (rect.height - side) * 0.5f,
                    side,
                    side
                );
                GUI.color = accent;
                GUI.DrawTexture(square, Texture2D.whiteTexture);
                GUI.color = saved;
            };

            return HStack.Create(
                SpacingScale.Xs,
                r => {
                    r.Add(
                        Button.Create(
                            (string)"CC_Playground_UseAnim_Toggle_Scale".Translate(),
                            () => big.Set(!big.Value),
                            ButtonVariant.Secondary
                        ),
                        120f
                    );
                    r.AddFlex(demo);
                }
            );
        });
    }

    [DocVariant("CC_Playground_Hook_UseAnim_Slide", Order = 2)]
    public static DocSample DocsSlide() {
        return new DocSample(() => {
            StateHandle<bool> right = UseState(false);
            float offset = UseAnim.Animate(right.Value ? 1f : 0f, 0.4f, t => 1f - Mathf.Pow(1f - t, 3f));

            LightweaveNode demo = NodeBuilder.New("UseAnimSlide", 0, nameof(UseAnimDoc));
            demo.Paint = (rect, _) => {
                Color saved = GUI.color;
                Color accent = RenderContext.Current.Theme.GetColor(ThemeSlot.SurfaceAccent);
                float side = Mathf.Min(rect.height * 0.6f, 32f);
                float travel = Mathf.Max(0f, rect.width - side);
                Rect square = new Rect(
                    rect.x + travel * offset,
                    rect.y + (rect.height - side) * 0.5f,
                    side,
                    side
                );
                GUI.color = accent;
                GUI.DrawTexture(square, Texture2D.whiteTexture);
                GUI.color = saved;
            };

            return HStack.Create(
                SpacingScale.Xs,
                r => {
                    r.Add(
                        Button.Create(
                            (string)"CC_Playground_UseAnim_Toggle_Slide".Translate(),
                            () => right.Set(!right.Value),
                            ButtonVariant.Secondary
                        ),
                        120f
                    );
                    r.AddFlex(demo);
                }
            );
        });
    }

    [DocVariant("CC_Playground_Hook_UseAnim_Color", Order = 3)]
    public static DocSample DocsColor() {
        return new DocSample(() => {
            StateHandle<bool> alt = UseState(false);
            Color from = new Color(0.95f, 0.78f, 0.17f, 1f);
            Color to = new Color(0.55f, 0.27f, 0.78f, 1f);
            float r = UseAnim.Animate(alt.Value ? to.r : from.r, 0.5f);
            float g = UseAnim.Animate(alt.Value ? to.g : from.g, 0.5f);
            float b = UseAnim.Animate(alt.Value ? to.b : from.b, 0.5f);

            LightweaveNode demo = NodeBuilder.New("UseAnimColor", 0, nameof(UseAnimDoc));
            demo.Paint = (rect, _) => {
                Color saved = GUI.color;
                GUI.color = new Color(r, g, b, 1f);
                GUI.DrawTexture(rect, Texture2D.whiteTexture);
                GUI.color = saved;
            };

            return HStack.Create(
                SpacingScale.Xs,
                row => {
                    row.Add(
                        Button.Create(
                            (string)"CC_Playground_UseAnim_Toggle_Color".Translate(),
                            () => alt.Set(!alt.Value),
                            ButtonVariant.Secondary
                        ),
                        120f
                    );
                    row.AddFlex(demo);
                }
            );
        });
    }

    [DocVariant("CC_Playground_Hook_UseAnim_Stagger", Order = 4)]
    public static DocSample DocsStagger() {
        return new DocSample(() => {
            StateHandle<bool> visible = UseState(false);
            float a0 = UseAnim.Animate(visible.Value ? 1f : 0f, 0.3f);
            float a1 = UseAnim.Animate(visible.Value ? 1f : 0f, 0.4f);
            float a2 = UseAnim.Animate(visible.Value ? 1f : 0f, 0.5f);
            float a3 = UseAnim.Animate(visible.Value ? 1f : 0f, 0.6f);
            float a4 = UseAnim.Animate(visible.Value ? 1f : 0f, 0.7f);
            float[] alphas = { a0, a1, a2, a3, a4 };

            LightweaveNode demo = NodeBuilder.New("UseAnimStagger", 0, nameof(UseAnimDoc));
            demo.Paint = (rect, _) => {
                Color saved = GUI.color;
                Color accent = RenderContext.Current.Theme.GetColor(ThemeSlot.SurfaceAccent);
                float rowH = rect.height / 5f;
                for (int i = 0; i < 5; i++) {
                    Rect bar = new Rect(rect.x, rect.y + rowH * i + 2f, rect.width * alphas[i], rowH - 4f);
                    GUI.color = new Color(accent.r, accent.g, accent.b, accent.a * alphas[i]);
                    GUI.DrawTexture(bar, Texture2D.whiteTexture);
                }
                GUI.color = saved;
            };

            return HStack.Create(
                SpacingScale.Xs,
                row => {
                    row.Add(
                        Button.Create(
                            (string)"CC_Playground_UseAnim_Toggle_Stagger".Translate(),
                            () => visible.Set(!visible.Value),
                            ButtonVariant.Secondary
                        ),
                        120f
                    );
                    row.AddFlex(demo);
                }
            );
        });
    }

    [DocVariant("CC_Playground_Hook_UseAnim_Blur", Order = 5)]
    public static DocSample DocsBlur() {
        return new DocSample(() => {
            StateHandle<bool> blurred = UseState(false);
            float blur = UseAnim.Animate(blurred.Value ? 1f : 0f, 0.4f);

            LightweaveNode demo = NodeBuilder.New("UseAnimBlur", 0, nameof(UseAnimDoc));
            demo.Paint = (rect, _) => {
                Color saved = GUI.color;
                Color accent = RenderContext.Current.Theme.GetColor(ThemeSlot.SurfaceAccent);
                float alpha = Mathf.Lerp(1f, 0.3f, blur);
                float scale = Mathf.Lerp(1f, 0.85f, blur);
                float gray = blur * 0.5f;
                float side = Mathf.Min(rect.width, rect.height) * 0.5f * scale;
                Rect square = new Rect(
                    rect.x + (rect.width - side) * 0.5f,
                    rect.y + (rect.height - side) * 0.5f,
                    side,
                    side
                );
                Color tint = new Color(
                    Mathf.Lerp(accent.r, gray, blur),
                    Mathf.Lerp(accent.g, gray, blur),
                    Mathf.Lerp(accent.b, gray, blur),
                    accent.a * alpha
                );
                GUI.color = tint;
                GUI.DrawTexture(square, Texture2D.whiteTexture);
                GUI.color = saved;
            };

            return HStack.Create(
                SpacingScale.Xs,
                row => {
                    row.Add(
                        Button.Create(
                            (string)"CC_Playground_UseAnim_Toggle_Blur".Translate(),
                            () => blurred.Set(!blurred.Value),
                            ButtonVariant.Secondary
                        ),
                        120f
                    );
                    row.AddFlex(demo);
                }
            );
        });
    }

    [DocVariant("CC_Playground_Hook_UseAnim_Expand", Order = 6)]
    public static DocSample DocsExpand() {
        return new DocSample(() => {
            StateHandle<bool> wide = UseState(false);
            float widthFactor = UseAnim.Animate(wide.Value ? 1f : 0.2f, 0.4f, t => 1f - Mathf.Pow(1f - t, 3f));

            LightweaveNode demo = NodeBuilder.New("UseAnimExpand", 0, nameof(UseAnimDoc));
            demo.Paint = (rect, _) => {
                Color saved = GUI.color;
                Color accent = RenderContext.Current.Theme.GetColor(ThemeSlot.SurfaceAccent);
                float h = Mathf.Min(rect.height * 0.6f, 32f);
                Rect bar = new Rect(rect.x, rect.y + (rect.height - h) * 0.5f, rect.width * widthFactor, h);
                GUI.color = accent;
                GUI.DrawTexture(bar, Texture2D.whiteTexture);
                GUI.color = saved;
            };

            return HStack.Create(
                SpacingScale.Xs,
                row => {
                    row.Add(
                        Button.Create(
                            (string)"CC_Playground_UseAnim_Toggle_Expand".Translate(),
                            () => wide.Set(!wide.Value),
                            ButtonVariant.Secondary
                        ),
                        120f
                    );
                    row.AddFlex(demo);
                }
            );
        });
    }

    [DocVariant("CC_Playground_Hook_UseAnim_Shrink", Order = 7)]
    public static DocSample DocsShrink() {
        return new DocSample(() => {
            StateHandle<bool> small = UseState(false);
            float widthFactor = UseAnim.Animate(small.Value ? 0.2f : 1f, 0.4f, t => 1f - Mathf.Pow(1f - t, 3f));

            LightweaveNode demo = NodeBuilder.New("UseAnimShrink", 0, nameof(UseAnimDoc));
            demo.Paint = (rect, _) => {
                Color saved = GUI.color;
                Color accent = RenderContext.Current.Theme.GetColor(ThemeSlot.SurfaceAccent);
                float h = Mathf.Min(rect.height * 0.6f, 32f);
                Rect bar = new Rect(rect.x, rect.y + (rect.height - h) * 0.5f, rect.width * widthFactor, h);
                GUI.color = accent;
                GUI.DrawTexture(bar, Texture2D.whiteTexture);
                GUI.color = saved;
            };

            return HStack.Create(
                SpacingScale.Xs,
                row => {
                    row.Add(
                        Button.Create(
                            (string)"CC_Playground_UseAnim_Toggle_Shrink".Translate(),
                            () => small.Set(!small.Value),
                            ButtonVariant.Secondary
                        ),
                        120f
                    );
                    row.AddFlex(demo);
                }
            );
        });
    }

    [DocVariant("CC_Playground_Hook_UseAnim_Shake", Order = 8)]
    public static DocSample DocsShake() {
        return new DocSample(() => {
            RefHandle<float> shakeStart = UseRef(-1f);

            LightweaveNode demo = NodeBuilder.New("UseAnimShake", 0, nameof(UseAnimDoc));
            demo.Paint = (rect, _) => {
                Color saved = GUI.color;
                Color accent = RenderContext.Current.Theme.GetColor(ThemeSlot.SurfaceAccent);
                float elapsed = shakeStart.Current >= 0f ? Time.unscaledTime - shakeStart.Current : 999f;
                float duration = 0.6f;
                float jitter = 0f;
                if (elapsed < duration) {
                    float decay = 1f - elapsed / duration;
                    jitter = Mathf.Sin(elapsed * 30f) * 12f * decay;
                    AnimationClock.RegisterActive(RenderContext.Current.RootId);
                }
                float side = Mathf.Min(rect.width, rect.height) * 0.45f;
                Rect square = new Rect(
                    rect.x + (rect.width - side) * 0.5f + jitter,
                    rect.y + (rect.height - side) * 0.5f,
                    side,
                    side
                );
                GUI.color = accent;
                GUI.DrawTexture(square, Texture2D.whiteTexture);
                GUI.color = saved;
            };

            return HStack.Create(
                SpacingScale.Xs,
                row => {
                    row.Add(
                        Button.Create(
                            (string)"CC_Playground_UseAnim_Toggle_Shake".Translate(),
                            () => shakeStart.Current = Time.unscaledTime,
                            ButtonVariant.Secondary
                        ),
                        120f
                    );
                    row.AddFlex(demo);
                }
            );
        });
    }

    [DocVariant("CC_Playground_Hook_UseAnim_Bounce", Order = 9)]
    public static DocSample DocsBounce() {
        return new DocSample(() => {
            StateHandle<bool> up = UseState(false);
            float lift = UseAnim.Animate(up.Value ? 1f : 0f, 0.6f, t => {
                if (t < 1f / 2.75f) {
                    return 7.5625f * t * t;
                }
                if (t < 2f / 2.75f) {
                    t -= 1.5f / 2.75f;
                    return 7.5625f * t * t + 0.75f;
                }
                if (t < 2.5f / 2.75f) {
                    t -= 2.25f / 2.75f;
                    return 7.5625f * t * t + 0.9375f;
                }
                t -= 2.625f / 2.75f;
                return 7.5625f * t * t + 0.984375f;
            });

            LightweaveNode demo = NodeBuilder.New("UseAnimBounce", 0, nameof(UseAnimDoc));
            demo.Paint = (rect, _) => {
                Color saved = GUI.color;
                Color accent = RenderContext.Current.Theme.GetColor(ThemeSlot.SurfaceAccent);
                float side = Mathf.Min(rect.width, rect.height) * 0.35f;
                float travel = Mathf.Max(0f, rect.height - side - 8f);
                Rect square = new Rect(
                    rect.x + (rect.width - side) * 0.5f,
                    rect.y + rect.height - side - 4f - travel * lift,
                    side,
                    side
                );
                GUI.color = accent;
                GUI.DrawTexture(square, Texture2D.whiteTexture);
                GUI.color = saved;
            };

            return HStack.Create(
                SpacingScale.Xs,
                row => {
                    row.Add(
                        Button.Create(
                            (string)"CC_Playground_UseAnim_Toggle_Bounce".Translate(),
                            () => up.Set(!up.Value),
                            ButtonVariant.Secondary
                        ),
                        120f
                    );
                    row.AddFlex(demo);
                }
            );
        });
    }

    [DocVariant("CC_Playground_Hook_UseAnim_Pulse", Order = 10)]
    public static DocSample DocsPulse() {
        return new DocSample(() => {
            float wave = UseAnim.Wave(period: 1.6f, easing: t => t * t * (3f - 2f * t));
            float scale = Mathf.Lerp(0.95f, 1.18f, wave);
            float alpha = Mathf.Lerp(0.6f, 1f, wave);

            LightweaveNode demo = NodeBuilder.New("UseAnimPulse", 0, nameof(UseAnimDoc));
            demo.Paint = (rect, _) => {
                Color saved = GUI.color;
                Color accent = RenderContext.Current.Theme.GetColor(ThemeSlot.SurfaceAccent);
                float side = Mathf.Min(rect.width, rect.height) * 0.4f * scale;
                Rect square = new Rect(
                    rect.x + (rect.width - side) * 0.5f,
                    rect.y + (rect.height - side) * 0.5f,
                    side,
                    side
                );
                GUI.color = new Color(accent.r, accent.g, accent.b, accent.a * alpha);
                GUI.DrawTexture(square, Texture2D.whiteTexture);
                GUI.color = saved;
            };

            return demo;
        });
    }

    [DocVariant("CC_Playground_Hook_UseAnim_Wobble", Order = 11)]
    public static DocSample DocsWobble() {
        return new DocSample(() => {
            StateHandle<bool> active = UseState(false);
            RefHandle<float> startTime = UseRef(-1f);

            LightweaveNode demo = NodeBuilder.New("UseAnimWobble", 0, nameof(UseAnimDoc));
            demo.Paint = (rect, _) => {
                Color saved = GUI.color;
                Matrix4x4 savedMatrix = GUI.matrix;
                Color accent = RenderContext.Current.Theme.GetColor(ThemeSlot.SurfaceAccent);
                float side = Mathf.Min(rect.width, rect.height) * 0.4f;
                Rect square = new Rect(
                    rect.x + (rect.width - side) * 0.5f,
                    rect.y + (rect.height - side) * 0.5f,
                    side,
                    side
                );

                float angle = 0f;
                if (active.Value && startTime.Current >= 0f) {
                    float elapsed = Time.unscaledTime - startTime.Current;
                    float duration = 1.2f;
                    if (elapsed < duration) {
                        float decay = 1f - elapsed / duration;
                        angle = Mathf.Sin(elapsed * 6f) * 12f * decay;
                        AnimationClock.RegisterActive(RenderContext.Current.RootId);
                    }
                }

                Vector2 pivot = new Vector2(square.x + square.width * 0.5f, square.y + square.height * 0.5f);
                GUIUtility.RotateAroundPivot(angle, pivot);
                GUI.color = accent;
                GUI.DrawTexture(square, Texture2D.whiteTexture);
                GUI.matrix = savedMatrix;
                GUI.color = saved;
            };

            return HStack.Create(
                SpacingScale.Xs,
                row => {
                    row.Add(
                        Button.Create(
                            (string)"CC_Playground_UseAnim_Toggle_Wobble".Translate(),
                            () => {
                                active.Set(true);
                                startTime.Current = Time.unscaledTime;
                            },
                            ButtonVariant.Secondary
                        ),
                        120f
                    );
                    row.AddFlex(demo);
                }
            );
        });
    }

    [DocVariant("CC_Playground_Hook_UseAnim_Seesaw", Order = 12)]
    public static DocSample DocsSeesaw() {
        return new DocSample(() => {
            LightweaveNode demo = NodeBuilder.New("UseAnimSeesaw", 0, nameof(UseAnimDoc));
            demo.Paint = (rect, _) => {
                AnimationClock.RegisterActive(RenderContext.Current.RootId);
                Color saved = GUI.color;
                Matrix4x4 savedMatrix = GUI.matrix;
                Color accent = RenderContext.Current.Theme.GetColor(ThemeSlot.SurfaceAccent);
                float side = Mathf.Min(rect.width, rect.height) * 0.4f;
                Rect square = new Rect(
                    rect.x + (rect.width - side) * 0.5f,
                    rect.y + (rect.height - side) * 0.5f,
                    side,
                    side
                );
                float angle = Mathf.Sin(Time.unscaledTime * 1.2f) * 18f;
                Vector2 pivot = new Vector2(square.x + square.width * 0.5f, square.y + square.height * 0.5f);
                GUIUtility.RotateAroundPivot(angle, pivot);
                GUI.color = accent;
                GUI.DrawTexture(square, Texture2D.whiteTexture);
                GUI.matrix = savedMatrix;
                GUI.color = saved;
            };
            return demo;
        });
    }

    [DocVariant("CC_Playground_Hook_UseAnim_Spin", Order = 13)]
    public static DocSample DocsSpin() {
        return new DocSample(() => {
            LightweaveNode demo = NodeBuilder.New("UseAnimSpin", 0, nameof(UseAnimDoc));
            demo.Paint = (rect, _) => {
                AnimationClock.RegisterActive(RenderContext.Current.RootId);
                Color saved = GUI.color;
                Matrix4x4 savedMatrix = GUI.matrix;
                Color accent = RenderContext.Current.Theme.GetColor(ThemeSlot.SurfaceAccent);
                float side = Mathf.Min(rect.width, rect.height) * 0.4f;
                Rect square = new Rect(
                    rect.x + (rect.width - side) * 0.5f,
                    rect.y + (rect.height - side) * 0.5f,
                    side,
                    side
                );
                float angle = Time.unscaledTime % 2f / 2f * 360f;
                Vector2 pivot = new Vector2(square.x + square.width * 0.5f, square.y + square.height * 0.5f);
                GUIUtility.RotateAroundPivot(angle, pivot);
                GUI.color = accent;
                GUI.DrawTexture(square, Texture2D.whiteTexture);
                GUI.matrix = savedMatrix;
                GUI.color = saved;
            };
            return demo;
        });
    }

    [DocVariant("CC_Playground_Hook_UseAnim_Blink", Order = 14)]
    public static DocSample DocsBlink() {
        return new DocSample(() => {
            LightweaveNode demo = NodeBuilder.New("UseAnimBlink", 0, nameof(UseAnimDoc));
            demo.Paint = (rect, _) => {
                AnimationClock.RegisterActive(RenderContext.Current.RootId);
                Color saved = GUI.color;
                Color accent = RenderContext.Current.Theme.GetColor(ThemeSlot.SurfaceAccent);
                bool on = Mathf.FloorToInt(Time.unscaledTime / 0.5f) % 2 == 0;
                float alpha = on ? 1f : 0.2f;
                float side = Mathf.Min(rect.width, rect.height) * 0.4f;
                Rect square = new Rect(
                    rect.x + (rect.width - side) * 0.5f,
                    rect.y + (rect.height - side) * 0.5f,
                    side,
                    side
                );
                GUI.color = new Color(accent.r, accent.g, accent.b, accent.a * alpha);
                GUI.DrawTexture(square, Texture2D.whiteTexture);
                GUI.color = saved;
            };
            return demo;
        });
    }

    public static DocSample DocsUsage() {
        return new DocSample(BuildFadeDoc, useFullSource: true);
    }

    private static LightweaveNode BuildFadeDoc() {
        StateHandle<bool> target = UseState(false);

        LightweaveNode fadeNode = NodeBuilder.New("UseAnimFade", 0, nameof(UseAnimDoc));
        fadeNode.Paint = (rect, _) => {
            float t = UseAnim.Animate(target.Value ? 1f : 0f, 0.35f);
            Color saved = GUI.color;
            Color accent = RenderContext.Current.Theme.GetColor(ThemeSlot.SurfaceAccent);
            GUI.color = new Color(accent.r, accent.g, accent.b, accent.a * t);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = saved;
        };

        return HStack.Create(
            SpacingScale.Xs,
            r => {
                r.Add(
                    Button.Create(
                        (string)"CC_Playground_UseAnim_Toggle".Translate(),
                        () => target.Set(!target.Value),
                        ButtonVariant.Secondary
                    ),
                    120f
                );
                r.AddFlex(fadeNode);
            }
        );
    }
}

[Doc(
    Id = "usefocus",
    Summary = "Hook that exposes a focus handle for routing keyboard focus to an input.",
    WhenToUse = "Programmatically focus a TextField when a button is pressed, a panel opens, or after a value clears.",
    SourcePath = "Lightweave/Lightweave/Hooks/UseFocus.cs",
    Target = typeof(UseFocus),
    TargetMember = nameof(UseFocus.Use),
    ShowRtl = false
)]
public static class UseFocusDoc {
    [DocVariant("CC_Playground_Hook_UseFocus_OnDemand")]
    public static DocSample DocsOnDemand() {
        return new DocSample(BuildOnDemandDoc, useFullSource: true);
    }

    [DocVariant("CC_Playground_Hook_UseFocus_ClearAndRefocus")]
    public static DocSample DocsClearAndRefocus() {
        return new DocSample(BuildClearAndRefocusDoc, useFullSource: true);
    }

    [DocVariant("CC_Playground_Hook_UseFocus_Status")]
    public static DocSample DocsStatus() {
        return new DocSample(BuildStatusDoc, useFullSource: true);
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(BuildOnDemandDoc, useFullSource: true);
    }

    private static LightweaveNode BuildOnDemandDoc() {
        StateHandle<string> code = UseState(string.Empty);
        UseFocus.FocusHandle focus = UseFocus.Use();

        return HStack.Create(
            SpacingScale.Xs,
            r => {
                r.Add(
                    Button.Create(
                        (string)"CC_Playground_Hook_UseFocus_FocusSphere".Translate(),
                        focus.Request,
                        ButtonVariant.Secondary
                    ),
                    140f
                );
                r.AddFlex(TextField.Create(
                    code.Value,
                    v => code.Set(v),
                    (string)"CC_Playground_Hook_UseFocus_SpherePlaceholder".Translate(),
                    focus: focus
                ));
            }
        );
    }

    private static LightweaveNode BuildClearAndRefocusDoc() {
        StateHandle<string> entry = UseState(string.Empty);
        UseFocus.FocusHandle focus = UseFocus.Use();

        return HStack.Create(
            SpacingScale.Xs,
            r => {
                r.AddFlex(TextField.Create(
                    entry.Value,
                    v => entry.Set(v),
                    (string)"CC_Playground_Hook_UseFocus_GlyphPlaceholder".Translate(),
                    focus: focus
                ));
                r.Add(
                    Button.Create(
                        (string)"CC_Playground_Hook_UseFocus_Erase".Translate(),
                        () => {
                            entry.Set(string.Empty);
                            focus.Request();
                        },
                        ButtonVariant.Secondary
                    ),
                    96f
                );
            }
        );
    }

    private static LightweaveNode BuildStatusDoc() {
        StateHandle<string> note = UseState(string.Empty);
        UseFocus.FocusHandle focus = UseFocus.Use();

        return Stack.Create(
            SpacingScale.Xxs,
            s => {
                s.Add(Caption.Create(
                    focus.IsFocused
                        ? (string)"CC_Playground_Hook_UseFocus_StatusListening".Translate()
                        : (string)"CC_Playground_Hook_UseFocus_StatusIdle".Translate()
                ), 14f);
                s.Add(TextField.Create(
                    note.Value,
                    v => note.Set(v),
                    (string)"CC_Playground_Hook_UseFocus_NotePlaceholder".Translate(),
                    focus: focus
                ), 28f);
            }
        );
    }
}

[Doc(
    Id = "usehotkey",
    Summary = "Hook that binds a key chord to a callback while a node is mounted.",
    WhenToUse = "Add Escape-to-close, Ctrl+S, or other shortcuts scoped to a specific surface.",
    SourcePath = "Lightweave/Lightweave/Hooks/UseHotkey.cs",
    Target = typeof(UseHotkey),
    TargetMember = nameof(UseHotkey.Use),
    ShowRtl = false
)]
public static class UseHotkeyDoc {
    [DocVariant("CC_Playground_Hook_UseHotkey_Single")]
    public static DocSample DocsSingle() {
        return new DocSample(BuildSingleDoc, useFullSource: true);
    }

    [DocVariant("CC_Playground_Hook_UseHotkey_Modifiers")]
    public static DocSample DocsModifiers() {
        return new DocSample(BuildModifiersDoc, useFullSource: true);
    }

    [DocVariant("CC_Playground_Hook_UseHotkey_Bond")]
    public static DocSample DocsBond() {
        return new DocSample(BuildBondDoc, useFullSource: true);
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(BuildSingleDoc, useFullSource: true);
    }

    private static LightweaveNode BuildSingleDoc() {
        StateHandle<string> status = UseState<string>(
            (string)"CC_Playground_Hook_UseHotkey_SingleIdle".Translate()
        );

        LightweaveNode host = NodeBuilder.New("UseHotkeySingle", 0, nameof(UseHotkeyDoc));
        host.Paint = (rect, _) => {
            UseHotkey.Use(
                KeyCode.Escape,
                () => status.Set((string)"CC_Playground_Hook_UseHotkey_SingleEscape".Translate())
            );
            DrawHotkeyLabel(rect, status.Value);
        };

        return Stack.Create(
            SpacingScale.Xxs,
            s => {
                s.Add(Caption.Create(
                    (string)"CC_Playground_Hook_UseHotkey_SingleHint".Translate()
                ), 14f);
                s.Add(host, 18f);
            }
        );
    }

    private static LightweaveNode BuildModifiersDoc() {
        StateHandle<string> status = UseState<string>(
            (string)"CC_Playground_Hook_UseHotkey_ModifiersIdle".Translate()
        );

        LightweaveNode host = NodeBuilder.New("UseHotkeyMods", 0, nameof(UseHotkeyDoc));
        host.Paint = (rect, _) => {
            UseHotkey.Use(
                KeyCode.S,
                () => status.Set((string)"CC_Playground_Hook_UseHotkey_ModifiersSaved".Translate()),
                KeyModifiers.Control
            );
            UseHotkey.Use(
                KeyCode.Z,
                () => status.Set((string)"CC_Playground_Hook_UseHotkey_ModifiersUndone".Translate()),
                KeyModifiers.Control
            );
            DrawHotkeyLabel(rect, status.Value);
        };

        return Stack.Create(
            SpacingScale.Xxs,
            s => {
                s.Add(Caption.Create(
                    (string)"CC_Playground_Hook_UseHotkey_ModifiersHint".Translate()
                ), 14f);
                s.Add(host, 18f);
            }
        );
    }

    private static LightweaveNode BuildBondDoc() {
        StateHandle<string> status = UseState<string>(
            (string)"CC_Playground_Hook_UseHotkey_BondIdle".Translate()
        );

        LightweaveNode host = NodeBuilder.New("UseHotkeyBond", 0, nameof(UseHotkeyDoc));
        host.Paint = (rect, _) => {
            UseHotkey.Use(
                KeyCode.B,
                () => status.Set((string)"CC_Playground_Hook_UseHotkey_BondInvoked".Translate()),
                KeyModifiers.Control | KeyModifiers.Shift
            );
            DrawHotkeyLabel(rect, status.Value);
        };

        return Stack.Create(
            SpacingScale.Xxs,
            s => {
                s.Add(Caption.Create(
                    (string)"CC_Playground_Hook_UseHotkey_BondHint".Translate()
                ), 14f);
                s.Add(host, 18f);
            }
        );
    }

    private static void DrawHotkeyLabel(Rect rect, string value) {
        Theme.Theme theme = RenderContext.Current.Theme;
        GUIStyle style = GuiStyleCache.GetOrCreate(
            theme.GetFont(FontRole.Body),
            Mathf.RoundToInt(new Rem(0.875f).ToFontPx())
        );
        style.alignment = TextAnchor.MiddleLeft;

        Color saved = GUI.color;
        GUI.color = theme.GetColor(ThemeSlot.TextPrimary);
        GUI.Label(rect, value, style);
        GUI.color = saved;
    }
}


[Doc(
    Id = "useref",
    Summary = "Hook that returns a mutable handle whose value persists across renders without triggering re-paints.",
    WhenToUse = "Hold scratch state - timestamps, last-known-rect, accumulators - that the next render reads but does not visualise.",
    SourcePath = "Lightweave/Lightweave/Hooks/Hooks.cs",
    Target = typeof(Cosmere.Lightweave.Hooks.Hooks),
    TargetMember = nameof(Cosmere.Lightweave.Hooks.Hooks.UseRef),
    ShowRtl = false
)]
public static class UseRefDoc {
    [DocVariant("CC_Playground_Hook_UseRef_LastTick")]
    public static DocSample DocsLastTick() {
        return new DocSample(BuildLastTickDoc, useFullSource: true);
    }

    [DocVariant("CC_Playground_Hook_UseRef_PressCount")]
    public static DocSample DocsPressCount() {
        return new DocSample(BuildPressCountDoc, useFullSource: true);
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(BuildLastTickDoc, useFullSource: true);
    }

    private static LightweaveNode BuildLastTickDoc() {
        RefHandle<float> startTime = UseRef(Time.unscaledTime);
        StateHandle<int> renderTick = UseState(0);

        string elapsed = (Time.unscaledTime - startTime.Current).ToString("0.0");
        string label = ((string)"CC_Playground_Hook_UseRef_LastTickLabel".Translate())
            .Replace("{SECONDS}", elapsed);

        return HStack.Create(
            SpacingScale.Xs,
            r => {
                r.AddFlex(Text.Create(
                    label,
                    FontRole.Body,
                    new Rem(0.9375f),
                    ThemeSlot.TextSecondary,
                    TextAlign.Left
                ));
                r.Add(
                    Button.Create(
                        (string)"CC_Playground_Hook_UseRef_Reset".Translate(),
                        () => {
                            startTime.Current = Time.unscaledTime;
                            renderTick.Set(renderTick.Value + 1);
                        },
                        ButtonVariant.Secondary
                    ),
                    96f
                );
            }
        );
    }

    private static LightweaveNode BuildPressCountDoc() {
        RefHandle<int> presses = UseRef(0);
        StateHandle<int> shown = UseState(0);

        return HStack.Create(
            SpacingScale.Xs,
            r => {
                r.Add(
                    Button.Create(
                        (string)"CC_Playground_Hook_UseRef_Press".Translate(),
                        () => presses.Current = presses.Current + 1,
                        ButtonVariant.Secondary
                    ),
                    120f
                );
                r.AddFlex(Text.Create(
                    ((string)"CC_Playground_Hook_UseRef_PressCountLabel".Translate())
                        .Replace("{COUNT}", shown.Value.ToString()),
                    FontRole.Body,
                    new Rem(0.9375f),
                    ThemeSlot.TextSecondary,
                    TextAlign.Left
                ));
                r.Add(
                    Button.Create(
                        (string)"CC_Playground_Hook_UseRef_Sync".Translate(),
                        () => shown.Set(presses.Current)
                    ),
                    96f
                );
            }
        );
    }
}

[Doc(
    Id = "usememo",
    Summary = "Hook that caches the result of an expensive computation and only recomputes when its dependencies change.",
    WhenToUse = "Avoid resorting / re-filtering large lists or rebuilding palettes on every render.",
    SourcePath = "Lightweave/Lightweave/Hooks/Hooks.cs",
    Target = typeof(Cosmere.Lightweave.Hooks.Hooks),
    TargetMember = nameof(Cosmere.Lightweave.Hooks.Hooks.UseMemo),
    ShowRtl = false
)]
public static class UseMemoDoc {
    [DocVariant("CC_Playground_Hook_UseMemo_SortedRoster")]
    public static DocSample DocsSortedRoster() {
        return new DocSample(BuildSortedRosterDoc, useFullSource: true);
    }

    [DocVariant("CC_Playground_Hook_UseMemo_Filter")]
    public static DocSample DocsFilter() {
        return new DocSample(BuildFilterDoc, useFullSource: true);
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(BuildSortedRosterDoc, useFullSource: true);
    }

    private static readonly string[] RadiantNames = {
        "Kaladin", "Shallan", "Dalinar", "Jasnah", "Renarin", "Lift", "Szeth",
    };

    private static LightweaveNode BuildSortedRosterDoc() {
        StateHandle<bool> ascending = UseState(true);

        string[] sorted = UseMemo(
            () => {
                string[] copy = (string[])RadiantNames.Clone();
                Array.Sort(copy, StringComparer.Ordinal);
                if (!ascending.Value) {
                    Array.Reverse(copy);
                }
                return copy;
            },
            new object[] { ascending.Value }
        );

        string joined = string.Join(", ", sorted);

        return HStack.Create(
            SpacingScale.Xs,
            r => {
                r.Add(
                    Button.Create(
                        ascending.Value
                            ? (string)"CC_Playground_Hook_UseMemo_Descend".Translate()
                            : (string)"CC_Playground_Hook_UseMemo_Ascend".Translate(),
                        () => ascending.Set(!ascending.Value),
                        ButtonVariant.Secondary
                    ),
                    96f
                );
                r.AddFlex(Text.Create(
                    joined,
                    FontRole.Body,
                    new Rem(0.875f),
                    ThemeSlot.TextSecondary,
                    TextAlign.Left
                ));
            }
        );
    }

    private static LightweaveNode BuildFilterDoc() {
        StateHandle<string> query = UseState(string.Empty);

        string[] filtered = UseMemo(
            () => {
                if (string.IsNullOrEmpty(query.Value)) {
                    return RadiantNames;
                }
                List<string> hits = new List<string>();
                for (int i = 0; i < RadiantNames.Length; i++) {
                    if (RadiantNames[i].IndexOf(query.Value, StringComparison.OrdinalIgnoreCase) >= 0) {
                        hits.Add(RadiantNames[i]);
                    }
                }
                return hits.ToArray();
            },
            new object[] { query.Value }
        );

        string label = filtered.Length == 0
            ? (string)"CC_Playground_Hook_UseMemo_NoMatches".Translate()
            : string.Join(", ", filtered);

        return Stack.Create(
            SpacingScale.Xxs,
            s => {
                s.Add(TextField.Create(
                    query.Value,
                    v => query.Set(v),
                    (string)"CC_Playground_Hook_UseMemo_FilterPlaceholder".Translate()
                ), 28f);
                s.Add(Caption.Create(label), 14f);
            }
        );
    }
}

[Doc(
    Id = "useeffect",
    Summary = "Hook that runs a side effect after render when its dependencies change, returning an optional cleanup callback.",
    WhenToUse = "Subscribe to game events, start timers, or sync external state - and tear them down when deps change or the node unmounts.",
    SourcePath = "Lightweave/Lightweave/Hooks/Hooks.cs",
    Target = typeof(Cosmere.Lightweave.Hooks.Hooks),
    TargetMember = nameof(Cosmere.Lightweave.Hooks.Hooks.UseEffect),
    ShowRtl = false
)]
public static class UseEffectDoc {
    [DocVariant("CC_Playground_Hook_UseEffect_StormwallTick")]
    public static DocSample DocsStormwallTick() {
        return new DocSample(BuildStormwallTickDoc, useFullSource: true);
    }

    [DocVariant("CC_Playground_Hook_UseEffect_DepChange")]
    public static DocSample DocsDepChange() {
        return new DocSample(BuildDepChangeDoc, useFullSource: true);
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(BuildStormwallTickDoc, useFullSource: true);
    }

    private static LightweaveNode BuildStormwallTickDoc() {
        StateHandle<bool> watching = UseState(false);
        StateHandle<int> ticks = UseState(0);
        RefHandle<float> nextTick = UseRef(Time.unscaledTime);

        UseEffect(
            () => {
                nextTick.Current = Time.unscaledTime + 1f;
                return null;
            },
            new object[] { watching.Value }
        );

        if (watching.Value && Time.unscaledTime >= nextTick.Current) {
            ticks.Set(ticks.Value + 1);
            nextTick.Current = Time.unscaledTime + 1f;
        }

        string label = watching.Value
            ? ((string)"CC_Playground_Hook_UseEffect_StormwallActive".Translate())
                .Replace("{TICKS}", ticks.Value.ToString())
            : (string)"CC_Playground_Hook_UseEffect_StormwallIdle".Translate();

        return HStack.Create(
            SpacingScale.Xs,
            r => {
                r.Add(
                    Button.Create(
                        watching.Value
                            ? (string)"CC_Playground_Hook_UseEffect_StopWatch".Translate()
                            : (string)"CC_Playground_Hook_UseEffect_StartWatch".Translate(),
                        () => watching.Set(!watching.Value)
                    ),
                    140f
                );
                r.AddFlex(Text.Create(
                    label,
                    FontRole.Body,
                    new Rem(0.9375f),
                    ThemeSlot.TextSecondary,
                    TextAlign.Left
                ));
            }
        );
    }

    private static LightweaveNode BuildDepChangeDoc() {
        StateHandle<int> seed = UseState(0);
        StateHandle<int> reactions = UseState(0);

        UseEffect(
            () => {
                reactions.Set(reactions.Value + 1);
                return null;
            },
            new object[] { seed.Value }
        );

        return HStack.Create(
            SpacingScale.Xs,
            r => {
                r.Add(
                    Button.Create(
                        (string)"CC_Playground_Hook_UseEffect_Bump".Translate(),
                        () => seed.Set(seed.Value + 1),
                        ButtonVariant.Secondary
                    ),
                    96f
                );
                r.AddFlex(Text.Create(
                    ((string)"CC_Playground_Hook_UseEffect_DepChangeLabel".Translate())
                        .Replace("{SEED}", seed.Value.ToString())
                        .Replace("{COUNT}", reactions.Value.ToString()),
                    FontRole.Body,
                    new Rem(0.9375f),
                    ThemeSlot.TextSecondary,
                    TextAlign.Left
                ));
            }
        );
    }
}

[Doc(
    Id = "usecontext",
    Summary = "Hook that reads the nearest context value of type T from the render tree.",
    WhenToUse = "Pass theming, locale, or session data deep without prop-drilling. Provide values via Provider nodes.",
    SourcePath = "Lightweave/Lightweave/Hooks/Hooks.cs",
    Target = typeof(Cosmere.Lightweave.Hooks.Hooks),
    TargetMember = nameof(Cosmere.Lightweave.Hooks.Hooks.UseContext),
    ShowRtl = false
)]
public static class UseContextDoc {
    [DocVariant("CC_Playground_Hook_UseContext_AmbientTheme")]
    public static DocSample DocsAmbientTheme() {
        return new DocSample(BuildAmbientThemeDoc, useFullSource: true);
    }

    [DocVariant("CC_Playground_Hook_UseContext_AmbientDirection")]
    public static DocSample DocsAmbientDirection() {
        return new DocSample(BuildAmbientDirectionDoc, useFullSource: true);
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(BuildAmbientThemeDoc, useFullSource: true);
    }

    private static LightweaveNode BuildAmbientThemeDoc() {
        Theme.Theme theme = UseTheme();

        string label = ((string)"CC_Playground_Hook_UseContext_ThemeLabel".Translate())
            .Replace("{NAME}", theme.GetType().Name);

        return Text.Create(
            label,
            FontRole.Body,
            new Rem(0.9375f),
            ThemeSlot.TextSecondary,
            TextAlign.Left
        );
    }

    private static LightweaveNode BuildAmbientDirectionDoc() {
        Direction direction = UseDirection();
        string label = ((string)"CC_Playground_Hook_UseContext_DirectionLabel".Translate())
            .Replace("{DIR}", direction.ToString());

        return Text.Create(
            label,
            FontRole.Body,
            new Rem(0.9375f),
            ThemeSlot.TextSecondary,
            TextAlign.Left
        );
    }
}

[Doc(
    Id = "usetheme",
    Summary = "Hook that returns the active Theme for the current render subtree.",
    WhenToUse = "Read theme colors, fonts, or spacing tokens when authoring Paint callbacks.",
    SourcePath = "Lightweave/Lightweave/Hooks/Hooks.cs",
    Target = typeof(Cosmere.Lightweave.Hooks.Hooks),
    TargetMember = nameof(Cosmere.Lightweave.Hooks.Hooks.UseTheme),
    ShowRtl = false
)]
public static class UseThemeDoc {
    [DocVariant("CC_Playground_Hook_UseTheme_Surface")]
    public static DocSample DocsSurface() {
        return new DocSample(BuildSurfaceDoc, useFullSource: true);
    }

    [DocVariant("CC_Playground_Hook_UseTheme_Accent")]
    public static DocSample DocsAccent() {
        return new DocSample(BuildAccentDoc, useFullSource: true);
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(BuildSurfaceDoc, useFullSource: true);
    }

    private static LightweaveNode BuildSurfaceDoc() {
        Theme.Theme theme = UseTheme();

        LightweaveNode swatch = NodeBuilder.New("UseThemeSurface", 0, nameof(UseThemeDoc));
        swatch.Paint = (rect, _) => {
            Color saved = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.SurfaceRaised);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = saved;
        };

        return HStack.Create(
            SpacingScale.Xs,
            r => {
                r.Add(swatch, 32f);
                r.AddFlex(Text.Create(
                    (string)"CC_Playground_Hook_UseTheme_SurfaceLabel".Translate(),
                    FontRole.Body,
                    new Rem(0.9375f),
                    ThemeSlot.TextSecondary,
                    TextAlign.Left
                ));
            }
        );
    }

    private static LightweaveNode BuildAccentDoc() {
        Theme.Theme theme = UseTheme();

        LightweaveNode swatch = NodeBuilder.New("UseThemeAccent", 0, nameof(UseThemeDoc));
        swatch.Paint = (rect, _) => {
            Color saved = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.SurfaceAccent);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = saved;
        };

        return HStack.Create(
            SpacingScale.Xs,
            r => {
                r.Add(swatch, 32f);
                r.AddFlex(Text.Create(
                    (string)"CC_Playground_Hook_UseTheme_AccentLabel".Translate(),
                    FontRole.Body,
                    new Rem(0.9375f),
                    ThemeSlot.TextSecondary,
                    TextAlign.Left
                ));
            }
        );
    }
}

[Doc(
    Id = "usedirection",
    Summary = "Hook that returns the active text direction (Ltr or Rtl) for the current subtree.",
    WhenToUse = "Mirror layout decisions - leading vs trailing icons, gradient direction, slide-in axis - to match the active locale.",
    SourcePath = "Lightweave/Lightweave/Hooks/Hooks.cs",
    Target = typeof(Cosmere.Lightweave.Hooks.Hooks),
    TargetMember = nameof(Cosmere.Lightweave.Hooks.Hooks.UseDirection),
    ShowRtl = true
)]
public static class UseDirectionDoc {
    [DocVariant("CC_Playground_Hook_UseDirection_Arrow")]
    public static DocSample DocsArrow() {
        return new DocSample(BuildArrowDoc, useFullSource: true);
    }

    [DocVariant("CC_Playground_Hook_UseDirection_Label")]
    public static DocSample DocsLabel() {
        return new DocSample(BuildLabelDoc, useFullSource: true);
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(BuildArrowDoc, useFullSource: true);
    }

    private static LightweaveNode BuildArrowDoc() {
        Direction direction = UseDirection();
        string arrow = direction == Direction.Rtl ? "<-" : "->";

        return Text.Create(
            arrow,
            FontRole.BodyBold,
            new Rem(1.25f),
            ThemeSlot.TextPrimary,
            TextAlign.Center,
            FontStyle.Bold
        );
    }

    private static LightweaveNode BuildLabelDoc() {
        Direction direction = UseDirection();
        string key = direction == Direction.Rtl
            ? "CC_Playground_Hook_UseDirection_LabelRtl"
            : "CC_Playground_Hook_UseDirection_LabelLtr";

        return Text.Create(
            (string)key.Translate(),
            FontRole.Body,
            new Rem(0.9375f),
            ThemeSlot.TextSecondary,
            TextAlign.Left
        );
    }
}

[Doc(
    Id = "usebreakpoint",
    Summary = "Hook that returns the active responsive breakpoint (Xs..Xxl) for the current viewport.",
    WhenToUse = "Switch column counts, hide secondary UI, or pick larger fonts based on the host window size.",
    SourcePath = "Lightweave/Lightweave/Hooks/Hooks.cs",
    Target = typeof(Cosmere.Lightweave.Hooks.Hooks),
    TargetMember = nameof(Cosmere.Lightweave.Hooks.Hooks.UseBreakpoint),
    ShowRtl = false
)]
public static class UseBreakpointDoc {
    [DocVariant("CC_Playground_Hook_UseBreakpoint_Current")]
    public static DocSample DocsCurrent() {
        return new DocSample(BuildCurrentDoc, useFullSource: true);
    }

    [DocVariant("CC_Playground_Hook_UseBreakpoint_Adaptive")]
    public static DocSample DocsAdaptive() {
        return new DocSample(BuildAdaptiveDoc, useFullSource: true);
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(BuildCurrentDoc, useFullSource: true);
    }

    private static LightweaveNode BuildCurrentDoc() {
        Breakpoint bp = UseBreakpoint();

        return Text.Create(
            ((string)"CC_Playground_Hook_UseBreakpoint_CurrentLabel".Translate())
                .Replace("{BP}", bp.ToString()),
            FontRole.BodyBold,
            new Rem(1f),
            ThemeSlot.TextPrimary,
            TextAlign.Left,
            FontStyle.Bold
        );
    }

    private static LightweaveNode BuildAdaptiveDoc() {
        Breakpoint bp = UseBreakpoint();

        string copy;
        if (bp >= Breakpoint.Lg) {
            copy = (string)"CC_Playground_Hook_UseBreakpoint_AdaptiveLarge".Translate();
        }
        else if (bp >= Breakpoint.Md) {
            copy = (string)"CC_Playground_Hook_UseBreakpoint_AdaptiveMedium".Translate();
        }
        else {
            copy = (string)"CC_Playground_Hook_UseBreakpoint_AdaptiveCompact".Translate();
        }

        return Text.Create(
            copy,
            FontRole.Body,
            new Rem(0.9375f),
            ThemeSlot.TextSecondary,
            TextAlign.Left
        );
    }
}
