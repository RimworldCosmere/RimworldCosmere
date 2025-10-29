using Cosmere.Core.Gizmo;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Roshar.Surgebinding.Ability;
using Cosmere.System.Roshar.Surgebinding.Hediff;
using Verse;

namespace Cosmere.System.Roshar.Gizmo;

[StaticConstructorOnStartup]
public class SurgebindingAbilitySubGizmo : AbilitySubGizmo<Surgebinder, SurgebindingHediff> {
    public SurgebindingAbilitySubGizmo() { }

    public SurgebindingAbilitySubGizmo(Verse.Gizmo parent, Surgebinder gene, SurgebindingAbility ability) : base(
        parent,
        gene,
        ability
    ) { }
}