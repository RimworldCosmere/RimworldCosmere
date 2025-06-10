using System;
using System.Collections.Generic;
using System.Linq;
using CosmereCore.Utils;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Abilities.Allomancy.Hediffs.Comps {
    public class BronzeAuraProperties : HediffCompProperties {
        public Color lineColor = new Color(0.6f, 0.3f, 0.3f, 1f);
        public float radius = 15f;

        public BronzeAuraProperties() {
            compClass = typeof(BronzeAura);
        }

        public Material lineMaterial =>
            MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.SolidColorBehind, lineColor);
    }

    public class BronzeAura : HediffComp {
        public Dictionary<Thing, float> thingsToDraw { get; } = new Dictionary<Thing, float>();

        public new BronzeAuraProperties props => (BronzeAuraProperties)base.props;

        private new AllomanticHediff parent => base.parent as AllomanticHediff;

        private bool isAtLeastPassive => parent.Severity >= 0.5f;

        private void GetThingsToDraw() {
            var map = parent.pawn.Map;
            var center = parent.pawn.Position;
            var radius = props.radius * parent.Severity;
            if (map == null || !center.IsValid || !Find.Selector.IsSelected(parent.pawn)) {
                return;
            }

            thingsToDraw.Clear();
            foreach (var cell in GenRadial.RadialCellsAround(center,
                         (float)Math.Round(Math.Min(GenRadial.MaxRadialPatternRadius, radius)), false)) {
                foreach (var thing in cell.GetThingList(map)
                             .Where(thing =>
                                 InvestitureDetector.HasInvestiture(thing) && !thingsToDraw.ContainsKey(thing))) {
                    thingsToDraw[thing] = InvestitureDetector.GetInvestiture(thing);
                }
            }
        }

        public override void CompPostTick(ref float severityAdjustment) {
            if (!isAtLeastPassive) {
                thingsToDraw.Clear();
                return;
            }

            if (!base.parent.pawn.IsHashIntervalTick(GenTicks.TicksPerRealSecond)) return;

            GetThingsToDraw();
        }
    }
}