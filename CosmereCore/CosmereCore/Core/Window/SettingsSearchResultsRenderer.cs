using Cosmere.Core.Settings.Search;
using Cosmere.Core.UI;
using Cosmere.Core.UI.Skin;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.Window;

public sealed class SettingsSearchResultsRenderer {
    private const float RowHeight = 46f;
    private const float RowGap = 4f;

    private Vector2 scroll = Vector2.zero;

    public SettingsSearchDocument? Draw(
        Rect viewportRect,
        ISystemSkin skin,
        IReadOnlyList<SettingsSearchResult> results,
        string query
    ) {
        if (results.Count == 0) {
            UIText.EllipsisLabel(
                viewportRect.TopPartPixels(RowHeight).ContractedBy(SettingsWindowLayout.ContentPadding, 0f),
                (string)"CC_Settings_Search_NoResults".Translate(query.Named("QUERY")),
                GameFont.Small,
                TextAnchor.MiddleLeft,
                new Color(0.62f, 0.64f, 0.68f)
            );
            return null;
        }

        SettingsSearchDocument? chosen = null;
        float contentHeight = results.Count * (RowHeight + RowGap);
        Rect viewRect = new Rect(0f, 0f, viewportRect.width - 20f, contentHeight);

        Widgets.BeginScrollView(viewportRect, ref scroll, viewRect);
        try {
            for (int i = 0; i < results.Count; i++) {
                Rect row = new Rect(0f, i * (RowHeight + RowGap), viewRect.width, RowHeight);
                if (row.yMax < scroll.y || row.y > scroll.y + viewportRect.height) continue;

                if (DrawRow(row, skin, results[i].Document)) chosen = results[i].Document;
            }
        } finally {
            Widgets.EndScrollView();
        }

        return chosen;
    }

    public void Reset() {
        scroll = Vector2.zero;
    }

    private static bool DrawRow(Rect rect, ISystemSkin skin, SettingsSearchDocument document) {
        Widgets.DrawHighlightIfMouseover(rect);
        MouseoverSounds.DoRegion(rect);

        Rect inner = rect.ContractedBy(SettingsWindowLayout.ContentPadding, 4f);
        Rect labelRect = inner.TopPartPixels(inner.height * 0.58f);
        Rect pathRect = inner.BottomPartPixels(inner.height * 0.42f);

        UIText.EllipsisLabel(
            labelRect,
            document.Label,
            GameFont.Small,
            TextAnchor.MiddleLeft,
            skin.HeaderTextColor
        );
        UIText.EllipsisLabel(
            pathRect,
            document.SystemName + " / " + document.SectionName,
            GameFont.Tiny,
            TextAnchor.MiddleLeft,
            new Color(0.62f, 0.64f, 0.68f)
        );

        if (document.Description is { Length: > 0 } description) {
            TooltipHandler.TipRegion(rect, description);
        }

        return Widgets.ButtonInvisible(rect);
    }
}
