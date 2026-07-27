using UnityEngine;

namespace Cosmere.Core.Ability.Autocast;

// Something a pawn can hold autocast rules against. An ability is cast; a
// feruchemical dial is held. Providers describe their own, so the autocast panel
// does not have to know which arts work which way.
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
