using Cosmere.Lightweave.Rendering;
using Cosmere.Lightweave.Runtime;
using Cosmere.Lightweave.Tokens;
using Cosmere.Lightweave.Types;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.MainMenu;

public static class AccentStripe {
    public static LightweaveNode Create() {
        LightweaveNode node = NodeBuilder.New("AccentStripe");
        node.PreferredHeight = 2f;
        node.Paint = (rect, _) => {
            if (Event.current.type != EventType.Repaint) {
                return;
            }
            Color accent = RenderContext.Current.Theme.GetColor(ThemeSlot.SurfaceAccent);
            Color prev = GUI.color;
            GUI.color = new Color(accent.r, accent.g, accent.b, 0.85f);
            GUI.DrawTexture(RectSnap.Snap(rect), BaseContent.WhiteTex);
            GUI.color = prev;
        };
        return node;
    }

    

    
}

public static class LightweaveWordmark {
    public static LightweaveNode Create() {
        LightweaveNode node = NodeBuilder.New("LightweaveWordmark");
        node.PreferredHeight = new Rem(1.0f).ToPixels();
        node.Paint = (rect, _) => {
            if (Event.current.type != EventType.Repaint) {
                return;
            }
            Color muted = RenderContext.Current.Theme.GetColor(ThemeSlot.TextMuted);
            Color accent = RenderContext.Current.Theme.GetColor(ThemeSlot.SurfaceAccent);
            Color prev = GUI.color;
            GameFont prevFont = Text.Font;
            TextAnchor prevAnchor = Text.Anchor;

            string powered = "CL_MainMenu_PoweredBy".Translate();
            string mark = "Lightweave";
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleCenter;
            Vector2 poweredSize = Text.CalcSize(powered);
            Vector2 markSize = Text.CalcSize(mark);
            float gap = 6f;
            float totalWidth = poweredSize.x + gap + markSize.x;
            float startX = rect.x + (rect.width - totalWidth) * 0.5f;
            float y = rect.y;

            Rect poweredRect = new Rect(startX, y, poweredSize.x, rect.height);
            Rect markRect = new Rect(startX + poweredSize.x + gap, y, markSize.x, rect.height);

            GUI.color = new Color(muted.r, muted.g, muted.b, muted.a * 0.85f);
            Widgets.Label(RectSnap.SnapText(poweredRect), powered);
            GUI.color = new Color(accent.r, accent.g, accent.b, 0.85f);
            Widgets.Label(RectSnap.SnapText(markRect), mark);

            Text.Font = prevFont;
            Text.Anchor = prevAnchor;
            GUI.color = prev;
        };
        return node;
    }

    

    
}
