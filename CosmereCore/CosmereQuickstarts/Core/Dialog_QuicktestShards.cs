using Cosmere.Core.Def;
using Cosmere.Core.UI;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Quickstart;

/// <summary>
///     Second step of the vanilla quicktest branch: which Shards the generated game comes up with.
///     Starts on the set that covers Scadrial, Roshar and Taldain at once, since that is what a
///     throwaway test map is usually for.
/// </summary>
public class Dialog_QuicktestShards : Verse.Window {
    private const float WindowWidth = 520f;
    private const float WindowHeight = 600f;
    private const float TitleHeight = 36f;
    private const float RowHeight = 30f;
    private const float PlanetHeaderHeight = 34f;
    private const float ButtonHeight = 40f;

    private readonly HashSet<string> selected = [];
    private Vector2 scrollPos;

    public Dialog_QuicktestShards() {
        forcePause = true;
        absorbInputAroundWindow = true;
        closeOnClickedOutside = true;
        doCloseX = true;
        draggable = true;

        foreach (string defName in CosmereQuicktest.DefaultShards) {
            if (DefDatabase<ShardDef>.GetNamedSilentFail(defName) != null) selected.Add(defName);
        }
    }

    public override Vector2 InitialSize => new Vector2(WindowWidth, WindowHeight);

    public override void DoWindowContents(Rect inRect) {
        using (new TextBlock(GameFont.Medium)) {
            Widgets.Label(inRect.TopPartPixels(TitleHeight), "CC_Quicktest_Shards_Title".Translate());
        }

        float listTop = inRect.y + TitleHeight + Spacing.Get(0.5f);
        float listBottom = inRect.yMax - ButtonHeight - Spacing.Get(0.5f);

        List<IGrouping<string?, ShardDef>> grouped = DefDatabase<ShardDef>.AllDefsListForReading
            .GroupBy(s => s.planet)
            .OrderBy(g => g.Key)
            .ToList();

        Rect scrollArea = new Rect(inRect.x, listTop, inRect.width, listBottom - listTop);
        float totalHeight = grouped.Sum(g => g.Count() * RowHeight + PlanetHeaderHeight);
        Rect viewRect = new Rect(0f, 0f, scrollArea.width - 16f, totalHeight);

        Widgets.BeginScrollView(scrollArea, ref scrollPos, viewRect);
        float y = 0f;

        foreach (IGrouping<string?, ShardDef> group in grouped) {
            using (new TextBlock(GameFont.Small, TextAnchor.LowerLeft, ColoredText.SubtleGrayColor)) {
                Widgets.Label(new Rect(0f, y, viewRect.width, PlanetHeaderHeight), group.Key ?? "Unknown");
            }

            y += PlanetHeaderHeight;

            foreach (ShardDef shard in group) {
                Rect row = new Rect(Spacing.Get(0.5f), y, viewRect.width - Spacing.Get(0.5f), RowHeight);
                bool on = selected.Contains(shard.defName);
                bool toggled = on;
                Widgets.CheckboxLabeled(row, shard.label.CapitalizeFirst(), ref toggled);
                TooltipHandler.TipRegion(row, shard.description);

                if (toggled != on) {
                    if (toggled) {
                        selected.Add(shard.defName);
                    } else {
                        selected.Remove(shard.defName);
                    }
                }

                y += RowHeight;
            }
        }

        Widgets.EndScrollView();

        Rect startRect = new Rect(inRect.x, listBottom + Spacing.Get(0.5f), inRect.width, ButtonHeight);
        if (!Widgets.ButtonText(startRect, "CC_Quicktest_Shards_Start".Translate())) return;

        Close();
        CosmereQuicktest.Start(selected.ToList());
    }
}
