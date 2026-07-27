using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart.Action;

public class AddTraitAction : ProgressionAction {
    public int degree;
    public string pawnName = string.Empty;
    public string trait = string.Empty;

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
