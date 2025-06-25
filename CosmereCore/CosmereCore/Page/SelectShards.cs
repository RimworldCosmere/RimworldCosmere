using System.Collections.Generic;
using System.Linq;
using CosmereCore.Def;
using CosmereCore.Util;
using CosmereFramework.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereCore.Page;

public class SelectShards : RimWorld.Page {
    private Vector2 scrollPos = Vector2.zero;

    public override string PageTitle => "Select Shards";

    public override void DoWindowContents(Rect inRect) {
        DefDatabase<ShardDef>.AddAllInMods();

        const float TopMargin = 40f;
        const float BottomMargin = 60f;
        const float RowHeight = 70f;
        const float PlanetHeaderHeight = 40f;
        const float CheckTextPadding = 12f;

        UIUtil.WithFont(GameFont.Medium,
            () => Widgets.Label(inRect.TopPartPixels(TopMargin), "Choose which Shards are active in this game:"));

        List<IGrouping<string?, ShardDef>> grouped = DefDatabase<ShardDef>.AllDefsListForReading.GroupBy(s => s.planet)
            .OrderBy(g => g.Key == "N/A" ? 0 : 1)
            .ThenBy(g => g.Key)
            .ToList();

        float totalHeight = grouped.Sum(g => g.Count() * RowHeight + PlanetHeaderHeight);
        Rect scrollArea = new Rect(0f, TopMargin + 10f, inRect.width, inRect.height - TopMargin - BottomMargin);
        Rect viewRect = new Rect(0f, 0f, scrollArea.width - 16f, totalHeight);

        Widgets.BeginScrollView(scrollArea, ref scrollPos, viewRect);
        float y = 0f;

        foreach (IGrouping<string?, ShardDef>? group in grouped) {
            Rect planetLabelRect = new Rect(0f, y, viewRect.width, PlanetHeaderHeight);
            UIUtil.WithFont(GameFont.Medium, () => Widgets.Label(planetLabelRect, group.Key));
            y += PlanetHeaderHeight;

            Widgets.DrawLineHorizontal(0f, y - 6f, viewRect.width);

            foreach (ShardDef? shard in group) {
                Rect rowRect = new Rect(0f, y, viewRect.width, RowHeight);
                Rect checkRect = new Rect(8f, y + 10f, 24f, 24f);

                bool isEnabled = ShardUtility.AreAnyEnabled(shard);
                bool isBlockedByOther =
                    !isEnabled && shard.mutuallyExclusiveWith?.Any(ShardUtility.IsEnabled) == true;

                bool originalGUIState = GUI.enabled;
                GUI.enabled = !isBlockedByOther;

                Rect labelRect = new Rect(checkRect.xMax + CheckTextPadding, y + 6f,
                    rowRect.width - checkRect.xMax - 20f, 24f);
                bool toggled = isEnabled;
                Widgets.CheckboxLabeled(labelRect, shard.label.CapitalizeFirst(), ref toggled, isBlockedByOther);

                if (toggled != isEnabled) {
                    if (toggled) {
                        ShardUtility.Enable(shard);
                        Messages.Message($"Enabled shard: {shard.label.CapitalizeFirst()}",
                            MessageTypeDefOf.PositiveEvent);
                    } else {
                        ShardUtility.shards.enabledShardDefs.Remove(shard);
                        Messages.Message($"Disabled shard: {shard.label.CapitalizeFirst()}",
                            MessageTypeDefOf.NeutralEvent);
                    }
                }

                GUI.enabled = originalGUIState;

                Rect descRect = new Rect(checkRect.xMax + CheckTextPadding, y + 30f,
                    rowRect.width - checkRect.xMax - 20f, 30f);
                UIUtil.WithFont(GameFont.Tiny,
                    () => Widgets.Label(descRect, shard.description.Truncate(descRect.width - 10f)));

                y += RowHeight;
            }
        }

        Widgets.EndScrollView();
        DoBottomButtons(inRect);
    }
}