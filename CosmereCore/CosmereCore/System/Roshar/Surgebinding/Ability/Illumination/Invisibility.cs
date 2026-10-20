using Concord;
using Cosmere.Core.Ability;
using RimWorld;
using Verse;
using Verse.Profile;

namespace Cosmere.System.Roshar.Surgebinding.Ability.Illumination;

public class Invisibility : SurgebindingAbility {
    public static readonly HashSet<Pawn> InvisiblePawns = [];

    public Invisibility(Pawn pawn) : base(pawn) { }

    public Invisibility(Pawn pawn, AbilityDef def) : base(pawn, def) { }

    public override float GetStrength(Status? desiredStatus = null) {
        return base.GetStrength(desiredStatus) * (0.5f + Gene.CurrentIdeal * 0.5f);
    }

    protected override void OnEnable() {
        base.OnEnable();
        InvisiblePawns.Add(pawn);
    }

    protected override void OnDisable() {
        base.OnDisable();
        InvisiblePawns.Remove(pawn);
    }

    [Patch(typeof(MemoryUtility))]
    public static class InvisibilityStateClearer {
        [Inject(At.Return, nameof(MemoryUtility.ClearAllMapsAndWorld))]
        private static void AfterClearAllMapsAndWorld() {
            InvisiblePawns.Clear();
        }
    }
}
