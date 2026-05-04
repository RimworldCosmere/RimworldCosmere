using Cosmere.Core.UI.Dock;
using Cosmere.System.Scadrial.Gene;
using Verse;

namespace Cosmere.System.Scadrial.UI;

public sealed class ScadrialTwinbornPairProvider : IDualInvestiturePairProvider {
    public void Build(Pawn pawn, IReadOnlyList<string> activeSystemIds, Dictionary<string, IDualInvestiturePair> output) {
        if (pawn.genes == null) return;

        bool hasAllomancy = false;
        bool hasFeruchemy = false;
        for (int i = 0; i < activeSystemIds.Count; i++) {
            if (activeSystemIds[i] == AllomancyInvestitureProvider.Id) {
                hasAllomancy = true;
            }
            else if (activeSystemIds[i] == FeruchemyInvestitureProvider.Id) hasFeruchemy = true;
        }

        if (!hasAllomancy || !hasFeruchemy) return;

        Dictionary<string, Allomancer> allomancers = new Dictionary<string, Allomancer>();
        Dictionary<string, Feruchemist> feruchemists = new Dictionary<string, Feruchemist>();

        List<Verse.Gene> all = pawn.genes.GenesListForReading;
        for (int i = 0; i < all.Count; i++) {
            if (all[i] is Allomancer a && !a.Overridden) {
                allomancers[a.metal.defName] = a;
            }
            else if (all[i] is Feruchemist f && !f.Overridden) feruchemists[f.metal.defName] = f;
        }

        foreach (KeyValuePair<string, Allomancer> kv in allomancers) {
            if (feruchemists.TryGetValue(kv.Key, out Feruchemist? f)) {
                output[kv.Key] = new ScadrialTwinbornPair(kv.Value, f);
            }
        }
    }
}
