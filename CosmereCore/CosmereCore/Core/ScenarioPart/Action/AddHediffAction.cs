using RimWorld;
using Verse;

namespace Cosmere.Core.ScenarioPart.Action;

/// <summary>
///     Puts a hediff on a named pawn, or on the whole colony when no name is given. For a beat
///     whose cost lands on a body rather than on a stockpile.
/// </summary>
public class AddHediffAction : ProgressionAction {
    public string hediff = string.Empty;

    /// <summary>Leave empty to hit every free colonist instead of one named pawn.</summary>
    public string pawnName = string.Empty;

    /// <summary>Chance per colonist when applying colony-wide. Ignored for a named pawn.</summary>
    public float chance = 1f;

    public float severity = -1f;

    /// <summary>The beat still reads as intended when the pawn is already gone.</summary>
    public bool optional;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        HediffDef? def = DefDatabase<HediffDef>.GetNamedSilentFail(hediff);
        if (def == null) {
            Log.Warn($"ScenarioProgression: HediffDef '{hediff}' not found for AddHediff");
            return;
        }

        if (pawnName.Length > 0) {
            Pawn? pawn = comp.FindPawnByName(pawnName);
            if (pawn == null) {
                if (!optional) Log.Warn($"ScenarioProgression: Pawn '{pawnName}' not found for AddHediff");
                return;
            }

            Apply(pawn, def);
            return;
        }

        List<Verse.Map> maps = Find.Maps;
        for (int i = 0; i < maps.Count; i++) {
            if (!maps[i].IsPlayerHome) continue;

            List<Pawn> colonists = maps[i].mapPawns.FreeColonistsSpawned;
            for (int j = 0; j < colonists.Count; j++) {
                if (Rand.Chance(chance)) Apply(colonists[j], def);
            }
        }
    }

    private void Apply(Pawn pawn, HediffDef def) {
        if (pawn.health.hediffSet.HasHediff(def)) return;

        Verse.Hediff made = HediffMaker.MakeHediff(def, pawn);
        if (severity > 0f) made.Severity = severity;
        pawn.health.AddHediff(made);
    }

    public override string? Describe() {
        HediffDef? def = DefDatabase<HediffDef>.GetNamedSilentFail(hediff);
        if (def == null) return null;

        string who = pawnName.Length > 0
            ? pawnName
            : "CC_Progression_Effect_WholeColony".Translate().Resolve();

        return "CC_Progression_Effect_Hediff".Translate(who.Named("PAWN"), def.label.Named("HEDIFF")).Resolve();
    }
}
