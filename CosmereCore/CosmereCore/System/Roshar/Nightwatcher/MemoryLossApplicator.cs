using Cosmere.System.Roshar.Def;
using RimWorld;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Roshar.Nightwatcher;

public class MemoryLossApplicator : ICurseApplicator {
    public void Apply(Pawn pawn, NightwatcherCurseDef def) {
        if (pawn.skills == null) return;
        List<SkillRecord> skills = pawn.skills.skills;
        for (int i = 0; i < skills.Count; i++) {
            skills[i].Level = 4;
            skills[i].xpSinceLastLevel = 0f;
            skills[i].xpSinceMidnight = 0f;
        }

        Logger.Info($"MemoryLossApplicator: reset all skills to max 4 for {pawn.NameShortColored}");
    }
}