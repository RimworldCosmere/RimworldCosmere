using RimWorld;

namespace Cosmere.Core.UI.Model;

public sealed record InvestitureAbility(
    string AbilityDefName,
    string Label,
    AbilityDef Def,
    bool IsTargeted,
    bool CanFlare,
    bool IsActive,
    bool IsFlaring
);
