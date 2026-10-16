using Cosmere.Core.Nightwatcher;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Nightwatcher;

public class MemoryLossApplicator : ICurseApplicator, INightwatcherEffectDescriber {
    public string? DescribeEffects(NightwatcherApplicationContext? context = null) {
        return "All skills reset to 4";
    }

    public void Apply(Pawn pawn, Verse.Def def, NightwatcherApplicationContext? context = null) {
        if (pawn.skills == null) return;
        List<SkillRecord> skills = pawn.skills.skills;
        for (int i = 0; i < skills.Count; i++) {
            skills[i].Level = 4;
            skills[i].xpSinceLastLevel = 0f;
            skills[i].xpSinceMidnight = 0f;
        }

        Log.Info($"MemoryLossApplicator: reset all skills to max 4 for {pawn.NameShortColored}");
    }
}
