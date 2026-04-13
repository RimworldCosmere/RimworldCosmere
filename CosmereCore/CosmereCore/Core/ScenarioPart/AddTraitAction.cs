using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart;

public class AddTraitAction : ProgressionAction {
    public string pawnName = "";
    public string trait = "";
    public int degree;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        Pawn? pawn = comp.FindPawnByName(pawnName);
        if (pawn == null) {
            Logger.Warning($"ScenarioProgression: Pawn '{pawnName}' not found for AddTrait");
            return;
        }

        TraitDef? traitDef = DefDatabase<TraitDef>.GetNamedSilentFail(trait);
        if (traitDef == null) {
            Logger.Warning($"ScenarioProgression: Trait '{trait}' not found");
            return;
        }

        pawn.story?.traits?.GainTrait(new Trait(traitDef, degree));
    }
}
