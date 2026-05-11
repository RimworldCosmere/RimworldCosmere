using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Theme;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Playground;

[Doc(
    Id = "window",
    Summary = "Top-level resizable Lightweave window assembled from Header / Body / Footer slots.",
    WhenToUse = "Host a tool, dialog, or panel as its own movable window in the WindowStack.",
    SourcePath = "Lightweave/Lightweave/Runtime/LightweaveWindow.cs",
    Target = typeof(LightweaveWindow)
)]
public static class WindowDoc {
    [DocVariant("CL_Playground_Window_Bordered")]
    public static DocSample DocsBordered() {
        return new DocSample(() => 
            Button.Create(
                (string)"CL_Playground_Window_Open".Translate(),
                () => Find.WindowStack.Add(new BorderedSampleWindow()),
                ButtonVariant.Secondary
            ),
            companion: typeof(BorderedSampleWindow)
        );
    }

    [DocVariant("CL_Playground_Window_Borderless")]
    public static DocSample DocsBorderless() {
        return new DocSample(() => 
            Button.Create(
                (string)"CL_Playground_Window_Open".Translate(),
                () => Find.WindowStack.Add(new BorderlessSampleWindow()),
                ButtonVariant.Secondary
            ),
            companion: typeof(BorderlessSampleWindow)
        );
    }

    [DocVariant("CL_Playground_Window_FixedSize")]
    public static DocSample DocsFixedSize() {
        return new DocSample(() => 
            Button.Create(
                (string)"CL_Playground_Window_Open".Translate(),
                () => Find.WindowStack.Add(new FixedSizeSampleWindow()),
                ButtonVariant.Secondary
            ),
            companion: typeof(FixedSizeSampleWindow)
        );
    }

    [DocVariant("CL_Playground_Window_Large")]
    public static DocSample DocsLarge() {
        return new DocSample(() => 
            Button.Create(
                (string)"CL_Playground_Window_Open".Translate(),
                () => Find.WindowStack.Add(new LargeSampleWindow()),
                ButtonVariant.Secondary
            ),
            companion: typeof(LargeSampleWindow)
        );
    }

    [DocVariant("CL_Playground_Window_WithFooter")]
    public static DocSample DocsWithFooter() {
        return new DocSample(() => 
            Button.Create(
                (string)"CL_Playground_Window_Open".Translate(),
                () => Find.WindowStack.Add(new DialogSampleWindow()),
                ButtonVariant.Secondary
            ),
            companion: typeof(DialogSampleWindow)
        );
    }

