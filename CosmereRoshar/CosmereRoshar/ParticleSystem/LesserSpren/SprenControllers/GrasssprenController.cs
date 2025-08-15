using Verse;

namespace Cosmere.Roshar.ParticleSystem.LesserSpren.SprenControllers;

public class GrasssprenController : BaseSprenController {
    public override SprenType sprenType => SprenType.Grassspren;
    public override bool isEnabled => true;
    public override bool isNatureSpren => true;
    public override float cellSpawnChance => 0.01f; // 1% chance per cell
    public override int minParticlesPerCell => 1;
    public override int maxParticlesPerCell => 3;

    public override bool IsCellValid(IntVec3 position, Map map) {
        if (!IsInBounds(position, map)) return false;

        // Check for grass and moss plants at this position
        return map.thingGrid.ThingsListAt(position)
            .Any(thing => thing.def.defName.StartsWith("Plant_Grass") ||
                          thing.def.defName.StartsWith("Plant_Moss") ||
                          thing.def.defName.Contains("Grass") ||
                          thing.def.defName.Contains("Moss")
            );
    }
}