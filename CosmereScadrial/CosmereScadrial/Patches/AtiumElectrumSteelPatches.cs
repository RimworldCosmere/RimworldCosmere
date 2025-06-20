using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Log = CosmereFramework.Log;

namespace CosmereScadrial.Patches;

[HarmonyPatch]
public static class AtiumElectrumSteelPatches {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Verb_MeleeAttack), "TryCastShot")]
    public static bool PrefixVerbShoot(Verb_MeleeAttack instance, ref bool result) {
        return Patch(instance, ref result);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Verb_Shoot), "TryCastShot")]
    public static bool PrefixVerbShoot(Verb_Shoot instance, ref bool result) {
        return Patch(instance, ref result);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Verb_CastAbility), "TryCastShot")]
    public static bool PrefixVerbCastAbility(Verb_CastAbility instance, ref bool result) {
        return Patch(instance, ref result);
    }

    private static bool Patch(Verb verb, ref bool result) {
        if (!verb.CurrentTarget.HasThing || verb.CurrentTarget.Thing is not Pawn targetPawn) return true;
        if (verb.caster is not Pawn casterPawn) return true;
        if (casterPawn.Equals(targetPawn)) return true;

        return ShouldHit(verb, casterPawn, targetPawn);
    }

    private static bool ShouldHit(Verb verb, Pawn casterPawn, Pawn targetPawn) {
        float baseHitChance = 1f;

        float casterAtiumSeverity = casterPawn.health?.hediffSet
            ?.GetFirstHediffOfDef(HediffDefOf.Cosmere_Hediff_AtiumBuff)?.Severity ?? 0f;
        float targetAtiumSeverity =
            targetPawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.Cosmere_Hediff_AtiumBuff)?.Severity ?? 0f;
        float targetSteelBubbleSeverity =
            targetPawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.Cosmere_Hediff_SteelBubble)?.Severity ?? 0f;
        float targetElectrumSeverity =
            targetPawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.Cosmere_Hediff_ElectrumBuff)?.Severity ?? 0f;

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
        Log.Verbose(
            $"{casterPawn.NameFullColored} chance to hit {targetPawn.NameFullColored} = {baseHitChance:P} casterAtiumSeverity={casterAtiumSeverity} targetAtiumSeverity={targetAtiumSeverity} targetSteelBubbleSeverity={targetSteelBubbleSeverity} targetElectrumSeverity={targetElectrumSeverity}");
        return Rand.Chance(baseHitChance);
    }
}