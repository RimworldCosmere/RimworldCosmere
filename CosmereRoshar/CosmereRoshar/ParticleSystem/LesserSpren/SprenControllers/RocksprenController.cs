using Verse;

namespace Cosmere.Roshar.ParticleSystem.LesserSpren.SprenControllers;

public class RocksprenController : BaseSprenController {
    public override SprenType sprenType => SprenType.Rockspren;
    public override bool isEnabled => true;
    public override bool isNatureSpren => true;
    public override int maxParticlesPerCell => 6;

    public override bool IsCellValid(IntVec3 position, Map map) {
        if (!IsInBounds(position, map)) return false;

        // Check terrain for rock
        TerrainDef? terrain = GetTerrain(position, map);
        if (terrain != null) {
            bool hasRockTag = HasTerrainTag(terrain, "Rock");
            if (hasRockTag) {
                return true;
            }

            if (TerrainNameContains(terrain, "rock", "stone", "granite", "marble", "slate", "rubble")) {
                return true;
            }
        }

        // Check for stone chunks at this position
        foreach (string defName in map.thingGrid.ThingsListAt(position).Select(thing => thing.def.defName)) {
            if (defName.StartsWith("Chunk")) {
                return true;
            }

            if (defName.Equals("Filth_RubbleRock")) {
                return true;
            }
        }

        return false;
    }
}