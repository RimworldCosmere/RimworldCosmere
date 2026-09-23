namespace Cosmere.Core.UI.Model;

public sealed record ResourceBar(
    string Label,
    float Current,
    float Max,
    float? TargetValue = null
) {
    public float Fraction => Max <= 0f ? 0f : Current / Max;
}
