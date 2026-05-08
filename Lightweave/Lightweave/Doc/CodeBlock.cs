using System;
using System.Runtime.CompilerServices;
using System.Text;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Lightweave.Doc;

public static partial class Doc {
    private const int CollapsedLineCount = 3;

    private sealed class ParsedCode {
        public string Normalized = string.Empty;
        public string[] CodeLines = Array.Empty<string>();
        public string[] HighlightedLines = Array.Empty<string>();
        public string CopyText = string.Empty;
        public string MaxLineNumber = "0";
        public Dictionary<long, float[]> HeightsByKey = new Dictionary<long, float[]>();
    }

    private static readonly Dictionary<string, ParsedCode> ParsedCodeCache = new Dictionary<string, ParsedCode>();

    private static ParsedCode GetParsed(string? code) {
        string raw = code ?? string.Empty;
        if (ParsedCodeCache.TryGetValue(raw, out ParsedCode? cached)) {
            return cached;
        }

        ParsedCode entry = new ParsedCode();
        entry.Normalized = raw.Replace("\r\n", "\n").TrimEnd('\n');
        entry.CodeLines = entry.Normalized.Length == 0 ? new[] { string.Empty } : entry.Normalized.Split('\n');
        entry.CopyText = StripUsingHeader(entry.Normalized);
        int total = entry.CodeLines.Length;
        entry.HighlightedLines = new string[total];
        for (int i = 0; i < total; i++) {
            entry.HighlightedLines[i] = SyntaxHighlight(entry.CodeLines[i].Replace("<", "<​"));
        }

        entry.MaxLineNumber = total.ToString();
        ParsedCodeCache[raw] = entry;
        return entry;
    }

