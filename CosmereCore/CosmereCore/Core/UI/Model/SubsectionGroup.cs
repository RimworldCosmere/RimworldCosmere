using UnityEngine;

namespace Cosmere.Core.UI.Model;

public sealed record SubsectionGroup(
    string SubsectionId,
    string Label,
    Texture2D? Icon,
    List<AbilityEntry> Abilities
);
