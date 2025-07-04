using System;
using System.Collections.Generic;
using System.Linq;
using CosmereFramework.UI;
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


    public static void Dropdown<T>(
        Listing_Standard listing,
        Func<T?, string?> labelGetter,
        T? selected,
        string placeholder,
        Dictionary<string, T> items,
        Action<T?> onSelected,
        string? noOptionsText = "None",
        bool allowNone = true,
        float? widthOverride = null
    ) {
        Rect buttonRect = listing.GetRect(30f);
        if (widthOverride.HasValue) buttonRect.width = widthOverride.Value;

        string label = labelGetter(selected) ?? placeholder;

        if (!Widgets.ButtonText(buttonRect, label)) return;
        List<FloatMenuOption> options = [];
        if (allowNone) {
            options.Add(new FloatMenuOption(noOptionsText, () => onSelected(default)));
        }

        options.AddRange(items.Select(s => new FloatMenuOption(s.Key, () => onSelected(s.Value))));
        Find.WindowStack.Add(new DropdownMenu(buttonRect, options));
    }

    public static Dictionary<string, int> GetEnumValues<T>() where T : Enum {
        return Enum.GetValues(typeof(T))
            .Cast<T>()
            .ToDictionary(l => l.ToString(), level => (int)(object)level);
    }

    public static void IntEnumDropdown<T>(
        Listing_Standard sub,
        T currentValue,
        Action<T?> onSelected,
        bool allowNone = true,
        string? placeholder = null
    ) where T : Enum {
        if (Enum.GetUnderlyingType(typeof(T)) != typeof(int))
            throw new ArgumentException($"Enum type {typeof(T)} must have int as its underlying type.");

        int intValue = (int)(object)currentValue;

        Dropdown(
            sub,
            val => ((T)(object)val).ToString(),
            intValue,
            placeholder,
            GetEnumValues<T>(),
            updatedVal => onSelected((T)(object)updatedVal),
            allowNone: allowNone
        );
    }

    public static void BoolEnumDropdown(
        Listing_Standard sub,
        bool currentValue,
        Action<bool> onSelected
    ) {
        Dropdown(
            sub,
            val => val ? "Yes" : "No",
            currentValue,
            "",
            new Dictionary<string, bool> {
                { "Yes", true },
                { "No", false },
            },
            onSelected,
            allowNone: false
        );
    }
}