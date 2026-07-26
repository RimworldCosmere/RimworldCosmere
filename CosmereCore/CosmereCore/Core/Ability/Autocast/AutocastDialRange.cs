using UnityEngine;

namespace Cosmere.Core.Ability.Autocast;

/// A dial reads as two directions away from rest rather than as one number, and
/// both the editor and the rule summary need to speak of it that way. Kept here
/// so neither has to know that fifty is the middle.
public static class AutocastDialRange {
    private const float FallbackIdle = 50f;
    private const float FallbackMin = 0f;
    private const float FallbackMax = 100f;

    public static float Idle(AutocastRuleKind kind) {
        return AutocastDialRegistry.For(kind)?.IdleTarget ?? FallbackIdle;
    }

    public static float Min(AutocastRuleKind kind) {
        return AutocastDialRegistry.For(kind)?.MinTarget ?? FallbackMin;
    }

    public static float Max(AutocastRuleKind kind) {
        return AutocastDialRegistry.For(kind)?.MaxTarget ?? FallbackMax;
    }

    public static bool IsTapping(AutocastRuleKind kind, float target) {
        return target < Idle(kind);
    }

    /// How far from rest, as a fraction of the travel available in that direction.
    public static float Intensity(AutocastRuleKind kind, float target) {
        float idle = Idle(kind);
        float span = target < idle ? idle - Min(kind) : Max(kind) - idle;

        return span <= 0f ? 0f : Mathf.Clamp01(Mathf.Abs(target - idle) / span);
    }

    public static float TargetFor(AutocastRuleKind kind, bool tapping, float intensity) {
        float idle = Idle(kind);
        float clamped = Mathf.Clamp01(intensity);

        return tapping
            ? idle - clamped * (idle - Min(kind))
            : idle + clamped * (Max(kind) - idle);
    }
}
