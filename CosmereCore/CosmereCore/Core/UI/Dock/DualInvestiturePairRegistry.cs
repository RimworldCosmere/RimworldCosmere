
using Verse;
namespace Cosmere.Core.UI.Dock;

public static class DualInvestiturePairRegistry {
    private static readonly List<IDualInvestiturePairProvider> providers = [];

    public static void Register(IDualInvestiturePairProvider provider) {
        providers.Add(provider);
    }

    public static Dictionary<string, IDualInvestiturePair> Build(Pawn pawn, IReadOnlyList<string> activeSystemIds) {
        Dictionary<string, IDualInvestiturePair> result = new Dictionary<string, IDualInvestiturePair>();
        for (int i = 0; i < providers.Count; i++) {
            providers[i].Build(pawn, activeSystemIds, result);
        }

        return result;
    }
}
