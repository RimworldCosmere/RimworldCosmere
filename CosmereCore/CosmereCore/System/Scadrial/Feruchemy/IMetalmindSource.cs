using Cosmere.Core.Def;

namespace Cosmere.System.Scadrial.Feruchemy;

public interface IMetalmindSource {
    float StoredAmount { get; }

    /// <summary>
    ///     Gets the charge put here by Compounding. Shares MaxAmount with StoredAmount, and pays out
    ///     far harder when tapped.
    /// </summary>
    float CompoundedAmount { get; }

    float MaxAmount { get; }

    bool CanStore { get; }

    bool CanTap { get; }

    bool CanTapCompounded { get; }

    /// <summary>
    ///     Gets a value indicating whether this metalmind sits inside the body, which is the only kind that can be compounded
    ///     into. Worn bands and earrings still hold compounded charge once it is there.
    /// </summary>
    bool IsImplanted { get; }

    bool Equipped { get; }

    MetalDef? Metal { get; }

    /// Whether this can be compounded INTO. Distinct from <see cref="AddCompounded"/>,
    /// which stays open so an explanted implant can hand charge back to the item.
    bool CanStoreCompounded { get; }

    float TotalStored { get; }

    float FreeSpace { get; }

    /// <summary>
    ///     Gets a value indicating whether the metalmind is spent. Compounded charge consumes the metalmind carrying it, so
    ///     capacity falls as it is drawn, and this reads true once nothing is left to hold.
    /// </summary>
    bool IsBurnedOut { get; }

    /// <summary>
    ///     Gets an id that is stable across saves and across other metalminds being destroyed, so the
    ///     player's chosen target survives a burn-out somewhere else in the list.
    /// </summary>
    string SourceId { get; }

    // What the target picker calls this one.
    string SourceLabel { get; }

    float AddStored(float amount, ConnectionKey? ledgerKey = null);

    float ConsumeStored(float amount, ConnectionKey? ledgerKey = null);

    float AddCompounded(float amount);

    float ConsumeCompounded(float amount, ConnectionKey? ledgerKey = null);

    // Charge on this metalmind filed under that key; 0 if it never has been.
    float StoredFor(ConnectionKey ledgerKey);
}
