using System.Linq;
using CosmereScadrial.Abilities.Allomancy.Hediffs;
using CosmereScadrial.Defs;
using CosmereScadrial.Utils;
using RimWorld;
using Verse;

namespace CosmereScadrial.Comps.Things {
    public class TimeBubbleProperties : CompProperties_ThingContainer {
        public int applyEveryXTicks = 60;
        public float baseRadius = 4f;
        public float radiusPerSeverity = 1.5f;

        public TimeBubbleProperties() {
            compClass = typeof(TimeBubble);
        }
    }

    public class TimeBubble : ThingComp {
        public MetallicArtsMetalDef metal = null;
        public Pawn owner;
        private int ticksAlive;
        private new TimeBubbleProperties Props => (TimeBubbleProperties)props;

        private HediffDef hediffToApply => metal.defName switch {
            "Cadmium" => HediffDefOf.Cosmere_Hediff_TimeBubble_Cadmium,
            "Bendalloy" => HediffDefOf.Cosmere_Hediff_TimeBubble_Bendalloy,
            _ => null,
        };

        public override void CompTick() {
            base.CompTick();

            if (metal == null) return;

            ticksAlive++;

            if (owner == null || owner.Dead || owner.Map != parent.Map || !IsBurningMetal(owner)) {
                parent.Destroy();
                return;
            }

            var radius = Props.baseRadius + GetSeverity(owner) * Props.radiusPerSeverity;
            if (!owner.Position.InHorDistOf(parent.Position, radius)) {
                parent.Destroy();
                return;
            }

            if (ticksAlive % Props.applyEveryXTicks != 0) return;

            foreach (var pawn in GenRadial.RadialDistinctThingsAround(parent.Position, parent.Map, radius, true)
                         .OfType<Pawn>()) {
                if (pawn.health.hediffSet.HasHediff(hediffToApply)) continue;

                var hediff = HediffMaker.MakeHediff(hediffToApply, pawn);
                hediff.Severity = 1.0f;
                pawn.health.AddHediff(hediff);
            }
        }

        private bool IsBurningMetal(Pawn pawn) {
            return AllomancyUtility.IsBurning(pawn, metal);
        }

        private float GetSeverity(Pawn pawn) {
            var hediff = pawn.health?.hediffSet?.hediffs.FirstOrDefault(x => {
                return x is AllomanticHediff hediff &&
                       hediff.sourceAbilities.Any(ability => ability.metal.Equals(metal));
            });

            return hediff is not AllomanticHediff allomanticHediff ? 0f : allomanticHediff.CalculateSeverity();
        }
    }
}