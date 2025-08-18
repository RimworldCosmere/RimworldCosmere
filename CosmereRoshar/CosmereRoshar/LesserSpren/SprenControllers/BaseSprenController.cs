using Cosmere.Resources.Def;
using Cosmere.Roshar.LesserSpren.ParticleSystem;
using UnityEngine;
using Verse;

namespace Cosmere.Roshar.LesserSpren.SprenControllers;

public abstract class BaseSprenController {
    protected readonly List<SprenSpawnInformation> activeSpawnInfoInt = [];
    protected readonly List<SprenSpawnInformation> validSpawnInfoInt = [];
    private bool infoInitialized;
    private int nextActiveInfoRefresh;

    private int nextValidInfoRefresh;
    public abstract SprenType sprenType { get; }
    public abstract bool isEnabled { get; }
    public abstract bool isNatureSpren { get; }
    public virtual List<SprenSpawnInformation> validSpawnInfo => validSpawnInfoInt;
    public virtual List<SprenSpawnInformation> activeSpawnInfo => activeSpawnInfoInt;

    public abstract IntRange validInfoRefreshInterval { get; }
    public abstract IntRange activeInfoRefreshInterval { get; }

    public virtual int minParticlesPerCell => 2;
    public virtual int maxParticlesPerCell => 4;

    public virtual float cellSpawnChance => 0.05f; // 5% default chance per valid cell to spawn spren

    protected virtual float maxSpreadDistance => 0.5f;
    protected virtual float movementSpeed => 5f;
    protected virtual float randomDirectionAmount => 0.3f;

    // Default spawn information that uses the virtual properties
    protected SprenSpawnInformation defaultSpawnInformation => new SprenSpawnInformation(
        null,
        null,
        cellSpawnChance,
        minParticlesPerCell,
        maxParticlesPerCell,
        maxSpreadDistance,
        movementSpeed,
        randomDirectionAmount
    );

    public virtual List<GemDef> compatibleGemTypes => [];
    public virtual float captureRarityMultiplier => 0.7f;
    public virtual bool canBeCaptured => compatibleGemTypes.Count > 0;

    public abstract Color sprenColor { get; }
    public virtual float sprenSizeMultiplier => 1f;
    public virtual float emissionRateMultiplier => 1f;
    public virtual float sprenSpeedMultiplier => 1f;

    public virtual void InitializeInfo(Map map) {
        if (infoInitialized) return;

        RefreshValidInfo(map);
        RefreshActiveInfo(map);

        // Schedule initial refresh times
        int currentTick = Find.TickManager?.TicksGame ?? 0;
        nextValidInfoRefresh = currentTick + validInfoRefreshInterval.RandomInRange;
        nextActiveInfoRefresh = currentTick + activeInfoRefreshInterval.RandomInRange;

        infoInitialized = true;
    }

    public virtual void UpdateInfo(Map map) {
        if (!infoInitialized) return;

        int currentTick = Find.TickManager?.TicksGame ?? 0;

        // Check if it's time to refresh valid cells
        if (currentTick >= nextValidInfoRefresh) {
            RefreshValidInfo(map);
            nextValidInfoRefresh = currentTick + validInfoRefreshInterval.RandomInRange;
        }

        // Check if it's time to refresh active cells
        if (currentTick >= nextActiveInfoRefresh) {
            RefreshActiveInfo(map);
            nextActiveInfoRefresh = currentTick + activeInfoRefreshInterval.RandomInRange;
        }
    }

    protected abstract void RefreshValidInfo(Map map);
    protected abstract void RefreshActiveInfo(Map map);

    public abstract SprenSpawnInformation? GetSprenSpawnInformation(
        IntVec3 position,
        Map? map,
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