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
            repoRoot = DetectRepoRoot() ?? "";
            return repoRoot;
        }
    }


    public static bool SourceFileExists(string sourcePath) {
        try {
            string root = RepoRoot;
            if (string.IsNullOrEmpty(root)) return false;
            string abs = Path.Combine(root, sourcePath);
            return File.Exists(abs);
        }
        catch (Exception) {
            return false;
        }
    }

    private static string? DetectRepoRoot() {
        try {
            string asmPath = typeof(SourceLink).Assembly.Location;
            if (!string.IsNullOrEmpty(asmPath)) {
                string asmDir = Path.GetDirectoryName(asmPath) ?? "";
                string up2 = Path.GetFullPath(Path.Combine(asmDir, "..", ".."));
                if (Directory.Exists(Path.Combine(up2, "CosmereCore"))) {
                    return up2;
                }
            }
        }
        catch (Exception) {
        }

        try {
            string modsParent = Path.GetFullPath(Path.Combine(GenFilePaths.ModsFolderPath, "..", ".."));
            if (Directory.Exists(Path.Combine(modsParent, "CosmereCore"))) {
                return modsParent;
            }
        }
        catch (Exception) {
        }

        return null;
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
            string root = RepoRoot;
            if (string.IsNullOrEmpty(root)) {
                LightweaveLog.Warning("SourceLink open failed: repo root could not be detected, falling back to GitHub");
                Application.OpenURL(BuildGithubUrl(sourcePath));
                return;
            }

            string abs = Path.Combine(root, sourcePath);
            string unix = abs.Replace('\\', '/');
            if (Application.platform != RuntimePlatform.WindowsPlayer && Application.platform != RuntimePlatform.WindowsEditor) {
                if (!unix.StartsWith("/")) {
                    unix = "/" + unix;
                }
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
