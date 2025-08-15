using Verse;

namespace Cosmere.Roshar.ParticleSystem.LesserSpren.SprenControllers;

public class RiversprenController : BaseSprenController {
    public override SprenType sprenType => SprenType.Riverspren;
    public override bool isEnabled => true;
    public override bool isNatureSpren => true;
    public override float cellSpawnChance => 0.03f;

    public override bool IsCellValid(IntVec3 position, Map map) {
        if (!IsInBounds(position, map)) return false;

        TerrainDef? terrain = GetTerrain(position, map);
        if (terrain == null) return false;
        if (terrain.waterBodyType.Equals(WaterBodyType.None)) return false;

        return HasTerrainTag(terrain, "River");
    }
}