using UnityEngine;
using Verse;

namespace CosmereFramework.Util;

public static class UIUtil {
    public static void DrawIcon(
        Rect rect,
        Texture2D icon,
        Texture? background = null,
        Material? material = null,
        Color? color = null,
        float? scale = null,
        Vector2? offset = null,
        bool doBorder = true,
        Texture? borderTexture = null,
        float iconBorderSize = 1f
    ) {
        if (doBorder) {
            GUI.DrawTexture(rect, borderTexture ?? BaseContent.BlackTex);
            rect = rect.ContractedBy(iconBorderSize);
        }

        using (new TextBlock(color ?? Color.white)) {
            if (background != null) GenUI.DrawTextureWithMaterial(rect, background, material);

            Rect iconRect = offset == null
                ? rect
                : new Rect(rect.x + offset.Value.x, rect.y + offset.Value.y, rect.width, rect.height);
            Widgets.DrawTextureFitted(iconRect, icon, scale ?? 1f);
        }
    }
}