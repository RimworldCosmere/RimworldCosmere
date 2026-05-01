using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Playground;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;
using static Cosmere.Lightweave.Doc.DocChips;

namespace Cosmere.Lightweave.Feedback;

[Doc(
    Id = "spinner",
    Summary = "Animated indeterminate progress indicator.",
    WhenToUse = "Show that work is in progress when duration is unknown.",
    SourcePath = "CosmereCore/CosmereCore/Lightweave/Feedback/Spinner.cs"
)]
public static class Spinner {
    public static LightweaveNode Create(
        [DocParam("Diameter of the spinner. Defaults to 1.5rem.")]
        Rem size = default,
        [DocParam("Override the arc color. Defaults to SurfaceAccent.")]
        ThemeSlot? color = null,
        [DocParam("Time in seconds for one full rotation.")]
        float rotationPeriodSeconds = 0.8f,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        Rem resolvedSize = size.Equals(default) ? new Rem(1.5f) : size;
        ThemeSlot resolvedColor = color ?? ThemeSlot.SurfaceAccent;

        LightweaveNode node = NodeBuilder.New("Spinner", line, file);
        node.PreferredHeight = resolvedSize.ToPixels();

        node.Paint = (rect, paintChildren) => {
            AnimationClock.RegisterActive(RenderContext.Current.RootId);

            Theme.Theme theme = RenderContext.Current.Theme;
            Color arcColor = theme.GetColor(resolvedColor);

            float sizePx = resolvedSize.ToPixels();
            float cx = rect.x + rect.width * 0.5f;
            float cy = rect.y + rect.height * 0.5f;
            float radius = sizePx * 0.5f;
            float lineWidth = Mathf.Max(2f, sizePx * 0.12f);

            float period = rotationPeriodSeconds > 0f ? rotationPeriodSeconds : 0.8f;
            float angle = Time.unscaledTime % period / period * 360f;

            // Draw a 270-degree arc as 54 short line segments
            int segmentCount = 54;
            float arcSpan = 270f;
            float segStep = arcSpan / segmentCount;

            Color saved = GUI.color;
            for (int i = 0; i < segmentCount; i++) {
                float a0 = angle + i * segStep;
                float a1 = angle + (i + 1) * segStep;
                float t = (float)i / segmentCount;

                float rad0 = a0 * Mathf.Deg2Rad;
                float rad1 = a1 * Mathf.Deg2Rad;

                Vector2 p0 = new Vector2(cx + Mathf.Sin(rad0) * radius, cy - Mathf.Cos(rad0) * radius);
                Vector2 p1 = new Vector2(cx + Mathf.Sin(rad1) * radius, cy - Mathf.Cos(rad1) * radius);

                float alpha = Mathf.Lerp(0.15f, 1f, t);
                GUI.color = new Color(arcColor.r, arcColor.g, arcColor.b, arcColor.a * alpha);
                Widgets.DrawLine(p0, p1, GUI.color, lineWidth);
            }

            GUI.color = saved;
            paintChildren();
        };

        return node;
    }

    [DocVariant("CC_Playground_Label_Small")]
    public static DocSample DocsSmall() {
        return new DocSample(CenterFixed(Spinner.Create(new Rem(1f)), 24f, 24f));
    }

    [DocVariant("CC_Playground_Label_Medium")]
    public static DocSample DocsMedium() {
        return new DocSample(CenterFixed(Spinner.Create(new Rem(1.5f)), 32f, 32f));
    }

    [DocVariant("CC_Playground_Label_Large")]
    public static DocSample DocsLarge() {
        return new DocSample(CenterFixed(Spinner.Create(new Rem(2f)), 44f, 44f));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(CenterFixed(Spinner.Create(new Rem(1.5f)), 32f, 32f));
    }
}