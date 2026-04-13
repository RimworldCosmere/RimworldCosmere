using RimWorld;
using Verse;

namespace Cosmere.Core.Ideology;

public class ThoughtWorker_Precept_VorinRoles : ThoughtWorker_Precept {
    protected override ThoughtState ShouldHaveThought(Pawn p) {
        if (p.Faction == null || p.Map == null) return ThoughtState.Inactive;

        // Men: physical work (combat, construction, mining). Women: scholarly work (research, art, crafting).
        // Mood boost if the pawn has work assignments matching their Vorin role.

        bool isMale = p.gender == Gender.Male;
        Pawn_WorkSettings workSettings = p.workSettings;
        if (workSettings == null) return ThoughtState.Inactive;

        if (isMale) {
            // Men should have combat-type work enabled
            WorkTypeDef hunting = DefDatabase<WorkTypeDef>.GetNamedSilentFail("Hunting");
            WorkTypeDef construction = DefDatabase<WorkTypeDef>.GetNamedSilentFail("Construction");
            WorkTypeDef mining = DefDatabase<WorkTypeDef>.GetNamedSilentFail("Mining");

            bool hasMaleWork = (hunting != null && workSettings.GetPriority(hunting) > 0)
                            || (construction != null && workSettings.GetPriority(construction) > 0)
                            || (mining != null && workSettings.GetPriority(mining) > 0);
            return hasMaleWork;
        } else {
            // Women should have scholarly work enabled
            WorkTypeDef research = DefDatabase<WorkTypeDef>.GetNamedSilentFail("Research");
            WorkTypeDef art = DefDatabase<WorkTypeDef>.GetNamedSilentFail("Art");
            WorkTypeDef crafting = DefDatabase<WorkTypeDef>.GetNamedSilentFail("Crafting");

            bool hasFemaleWork = (research != null && workSettings.GetPriority(research) > 0)
                              || (art != null && workSettings.GetPriority(art) > 0)
                              || (crafting != null && workSettings.GetPriority(crafting) > 0);
            return hasFemaleWork;
        }
    }
}
