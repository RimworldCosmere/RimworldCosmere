using Cosmere.Core.Threat;
using Cosmere.System.Roshar.Gene;
using Verse;
using Shardblade = Cosmere.System.Roshar.Surgebinding.Ability.Shardblade;
using Shardplate = Cosmere.System.Roshar.Surgebinding.Ability.Shardplate;

namespace Cosmere.System.Roshar.Threat;

/// <summary>
///     Reads a pawn's Nahel bonds and shards, and hands the numbers to RadiantThreat.
/// </summary>
/// <remarks>
///     Shards are read from the abilities rather than from what is equipped, because Plate toggles
///     in and out of combat and a Blade is ten heartbeats away from a bare hand.
/// </remarks>
public class RosharThreatContributor : IThreatContributor {
    public string SystemId => "Roshar";

    public float GainForPawn(Pawn pawn) {
        List<Verse.Gene>? genes = pawn.genes?.GenesListForReading;
        if (genes == null) return 0f;

        List<float> bonds = [];
        for (int i = 0; i < genes.Count; i++) {
            if (genes[i] is Surgebinder { Active: true } surgebinder) {
                bonds.Add(RadiantThreat.ForBond(surgebinder.CurrentIdeal));
            }
        }

        float shards = RadiantThreat.ForShards(HasBlade(pawn), HasPlate(pawn));
        if (bonds.Count == 0 && shards <= 0f) return 0f;

        return RadiantThreat.ForPawn(bonds, shards) - 1f;
    }

    /// <summary>
    ///     Asks the ability its own question, so a Radiant's living Blade and a carried dead one
    ///     are judged by the one rule rather than by a second copy of it here.
    /// </summary>
    private static bool HasBlade(Pawn pawn) {
        return pawn.abilities?.GetAbility(AbilityDefOf.Cosmere_Roshar_Ability_ToggleShardblade)
            is Shardblade blade && blade.PawnHasShardblade();
    }

    private static bool HasPlate(Pawn pawn) {
        return pawn.abilities?.GetAbility(AbilityDefOf.Cosmere_Roshar_Ability_ToggleShardplate) is Shardplate;
    }
}
