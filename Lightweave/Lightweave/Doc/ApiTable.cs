using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.Doc;

public static partial class Doc {
    public static LightweaveNode ApiTable(
        IReadOnlyList<ApiParam> parameters,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New("Doc.ApiTable", line, file);

        float headerHeightPx = new Rem(2f).ToPixels();
        float rowPaddingY = new Rem(0.625f).ToPixels();
        float cellPaddingX = new Rem(0.75f).ToPixels();
        float bodyFontPx = new Rem(0.8125f).ToFontPx();
        float headerFontPx = new Rem(0.75f).ToFontPx();
        float monoFontPx = new Rem(0.8125f).ToFontPx();
        float borderPx = Mathf.Max(1f, new Rem(1f / 16f).ToPixels());
        float nameMinRem = 6f;
        float nameMaxRem = 16f;
        float typeWidthRem = 10f;
        float defaultWidthRem = 7f;

        GUIStyle HeaderStyle(Theme.Theme theme) {
            GUIStyle s = GuiStyleCache.GetOrCreate(theme, FontRole.Body, Mathf.RoundToInt(headerFontPx), FontStyle.Bold);
            s.alignment = TextAnchor.MiddleLeft;
            s.clipping = TextClipping.Clip;
            s.wordWrap = false;
            return s;
        }

        GUIStyle BodyStyle(Theme.Theme theme, bool mono) {
            Font f = theme.GetFont(mono ? FontRole.Mono : FontRole.Body);
            float px = mono ? monoFontPx : bodyFontPx;
            GUIStyle s = GuiStyleCache.GetOrCreate(f, Mathf.RoundToInt(px));
            s.alignment = TextAnchor.UpperLeft;
            s.wordWrap = true;
            s.clipping = TextClipping.Clip;
            return s;
        }

        float CellHeight(GUIStyle style, string? text, float innerWidth) {
            if (string.IsNullOrEmpty(text) || innerWidth <= 0f) {
                return 0f;
            }

            return style.CalcHeight(new GUIContent(text), innerWidth);
        }

        float NameColumnWidth(Theme.Theme theme) {
            if (parameters == null || parameters.Count == 0) {
                return new Rem(nameMinRem).ToPixels();
            }

            GUIStyle nameStyle = BodyStyle(theme, true);
            float maxText = 0f;
            for (int i = 0; i < parameters.Count; i++) {
                string? n = parameters[i].Name;
                if (string.IsNullOrEmpty(n)) {
                    continue;
                }

                float w = nameStyle.CalcSize(new GUIContent(n)).x;
                if (w > maxText) {
                    maxText = w;
                }
            }

            float content = maxText + cellPaddingX * 2f;
            float minPx = new Rem(nameMinRem).ToPixels();
            float maxPx = new Rem(nameMaxRem).ToPixels();
            return Mathf.Clamp(content, minPx, maxPx);
        }

        float RowHeight(ApiParam p, float[] widths, Theme.Theme theme) {
            GUIStyle nameStyle = BodyStyle(theme, true);
            GUIStyle typeStyle = BodyStyle(theme, true);
            GUIStyle defaultStyle = BodyStyle(theme, true);
            GUIStyle descStyle = BodyStyle(theme, false);

            float nameH = CellHeight(nameStyle, p.Name, widths[0] - cellPaddingX * 2f);
            float typeH = CellHeight(typeStyle, p.Type, widths[1] - cellPaddingX * 2f);
            float defaultH = CellHeight(defaultStyle, p.DefaultValue, widths[2] - cellPaddingX * 2f);
            float descH = CellHeight(descStyle, p.Description, widths[3] - cellPaddingX * 2f);

            float maxContent = Mathf.Max(Mathf.Max(nameH, typeH), Mathf.Max(defaultH, descH));
            return Mathf.Max(maxContent, new Rem(1.5f).ToPixels()) + rowPaddingY * 2f;
        }

        float[] ResolveWidths(float totalWidth, Theme.Theme theme) {
            float nameW = NameColumnWidth(theme);
            float typeW = new Rem(typeWidthRem).ToPixels();
            float defaultW = new Rem(defaultWidthRem).ToPixels();
            float consumed = nameW + typeW + defaultW;
            float descW = Mathf.Max(new Rem(10f).ToPixels(), totalWidth - consumed);
            return new[] { nameW, typeW, defaultW, descW };
        }

        node.Measure = availableWidth => {
            if (parameters == null || parameters.Count == 0) {
                return headerHeightPx + borderPx * 2f;
            }

            Theme.Theme theme = RenderContext.Current.Theme;
            float[] widths = ResolveWidths(availableWidth, theme);
            float total = headerHeightPx + borderPx;
            for (int i = 0; i < parameters.Count; i++) {
                total += RowHeight(parameters[i], widths, theme) + borderPx;
            }

            return total;
        };

        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;

            BorderSpec borderSpec = BorderSpec.All(new Rem(1f / 16f), ThemeSlot.BorderSubtle);
            RadiusSpec rad = RadiusSpec.All(RadiusScale.Md);
            PaintBox.Draw(rect, BackgroundSpec.Of(ThemeSlot.SurfacePrimary), borderSpec, rad);

            if (parameters == null || parameters.Count == 0) {
                GUIStyle empty = BodyStyle(theme, false);
                Color savedEmpty = GUI.color;
                GUI.color = theme.GetColor(ThemeSlot.TextMuted);
                Rect emptyRect = new Rect(
                    rect.x + cellPaddingX,
                    rect.y + rowPaddingY,
                    rect.width - cellPaddingX * 2f,
                    rect.height - rowPaddingY * 2f
                );
                GUI.Label(RectSnap.Snap(emptyRect), (string)"CL_Playground_Panel_ApiEmpty".Translate(), empty);
                GUI.color = savedEmpty;
                return;
            }

            Rect inner = new Rect(
                rect.x + borderPx,
                rect.y + borderPx,
                Mathf.Max(0f, rect.width - borderPx * 2f),
                Mathf.Max(0f, rect.height - borderPx * 2f)
            );

            float[] widths = ResolveWidths(inner.width, theme);
            string[] headers = {
                (string)"CL_Playground_Panel_ApiName".Translate(),
                (string)"CL_Playground_Panel_ApiType".Translate(),
                (string)"CL_Playground_Panel_ApiDefault".Translate(),
                (string)"CL_Playground_Panel_ApiDescription".Translate(),
            };

            Rect headerRect = new Rect(inner.x, inner.y, inner.width, headerHeightPx);
            Color savedColor = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.SurfaceSunken);
            GUI.DrawTexture(headerRect, Texture2D.whiteTexture);
            GUI.color = savedColor;

