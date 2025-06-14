using System.Collections.Generic;
using RimWorld;
using Verse;

namespace CosmereScadrial.Abilities.Allomancy.Hediffs.Comps {
    public class GoldBurnProperties : HediffCompProperties {
        public GoldBurnProperties() {
            compClass = typeof(GoldBurn);
        }
    }

    public class GoldBurn : HediffComp {
        private readonly List<ThoughtDef> memoriesAdded = [];
        private Pawn hallucination;
        private bool hallucinationSpawned;
        private float lastSeverity;
        private int tickCounter;
        private MemoryThoughtHandler memories => Pawn.needs.mood.thoughts.memories;

        private new AllomanticHediff parent => base.parent as AllomanticHediff;

        public override void CompPostTick(ref float severityAdjustment) {
            base.CompPostTick(ref severityAdjustment);

            if (Pawn.Dead || !Pawn.IsHashIntervalTick(60)) {
                return;
            }

            tickCounter += 60;
            parent.extraSeverity += 0.01f; // Increase severity slowly over time
            lastSeverity = parent.Severity;

            // 2. Apply random thoughts (positive or negative)
            if (Rand.Chance(0.2f)) // ~1 every 5 ticks
            {
                var random = new[] {
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
                }.RandomElement();
                memoriesAdded.AddDistinct(random);
                var thought = memories.GetFirstMemoryOfDef(random);
                if (thought == null) {
                    memories.TryGainMemoryFast(random);
                } else if (thought.CurStageIndex < random.stages.Count) {
                    thought.SetForcedStage(thought.CurStageIndex + 1);
                }
            }

            // 3. Mental break chance
            if (Rand.Chance(0.0025f) && Pawn.mindState != null && !Pawn.InMentalState) {
                Pawn.mindState.mentalStateHandler.TryStartMentalState(
                    MentalStateDefOf.Wander_OwnRoom,
                    "Gold vision triggered a break",
                    true
                );
            }
        }

        public override void CompPostPostRemoved() {
            base.CompPostPostRemoved();

            // Apply afterglow buff
            var hediff = Pawn.health.AddHediff(HediffDefOf.Cosmere_Hediff_PostGold);
            hediff.Severity = lastSeverity;

            memoriesAdded.ForEach(mem => memories.RemoveMemoriesOfDef(mem));

            var thought = memories.GetFirstMemoryOfDef(ThoughtDefOf.Cosmere_Thought_PostGold_Afterglow);
            if (thought != null) {
                thought.moodPowerFactor = lastSeverity;
            }
        }
    }
}