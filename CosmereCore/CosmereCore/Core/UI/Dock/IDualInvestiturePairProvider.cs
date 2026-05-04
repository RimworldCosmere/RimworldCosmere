
using Verse;
namespace Cosmere.Core.UI.Dock;

public interface IDualInvestiturePairProvider {
    void Build(Pawn pawn, IReadOnlyList<string> activeSystemIds, Dictionary<string, IDualInvestiturePair> output);
}
