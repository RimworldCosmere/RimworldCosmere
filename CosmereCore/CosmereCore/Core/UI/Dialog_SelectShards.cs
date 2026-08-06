using System.Collections.Generic;
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
    private readonly List<ShardDef> shards;
    private readonly HashSet<string> required;
    private Vector2 scrollPos = Vector2.zero;

    public Dialog_SelectShards(List<ShardDef> permitted, IEnumerable<string>? requiredShards = null) {
        shards = permitted;
        required = requiredShards == null ? [] : [..requiredShards];

        forcePause = true;
        absorbInputAroundWindow = true;
        closeOnClickedOutside = true;
        doCloseX = true;
        doCloseButton = true;
    }

    public override Vector2 InitialSize => new Vector2(640f, 620f);

    public override void DoWindowContents(Rect inRect) {
        const float TitleHeight = 40f;
        const float RowHeight = 70f;
        const float PlanetHeaderHeight = 40f;
        const float CheckTextPadding = 12f;
        const float ScrollbarWidth = 16f;

        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft, Color.white)) {
            Widgets.Label(inRect.TopPartPixels(TitleHeight), "CC_SelectShard_Label".Translate());
        }

        List<IGrouping<string?, ShardDef>> grouped = shards
            .GroupBy(s => s.planet)
            .OrderBy(g => g.Key == "N/A" ? 0 : 1)
            .ThenBy(g => g.Key)
            .ToList();

        float totalHeight = 0f;
        for (int i = 0; i < grouped.Count; i++) {
            totalHeight += grouped[i].Count() * RowHeight + PlanetHeaderHeight;
        }

        // CloseButSize leaves room for the close button the window draws for us.
        Rect scrollArea = new Rect(
            0f,
            TitleHeight + 10f,
            inRect.width,
            inRect.height - TitleHeight - 10f - CloseButSize.y - 10f
        );
        Rect viewRect = new Rect(0f, 0f, scrollArea.width - ScrollbarWidth, totalHeight);

        Widgets.BeginScrollView(scrollArea, ref scrollPos, viewRect);
        float y = 0f;

        foreach (IGrouping<string?, ShardDef> group in grouped) {
            using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft, Color.white)) {
                Widgets.Label(new Rect(0f, y, viewRect.width, PlanetHeaderHeight), group.Key);
            }

            y += PlanetHeaderHeight;
            Widgets.DrawLineHorizontal(0f, y - 6f, viewRect.width);

            foreach (ShardDef shard in group) {
                y = DrawShardRow(shard, y, viewRect.width, RowHeight, CheckTextPadding, required.Contains(shard.defName));
            }
        }

        Widgets.EndScrollView();
    }

    private static float DrawShardRow(
        ShardDef shard, float y, float width, float rowHeight, float padding, bool isRequired
    ) {
        Rect checkRect = new Rect(8f, y + 10f, 24f, 24f);

        bool isEnabled = ShardUtility.AreAnyEnabled(shard);
        bool blocked = !isEnabled && shard.mutuallyExclusiveWith.Any(ShardUtility.IsEnabled);

        // A scenario's own Shards are the premise of the story it tells, so they cannot be
        // taken away - but nothing stops a player adding more on top.
        bool wasGuiEnabled = GUI.enabled;
        GUI.enabled = !blocked && !isRequired;

        Rect labelRect = new Rect(checkRect.xMax + padding, y + 6f, width - checkRect.xMax - 20f, 24f);
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

        Rect descRect = new Rect(checkRect.xMax + padding, y + 30f, width - checkRect.xMax - 20f, 30f);
        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, ColoredText.SubtleGrayColor)) {
            Widgets.Label(descRect, shard.description.Truncate(descRect.width - 10f));
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
