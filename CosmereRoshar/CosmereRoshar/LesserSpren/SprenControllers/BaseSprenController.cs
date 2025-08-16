using Cosmere.Resources.Def;
using Cosmere.Roshar.LesserSpren.ParticleSystem;
using UnityEngine;
using Verse;

namespace Cosmere.Roshar.LesserSpren.SprenControllers;

public record SprenSpawnInformation(IntVec3? position, float spawnChance, int minParticles, int maxParticles) {
    public int maxParticles = maxParticles;
    public int minParticles = minParticles;
    public IntVec3? position = position;
    public float spawnChance = spawnChance;

    public SprenSpawnInformation With(
        IntVec3? position = null,
        float? spawnChance = null,
        int? minParticles = null,
        int? maxParticles = null
    ) {
        return this with {
            position = position ?? this.position,
            spawnChance = spawnChance ?? this.spawnChance,
            minParticles = minParticles ?? this.minParticles,
            maxParticles = maxParticles ?? this.maxParticles,
        };
    }
}

public abstract class BaseSprenController {
    // Cell management - each controller manages its own spawn cells
    protected readonly List<IntVec3> activeSpawnCellsInt = [];
    protected readonly List<IntVec3> validSpawnCellsInt = [];
    protected bool cellsInitialized;
    protected int nextActiveCellsRefresh;

    // Timing for refresh intervals
    protected int nextValidCellsRefresh;
    public abstract SprenType sprenType { get; }
    public abstract bool isEnabled { get; }
    public abstract bool isNatureSpren { get; }
    public abstract List<IntVec3> validSpawnCells { get; }
    public abstract List<IntVec3> activeSpawnCells { get; }

    // Refresh intervals for cell recalculation - different for static vs dynamic spren
    public virtual IntRange validCellsRefreshInterval => isNatureSpren
        ? new IntRange(
            GenTicks.SecondsToTicks(600),
            GenTicks.SecondsToTicks(3600)
        ) // Static: 10-60 minutes (600-3600 seconds)
        : new IntRange(
            GenTicks.SecondsToTicks(120),
            GenTicks.SecondsToTicks(240)
        ); // Dynamic: 1-2 minutes (60-120 seconds)

    public virtual IntRange activeCellsRefreshInterval => isNatureSpren
        ? new IntRange(
            GenTicks.SecondsToTicks(240),
            GenTicks.SecondsToTicks(480)
        ) // Static: 2-4 minutes (120-240 seconds) 
        : new IntRange(GenTicks.SecondsToTicks(10), GenTicks.SecondsToTicks(20)); // Dynamic: 10-20 seconds

    // Particle count configuration
    public virtual int minParticlesPerCell => 2;
    public virtual int maxParticlesPerCell => 4;

    // Spawn chance configuration
    public virtual float cellSpawnChance => 0.05f; // 5% default chance per valid cell to spawn spren

    // Default spawn information that uses the virtual properties
    public SprenSpawnInformation defaultSpawnInformation => new SprenSpawnInformation(
        null,
        cellSpawnChance,
        minParticlesPerCell,
        maxParticlesPerCell
    );

    // Capture configuration
    public virtual List<GemDef> compatibleGemTypes => [];
    public virtual float captureRarityMultiplier => 0.7f;
    public virtual bool canBeCaptured => compatibleGemTypes.Count > 0;

    // Visual configuration
    public abstract Color sprenColor { get; }
    public virtual float sprenSizeMultiplier => 1f;
    public virtual float emissionRateMultiplier => 1f;
    public virtual float sprenSpeedMultiplier => 1f;

    // Initialize/update cell collections for this controller - generic implementation
    public virtual void InitializeCells(Map map) {
        if (cellsInitialized) return;

        RefreshValidCells(map);
        RefreshActiveCells(map);

        // Schedule initial refresh times
        int currentTick = Find.TickManager?.TicksGame ?? 0;
        nextValidCellsRefresh = currentTick + validCellsRefreshInterval.RandomInRange;
        nextActiveCellsRefresh = currentTick + activeCellsRefreshInterval.RandomInRange;

        cellsInitialized = true;
    }

    // Generic update method that handles timing - can be overridden if needed
    public virtual void UpdateCells(Map map) {
        if (!cellsInitialized) return;

        int currentTick = Find.TickManager?.TicksGame ?? 0;

        // Check if it's time to refresh valid cells
        if (currentTick >= nextValidCellsRefresh) {
            RefreshValidCells(map);
            nextValidCellsRefresh = currentTick + validCellsRefreshInterval.RandomInRange;
        }

        // Check if it's time to refresh active cells
        if (currentTick >= nextActiveCellsRefresh) {
            RefreshActiveCells(map);
            nextActiveCellsRefresh = currentTick + activeCellsRefreshInterval.RandomInRange;
        }
    }

    // Abstract methods that controllers must implement for their specific logic
    protected abstract void RefreshValidCells(Map map);
    protected abstract void RefreshActiveCells(Map map);

    public abstract SprenSpawnInformation? GetSprenSpawnInformation(
        IntVec3 position,
        Map map,
        bool isDynamicCell = false
    );

    public virtual List<SprenSpawnInformation> GetDynamicCells(Map map) {
        return [];
    }

    protected static bool HasTerrainTag(TerrainDef terrain, string tag) {
        return terrain.HasTag(tag);
    }

    protected static bool TerrainNameContains(TerrainDef terrain, params string[] keywords) {
        string terrainName = terrain.defName.ToLower();
        return keywords.Any(keyword => terrainName.Contains(keyword.ToLower()));
    }

    protected static TerrainDef? GetTerrain(IntVec3 position, Map map) {
        return map.terrainGrid.TerrainAt(position);
    }

    protected static bool IsInBounds(IntVec3 position, Map map) {
        return position.InBounds(map);
    }
}