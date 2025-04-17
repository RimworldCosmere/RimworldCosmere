using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Defs;
using RimWorld;
using UnityEngine;
using Verse;
using PawnUtility = CosmereFramework.Utils.PawnUtility;

namespace CosmereScadrial.Abilities.Gizmos {
    public class BurnGizmo : Command_Ability {
        private static readonly Dictionary<Color, Texture2D> cachedTextures = new Dictionary<Color, Texture2D>();

        private new readonly AllomanticAbility ability;

        public BurnGizmo(AllomanticAbility ability) : base(ability, ability.pawn) {
            this.ability = ability;
        }

        public override string TopRightLabel => ability.status.ToString();

        public override string DescPostfix => "\n\n(Ctrl-click to flare)";

        private MetallicArtsMetalDef metal => ability.metal;

        public override Texture2D BGTexture {
            get {
                var fill = metal.color;

                if (cachedTextures.TryGetValue(fill, out var tex)) return tex;

                tex = new Texture2D(128, 128, TextureFormat.ARGB32, false);
                var colors = Enumerable.Repeat(fill, tex.width * tex.height).ToArray();

                tex.SetPixels(colors);
                tex.Apply();

                cachedTextures[fill] = tex; // Cache the texture for future use.

                return tex;
            }
        }

        private bool IsActive() {
            return ability.atLeastPassive;
        }

        /*
         @Todo Replace TopRightLabel with this, and have a cool icon for Off/Passive/Burning/Flaring
        public override GizmoResult GizmoOnGUI(Vector2 loc, float maxWidth, GizmoRenderParms parms) {
            var gizmoResult = base.GizmoOnGUI(loc, maxWidth, parms);
            if (disabled) {
                return gizmoResult;
            }

            var rect = new Rect(loc.x, loc.y, GetWidth(maxWidth), 75f);
            GUI.DrawTexture(new Rect((float)(rect.x + (double)rect.width - 24.0), rect.y, 24f, 24f), IsActive() ? Widgets.CheckboxOnTex : (Texture)Widgets.CheckboxOffTex);
            return gizmoResult;
        }*/

        public override void ProcessInput(Event ev) {
            var ctrlHeld = ev.control;
            if (!ctrlHeld) {
                base.ProcessInput(ev);
                return;
            }

            // If they are asleep, don't let them flare
            if (PawnUtility.IsAsleep(Pawn)) {
                Messages.Message($"{Pawn.NameFullColored} is not allowed to flare while they are asleep.", Pawn, MessageTypeDefOf.NegativeEvent);
                return;
            }

            // If they are not asleep, and ctrl is held, and they are not Burning OR Flaring, turn on burning
            if (!ability.atLeastPassive) {
                base.ProcessInput(ev);
            }

            // If they are not asleep, and ctrl is held, and they are already burning or flaring, Toggle flaring.
            ability.UpdateStatus(BurningStatus.Flaring);
        }
    }
}