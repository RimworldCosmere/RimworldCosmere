using Verse;

namespace Cosmere.Core.ScenarioPart;

public static class NamedPawnApplierRegistry {
    private static readonly List<INamedPawnApplier> appliers = [];

    public static void Register(INamedPawnApplier applier) {
        appliers.Add(applier);
    }

    public static void ApplyAll(Pawn pawn, NamedPawnDef template) {
        for (int i = 0; i < appliers.Count; i++) {
            appliers[i].Apply(pawn, template);
        }
    }
}
