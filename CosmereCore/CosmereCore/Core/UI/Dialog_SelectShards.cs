using System.Collections.Generic;
using System.Linq;
using Cosmere.Core.Def;
using Cosmere.Core.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI;

/// <summary>
///     Picking Shards, with each one's description visible and conflicts greyed out.
/// </summary>
/// <remarks>
///     Was a whole page in colony creation. It is a dialog off the world-generation page now, so
///     the world and its Shards are chosen in one place. Two faults did not survive the move:
///     the page called DefDatabase.AddAllInMods() every OnGUI frame, and its title was a
///     hardcoded English string.
/// </remarks>
public class Dialog_SelectShards : Verse.Window {
    private const float TitleHeight = 40f;
    private const float PlanetHeaderHeight = 40f;
    private const float CheckboxIndent = 32f;
    private const float TextPadding = 12f;
    private const float RightMargin = 20f;
    private const float LabelHeight = 24f;
    private const float RowTopPad = 6f;
    private const float RowBottomPad = 12f;
    private const float ScrollbarWidth = 16f;

    private readonly List<IGrouping<string?, ShardDef>> grouped;
    private readonly HashSet<string> required;
    private Vector2 scrollPos = Vector2.zero;

    public Dialog_SelectShards(List<ShardDef> permitted, IEnumerable<string>? requiredShards = null) {
        // Grouped once here rather than in DoWindowContents, which ran it every OnGUI frame.
        grouped = permitted
            .GroupBy(s => s.planet)
            .OrderBy(g => g.Key == "N/A" ? 0 : 1)
            .ThenBy(g => g.Key)
            .ToList();

        required = requiredShards == null ? [] : [..requiredShards];

        forcePause = true;
        absorbInputAroundWindow = true;
        closeOnClickedOutside = true;
        doCloseX = true;
        doCloseButton = true;
    }

    public override Vector2 InitialSize => new Vector2(640f, 620f);

    public override void DoWindowContents(Rect inRect) {
        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft, Color.white)) {
            Widgets.Label(inRect.TopPartPixels(TitleHeight), "CC_SelectShard_Label".Translate());
        }

        // CloseButSize leaves room for the close button the window draws for us.
        Rect scrollArea = new Rect(
            0f,
            TitleHeight + 10f,
            inRect.width,
            inRect.height - TitleHeight - 10f - CloseButSize.y - 10f
        );

        float viewWidth = scrollArea.width - ScrollbarWidth;
        float totalHeight = 0f;
        for (int i = 0; i < grouped.Count; i++) {
            totalHeight += PlanetHeaderHeight;
            foreach (ShardDef shard in grouped[i]) {
                totalHeight += RowHeight(shard, viewWidth);
            }
        }

        Rect viewRect = new Rect(0f, 0f, viewWidth, totalHeight);

        Widgets.BeginScrollView(scrollArea, ref scrollPos, viewRect);
        float y = 0f;

        for (int i = 0; i < grouped.Count; i++) {
            using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft, Color.white)) {
                Widgets.Label(new Rect(0f, y, viewRect.width, PlanetHeaderHeight), grouped[i].Key);
            }

            y += PlanetHeaderHeight;
            Widgets.DrawLineHorizontal(0f, y - 6f, viewRect.width);

            foreach (ShardDef shard in grouped[i]) {
                y = DrawShardRow(shard, y, viewRect.width, required.Contains(shard.defName));
            }
        }

        Widgets.EndScrollView();
    }

    private static float TextWidth(float rowWidth) {
        return rowWidth - CheckboxIndent - RightMargin;
    }

    private static float RowHeight(ShardDef shard, float rowWidth) {
        float descHeight = UIText.WrappedHeight(shard.description, TextWidth(rowWidth), GameFont.Tiny);

        return RowTopPad + LabelHeight + descHeight + RowBottomPad;
    }

    private static float DrawShardRow(ShardDef shard, float y, float width, bool isRequired) {
        float textWidth = TextWidth(width);
        float rowHeight = RowHeight(shard, width);

        bool isEnabled = ShardUtility.AreAnyEnabled(shard);
        bool blocked = !isEnabled && shard.mutuallyExclusiveWith.Any(ShardUtility.IsEnabled);

        // A scenario's own Shards are the premise of its story, so they cannot be unchecked - only added to.
        bool wasGuiEnabled = GUI.enabled;
        GUI.enabled = !blocked && !isRequired;

        Rect labelRect = new Rect(CheckboxIndent + TextPadding, y + RowTopPad, textWidth, LabelHeight);
        bool toggled = isEnabled;
        Widgets.CheckboxLabeled(labelRect, shard.LabelCap, ref toggled, blocked || isRequired);

        if (toggled != isEnabled && !isRequired) {
            if (toggled) {
                ShardUtility.Enable(shard);
            } else {
                ShardUtility.Disable(shard);
            }
        }

        GUI.enabled = wasGuiEnabled;

        Rect descRect = new Rect(labelRect.x, labelRect.yMax, textWidth, rowHeight - RowTopPad - LabelHeight);
        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, ColoredText.SubtleGrayColor)) {
            Text.WordWrap = true;
            Widgets.Label(descRect, shard.description);
        }

        if (isRequired) {
            TooltipHandler.TipRegion(
                new Rect(0f, y, width, rowHeight),
                "CC_SelectShard_Required".Translate(shard.LabelCap.Named("SHARD"))
            );
        } else if (blocked) {
            TooltipHandler.TipRegion(
                new Rect(0f, y, width, rowHeight),
                "CC_SelectShard_Blocked".Translate(shard.LabelCap.Named("SHARD"))
            );
        }

        return y + rowHeight;
    }
}
