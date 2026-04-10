using Cosmere.Core.Ability;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Illumination;

public class Invisibility : SurgebindingAbility {
    public static readonly HashSet<Pawn> InvisiblePawns = [];

    public Invisibility(Pawn pawn) : base(pawn) { }
    public Invisibility(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    public override float GetStrength(Status? desiredStatus = null) {
        return base.GetStrength(desiredStatus) * (0.5f + gene.currentIdeal * 0.5f);
    }

    protected override void OnEnable() {
        base.OnEnable();
        InvisiblePawns.Add(pawn);
    }

    protected override void OnDisable() {
        base.OnDisable();
        InvisiblePawns.Remove(pawn);
    }
}
