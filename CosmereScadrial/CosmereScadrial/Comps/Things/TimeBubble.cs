using System.Linq;
using CosmereScadrial.Abilities.Allomancy.Hediffs;
using CosmereScadrial.Defs;
using CosmereScadrial.Utils;
using RimWorld;
using Verse;

namespace CosmereScadrial.Comps.Things;

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
    private int ticksAlive;
    public Pawn owner { get; set; }
    private TimeBubbleProperties props => (TimeBubbleProperties)base.props;

    private HediffDef? hediffToApply => metal.defName switch {
        "Cadmium" => HediffDefOf.Cosmere_Hediff_Time_Bubble_Cadmium,
        "Bendalloy" => HediffDefOf.Cosmere_Hediff_Time_Bubble_Bendalloy,
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

        float radius = props.baseRadius + GetSeverity(owner) * props.radiusPerSeverity;
        if (!owner.Position.InHorDistOf(parent.Position, radius)) {
            parent.Destroy();
            return;
        }

        if (ticksAlive % props.applyEveryXTicks != 0) return;

        foreach (Pawn? pawn in GenRadial.RadialDistinctThingsAround(parent.Position, parent.Map, radius, true)
                     .OfType<Pawn>()) {
            if (pawn.health.hediffSet.HasHediff(hediffToApply)) continue;

            Hediff? hediff = HediffMaker.MakeHediff(hediffToApply, pawn);
            hediff.Severity = 1.0f;
            pawn.health.AddHediff(hediff);
        }
    }

    private bool IsBurningMetal(Pawn pawn) {
        return AllomancyUtility.IsBurning(pawn, metal);
    }

    private float GetSeverity(Pawn pawn) {
        Hediff? hediff = pawn.health?.hediffSet?.hediffs.FirstOrDefault(x => {
            return x is AllomanticHediff hediff &&
                   hediff.sourceAbilities.Any(ability => ability.metal.Equals(metal));
        });

        return hediff is not AllomanticHediff allomanticHediff ? 0f : allomanticHediff.severityCalculator.severity;
    }
}