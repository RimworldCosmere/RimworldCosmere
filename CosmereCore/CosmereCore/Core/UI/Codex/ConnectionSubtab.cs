using Cosmere.Core.Def;
using Cosmere.Core.ShardConnection;
using Cosmere.Core.Util;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Cosmere.Core.UI.Codex;

public static class ConnectionSubtab {
    private static readonly List<(ConnectionCodexPage tab, string labelKey)> pages = [
        (ConnectionCodexPage.Shards, "CC_Connection_Tab_Shards"),
        (ConnectionCodexPage.Planets, "CC_Connection_Tab_Planets"),
    ];

    private const float ShardRowHeight = 34f;
    private const float PlanetRowHeight = 70f;
    private const float PlanetSummaryHeight = 34f;
    private const float PlanetDetailHeight = 18f;
    private const float RowGap = 4f;
    private const float GroupHeaderHeight = 26f;
    private const float GroupGap = 10f;
    private const float LabelWidth = 132f;
    private const float ValueWidth = 96f;
    private const float BarInset = 8f;
    private const float ScrollbarWidth = 20f;

    public static void DrawTabBar(Rect rect, CodexState state, Color accent) {
        if (SubtabBar.Draw(rect, pages, state.ConnectionPage, accent, out ConnectionCodexPage clicked)) {
            state.ConnectionPage = clicked;
        }
    }

    public static void Draw(Rect rect, Pawn pawn, CodexState state) {
        if (state.ConnectionPage == ConnectionCodexPage.Planets) {
            DrawPlanets(rect, pawn, state);
            return;
        }

        DrawShards(rect, pawn, state);
    }

    private static void DrawShards(Rect rect, Pawn pawn, CodexState state) {
        List<(string label, List<ShardDef> shards)> groups = GroupedShards();
        if (groups.Count == 0) {
            DrawEmpty(rect, "CC_Connection_NoShards");
            return;
        }

        float height = ShardContentHeight(groups);
        ClampScroll(ref state.ShardConnectionScroll, height, rect.height);
        float width = height > rect.height ? rect.width - ScrollbarWidth : rect.width;
        Rect view = new Rect(0f, 0f, width, height);
        Widgets.BeginScrollView(rect, ref state.ShardConnectionScroll, view);

        float y = 0f;
        for (int i = 0; i < groups.Count; i++) {
            y = DrawShardGroup(view.width, y, groups[i].label, groups[i].shards, pawn);
            if (i < groups.Count - 1) y += GroupGap;
        }

        Widgets.EndScrollView();
    }

    private static void DrawPlanets(Rect rect, Pawn pawn, CodexState state) {
        List<WorldConnection> connections = WorldConnectionUtility.AllFor(pawn);
        if (connections.Count == 0) {
            DrawEmpty(rect, "CC_Connection_NoPlanets");
            return;
        }

        float height = connections.Count * PlanetRowHeight + (connections.Count - 1) * RowGap;
        ClampScroll(ref state.WorldConnectionScroll, height, rect.height);
        float width = height > rect.height ? rect.width - ScrollbarWidth : rect.width;
        Rect view = new Rect(0f, 0f, width, height);
        Widgets.BeginScrollView(rect, ref state.WorldConnectionScroll, view);

        float y = 0f;
        for (int i = 0; i < connections.Count; i++) {
            DrawPlanetRow(new Rect(0f, y, view.width, PlanetRowHeight), connections[i]);
            y += PlanetRowHeight + RowGap;
        }

        Widgets.EndScrollView();
    }

    private static float DrawShardGroup(float width, float y, string label, List<ShardDef> shards, Pawn pawn) {
        using (new TextBlock(GameFont.Small, TextAnchor.LowerLeft, ConnectionPalette.GroupLabel)) {
            Widgets.Label(new Rect(0f, y, width, GroupHeaderHeight), label);
        }

        y += GroupHeaderHeight;
        Widgets.DrawLineHorizontal(0f, y - 3f, width, ConnectionPalette.Divider);

        for (int i = 0; i < shards.Count; i++) {
            DrawShardRow(new Rect(0f, y, width, ShardRowHeight), shards[i], pawn);
            y += ShardRowHeight;
            if (i < shards.Count - 1) y += RowGap;
        }

        return y;
    }

