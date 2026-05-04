using Cosmere.Core.Comp.Thing;
using Cosmere.System.Roshar.Comp.Map;
using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.System.Roshar.Gene;
using Cosmere.System.Roshar.Util;
using RimWorld;
using UnityEngine;
using Verse;
using Logger = Cosmere.Core.Logger;

namespace Cosmere.System.Roshar.GameCondition;

// TODO: Force the weather here?
// TODO: Draw a warning at the top
// TODO: Encourage pawns to go inside? Probably a workgiver?
public class Highstorm : RimWorld.GameCondition {
    private const float totalInvestitureToAbsorbPerItem = 2000f;

    private const float ShelterThreshold = 0.4f;

    private static readonly SimpleCurve stormIntensityCurve = [
        new CurvePoint(0f, 0f),
        new CurvePoint(0.2f, 0.15f),
        new CurvePoint(0.25f, 0.7f),
        new CurvePoint(0.35f, 1f),
        new CurvePoint(0.5f, 0.85f),
        new CurvePoint(0.65f, 1f),
        new CurvePoint(0.75f, 0.7f),
        new CurvePoint(0.8f, 0.15f),
        new CurvePoint(1f, 0f),
    ];

    private static readonly List<(IntVec3 offset, float weight)> weightedOffsets = [
        (IntVec3.West, 5f),
        (IntVec3.NorthWest, 3f),
        (IntVec3.SouthWest, 3f),
        (IntVec3.North, 1f),
        (IntVec3.South, 1f),
    ];

    private static readonly float TotalOffsetWeight = 13f;

    private static ResearchProjectDef? shieldingResearchCache;
    private static bool shieldingResearchLookedUp;
    private readonly List<Verse.Thing> exposedThings = [];

    private readonly Verse.Thing highstorm = ThingMaker.MakeThing(ThingDefOf.Cosmere_Roshar_Thing_Highstorm);

    private readonly int tickInterval = 30;

    public float CurrentIntensity { get; private set; }

    public bool IsDangerousPhase => CurrentIntensity >= ShelterThreshold;

    private float investitureToAbsorb => totalInvestitureToAbsorbPerItem / Duration * tickInterval * CurrentIntensity;

    private static IntVec3 GetRandomStormOffset(bool timesTwo) {
        float choice = Rand.Range(0f, TotalOffsetWeight);
        int multiplier = timesTwo ? 2 : 1;

        float cumulative = 0f;
        for (int i = 0; i < weightedOffsets.Count; i++) {
            cumulative += weightedOffsets[i].weight;
            if (choice <= cumulative) {
                return weightedOffsets[i].offset * multiplier;
            }
        }

        return IntVec3.West * multiplier;
    }

    public override void End() {
        base.End();
        StormShelterManager.ClearCache();
        StormShelterManager.FirstTickOfHighstorm = true;
        SingleMap?.weatherManager.TransitionTo(WeatherDefOf.FoggyRain);
        highstorm.Destroy();
    }

    public override float MinWindSpeed() {
        return CurrentIntensity;
    }

    public override void GameConditionTick() {
        base.GameConditionTick();
        DefModExtension.Highstorm? ext = def.GetModExtension<DefModExtension.Highstorm>();
        if (ext == null) {
            Logger.Error("DefModExtension.Highstorm is null on GameConditionDef!");
            return;
        }

        if (StormShelterManager.FirstTickOfHighstorm) {
            Map? shelterMap = SingleMap ?? Find.CurrentMap;
            if (shelterMap != null) {
                StormShelterManager.RebuildShelterCache(shelterMap);
            }

            StormShelterManager.FirstTickOfHighstorm = false;
        }

        if (!GenTicks.IsTickInterval(tickInterval)) return;
        float baseIntensity = stormIntensityCurve.Evaluate(Mathf.Clamp01(TicksPassed / (float)Duration));
        float seasonalMultiplier = 1f;
        HighstormScheduler? scheduler = SingleMap?.GetComponent<HighstormScheduler>();
        if (scheduler != null) {
            seasonalMultiplier = scheduler.SeasonalIntensity;
        }

        CurrentIntensity = baseIntensity * seasonalMultiplier;

        if (SingleMap == null) {
            Logger.Error("SingleMap is null, cannot process storm!");
            return;
        }

        ScatterAndDamageExposedItems();
    }

