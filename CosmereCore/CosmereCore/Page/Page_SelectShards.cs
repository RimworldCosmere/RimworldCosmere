using System.Linq;
using CosmereCore.Defs;
using CosmereCore.Utils;
using CosmereFramework.Utils;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereCore.Pages {
    public class Page_SelectShards : Page {
        private Vector2 scrollPos = Vector2.zero;

        public override string PageTitle => "Select Shards";

        public override void DoWindowContents(Rect inRect) {
            DefDatabase<ShardDef>.AddAllInMods();

            const float topMargin = 40f;
            const float bottomMargin = 60f;
            const float rowHeight = 70f;
            const float planetHeaderHeight = 40f;
            const float checkTextPadding = 12f;

            UIUtil.WithFont(GameFont.Medium,
                () => Widgets.Label(inRect.TopPartPixels(topMargin), "Choose which Shards are active in this game:"));

            var grouped = DefDatabase<ShardDef>.AllDefsListForReading
                .GroupBy(s => s.planet)
                .OrderBy(g => g.Key == "N/A" ? 0 : 1)
                .ThenBy(g => g.Key)
                .ToList();

            var totalHeight = grouped.Sum(g => g.Count() * rowHeight + planetHeaderHeight);
            var scrollArea = new Rect(0f, topMargin + 10f, inRect.width, inRect.height - topMargin - bottomMargin);
            var viewRect = new Rect(0f, 0f, scrollArea.width - 16f, totalHeight);

            Widgets.BeginScrollView(scrollArea, ref scrollPos, viewRect);
            var y = 0f;

            foreach (var group in grouped) {
                var planetLabelRect = new Rect(0f, y, viewRect.width, planetHeaderHeight);
                UIUtil.WithFont(GameFont.Medium, () => Widgets.Label(planetLabelRect, group.Key));
                y += planetHeaderHeight;

                Widgets.DrawLineHorizontal(0f, y - 6f, viewRect.width);

                foreach (var shard in group) {
                    var rowRect = new Rect(0f, y, viewRect.width, rowHeight);
                    var checkRect = new Rect(8f, y + 10f, 24f, 24f);

                    var isEnabled = ShardUtility.IsEnabled(shard);
                    var isBlockedByOther = !isEnabled && shard.mutuallyExclusiveWith?.Any(ShardUtility.IsEnabled) == true;

                    var originalGUIState = GUI.enabled;
                    GUI.enabled = !isBlockedByOther;

                    var labelRect = new Rect(checkRect.xMax + checkTextPadding, y + 6f,
                        rowRect.width - checkRect.xMax - 20f, 24f);
                    var toggled = isEnabled;
                    Widgets.CheckboxLabeled(labelRect, shard.label.CapitalizeFirst(), ref toggled, isBlockedByOther);

                    if (toggled != isEnabled) {
                        if (toggled) {
                            ShardUtility.Enable(shard);
                            Messages.Message($"Enabled shard: {shard.label.CapitalizeFirst()}",
                                MessageTypeDefOf.PositiveEvent);
                        }
                        else {
                            ShardUtility.Shards.enabledShardDefs.Remove(shard);
                            Messages.Message($"Disabled shard: {shard.label.CapitalizeFirst()}",
                                MessageTypeDefOf.NeutralEvent);
                        }
                    }

                    GUI.enabled = originalGUIState;

                    var descRect = new Rect(checkRect.xMax + checkTextPadding, y + 30f,
                        rowRect.width - checkRect.xMax - 20f, 30f);
                    UIUtil.WithFont(GameFont.Tiny,
                        () => Widgets.Label(descRect, shard.description.Truncate(descRect.width - 10f)));

                    y += rowHeight;
                }
            }

            Widgets.EndScrollView();
            DoBottomButtons(inRect);
        }
    }
}