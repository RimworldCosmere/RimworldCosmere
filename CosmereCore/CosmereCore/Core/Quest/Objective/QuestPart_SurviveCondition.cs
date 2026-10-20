using RimWorld;
using Verse;

namespace Cosmere.Core.Quest.Objective;

/// <summary>
///     Completes once the subject has stood exposed through a GameCondition and it has ended
///     with them still standing. The Stormfather capstone, and anything else worth surviving.
/// </summary>
public class QuestPart_SurviveCondition : QuestPart_CosmereActivable {
    public GameConditionDef? conditionDef;
    private bool conditionSeen;
    public bool requireUnroofed = true;
    public Pawn? subject;

    protected override bool IsSatisfied() {
        if (subject == null || subject.Dead) {
            Fail();
            return false;
        }

        Verse.Map? map = subject.MapHeld;
        if (map == null || conditionDef == null) return false;

        if (!map.gameConditionManager.ConditionIsActive(conditionDef)) return conditionSeen;

        if (!IsExposed(map)) return false;

        if (subject.Downed) {
            Fail();
            return false;
        }

        conditionSeen = true;
        return false;
    }

    private bool IsExposed(Verse.Map map) {
        if (subject is not { Spawned: true }) return false;

        return !requireUnroofed || !subject.Position.Roofed(map);
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Defs.Look(ref conditionDef, "conditionDef");
        Scribe_References.Look(ref subject, "subject");
        Scribe_Values.Look(ref requireUnroofed, "requireUnroofed", true);
        Scribe_Values.Look(ref conditionSeen, "conditionSeen");
    }
}
