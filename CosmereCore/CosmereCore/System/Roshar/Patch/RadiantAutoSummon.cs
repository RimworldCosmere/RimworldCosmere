using System.Collections.Generic;
using Cosmere.Core.Ability;
using Cosmere.System.Roshar.Gene;
using HarmonyLib;
using RimWorld;
using Verse;
using Logger = Cosmere.Core.Logger;
using Shardblade = Cosmere.System.Roshar.Surgebinding.Ability.Shardblade;
using Shardplate = Cosmere.System.Roshar.Surgebinding.Ability.Shardplate;

namespace Cosmere.System.Roshar.Patch;

[HarmonyPatch]
public static class RadiantAutoSummon {
    private static readonly HashSet<int> AutoSummonedPawns = [];
    private static readonly Dictionary<int, int> LastCombatTick = new();
    private const int DismissGraceTicks = 5000;

    private static AbilityDef? cachedBladeAbilityDef;
    private static AbilityDef? cachedPlateAbilityDef;
    private static bool defsResolved;

    private static void ResolveDefsIfNeeded() {
        if (defsResolved) return;
        cachedBladeAbilityDef = DefDatabase<AbilityDef>.GetNamedSilentFail("Cosmere_Roshar_Ability_ToggleShardblade");
        cachedPlateAbilityDef = DefDatabase<AbilityDef>.GetNamedSilentFail("Cosmere_Roshar_Ability_ToggleShardplate");
        defsResolved = cachedBladeAbilityDef != null || cachedPlateAbilityDef != null;
    }

    private static bool TryAutoSummon(Pawn pawn) {
        if (pawn.Drafted) return false;

        Surgebinder? surgebinder = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) return false;

        ResolveDefsIfNeeded();

        bool summoned = false;

        if (cachedBladeAbilityDef != null) {
            RimWorld.Ability? bladeAbility = pawn.abilities?.GetAbility(cachedBladeAbilityDef);
            if (bladeAbility is Shardblade blade && blade.status.active == Active.Off) {
                blade.UpdateStatus(Active.On);
                summoned = true;
                Logger.Verbose($"RadiantAutoSummon: {pawn.LabelShort} auto-summoned Shardblade");
            }
        }

        if (cachedPlateAbilityDef != null) {
            RimWorld.Ability? plateAbility = pawn.abilities?.GetAbility(cachedPlateAbilityDef);
            if (plateAbility is Shardplate plate && plate.status.active == Active.Off) {
                plate.UpdateStatus(Active.On);
                summoned = true;
                Logger.Verbose($"RadiantAutoSummon: {pawn.LabelShort} auto-summoned Shardplate");
            }
        }

        if (summoned) {
            AutoSummonedPawns.Add(pawn.thingIDNumber);
        }

        LastCombatTick[pawn.thingIDNumber] = GenTicks.TicksGame;
        return summoned;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(JobGiver_ConfigurableHostilityResponse), "TryGetAttackNearbyEnemyJob")]
    private static void AutoSummonOnAttackJob(Pawn pawn, Verse.AI.Job __result) {
        if (__result == null) return;
        if (pawn.playerSettings?.hostilityResponse != HostilityResponseMode.Attack) return;
        TryAutoSummon(pawn);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(JobGiver_ConfigurableHostilityResponse), "TryGiveJob")]
    private static void AutoDismissPostfix(Pawn pawn, Verse.AI.Job __result) {
        if (__result != null) return;
        if (pawn.Drafted) return;

        int pawnId = pawn.thingIDNumber;
        if (!AutoSummonedPawns.Contains(pawnId)) return;

        if (LastCombatTick.TryGetValue(pawnId, out int lastTick) &&
            GenTicks.TicksGame - lastTick < DismissGraceTicks) {
            return;
        }

        Surgebinder? surgebinder = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
        if (surgebinder == null) {
            AutoSummonedPawns.Remove(pawnId);
            return;
        }

        if (cachedBladeAbilityDef != null) {
            RimWorld.Ability? bladeAbility = pawn.abilities?.GetAbility(cachedBladeAbilityDef);
            if (bladeAbility is Shardblade blade && blade.status.active == Active.On) {
                blade.UpdateStatus(Active.Off);
                Logger.Verbose($"RadiantAutoSummon: {pawn.LabelShort} auto-dismissed Shardblade");
            }
        }

        if (cachedPlateAbilityDef != null) {
            RimWorld.Ability? plateAbility = pawn.abilities?.GetAbility(cachedPlateAbilityDef);
            if (plateAbility is Shardplate plate && plate.status.active == Active.On) {
                plate.UpdateStatus(Active.Off);
                Logger.Verbose($"RadiantAutoSummon: {pawn.LabelShort} auto-dismissed Shardplate");
            }
        }

        AutoSummonedPawns.Remove(pawnId);
        LastCombatTick.Remove(pawnId);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.PreApplyDamage))]
    private static void AutoSummonOnDamage(Pawn __instance, ref DamageInfo dinfo) {
        if (__instance.Drafted) return;
        if (dinfo.Instigator is not Pawn attacker) return;
        if (!attacker.HostileTo(__instance)) return;
        TryAutoSummon(__instance);
    }
}
