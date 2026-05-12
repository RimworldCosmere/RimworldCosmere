using Cosmere.Lightweave.Layout;
using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.MainMenu;

[StaticConstructorOnStartup]
public static class TitleHero {
    private const float TitleAspect = 1032f / 146f;
    private static readonly Texture2D TitleTex = ContentFinder<Texture2D>.Get("UI/HeroArt/GameTitle");

    public static LightweaveNode Create() {
        return Stack.Create(
            new Rem(0.875f),
            s => {
                s.Add(CreateLogo());
                string sub = "CL_MainMenu_WordmarkSub".Translate();
                if (!string.IsNullOrEmpty(sub)) {
                    s.Add(CreateSubtitle(sub));
                }
            }
        );
    }

    private static LightweaveNode CreateLogo() {
        LightweaveNode logo = NodeBuilder.New("RimWorldLogo");
        logo.PreferredHeight = new Rem(7.2f).ToPixels();
        logo.Paint = (rect, _) => {
            if (TitleTex == null) return;
            float h = rect.height;
            float w = h * TitleAspect;
            if (w > rect.width) {
                w = rect.width;
                h = w / TitleAspect;
            }

            Rect target = new Rect(
                rect.x + (rect.width - w) * 0.5f,
                rect.y + (rect.height - h) * 0.5f,
                w,
                h
            );
            Color saved = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTexture(RectSnap.Snap(target), TitleTex, ScaleMode.ScaleToFit);
            GUI.color = saved;
        };
        return logo;
    }

    private static LightweaveNode CreateSubtitle(string raw) {
        LightweaveNode node = NodeBuilder.New("RimWorldSubtitle");
        node.PreferredHeight = new Rem(1.5f).ToPixels();
        node.Paint = (rect, _) => {
            if (Event.current.type != EventType.Repaint) {
                return;
            }

            Theme.Theme theme = RenderContext.Current.Theme;
            Font font = theme.GetFont(FontRole.Display);
            int pixelSize = Mathf.RoundToInt(new Rem(1.375f).ToFontPx());
            GUIStyle style = GuiStyleCache.GetOrCreate(font, pixelSize, FontStyle.Italic);
            style.alignment = TextAnchor.MiddleRight;

            float logoH = new Rem(7.2f).ToPixels();
            float wordmarkW = Mathf.Min(logoH * TitleAspect, rect.width);
            float wordmarkRight = rect.x + (rect.width + wordmarkW) * 0.5f;

            float tracking = pixelSize * 0.02f;
            float totalW = 0f;
            for (int i = 0; i < raw.Length; i++) {
                GUIContent gc = new GUIContent(raw[i].ToString());
                totalW += style.CalcSize(gc).x;
                if (i < raw.Length - 1) {
                    totalW += tracking;
                }
            }

            float cursor = wordmarkRight - totalW;
            Color saved = GUI.color;
            GUI.color = theme.GetColor(ThemeSlot.TextMuted);
            for (int i = 0; i < raw.Length; i++) {
                string ch = raw[i].ToString();
                GUIContent gc = new GUIContent(ch);
                float w = style.CalcSize(gc).x;
                GUI.Label(RectSnap.Snap(new Rect(cursor, rect.y, w, rect.height)), ch, style);
                cursor += w + tracking;
            }

            GUI.color = saved;
        };
        return node;
    }
}