    [DocVariant("CL_Playground_Window_StatusBar")]
    public static DocSample DocsStatusBar() {
        return new DocSample(() => 
            Button.Create(
                (string)"CL_Playground_Window_Open".Translate(),
                () => Find.WindowStack.Add(new StatusBarSampleWindow()),
                ButtonVariant.Secondary
            ),
            companion: typeof(StatusBarSampleWindow)
        );
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => 
            Button.Create(
                (string)"CL_Playground_Window_Open".Translate(),
                () => Find.WindowStack.Add(new BorderedSampleWindow()),
                ButtonVariant.Secondary
            ),
            companion: typeof(BorderedSampleWindow)
        );
    }

    private sealed class BorderedSampleWindow : LightweaveWindow {
        public BorderedSampleWindow() {
            doCloseX = true;
            forcePause = false;
            closeOnClickedOutside = false;
        }

        public override Vector2 InitialSize => new Vector2(480f, 320f);
        protected override Vector2 MinWindowSize => new Vector2(320f, 200f);

        protected override LightweaveNode Header() {
            return WindowHeader.Create(
                title: (string)"CL_Playground_Window_Sample_Title".Translate(),
                onClose: () => Close()
            );
        }

        protected override LightweaveNode Body() {
            return WindowBody.Create(
                style: new Style { Padding = EdgeInsets.All(SpacingScale.Md) },
                children: c => c.Add(Typography.Typography.Text.Create(
                    (string)"CL_Playground_Window_Sample_Bordered_Body".Translate(),
                    wrap: true,
                    style: new Style { FontFamily = FontRole.Body, FontSize = new Rem(0.9375f), TextColor = ThemeSlot.TextSecondary }
                ))
            );
        }
    }

    private sealed class BorderlessSampleWindow : LightweaveWindow {
        public BorderlessSampleWindow() {
            doCloseX = true;
            forcePause = false;
            closeOnClickedOutside = false;
        }

        public override Vector2 InitialSize => new Vector2(480f, 320f);
        protected override bool DrawBorder => false;
        protected override Vector2 MinWindowSize => new Vector2(320f, 200f);

        protected override LightweaveNode Header() {
            return WindowHeader.Create(
                title: (string)"CL_Playground_Window_Sample_Title".Translate(),
                onClose: () => Close()
            );
        }

        protected override LightweaveNode Body() {
            return WindowBody.Create(
                style: new Style { Padding = EdgeInsets.All(SpacingScale.Md), Background = BackgroundSpec.Of(ThemeSlot.SurfacePrimary) },
                children: c => c.Add(Typography.Typography.Text.Create(
                    (string)"CL_Playground_Window_Sample_Borderless_Body".Translate(),
                    wrap: true,
                    style: new Style { FontFamily = FontRole.Body, FontSize = new Rem(0.9375f), TextColor = ThemeSlot.TextSecondary }
                ))
            );
        }
    }

    private sealed class FixedSizeSampleWindow : LightweaveWindow {
        public FixedSizeSampleWindow() {
            doCloseX = true;
            forcePause = false;
            closeOnClickedOutside = false;
        }

        public override Vector2 InitialSize => new Vector2(420f, 280f);
        protected override bool EdgeResizable => false;
        protected override Vector2 MinWindowSize => new Vector2(420f, 280f);

        protected override LightweaveNode Header() {
            return WindowHeader.Create(
                title: (string)"CL_Playground_Window_Sample_Title".Translate(),
                onClose: () => Close()
            );
        }

        protected override LightweaveNode Body() {
            return WindowBody.Create(
                style: new Style { Padding = EdgeInsets.All(SpacingScale.Md) },
                children: c => c.Add(Typography.Typography.Text.Create(
                    (string)"CL_Playground_Window_Sample_Fixed_Body".Translate(),
                    wrap: true,
                    style: new Style { FontFamily = FontRole.Body, FontSize = new Rem(0.9375f), TextColor = ThemeSlot.TextSecondary }
                ))
            );
        }
    }

    private sealed class LargeSampleWindow : LightweaveWindow {
        public LargeSampleWindow() {
            doCloseX = true;
            forcePause = false;
            closeOnClickedOutside = false;
        }

        public override Vector2 InitialSize => new Vector2(720f, 520f);
        protected override Vector2 MinWindowSize => new Vector2(520f, 360f);

        protected override LightweaveNode Header() {
            return WindowHeader.Create(
                title: (string)"CL_Playground_Window_Sample_Title".Translate(),
                onClose: () => Close()
            );
        }

        protected override LightweaveNode Body() {
            return WindowBody.Create(
                style: new Style { Padding = EdgeInsets.All(SpacingScale.Md) },
                children: c => c.Add(Typography.Typography.Text.Create(
                    (string)"CL_Playground_Window_Sample_Large_Body".Translate(),
                    wrap: true,
                    style: new Style { FontFamily = FontRole.Body, FontSize = new Rem(0.9375f), TextColor = ThemeSlot.TextSecondary }
                ))
            );
        }
    }

    private sealed class DialogSampleWindow : LightweaveWindow {
        public DialogSampleWindow() {
            doCloseX = true;
            forcePause = false;
            closeOnClickedOutside = false;
        }

        public override Vector2 InitialSize => new Vector2(480f, 280f);
        protected override Vector2 MinWindowSize => new Vector2(360f, 220f);

        protected override LightweaveNode Header() {
            return WindowHeader.Create(
                title: (string)"CL_Playground_Window_Sample_Dialog_Title".Translate(),
                onClose: () => Close()
            );
        }

        protected override LightweaveNode Body() {
            return WindowBody.Create(
                style: new Style { Padding = EdgeInsets.All(SpacingScale.Md) },
                children: c => c.Add(Typography.Typography.Text.Create(
                    (string)"CL_Playground_Window_Sample_Dialog_Body".Translate(),
                    wrap: true,
                    style: new Style { FontFamily = FontRole.Body, FontSize = new Rem(0.9375f), TextColor = ThemeSlot.TextSecondary }
                ))
            );
        }

       protected override LightweaveNode Footer() {
            return WindowFooter.Create(
                children: c => c.Add(HStack.Create(
                    SpacingScale.Xxs,
                    r => {
                        r.AddFlex(Spacer.Flex());
                        r.Add(
                            Button.Create(
                                (string)"CL_Playground_Window_Cancel".Translate(),
                                () => Close(),
                                ButtonVariant.Secondary,
                                style: new Style { Width = Length.Stretch }
                            ),
                            96f
                        );
                        r.Add(
                            Button.Create(
                                (string)"CL_Playground_Window_Confirm".Translate(),
                                () => Close(),
                                ButtonVariant.Primary,
                                style: new Style { Width = Length.Stretch }
                            ),
                            96f
                        );
                    }
                ))
            );
        }
    }

    private sealed class StatusBarSampleWindow : LightweaveWindow {
        public StatusBarSampleWindow() {
            doCloseX = true;
            forcePause = false;
            closeOnClickedOutside = false;
        }

        public override Vector2 InitialSize => new Vector2(560f, 360f);
        protected override Vector2 MinWindowSize => new Vector2(360f, 220f);

        protected override LightweaveNode Header() {
            return WindowHeader.Create(
                title: (string)"CL_Playground_Window_Sample_Status_Title".Translate(),
                onClose: () => Close()
            );
        }

        protected override LightweaveNode Body() {
            return WindowBody.Create(
                style: new Style { Padding = EdgeInsets.All(SpacingScale.Md) },
                children: c => c.Add(Typography.Typography.Text.Create(
                    (string)"CL_Playground_Window_Sample_Status_Body".Translate(),
                    wrap: true,
                    style: new Style { FontFamily = FontRole.Body, FontSize = new Rem(0.9375f), TextColor = ThemeSlot.TextSecondary }
                ))
            );
        }

        protected override LightweaveNode Footer() {
            return WindowFooter.Create(
                showResizeGrip: true,
                children: c => c.Add(HStack.Create(
                    SpacingScale.Sm,
                    r => {
                        r.AddFlex(Typography.Typography.Caption.Create((string)"CL_Playground_Window_Sample_Status_Indicator".Translate()));
                    }
                ))
            );
        }
    }
}
