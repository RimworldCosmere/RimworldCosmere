using UnityEngine;

namespace Cosmere.Core.Ability.Autocast;

/// <summary>
///     Something a pawn can hold autocast rules against: an ability is cast, a feruchemical
///     dial is held. Providers describe their own, so the panel doesn't need to know which is which.
/// </summary>
public readonly struct AutocastTarget {
    public AutocastTarget(AutocastRuleKind kind, string id, string label, Texture2D? icon) {
        Kind = kind;
        Id = id;
        Label = label;
        Icon = icon;
    }

    public AutocastRuleKind Kind { get; }

    // The ability's defName, or the metal's, depending on Kind.
    public string Id { get; }

    public string Label { get; }

    public Texture2D? Icon { get; }
}
