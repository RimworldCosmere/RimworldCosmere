using CosmereScadrial.Abilities.Allomancy;
using UnityEngine;
using Verse;

namespace CosmereScadrial.FloatMenu {
    public class AllomanticAbilityFloatMenuOption : FloatMenuOption {
        private readonly AbstractAbility ability;
        private readonly LocalTargetInfo target;
        private bool? lastCtrlState;

        public AllomanticAbilityFloatMenuOption(AbstractAbility ability, LocalTargetInfo target) : base(
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
            // If we are already flaring the target of the given ability
            // Update the action to deflare on control, or turn off on click.
            if (ability.status == BurningStatus.Flaring && target == ability.target) {
                action = () => ability.UpdateStatus(Event.current.control ? BurningStatus.Burning : BurningStatus.Off);
                tooltip = $"{ability.def.description}\n\n(Ctrl-click to de-flare)";
                Label = ability.GetRightClickLabel(target, Event.current.control ? BurningStatus.Burning : BurningStatus.Off);
                return;
            }

            // If we are already burning/flaring/passing on the target of the given ability
            // And we arent holding control
            // Update the action to turn off the ability
            if (ability.atLeastPassive && !Event.current.control && target == ability.target) {
                action = () => ability.UpdateStatus(BurningStatus.Off);
                tooltip = $"{ability.def.description}\n\n(Ctrl-click to flare)";
                Label = ability.GetRightClickLabel(target, BurningStatus.Off);
                return;
            }

            // By default, we are turning on the ability, on the target at the cell, flaring if control is held
            var disabled = !ability.CanActivate(Event.current.control ? BurningStatus.Flaring : BurningStatus.Burning, out var reason);
            action = disabled ? null : () => ability.QueueCastingJob(target, target.Cell, Event.current.control);

            // If we are already using this ability on one target, that isnt this one, turn off the ability, and activate it on the new one
            // This may not be necessary
            /*
            if (ability.atLeastPassive && target != ability.target) {
                action = disabled ? null : () => {
                    ability.UpdateStatus(BurningStatus.Off);
                    ability.Activate(target, target, Event.current.control);
                };
            }*/

            Label = ability.GetRightClickLabel(target, Event.current.control ? BurningStatus.Flaring : BurningStatus.Burning, reason);
            tooltip = $"{ability.def.description}\n\n(Ctrl-click to flare)";
        }
    }
}