using Cosmere.Core.Ability.Autocast;
using Cosmere.Core.Def;
using Cosmere.System.Scadrial.Gene;
using Verse;

namespace Cosmere.System.Scadrial.UI;

// Lets an autocast rule hold a feruchemical dial without Core having to know
// what a metalmind is.
public sealed class FeruchemyDial : IAutocastDial {
    public AutocastRuleKind Kind => AutocastRuleKind.FeruchemyDial;

    public float IdleTarget => Feruchemist.IdleTarget;

    public float MinTarget => 0f;

    public float MaxTarget => 100f;

    public bool TrySetTarget(Pawn pawn, string targetId, float value) {
        if (pawn.genes == null) return false;

        MetalDef? metal = DefDatabase<MetalDef>.GetNamedSilentFail(targetId);
        if (metal == null || !pawn.genes.HasFeruchemicGeneForMetal(metal)) return false;

        Feruchemist? gene = pawn.genes.GetFeruchemicGeneForMetal(metal);
        if (gene == null) return false;

        gene.targetValue = value;

        return true;
    }
}
