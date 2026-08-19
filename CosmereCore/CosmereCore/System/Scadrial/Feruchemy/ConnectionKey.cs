namespace Cosmere.System.Scadrial.Feruchemy;

/// <summary>
///     The name a duralumind files charge under. A Shard key carries that Shard's own defName, so a
///     tie to Ruin can never be handed back as a tie to Preservation.
/// </summary>
/// <remarks>
///     A struct with a private constructor, so <see cref="For" /> is the only thing that can make
///     one. The store reads <see cref="Name" /> and never parses it.
/// </remarks>
public readonly struct ConnectionKey {
    private readonly string? name;

    private ConnectionKey(string name) {
        this.name = name;
    }

    public string Name => name ?? string.Empty;

    public static ConnectionKey For(DuraluminLedger ledger, string? shardDefName) {
        if (ledger != DuraluminLedger.Shard || string.IsNullOrEmpty(shardDefName)) {
            return new ConnectionKey(ledger.ToString());
        }

        return new ConnectionKey(ledger + ":" + shardDefName);
    }
}
