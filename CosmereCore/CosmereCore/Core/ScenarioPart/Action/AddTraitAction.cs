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
            Log.Warn($"ScenarioProgression: Pawn '{pawnName}' not found for AddTrait");
            return;
        }

        TraitDef? traitDef = DefDatabase<TraitDef>.GetNamedSilentFail(trait);
        if (traitDef == null) {
            Log.Warn($"ScenarioProgression: Trait '{trait}' not found");
            return;
        }

        pawn.story?.traits?.GainTrait(new Trait(traitDef, degree));
    }

    public override string? Describe() {
        TraitDef? def = DefDatabase<TraitDef>.GetNamedSilentFail(trait);
        if (def == null) return null;

        return "CC_Progression_Effect_Trait".Translate(
            pawnName.Named("PAWN"),
            def.DataAtDegree(degree).label.Named("TRAIT")
        ).Resolve();
    }
}
