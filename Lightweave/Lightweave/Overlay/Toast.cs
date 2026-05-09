using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;
using static Cosmere.Lightweave.Hooks.Hooks;
using Cosmere.Lightweave.Layout;

namespace Cosmere.Lightweave.Overlay;

public enum ToastKind {
    Info,
    Success,
    Warning,
    Danger,
}

public enum ToastPosition {
    TopLeft,
    TopCenter,
    TopRight,
    MiddleLeft,
    MiddleCenter,
    MiddleRight,
    BottomLeft,
    BottomCenter,
    BottomRight,
}

public enum ToastTarget {
    CurrentWindow,
    GameWindow,
}

public sealed record ToastMessage(
    string Id,
    string Text,
    ToastKind Kind = ToastKind.Info,
    float DurationSeconds = 4f
);

[Doc(
    Id = "toast",
    Summary = "Stacked transient notifications anchored to a window corner.",
    WhenToUse = "Confirm an action, surface a non-blocking warning, or report a result.",
    SourcePath = "Lightweave/Lightweave/Overlay/Toast.cs"
)]
public static class Toast {
    public static LightweaveNode Create(
        [DocParam("Active toast messages to render.")]
        IReadOnlyList<ToastMessage> toasts,
        [DocParam("Invoked with a toast id when it should be removed.")]
        Action<string> onDismiss,
        [DocParam("Anchor corner or edge for the stack.")]
        ToastPosition position = ToastPosition.BottomRight,
        [DocParam("Whether the stack is positioned in the host window or the full screen.")]
        ToastTarget target = ToastTarget.CurrentWindow,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        Hooks.Hooks.RefHandle<Dictionary<string, float>> spawnsRef = Hooks.Hooks.UseRef<Dictionary<string, float>>(
            new Dictionary<string, float>(),
            line,
            file
        );

        LightweaveNode node = NodeBuilder.New($"Toast:{position}", line, file);
        node.Paint = (_, _) => {
            Dictionary<string, float> spawns = spawnsRef.Current;
            float now = Time.unscaledTime;

            HashSet<string> presentIds = new HashSet<string>();
            for (int i = 0; i < toasts.Count; i++) {
                ToastMessage msg = toasts[i];
                presentIds.Add(msg.Id);
                if (!spawns.ContainsKey(msg.Id)) {
                    spawns[msg.Id] = now;
                }
            }

            List<string> stale = new List<string>();
            foreach (KeyValuePair<string, float> kvp in spawns) {
                if (!presentIds.Contains(kvp.Key)) {
                    stale.Add(kvp.Key);
                }
            }

            for (int i = 0; i < stale.Count; i++) {
                spawns.Remove(stale[i]);
            }

            List<string> expired = new List<string>();
            for (int i = 0; i < toasts.Count; i++) {
                ToastMessage msg = toasts[i];
                float spawnTime = spawns[msg.Id];
                if (now - spawnTime > msg.DurationSeconds) {
                    expired.Add(msg.Id);
                }
            }

            for (int i = 0; i < expired.Count; i++) {
                onDismiss?.Invoke(expired[i]);
            }

            if (toasts.Count == 0) {
                return;
            }

            float widthPx = new Rem(20f).ToPixels();
            float minHeightPx = new Rem(3.5f).ToPixels();
            float gapPx = SpacingScale.Sm.ToPixels();
            float marginPx = new Rem(1.5f).ToPixels();
            float fadePx = 0.2f;

            Rect host = target == ToastTarget.GameWindow
                ? new Rect(0f, 0f, Screen.width, Screen.height)
                : RenderContext.Current.RootRect;

            HorizontalAnchor hAnchor = HorizontalOf(position);
            VerticalAnchor vAnchor = VerticalOf(position);

            float anchorX;
            switch (hAnchor) {
                case HorizontalAnchor.Left:
                    anchorX = host.x + marginPx;
                    break;
                case HorizontalAnchor.Right:
                    anchorX = host.xMax - widthPx - marginPx;
                    break;
                default:
                    anchorX = host.x + (host.width - widthPx) / 2f;
                    break;
            }

            int count = toasts.Count;
            float[] heights = new float[count];
            float[] alphas = new float[count];
            ToastMessage[] snapshot = new ToastMessage[count];

            Theme.Theme theme = RenderContext.Current.Theme;
            Direction dir = RenderContext.Current.Direction;
            Font font = theme.GetFont(FontRole.Body);
            int textPixelSize = Mathf.RoundToInt(new Rem(0.9375f).ToFontPx());
            GUIStyle textStyle = GuiStyleCache.GetOrCreate(font, textPixelSize);
            textStyle.alignment = TextAnchor.UpperLeft;
            textStyle.wordWrap = true;

            float stripWidth = 8f;
            float padPx = SpacingScale.Md.ToPixels();
            float closeSize = new Rem(1.25f).ToPixels();

            float totalHeight = 0f;
            for (int i = 0; i < count; i++) {
                ToastMessage msg = toasts[i];
                snapshot[i] = msg;
                float spawnTime = spawns[msg.Id];
                float age = now - spawnTime;
                float remaining = msg.DurationSeconds - age;
                float fadeIn = fadePx > 0f ? Mathf.Clamp01(age / fadePx) : 1f;
                float fadeOut = fadePx > 0f ? Mathf.Clamp01(remaining / fadePx) : 1f;
                alphas[i] = fadeIn * fadeOut;

                float textLeft;
                float textRight;
                if (dir == Direction.Ltr) {
                    textLeft = padPx + stripWidth;
                    textRight = padPx + closeSize + padPx;
                }
                else {
                    textLeft = padPx + closeSize + padPx;
                    textRight = padPx + stripWidth;
                }

                float textWidth = Mathf.Max(0f, widthPx - textLeft - textRight);
                float textHeight = textStyle.CalcHeight(new GUIContent(msg.Text), textWidth);
                float rowHeight = Mathf.Max(minHeightPx, textHeight + padPx * 2f);
                heights[i] = rowHeight;
                totalHeight += rowHeight;
                if (i < count - 1) {
                    totalHeight += gapPx;
                }

                if (alphas[i] < 1f) {
                    AnimationClock.RegisterActive(RenderContext.Current.RootId);
                }
            }

            float startY;
            switch (vAnchor) {
                case VerticalAnchor.Top:
                    startY = host.y + marginPx;
                    break;
                case VerticalAnchor.Bottom:
                    startY = host.yMax - marginPx - totalHeight;
                    break;
                default:
                    startY = host.y + (host.height - totalHeight) / 2f;
                    break;
            }

            float[] positionsY = new float[count];
            float cursorY = startY;
            for (int i = 0; i < count; i++) {
                positionsY[i] = cursorY;
                cursorY += heights[i] + gapPx;
            }

            RenderContext.Current.PendingOverlays.Enqueue(() => {
                Color savedGuiColor = GUI.color;
                for (int i = 0; i < count; i++) {
                    ToastMessage msg = snapshot[i];
                    float alpha = alphas[i];
                    Rect toastRect = new Rect(anchorX, positionsY[i], widthPx, heights[i]);

                    Color savedInner = GUI.color;
                    GUI.color = new Color(1f, 1f, 1f, alpha);

                    BackgroundSpec bg = BackgroundSpec.Of(ThemeSlot.SurfaceRaised);
                    BorderSpec border = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderDefault);
                    RadiusSpec radius = RadiusSpec.All(new Rem(0.25f));
                    PaintBox.Draw(toastRect, bg, border, radius);

                    ThemeSlot stripSlot = StripSlot(msg.Kind);
                    Color stripColor = theme.GetColor(stripSlot);
                    Color stripWithAlpha = new Color(
                        stripColor.r,
                        stripColor.g,
                        stripColor.b,
                        stripColor.a * alpha
                    );
                    float stripX = dir == Direction.Ltr
                        ? toastRect.x
                        : toastRect.xMax - stripWidth;
                    Rect stripRect = new Rect(stripX, toastRect.y, stripWidth, toastRect.height);
                    GUI.color = new Color(1f, 1f, 1f, 1f);
                    PaintBox.Draw(stripRect, BackgroundSpec.Of(stripWithAlpha), null, null);
                    GUI.color = new Color(1f, 1f, 1f, alpha);

                    float textLeft;
                    float textRight;
                    if (dir == Direction.Ltr) {
                        textLeft = padPx + stripWidth;
                        textRight = padPx + closeSize + padPx;
                    }
                    else {
                        textLeft = padPx + closeSize + padPx;
                        textRight = padPx + stripWidth;
                    }

                    Rect textRect = new Rect(
                        toastRect.x + textLeft,
                        toastRect.y + padPx,
                        Mathf.Max(0f, toastRect.width - textLeft - textRight),
                        Mathf.Max(0f, toastRect.height - padPx * 2f)
                    );

                    Color savedTextColor = GUI.color;
                    Color textColor = theme.GetColor(ThemeSlot.TextPrimary);
                    GUI.color = new Color(textColor.r, textColor.g, textColor.b, textColor.a * alpha);
                    GUI.Label(RectSnap.Snap(textRect), msg.Text, textStyle);
                    GUI.color = savedTextColor;

                    float closeX = dir == Direction.Ltr
                        ? toastRect.xMax - padPx - closeSize
                        : toastRect.x + padPx;
                    Rect closeRect = new Rect(
                        closeX,
                        toastRect.y + padPx,
                        closeSize,
                        closeSize
                    );

                    Font closeFont = theme.GetFont(FontRole.BodyBold);
                    int closePixelSize = Mathf.RoundToInt(new Rem(1f).ToFontPx());
                    GUIStyle closeStyle = GuiStyleCache.GetOrCreate(closeFont, closePixelSize, FontStyle.Bold);
                    closeStyle.alignment = TextAnchor.MiddleCenter;

                    Color savedCloseColor = GUI.color;
                    Color closeColor = theme.GetColor(ThemeSlot.TextMuted);
                    GUI.color = new Color(closeColor.r, closeColor.g, closeColor.b, closeColor.a * alpha);
                    GUI.Label(RectSnap.Snap(closeRect), "×", closeStyle);
                    GUI.color = savedCloseColor;

                    GUI.color = savedInner;

                    Event e = Event.current;
                    if (e.type == EventType.MouseUp && e.button == 0 && closeRect.Contains(e.mousePosition)) {
                        onDismiss?.Invoke(msg.Id);
                        e.Use();
                    }
                }

                GUI.color = savedGuiColor;
            }
            );
        };
        return node;
    }

    private static HorizontalAnchor HorizontalOf(ToastPosition p) {
        switch (p) {
            case ToastPosition.TopLeft:
            case ToastPosition.MiddleLeft:
            case ToastPosition.BottomLeft:
                return HorizontalAnchor.Left;
            case ToastPosition.TopRight:
            case ToastPosition.MiddleRight:
            case ToastPosition.BottomRight:
                return HorizontalAnchor.Right;
            default:
                return HorizontalAnchor.Center;
        }
    }

    private static VerticalAnchor VerticalOf(ToastPosition p) {
        switch (p) {
            case ToastPosition.TopLeft:
            case ToastPosition.TopCenter:
            case ToastPosition.TopRight:
                return VerticalAnchor.Top;
            case ToastPosition.BottomLeft:
            case ToastPosition.BottomCenter:
            case ToastPosition.BottomRight:
                return VerticalAnchor.Bottom;
            default:
                return VerticalAnchor.Middle;
        }
    }

    private static ThemeSlot StripSlot(ToastKind kind) {
        switch (kind) {
            case ToastKind.Success:
                return ThemeSlot.StatusSuccess;
            case ToastKind.Warning:
                return ThemeSlot.StatusWarning;
            case ToastKind.Danger:
                return ThemeSlot.StatusDanger;
            default:
                return ThemeSlot.SurfaceAccent;
        }
    }

    private enum HorizontalAnchor {
        Left,
        Center,
        Right,
    }

    private enum VerticalAnchor {
        Top,
        Middle,
        Bottom,
    }

    private static LightweaveNode BuildVariantDemo(string buttonKey, string messageKey, ToastKind kind, ButtonVariant buttonVariant) {
        StateHandle<List<ToastMessage>> toasts = UseState(new List<ToastMessage>());
        RefHandle<int> counter = UseRef(0);

        void PushToast() {
            counter.Current = counter.Current + 1;
            List<ToastMessage> next = new List<ToastMessage>(toasts.Value) {
                new ToastMessage(
                    "playground-toast-" + counter.Current,
                    (string)messageKey.Translate(),
                    kind,
                    3f
                ),
            };
            toasts.Set(next);
        }

        void DismissToast(string id) {
            List<ToastMessage> next = new List<ToastMessage>();
            for (int i = 0; i < toasts.Value.Count; i++) {
                if (toasts.Value[i].Id != id) {
                    next.Add(toasts.Value[i]);
                }
            }

            toasts.Set(next);
        }

        LightweaveNode toastLayer = Create(toasts.Value, DismissToast);
        toastLayer.PreferredHeight = 0f;

        return Stack.Create(
            new Rem(0f),
            s => {
                s.Add(
                    Button.Create(
                        (string)buttonKey.Translate(),
                        () => PushToast(),
                        buttonVariant
                    )
                );
                s.Add(toastLayer);
            }
        );
    }

    [DocVariant("CL_Playground_Toast_Info")]
    public static DocSample DocsInfo() {
        return new DocSample(() => BuildVariantDemo(
            "CL_Playground_Toast_Info",
            "CL_Playground_Toast_Msg_Info",
            ToastKind.Info,
            ButtonVariant.Secondary
        ));
    }

    [DocVariant("CL_Playground_Toast_Success")]
    public static DocSample DocsSuccess() {
        return new DocSample(() => BuildVariantDemo(
            "CL_Playground_Toast_Success",
            "CL_Playground_Toast_Msg_Success",
            ToastKind.Success,
            ButtonVariant.Secondary
        ));
    }

    [DocVariant("CL_Playground_Toast_Warning")]
    public static DocSample DocsWarning() {
        return new DocSample(() => BuildVariantDemo(
            "CL_Playground_Toast_Warning",
            "CL_Playground_Toast_Msg_Warning",
            ToastKind.Warning,
            ButtonVariant.Secondary
        ));
    }

    [DocVariant("CL_Playground_Toast_Danger")]
    public static DocSample DocsDanger() {
        return new DocSample(() => BuildVariantDemo(
            "CL_Playground_Toast_Danger",
            "CL_Playground_Toast_Msg_Danger",
            ToastKind.Danger,
            ButtonVariant.Danger
        ));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => BuildVariantDemo(
            "CL_Playground_Toast_Info",
            "CL_Playground_Toast_Msg_Info",
            ToastKind.Info,
            ButtonVariant.Secondary
        ));
    }
}