using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart;

public abstract class ProgressionTrigger {
    public abstract bool IsMet(GameComponent_ScenarioProgression comp);
}

public class DaysPassedTrigger : ProgressionTrigger {
    public int days;

    public override bool IsMet(GameComponent_ScenarioProgression comp) {
        return GenDate.DaysPassed >= days;
    }
}

public class PawnSkillTrigger : ProgressionTrigger {
    public string pawnName = "";
    public string skill = "";
    public int minLevel;

    public override bool IsMet(GameComponent_ScenarioProgression comp) {
        Pawn? pawn = comp.FindPawnByName(pawnName);
        if (pawn == null) return false;

        SkillDef? skillDef = DefDatabase<SkillDef>.GetNamedSilentFail(skill);
        if (skillDef == null) return false;

        SkillRecord? record = pawn.skills?.GetSkill(skillDef);
        return record != null && record.Level >= minLevel;
    }
}

public class PawnIdealTrigger : ProgressionTrigger {
    public string pawnName = "";
    public int minIdeal;

    public override bool IsMet(GameComponent_ScenarioProgression comp) {
        Pawn? pawn = comp.FindPawnByName(pawnName);
        if (pawn == null) return false;

        Cosmere.System.Roshar.Gene.Surgebinder? surgebinder =
            pawn.genes?.GetFirstGeneOfType<Cosmere.System.Roshar.Gene.Surgebinder>();
        return surgebinder != null && surgebinder.currentIdeal >= minIdeal;
    }
}

public class ColonyWealthTrigger : ProgressionTrigger {
    public float minWealth;

    public override bool IsMet(GameComponent_ScenarioProgression comp) {
        Map? map = Find.CurrentMap;
        return map != null && map.wealthWatcher.WealthTotal >= minWealth;
    }
}

public class ResearchCompletedTrigger : ProgressionTrigger {
    public string research = "";

    public override bool IsMet(GameComponent_ScenarioProgression comp) {
        ResearchProjectDef? def = DefDatabase<ResearchProjectDef>.GetNamedSilentFail(research);
        return def != null && def.IsFinished;
    }
}

public class EventOccurredTrigger : ProgressionTrigger {
    public string eventKey = "";

    public override bool IsMet(GameComponent_ScenarioProgression comp) {
        return comp.HasEventFired(eventKey);
    }
}

public class PawnCountTrigger : ProgressionTrigger {
    public int minCount;

    public override bool IsMet(GameComponent_ScenarioProgression comp) {
        Map? map = Find.CurrentMap;
        return map != null && map.mapPawns.FreeColonistsCount >= minCount;
    }
}

public class PawnAliveTrigger : ProgressionTrigger {
    public string pawnName = "";
    public bool alive = true;

    public override bool IsMet(GameComponent_ScenarioProgression comp) {
        Pawn? pawn = comp.FindPawnByName(pawnName);
        bool isAlive = pawn != null && !pawn.Dead;
        return alive == isAlive;
    }
}

public class ThreatsSurvivedTrigger : ProgressionTrigger {
    public int minThreats;

    public override bool IsMet(GameComponent_ScenarioProgression comp) {
        return (Find.StoryWatcher?.statsRecord?.numThreatBigs ?? 0) >= minThreats;
    }
}
