using System;
using Cosmere.Core.Def;
using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.LesserSpren.SprenController;

[StaticConstructorOnStartup]
public abstract class BaseSprenController {
    protected static readonly Texture2D DefaultTexture = ContentFinder<Texture2D>.Get(
        "Things/Pawn/Animal/LesserSpren",
        false
    );

    protected static readonly Material DefaultMaterial = new Material(Verse.ShaderDatabase.TransparentPostLight);

    protected readonly HashSet<SprenSpawnInformation> activeSpawnInfoInt =
        new HashSet<SprenSpawnInformation>(SprenSpawnInfoComparer.Instance);

    protected readonly HashSet<SprenSpawnInformation> validSpawnInfoInt =
        new HashSet<SprenSpawnInformation>(SprenSpawnInfoComparer.Instance);

    private Material? configuredMaterial;
    private bool infoInitialized;
    private int nextActiveInfoRefresh;
    private int nextValidInfoRefresh;

    public virtual Texture2D SprenTexture => DefaultTexture;

    public virtual Material SprenMaterial => GetConfiguredMaterial();

    public abstract SprenType SprenType { get; }

    public abstract bool IsEnabled { get; }

    public virtual IReadOnlyCollection<SprenSpawnInformation> ValidSpawnInfo => validSpawnInfoInt;

    public virtual IReadOnlyCollection<SprenSpawnInformation> ActiveSpawnInfo => activeSpawnInfoInt;

    public abstract IntRange ValidInfoRefreshInterval { get; }

    public abstract IntRange ActiveInfoRefreshInterval { get; }

    public virtual int MinParticlesPerCell => 1;

    public virtual int MaxParticlesPerCell => 2;

    public virtual FloatRange Lifetime => new FloatRange(4f, 10f);

    public virtual float CellSpawnChance => 0.05f; // 5% default chance per valid cell to spawn spren

    protected virtual float MaxSpreadDistance => 0.5f;

    protected virtual float MovementSpeed => 5f;

    protected virtual float RandomDirectionAmount => 0.3f;

    protected internal SprenSpawnInformation DefaultSpawnInformation => new SprenSpawnInformation(
        null,
        null,
        CellSpawnChance,
        MinParticlesPerCell,
        MaxParticlesPerCell,
        MaxSpreadDistance,
        MovementSpeed,
        RandomDirectionAmount
    );

    public virtual List<GemDef> CompatibleGemTypes => [];

    public virtual float CaptureRarityMultiplier => 0.7f;

    public virtual bool CanBeCaptured => CompatibleGemTypes.Count > 0;

    public abstract Color SprenColor { get; }

    public virtual float SprenSizeMultiplier => 1f;

    public virtual float EmissionRateMultiplier => 1f;

    public virtual float SprenSpeedMultiplier => 1f;

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

    protected virtual void ConfigureMaterial(Material material) { }

    public virtual void InitializeInfo(Map map) {
        if (infoInitialized) return;

        RefreshValidInfo(map);
        RefreshActiveInfo(map);

        int currentTick = Find.TickManager?.TicksGame ?? 0;
        nextValidInfoRefresh = currentTick + ValidInfoRefreshInterval.RandomInRange;
        nextActiveInfoRefresh = currentTick + ActiveInfoRefreshInterval.RandomInRange;

        infoInitialized = true;
    }

    public virtual bool UpdateInfo(Map map) {
        if (!infoInitialized) return false;

        int currentTick = Find.TickManager?.TicksGame ?? 0;

        if (currentTick >= nextValidInfoRefresh) {
            RefreshValidInfo(map);
            nextValidInfoRefresh = currentTick + ValidInfoRefreshInterval.RandomInRange;
        }

        if (currentTick < nextActiveInfoRefresh) return false;

        RefreshActiveInfo(map);
        nextActiveInfoRefresh = currentTick + ActiveInfoRefreshInterval.RandomInRange;

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
            if (Rand.Chance(CellSpawnChance)) {
                activeSpawnInfoInt.Add(info);
            }
        }
    }

    public virtual string DebugStringAt(IntVec3 position) {
        return string.Empty;
    }

    public abstract SprenSpawnInformation? GetSprenSpawnInformation(IntVec3 position, Map? map);

    public virtual List<SprenSpawnInformation> GetDynamicSpawnInfo(Map? map) {
        return [];
    }

    protected static bool HasTerrainTag(TerrainDef terrain, string tag) {
        return terrain.HasTag(tag);
    }

    protected static bool TerrainNameContains(TerrainDef terrain, params string[] keywords) {
        string terrainName = terrain.defName;
        return keywords.Any(keyword => terrainName.Contains(keyword, StringComparison.OrdinalIgnoreCase));
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
        return $"SprenName_{SprenType}".Translate();
    }
}
