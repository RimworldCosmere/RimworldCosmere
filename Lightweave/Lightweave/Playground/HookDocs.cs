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
using static Cosmere.Lightweave.Layout.Layout;
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
    WhenToUse = "Track local UI state - counters, toggles, draft values - inside a render function.",
    SourcePath = "Lightweave/Lightweave/Hooks/Hooks.cs",
    ShowRtl = false
)]
public static class UseStateDoc {
    [DocVariant("CC_Playground_UseState_Label")]
    public static DocSample DocsCounter() {
        return BuildCounterDoc();
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return BuildCounterDoc();
    }

    private static DocSample BuildCounterDoc() {
        StateHandle<int> count = UseState(0);

        LightweaveNode countLabel = Text.Create(
            count.Value.ToString(),
            FontRole.BodyBold,
            new Rem(1f),
            ThemeSlot.TextPrimary,
            TextAlign.Center,
            FontStyle.Bold
        );

        LightweaveNode row = HStack.Create(
            SpacingScale.Xs,
            r => {
                r.Add(
                    Button.Create(
                        (string)"CC_Playground_UseState_Decrement".Translate(),
                        () => count.Set(count.Value - 1),
                        ButtonVariant.Secondary
                    ),
                    48f
                );
                r.AddFlex(countLabel);
                r.Add(
                    Button.Create(
                        (string)"CC_Playground_UseState_Increment".Translate(),
                        () => count.Set(count.Value + 1)
                    ),
                    48f
                );
            }
        );

        return new DocSample(row);
    }
}

[Doc(
    Id = "useanim",
    Summary = "Hook that smoothly animates a float toward a target value each frame.",
    WhenToUse = "Drive transitions for opacity, scale, or color blends without manual tweening.",
    SourcePath = "Lightweave/Lightweave/Hooks/Hooks.cs",
    ShowRtl = false
)]
public static class UseAnimDoc {
    [DocVariant("CC_Playground_Label_Default")]
    public static DocSample DocsFade() {
        return BuildFadeDoc();
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return BuildFadeDoc();
    }

    private static DocSample BuildFadeDoc() {
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

        LightweaveNode row = HStack.Create(
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

        return new DocSample(row);
    }
}

[Doc(
    Id = "usefocus",
    Summary = "Hook that exposes a focus handle for routing keyboard focus to an input.",
    WhenToUse = "Programmatically focus a TextField when a button is pressed or a panel opens.",
    SourcePath = "Lightweave/Lightweave/Hooks/Hooks.cs",
    ShowRtl = false
)]
public static class UseFocusDoc {
    [DocVariant("CC_Playground_Label_Default")]
    public static DocSample DocsFocus() {
        return BuildFocusDoc();
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return BuildFocusDoc();
    }

    private static DocSample BuildFocusDoc() {
        StateHandle<string> text = UseState(string.Empty);
        UseFocus.FocusHandle focus = UseFocus.Use();

        LightweaveNode row = HStack.Create(
            SpacingScale.Xs,
            r => {
                r.Add(
                    Button.Create(
                        (string)"CC_Playground_UseFocus_Focus".Translate(),
                        () => focus.Request(),
                        ButtonVariant.Secondary
                    ),
                    120f
                );
                r.AddFlex(
                    TextField.Create(
                        text.Value,
                        v => text.Set(v),
                        (string)"CC_Playground_UseFocus_Placeholder".Translate(),
                        focus: focus
                    )
                );
            }
        );

        return new DocSample(row);
    }
}

[Doc(
    Id = "usehotkey",
    Summary = "Hook that binds a key chord to a callback while a node is mounted.",
    WhenToUse = "Add Escape-to-close, Ctrl+S, or other shortcuts scoped to a specific surface.",
    SourcePath = "Lightweave/Lightweave/Hooks/Hooks.cs",
    ShowRtl = false
)]
public static class UseHotkeyDoc {
    [DocVariant("CC_Playground_Label_Default")]
    public static DocSample DocsHotkey() {
        return BuildHotkeyDoc();
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return BuildHotkeyDoc();
    }

    private static DocSample BuildHotkeyDoc() {
        StateHandle<string> status = UseState<string>(
            (string)"CC_Playground_UseHotkey_Idle".Translate()
        );

        LightweaveNode hotkeyHost = NodeBuilder.New("UseHotkeyHost", 0, nameof(UseHotkeyDoc));
        hotkeyHost.Paint = (rect, _) => {
            UseHotkey.Use(
                KeyCode.Escape,
                () => status.Set("CC_Playground_UseHotkey_Escape".Translate())
            );
            UseHotkey.Use(
                KeyCode.S,
                () => status.Set((string)"CC_Playground_UseHotkey_Saved".Translate()),
                KeyModifiers.Control
            );

            Theme.Theme theme = RenderContext.Current.Theme;
            GUIStyle style = GuiStyleCache.Get(
                theme.GetFont(FontRole.Body),
                Mathf.RoundToInt(new Rem(0.875f).ToFontPx())
            );
            style.alignment = TextAnchor.MiddleLeft;

            Color saved = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.TextPrimary);
            GUI.Label(rect, status.Value, style);
            GUI.color = saved;
        };

        LightweaveNode hint = Caption.Create(
            (string)"CC_Playground_UseHotkey_Hint".Translate()
        );

        LightweaveNode stack = Stack.Create(
            SpacingScale.Xxs,
            s => {
                s.Add(hint, 14f);
                s.Add(hotkeyHost, 18f);
            }
        );

        return new DocSample(stack);
    }
}
