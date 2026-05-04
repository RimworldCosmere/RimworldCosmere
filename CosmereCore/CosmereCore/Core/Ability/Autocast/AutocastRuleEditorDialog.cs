using System;
using UnityEngine;
using Verse;

namespace Cosmere.Core.Ability.Autocast;

public sealed class AutocastRuleEditorDialog : Verse.Window {
    private readonly string abilityLabel;
    private readonly AutocastRule rule;
    private Vector2 scroll;

    public AutocastRuleEditorDialog(AutocastRule rule, string abilityLabel) {
        this.rule = rule;
        this.abilityLabel = abilityLabel;
        doCloseX = true;
        forcePause = true;
        absorbInputAroundWindow = true;
        closeOnClickedOutside = true;
        draggable = false;
    }

    public override Vector2 InitialSize => new Vector2(520f, 460f);

    public override void DoWindowContents(Rect inRect) {
        float y = inRect.y;

        using (new TextBlock(GameFont.Medium, TextAnchor.MiddleLeft, Color.white))
            Widgets.Label(
                new Rect(inRect.x, y, inRect.width, 30f),
                "CC_Autocast_Editor_Title".Translate(abilityLabel.Named("ABILITY"))
            );
        y += 34f;

        Rect enabledRect = new Rect(inRect.x, y, 160f, 24f);
        Widgets.CheckboxLabeled(
            enabledRect,
            "CC_Autocast_Editor_Enabled".Translate(),
            ref rule.Enabled,
            placeCheckboxNearText: true
        );
        y += 28f;

        Rect capLabel = new Rect(inRect.x, y, 160f, 24f);
        Widgets.Label(
            capLabel,
            "CC_Autocast_Editor_MaxReserveSpend".Translate(((int)(rule.CostCapFraction * 100f)).Named("PERCENT"))
        );
        Rect capSlider = new Rect(inRect.x + 170f, y + 4f, inRect.width - 180f, 18f);
        rule.CostCapFraction = Widgets.HorizontalSlider(capSlider, rule.CostCapFraction, 0f, 1f);
        y += 28f;

        Rect triggerHeader = new Rect(inRect.x, y, inRect.width, 24f);
        using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, new Color(0.85f, 0.85f, 0.85f)))
            Widgets.Label(triggerHeader, "CC_Autocast_Editor_TriggersHeader".Translate());
        y += 26f;

        Rect listRect = new Rect(inRect.x, y, inRect.width, inRect.height - (y - inRect.y) - 40f);
        Rect viewRect = new Rect(0f, 0f, listRect.width - 16f, rule.Triggers.Count * 44f + 8f);
        Widgets.BeginScrollView(listRect, ref scroll, viewRect);

        float rowY = 0f;
        int toRemove = -1;
        for (int i = 0; i < rule.Triggers.Count; i++) {
            Rect row = new Rect(0f, rowY, viewRect.width, 40f);
            if (DrawTriggerRow(row, rule.Triggers[i])) toRemove = i;
            rowY += 44f;
        }

        if (toRemove >= 0) rule.Triggers.RemoveAt(toRemove);

        Widgets.EndScrollView();

        Rect addButton = new Rect(inRect.x, inRect.yMax - 32f, 160f, 28f);
        if (Widgets.ButtonText(addButton, "CC_Autocast_Editor_AddTrigger".Translate())) {
            rule.Triggers.Add(
                new AutocastTrigger(AutocastTriggerKind.ReservePercent, AutocastComparison.LessThan, 0.5f)
            );
        }

        Rect closeButton = new Rect(inRect.xMax - 120f, inRect.yMax - 32f, 120f, 28f);
        if (Widgets.ButtonText(closeButton, "CC_Autocast_Editor_Close".Translate())) Close();
    }

    private static bool DrawTriggerRow(Rect row, AutocastTrigger trigger) {
        Rect kindRect = new Rect(row.x, row.y + 8f, 150f, 24f);
        if (Widgets.ButtonText(kindRect, TriggerKindLabel(trigger.Kind))) {
            List<FloatMenuOption> opts = [];
            foreach (AutocastTriggerKind kind in Enum.GetValues(typeof(AutocastTriggerKind))) {
                AutocastTriggerKind captured = kind;
                opts.Add(new FloatMenuOption(TriggerKindLabel(kind), () => trigger.Kind = captured));
            }

            Find.WindowStack.Add(new FloatMenu(opts));
        }

        if (trigger.Kind == AutocastTriggerKind.Drafted) {
            Rect draftedLabel = new Rect(row.x + 160f, row.y + 8f, row.width - 200f, 24f);
            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, Color.white))
                Widgets.Label(draftedLabel, "CC_Autocast_Editor_FiresWhileDrafted".Translate());
        }
        else {
            Rect cmpRect = new Rect(row.x + 160f, row.y + 8f, 80f, 24f);
            if (Widgets.ButtonText(cmpRect, ComparisonLabel(trigger.Comparison))) {
                List<FloatMenuOption> opts = [];
                foreach (AutocastComparison cmp in Enum.GetValues(typeof(AutocastComparison))) {
                    AutocastComparison captured = cmp;
                    opts.Add(new FloatMenuOption(ComparisonLabel(cmp), () => trigger.Comparison = captured));
                }

                Find.WindowStack.Add(new FloatMenu(opts));
            }

            Rect sliderRect = new Rect(row.x + 250f, row.y + 12f, row.width - 310f, 18f);
            bool isPercent = trigger.Kind == AutocastTriggerKind.HealthPercent ||
                             trigger.Kind == AutocastTriggerKind.ReservePercent;
            float max = isPercent ? 1f : 30f;
            trigger.Threshold = Widgets.HorizontalSlider(sliderRect, trigger.Threshold, 0f, max);

            Rect valueLabel = new Rect(sliderRect.xMax + 4f, row.y + 8f, 46f, 24f);
            using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, Color.white)) {
                Widgets.Label(
                    valueLabel,
                    isPercent
                        ? "CC_Autocast_Editor_PercentLabel".Translate(
                            ((int)(trigger.Threshold * 100f)).Named("PERCENT")
                        )
                        : "CC_Autocast_Editor_CellsLabel".Translate(((int)trigger.Threshold).Named("COUNT"))
                );
            }
        }

        Rect remove = new Rect(row.xMax - 24f, row.y + 10f, 20f, 20f);
        return Widgets.ButtonImage(remove, TexButton.Delete);
    }

    private static string TriggerKindLabel(AutocastTriggerKind kind) {
        return kind switch {
            AutocastTriggerKind.HealthPercent => "CC_Autocast_Trigger_HealthPercent".Translate(),
            AutocastTriggerKind.ReservePercent => "CC_Autocast_Trigger_ReservePercent".Translate(),
            AutocastTriggerKind.Drafted => "CC_Autocast_Trigger_Drafted".Translate(),
            _ => kind.ToString(),
        };
    }

    private static string ComparisonLabel(AutocastComparison cmp) {
        return cmp switch {
            AutocastComparison.LessThan => "<",
            AutocastComparison.GreaterThan => ">",
            AutocastComparison.EqualTo => "=",
            _ => "?",
        };
    }
}