using Concord;
using Cosmere.System.Roshar.Surgebinding.Ability.Transformation;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.Soulcasting;

[Patch]
public abstract class SoulcastWorkTypePatch : Pawn_WorkSettings {
    [InjectField("pawn")]
    private readonly Pawn trackedPawn = null!;

    protected SoulcastWorkTypePatch(Pawn pawn) : base(pawn) { }

    [Inject(At.Head, nameof(SetPriority))]
    private Control BeforeSetPriority(WorkTypeDef w, int priority) {
        if (priority != 0) return Control.Continue;

        WorkTypeDef? soulcastWork = RosharWorkTypeDefOf.Cosmere_Roshar_WorkType_Soulcasting;
        if (soulcastWork == null || w != soulcastWork) return Control.Continue;

        if (trackedPawn?.abilities == null) return Control.Continue;

        List<Ability> abilities = trackedPawn.abilities.AllAbilitiesForReading;
        for (int i = 0; i < abilities.Count; i++) {
            if (abilities[i] is Soulcast) return Control.Cancel;
        }

        return Control.Continue;
    }
}
