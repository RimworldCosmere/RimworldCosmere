using Cosmere.Core.ScenarioPart;
using Cosmere.System.Scadrial.Util;
using Verse;

namespace Cosmere.System.Scadrial.ScenarioPart;

public sealed class ScadrialNamedPawnApplier : INamedPawnApplier {
    public void Apply(Pawn pawn, NamedPawnDef template) {
        if (template.mistborn) {
            GeneUtility.AddMistborn(pawn, false, true);
        }

        if (template.fullFeruchemist) {
            GeneUtility.AddFullFeruchemist(pawn, false, true);
        }
    }
}
