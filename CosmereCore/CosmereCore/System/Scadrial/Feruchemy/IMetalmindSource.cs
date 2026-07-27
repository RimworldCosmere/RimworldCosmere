using Cosmere.Core.Def;

namespace Cosmere.System.Scadrial.Feruchemy;

public interface IMetalmindSource {
    float StoredAmount { get; }

    // Charge put here by Compounding. Shares MaxAmount with StoredAmount, and
    // pays out far harder when tapped.
    float CompoundedAmount { get; }

    float MaxAmount { get; }

    bool CanStore { get; }

    bool CanTap { get; }

    bool CanTapCompounded { get; }

    // Only a metalmind inside the body can be compounded into. Worn bands and
    // earrings still hold compounded charge once it is there.
    bool IsImplanted { get; }

    bool Equipped { get; }

    MetalDef? Metal { get; }

    // Whether this metalmind may be compounded INTO. Distinct from AddCompounded,
    // which is the mechanism and stays open so an explanted implant can hand its
    // compounded charge back to the item.
    bool CanStoreCompounded { get; }

    float TotalStored { get; }

    float FreeSpace { get; }

    // Compounded charge consumes the metalmind carrying it, so capacity falls as
    // it is drawn. True once nothing is left to hold and the metalmind is spent.
    bool IsBurnedOut { get; }

    // Stable across saves and across other metalminds being destroyed, so the
    // player's chosen target survives a burn-out somewhere else in the list.
    string SourceId { get; }

    // What the target picker calls this one.
    string SourceLabel { get; }

    void AddStored(float amount);

    void ConsumeStored(float amount);

    void AddCompounded(float amount);

    void ConsumeCompounded(float amount);
}
