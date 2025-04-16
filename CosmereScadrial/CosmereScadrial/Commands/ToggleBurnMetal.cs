using CosmereScadrial.Comps;
using RimWorld;
using UnityEngine;
using Verse;
using PawnUtility = CosmereFramework.Utils.PawnUtility;
using CompToggleBurnMetal = CosmereScadrial.Comps.ToggleBurnMetal;

namespace CosmereScadrial.Commands {
    public class ToggleBurnMetal(Ability ability, Pawn pawn) : Command_Ability(ability, pawn) {
        private CompToggleBurnMetal Comp => ability.CompOfType<CompToggleBurnMetal>();

        public override string TopRightLabel => Comp.Status.ToString();

        public override string DescPostfix => "\n\n(Ctrl-click to flare)";

        /*
        public override Texture2D BGTexture {
            get {
                var tex = new Texture2D(128, 128, TextureFormat.ARGB32, false);
                tex.SetPixels([BGTextureColor]);

                return tex;
            }
        }

        private Color BGTextureColor => Comp.Status switch {
            ToggleBurnMetalStatus.Off => Color.red,
            ToggleBurnMetalStatus.Passive => Color.yellow,
            ToggleBurnMetalStatus.Burning => Color.green,
            ToggleBurnMetalStatus.Flaring => Color.cyan,
            _ => Color.red,
        };*/

        public override void ProcessInput(Event ev) {
            var ctrlHeld = ev.control;
            if (!ctrlHeld) {
                base.ProcessInput(ev);
                return;
            }

            // If they are asleep, don't let them flare, but do still turn on burning, if they arent already
            if (PawnUtility.IsAsleep(Pawn)) {
                if (Comp.Status == ToggleBurnMetalStatus.Off) {
                    base.ProcessInput(ev);
                }

                Messages.Message($"{Pawn.NameFullColored} is not allowed to flare while they are asleep.", Pawn, MessageTypeDefOf.NegativeEvent);
                return;
            }

            // If they are not asleep, and ctrl is held, and they are not Burning OR Flaring, turn on burning
            if (Comp.Status < ToggleBurnMetalStatus.Passive) {
                base.ProcessInput(ev);
            }

            // If they are not asleep, and ctrl is held, and they are already burning or flaring, Toggle flaring.
            Comp.ToggleFlaring();
        }
    }
}