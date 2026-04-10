using Cosmere.Core.Comp.Thing;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.GameCondition;

public class TheWeeping : RimWorld.GameCondition {
    private const int DrainTickInterval = 250;
    private const float SpheresDrainPerInterval = 0.5f;

    public override void Init() {
        base.Init();
        SingleMap?.weatherManager.TransitionTo(WeatherDefOf.Rain);
    }

    public override void GameConditionTick() {
        base.GameConditionTick();
        if (!GenTicks.IsTickInterval(DrainTickInterval)) return;

        Map? map = SingleMap;
        if (map == null) return;

        if (map.weatherManager.curWeather != WeatherDefOf.Rain &&
            map.weatherManager.curWeather != WeatherDefOf.FoggyRain) {
            map.weatherManager.TransitionTo(WeatherDefOf.Rain);
        }

        DrainExposedSpheres(map);
    }

    public override void End() {
        base.End();
        SingleMap?.weatherManager.TransitionTo(WeatherDefOf.Clear);
    }

    private static void DrainExposedSpheres(Map map) {
        List<Verse.Thing> allThings = map.listerThings.AllThings;
        for (int i = 0; i < allThings.Count; i++) {
            Verse.Thing thing = allThings[i];
            if (thing is Pawn) continue;
            if (!thing.TryGetComp(out InvestitureHolder holder)) continue;
            if (holder.currentInvestitureSelf <= 0f) continue;
            if (thing.Position.Roofed(map)) continue;

            holder.currentInvestitureSelf = Mathf.Max(0f, holder.currentInvestitureSelf - SpheresDrainPerInterval);
        }
    }

    public static bool IsActive(Map map) {
        List<RimWorld.GameCondition> conditions = map.gameConditionManager.ActiveConditions;
        for (int i = 0; i < conditions.Count; i++) {
            if (conditions[i] is TheWeeping) return true;
        }
        return false;
    }
}
