using Verse;

namespace Cosmere.Core.Ability.Autocast;

public sealed class AutocastTrigger : IExposable {
    public AutocastTriggerKind Kind;
    public AutocastComparison Comparison;
    public float Threshold;

    public AutocastTrigger() { }

    public AutocastTrigger(AutocastTriggerKind kind, AutocastComparison comparison, float threshold) {
        Kind = kind;
        Comparison = comparison;
        Threshold = threshold;
    }

    public void ExposeData() {
        Scribe_Values.Look(ref Kind, "kind");
        Scribe_Values.Look(ref Comparison, "comparison");
        Scribe_Values.Look(ref Threshold, "threshold");
    }
}
