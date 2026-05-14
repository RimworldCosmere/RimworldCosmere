using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;
using static Cosmere.Lightweave.Typography.Typography;

namespace Cosmere.Lightweave.Input;

[Doc(
    Id = "iconbutton",
    Summary = "Compact square button hosting a single icon.",
    WhenToUse = "Trigger an action where space is tight or the icon is unambiguous.",
    SourcePath = "Lightweave/Lightweave/Input/IconButton.cs"
)]
public static class IconButton {
    public static LightweaveNode Create(
        LightweaveNode icon,
        Action? onClick,
        ButtonVariant variant = ButtonVariant.Ghost,
        Rem? iconSize = null,
        bool disabled = false,
        string? tooltipKey = null,
        bool? playHoverSound = null,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New("IconButton", line, file);
        node.ApplyStyling("icon-button", style, classes, id);
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

            if (!string.IsNullOrEmpty(tooltipKey)) {
                TooltipHandler.TipRegion(square, (string)tooltipKey.Translate());
            }

            InteractionState state = InteractionState.Resolve(square, null, disabled);

            ThemeSlot? bgSlot = ButtonVariants.Background(variant, state);
            ThemeSlot? borderSlot = ButtonVariants.Border(variant, state);

            BackgroundSpec? bg = bgSlot.HasValue ? BackgroundSpec.Of(bgSlot.Value) : null;
            BorderSpec? border = borderSlot.HasValue
                ? BorderSpec.All(new Rem(1f / 16f), borderSlot.Value)
                : null;
            RadiusSpec radius = RadiusSpec.All(RadiusScale.Sm);

            PaintBox.Draw(square, bg, border, radius);

            float overlay = ButtonVariants.OverlayAlpha(state);
            if (overlay > 0f) {
                Color overlayColor = InteractionFeedback.OverlayColor(RenderContext.Current.Theme, state, overlay);
                PaintBox.Draw(square, BackgroundSpec.Of(overlayColor), null, radius);
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

            InteractionFeedback.Apply(square, !disabled, playHoverSound ?? true);

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

    

    [DocVariant("CL_Playground_Label_Ghost")]
    public static DocSample DocsGhost() {
        bool forced = RenderContext.Current.ForceDisabled;
        return new DocSample(() => Create(Icon.Create(TexButton.Reveal, new Rem(1f), style: new Style { TextColor = ThemeSlot.TextPrimary }), () => { }, disabled: forced));
    }

    [DocVariant("CL_Playground_Label_Primary")]
    public static DocSample DocsPrimary() {
        bool forced = RenderContext.Current.ForceDisabled;
        return new DocSample(() => Create(Icon.Create(TexButton.NewItem, new Rem(1f), style: new Style { TextColor = ThemeSlot.TextPrimary }), () => { }, ButtonVariant.Primary, disabled: forced));
    }

    [DocVariant("CL_Playground_Label_Secondary")]
    public static DocSample DocsSecondary() {
        bool forced = RenderContext.Current.ForceDisabled;
        return new DocSample(() => Create(Icon.Create(TexButton.Search, new Rem(1f), style: new Style { TextColor = ThemeSlot.TextPrimary }), () => { }, ButtonVariant.Secondary, disabled: forced));
    }

    [DocState("CL_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        bool forced = RenderContext.Current.ForceDisabled;
        return new DocSample(() => Create(Icon.Create(TexButton.Info, new Rem(1f), style: new Style { TextColor = ThemeSlot.TextPrimary }), () => { }, disabled: forced));
    }

    [DocState("CL_Playground_Label_Hover")]
    public static DocSample DocsHover() {
        bool forced = RenderContext.Current.ForceDisabled;
        return new DocSample(() => Create(Icon.Create(TexButton.Rename, new Rem(1f), style: new Style { TextColor = ThemeSlot.TextPrimary }), () => { }, disabled: forced));
    }

    [DocState("CL_Playground_Label_Disabled")]
    public static DocSample DocsDisabled() {
        return new DocSample(() => Create(Icon.Create(TexButton.CloseXSmall, new Rem(1f), style: new Style { TextColor = ThemeSlot.TextPrimary }), () => { }, disabled: true));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => Create(Icon.Create(TexButton.Plus, new Rem(1f), style: new Style { TextColor = ThemeSlot.TextPrimary }), () => { }));
    }
}
