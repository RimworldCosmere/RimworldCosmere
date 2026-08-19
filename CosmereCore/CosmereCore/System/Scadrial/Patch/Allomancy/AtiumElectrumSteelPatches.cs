using Concord;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Scadrial.Patch.Allomancy;

/// <summary>
///     sets the return value then lets the original run: the original wins, so the hit-chance calc below
///     is a no-op. ported unchanged from the harmony prefixes - making it bite is a balance change, not a fix.
/// </summary>
public static class AtiumElectrumSteel {
    /// <summary>
    ///     Returns the roll rather than taking the ControlHandle: Concord needs the handle used only as the
    ///     direct receiver of a control call, so it cant be passed to a shared helper.
    /// </summary>
    internal static bool TryResolveHit(Verb verb, out bool hit) {
        hit = false;
        if (!verb.CurrentTarget.HasThing || verb.CurrentTarget.Thing is not Pawn targetPawn) return false;
        if (verb.caster is not Pawn casterPawn) return false;
        if (casterPawn.Equals(targetPawn)) return false;

        hit = ShouldHit(verb, casterPawn, targetPawn);
        return true;
    }

    private static bool ShouldHit(Verb verb, Pawn casterPawn, Pawn targetPawn) {
        float baseHitChance = 1f;

        float casterAtiumSeverity = casterPawn.health?.hediffSet
                                        ?.GetFirstHediffOfDef(HediffDefOf.Cosmere_Scadrial_Hediff_AtiumBuff)
                                        ?.Severity ??
                                    0f;
        float targetAtiumSeverity =
            targetPawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.Cosmere_Scadrial_Hediff_AtiumBuff)
                ?.Severity ??
            0f;
        float targetSteelBubbleSeverity =
            targetPawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.Cosmere_Scadrial_Hediff_SteelBubble)
                ?.Severity ??
            0f;
        float targetElectrumSeverity =
            targetPawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.Cosmere_Scadrial_Hediff_ElectrumBuff)
                ?.Severity ??
            0f;

        bool targetHasAtium = targetAtiumSeverity > 0f;
        bool targetHasSteelBubble = targetSteelBubbleSeverity > 0f;
        bool targetHasElectrum = targetElectrumSeverity > 0f;

        // Apply defense penalties
        if (targetHasAtium) {
            // Base 50% chance, scales with severity
            float atiumPenalty = 0.5f + targetAtiumSeverity * 0.5f; // Max: 100% penalty (no hit)
            baseHitChance *= 1f - Mathf.Clamp01(atiumPenalty);
        }

        if (targetHasElectrum) {
            // Base 75% chance, scales with severity
            float electrumPenalty = 0.25f + targetElectrumSeverity * 0.5f; // Max 75% reduction
            baseHitChance *= 1f - Mathf.Clamp01(electrumPenalty);
        }

        if (verb is not Verb_CastAbility && targetHasSteelBubble) {
            // Base 75% chance, scales with severity
            float steelPenalty = 0.25f + targetSteelBubbleSeverity * 0.5f;
            baseHitChance *= 1f - Mathf.Clamp01(steelPenalty);
        }

        // If caster has Atium, they partially negate the penalties
        if (casterAtiumSeverity > 0f) {
            float mitigation = casterAtiumSeverity * 0.75f; // Up to +75% recovery
            baseHitChance += mitigation * (1f - baseHitChance); // Scale it back up
        }

        // Clamp final value for safety
        baseHitChance = Mathf.Clamp(baseHitChance, 0.05f, 1f);
        return Rand.Chance(baseHitChance);
    }
}

[Patch]
public abstract class AtiumElectrumSteelMeleePatch : Verb_MeleeAttack {
    [Inject(At.Head, nameof(TryCastShot))]
    private void BeforeTryCastShot(ControlHandle<bool> ch) {
        if (AtiumElectrumSteel.TryResolveHit(this, out bool hit)) ch.ReturnValue = hit;
    }
}

[Patch]
public abstract class AtiumElectrumSteelShootPatch : Verb_Shoot {
    [Inject(At.Head, nameof(TryCastShot))]
    private void BeforeTryCastShot(ControlHandle<bool> ch) {
        if (AtiumElectrumSteel.TryResolveHit(this, out bool hit)) ch.ReturnValue = hit;
    }
}

[Patch]
public abstract class AtiumElectrumSteelCastAbilityPatch : Verb_CastAbility {
    [Inject(At.Head, nameof(TryCastShot))]
    private void BeforeTryCastShot(ControlHandle<bool> ch) {
        if (AtiumElectrumSteel.TryResolveHit(this, out bool hit)) ch.ReturnValue = hit;
    }
}
