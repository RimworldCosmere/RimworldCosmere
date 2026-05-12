using Cosmere.Lightweave.Fonts;
using Cosmere.Lightweave.Input;
using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.LoadColony;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;
using static Cosmere.Lightweave.Typography.Typography;
using Display = Cosmere.Lightweave.Typography.Display;
using Eyebrow = Cosmere.Lightweave.Typography.Eyebrow;
using HotkeyBadge = Cosmere.Lightweave.Typography.HotkeyBadge;
using Text = Cosmere.Lightweave.Typography.Typography.Text;

namespace Cosmere.Lightweave.MainMenu;

public static class ContinueCard {
    public static LightweaveNode Create(
        SaveMetadata.LatestSave? save
    ) {
        if (save == null) {
            return BuildWelcome();
        }
        return BuildContinue(save);
    }

    private static LightweaveNode BuildContinue(SaveMetadata.LatestSave save) {
        return Box.Create(
            children: c => c.Add(HStack.Create(SpacingScale.None, h => {
                h.AddFlex(BuildContinueContent(save));
                h.Add(BuildContinueButton(save), new Rem(15.5f).ToPixels());
            })),
            style: new Style {
                Background = BackgroundSpec.Blur(new Color(0f, 0f, 0f, 0.72f)),
                Border = BorderSpec.All(new Rem(0.0625f), ThemeSlot.BorderSubtle),
                Radius = RadiusSpec.All(RadiusScale.None),
            }
        );
    }

    private static LightweaveNode BuildContinueContent(SaveMetadata.LatestSave save) {
        return Box.Create(
            children: c => c.Add(HStack.Create(SpacingScale.Md, h => {
                h.Add(BuildThumbnail(save), new Rem(14.7f).ToPixels());
                h.AddFlex(BuildBody(save));
            })),
            style: new Style {
                Padding = new EdgeInsets(Top: new Rem(1.25f), Bottom: new Rem(1.25f), Left: new Rem(1.5f), Right: new Rem(1.5f)),
            }
        );
    }

    private static LightweaveNode BuildThumbnail(SaveMetadata.LatestSave save) {
        LightweaveNode node = NodeBuilder.New("ContinueCard:Thumbnail");
        node.PreferredHeight = new Rem(9.1875f).ToPixels();
        node.Paint = (rect, _) => {
            Color saved = GUI.color;
            GUI.color = new Color(0.122f, 0.086f, 0.067f, 1f);
            GUI.DrawTexture(RectSnap.Snap(rect), BaseContent.WhiteTex);
            GUI.color = saved;

            Texture2D? shot = ColonyScreenshotCache.GetOrLoad(save);
            if (shot != null) {
                Color saved2 = GUI.color;
                GUI.color = Color.white;
                GUI.DrawTexture(RectSnap.Snap(rect), shot, ScaleMode.ScaleAndCrop);
                GUI.color = saved2;
            }
            else {
                DrawScanlines(rect);
            }
        };
        return node;
    }

    private static void DrawScanlines(Rect rect) {
        Color saved = GUI.color;

        Color baseFill = new Color(0.122f, 0.086f, 0.067f, 1f);
        GUI.color = baseFill;
        GUI.DrawTexture(RectSnap.Snap(rect), BaseContent.WhiteTex);

        Color highlight = new Color(0.353f, 0.255f, 0.157f, 1f);
        int steps = 5;
        for (int i = 0; i < steps; i++) {
            float t = (i + 1) / (float)steps;
            float radius = Mathf.Min(rect.width, rect.height) * (0.85f * t);
            float alpha = 0.45f * (1f - t);
            Color c = highlight;
            c.a = alpha;
            GUI.color = c;
            float cx = rect.x + rect.width * 0.30f;
            float cy = rect.y + rect.height * 0.30f;
            Rect r = new Rect(cx - radius * 0.5f, cy - radius * 0.5f, radius, radius);
            GUI.DrawTexture(RectSnap.Snap(r), BaseContent.WhiteTex);
        }

        Color line = new Color(1f, 0.784f, 0.549f, 1f);
        line.a = 0.10f;
        GUI.color = line;
        float step = new Rem(0.25f).ToPixels();
        for (float y = rect.y + step; y < rect.yMax; y += step) {
            Rect r = new Rect(rect.x + 2f, y, rect.width - 4f, 1f);
            GUI.DrawTexture(RectSnap.Snap(r), BaseContent.WhiteTex);
        }
        GUI.color = saved;
    }