    private void ScatterAndDamageExposedItems() {
        List<Verse.Thing> allThings = SingleMap.listerThings.AllThings;
        exposedThings.Clear();

        for (int i = 0; i < allThings.Count; i++) {
            Verse.Thing thing = allThings[i];
            if (thing.Map != null && thing.ShouldBeMovedByStorm()) {
                exposedThings.Add(thing);
            }
        }

        for (int i = 0; i < exposedThings.Count; i++) {
            Verse.Thing thing = exposedThings[i];
            if (!Rand.Chance(CurrentIntensity)) continue;
            if (thing?.Map == null) continue;

            MoveItem(thing);
            if (thing?.Map == null) continue;

            if (!thing.Position.Roofed(thing.Map)) {
                DamageItem(thing);
            }

            if (thing?.Map == null) continue;

            if (!thing.Position.Roofed(thing.Map) &&
                thing.TryGetComp(out InvestitureHolder investiture) &&
                !investiture.isFull) {
                investiture.AbsorbInvestitureFrom(highstorm, investitureToAbsorb);
            }
        }
    }

    /// <summary>
    ///     Moves items from right to left.
    /// </summary>
    /// TODO: MAJOR overhaul idea: Have a component that keeps track of how much shelter a cell has from any given direction
    private bool MoveItem(Verse.Thing thing) {
        if (!Mod.enableHighstormPushing) return false;
        if (thing.IsBehindSolidThing(IntVec3.East, 2)) return false;
        if (thing is Mineable or Plant) return false;
        if (!thing.CanBeMoved() && !thing.def.useHitPoints) return false;

        IntVec3 oldPos = thing.Position;
        IntVec3 newPos = oldPos + GetRandomStormOffset(thing is Pawn);
        Map map = thing.Map;


        if (!CanMoveToNewPosition(thing, newPos)) {
            if (!newPos.InBounds(map)) return false;

            foreach (Verse.Thing collidedThing in newPos.GetThingList(thing.Map).ToList()) {
                DamageItem(collidedThing);
            }

            DamageItem(thing);
            return false;
        }

        if (thing.def.destroyOnDrop) {
            DamageItem(thing);
        }

        if (newPos.DistanceToEdge(map) == 0) {
            if (thing is not Pawn) {
                thing.DeSpawn();
                return true;
            }
        }

        if (!newPos.InBounds(map)) return true;

        if (thing is not Pawn) {
            if (thing.Spawned) {
                thing.DeSpawn();
            }

            if (!GenPlace.TryPlaceThing(thing, newPos, map, ThingPlaceMode.Near, out Verse.Thing newThing)) {
                GenSpawn.Spawn(thing, oldPos, map);
                return false;
            }

            foreach (Verse.Thing t in newPos.GetThingList(map).ToList()) {
                if (t == newThing) continue;
                if (newThing.CanStackWith(t) && t.TryAbsorbStack(newThing, true)) return true;
                if (t.def.saveCompressible && newThing.def.saveCompressible) {
                    if (MoveItem(t)) return true;

                    newThing.DeSpawn();
                    GenSpawn.Spawn(newThing, oldPos, map);
                    return false;
                }
            }
        }
        else {
            Pawn pushed = (Pawn)thing;
            pushed.Position = newPos;
            pushed.Notify_Teleported(false);
            pushed.stances?.stunner?.StunFor(60, highstorm, false);
        }

        FleckMaker.ThrowDustPuff(newPos, map, 1.5f);

        return true;
    }

    private bool CanMoveToNewPosition(Verse.Thing thing, IntVec3 newPos) {
        if (!thing.CanBeMoved()) return false;
        if (!newPos.InBounds(thing.Map)) return true;

        List<Verse.Thing> thingsAtPos = newPos.GetThingList(thing.Map);
        for (int i = 0; i < thingsAtPos.Count; i++) {
            if (thingsAtPos[i].IsSolid()) return false;
        }

        return true;
    }

