
using Verse;
namespace Cosmere.System.Roshar.Surgebinding.Ability.Illumination;

public static class LightweavingDecoyRegistry {
    private static readonly List<Pawn> Decoys = [];

    public static List<Pawn> All => Decoys;

    public static void Add(Pawn decoy) {
        Decoys.Add(decoy);
    }

    public static void Remove(Pawn decoy) {
        Decoys.Remove(decoy);
    }

    public static void RemoveAt(int index) {
        Decoys.RemoveAt(index);
    }

    public static void Clear() {
        Decoys.Clear();
    }
}
