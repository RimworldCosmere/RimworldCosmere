using Verse;

namespace Cosmere.Roshar.ParticleSystem.LesserSpren.SprenControllers;

public class SandsprenController : BaseSprenController {
    public override SprenType sprenType => SprenType.Sandspren;
    public override bool isEnabled => true;
    public override bool isNatureSpren => true;
    public override float cellSpawnChance => 0.01f;

    public override bool IsCellValid(IntVec3 position, Map map) {
        if (!IsInBounds(position, map)) return false;

        TerrainDef? terrain = GetTerrain(position, map);
        if (terrain == null) return false;

        return TerrainNameContains(terrain, "sand", "gravel", "desert");
    }
}