    private static float GetShieldingMultiplier() {
        if (!shieldingResearchLookedUp) {
            shieldingResearchCache =
                DefDatabase<ResearchProjectDef>.GetNamedSilentFail("Cosmere_Roshar_HighstormShielding");
            shieldingResearchLookedUp = true;
        }

        if (shieldingResearchCache != null && shieldingResearchCache.IsFinished) return 0.15f;
        return 1f;
    }

    private static bool IsHighstormImmuneBuilding(Building building) {
        if (building.TryGetComp<StormlightReceiver>() != null) return true;
        if (building.def == RimWorld.ThingDefOf.HiddenConduit) return true;
        string defName = building.def.defName;
        if (defName == "Cosmere_Roshar_Thing_StormlightConduitHidden") return true;
        if (defName == "Cosmere_Roshar_Thing_StormlightConduitWeatherproof") return true;
        return false;
    }

    private static float GetBuildingDamageMultiplier(Building building) {
        ThingDef? stuff = building.Stuff;
        if (stuff == null) return 0.3f;

        string stuffName = stuff.defName;
        if (stuffName == "Plasteel") return 0f;
        if (stuffName.StartsWith("Vac")) return 0f;
        if (stuffName == "Silver" || stuffName == "Gold") return 0.40f;
        if (stuffName == "Steel") return 0.08f;
        if (stuffName == "Uranium") return 0.40f;

        List<StuffCategoryDef>? categories = stuff.stuffProps?.categories;
        if (categories == null) return 0.3f;

        for (int i = 0; i < categories.Count; i++) {
            StuffCategoryDef cat = categories[i];
            if (cat == StuffCategoryDefOf.Woody) return 1f;
            if (cat == StuffCategoryDefOf.Stony) return 0.04f;
            if (cat == Cosmere.Core.StuffCategoryDefOf.Cosmere_Core_StuffCategory_Gems) return 0.20f;
            if (cat == Cosmere.Core.StuffCategoryDefOf.Cosmere_Core_StuffCategory_RawGems) return 0.20f;
            if (cat == Cosmere.Core.StuffCategoryDefOf.Cosmere_Core_StuffCategory_CutGems) return 0.20f;
        }

        return 0.3f;
    }

    private void DamageItem(Verse.Thing thing) {
        Map? map = thing.Map;
        if (map != null && thing.Position.Fogged(map)) return;
        if (thing is Mineable) return;
        DamageInfo damage = new DamageInfo(
            DamageDefOf.TornadoScratch,
            Rand.Range(3f, 10f) * CurrentIntensity,
            instigator: highstorm,
            spawnFilth: false
        );
        switch (thing) {
            case Building building: {
                    if (map == null) break;
                    if (IsHighstormImmuneBuilding(building)) break;
                    if (StormShelterManager.IsProtectedByShelter(building.Position, map)) break;
                    if (building.Position.Roofed(map)) break;

                    float materialMultiplier = GetBuildingDamageMultiplier(building);
                    if (materialMultiplier <= 0f) break;

                    float totalMultiplier = materialMultiplier * GetShieldingMultiplier();
                    damage.SetAmount(damage.Amount * totalMultiplier);
                    building.TakeDamage(damage);
                    if (building.Destroyed) {
                        StormShelterManager.RebuildShelterCache(map);
                    }

                    break;
                }
            case Pawn pawn: {
                    if (pawn.Dead) break;
                    if (StormlightUtility.IsHighstormImmune(pawn)) break;
                    Surgebinder? surgebinder = pawn.genes?.GetFirstGeneOfType<Surgebinder>();
                    if (surgebinder != null) {
                        damage.SetAmount(damage.Amount * 0.5f / (surgebinder.CurrentIdeal + 1));
                    }

                    pawn.TakeDamage(damage);

                    break;
                }
        }
    }
}