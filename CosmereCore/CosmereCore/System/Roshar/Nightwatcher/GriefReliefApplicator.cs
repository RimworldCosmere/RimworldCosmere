using System.Collections.Generic;
using Cosmere.System.Roshar.Def;
using RimWorld;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Roshar.Nightwatcher;

public class GriefReliefApplicator : IBoonApplicator {
    public void Apply(Pawn pawn, NightwatcherBoonDef def, Dictionary<string, object>? context = null) {
        if (pawn.needs?.mood?.thoughts?.memories == null) return;
        List<Thought_Memory> memories = pawn.needs.mood.thoughts.memories.Memories;
        for (int i = memories.Count - 1; i >= 0; i--) {
            if (memories[i].MoodOffset() < 0f)
                pawn.needs.mood.thoughts.memories.RemoveMemory(memories[i]);
        }
        Logger.Info($"GriefReliefApplicator: cleared negative memories for {pawn.NameShortColored}");
    }
}
