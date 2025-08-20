using Cosmere.Resources.Def;
using Cosmere.Roshar.LesserSpren.ParticleSystem;
using UnityEngine;
using Verse;

namespace Cosmere.Roshar.LesserSpren.SprenControllers;

[StaticConstructorOnStartup]
public abstract class BaseSprenController {
    protected static readonly Texture2D DefaultTexture = ContentFinder<Texture2D>.Get("Things/Pawn/Animal/LesserSpren");
    protected static readonly Material DefaultMaterial = new Material(Verse.ShaderDatabase.TransparentPostLight);

    protected readonly HashSet<SprenSpawnInformation> activeSpawnInfoInt =
        new HashSet<SprenSpawnInformation>(SprenSpawnInfoComparer.Instance);

    protected readonly HashSet<SprenSpawnInformation> validSpawnInfoInt =
        new HashSet<SprenSpawnInformation>(SprenSpawnInfoComparer.Instance);

    private Material? configuredMaterial;
    private bool infoInitialized;
    private int nextActiveInfoRefresh;
    private int nextValidInfoRefresh;

    public virtual Texture2D sprenTexture => DefaultTexture;
    public virtual Material sprenMaterial => GetConfiguredMaterial();
    public abstract SprenType sprenType { get; }
    public abstract bool isEnabled { get; }
    public virtual IReadOnlyCollection<SprenSpawnInformation> validSpawnInfo => validSpawnInfoInt;
    public virtual IReadOnlyCollection<SprenSpawnInformation> activeSpawnInfo => activeSpawnInfoInt;

    public abstract IntRange validInfoRefreshInterval { get; }
    public abstract IntRange activeInfoRefreshInterval { get; }

    public virtual int minParticlesPerCell => 2;
    public virtual int maxParticlesPerCell => 4;
    public virtual FloatRange lifetime => new FloatRange(4f, 10f);

    public virtual float cellSpawnChance => 0.05f; // 5% default chance per valid cell to spawn spren

    protected virtual float maxSpreadDistance => 0.5f;
    protected virtual float movementSpeed => 5f;
    protected virtual float randomDirectionAmount => 0.3f;

    // Default spawn information that uses the virtual properties
    protected internal SprenSpawnInformation defaultSpawnInformation => new SprenSpawnInformation(
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

    private Material GetConfiguredMaterial() {
        if (configuredMaterial == null) {
            configuredMaterial = new Material(GetBaseMaterial());
            ConfigureMaterial(configuredMaterial);
        }

        return configuredMaterial;
    }

    protected virtual Material GetBaseMaterial() {
        return DefaultMaterial;
    }

    protected virtual void ConfigureMaterial(Material material) {
        // Base implementation - override in derived classes for custom behavior
    }

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

    public virtual bool UpdateInfo(Map map) {
        if (!infoInitialized) return false;

        int currentTick = Find.TickManager?.TicksGame ?? 0;

        // Check if it's time to refresh valid cells
        if (currentTick >= nextValidInfoRefresh) {
            RefreshValidInfo(map);
            nextValidInfoRefresh = currentTick + validInfoRefreshInterval.RandomInRange;
        }

        // Check if it's time to refresh active cells
        if (currentTick < nextActiveInfoRefresh) return false;

        RefreshActiveInfo(map);
        nextActiveInfoRefresh = currentTick + activeInfoRefreshInterval.RandomInRange;

        return true;
    }

    protected virtual void RefreshValidInfo(Map map) {
        validSpawnInfoInt.Clear();

        foreach (IntVec3 cell in map.AllCells) {
            SprenSpawnInformation? spawnInfo = GetSprenSpawnInformation(cell, map);
            if (spawnInfo != null) {
                validSpawnInfoInt.Add(spawnInfo);
            }
        }
    }

    protected virtual void RefreshActiveInfo(Map map) {
        activeSpawnInfoInt.RemoveWhere(info => GenTicks.TicksGame > info.cleanupTick);

        foreach (SprenSpawnInformation? info in validSpawnInfoInt) {
            if (Rand.Chance(cellSpawnChance)) {
                activeSpawnInfoInt.Add(info);
            }
        }
    }

    public virtual string DebugStringAt(IntVec3 position) {
        return "";
    }

    public abstract SprenSpawnInformation? GetSprenSpawnInformation(IntVec3 position, Map? map);

    public virtual List<SprenSpawnInformation> GetDynamicSpawnInfo(Map? map) {
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

    protected static bool IsInBounds(IntVec3 position, Map? map) {
        return position != IntVec3.Invalid && map != null && position.InBounds(map);
    }

    public virtual void ResetForNewMap() {
        validSpawnInfoInt.Clear();
        activeSpawnInfoInt.Clear();
        infoInitialized = false;
        nextValidInfoRefresh = 0;
        nextActiveInfoRefresh = 0;
    }

    public bool RemoveActiveSpawnInfo(SprenSpawnInformation info) {
        return activeSpawnInfoInt.Remove(info);
    }

    public bool RemoveActiveSpawnInfoAt(IntVec3 position, Map map) {
        return activeSpawnInfoInt.RemoveWhere(info => info.position == position && info.map == map) > 0;
    }

    public string GetLocalizedName() {
        return $"SprenName_{sprenType}".Translate();
    }
}