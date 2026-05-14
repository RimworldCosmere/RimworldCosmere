using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;
using static Cosmere.Lightweave.Hooks.Hooks;
using static Cosmere.Lightweave.Typography.Typography;
using Eyebrow = Cosmere.Lightweave.Typography.Eyebrow;
using Text = Cosmere.Lightweave.Typography.Typography.Text;

namespace Cosmere.Lightweave.Overlay;

[Doc(
    Id = "dialog",
    Summary = "Overlay surface for confirmations, forms, or focused tasks. Modal by default; toggle isModal to compose a non-blocking variant.",
    WhenToUse = "Use to host any focus-stealing surface (Load Colony, Mods, Options) where the player must finish a single task before returning to play. Pass isOpen to spawn its own window, or omit isOpen and embed inline inside an existing LightweaveWindow.",
    SourcePath = "Lightweave/Lightweave/Overlay/Dialog.cs",
    PreferredVariantHeight = 320f
)]
public static class Dialog {
    public static LightweaveNode Create(
        [DocParam("Builds the dialog content node.")]
        Func<LightweaveNode> content,
        [DocParam("Whether the dialog is currently visible. Null means inline mode: the caller is already inside a LightweaveWindow and the dialog renders as a node, not a managed window.", TypeOverride = "bool?", DefaultOverride = "null")]
        bool? isOpen = null,
        [DocParam("Invoked when the dialog requests dismissal. Only meaningful in windowed mode.", TypeOverride = "Action?", DefaultOverride = "null")]
        Action? onDismiss = null,
        [DocParam("Blocks input outside the dialog and draws the full-screen scrim. Set false for non-modal floating dialogs.")]
        bool isModal = true,
        [DocParam("Explicit pixel width for the dialog card. Null = Screen.width * WidthFraction clamped to MaxWidth.", TypeOverride = "float?", DefaultOverride = "null")]
        float? width = null,
        [DocParam("Explicit pixel height for the dialog card. Null = Screen.height * HeightFraction clamped to MaxHeight.", TypeOverride = "float?", DefaultOverride = "null")]
        float? height = null,
        [DocParam("Fraction of screen width to use when Width is null.")]
        float widthFraction = 0.66f,
        [DocParam("Fraction of screen height to use when Height is null.")]
        float heightFraction = 0.82f,
        [DocParam("Hard upper bound for computed width when Width is null.")]
        float maxWidth = 1800f,
        [DocParam("Hard upper bound for computed height when Height is null.")]
        float maxHeight = 1300f,
        [DocParam("Scrim fill color drawn behind the dialog card. Only drawn when isModal is true. Defaults to rgba(0,0,0,0.55).", TypeOverride = "Color?", DefaultOverride = "null")]
        Color? scrimColor = null,
        [DocParam("Dialog card background. Defaults to BackgroundSpec.Blur(rgba(0,0,0,0.4), 10px).", TypeOverride = "BackgroundSpec?", DefaultOverride = "null")]
        BackgroundSpec? cardBackground = null,
        [DocParam("Dialog card border. Defaults to 1/16rem BorderDefault on all sides.", TypeOverride = "BorderSpec?", DefaultOverride = "null")]
        BorderSpec? cardBorder = null,
        [DocParam("Dialog card padding (between border and content). Defaults to 1/16rem on all sides.", TypeOverride = "EdgeInsets?", DefaultOverride = "null")]
        EdgeInsets? cardPadding = null,
        [DocParam("Dialog card corner radius. Null = sharp corners.", TypeOverride = "RadiusSpec?", DefaultOverride = "null")]
        RadiusSpec? cardRadius = null,
        [DocParam("Draw the subtle vertical accent gradient overlay inside the card.")]
        bool drawGradient = true,
        [DocParam("Top color of the accent gradient. Defaults to rgba(0.831,0.659,0.341,0.10).", TypeOverride = "Color?", DefaultOverride = "null")]
        Color? gradientTopColor = null,
        [DocParam("Bottom color of the accent gradient. Defaults to fully-transparent gold.", TypeOverride = "Color?", DefaultOverride = "null")]
        Color? gradientBottomColor = null,
        [DocParam("Wrap the entire shell in a Vignette.")]
        bool drawVignette = true,
        [DocParam("Vignette falloff shape.")]
        VignetteShape vignetteShape = VignetteShape.Radial,
        [DocParam("Vignette alpha multiplier.")]
        float vignetteIntensity = 0.9f,
        [DocParam("Vignette coverage multiplier. >1 = darker / wider, <1 = lighter / narrower.")]
        float vignetteScale = 1.4f,
        [DocParam("Vignette color. Defaults to ThemeSlot.OverlayDim.", TypeOverride = "ColorRef?", DefaultOverride = "null")]
        ColorRef? vignetteColor = null,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        DialogShellOptions options = new DialogShellOptions {
            IsModal = isModal,
            Width = width,
            Height = height,
            WidthFraction = widthFraction,
            HeightFraction = heightFraction,
            MaxWidth = maxWidth,
            MaxHeight = maxHeight,
            ScrimColor = scrimColor,
            CardBackground = cardBackground,
            CardBorder = cardBorder,
            CardPadding = cardPadding,
            CardRadius = cardRadius,
            DrawGradient = drawGradient,
            GradientTopColor = gradientTopColor,
            GradientBottomColor = gradientBottomColor,
            DrawVignette = drawVignette,
            VignetteShape = vignetteShape,
            VignetteIntensity = vignetteIntensity,
            VignetteScale = vignetteScale,
            VignetteColor = vignetteColor,
        };

        if (!isOpen.HasValue) {
            LightweaveNode shell = BuildShell(content(), options);
            shell.ApplyStyling("dialog", style, classes, id);
            return shell;
        }

        RefHandle<DialogWindow?> windowRef = UseRef<DialogWindow?>(null, line, file);
        LightweaveNode node = NodeBuilder.New("Dialog", line, file);
        node.ApplyStyling("dialog", style, classes, id);
        bool openNow = isOpen.Value;
        Action? dismiss = onDismiss;
        node.Paint = (_, _) => {
            DialogWindow? current = windowRef.Current;
            bool inStack = current != null && Find.WindowStack.IsOpen(current);

            if (openNow) {
                if (current == null || !inStack) {
                    DialogWindow fresh = new DialogWindow(content, dismiss ?? (() => { }), options);
                    Find.WindowStack.Add(fresh);
                    windowRef.Current = fresh;
                }

                return;
            }

            if (current != null) {
                if (inStack) {
                    current.Close();
                }

                windowRef.Current = null;
            }
        };
        return node;
    }

