using System;

namespace Cosmere.System.Scadrial.Feruchemy;

/// <summary>
///     The arithmetic behind how fast a metalmind fills and empties.
/// </summary>
/// <remarks>
///     Deliberately free of RimWorld and Unity types so the test host can load it. Storing and
///     tapping share one rate on purpose: a metalmind gives back exactly what went in, so there
///     is a single multiplier per metal and a single efficiency term, never one per direction.
/// </remarks>
public static class FeruchemyRate {
    /// <summary>Charge moved per real second with the dial at its stop.</summary>
    public const float MaxTransferPerSecond = 1f;

    public const float MaxSeverity = 20f;

    /// <summary>What a master feruchemist saves over a novice, before savant rank.</summary>
    public const float MaxEfficiency = 2f;

    /// <summary>Charge moved per real second, per point of severity.</summary>
    public const float AmountPerSecond = MaxTransferPerSecond / MaxSeverity;

    public const int MaxSkillLevel = 20;

    /// <summary>
    ///     How much less charge this feruchemist spends for the same effect.
    /// </summary>
    /// <remarks>
    ///     Skill and strength buy duration rather than magnitude - the ladder a metal pays out on
    ///     is the same for everyone, but a master draws on it half as fast. Savant rank multiplies
    ///     on top, and applies to filling as much as to drawing.
    /// </remarks>
    public static float Efficiency(float strength, int skillLevel, float savantMultiplier = 1f) {
        float clampedStrength = Math.Min(Math.Max(strength, 0f), 1f);
        float clampedSkill = Math.Min(Math.Max(skillLevel, 0), MaxSkillLevel) / (float)MaxSkillLevel;
        float raw = (float)Math.Sqrt(clampedStrength * clampedSkill);

        return (1f + (MaxEfficiency - 1f) * raw) * Math.Max(savantMultiplier, 1f);
    }

    /// <summary>Charge moved per real second at this dial setting, in either direction.</summary>
    public static float PerSecond(float severity, float metalRateMultiplier, float efficiency) {
        if (severity <= 0f) return 0f;

        return AmountPerSecond * severity * metalRateMultiplier / Math.Max(efficiency, 0.01f);
    }

    /// <summary>How long a metalmind of this size takes to fill, or to empty. They are the same.</summary>
    public static float SecondsToMove(float amount, float severity, float metalRateMultiplier, float efficiency) {
        float perSecond = PerSecond(severity, metalRateMultiplier, efficiency);

        return perSecond <= 0f ? float.PositiveInfinity : amount / perSecond;
    }
}