    private static void DrawShardRow(Rect row, ShardDef shard, Pawn pawn) {
        bool enabled = ShardUtility.IsEnabled(shard);
        int strength = enabled ? ConnectionUtility.StrengthOf(pawn, shard) : 0;
        ConnectionTier tier = ConnectionMath.TierOf(strength);
        Color tint = enabled ? ConnectionPalette.ForTier(tier) : ConnectionPalette.Disabled;

        Widgets.DrawBoxSolid(row, ConnectionPalette.Ground);
        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, enabled ? Color.white : ConnectionPalette.Disabled)) {
            Widgets.Label(row.LeftPartPixels(LabelWidth).ContractedBy(BarInset, 0f), shard.LabelCap);
        }

        Rect track = ConnectionTrack(row);
        DrawConnectionBar(track, strength, ConnectionMath.Max, tint);

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

    private static void DrawPlanetRow(Rect row, WorldConnection connection) {
        Color tint = connection.Strength > 0 ? ConnectionPalette.Selected : ConnectionPalette.Disabled;
        Color labelColor = connection.Strength > 0 ? Color.white : ConnectionPalette.Disabled;
        Widgets.DrawBoxSolid(row, ConnectionPalette.Ground);

        Rect summary = new Rect(row.x, row.y, row.width, PlanetSummaryHeight);
        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, labelColor)) {
            Rect label = summary.LeftPartPixels(LabelWidth).ContractedBy(BarInset, 0f);
            Widgets.Label(label, connection.World.LabelCap.Truncate(label.width));
        }

        Rect track = ConnectionTrack(summary);
        DrawConnectionBar(track, connection.Strength, ConnectionMath.Max, tint);
        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleRight, tint)) {
            string readout = "CC_PlanetConnection_InfoValue".Translate(
                connection.Strength.Named("LEVEL"),
                ConnectionMath.Max.Named("MAX")
            );
            Widgets.Label(new Rect(summary.xMax - ValueWidth, summary.y, ValueWidth - BarInset, summary.height), readout);
        }

        float detailWidth = (row.width - LabelWidth) / 2f;
        float detailX = row.x + LabelWidth;
        float detailY = row.y + PlanetSummaryHeight;
        Rect ancestry = new Rect(detailX, detailY, detailWidth, PlanetDetailHeight);
        Rect residence = new Rect(ancestry.xMax, detailY, detailWidth, PlanetDetailHeight);
        Rect investiture = new Rect(detailX, detailY + PlanetDetailHeight, detailWidth, PlanetDetailHeight);
        Rect earned = new Rect(investiture.xMax, investiture.y, detailWidth, PlanetDetailHeight);
        using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleLeft, ConnectionPalette.GroupLabel)) {
            Widgets.Label(ancestry, "CC_PlanetConnection_Ancestry".Translate(connection.Ancestry.Named("VALUE")).Truncate(ancestry.width));
            Widgets.Label(residence, "CC_PlanetConnection_Residence".Translate(connection.Residence.Named("VALUE")).Truncate(residence.width));
            Widgets.Label(
                investiture,
                "CC_PlanetConnection_Investiture".Translate(connection.Investiture.Named("VALUE")).Truncate(investiture.width)
            );
            Widgets.Label(earned, "CC_PlanetConnection_Earned".Translate(connection.Earned.Named("VALUE")).Truncate(earned.width));
        }

        Widgets.DrawHighlightIfMouseover(row);
        MouseoverSounds.DoRegion(row);
        TooltipHandler.TipRegion(
            row,
            "CC_PlanetConnection_WorldTip".Translate(
                connection.World.LabelCap.Named("WORLD"),
                connection.Strength.Named("LEVEL"),
                ConnectionMath.Max.Named("MAX"),
                connection.Ancestry.Named("ANCESTRY"),
                connection.Residence.Named("RESIDENCE"),
                connection.Investiture.Named("INVESTITURE"),
                connection.Earned.Named("EARNED")
            )
        );
        if (Widgets.ButtonInvisible(row)) Find.WindowStack.Add(new Dialog_InfoCard(connection.World));
    }

    private static Rect ConnectionTrack(Rect row) {
        return new Rect(
            row.x + LabelWidth,
            row.y + BarInset,
            row.width - LabelWidth - ValueWidth,
            row.height - BarInset * 2f
        );
    }

    private static void DrawConnectionBar(Rect track, int strength, int maximum, Color tint) {
        Widgets.DrawBoxSolid(track, ConnectionPalette.Track);
        if (strength <= 0) return;

        float fill = Mathf.Clamp01(strength / (float)maximum);
        Widgets.DrawBoxSolid(new Rect(track.x, track.y, track.width * fill, track.height), tint);
    }

    private static void DrawEmpty(Rect rect, string labelKey) {
        using (new TextBlock(GameFont.Small, TextAnchor.MiddleCenter, ConnectionPalette.Disabled)) {
            Widgets.Label(rect, labelKey.Translate());
        }
    }

    private static void ClampScroll(ref Vector2 scroll, float contentHeight, float viewportHeight) {
        scroll.y = Mathf.Min(scroll.y, Mathf.Max(0f, contentHeight - viewportHeight));
    }

    private static float ShardContentHeight(List<(string label, List<ShardDef> shards)> groups) {
        float height = 0f;
        for (int i = 0; i < groups.Count; i++) {
            height += GroupHeaderHeight + groups[i].shards.Count * ShardRowHeight;
            height += Mathf.Max(0, groups[i].shards.Count - 1) * RowGap;
            if (i < groups.Count - 1) height += GroupGap;
        }

        return height;
    }

    private static List<(string, List<ShardDef>)> GroupedShards() {
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