    private static LightweaveNode BuildBody(SaveMetadata.LatestSave save) {
        return Stack.Create(SpacingScale.Sm, s => {
            s.Add(BuildEyebrowLine(save));
            s.Add(BuildTitleLine(save));
            s.Add(BuildStatsRow(save));
        });
    }

    private static LightweaveNode BuildEyebrowLine(SaveMetadata.LatestSave save) {
        LightweaveNode node = NodeBuilder.New("ContinueCard:Eyebrow");
        node.PreferredHeight = new Rem(1.5f).ToPixels();
        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Font font = theme.GetFont(FontRole.Mono);
            int px = Mathf.RoundToInt(new Rem(1.0667f).ToFontPx());
            GUIStyle style = GuiStyleCache.GetOrCreate(font, px, FontStyle.Normal);
            style.alignment = TextAnchor.MiddleLeft;

            string parts = BuildEyebrowText(save);
            string upper = parts.ToUpperInvariant();
            float tracking = px * 0.18f;
            float cursor = rect.x;
            Color saved = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.SurfaceAccent);
            for (int i = 0; i < upper.Length; i++) {
                char c = upper[i];
                string ch = c.ToString();
                GUIContent gcc = new GUIContent(ch);
                float w = style.CalcSize(gcc).x;
                GUI.Label(RectSnap.Snap(new Rect(cursor, rect.y, w, rect.height)), ch, style);
                cursor += w + tracking;
            }
            GUI.color = saved;
        };
        return node;
    }

    private static string BuildEyebrowText(SaveMetadata.LatestSave save) {
        System.Collections.Generic.List<string> parts = new System.Collections.Generic.List<string>();
        parts.Add("CL_MainMenu_Continue".Translate());
        if (save.Sidecar != null) {
            if (save.Sidecar.InGameYear > 0) {
                parts.Add("CL_MainMenu_YearAD".Translate(save.Sidecar.InGameYear.Named("YEAR")));
            }
            if (!string.IsNullOrEmpty(save.Sidecar.Quadrum)) {
                parts.Add(save.Sidecar.Quadrum);
            }
            if (!string.IsNullOrEmpty(save.Sidecar.Biome)) {
                parts.Add(save.Sidecar.Biome);
            }
        }
        return string.Join(" · ", parts);
    }

    private static LightweaveNode BuildTitleLine(SaveMetadata.LatestSave save) {
        LightweaveNode node = NodeBuilder.New("ContinueCard:Title");
        int px = Mathf.RoundToInt(new Rem(2.875f).ToFontPx());
        node.PreferredHeight = Mathf.Ceil(px * 1.35f);
        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Font font = LightweaveFonts.IMFellEnglishSC ?? theme.GetFont(FontRole.Display);
            GUIStyle style = GuiStyleCache.GetOrCreate(font, px, FontStyle.Normal);
            style.alignment = TextAnchor.MiddleLeft;
            style.clipping = TextClipping.Overflow;

            string title = save.DisplayName ?? string.Empty;
            if (title.Length == 0) {
                return;
            }
            int tracking = Mathf.Max(0, Mathf.RoundToInt(px * 0.02f));

            int[] widths = new int[title.Length];
            for (int i = 0; i < title.Length; i++) {
                GUIContent gc = new GUIContent(title[i].ToString());
                widths[i] = Mathf.CeilToInt(style.CalcSize(gc).x);
            }

            Color saved = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.TextPrimary);

            int cursor = Mathf.FloorToInt(rect.x);
            int y = Mathf.FloorToInt(rect.y);
            int h = Mathf.CeilToInt(rect.height);
            for (int i = 0; i < title.Length; i++) {
                string ch = title[i].ToString();
                GUI.Label(new Rect(cursor, y, widths[i], h), ch, style);
                cursor += widths[i] + (i < title.Length - 1 ? tracking : 0);
            }
            GUI.color = saved;
        };
        return node;
    }

    private static LightweaveNode BuildStatsRow(SaveMetadata.LatestSave save) {
        return HStack.Create(new Rem(1.375f), h => {
            void AddStat(string value, string label, ThemeSlot valueColor) {
                LightweaveNode column = BuildStatColumn(value, label, valueColor);
                h.AddFlex(column);
            }
            void AddSep() {
                h.AddHug(BuildStatSeparator());
            }

            if (save.Sidecar == null) {
                string when = SaveMetadata.FormatRelative(save.LastWriteTime);
                AddStat(when, "saved", ThemeSlot.TextPrimary);
                if (save.Permadeath) {
                    AddSep();
                    AddStat((string)"CL_MainMenu_Stat_Permadeath".Translate(), string.Empty, ThemeSlot.AccentMuted);
                }
                return;
            }

            SaveSidecarData sidecar = save.Sidecar!;
            int threatPct = Mathf.RoundToInt(sidecar.ThreatScale * 100f);
            string wealth = FormatWealth(sidecar.Wealth);

            AddStat(sidecar.ColonistCount.ToString(), (string)"CL_MainMenu_Stat_Colonists".Translate(), ThemeSlot.TextPrimary);
            AddSep();
            AddStat(sidecar.AnimalCount.ToString(), (string)"CL_MainMenu_Stat_Animals".Translate(), ThemeSlot.TextPrimary);
            AddSep();
            AddStat(wealth, (string)"CL_MainMenu_Stat_Wealth".Translate(), ThemeSlot.TextPrimary);
            AddSep();
            AddStat(sidecar.DaysSurvived + " days", "alive", ThemeSlot.TextPrimary);
            AddSep();
            AddStat(threatPct + "%", "difficulty", ThemeSlot.TextPrimary);
            if (save.Permadeath) {
                AddSep();
                AddStat((string)"CL_MainMenu_Stat_Permadeath".Translate(), string.Empty, ThemeSlot.AccentMuted);
            }
        });
    }
    private static LightweaveNode BuildStatSeparator() {
        LightweaveNode node = NodeBuilder.New("ContinueCard:StatSep");
        node.PreferredHeight = new Rem(1.625f).ToPixels();
        node.MeasureWidth = () => new Rem(0.5f).ToPixels();
        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Font font = theme.GetFont(FontRole.Mono);
            int px = Mathf.RoundToInt(new Rem(1.25f).ToFontPx());
            GUIStyle style = GuiStyleCache.GetOrCreate(font, px, FontStyle.Normal);
            style.alignment = TextAnchor.MiddleCenter;
            float valueH = new Rem(1.625f).ToPixels();
            Color saved = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.TextMuted);
            GUI.Label(RectSnap.Snap(new Rect(rect.x, rect.y, rect.width, valueH)), "·", style);
            GUI.color = saved;
        };
        return node;
    }


    private static LightweaveNode BuildStatColumn(string value, string label, ThemeSlot valueColor) {
        LightweaveNode node = NodeBuilder.New($"StatColumn:{value}");
        node.PreferredHeight = (new Rem(1.625f).ToPixels()) + (new Rem(0.125f).ToPixels()) + (new Rem(1.0f).ToPixels());
        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;

            Font valueFont = theme.GetFont(FontRole.Mono);
            int valuePx = Mathf.RoundToInt(new Rem(1.5f).ToFontPx());
            GUIStyle valueStyle = GuiStyleCache.GetOrCreate(valueFont, valuePx, FontStyle.Bold);
            valueStyle.alignment = TextAnchor.UpperLeft;

            Font labelFont = theme.GetFont(FontRole.Mono);
            int labelPx = Mathf.RoundToInt(new Rem(0.875f).ToFontPx());
            GUIStyle labelStyle = GuiStyleCache.GetOrCreate(labelFont, labelPx, FontStyle.Normal);
            labelStyle.alignment = TextAnchor.UpperLeft;

            float valueH = new Rem(1.625f).ToPixels();
            float gap = new Rem(0.125f).ToPixels();

            Color saved = GUI.color;
            GUI.color = theme.GetColor(valueColor);
            GUI.Label(RectSnap.Snap(new Rect(rect.x, rect.y, rect.width, valueH)), value, valueStyle);

            if (!string.IsNullOrEmpty(label)) {
                GUI.color = theme.GetColor(ThemeSlot.TextMuted);
                GUI.Label(RectSnap.Snap(new Rect(rect.x, rect.y + valueH + gap, rect.width, rect.height - valueH - gap)), label, labelStyle);
            }
            GUI.color = saved;
        };

        node.MeasureWidth = () => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Font valueFont = theme.GetFont(FontRole.Mono);
            int valuePx = Mathf.RoundToInt(new Rem(1.5f).ToFontPx());
            GUIStyle valueStyle = GuiStyleCache.GetOrCreate(valueFont, valuePx, FontStyle.Bold);
            float valueW = valueStyle.CalcSize(new GUIContent(value)).x;

            float labelW = 0f;
            if (!string.IsNullOrEmpty(label)) {
                Font labelFont = theme.GetFont(FontRole.Mono);
                int labelPx = Mathf.RoundToInt(new Rem(0.875f).ToFontPx());
                GUIStyle labelStyle = GuiStyleCache.GetOrCreate(labelFont, labelPx, FontStyle.Normal);
                labelW = labelStyle.CalcSize(new GUIContent(label)).x;
            }
            return Mathf.Max(valueW, labelW);
        };

        node.Measure = availableWidth => new Rem(2.5f).ToPixels();
        node.PreferredHeight = new Rem(2.5f).ToPixels();

        return node;
    }

    

    private static string FormatWealth(float wealth) {
        if (wealth >= 1000f) {
            return Mathf.RoundToInt(wealth / 1000f) + "k";
        }
        return Mathf.RoundToInt(wealth).ToString();
    }

    private static LightweaveNode BuildContinueButton(SaveMetadata.LatestSave save) {
        LightweaveNode node = NodeBuilder.New("ContinueCard:ResumeButton");
        node.Style = new Style {
            Width = Length.Stretch,
            Height = Length.Stretch,
            LetterSpacing = Tracking.Widest,
        };
        node.Paint = (rect, _) => {
            InteractionState state = InteractionState.Resolve(rect, null, false);

            Color topColor = new Color(212f / 255f, 168f / 255f, 87f / 255f, 1f);
            Color bottomColor = new Color(184f / 255f, 136f / 255f, 56f / 255f, 1f);
            if (state.Pressed) {
                topColor = new Color(160f / 255f, 120f / 255f, 50f / 255f, 1f);
                bottomColor = new Color(120f / 255f, 84f / 255f, 28f / 255f, 1f);
            }
            else if (state.Hovered) {
                topColor = new Color(224f / 255f, 185f / 255f, 106f / 255f, 1f);
                bottomColor = new Color(199f / 255f, 151f / 255f, 65f / 255f, 1f);
            }
            BackgroundSpec.Gradient bg = new BackgroundSpec.Gradient(GradientTextureCache.Vertical(topColor, bottomColor));
            PaintBox.Draw(rect, bg, null, RadiusSpec.All(RadiusScale.None));

            Color leftBorder = new Color(0f, 0f, 0f, 0.4f);
            Rect leftStroke = new Rect(rect.x, rect.y, 1f, rect.height);
            Color savedColor = GUI.color;
            GUI.color = leftBorder;
            GUI.DrawTexture(RectSnap.Snap(leftStroke), BaseContent.WhiteTex);
            GUI.color = savedColor;

            if (Event.current.type == EventType.Repaint) {
                Font? font = LightweaveFonts.CarlitoBold ?? LightweaveFonts.CarlitoRegular;
                if (font == null) {
                    font = RenderContext.Current.Theme.GetFont(FontRole.BodyBold);
                }
                Rem fontSizeRem = new Rem(1.25f);
                int pixelSize = Mathf.RoundToInt(fontSizeRem.ToFontPx());
                GUIStyle gstyle = GuiStyleCache.GetOrCreate(font, pixelSize, FontStyle.Bold);
                gstyle.alignment = TextAnchor.MiddleLeft;
                gstyle.clipping = TextClipping.Overflow;

                Style resolved = node.GetResolvedStyle();
                int letterSpacing = resolved.LetterSpacing.HasValue
                    ? Mathf.Max(0, Mathf.RoundToInt(resolved.LetterSpacing.Value.ToPixels(fontSizeRem.ToFontPx())))
                    : 0;

                Color inkColor = new Color(26f / 255f, 19f / 255f, 10f / 255f, 1f);
                string text = "▶  " + ((string)"CL_MainMenu_Continue_Action".Translate()).ToUpperInvariant();

                int[] widths = new int[text.Length];
                int totalW = 0;
                for (int i = 0; i < text.Length; i++) {
                    GUIContent gc = new GUIContent(text[i].ToString());
                    widths[i] = Mathf.CeilToInt(gstyle.CalcSize(gc).x);
                    totalW += widths[i];
                    if (i < text.Length - 1) {
                        totalW += letterSpacing;
                    }
                }
                int startX = Mathf.FloorToInt(rect.x + (rect.width - totalW) * 0.5f);
                int y = Mathf.FloorToInt(rect.y);
                int h = Mathf.CeilToInt(rect.height);

                Color saved = GUI.color;
                GUI.color = inkColor;
                int cursor = startX;
                for (int i = 0; i < text.Length; i++) {
                    string ch = text[i].ToString();
                    GUI.Label(new Rect(cursor, y, widths[i], h), ch, gstyle);
                    cursor += widths[i] + letterSpacing;
                }
                GUI.color = saved;
            }

            InteractionFeedback.Apply(rect, true, true);

            Event e = Event.current;
            if (e.type == EventType.MouseUp && e.button == 0 && rect.Contains(e.mousePosition)) {
                MainMenuActions.ContinueLatestSave(save.FileName);
                e.Use();
            }
        };
        return node;
    }

    private static LightweaveNode BuildWelcome() {
        return Box.Create(
            children: c => c.Add(Stack.Create(SpacingScale.Xs, s => {
                s.Add(Eyebrow.Create("CL_MainMenu_Welcome_Eyebrow".Translate()));
                s.Add(Display.Create("CL_MainMenu_Welcome".Translate(), style: new Style { TextAlign = TextAlign.Start }, level: 3));
                s.Add(Text.Create(
                    "CL_MainMenu_Welcome_Hint".Translate(),
                    wrap: true,
                    style: new Style { TextColor = ThemeSlot.TextSecondary }
                ));
            })),
            style: new Style {
                Padding = EdgeInsets.All(SpacingScale.Md),
                Background = BackgroundSpec.Of(ThemeSlot.SurfaceRaised),
                Border = BorderSpec.All(new Rem(0.0625f), ThemeSlot.BorderSubtle),
                Radius = RadiusSpec.All(RadiusScale.Md),
            }
        );
    }


}
