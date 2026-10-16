using System.Text;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Ability.Autocast;

/// <summary>
///     One line describing what a rule does, so a pawn with several rules against the same
///     metal can be told apart at a glance without opening each one.
/// </summary>
public static class AutocastRuleSummary {
    public static string Describe(AutocastRule rule) {
        // a rule with no triggers never runs, so it says so instead of claiming a condition it lacks.
        bool idle = rule.Triggers.Count == 0;

        if (rule.Kind != AutocastRuleKind.FeruchemyDial) {
            return idle
                ? "CC_Autocast_Summary_NoTriggers".Translate().Resolve()
                : "CC_Autocast_Summary_Cast".Translate(Conditions(rule).Named("CONDITIONS")).Resolve();
        }

        int percent = Percent(AutocastDialRange.Intensity(rule.Kind, rule.ActiveTarget));
        string action = AutocastDialRange.IsTapping(rule.Kind, rule.ActiveTarget)
            ? "CC_Autocast_Summary_Tap".Translate(percent.Named("PERCENT")).Resolve()
            : "CC_Autocast_Summary_Store".Translate(percent.Named("PERCENT")).Resolve();

        return idle
            ? "CC_Autocast_Summary_DialIdle".Translate(action.Named("ACTION")).Resolve()
            : "CC_Autocast_Summary_Dial".Translate(
                action.Named("ACTION"),
                Conditions(rule).Named("CONDITIONS")
            ).Resolve();
    }

    private static string Conditions(AutocastRule rule) {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < rule.Triggers.Count; i++) {
            if (i > 0) sb.Append("CC_Autocast_Summary_And".Translate().Resolve());
            sb.Append(Describe(rule.Triggers[i]));
        }

        return sb.ToString();
    }

    private static string Describe(AutocastTrigger trigger) {
        switch (trigger.Kind) {
            case AutocastTriggerKind.Drafted:
                return "CC_Autocast_Summary_Drafted".Translate().Resolve();

            case AutocastTriggerKind.EnemyProximity:
            case AutocastTriggerKind.AllyProximity:
                string who = trigger.Kind == AutocastTriggerKind.EnemyProximity
                    ? "CC_Autocast_Summary_Enemy".Translate().Resolve()
                    : "CC_Autocast_Summary_Ally".Translate().Resolve();

                return "CC_Autocast_Summary_Proximity".Translate(
                    who.Named("WHO"),
                    Comparison(trigger.Comparison).Named("OP"),
                    ((int)trigger.Threshold).Named("COUNT")
                ).Resolve();

            default:
                string what = trigger.Kind == AutocastTriggerKind.HealthPercent
                    ? "CC_Autocast_Summary_Health".Translate().Resolve()
                    : "CC_Autocast_Summary_Reserve".Translate().Resolve();

                return "CC_Autocast_Summary_Percent".Translate(
                    what.Named("WHAT"),
                    Comparison(trigger.Comparison).Named("OP"),
                    Percent(trigger.Threshold).Named("PERCENT")
                ).Resolve();
        }
    }

    private static string Comparison(AutocastComparison comparison) {
        return comparison switch {
            AutocastComparison.LessThan => "<",
            AutocastComparison.GreaterThan => ">",
            _ => "=",
        };
    }

    private static int Percent(float fraction) {
        return Mathf.RoundToInt(fraction * 100f);
    }
}