    public static LightweaveNode CodeBlock(
        string code,
        bool flat = false,
        bool collapsible = true,
        string? key = null,
        RadiusSpec? backgroundRadius = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New("Doc.CodeBlock", line, file);

        ParsedCode parsed = GetParsed(code);
        string[] highlightedLines = parsed.HighlightedLines;
        string copyText = parsed.CopyText;
        int totalLines = highlightedLines.Length;
        string maxLineNumber = parsed.MaxLineNumber;

        string keySuffix = key ?? string.Empty;
        Hooks.Hooks.StateHandle<bool> expanded = Hooks.Hooks.UseState(false, line, file + "#expanded" + keySuffix);
        Hooks.Hooks.StateHandle<float> copiedAt = Hooks.Hooks.UseState(-1f, line, file + "#copied" + keySuffix);

        bool canCollapse = collapsible && totalLines > CollapsedLineCount;
        bool isCollapsed = canCollapse && !expanded.Value;
        int visibleLineCount = isCollapsed ? CollapsedLineCount : totalLines;

        float fontPx = new Rem(0.8125f).ToFontPx();
        float lineHeightPx = new Rem(1.4f).ToPixels();
        float padXPx = new Rem(0.875f).ToPixels();
        float padYPx = new Rem(0.875f).ToPixels();
        float gutterGapPx = new Rem(0.625f).ToPixels();
        float copyBtnSizePx = new Rem(1.5f).ToPixels();
        float copyBtnPadPx = new Rem(0.5f).ToPixels();
        float viewBtnHeightPx = new Rem(2f).ToPixels();
        float viewBtnGapPx = new Rem(0.625f).ToPixels();
        Rem radius = new Rem(0.375f);
        Rem borderThickness = new Rem(1f / 16f);

        float overlayHeight = canCollapse && isCollapsed ? viewBtnHeightPx + viewBtnGapPx : 0f;

        float[] GetLineHeights(float availableWidth) {
            int widthBucket = Mathf.RoundToInt(availableWidth);
            long key = ((long)visibleLineCount << 32) | (uint)widthBucket;
            if (parsed.HeightsByKey.TryGetValue(key, out float[]? cached)) {
                return cached;
            }

            float[] heights = ComputeLineHeights(highlightedLines, visibleLineCount, availableWidth, fontPx, lineHeightPx, padXPx, gutterGapPx, maxLineNumber);
            parsed.HeightsByKey[key] = heights;
            return heights;
        }

        node.Measure = availableWidth => {
            float[] heights = GetLineHeights(availableWidth);
            float bodyH = padYPx * 2f;
            for (int i = 0; i < visibleLineCount; i++) {
                bodyH += heights[i];
            }
            return bodyH + overlayHeight;
        };

        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            BackgroundSpec bg = BackgroundSpec.Of(ThemeSlot.SurfaceSunken);
            BorderSpec? border = flat ? null : BorderSpec.All(borderThickness, ThemeSlot.BorderSubtle);
            RadiusSpec? rad = backgroundRadius ?? (flat ? null : RadiusSpec.All(radius));
            PaintBox.Draw(rect, bg, border, rad);

            Font mono = theme.GetFont(FontRole.Mono);
            int fontSize = Mathf.RoundToInt(fontPx);
            GUIStyle sharedStyle = GuiStyleCache.GetOrCreate(mono, fontSize);
            sharedStyle.wordWrap = false;
            sharedStyle.clipping = TextClipping.Clip;

            sharedStyle.alignment = TextAnchor.MiddleRight;
            sharedStyle.richText = false;
            float gutterTextWidth = sharedStyle.CalcSize(new GUIContent(maxLineNumber)).x;
            float gutterColX = rect.x + padXPx;
            float gutterColWidth = gutterTextWidth;
            float dividerX = gutterColX + gutterColWidth + gutterGapPx * 0.5f;
            float codeX = dividerX + gutterGapPx * 0.5f;
            float codeWidth = Mathf.Max(0f, rect.xMax - padXPx - codeX);

            float[] lineHeights = GetLineHeights(rect.width);

            Color savedColor = GUI.color;
            Color gutterColor = theme.GetColor(ThemeSlot.TextMuted);
            gutterColor.a *= 0.7f;
            Color dividerColor = theme.GetColor(ThemeSlot.BorderSubtle);

            float startY = rect.y + padYPx;
            float dividerHeight = 0f;
            for (int i = 0; i < visibleLineCount; i++) {
                dividerHeight += lineHeights[i];
            }

            Rect dividerRect = new Rect(dividerX, startY, 1f, dividerHeight);
            GUI.color = dividerColor;
            GUI.DrawTexture(dividerRect, Texture2D.whiteTexture);
            GUI.color = savedColor;

            float ly = startY;
            for (int i = 0; i < visibleLineCount; i++) {
                float h = lineHeights[i];
                Rect numberRect = new Rect(gutterColX, ly, gutterColWidth, lineHeightPx);
                Rect lineRect = new Rect(codeX, ly, codeWidth, h);

                GUI.color = gutterColor;
                sharedStyle.alignment = TextAnchor.MiddleRight;
                sharedStyle.richText = false;
                sharedStyle.wordWrap = false;
                sharedStyle.clipping = TextClipping.Clip;
                GUI.Label(RectSnap.Snap(numberRect), (i + 1).ToString(), sharedStyle);

                GUI.color = Color.white;
                sharedStyle.alignment = TextAnchor.UpperLeft;
                sharedStyle.richText = true;
                sharedStyle.wordWrap = true;
                sharedStyle.clipping = TextClipping.Clip;
                GUI.Label(RectSnap.Snap(lineRect), highlightedLines[i], sharedStyle);

                ly += h;
            }

            GUI.color = savedColor;

            DrawCopyButton(rect, theme, copyBtnSizePx, copyBtnPadPx, copyText, copiedAt);

            if (canCollapse && isCollapsed) {
                DrawViewCodeOverlay(rect, theme, startY, dividerHeight, viewBtnHeightPx, viewBtnGapPx, expanded);
            }
        };

