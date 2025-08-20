namespace Cosmere.Roshar.LesserSpren.SprenControllers;

public class SprenSpawnInfoComparer : IEqualityComparer<SprenSpawnInformation> {
    public static readonly SprenSpawnInfoComparer Instance = new SprenSpawnInfoComparer();

    public bool Equals(SprenSpawnInformation? x, SprenSpawnInformation? y) {
        if (x == null && y == null) return true;
        if (x == null || y == null) return false;

        // Consider two spawn infos equal if they have the same position and map
        // This prevents duplicate spawning in the same cell
        return x.position == y.position && x.map == y.map;
    }

    public int GetHashCode(SprenSpawnInformation obj) {
        if (obj == null) return 0;

        // Combine hash codes of position and map for unique identification
        int hash = 17;
        hash = hash * 31 + (obj.position?.GetHashCode() ?? 0);
        hash = hash * 31 + (obj.map?.GetHashCode() ?? 0);
        return hash;
    }
}