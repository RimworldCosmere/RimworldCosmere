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
            background: BackgroundSpec.Blur(new Color(15f / 255f, 12f / 255f, 8f / 255f, 0.92f)),
            border: BorderSpec.All(new Rem(0.0625f), ThemeSlot.BorderSubtle),
            radius: RadiusSpec.All(RadiusScale.None),
            children: c => c.Add(HStack.Create(SpacingScale.None, h => {
                h.AddFlex(BuildContinueContent(save));
                h.Add(BuildContinueButton(save), new Rem(11f).ToPixels());
            }))
        );
    }

    private static LightweaveNode BuildContinueContent(SaveMetadata.LatestSave save) {
        return Box.Create(
            padding: new EdgeInsets(Top: new Rem(1.25f), Bottom: new Rem(1.25f), Left: new Rem(1.5f), Right: new Rem(1.5f)),
            children: c => c.Add(HStack.Create(SpacingScale.Md, h => {
                h.Add(BuildThumbnail(save), new Rem(6f).ToPixels());
                h.AddFlex(BuildBody(save));
            }))
        );
    }

    private static LightweaveNode BuildThumbnail(SaveMetadata.LatestSave save) {
        LightweaveNode node = NodeBuilder.New("ContinueCard:Thumbnail");
        node.PreferredHeight = new Rem(6f).ToPixels();
        node.Paint = (rect, _) => {
            Color saved = GUI.color;
            GUI.color = new Color(0.122f, 0.086f, 0.067f, 1f);
            GUI.DrawTexture(RectSnap.Snap(rect), BaseContent.WhiteTex);
            GUI.color = saved;

            Texture2D? shot = ColonyScreenshotCache.GetOrLoad(save);
            if (shot != null) {
                Color saved2 = GUI.color;
                GUI.color = Color.white;
                GUI.DrawTexture(rect, shot, ScaleMode.ScaleAndCrop);
                GUI.color = saved2;
            }
            else {
                DrawScanlines(rect);
            }

            Color savedLine = GUI.color;
            Color line = new Color(1f, 0.784f, 0.549f, 0.06f);
            GUI.color = line;
            float step = new Rem(0.25f).ToPixels();
            for (float y = rect.y + step; y < rect.yMax; y += step) {
                Rect r = new Rect(rect.x + 2f, y, rect.width - 4f, 1f);
                GUI.DrawTexture(RectSnap.Snap(r), BaseContent.WhiteTex);
            }
            GUI.color = savedLine;

            float borderThick = Mathf.Max(1f, new Rem(0.0875f).ToPixels());
            Color savedBorder = GUI.color;
            Color borderCol = new Color(0.55f, 0.42f, 0.25f, 0.85f);
            GUI.color = borderCol;
            GUI.DrawTexture(RectSnap.Snap(new Rect(rect.x, rect.y, rect.width, borderThick)), BaseContent.WhiteTex);
            GUI.DrawTexture(RectSnap.Snap(new Rect(rect.x, rect.yMax - borderThick, rect.width, borderThick)), BaseContent.WhiteTex);
            GUI.DrawTexture(RectSnap.Snap(new Rect(rect.x, rect.y, borderThick, rect.height)), BaseContent.WhiteTex);
            GUI.DrawTexture(RectSnap.Snap(new Rect(rect.xMax - borderThick, rect.y, borderThick, rect.height)), BaseContent.WhiteTex);
            GUI.color = savedBorder;
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
        node.PreferredHeight = new Rem(1.0f).ToPixels();
        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Font font = theme.GetFont(FontRole.Mono);
            int px = Mathf.RoundToInt(new Rem(0.6875f).ToFontPx());
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
        node.PreferredHeight = new Rem(2.5f).ToPixels();
        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;
            Font font = theme.GetFont(FontRole.Display);
            int px = Mathf.RoundToInt(new Rem(2.25f).ToFontPx());
            GUIStyle style = GuiStyleCache.GetOrCreate(font, px, FontStyle.Italic);
            style.alignment = TextAnchor.MiddleLeft;
            style.clipping = TextClipping.Clip;

            string title = save.DisplayName ?? string.Empty;
            float tracking = px * 0.04f;

            Color saved = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.TextPrimary);

            float cursor = rect.x;
            float midY = rect.y;
            float lineH = rect.height;
            for (int i = 0; i < title.Length; i++) {
                string ch = title[i].ToString();
                GUIContent gc = new GUIContent(ch);
                float w = style.CalcSize(gc).x;
                GUI.Label(RectSnap.Snap(new Rect(cursor, midY, w, lineH)), ch, style);
                cursor += w + (i < title.Length - 1 ? tracking : 0f);
            }
            GUI.color = saved;
        };
        return node;
    }

    private static LightweaveNode BuildStatsRow(SaveMetadata.LatestSave save) {
        return HStack.Create(new Rem(1.375f), h => {
            void AddStat(string value, string label, ThemeSlot valueColor) {
                LightweaveNode column = BuildStatColumn(value, label, valueColor);
                h.AddHug(column);
            }
            void AddSep() {
                h.AddHug(Text.Create("·", FontRole.Mono, new Rem(0.75f), ThemeSlot.TextMuted));
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

    private static LightweaveNode BuildStatColumn(string value, string label, ThemeSlot valueColor) {
        LightweaveNode node = NodeBuilder.New($"StatColumn:{value}");
        node.Paint = (rect, _) => {
            Theme.Theme theme = RenderContext.Current.Theme;

            Font valueFont = theme.GetFont(FontRole.Mono);
            int valuePx = Mathf.RoundToInt(new Rem(1.125f).ToFontPx());
            GUIStyle valueStyle = GuiStyleCache.GetOrCreate(valueFont, valuePx, FontStyle.Bold);
            valueStyle.alignment = TextAnchor.UpperLeft;

            Font labelFont = theme.GetFont(FontRole.Mono);
            int labelPx = Mathf.RoundToInt(new Rem(0.75f).ToFontPx());
            GUIStyle labelStyle = GuiStyleCache.GetOrCreate(labelFont, labelPx, FontStyle.Normal);
            labelStyle.alignment = TextAnchor.UpperLeft;

            float valueH = new Rem(1.25f).ToPixels();
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
            int valuePx = Mathf.RoundToInt(new Rem(1.125f).ToFontPx());
            GUIStyle valueStyle = GuiStyleCache.GetOrCreate(valueFont, valuePx, FontStyle.Bold);
            float valueW = valueStyle.CalcSize(new GUIContent(value)).x;

            float labelW = 0f;
            if (!string.IsNullOrEmpty(label)) {
                Font labelFont = theme.GetFont(FontRole.Mono);
                int labelPx = Mathf.RoundToInt(new Rem(0.75f).ToFontPx());
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
        return Button.Create(
            label: "CL_MainMenu_Continue_Action".Translate(),
            onClick: () => MainMenuActions.ContinueLatestSave(save.FileName),
            variant: ButtonVariant.Primary,
            fullWidth: true,
            fillHeight: true
        );
    }

    private static LightweaveNode BuildWelcome() {
        return Box.Create(
            padding: EdgeInsets.All(SpacingScale.Md),
            background: BackgroundSpec.Of(ThemeSlot.SurfaceRaised),
            border: BorderSpec.All(new Rem(0.0625f), ThemeSlot.BorderSubtle),
            radius: RadiusSpec.All(RadiusScale.Md),
            children: c => c.Add(Stack.Create(SpacingScale.Xs, s => {
                s.Add(Eyebrow.Create("CL_MainMenu_Welcome_Eyebrow".Translate()));
                s.Add(Display.Create("CL_MainMenu_Welcome".Translate(), level: 3, align: TextAlign.Start));
                s.Add(Text.Create("CL_MainMenu_Welcome_Hint".Translate(), color: ThemeSlot.TextSecondary, wrap: true));
            }))
        );
    }


}
