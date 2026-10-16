using Cosmere.System.Scadrial.Allomancy.Ability;
using Cosmere.System.Scadrial.Allomancy.Hediff;
using RimWorld;
using Verse;
using IAllomancerAbility =
    Cosmere.Core.Ability.IAbility<Cosmere.System.Scadrial.Gene.Allomancer,
        Cosmere.Core.Hediff.IHediff<Cosmere.System.Scadrial.Gene.Allomancer>>;

namespace Cosmere.System.Scadrial.Allomancy.Comp.Hediff;

public class GoldBurnProperties : HediffCompProperties {
    public GoldBurnProperties() {
        compClass = typeof(GoldBurn);
    }
}

public class GoldBurn : HediffComp {
    private static readonly ThoughtDef[] GoldThoughts = [
        ThoughtDefOf.Cosmere_Thought_Gold_Dread,
        ThoughtDefOf.Cosmere_Thought_Gold_Awe,
        ThoughtDefOf.Cosmere_Thought_Gold_Gratitude,
        ThoughtDefOf.Cosmere_Thought_Gold_Regret,
        ThoughtDefOf.Cosmere_Thought_Gold_Shame,
        ThoughtDefOf.Cosmere_Thought_Gold_Rage,
        ThoughtDefOf.Cosmere_Thought_Gold_Loss,
        ThoughtDefOf.Cosmere_Thought_Gold_Hope,
        ThoughtDefOf.Cosmere_Thought_Gold_Pride,
        ThoughtDefOf.Cosmere_Thought_Gold_Determination,
        ThoughtDefOf.Cosmere_Thought_Gold_Jealousy,
        ThoughtDefOf.Cosmere_Thought_Gold_Curiosity,
    ];

    private readonly List<ThoughtDef> memoriesAdded = [];
    private float lastSeverity;

    private MemoryThoughtHandler Memories => Pawn.needs.mood.thoughts.memories;

    private AllomanticHediff Parent => (AllomanticHediff)parent;

    public override void CompPostTick(ref float severityAdjustment) {
        base.CompPostTick(ref severityAdjustment);

        if (Pawn.Dead || !Pawn.IsHashIntervalTick(60)) {
            return;
        }

        Parent.ExtraSeverity += 0.01f;
        lastSeverity = Parent.Severity;

        // ~1 every 5 ticks
        if (Rand.Chance(0.2f)) {
            ThoughtDef random = GoldThoughts.RandomElement();
            memoriesAdded.AddDistinct(random);
            Thought_Memory? thought = Memories.GetFirstMemoryOfDef(random);
            if (thought == null) {
                Memories.TryGainMemoryFast(random);
            } else if (thought.CurStageIndex < random.stages.Count - 1) {
                thought.SetForcedStage(thought.CurStageIndex + 1);
            }
        }

        if (Rand.Chance(0.0025f) && Pawn.mindState != null && !Pawn.InMentalState) {
            IAllomancerAbility[] snapshot = [.. Parent.SourceAbilities];
            for (int i = 0; i < snapshot.Length; i++) {
                snapshot[i].UpdateStatus(BurningStatus.Off);
            }

            Pawn.mindState.mentalStateHandler.TryStartMentalState(
                RimWorld.MentalStateDefOf.Wander_OwnRoom,
                "Gold vision triggered a break",
                true
            );
        }
    }

    public override void CompPostPostRemoved() {
        base.CompPostPostRemoved();
        if (Pawn.Dead || Pawn.needs?.mood == null) return;

        Verse.Hediff? hediff = Pawn.health.AddHediff(HediffDefOf.Cosmere_Scadrial_Hediff_PostGold);
        hediff.Severity = lastSeverity;

        for (int i = 0; i < memoriesAdded.Count; i++) {
            Memories.RemoveMemoriesOfDef(memoriesAdded[i]);
        }

        Thought_Memory? thought = Memories.GetFirstMemoryOfDef(ThoughtDefOf.Cosmere_Thought_PostGold_Afterglow);
        if (thought != null) {
            thought.moodPowerFactor = lastSeverity;
        }
    }
}
