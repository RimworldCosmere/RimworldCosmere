using System;
using System.Runtime.CompilerServices;
using Cosmere.Lightweave.Doc;
using Cosmere.Lightweave.Icons;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;

namespace Cosmere.Lightweave.Typography;

[Doc(
    Id = "glyph",
    Summary = "Single-character glyph rendered centered (both axes) inside its rect via TextAnchor.MiddleCenter.",
    WhenToUse = "Close X, expand/collapse chevrons, dropdown carets, status dots, any short symbolic mark that must sit pixel-perfect in the middle of a sized box. For body copy use Text; for hero text use Display.",
    SourcePath = "Lightweave/Lightweave/Typography/Glyph.cs",
    ShowRtl = false
)]
public static class Glyph {
    public static LightweaveNode Create(
        [DocParam("Glyph content — typically a single character or short cluster like '✕', '▾', '▶'.")]
        string glyph,
        [DocParam("Inline style override. FontFamily/FontSize/FontWeight/TextColor honored; TextAlign is ignored (always MiddleCenter).", TypeOverride = "Style?", DefaultOverride = "null")]
        Style? style = null,
        [DocParam("Additional class names merged after the base 'glyph' class.", TypeOverride = "string[]?", DefaultOverride = "null")]
        string[]? classes = null,
        [DocParam("Stable id for state-style lookup.", TypeOverride = "string?", DefaultOverride = "null")]
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        LightweaveNode node = NodeBuilder.New($"Glyph:{glyph}", line, file);
        node.ApplyStyling("glyph", style, classes, id);

        GUIStyle ResolveGuiStyle() {
            Style s = node.GetResolvedStyle();
            Theme.Theme theme = RenderContext.Current.Theme;
            FontRef? fr = s.FontFamily;
            Rem fontSize = s.FontSize ?? new Rem(1f);
            FontStyle weight = s.FontWeight ?? FontStyle.Normal;
            int pixelSize = Mathf.RoundToInt(fontSize.ToFontPx());
            GUIStyle gs = fr switch {
                FontRef.Literal lit => GuiStyleCache.GetOrCreate(lit.Value, pixelSize, weight),
                FontRef.Role role => GuiStyleCache.GetOrCreate(theme, role.RoleValue, pixelSize, weight),
                _ => GuiStyleCache.GetOrCreate(theme, FontRole.Body, pixelSize, weight),
            };
            return gs;
        }

        node.MeasureWidth = () => {
            if (string.IsNullOrEmpty(glyph)) {
                return 0f;
            }

            GUIStyle gs = ResolveGuiStyle();
            return Mathf.Ceil(gs.CalcSize(new GUIContent(glyph)).x);
        };

        node.Measure = _ => {
            if (string.IsNullOrEmpty(glyph)) {
                return 0f;
            }

            GUIStyle gs = ResolveGuiStyle();
            return Mathf.Ceil(gs.CalcSize(new GUIContent(glyph)).y);
        };

        node.Paint = (rect, _) => {
            if (string.IsNullOrEmpty(glyph)) {
                return;
            }

            Theme.Theme theme = RenderContext.Current.Theme;
            Style s = node.GetResolvedStyle();
            GUIStyle gs = ResolveGuiStyle();
            gs.alignment = TextAnchor.UpperLeft;
            gs.clipping = TextClipping.Overflow;
            gs.wordWrap = false;

            Rem fontSizeRem = s.FontSize ?? new Rem(1f);
            int pixelSize = Mathf.RoundToInt(fontSizeRem.ToFontPx());
            FontStyle weight = s.FontWeight ?? FontStyle.Normal;

            Vector2 textSize = gs.CalcSize(new GUIContent(glyph));
            float drawX = rect.x + (rect.width - textSize.x) / 2f;
            float drawY = rect.y + (rect.height - textSize.y) / 2f;

            UnityEngine.Font? gFont = gs.font;
            if (gFont != null) {
                gFont.RequestCharactersInTexture(glyph, pixelSize, weight);
                if (gFont.GetCharacterInfo(glyph[0], out CharacterInfo ci, pixelSize, weight)) {
                    float visualCenterX = (ci.minX + ci.maxX) / 2f;
                    drawX = rect.x + rect.width / 2f - visualCenterX;

                    float ascentPx = gFont.fontSize > 0
                        ? (float)gFont.ascent * pixelSize / gFont.fontSize
                        : textSize.y * 0.85f;
                    float visualCenterFromTop = ascentPx - (ci.maxY + ci.minY) / 2f;
                    drawY = rect.y + rect.height / 2f - visualCenterFromTop;
                }
            }

            ColorRef? cr = s.TextColor;
            Color c = cr switch {
                ColorRef.Literal lit => lit.Value,
                ColorRef.Token tok => theme.GetColor(tok.Slot),
                _ => theme.GetColor(ThemeSlot.TextPrimary),
            };
            Color saved = GUI.color;
            GUI.color = c;
            GUI.Label(RectSnap.Snap(new Rect(drawX, drawY, textSize.x, textSize.y)), glyph, gs);
            GUI.color = saved;
        };
        return node;
    }

    public static LightweaveNode Create(
        IconRef icon,
        Style? style = null,
        string[]? classes = null,
        string? id = null,
        [CallerLineNumber] int line = 0,
        [CallerFilePath] string file = ""
    ) {
        UnityEngine.Font? font = icon.ResolveFont();
        Style merged = style ?? new Style();
        if (font != null && merged.FontFamily == null) {
            merged = merged with { FontFamily = (FontRef)font };
        }

        string[] mergedClasses;
        string familyClass = icon.Family switch {
            IconFamily.Phosphor => "icon-phosphor",
            IconFamily.RpgAwesome => "icon-rpg-awesome",
            _ => "icon",
        };
        if (classes == null) {
            mergedClasses = new[] { familyClass };
        }
        else {
            mergedClasses = new string[classes.Length + 1];
            mergedClasses[0] = familyClass;
            Array.Copy(classes, 0, mergedClasses, 1, classes.Length);
        }

        return Create(icon.Glyph, style: merged, classes: mergedClasses, id: id, line: line, file: file);
    }

    [DocVariant("CL_Playground_Label_Close")]
    public static DocSample DocsClose() {
        return new DocSample(() => Glyph.Create("✕", style: new Style { FontSize = new Rem(1.125f) }));
    }

    [DocVariant("CL_Playground_Label_ChevronDown")]
    public static DocSample DocsChevronDown() {
        return new DocSample(() => Glyph.Create("▾", style: new Style { FontSize = new Rem(1f) }));
    }

    [DocVariant("CL_Playground_Label_Play")]
    public static DocSample DocsPlay() {
        return new DocSample(() => Glyph.Create("▶", style: new Style { FontSize = new Rem(1f) }));
    }

    [DocUsage]
    public static DocSample DocsUsage() {
        return new DocSample(() => Glyph.Create("✕", style: new Style {
            FontSize = new Rem(1.125f),
            TextColor = ThemeSlot.TextSecondary,
        }));
    }
}
