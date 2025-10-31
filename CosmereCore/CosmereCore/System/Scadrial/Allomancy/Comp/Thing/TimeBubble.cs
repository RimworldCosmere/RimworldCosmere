using Cosmere;
using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Allomancy.Hediff;
using Cosmere.System.Scadrial.Def;
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
    private readonly MetallicArtsMetalDef? metal = null;
    private int ticksAlive;
    public Pawn? owner { get; set; }
    private new TimeBubbleProperties props => (TimeBubbleProperties)base.props;

    private HediffDef? hediffToApply => metal?.defName switch {
        "Cadmium" => HediffDefOf.Cosmere_Scadrial_Hediff_TimeBubbleCadmium,
        "Bendalloy" => HediffDefOf.Cosmere_Scadrial_Hediff_TimeBubbleBendalloy,
        _ => null,
    };

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
        Verse.Hediff? hediff = Enumerable.FirstOrDefault(
            pawn.health?.hediffSet?.hediffs,
            x => {
                return x is AllomanticHediff hediff &&
                       hediff.sourceAbilities.Cast<AllomancyAbility>()
                           .Any(ability => ability.metal.Equals(metal));
            }
        );

        return hediff is not AllomanticHediff allomanticHediff
            ? 0f
            : allomanticHediff.severityCalculator?.severity ?? 0f;
    }
}