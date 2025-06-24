using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace CosmereFramework.Utils;

public static class UIUtil {
    private static readonly Dictionary<string, bool> CollapsedStates = new Dictionary<string, bool>();

    public static void WithAnchor(TextAnchor anchor, Action draw) {
        TextAnchor prev = Text.Anchor;
        Text.Anchor = anchor;
        draw();
        Text.Anchor = prev;
    }

    public static void WithFont(GameFont font, Action draw) {
        GameFont prev = Text.Font;
        Text.Font = font;
        draw();
        Text.Font = prev;
    }

    public static void WithColor(Color color, Action draw) {
        Color prev = GUI.color;
        GUI.color = color;
        draw();
        GUI.color = prev;
    }

    public static Color GetBestTextColor(Color background) {
        float luminance = 0.299f * background.r + 0.587f * background.g + 0.114f * background.b;
        return luminance > 0.5f ? Color.black : Color.white;
    }

    public static void DrawContrastLabel(Rect rect, string text, Color overlayColor) {
        Text.Anchor = TextAnchor.MiddleCenter;
        Text.Font = GameFont.Small;

        Color originalColor = GUI.color;

        // Draw base label as black (outline)
        GUI.color = Color.black;
        Widgets.Label(new Rect(rect.x + 1, rect.y + 1, rect.width + 1, rect.height + 1), text);

        // Draw foreground label as metal color
        GUI.color = Color.white;
        Widgets.Label(rect, text);

        GUI.color = originalColor;
        Text.Anchor = TextAnchor.UpperLeft;
    }

    public static bool DrawCollapsibleHeader(string label, string key, Listing_Standard listing) {
        if (!CollapsedStates.TryGetValue(key, out bool expanded)) {
            expanded = true;
        }

        Rect headerRect = listing.GetRect(Text.LineHeight);

        string prefix = expanded ? "▼ " : "▶ ";
        WithAnchor(TextAnchor.MiddleLeft, () => Widgets.Label(headerRect, prefix + label));

        if (Widgets.ButtonInvisible(headerRect)) {
            expanded = !expanded;
            CollapsedStates[key] = expanded;
            SoundDefOf.Tick_High.PlayOneShotOnCamera();
        }

        listing.Gap(2f);

        return expanded;
    }

    public static void DrawIcon(Rect rect, Texture2D icon, Texture? background = null, Material? material = null,
        Color? color = null, float? scale = null, Vector2? offset = null, bool doBorder = true,
        float iconBorderSize = 1f) {
        if (doBorder) {
            GUI.DrawTexture(rect, BaseContent.BlackTex);
            rect = rect.ContractedBy(iconBorderSize);
        }

        GUI.color = color ?? Color.white;
        if (background != null) GenUI.DrawTextureWithMaterial(rect, background, material);

        Rect iconRect = offset == null
            ? rect
            : new Rect(rect.x + offset.Value.x, rect.y + offset.Value.y, rect.width, rect.height);
        Widgets.DrawTextureFitted(iconRect, icon, scale ?? 1f);
        GUI.color = Color.white;
    }
}