using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Ability.Autocast;

public sealed class AutocastRunner : GameComponent {
    private const int TickInterval = 60;

    public AutocastRunner(Game game) { }

    public override void GameComponentTick() {
        if (Find.TickManager.TicksGame % TickInterval != 0) return;

        GameComponent_Autocast store = GameComponent_Autocast.Get();
        List<Map> maps = Find.Maps;
        for (int m = 0; m < maps.Count; m++) {
            List<Pawn> pawns = maps[m].mapPawns.FreeColonistsSpawned;
            for (int p = 0; p < pawns.Count; p++) {
                TickPawn(pawns[p], store);
            }
        }
    }

    private static void TickPawn(Pawn pawn, GameComponent_Autocast store) {
        if (pawn.abilities == null) return;
        SeedDefaults(pawn, store);
        List<AutocastRule> rules = store.RulesFor(pawn);
        if (rules.Count == 0) return;

        for (int r = 0; r < rules.Count; r++) {
            AutocastRule rule = rules[r];
            if (!rule.Enabled || rule.Triggers.Count == 0) continue;

            RimWorld.Ability? ability = FindAbility(pawn, rule.AbilityDefName);
            if (ability == null || !ability.CanCast) continue;
            if (ability.def.targetRequired) continue;

            if (!AllTriggersPass(pawn, rule)) continue;

            ability.QueueCastingJob(pawn, LocalTargetInfo.Invalid);
            rule.FireCount++;
        }
    }

    private static void SeedDefaults(Pawn pawn, GameComponent_Autocast store) {
        List<RimWorld.Ability> abilities = pawn.abilities.AllAbilitiesForReading;
        for (int i = 0; i < abilities.Count; i++) {
            string defName = abilities[i].def.defName;
            if (!AutocastDefaults.HasDefaults(defName)) continue;
            store.GetOrCreateRule(pawn, defName);
        }
    }

    private static RimWorld.Ability? FindAbility(Pawn pawn, string defName) {
        List<RimWorld.Ability> list = pawn.abilities.AllAbilitiesForReading;
        for (int i = 0; i < list.Count; i++) {
            if (list[i].def.defName == defName) return list[i];
        }
        return null;
    }

    private static bool AllTriggersPass(Pawn pawn, AutocastRule rule) {
        for (int i = 0; i < rule.Triggers.Count; i++) {
            if (!Evaluate(pawn, rule.Triggers[i])) return false;
        }
        return true;
    }

    private static bool Evaluate(Pawn pawn, AutocastTrigger trigger) {
        switch (trigger.Kind) {
            case AutocastTriggerKind.HealthPercent:
                return Compare(pawn.health.summaryHealth.SummaryHealthPercent, trigger);
            case AutocastTriggerKind.ReservePercent:
                return Compare(ResolvePrimaryReserve(pawn), trigger);
            case AutocastTriggerKind.Drafted:
                return pawn.Drafted;
            case AutocastTriggerKind.EnemyWithinCells:
            case AutocastTriggerKind.AllyWithinCells:
                return false;
            default:
                return false;
        }
    }

    private static float ResolvePrimaryReserve(Pawn pawn) {
        Cosmere.Core.UI.Model.InvestitureSnapshot? snap = null;
        global::System.Collections.Generic.IReadOnlyList<Cosmere.Core.UI.Model.IInvestitureProvider> all =
            Cosmere.Core.UI.Model.PawnInvestitureProviders.All;
        for (int i = 0; i < all.Count; i++) {
            if (!all[i].IsInvested(pawn)) continue;
            snap = all[i].Snapshot(pawn);
            if (snap?.PrimaryBar != null) break;
        }
        if (snap?.PrimaryBar == null) return 0f;
        if (snap.PrimaryBar.Max <= 0f) return 0f;
        return snap.PrimaryBar.Current / snap.PrimaryBar.Max;
    }

    private static bool Compare(float value, AutocastTrigger trigger) {
        return trigger.Comparison switch {
            AutocastComparison.LessThan => value < trigger.Threshold,
            AutocastComparison.GreaterThan => value > trigger.Threshold,
            AutocastComparison.EqualTo => Mathf.Abs(value - trigger.Threshold) < 0.001f,
            _ => false,
        };
    }
}
