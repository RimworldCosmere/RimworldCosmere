using Verse;

namespace Cosmere.Roshar.ParticleSystem.LesserSpren.SprenControllers;

public abstract class BaseSprenController {
    public abstract SprenType sprenType { get; }
    public abstract bool isEnabled { get; }
    public abstract bool isNatureSpren { get; }

    // Particle count configuration
    public virtual int minParticlesPerCell => 2;
    public virtual int maxParticlesPerCell => 4;

    // Spawn chance configuration
    public virtual float cellSpawnChance => 0.05f; // 5% default chance per valid cell to spawn spren

    public abstract bool IsCellValid(IntVec3 position, Map map);

    public virtual List<IntVec3> GetDynamicCells(Map map) {
        return [];
    }

    protected static bool HasTerrainTag(TerrainDef terrain, string tag) {
        return terrain.HasTag(tag);
    }

    protected static bool TerrainNameContains(TerrainDef terrain, params string[] keywords) {
        string terrainName = terrain.defName.ToLower();
        return keywords.Any(keyword => terrainName.Contains(keyword.ToLower()));
    }

    protected static TerrainDef? GetTerrain(IntVec3 position, Map map) {
        return map.terrainGrid.TerrainAt(position);
    }

    protected static bool IsInBounds(IntVec3 position, Map map) {
        return position.InBounds(map);
    }
}