    internal static LightweaveNode BuildShell(LightweaveNode content, DialogShellOptions opts) {
        Color resolvedScrim = opts.ScrimColor ?? new Color(0f, 0f, 0f, 0.55f);
        BackgroundSpec resolvedCardBg = opts.CardBackground ?? BackgroundSpec.Blur(new Color(0f, 0f, 0f, 0.4f), 10f);
        BorderSpec resolvedCardBorder = opts.CardBorder ?? BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault);
        EdgeInsets resolvedCardPadding = opts.CardPadding ?? EdgeInsets.All(new Rem(1f / 16f));
        Color resolvedGradientTop = opts.GradientTopColor ?? new Color(0.831f, 0.659f, 0.341f, 0.10f);
        Color resolvedGradientBottom = opts.GradientBottomColor ?? new Color(0.831f, 0.659f, 0.341f, 0.0f);
        ColorRef resolvedVignetteColor = opts.VignetteColor ?? (ColorRef)ThemeSlot.OverlayDim;

        float screenW = Screen.width;
        float screenH = Screen.height;
        float modalW = opts.Width ?? Mathf.Min(screenW * opts.WidthFraction, opts.MaxWidth);
        float modalH = opts.Height ?? Mathf.Min(screenH * opts.HeightFraction, opts.MaxHeight);

