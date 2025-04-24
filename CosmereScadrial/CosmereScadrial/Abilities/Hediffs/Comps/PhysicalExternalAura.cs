using System;
using System.Collections.Generic;
using System.Linq;
using CosmereScadrial.Utils;
using UnityEngine;
using Verse;

namespace CosmereScadrial.Abilities.Hediffs.Comps {
    public class PhysicalExternalAuraProperties : HediffCompProperties {
        public Color lineColor = new Color(0.3f, 0.6f, 1f, 1f);
        public float radius = 15f;

        public PhysicalExternalAuraProperties() {
            compClass = typeof(PhysicalExternalAura);
        }

        public Material lineMaterial => MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.SolidColorBehind, lineColor);
    }

    public class PhysicalExternalAura : HediffComp {
        private static readonly HashSet<Thing> metalCache = new HashSet<Thing>();

        public Dictionary<Thing, float> thingsToDraw { get; } = new Dictionary<Thing, float>();

        public new PhysicalExternalAuraProperties props => (PhysicalExternalAuraProperties)base.props;

        private new AllomanticHediff parent => base.parent as AllomanticHediff;

        private bool isAtLeastPassive => parent.Severity >= 0.5f;

        private void GetThingsToDraw() {
            var map = parent.pawn.Map;
            var center = parent.pawn.Position;
            var radius = props.radius * parent.Severity;
            if (map == null || !center.IsValid) {
                return;
            }

            thingsToDraw.Clear();
            foreach (var cell in GenRadial.RadialCellsAround(center, Math.Min(GenRadial.MaxRadialPatternRadius, radius), false)) {
                foreach (var thing in cell.GetThingList(map).Where(thing => MetalDetector.IsCapableOfHavingMetal(thing.def) && !thingsToDraw.ContainsKey(thing))) {
                    thingsToDraw[thing] = MetalDetector.GetMetal(thing);
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