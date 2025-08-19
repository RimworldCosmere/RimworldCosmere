using Cosmere.Roshar.LesserSpren.SprenControllers;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Roshar.LesserSpren.ParticleSystem;

public class CellValidator(Map map) {
    private const int CacheRefreshInterval = 7200; // Refresh cache every 7200 ticks (2 minutes)

    private readonly Dictionary<SprenType, List<IntVec3>>
        cachedStaticCells = new Dictionary<SprenType, List<IntVec3>>();

    private readonly Map map = map;
    private int lastCacheRefreshTick;
    private bool staticCellsCached;

    public bool IsCellValidForSprenType(IntVec3 position, SprenType sprenType) {
        BaseSprenController controller = SprenControllerRegistry.GetController(sprenType)!;
        return controller.GetSprenSpawnInformation(position, map) != null;
    }

    // Terrain checking methods moved to individual spren controller classes

    public List<IntVec3> GetCellsForSprenType(SprenType sprenType) {
        BaseSprenController controller = SprenControllerRegistry.GetController(sprenType)!;

        // Check if cache needs refreshing for nature spren
        if (controller.isNatureSpren && ShouldRefreshCache()) {
            RefreshStaticSprenCache();
        }

        if (controller.isNatureSpren && staticCellsCached && cachedStaticCells.ContainsKey(sprenType)) {
            return cachedStaticCells[sprenType];
        }

        List<IntVec3> validCells = [];
        validCells.AddRange(map.AllCells.Where(cell => controller.GetSprenSpawnInformation(cell, map) != null));

        if (controller.isNatureSpren) {
            cachedStaticCells[sprenType] = validCells;
        }

        return validCells;
    }

    public void CacheStaticSprenCells() {
        if (staticCellsCached) return;

        foreach (BaseSprenController controller in SprenControllerRegistry.enabledStaticControllers) {
            GetCellsForSprenType(controller.sprenType);
        }

        staticCellsCached = true;
        lastCacheRefreshTick = Find.TickManager?.TicksGame ?? 0;
    }

    private bool ShouldRefreshCache() {
        return staticCellsCached &&
               Find.TickManager?.TicksGame - lastCacheRefreshTick >= CacheRefreshInterval;
    }

    private void RefreshStaticSprenCache() {
        // Clear existing cache
        cachedStaticCells.Clear();
        staticCellsCached = false;

        // Rebuild cache
        CacheStaticSprenCells();
    }

    public List<IntVec3> GetDynamicSprenCells(SprenType sprenType) {
        List<IntVec3> validCells = [];

        switch (sprenType) {
            case SprenType.Flamespren:
                validCells.AddRange(GetFireCells());
                break;
            case SprenType.Deathspren:
                validCells.AddRange(GetDeathCells());
                break;
            case SprenType.Decayspren:
                validCells.AddRange(GetDecayCells());
                break;
            case SprenType.Lifespren:
                validCells.AddRange(GetBirthCells());
                break;
            case SprenType.Joyspren:
                validCells.AddRange(GetHappyPawnCells());
                break;
            case SprenType.Fearspren:
                validCells.AddRange(GetFearfulPawnCells());
                break;
            case SprenType.Angerspren:
                validCells.AddRange(GetAngryPawnCells());
                break;
        }

        return validCells;
    }

    private List<IntVec3> GetFireCells() {
        List<IntVec3> cells = [];
        foreach (Verse.Thing? fire in map.listerThings.ThingsOfDef(RimWorld.ThingDefOf.Fire)) {
            cells.Add(fire.Position);
            cells.AddRange(GenAdj.CellsAdjacent8Way(fire));
        }

        return cells.Distinct().ToList();
    }

    private List<IntVec3> GetDeathCells() {
        List<IntVec3> cells = [];
        foreach (Verse.Thing? corpse in map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse)
                     .Where(corpse => corpse.GetRotStage() == RotStage.Fresh)) {
            cells.Add(corpse.Position);
            cells.AddRange(GenRadial.RadialCellsAround(corpse.Position, 2f, true));
        }

        return cells.Distinct().ToList();
    }

    private List<IntVec3> GetDecayCells() {
        List<IntVec3> cells = [];
        foreach (Verse.Thing? corpse in map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse)
                     .Where(corpse => corpse.GetRotStage() >= RotStage.Rotting)) {
            cells.Add(corpse.Position);
            cells.AddRange(GenRadial.RadialCellsAround(corpse.Position, 3f, true));
        }

        return cells.Distinct().ToList();
    }

    private List<IntVec3> GetBirthCells() {
        List<IntVec3> cells = [];
        // Check for recently born pawns or eggs hatching
        foreach (Pawn? pawn in map.mapPawns.AllPawnsSpawned) {
            if (pawn.ageTracker.AgeBiologicalTicks >= GenDate.TicksPerDay) continue;
            cells.Add(pawn.Position);
            cells.AddRange(GenRadial.RadialCellsAround(pawn.Position, 2f, true));
        }

        return cells.Distinct().ToList();
    }

    private List<IntVec3> GetHappyPawnCells() {
        List<IntVec3> cells = [];
        foreach (Pawn? pawn in map.mapPawns.FreeColonistsSpawned) {
            if (pawn.needs?.mood?.CurLevel > 0.8f) {
                cells.Add(pawn.Position);
            }
        }

        return cells;
    }

    private List<IntVec3> GetFearfulPawnCells() {
        List<IntVec3> cells = [];
        cells.AddRange(
            from pawn in map.mapPawns.AllPawnsSpawned
            where pawn.InMentalState &&
                  (pawn.MentalStateDef == MentalStateDefOf.PanicFlee ||
                   pawn.MentalStateDef == MentalStateDefOf.Manhunter)
            select pawn.Position
        );

        return cells;
    }

    private List<IntVec3> GetAngryPawnCells() {
        List<IntVec3> cells = [];
        foreach (Pawn? pawn in map.mapPawns.AllPawnsSpawned) {
            if (pawn.InMentalState &&
                (pawn.MentalStateDef == MentalStateDefOf.Berserk ||
                 pawn.MentalStateDef == MentalStateDefOf.SocialFighting)) {
                cells.Add(pawn.Position);
            }
        }

        return cells;
    }

    /**
     * Legacy method for compatibility
     */
    public bool IsCellValidForParticleEmissionMesh(IntVec3 position) {
        return Rand.Chance(1 / 3f);
    }

    public bool IsCellValidForParticleEmissionMesh(Vector3 position) {
        return IsCellValidForParticleEmissionMesh(position.ToIntVec3());
    }
}