        LightweaveNode modalCard = Box.Create(
            children: c => {
                if (opts.DrawGradient) {
                    c.Add(Box.Create(style: new Style {
                        Position = Position.Absolute,
                        Top = new Rem(0f),
                        Right = new Rem(0f),
                        Bottom = new Rem(0f),
                        Left = new Rem(0f),
                        Background = new BackgroundSpec.Gradient(
                            GradientTextureCache.Vertical(resolvedGradientTop, resolvedGradientBottom)
                        ),
                    }));
                }
                c.Add(content);
            },
            style: new Style {
                Position = Position.Relative,
                Background = resolvedCardBg,
                Border = resolvedCardBorder,
                Padding = resolvedCardPadding,
                Radius = opts.CardRadius,
            }
        );

        LightweaveNode centeringStack = Stack.Create(SpacingScale.None, root => {
            root.AddFlex(Spacer.Flex());
            root.Add(HStack.Create(SpacingScale.None, h => {
                h.AddFlex(Spacer.Flex());
                h.Add(modalCard, modalW);
                h.AddFlex(Spacer.Flex());
            }), modalH);
            root.AddFlex(Spacer.Flex());
        });

        LightweaveNode body = opts.IsModal
            ? Box.Create(
                children: c => c.Add(centeringStack),
                style: new Style { Background = BackgroundSpec.Of(resolvedScrim) }
            )
            : centeringStack;

        return opts.DrawVignette
            ? Vignette.Create(
                body,
                shape: opts.VignetteShape,
                intensity: opts.VignetteIntensity,
                scale: opts.VignetteScale,
                color: resolvedVignetteColor
            )
            : body;
    }

    private static LightweaveNode BuildPreview(
        bool isModal = true,
        bool drawVignette = true,
        bool drawGradient = true,
        float vignetteIntensity = 0.35f,
        float vignetteScale = 0.9f
    ) {
        LightweaveNode card = Stack.Create(SpacingScale.Sm, c => {
            c.Add(Eyebrow.Create((string)"CL_Playground_Dialog_Eyebrow".Translate()));
            c.Add(Heading.Create(3, (string)"CL_Playground_Dialog_Heading".Translate()));
            c.Add(Text.Create(
                (string)"CL_Playground_Dialog_Body".Translate(),
                wrap: true,
                style: new Style { FontFamily = FontRole.Body, FontSize = new Rem(0.875f), TextColor = ThemeSlot.TextSecondary }
            ));
            c.AddFlex(Spacer.Flex());
            c.Add(HStack.Create(SpacingScale.Sm, h => {
                h.AddFlex(Spacer.Flex());
                h.AddHug(Button.Create(
                    (string)"CL_Playground_Dialog_Cancel".Translate(),
                    () => { },
                    ButtonVariant.Secondary
                ));
                h.AddHug(Button.Create(
                    (string)"CL_Playground_Dialog_Confirm".Translate(),
                    () => { },
                    ButtonVariant.Primary
                ));
            }, style: new Style { Height = new Rem(2.5f) }));
        }, style: new Style { Padding = EdgeInsets.All(SpacingScale.Md) });

        return Create(
            content: () => card,
            isModal: isModal,
            width: 360f,
            height: 220f,
            scrimColor: new Color(0f, 0f, 0f, 0.25f),
            drawGradient: drawGradient,
            drawVignette: drawVignette,
            vignetteIntensity: vignetteIntensity,
            vignetteScale: vignetteScale
        );
    }

    [DocVariant("CL_Playground_Label_Default")]
    public static DocSample DocsDefault() {
        return new DocSample(() => BuildPreview(), useFullSource: true);
    }

    [DocVariant("CL_Playground_Dialog_Variant_NonModal", Order = 1)]
    public static DocSample DocsNonModal() {
        return new DocSample(() => BuildPreview(isModal: false), useFullSource: true);
    }

    [DocVariant("CL_Playground_Dialog_Variant_NoVignette", Order = 2)]
    public static DocSample DocsNoVignette() {
        return new DocSample(() => BuildPreview(drawVignette: false), useFullSource: true);
    }

    [DocVariant("CL_Playground_Dialog_Variant_NoGradient", Order = 3)]
    public static DocSample DocsNoGradient() {
        return new DocSample(() => BuildPreview(drawGradient: false), useFullSource: true);
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => BuildPreview(), useFullSource: true);
    }
}
