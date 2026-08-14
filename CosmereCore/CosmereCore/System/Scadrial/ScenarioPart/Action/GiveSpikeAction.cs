using Cosmere.Core.Def;
using Cosmere.Core.ScenarioPart;
using Cosmere.Core.ScenarioPart.Action;
using Cosmere.System.Scadrial.Hemalurgy;
using Cosmere.System.Scadrial.Hemalurgy.Util;
using RimWorld;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Scadrial.ScenarioPart.Action;

/// <summary>
///     Drives a charged spike into someone without anyone having to perform the surgery. Ruin
///     does not ask, and the story beats where it happens are not operations.
///     <para>
///         Routed through HemalurgicImplantUtility so the spike behaves exactly like a surgically
///         implanted one - it grants its power, joins the unified spikes hediff, and opens the
///         pawn to Ruin's influence, which scales with how many spikes they carry.
///     </para>
/// </summary>
public class GiveSpikeAction : ProgressionAction {
    /// <summary>The metal the spike is made of. Pewter for the one Ruin gave Spook.</summary>
    public string metal = string.Empty;

    public string pawnName = string.Empty;

    /// <summary>The GeneDef the spike grants, for the allomantic and feruchemic steal types.</summary>
    public string stolenDefName = string.Empty;

    /// <summary>
    ///     Set independently of the metal on purpose. The metal-to-type map has pewter stealing
    ///     Feruchemy, but the spike Ruin drove into Spook was pewter and granted Allomantic
    ///     pewter, so the beat needs to say which it means rather than infer it.
    /// </summary>
    public HemalurgicStealType stealType = HemalurgicStealType.PhysicalAllomancy;

    public float chargeStrength = 1f;
    public bool optional;

    public override void Execute(GameComponent_ScenarioProgression comp) {
        Pawn? pawn = comp.FindPawnByName(pawnName);
        if (pawn == null) {
            if (!optional) Logger.Warning($"ScenarioProgression: Pawn '{pawnName}' not found for GiveSpike");
            return;
        }

        MetalDef? metalDef = DefDatabase<MetalDef>.GetNamedSilentFail(metal);
        if (metalDef == null) {
            Logger.Warning($"ScenarioProgression: MetalDef '{metal}' not found for GiveSpike");
            return;
        }

        ImplantedSpikeData spike = new ImplantedSpikeData {
            metalDefName = metalDef.defName,
            stealType = stealType,
            stolenDefName = stolenDefName,
            stolenDefNames = [stolenDefName],
            chargeStrength = chargeStrength,
        };

        BodyPartRecord? torso = null;
        foreach (BodyPartRecord part in pawn.health.hediffSet.GetNotMissingParts()) {
            if (part.def != pawn.RaceProps.body.corePart.def) continue;

            torso = part;
            break;
        }

        HemalurgicImplantUtility.ApplyHemalurgicEffect(pawn, spike);
        HemalurgicImplantUtility.AddToUnifiedHediff(pawn, spike, torso);
        HemalurgicImplantUtility.UpdateRuinsInfluence(pawn);

        Logger.Important($"ScenarioProgression: drove a {metalDef.defName} spike into {pawnName}.");
    }

    public override string? Describe() {
        return "CS_Progression_Effect_Spike".Translate(pawnName.Named("PAWN"), metal.Named("METAL")).Resolve();
    }
}