        return node;
    }

    private static float[] ComputeLineHeights(
        string[] highlightedLines,
        int visibleLineCount,
        float availableWidth,
        float fontPx,
        float minLineHeightPx,
        float padXPx,
        float gutterGapPx,
        string maxLineNumber
    ) {
        float[] heights = new float[visibleLineCount];

        Theme.Theme theme = RenderContext.Current.Theme;
        Font mono = theme.GetFont(FontRole.Mono);
        int fontSize = Mathf.RoundToInt(fontPx);
        GUIStyle style = GuiStyleCache.GetOrCreate(mono, fontSize);

        style.wordWrap = false;
        style.richText = false;
        style.alignment = TextAnchor.MiddleRight;
        float gutterTextWidth = style.CalcSize(new GUIContent(maxLineNumber)).x;
        float codeWidth = Mathf.Max(1f, availableWidth - padXPx * 2f - gutterTextWidth - gutterGapPx);

        style.wordWrap = true;
        style.richText = true;
        style.alignment = TextAnchor.UpperLeft;

        for (int i = 0; i < visibleLineCount; i++) {
            string content = highlightedLines[i];
            if (string.IsNullOrEmpty(content)) {
                heights[i] = minLineHeightPx;
                continue;
            }

            float h = style.CalcHeight(new GUIContent(content), codeWidth);
            heights[i] = Mathf.Max(minLineHeightPx, h);
        }

        return heights;
    }

    private static void DrawCopyButton(
        Rect blockRect,
        Theme.Theme theme,
        float btnSize,
        float padding,
        string codeToCopy,
        Hooks.Hooks.StateHandle<float> copiedAt
    ) {
        Rect btnRect = new Rect(
            blockRect.xMax - btnSize - padding,
            blockRect.y + padding,
            btnSize,
            btnSize
        );

        Event e = Event.current;
        bool hover = btnRect.Contains(e.mousePosition);
        float now = Time.realtimeSinceStartup;
        float copyFlashDuration = 1.2f;
        bool justCopied = copiedAt.Value > 0f && (now - copiedAt.Value) < copyFlashDuration;

        Color savedColor = GUI.color;

        if (hover) {
            Color hoverBg = theme.GetColor(ThemeSlot.SurfaceRaised);
            hoverBg.a *= 0.85f;
            GUI.color = hoverBg;
            GUI.DrawTexture(btnRect, Texture2D.whiteTexture);
            GUI.color = savedColor;
        }

        Color iconColor = justCopied
            ? theme.GetColor(ThemeSlot.StatusSuccess)
            : hover
                ? theme.GetColor(ThemeSlot.TextPrimary)
                : theme.GetColor(ThemeSlot.TextMuted);

        Texture2D? icon = DocTextures.Copy;
        if (icon != null) {
            float iconInset = btnSize * 0.22f;
            Rect iconRect = new Rect(
                btnRect.x + iconInset,
                btnRect.y + iconInset,
                btnSize - iconInset * 2f,
                btnSize - iconInset * 2f
            );
            GUI.color = iconColor;
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
            GUI.color = savedColor;
        }
        else {
            DrawProceduralCopyIcon(btnRect, iconColor);
        }

        if (hover) {
            string tipKey = justCopied
                ? "CC_Playground_Panel_CopyCode_Copied"
                : "CC_Playground_Panel_CopyCode";
            TooltipHandler.TipRegion(btnRect, (string)tipKey.Translate());
            MouseoverSounds.DoRegion(btnRect, SoundDefOf.Mouseover_Standard);
        }

        if (e.type == EventType.MouseUp && e.button == 0 && hover) {
            GUIUtility.systemCopyBuffer = codeToCopy;
            copiedAt.Value = now;
            SoundDefOf.Click.PlayOneShotOnCamera();
            e.Use();
        }
    }

    private static void DrawProceduralCopyIcon(Rect btnRect, Color color) {
        Color savedColor = GUI.color;
        float inset = btnRect.width * 0.22f;
        float strokeW = Mathf.Max(1f, btnRect.width * 0.07f);
        float backOffset = btnRect.width * 0.18f;
        Rect frontSquare = new Rect(
            btnRect.x + inset + backOffset * 0.4f,
            btnRect.y + inset + backOffset * 0.6f,
            btnRect.width - inset * 2f - backOffset * 0.4f,
            btnRect.height - inset * 2f - backOffset * 0.6f
        );
        Rect backSquare = new Rect(
            btnRect.x + inset - backOffset * 0.2f,
            btnRect.y + inset - backOffset * 0.2f,
            frontSquare.width,
            frontSquare.height
        );

        GUI.color = color;
        DrawRectOutline(backSquare, strokeW);
        GUI.DrawTexture(new Rect(frontSquare.x, frontSquare.y, frontSquare.width, frontSquare.height), Texture2D.whiteTexture);
        Color innerFill = color;
        innerFill.r = Mathf.Lerp(innerFill.r, 0f, 0.85f);
        innerFill.g = Mathf.Lerp(innerFill.g, 0f, 0.85f);
        innerFill.b = Mathf.Lerp(innerFill.b, 0f, 0.85f);
        GUI.color = innerFill;
        GUI.DrawTexture(frontSquare.ContractedBy(strokeW), Texture2D.whiteTexture);
        GUI.color = savedColor;
    }

    private static void DrawRectOutline(Rect r, float thickness) {
        GUI.DrawTexture(new Rect(r.x, r.y, r.width, thickness), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(r.x, r.yMax - thickness, r.width, thickness), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(r.x, r.y, thickness, r.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(r.xMax - thickness, r.y, thickness, r.height), Texture2D.whiteTexture);
    }

    private static void DrawViewCodeOverlay(
        Rect blockRect,
        Theme.Theme theme,
        float bodyStartY,
        float bodyHeight,
        float btnHeight,
        float gap,
        Hooks.Hooks.StateHandle<bool> expanded
    ) {
        float fadeHeight = Mathf.Min(bodyHeight, new Rem(2.5f).ToPixels());
        Rect fadeRect = new Rect(
            blockRect.x + 2f,
            bodyStartY + bodyHeight - fadeHeight,
            blockRect.width - 4f,
            fadeHeight
        );
        Color savedColor = GUI.color;
        Color fade = theme.GetColor(ThemeSlot.SurfaceSunken);
        for (int i = 0; i < 8; i++) {
            float t = i / 7f;
            fade.a = Mathf.Lerp(0f, 0.95f, t);
            GUI.color = fade;
            float strip = fadeHeight / 8f;
            GUI.DrawTexture(new Rect(fadeRect.x, fadeRect.y + i * strip, fadeRect.width, strip), Texture2D.whiteTexture);
        }

        GUI.color = savedColor;

        float btnWidth = new Rem(7f).ToPixels();
        Rect btnRect = new Rect(
            blockRect.x + (blockRect.width - btnWidth) * 0.5f,
            bodyStartY + bodyHeight + gap,
            btnWidth,
            btnHeight
        );

        Event e = Event.current;
        bool hover = btnRect.Contains(e.mousePosition);

        Color btnBg = hover ? theme.GetColor(ThemeSlot.SurfaceAccent) : theme.GetColor(ThemeSlot.SurfaceRaised);
        GUI.color = btnBg;
        GUI.DrawTexture(btnRect, Texture2D.whiteTexture);
        GUI.color = savedColor;

        Color borderColor = theme.GetColor(ThemeSlot.BorderDefault);
        GUI.color = borderColor;
        DrawRectOutline(btnRect, 1f);
        GUI.color = savedColor;

        Font font = theme.GetFont(FontRole.Body);
        int btnFontSize = Mathf.RoundToInt(new Rem(0.875f).ToFontPx());
        GUIStyle btnStyle = GuiStyleCache.GetOrCreate(font, btnFontSize, FontStyle.Bold);
        btnStyle.alignment = TextAnchor.MiddleCenter;
        btnStyle.wordWrap = false;
        btnStyle.clipping = TextClipping.Clip;
        btnStyle.richText = false;

        GUI.color = theme.GetColor(ThemeSlot.TextPrimary);
        GUI.Label(RectSnap.Snap(btnRect), (string)"CC_Playground_Panel_ViewCode".Translate(), btnStyle);
        GUI.color = savedColor;

        if (hover) {
            MouseoverSounds.DoRegion(btnRect, SoundDefOf.Mouseover_Standard);
        }

        if (e.type == EventType.MouseUp && e.button == 0 && hover) {
            expanded.Value = true;
            SoundDefOf.Click.PlayOneShotOnCamera();
            e.Use();
        }
    }

    private static string StripUsingHeader(string code) {
        if (string.IsNullOrEmpty(code)) {
            return code;
        }

        int idx = 0;
        int n = code.Length;
        while (idx < n) {
            int lineEnd = code.IndexOf('\n', idx);
            int lineStop = lineEnd < 0 ? n : lineEnd;
            string ln = code.Substring(idx, lineStop - idx);
            string trimmed = ln.TrimStart();
            if (trimmed.Length == 0) {
                int afterBlank = lineEnd < 0 ? n : lineEnd + 1;
                return afterBlank < n ? code.Substring(afterBlank) : string.Empty;
            }

            if (!trimmed.StartsWith("using ", StringComparison.Ordinal)) {
                return code;
            }

            if (lineEnd < 0) {
                return string.Empty;
            }

            idx = lineEnd + 1;
        }

        return string.Empty;
    }

    private static string SyntaxHighlight(string line) {
        if (string.IsNullOrEmpty(line)) {
            return line;
        }

        StringBuilder sb = new StringBuilder(line.Length + 16);
        int i = 0;
        int n = line.Length;
        while (i < n) {
            char c = line[i];

            if (c == '/' && i + 1 < n && line[i + 1] == '/') {
                AppendColored(sb, line.Substring(i), SyntaxColors.Comment);
                return sb.ToString();
            }

            if (c == '"') {
                int start = i;
                i++;
                while (i < n) {
                    if (line[i] == '\\' && i + 1 < n) {
                        i += 2;
                        continue;
                    }

                    if (line[i] == '"') {
                        i++;
                        break;
                    }

                    i++;
                }

                AppendColored(sb, line.Substring(start, i - start), SyntaxColors.String);
                continue;
            }

            if (c == '\'' && i + 2 < n) {
                int start = i;
                i++;
                if (line[i] == '\\' && i + 1 < n) {
                    i += 2;
                }
                else {
                    i++;
                }

                if (i < n && line[i] == '\'') {
                    i++;
                    AppendColored(sb, line.Substring(start, i - start), SyntaxColors.String);
                    continue;
                }

                i = start;
                sb.Append(c);
                i++;
                continue;
            }

            if (char.IsDigit(c)) {
                int start = i;
                while (i < n && (char.IsDigit(line[i]) || line[i] == '.' || line[i] == 'f' || line[i] == 'F')) {
                    i++;
                }

                AppendColored(sb, line.Substring(start, i - start), SyntaxColors.Number);
                continue;
            }

            if (char.IsLetter(c) || c == '_') {
                int start = i;
                while (i < n && (char.IsLetterOrDigit(line[i]) || line[i] == '_')) {
                    i++;
                }

                string ident = line.Substring(start, i - start);
                string color;
                if (IsKeyword(ident)) {
                    color = SyntaxColors.Keyword;
                }
                else if (ident.Length > 0 && char.IsUpper(ident[0])) {
                    bool isMethodCall = i < n && line[i] == '(';
                    color = isMethodCall ? SyntaxColors.Method : SyntaxColors.Type;
                }
                else {
                    bool isMethodCall = i < n && line[i] == '(';
                    color = isMethodCall ? SyntaxColors.Method : SyntaxColors.Default;
                }

                if (color == SyntaxColors.Default) {
                    sb.Append(ident);
                }
                else {
                    AppendColored(sb, ident, color);
                }

                continue;
            }

            if (c == '<') {
                sb.Append('⟨');
            }
            else if (c == '>') {
                sb.Append('⟩');
            }
            else {
                sb.Append(c);
            }

            i++;
        }

        return sb.ToString();
    }

    private static void AppendColored(StringBuilder sb, string text, string hex) {
        sb.Append("<color=#");
        sb.Append(hex);
        sb.Append('>');
        AppendEscaped(sb, text);
        sb.Append("</color>");
    }

    private static void AppendEscaped(StringBuilder sb, string text) {
        for (int j = 0; j < text.Length; j++) {
            char ch = text[j];
            if (ch == '<') {
                sb.Append('⟨');
            }
            else if (ch == '>') {
                sb.Append('⟩');
            }
            else {
                sb.Append(ch);
            }
        }
    }

    private static bool IsKeyword(string s) {
        switch (s) {
            case "abstract":
            case "as":
            case "base":
            case "bool":
            case "break":
            case "byte":
            case "case":
            case "catch":
            case "char":
            case "class":
            case "const":
            case "continue":
            case "decimal":
            case "default":
            case "delegate":
            case "do":
            case "double":
            case "else":
            case "enum":
            case "event":
            case "explicit":
            case "extern":
            case "false":
            case "finally":
            case "fixed":
            case "float":
            case "for":
            case "foreach":
            case "goto":
            case "if":
            case "implicit":
            case "in":
            case "init":
            case "int":
            case "interface":
            case "internal":
            case "is":
            case "lock":
            case "long":
            case "namespace":
            case "new":
            case "null":
            case "object":
            case "operator":
            case "out":
            case "override":
            case "params":
            case "private":
            case "protected":
            case "public":
            case "readonly":
            case "ref":
            case "record":
            case "return":
            case "sbyte":
            case "sealed":
            case "short":
            case "sizeof":
            case "stackalloc":
            case "static":
            case "string":
            case "struct":
            case "switch":
            case "this":
            case "throw":
            case "true":
            case "try":
            case "typeof":
            case "uint":
            case "ulong":
            case "unchecked":
            case "unsafe":
            case "ushort":
            case "using":
            case "var":
            case "virtual":
            case "void":
            case "volatile":
            case "where":
            case "while":
            case "yield":
                return true;
            default:
                return false;
        }
    }

    private static class SyntaxColors {
        public const string Keyword = "569CD6";
        public const string String = "CE9178";
        public const string Number = "B5CEA8";
        public const string Type = "4EC9B0";
        public const string Method = "DCDCAA";
        public const string Comment = "6A9955";
        public const string Default = "default";
    }
}
