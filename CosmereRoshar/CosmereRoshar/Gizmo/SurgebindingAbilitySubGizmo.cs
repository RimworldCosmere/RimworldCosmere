using Cosmere.Core.Gizmo;
using Cosmere.Roshar.Gene;
using Cosmere.Roshar.Surgebinding.Ability;
using Cosmere.Roshar.Surgebinding.Hediff;
using Verse;

namespace Cosmere.Roshar.Gizmo;

[StaticConstructorOnStartup]
public class SurgebindingAbilitySubGizmo : AbilitySubGizmo<Surgebinder, SurgebindingHediff> {
    public SurgebindingAbilitySubGizmo() { }

    public SurgebindingAbilitySubGizmo(Verse.Gizmo parent, Surgebinder gene, SurgebindingAbility ability) : base(
        parent,
        gene,
        ability
    ) { }
}