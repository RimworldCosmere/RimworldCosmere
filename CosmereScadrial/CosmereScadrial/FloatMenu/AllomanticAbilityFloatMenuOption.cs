using CosmereScadrial.Abilities;
using UnityEngine;
using Verse;

namespace CosmereScadrial.FloatMenu {
    public class AllomanticAbilityFloatMenuOption : FloatMenuOption {
        private readonly AllomanticAbility ability;
        private readonly LocalTargetInfo target;
        private bool? lastCtrlState;

        public AllomanticAbilityFloatMenuOption(AllomanticAbility ability, LocalTargetInfo target) : base(
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
            var disabled = !ability.CanActivate(Event.current.control ? BurningStatus.Flaring : BurningStatus.Burning, out var reason);
            Label = ability.GetRightClickLabel(target, Event.current.control ? BurningStatus.Flaring : BurningStatus.Burning, reason);
            action = disabled ? null : () => ability.Activate(target, target, Event.current.control);
        }


        public override bool DoGUI(Rect rect, bool colonistOrdering, Verse.FloatMenu floatMenu) {
            var ctrl = Event.current.control;
            if (lastCtrlState != ctrl) {
                lastCtrlState = ctrl;

                var disabled = !ability.CanActivate(Event.current.control ? BurningStatus.Flaring : BurningStatus.Burning, out var reason);
                Label = ability.GetRightClickLabel(target, Event.current.control ? BurningStatus.Flaring : BurningStatus.Burning, reason);
                tooltip = $"{ability.def.description}\n\n(Ctrl-click to flare)";
                action = disabled ? null : () => ability.Activate(target, target, Event.current.control);
            }

            return base.DoGUI(rect, colonistOrdering, floatMenu);
        }
    }
}