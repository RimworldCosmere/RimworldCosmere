using Verse;

namespace Cosmere.Core.DefModExtension;

/// <summary>
///     Which pawns a skill is worth showing to.
/// </summary>
/// <remarks>
///     A skill nobody in the colony can ever use is noise on every character card. Both gates are
///     data rather than code so Core never has to know what any of these systems are - it asks the
///     registry, or it asks whether the pawn carries a gene.
/// </remarks>
public class InvestitureSkillExtension : Verse.DefModExtension {
    public string requiresInvestitureSystem = string.Empty;

    /// <summary>The gene a pawn must carry for this skill to appear at all.</summary>
    public string requiresGene = string.Empty;
}
