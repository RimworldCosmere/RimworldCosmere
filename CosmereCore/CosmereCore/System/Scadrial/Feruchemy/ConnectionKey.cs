namespace Cosmere.System.Scadrial.Feruchemy;

/// <summary>
///     The name a duralumind files its charge under. A Shard key carries that Shard's own defName,
///     so a tie to Ruin can never be handed back as a tie to Preservation.
/// </summary>
public static class ConnectionKey {
    public static string For(DuraluminLedger ledger, string? shardDefName) {
        if (ledger != DuraluminLedger.Shard) return ledger.ToString();

        return string.IsNullOrEmpty(shardDefName) ? ledger.ToString() : ledger + ":" + shardDefName;
    }
}
