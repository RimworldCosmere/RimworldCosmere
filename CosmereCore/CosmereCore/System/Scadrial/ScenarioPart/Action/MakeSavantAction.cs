using Cosmere.Core.Def;
using Cosmere.Core.Savant;
using Cosmere.Core.ScenarioPart;
using Cosmere.Core.ScenarioPart.Action;
using Cosmere.Core.Util;
using Cosmere.System.Scadrial.Savant;
using RimWorld;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Scadrial.ScenarioPart.Action;

/// <summary>
///     Burns someone all the way to savant on one metal.
///     <para>
///         Writes the burn-time record as well as the hediffs. The codex reads accumulated ticks
///         rather than the hediff, so a savant made only of hediffs shows as an ordinary Misting
///         on the one screen the player would go looking at.
///     </para>
/// </summary>
public class MakeSavantAction : ProgressionAction {
    /// <summary>Which metal they are savant in.</summary>
    public string metal = string.Empty;

    public string pawnName = string.Empty;

    /// <summary>Feruchemy rather than Allomancy. The two keep separate records and stages.</summary>
    public bool feruchemy;

    /// <summary>1 to 3. Three is the deep end, where it stops being reversible.</summary>
    public int stage = 3;

    public bool optional;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        Pawn? pawn = comp.FindPawnByName(pawnName);
        if (pawn == null) {
            if (!optional) Logger.Warning($"ScenarioProgression: Pawn '{pawnName}' not found for MakeSavant");
            return;
        }

        MetalDef? metalDef = DefDatabase<MetalDef>.GetNamedSilentFail(metal);
        if (metalDef == null) {
            Logger.Warning($"ScenarioProgression: MetalDef '{metal}' not found for MakeSavant");
            return;
        }

        SavantProfile profile = feruchemy ? SavantUtility.FeruchemyProfile : SavantUtility.AllomancyProfile;
        float ticks = stage switch {
            1 => profile.Stage1Ticks,
            2 => profile.Stage2Ticks,
            _ => profile.Stage3Ticks,
        };

        // Burn/store time is a Time record, which AddTo refuses. Never lowers somebody who
        // already burned their way past this on their own.
        RecordDef record = feruchemy
            ? RecordDefOf.GetTimeSpentStoringForMetal(metalDef)
            : RecordDefOf.GetTimeSpentBurningForMetal(metalDef);

        if (!RecordUtility.RaiseTo(pawn, record, ticks)) {
            Logger.Warning($"ScenarioProgression: could not write record '{record.defName}' for {pawnName}.");
        }

        HediffDef? savant = feruchemy
            ? ScadrialSavantUtility.GetFeruchemicalSavantHediffDef(metalDef)
            : ScadrialSavantUtility.GetAllomanticSavantHediffDef(metalDef);

        SavantUtility.ApplySavantHediffs(pawn, savant, null);
        Logger.Important($"ScenarioProgression: {pawnName} is a stage {stage} savant in {metalDef.defName}.");
    }

    public override string? Describe() {
        return "CS_Progression_Effect_Savant".Translate(pawnName.Named("PAWN"), metal.Named("METAL")).Resolve();
    }
}
