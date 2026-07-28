using Concord;
using Verse;

namespace Cosmere.System.Roshar.Patch.Nightwatcher;

[Patch]
public abstract class AgelessBodyPatch : Pawn_AgeTracker {
    [InjectField("pawn")]
    private readonly Pawn pawn = null!;

    protected AgelessBodyPatch(Pawn newPawn) : base(newPawn) { }

    [Inject(At.Return, nameof(BiologicalTicksPerTick))]
    private void AfterBiologicalTicksPerTick(ControlHandle<float> ch) {
        HediffDef? agelessDef = HediffDefOf.Cosmere_Roshar_Hediff_NW_BoonPassive_AgelessBody;
        if (agelessDef == null) return;
        if (pawn?.health?.hediffSet?.HasHediff(agelessDef) == true) {
            ch.ReturnValue *= 0.5f;
        }
    }
}
