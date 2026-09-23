using Verse;

namespace Cosmere.Core.ScenarioPart;

public interface INamedPawnApplier {
    void Apply(Pawn pawn, NamedPawnDef template);
}
