using Cosmere.Core.Nightwatcher;
using RimWorld;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Roshar.Nightwatcher;

public class GriefReliefApplicator : IBoonApplicator, INightwatcherEffectDescriber {
    public string? DescribeEffects(NightwatcherApplicationContext? context = null) {
        return "Clears all negative memories";
    }

    public void Apply(Pawn pawn, Verse.Def def, NightwatcherApplicationContext? context = null) {
        if (pawn.needs?.mood?.thoughts?.memories == null) return;
        List<Thought_Memory> memories = pawn.needs.mood.thoughts.memories.Memories;
        for (int i = memories.Count - 1; i >= 0; i--) {
            if (memories[i].MoodOffset() < 0f) {
                pawn.needs.mood.thoughts.memories.RemoveMemory(memories[i]);
            }
        }

        Logger.Info($"GriefReliefApplicator: cleared negative memories for {pawn.NameShortColored}");
    }
}