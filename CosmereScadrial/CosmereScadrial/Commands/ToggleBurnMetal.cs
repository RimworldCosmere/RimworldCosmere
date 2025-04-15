using CosmereScadrial.Comps;
using RimWorld;
using UnityEngine;
using Verse;
using PawnUtility = CosmereFramework.Utils.PawnUtility;

namespace CosmereScadrial.Commands {
    public class ToggleBurnMetal(Ability ability, Pawn pawn) : Command_Ability(ability, pawn) {
        public override string TopRightLabel => ability.CompOfType<CompToggleBurnMetal>().Status.ToString();

        public override string DescPostfix => "\n\n(Ctrl-click to flare)";

        public override void ProcessInput(Event ev) {
            var ctrlHeld = ev.control;
            var comp = ability.CompOfType<CompToggleBurnMetal>();
            if (!ctrlHeld) {
                base.ProcessInput(ev);
                return;
            }

            // If they are asleep, don't let them flare, but do still turn on burning, if they arent already
            if (PawnUtility.IsAsleep(Pawn)) {
                if (!comp.Burning) {
                    base.ProcessInput(ev);
                }

                return;
            }

            comp.ToggleFlaring();
            if (!comp.Burning) {
                base.ProcessInput(ev);
            }
        }
    }
}