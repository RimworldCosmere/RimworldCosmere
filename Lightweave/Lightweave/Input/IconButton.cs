using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using static Cosmere.Lightweave.Typography.Typography;

namespace Cosmere.Lightweave.Input;

[Doc(
    Id = "iconbutton",
    Summary = "Compact square button hosting a single icon.",
    WhenToUse = "Trigger an action where space is tight or the icon is unambiguous.",
    SourcePath = "CosmereCore/CosmereCore/Lightweave/Input/IconButton.cs"
)]
public static class IconButton {
    public static LightweaveNode Create(
        [DocParam("Icon node painted inside the button square.")]
        LightweaveNode icon,
        [DocParam("Action invoked on left mouse up while hovering.")]
        Action? onClick,
        [DocParam("Visual variant: Ghost (default), Primary, Secondary, or Danger.")]
        ButtonVariant variant = ButtonVariant.Ghost,
        [DocParam("Override icon size; defaults to 1.25rem.")]
        Rem? iconSize = null,
        [DocParam("Disables interaction and applies disabled styling.")]
        bool disabled = false,
        [DocParam("Translation key for an optional tooltip (reserved).")]
        string? tooltipKey = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        _ = tooltipKey;

        LightweaveNode node = NodeBuilder.New("IconButton", line, file);
        node.Children.Add(icon);

        float iconPx = (iconSize ?? new Rem(1.25f)).ToPixels();
        float padPx = SpacingScale.Xs.ToPixels();
        float squareSize = iconPx + padPx * 2f;
        node.PreferredHeight = squareSize;

        node.Paint = (rect, paintChildren) => {
            float size = Mathf.Min(squareSize, Mathf.Min(rect.width, rect.height));
            Rect square = new Rect(
                rect.x + (rect.width - size) / 2f,
                rect.y + (rect.height - size) / 2f,
                size,
                size
            );

            InteractionState state = InteractionState.Resolve(square, null, disabled);

            ThemeSlot bgSlot = ButtonVariants.Background(variant, state);
            ThemeSlot? borderSlot = ButtonVariants.Border(variant, state);

            BackgroundSpec bg = new BackgroundSpec.Solid(bgSlot);
            BorderSpec? border = borderSlot.HasValue
                ? BorderSpec.All(new Rem(1f / 16f), borderSlot.Value)
                : null;
            RadiusSpec radius = RadiusSpec.All(new Rem(0.25f));

            PaintBox.Draw(square, bg, border, radius);

            float overlay = ButtonVariants.OverlayAlpha(state);
            if (overlay > 0f) {
                Color overlayColor = state.Pressed
                    ? new Color(0f, 0f, 0f, overlay)
                    : new Color(1f, 1f, 1f, overlay);
                PaintBox.Draw(square, new BackgroundSpec.Solid(overlayColor), null, radius);
            }

            float innerPad = Mathf.Min(padPx, (size - iconPx) / 2f);
            Rect padRect = new Rect(
                square.x + innerPad,
                square.y + innerPad,
                size - innerPad * 2f,
                size - innerPad * 2f
            );
            icon.MeasuredRect = padRect;

            paintChildren();

            Event e = Event.current;
            if (!disabled &&
                onClick != null &&
                e.type == EventType.MouseUp &&
                e.button == 0 &&
                square.Contains(e.mousePosition)) {
                onClick.Invoke();
                e.Use();
            }
        };

        return node;
    }

    private static LightweaveNode MakeDocIcon() {
        return Icon.Create(Texture2D.whiteTexture, new Rem(1f), ThemeSlot.TextPrimary);
    }

    [DocVariant("CC_Playground_Label_Ghost")]
    public static DocSample DocsGhost() {
        bool forced = PlaygroundDemoContext.Current.ForceDisabled;
        return new DocSample(Create(MakeDocIcon(), () => { }, disabled: forced));
    }

    [DocVariant("CC_Playground_Label_Primary")]
    public static DocSample DocsPrimary() {
        bool forced = PlaygroundDemoContext.Current.ForceDisabled;
        return new DocSample(Create(MakeDocIcon(), () => { }, ButtonVariant.Primary, disabled: forced));
    }

    [DocVariant("CC_Playground_Label_Secondary")]
    public static DocSample DocsSecondary() {
        bool forced = PlaygroundDemoContext.Current.ForceDisabled;
        return new DocSample(Create(MakeDocIcon(), () => { }, ButtonVariant.Secondary, disabled: forced));
    }

    [DocState("CC_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        bool forced = PlaygroundDemoContext.Current.ForceDisabled;
        return new DocSample(Create(MakeDocIcon(), () => { }, disabled: forced));
    }

    [DocState("CC_Playground_Label_Hover")]
    public static DocSample DocsHover() {
        bool forced = PlaygroundDemoContext.Current.ForceDisabled;
        return new DocSample(Create(MakeDocIcon(), () => { }, disabled: forced));
    }

    [DocState("CC_Playground_Label_Disabled")]
    public static DocSample DocsDisabled() {
        return new DocSample(Create(MakeDocIcon(), () => { }, disabled: true));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(Create(MakeDocIcon(), () => { }));
    }
}
