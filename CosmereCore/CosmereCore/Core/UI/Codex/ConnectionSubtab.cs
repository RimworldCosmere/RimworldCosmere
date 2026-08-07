using Cosmere.Core.Def;
using Cosmere.Core.ShardConnection;
using Cosmere.Core.Util;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.UI.Codex;

/// <summary>
///     The Connection subtab: every Shard, and how strongly this pawn is tied to it.
/// </summary>
/// <remarks>
///     All eight Shards render always, greyed rather than hidden when the scenario switched one
///     off. Hiding them would make the list change shape between saves, and the absence of a
///     Shard is itself worth seeing - a Final Empire game has no Harmony, and that is the point.
/// </remarks>
public static class ConnectionSubtab {
    private const float RowHeight = 34f;
    private const float RowGap = 4f;
    private const float GroupHeaderHeight = 26f;
    private const float GroupGap = 10f;
    private const float LabelWidth = 132f;
    private const float ValueWidth = 96f;
    private const float BarInset = 8f;
    private const float ScrollbarWidth = 20f;

    public static void Draw(Rect rect, Pawn pawn, CodexState state) {
        List<(string label, List<ShardDef> shards)> groups = Grouped();
        if (groups.Count == 0) {
            using (new TextBlock(GameFont.Small, TextAnchor.MiddleCenter, ConnectionPalette.Disabled)) {
                Widgets.Label(rect, "CC_Connection_NoShards".Translate());
            }

            return;
        }

        float height = 0f;
        for (int i = 0; i < groups.Count; i++) {
            height += GroupHeaderHeight + groups[i].shards.Count * (RowHeight + RowGap) + GroupGap;
        }

        Rect view = new Rect(0f, 0f, rect.width - ScrollbarWidth, height);
        Widgets.BeginScrollView(rect, ref state.ConnectionScroll, view);

        float y = 0f;
        for (int g = 0; g < groups.Count; g++) {
            y = DrawGroup(view.width, y, groups[g].label, groups[g].shards, pawn);
        }

        Widgets.EndScrollView();
    }

    private static float DrawGroup(float width, float y, string label, List<ShardDef> shards, Pawn pawn) {
        using (new TextBlock(GameFont.Small, TextAnchor.LowerLeft, ConnectionPalette.GroupLabel)) {
            Widgets.Label(new Rect(0f, y, width, GroupHeaderHeight), label);
        }

        y += GroupHeaderHeight;
        Widgets.DrawLineHorizontal(0f, y - 3f, width, ConnectionPalette.Divider);

        for (int i = 0; i < shards.Count; i++) {
            DrawRow(new Rect(0f, y, width, RowHeight), shards[i], pawn);
            y += RowHeight + RowGap;
        }

        return y + GroupGap;
    }

    private static void DrawRow(Rect row, ShardDef shard, Pawn pawn) {
        bool enabled = ShardUtility.IsEnabled(shard);
        int strength = enabled ? ConnectionUtility.StrengthOf(pawn, shard) : 0;
        ConnectionTier tier = ConnectionMath.TierOf(strength);
        Color tint = enabled ? ConnectionPalette.ForTier(tier) : ConnectionPalette.Disabled;

        Widgets.DrawBoxSolid(row, ConnectionPalette.Ground);

        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, enabled ? Color.white : ConnectionPalette.Disabled)) {
            Widgets.Label(row.LeftPartPixels(LabelWidth).ContractedBy(BarInset, 0f), shard.LabelCap);
        }

        // The bar is the row. A number alone makes eight pawns impossible to compare; a filled
        // track turns the set into a profile you can read without arithmetic.
        Rect track = new Rect(
            row.x + LabelWidth,
            row.y + BarInset,
            row.width - LabelWidth - ValueWidth,
            row.height - BarInset * 2f
        );
        Widgets.DrawBoxSolid(track, ConnectionPalette.Track);

        if (strength > 0) {
            Widgets.DrawBoxSolid(
                new Rect(track.x, track.y, track.width * (strength / (float)ConnectionMath.Max), track.height),
                tint
            );
        }

        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleRight, tint)) {
            string tierWord = ConnectionPalette.LabelKeyFor(tier).Translate();
            string readout = enabled
                ? $"{tierWord} {strength}"
                : "CC_Connection_Absent".Translate().Resolve();
            Widgets.Label(new Rect(row.xMax - ValueWidth, row.y, ValueWidth - BarInset, row.height), readout);
        }

        Widgets.DrawHighlightIfMouseover(row);
        MouseoverSounds.DoRegion(row);
        TooltipHandler.TipRegion(
            row,
            enabled
                ? "CC_Connection_RowTip".Translate(
                    shard.LabelCap.Named("SHARD"),
                    ConnectionPalette.LabelKeyFor(tier).Translate().Named("TIER"),
                    strength.Named("VALUE")
                )
                : "CC_Connection_AbsentTip".Translate(shard.LabelCap.Named("SHARD"))
        );
    }

    /// <summary>
    ///     Shards grouped by the world that holds them, read off CosmereWorldDef.nativeShards so
    ///     a new shardworld groups itself. A Shard no loaded world claims - Autonomy, with
    ///     Taldain absent - falls into its own group rather than disappearing.
    /// </summary>
    private static List<(string, List<ShardDef>)> Grouped() {
        List<(string, List<ShardDef>)> groups = [];
        List<ShardDef> all = DefDatabase<ShardDef>.AllDefsListForReading;
        HashSet<ShardDef> claimed = [];

        List<CosmereWorldDef> worlds = WorldUtility.All;
        for (int i = 0; i < worlds.Count; i++) {
            if (worlds[i].crossWorld) continue;

            List<ShardDef> mine = [];
            for (int j = 0; j < worlds[i].nativeShards.Count; j++) {
                ShardDef shard = worlds[i].nativeShards[j];
                if (claimed.Add(shard)) mine.Add(shard);
            }

            if (mine.Count > 0) groups.Add((worlds[i].LabelCap, mine));
        }

        List<ShardDef> unclaimed = [];
        for (int i = 0; i < all.Count; i++) {
            if (!claimed.Contains(all[i])) unclaimed.Add(all[i]);
        }

        if (unclaimed.Count > 0) {
            groups.Add(("CC_Connection_Group_Unclaimed".Translate().Resolve(), unclaimed));
        }

        return groups;
    }
}
