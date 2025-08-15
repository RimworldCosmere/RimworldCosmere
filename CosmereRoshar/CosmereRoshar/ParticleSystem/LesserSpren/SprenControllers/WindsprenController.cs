using Verse;

namespace Cosmere.Roshar.ParticleSystem.LesserSpren.SprenControllers;

public class WindsprenController : BaseSprenController {
    public override SprenType sprenType => SprenType.Windspren;
    public override bool isEnabled => true;
    public override bool isNatureSpren => true;
    public override float cellSpawnChance => 0.0025f;
    public override int minParticlesPerCell => 1;
    public override int maxParticlesPerCell => 2;

    public override bool IsCellValid(IntVec3 position, Map map) {
        if (!IsInBounds(position, map)) return false;

        // Wind spren appear in open areas
        return !position.Roofed(map) &&
               map.thingGrid.ThingsListAt(position).Count(t => t.def.blockWind) == 0;
    }
}