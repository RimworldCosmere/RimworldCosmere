using CosmereCore.Util;
using RimWorld;
using Verse;

namespace CosmereCore.StatPart;

public class BuildingMaterial : RimWorld.StatPart {
    private const int baseMultiplier = 200;

    public override void TransformValue(StatRequest req, ref float val) {
        if (!req.HasThing || req.Thing.def.category != ThingCategory.Building) return;

        if (TryGetBuildingMass(req, out float mass)) val *= mass;
    }

    public override string? ExplanationPart(StatRequest req) {
        if (req.Thing?.def.category != ThingCategory.Building || !TryGetBuildingMass(req, out float mass)) {
            return null;
        }

        return "CS_StatsReport_BuildingMaterial".Translate() + ": x" + mass.ToStringPercent();
    }

    private bool TryGetBuildingMass(StatRequest req, out float mass) {
        float buildingMass = req.BuildableDef.GetStatValueAbstract(RimWorld.StatDefOf.Mass);
        mass = (buildingMass + baseMultiplier) *
               req.Thing.def.Size.x *
               req.Thing.def.Size.z *
               MetalDetector.GetMetal(req.Thing, 0, true);

        return true;
    }
}