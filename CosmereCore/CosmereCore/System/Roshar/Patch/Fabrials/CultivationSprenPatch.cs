using Concord;
using Cosmere.System.Roshar.Comp.Fabrials;
using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using Cosmere.System.Roshar.Thing.Building;
using RimWorld;
using Verse;

namespace Cosmere.System.Roshar.Patch.Fabrials;

[Patch]
public abstract class CultivationSprenPatch : Plant {
    private static readonly List<Building> ActiveLifeSprenBuildings = [];

    public static void RegisterBuilding(Building building) {
        if (building.GetComp<BasicFabrialAugmenter>()?.currentSpren == SprenType.Lifespren) {
            ActiveLifeSprenBuildings.Add(building);
        } else if (building.GetComp<BasicFabrialDiminisher>()?.currentSpren == SprenType.Lifespren) {
            ActiveLifeSprenBuildings.Add(building);
        }
    }

    public static void UnregisterBuilding(Building building) {
        ActiveLifeSprenBuildings.Remove(building);
    }

    [Inject(At.Return, nameof(GrowthRate))]
    private void AfterGrowthRate(ControlHandle<float> ch) {
        Plant self = this;
        if (self.Spawned && IsNearLifeSprenBuilding(self) == 1) {
            bool resting = true;
            if (!(GenLocalDate.DayPercent(self) < 0.25f)) {
                resting = GenLocalDate.DayPercent(self) > 0.8f;
            }

            if (self.LifeStage != PlantLifeStage.Growing || resting) {
                ch.ReturnValue *= 1f;
                return;
            }

            ch.ReturnValue *= 1.25f;
            return;
        }

        if (self.Spawned && IsNearLifeSprenBuilding(self) == 2) {
            ch.ReturnValue *= 0.25f;
        }
    }

    private static int IsNearLifeSprenBuilding(Plant plant) {
        Map? map = plant.Map;
        if (map == null) return 0;
        IntVec3 plantPos = plant.Position;

        foreach (Building thing in ActiveLifeSprenBuildings) {
            if (thing is FabrialBasicAugmenter building &&
                plantPos.DistanceTo(building.Position) <= 5f) {
                BasicFabrialAugmenter? comp = building.GetComp<BasicFabrialAugmenter>();
                if (comp is { powerOn: true } && comp.currentSpren == SprenType.Lifespren) {
                    return 1;
                }
            } else if (thing is FabrialBasicDiminisher diminisher &&
                       plantPos.DistanceTo(diminisher.Position) <= 5f) {
                BasicFabrialDiminisher? comp = diminisher.GetComp<BasicFabrialDiminisher>();
                if (comp is { powerOn: true } && comp.currentSpren == SprenType.Lifespren) {
                    return 2;
                }
            }
        }

        return 0;
    }
}
