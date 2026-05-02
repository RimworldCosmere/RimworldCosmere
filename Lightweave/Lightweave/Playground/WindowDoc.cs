using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Runtime;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Playground;

[Doc(
    Id = "window",
    Summary = "Top-level resizable Lightweave window with drag region and close affordance.",
    WhenToUse = "Host a tool, dialog, or panel as its own movable window in the WindowStack.",
    SourcePath = "Lightweave/Lightweave/Runtime/LightweaveWindow.cs"
)]
public static class WindowDoc {
    [DocVariant("CC_Playground_Window_Bordered")]
    public static DocSample DocsBordered() {
        return new DocSample(
            Button.Create(
                (string)"CC_Playground_Window_Open".Translate(),
                () => Find.WindowStack.Add(new PlaygroundSampleWindow(
                    "CC_Playground_Window_Sample_Title",
                    "CC_Playground_Window_Sample_Bordered_Body",
                    new Vector2(480f, 320f)
                )),
                ButtonVariant.Secondary
            )
        );
    }

    [DocVariant("CC_Playground_Window_Borderless")]
    public static DocSample DocsBorderless() {
        return new DocSample(
            Button.Create(
                (string)"CC_Playground_Window_Open".Translate(),
                () => Find.WindowStack.Add(new PlaygroundSampleWindow(
                    "CC_Playground_Window_Sample_Title",
                    "CC_Playground_Window_Sample_Borderless_Body",
                    new Vector2(480f, 320f),
                    drawBorder: false
                )),
                ButtonVariant.Secondary
            )
        );
    }

    [DocVariant("CC_Playground_Window_FixedSize")]
    public static DocSample DocsFixedSize() {
        return new DocSample(
            Button.Create(
                (string)"CC_Playground_Window_Open".Translate(),
                () => Find.WindowStack.Add(new PlaygroundSampleWindow(
                    "CC_Playground_Window_Sample_Title",
                    "CC_Playground_Window_Sample_Fixed_Body",
                    new Vector2(420f, 280f),
                    edgeResizable: false
                )),
                ButtonVariant.Secondary
            )
        );
    }

    [DocVariant("CC_Playground_Window_Large")]
    public static DocSample DocsLarge() {
        return new DocSample(
            Button.Create(
                (string)"CC_Playground_Window_Open".Translate(),
                () => Find.WindowStack.Add(new PlaygroundSampleWindow(
                    "CC_Playground_Window_Sample_Title",
                    "CC_Playground_Window_Sample_Large_Body",
                    new Vector2(720f, 520f),
                    minSize: new Vector2(520f, 360f)
                )),
                ButtonVariant.Secondary
            )
        );
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(
            Button.Create(
                (string)"CC_Playground_Window_Open".Translate(),
                () => Find.WindowStack.Add(new PlaygroundSampleWindow(
                    "CC_Playground_Window_Sample_Title",
                    "CC_Playground_Window_Sample_Bordered_Body",
                    new Vector2(480f, 320f)
                )),
                ButtonVariant.Secondary
            )
        );
    }
}
