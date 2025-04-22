using System;
using CosmereScadrial.Abilities;
using UnityEngine;
using Verse;

namespace CosmereScadrial.FloatMenu {
    public class AllomanticAbilityFloatMenuOption : FloatMenuOption {
        private readonly AbstractAllomanticAbility ability;
        private readonly LocalTargetInfo target;
        private bool? lastCtrlState;

        public AllomanticAbilityFloatMenuOption(AbstractAllomanticAbility ability, LocalTargetInfo target) : base(
            null,
            null,
            ability.def.uiIcon,
            ability.metal.color,
            MenuOptionPriority.Default,
            null,
            null,
            0f,
            null,
            null,
            true,
            0,
            HorizontalJustification.Left,
            true
        ) {
            this.ability = ability;
            this.target = target;

            UpdateDisplay();
        }


        public override bool DoGUI(Rect rect, bool colonistOrdering, Verse.FloatMenu floatMenu) {
            var ctrl = Event.current.control;
            if (lastCtrlState != ctrl) {
                lastCtrlState = ctrl;

                UpdateDisplay();
            }

            return base.DoGUI(rect, colonistOrdering, floatMenu);
        }

        private void UpdateDisplay() {
            if (ability.status == BurningStatus.Flaring && target == ability.target) {
                action = () => ability.UpdateStatus(Event.current.control ? BurningStatus.Burning : BurningStatus.Off);
                tooltip = $"{ability.def.description}\n\n(Ctrl-click to de-flare)";
                Label = ability.GetRightClickLabel(target, Event.current.control ? BurningStatus.Burning : BurningStatus.Off);
                return;
            }

            if (ability.atLeastPassive && !Event.current.control && target == ability.target) {
                action = () => ability.UpdateStatus(BurningStatus.Off);
                tooltip = $"{ability.def.description}\n\n(Ctrl-click to flare)";
                Label = ability.GetRightClickLabel(target, BurningStatus.Off);
                return;
            }

            Action act = () => ability.Activate(target, target, Event.current.control);
            if (ability.atLeastPassive && target != ability.target) {
                act = () => {
                    ability.UpdateStatus(BurningStatus.Off);
                    ability.Activate(target, target, Event.current.control);
                };
            }

            var disabled = !ability.CanActivate(Event.current.control ? BurningStatus.Flaring : BurningStatus.Burning, out var reason);
            Label = ability.GetRightClickLabel(target, Event.current.control ? BurningStatus.Flaring : BurningStatus.Burning, reason);
            tooltip = $"{ability.def.description}\n\n(Ctrl-click to flare)";
            action = disabled ? null : act;
        }
    }
}