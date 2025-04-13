using System.Linq;
using CosmereScadrial.Comps;
using RimWorld;
using UnityEngine;
using Pawn = Verse.Pawn;

namespace CosmereScadrial.Commands {
    public class ToggleBurnMetal(Ability ability, Pawn pawn) : Command_Ability(ability, pawn) {
        public override Color IconDrawColor {
            get {
                var comp = ability.comps?.FirstOrDefault(c => c is CompToggleBurnMetal) as CompToggleBurnMetal;
                return comp?.burning == true ? Color.red : Color.white;
            }
        }

        public override string TopRightLabel {
            get {
                var comp = ability.comps?.FirstOrDefault(c => c is CompToggleBurnMetal) as CompToggleBurnMetal;

                return comp?.burning == true ? "Burning" : "";
            }
        }

        public override void ProcessInput(Event ev) {
            base.ProcessInput(ev);

            // Force valid self-targeting
            ability.QueueCastingJob(ability.pawn); // <== this prevents crash
        }
    }
}