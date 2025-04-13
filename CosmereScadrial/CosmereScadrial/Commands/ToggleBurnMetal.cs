using CosmereScadrial.Comps;
using RimWorld;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Commands {
    public class ToggleBurnMetal(Ability ability, Pawn pawn) : Command_Ability(ability, pawn) {
        public override Color IconDrawColor {
            get {
                var comp = ability.CompOfType<CompToggleBurnMetal>();
                if (comp == null || !comp.burning) return Color.white;
                if (comp.flaring) return Color.red;
                return Color.green;
            }
        }

        public override string TopRightLabel {
            get {
                var comp = ability.CompOfType<CompToggleBurnMetal>();
                if (comp == null || !comp.burning) return "Off";
                if (comp.flaring) return "Flaring";
                return "Burning";
            }
        }

        public override string DescPostfix {
            get => "\n(Ctrl-click to flare)";
        }

        public override void ProcessInput(Event ev) {
            var ctrlHeld = ev.control;
            var comp = ability.CompOfType<CompToggleBurnMetal>();
            if (comp == null || !ctrlHeld) {
                base.ProcessInput(ev);
                return;
            }

            comp.ToggleFlaring();
            Log.Message($"[Cosmere] Toggled flaring: {(comp.flaring ? "Flare" : "Normal")}");
        }
    }
}