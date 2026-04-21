using RimWorld;
using UnityEngine;

namespace Cosmere.Core.UI.Radial;

public sealed record RadialLeaf(
    string LeafId,
    string Label,
    Texture2D? Icon,
    RadialActionKind Kind,
    AbilityDef? AbilityDef,
    bool IsActive,
    bool IsFlaring,
    bool IsSustained,
    bool IsLocked,
    string? LockReason,
    float? ReserveFraction,
    bool HasInsufficientResources,
    string? CostHint,
    int CooldownTicksRemaining
);