            float hx = inner.x;
            GUIStyle headStyle = HeaderStyle(theme);
            for (int i = 0; i < 4; i++) {
                Rect cell = new Rect(hx + cellPaddingX, headerRect.y, widths[i] - cellPaddingX * 2f, headerHeightPx);
                GUI.color = theme.GetColor(ThemeSlot.TextPrimary);
                GUI.Label(RectSnap.Snap(cell), headers[i], headStyle);
                GUI.color = savedColor;
                hx += widths[i];
            }

            float dividerY = headerRect.y + headerHeightPx;
            Rect divider = new Rect(inner.x, dividerY, inner.width, borderPx);
            GUI.color = theme.GetColor(ThemeSlot.BorderSubtle);
            GUI.DrawTexture(divider, Texture2D.whiteTexture);
            GUI.color = savedColor;

            float cursorY = dividerY + borderPx;
            GUIStyle nameStyle = BodyStyle(theme, true);
            GUIStyle typeStyle = BodyStyle(theme, true);
            GUIStyle defaultStyle = BodyStyle(theme, true);
            GUIStyle descStyle = BodyStyle(theme, false);

            for (int i = 0; i < parameters.Count; i++) {
                ApiParam p = parameters[i];
                float rowH = RowHeight(p, widths, theme);

                if (i % 2 == 1) {
                    Rect striped = new Rect(inner.x, cursorY, inner.width, rowH);
                    GUI.color = theme.GetColor(ThemeSlot.SurfaceRaised);
                    GUI.DrawTexture(striped, Texture2D.whiteTexture);
                    GUI.color = savedColor;
                }

                float rx = inner.x;
                string[] vals = { p.Name, p.Type, p.DefaultValue, p.Description };
                GUIStyle[] styles = { nameStyle, typeStyle, defaultStyle, descStyle };
                ThemeSlot[] slots = {
                    ThemeSlot.TextPrimary,
                    ThemeSlot.TextSecondary,
                    ThemeSlot.TextMuted,
                    ThemeSlot.TextPrimary,
                };

                for (int c = 0; c < 4; c++) {
                    Rect cell = new Rect(
                        rx + cellPaddingX,
                        cursorY + rowPaddingY,
                        widths[c] - cellPaddingX * 2f,
                        rowH - rowPaddingY * 2f
                    );
                    GUI.color = theme.GetColor(slots[c]);
                    GUI.Label(RectSnap.Snap(cell), vals[c] ?? string.Empty, styles[c]);
                    GUI.color = savedColor;
                    rx += widths[c];
                }

                cursorY += rowH;
                if (i < parameters.Count - 1) {
                    Rect rowDivider = new Rect(inner.x, cursorY, inner.width, borderPx);
                    GUI.color = theme.GetColor(ThemeSlot.BorderSubtle);
                    GUI.DrawTexture(rowDivider, Texture2D.whiteTexture);
                    GUI.color = savedColor;
                    cursorY += borderPx;
                }
            }
        };

        return node;
    }
}
