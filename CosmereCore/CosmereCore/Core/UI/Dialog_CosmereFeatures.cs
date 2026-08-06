using System.Collections.Generic;
using Cosmere.Core.Def;
using Cosmere.Core.UI.Skin;
using Cosmere.Core.Util;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Core.UI;

/// <summary>
///     Picking which phenomena happen on a world that belongs to none of them, one tab per
///     shardworld.
/// </summary>
/// <remarks>
///     Only reachable on the cross-world save. Every other world answers this for itself - you do
///     not choose whether Roshar has highstorms.
///     <para>
///         The tabs are built from the features that actually loaded, grouped by the world each
///         declares. A new shard mod shipping its own Features.xml gets a tab with no change
///         here, and its colour comes from the system skin its world names.
///     </para>
/// </remarks>
public class Dialog_CosmereFeatures : Verse.Window {
    private readonly List<CosmereWorldDef> tabs = [];
    private readonly Dictionary<CosmereWorldDef, List<CosmereFeatureDef>> byWorld = [];

    private CosmereWorldDef? active;
    private Vector2 scrollPos = Vector2.zero;

    public Dialog_CosmereFeatures() {
        forcePause = true;
        absorbInputAroundWindow = true;
        closeOnClickedOutside = true;
        doCloseX = true;
        doCloseButton = true;

        Group(FeatureUtility.All);
        active = tabs.Count > 0 ? tabs[0] : null;
    }

    public override Vector2 InitialSize => new Vector2(680f, 620f);

    /// <summary>One tab per world that actually shipped a feature, in the worlds' own order.</summary>
    private void Group(List<CosmereFeatureDef> features) {
        for (int i = 0; i < features.Count; i++) {
            CosmereWorldDef? world = features[i].world;
            if (world == null) continue;

            if (!byWorld.TryGetValue(world, out List<CosmereFeatureDef>? list)) {
                list = [];
                byWorld[world] = list;
                tabs.Add(world);
            }

            list.Add(features[i]);
        }

        tabs.SortBy(w => w.listOrder, w => w.defName);
    }

    public override void DoWindowContents(Rect inRect) {
        const float TitleHeight = 34f;
        const float BlurbHeight = 44f;
        const float RowHeight = 62f;
        const float ScrollbarWidth = 16f;
        const float Gap = 8f;

        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft, Color.white)) {
            Widgets.Label(inRect.TopPartPixels(TitleHeight), "CC_Features_Title".Translate());
        }

        using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, ColoredText.SubtleGrayColor)) {
            Widgets.Label(new Rect(0f, TitleHeight, inRect.width, BlurbHeight), "CC_Features_Blurb".Translate());
        }

        float y = TitleHeight + BlurbHeight;

        if (tabs.Count == 0) {
            using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, ColoredText.SubtleGrayColor)) {
                Widgets.Label(new Rect(0f, y, inRect.width, 40f), "CC_Features_None".Translate());
            }

            return;
        }

        // A single world needs no tabs - the title already says which one it is.
        if (tabs.Count > 1) {
            if (TabStrip.Draw(new Rect(0f, y, inRect.width, TabStrip.Height), BuildTabs(), active!, out CosmereWorldDef picked)) {
                active = picked;
                scrollPos = Vector2.zero;
            }

            y += TabStrip.Height + Gap;
        }

        List<CosmereFeatureDef> shown = active != null && byWorld.TryGetValue(active, out List<CosmereFeatureDef>? list)
            ? list
            : [];

        Rect scrollArea = new Rect(0f, y, inRect.width, inRect.height - y - CloseButSize.y - 10f);
        Rect viewRect = new Rect(0f, 0f, scrollArea.width - ScrollbarWidth, shown.Count * RowHeight);

        Widgets.BeginScrollView(scrollArea, ref scrollPos, viewRect);
        float rowY = 0f;
        for (int i = 0; i < shown.Count; i++) {
            rowY = DrawFeatureRow(shown[i], rowY, viewRect.width, RowHeight);
        }

        Widgets.EndScrollView();
    }

    private List<TabStripItem<CosmereWorldDef>> BuildTabs() {
        List<TabStripItem<CosmereWorldDef>> items = [];
        for (int i = 0; i < tabs.Count; i++) {
            CosmereWorldDef world = tabs[i];
            ISystemSkin skin = SystemSkinRegistry.ForOrFallback(world.SkinId);
            int on = CountEnabled(byWorld[world]);

            items.Add(new TabStripItem<CosmereWorldDef>(
                world,
                world.LabelCap,
                skin.AccentColor,
                "CC_Features_TabTip".Translate(on.Named("ON"), byWorld[world].Count.Named("TOTAL")).Resolve()
            ));
        }

        return items;
    }

    private static int CountEnabled(List<CosmereFeatureDef> features) {
        int count = 0;
        for (int i = 0; i < features.Count; i++) {
            if (FeatureUtility.IsChosen(features[i])) count++;
        }

        return count;
    }

    private static float DrawFeatureRow(CosmereFeatureDef feature, float y, float width, float rowHeight) {
        Rect row = new Rect(0f, y, width, rowHeight);
        Widgets.DrawHighlightIfMouseover(row);

        // A phenomenon whose Shard is switched off cannot happen whatever the player ticks, so
        // say so rather than offering a checkbox that does nothing.
        bool shardsPresent = feature.anyOfShards.Count == 0 ||
            ShardUtility.AreAnyEnabled(feature.anyOfShards.ToArray());

        bool wasGuiEnabled = GUI.enabled;
        GUI.enabled = shardsPresent;

        bool chosen = shardsPresent && FeatureUtility.IsChosen(feature);
        bool toggled = chosen;
        Widgets.CheckboxLabeled(new Rect(8f, y + 4f, width - 28f, 24f), feature.LabelCap, ref toggled);

        if (toggled != chosen) FeatureUtility.Set(feature, toggled);

        GUI.enabled = wasGuiEnabled;

        using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, ColoredText.SubtleGrayColor)) {
            Widgets.Label(new Rect(30f, y + 28f, width - 50f, rowHeight - 30f), feature.description);
        }

        if (!shardsPresent) {
            TooltipHandler.TipRegion(
                row,
                "CC_Features_NeedsShard".Translate(
                    string.Join(", ", feature.anyOfShards.Select(s => s.LabelCap)).Named("SHARDS")
                )
            );
        }

        return y + rowHeight;
    }
}
