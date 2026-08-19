using Cosmere.Core.Def;
using Cosmere.Core.UI.Dock;
using Cosmere.Core.Util;
using Cosmere.System.Scadrial.Feruchemy;
using Cosmere.System.Scadrial.Gene;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.UI.Feruchemy;

public static class FeruchemyLedgerRow {
    // Which tie duralumin's dial spends, and - only for the Shard tie - which Shard.
    public static float Draw(Rect inner, float y, Feruchemist gene) {
        if (!gene.StoresConnection) return y;

        float afterLedger = DockDropdownRow.Draw(
            new Rect(inner.x, y, inner.width, DockDropdownRow.Height),
            LedgerLabel(gene),
            LedgerTooltip(gene),
            () => LedgerMenu(gene)
        );

        if (!ShowsShardRow(gene.targetLedger)) return afterLedger;

        return DockDropdownRow.Draw(
            new Rect(inner.x, afterLedger + 6f, inner.width, DockDropdownRow.Height),
            ShardLabel(gene),
            ShardTooltip(),
            () => ShardMenu(gene)
        );
    }

    private static bool ShowsShardRow(DuraluminLedger targetLedger) {
        return targetLedger == DuraluminLedger.Shard;
    }

    public static float HeightFor(Feruchemist gene) {
        return HeightFor(gene.StoresConnection, gene.targetLedger);
    }

    /// How much taller the strip becomes: nothing for a metal that isn't duralumin, one row
    /// for a non-Shard tie, two rows plus the gap between them for Shard.
    public static float HeightFor(bool storesConnection, DuraluminLedger targetLedger) {
        if (!storesConnection) return 0f;

        return ShowsShardRow(targetLedger)
            ? DockDropdownRow.Height * 2f + 6f
            : DockDropdownRow.Height;
    }

    private static string Percent(float held, float max) {
        return max > 0f ? $"{held / max * 100f:0}%" : "0%";
    }

    // Capacity is kept in points; what a metalmind holds is charge. Convert before comparing.
    private static float CapacityCharge(DuraluminLedger ledger) {
        return ConnectionBudget.ChargeForPoints(ConnectionBudget.Capacity(ledger));
    }

    private static string LedgerName(DuraluminLedger ledger) {
        return ledger switch {
            DuraluminLedger.Residence => "CS_Duralumin_Ledger_Residence".Translate().Resolve(),
            DuraluminLedger.Shard => "CS_Duralumin_Ledger_Shard".Translate().Resolve(),
            _ => string.Empty,
        };
    }

    private static string LedgerLabel(Feruchemist gene) {
        string? blocked = gene.LedgerBlockedReason;
        if (blocked != null) return blocked;

        string held = Percent(gene.StoredForLedger, CapacityCharge(gene.targetLedger));
        string name = $"{LedgerName(gene.targetLedger)} ({held})";
        return "CS_Duralumin_LedgerLabel".Translate(name.Named("LEDGER")).Resolve();
    }

    private static string LedgerTooltip(Feruchemist gene) {
        string? blocked = gene.LedgerBlockedReason;
        if (blocked != null) return blocked;

        string units = $"{gene.StoredForLedger:0} / {CapacityCharge(gene.targetLedger):0}";
        return "CS_Duralumin_LedgerTip".Translate(units.Named("UNITS")).Resolve();
    }

    private static List<FloatMenuOption> LedgerMenu(Feruchemist gene) {
        return [
            LedgerOption(gene, DuraluminLedger.Residence),
            LedgerOption(gene, DuraluminLedger.Shard),
        ];
    }

    private static FloatMenuOption LedgerOption(Feruchemist gene, DuraluminLedger ledger) {
        float held = StoredFor(gene, ConnectionKey.For(ledger, gene.targetShardDefName));
        string label = $"{LedgerName(ledger)} ({Percent(held, CapacityCharge(ledger))})";
        return new FloatMenuOption(label, () => gene.targetLedger = ledger);
    }

    /// Scoped the same way Feruchemist.StoredForLedger is, so the dropdown preview never
    /// shows a percentage the row's own label then contradicts.
    private static float StoredFor(Feruchemist gene, ConnectionKey key) {
        float total = 0f;
        List<IMetalmindSource> sources = gene.metalminds;
        string target = gene.targetMetalmindId;
        for (int i = 0; i < sources.Count; i++) {
            if (!MetalmindDistribution.MatchesTarget(sources[i], target)) continue;
            total += sources[i].StoredFor(key);
        }

        return total;
    }

    private static string ShardLabel(Feruchemist gene) {
        string name;
        if (string.IsNullOrEmpty(gene.targetShardDefName)) {
            name = "CS_Duralumin_ShardPickerNone".Translate().Resolve();
        } else {
            ShardDef? shard = DefDatabase<ShardDef>.GetNamedSilentFail(gene.targetShardDefName);
            name = shard != null ? shard.LabelCap : gene.targetShardDefName;
        }

        return "CS_Duralumin_ShardLabel".Translate(name.Named("SHARD")).Resolve();
    }

    private static string ShardTooltip() {
        return "CS_Duralumin_ShardTip".Translate().Resolve();
    }

    // Only the Shards this save has turned on - the same set EnsureShardTarget seeds from.
    private static List<FloatMenuOption> ShardMenu(Feruchemist gene) {
        List<FloatMenuOption> options = [];
        CosmereWorldDef? world = WorldUtility.Primary;
        if (world == null) return options;

        for (int i = 0; i < world.nativeShards.Count; i++) {
            ShardDef shard = world.nativeShards[i];
            if (!ShardUtility.IsEnabled(shard)) continue;

            float held = StoredFor(gene, ConnectionKey.For(DuraluminLedger.Shard, shard.defName));
            string label = $"{shard.LabelCap} ({Percent(held, CapacityCharge(DuraluminLedger.Shard))})";
            string defName = shard.defName;
            options.Add(new FloatMenuOption(label, () => gene.targetShardDefName = defName));
        }

        return options;
    }
}
