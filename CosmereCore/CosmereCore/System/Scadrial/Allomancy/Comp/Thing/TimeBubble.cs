using Cosmere.Core.Ability;
using Cosmere.Core.Hediff;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Allomancy.Hediff;
using Cosmere.System.Scadrial.Def;
using Cosmere.System.Scadrial.Gene;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Allomancy.Comp.Thing;

public class TimeBubbleProperties : CompProperties_ThingContainer {
    public int applyEveryXTicks = 60;
    public float baseRadius = 4f;
    public float radiusPerSeverity = 1.5f;

    public TimeBubbleProperties() {
        compClass = typeof(TimeBubble);
    }
}

public class TimeBubble : ThingComp {
    public MetallicArtsMetalDef? metal;
    private int ticksAlive;

    public Pawn? owner { get; set; }

    private new TimeBubbleProperties props => (TimeBubbleProperties)base.props;

    private HediffDef? hediffToApply => metal == null
        ? null
        : metal == MetallicArtsMetalDefOf.Cadmium
            ? HediffDefOf.Cosmere_Scadrial_Hediff_TimeBubbleCadmium
            : HediffDefOf.Cosmere_Scadrial_Hediff_TimeBubbleBendalloy;

    public override void CompTick() {
        base.CompTick();

        if (metal == null) return;

        ticksAlive++;

        if (owner == null || owner.Dead || owner.Map != parent.Map || !owner.IsBurning(metal)) {
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

            Verse.Hediff? hediff = HediffMaker.MakeHediff(hediffToApply, pawn);
            hediff.Severity = 1.0f;
            pawn.health.AddHediff(hediff);
        }
    }

    private float GetSeverity(Pawn pawn) {
        List<Verse.Hediff>? hediffs = pawn.health?.hediffSet?.hediffs;
        if (hediffs == null) return 0f;

        for (int i = 0; i < hediffs.Count; i++) {
            if (hediffs[i] is not AllomanticHediff allomanticHediff) continue;
            foreach (IAbility<Allomancer, IHediff<Allomancer>> sa in allomanticHediff.SourceAbilities) {
                if (sa is AllomancyAbility ability && ability.metal.Equals(metal)) {
                    return allomanticHediff.severityCalculator?.severity ?? 0f;
                }
            }
        }

        return 0f;
    }
}
