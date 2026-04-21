using RimWorld;

namespace Cosmere.Core.UI.Model;

public sealed record AbilityEntry(
    AbilityDef Def,
    bool CanCast,
    int CooldownRemainingTicks,
    bool IsActive,
    bool IsFlaring,
    bool IsLocked,
    string? LockReason
);
