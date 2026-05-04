using System;
using System.IO;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Playground;

public static class SourceLink {
    private static string? repoRoot;

    private static string RepoRoot {
        get {
            if (repoRoot != null) return repoRoot;
            string guess = Path.GetFullPath(Path.Combine(GenFilePaths.ModsFolderPath, "..", ".."));
            string[] candidates = {
                "/mnt/c/Users/aequa/projects/RimworldCosmere/RimworldCosmere",
                @"C:\Users\aequa\projects\RimworldCosmere\RimworldCosmere",
                guess,
            };
            for (int i = 0; i < candidates.Length; i++) {
                if (!string.IsNullOrEmpty(candidates[i]) &&
                    Directory.Exists(Path.Combine(candidates[i], "CosmereCore"))) {
                    repoRoot = candidates[i];
                    return repoRoot;
                }
            }

            repoRoot = candidates[1];
            return repoRoot;
        }
    }

    public static LightweaveNode Create(
        string sourcePath,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        return LabelLink(
            "SourceLink:" + sourcePath,
            "CC_Playground_Panel_SourceLabel",
            "CC_Playground_Panel_SourceTooltip",
            () => TryOpenInEditor(sourcePath),
            line,
            file
        );
    }

    public static LightweaveNode GithubLink(
        string sourcePath,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        return LabelLink(
            "SourceLink.Github:" + sourcePath,
            "CC_Playground_Panel_GithubLabel",
            "CC_Playground_Panel_GithubTooltip",
            () => TryOpenInBrowser(sourcePath),
            line,
            file
        );
    }

    private static LightweaveNode LabelLink(
        string nodeId,
        string labelKey,
        string tooltipKey,
        Action onClick,
        int line,
        string file
    ) {
        LightweaveNode node = NodeBuilder.New(nodeId, line, file);
        node.PreferredHeight = new Rem(1.25f).ToPixels();
        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Font mono = theme.GetFont(FontRole.Mono);
            int pixelSize = Mathf.RoundToInt(new Rem(0.75f).ToFontPx());
            GUIStyle style = GuiStyleCache.GetOrCreate(mono, pixelSize);
            style.alignment = TextAnchor.MiddleLeft;

            Event e = Event.current;
            bool hovering = rect.Contains(e.mousePosition);
            ThemeSlot slot = hovering ? ThemeSlot.BorderFocus : ThemeSlot.SurfaceAccent;
            Color c = theme.GetColor(slot);

            string text = (string)labelKey.Translate();

            Color saved = GUI.color;
            GUI.color = c;
            GUI.Label(RectSnap.Snap(rect), text, style);
            GUI.color = saved;

            if (hovering) {
                Vector2 size = style.CalcSize(new GUIContent(text));
                float underlineY = rect.y + rect.height / 2f + size.y / 2f - 1f;
                float underlineWidth = Mathf.Min(size.x, rect.width);
                Rect underline = new Rect(rect.x, underlineY, underlineWidth, 1f);
                GUI.color = c;
                GUI.DrawTexture(underline, Texture2D.whiteTexture);
                GUI.color = saved;

                TooltipHandler.TipRegion(rect, (string)tooltipKey.Translate());
            }

            if (e.type == EventType.MouseUp && e.button == 0 && rect.Contains(e.mousePosition)) {
                onClick();
                e.Use();
            }
        };
        return node;
    }

    private static void TryOpenInEditor(string sourcePath) {
        try {
            string abs = Path.Combine(RepoRoot, sourcePath);
            string unix = abs.Replace('\\', '/');
            if (!unix.StartsWith("/")) {
                unix = "/" + unix;
            }

            Application.OpenURL("vscode://file" + unix);
        }
        catch (Exception ex) {
            LightweaveLog.Warning("SourceLink open failed: " + ex);
        }
    }

    internal static string BuildGithubUrl(string sourcePath) {
        string normalized = sourcePath.Replace('\\', '/').TrimStart('/');
        return "https://github.com/RimworldCosmere/RimworldCosmere/blob/main/" + normalized;
    }

    private static void TryOpenInBrowser(string sourcePath) {
        try {
            Application.OpenURL(BuildGithubUrl(sourcePath));
        }
        catch (Exception ex) {
            LightweaveLog.Warning("SourceLink browser open failed: " + ex);
        }
    }
}
