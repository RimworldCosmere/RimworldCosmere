using Cosmere.Roshar.Debug;
using Cosmere.Roshar.LesserSpren.SprenControllers;
using UnityEngine;
using Verse;
using Logger = Cosmere.Foundation.Logger;

namespace Cosmere.Roshar.LesserSpren.ParticleSystem;

public class SprenParticleSystem {
    private const float UPDATE_INTERVAL = 5f; // Update dynamic spren every 5 seconds
    private const int ACTIVE_CELLS_REFRESH_INTERVAL = 1800; // Refresh active cells every 1800 ticks (30 seconds)
    private const int PARTICLE_EMISSION_INTERVAL = 300; // Re-emit particles every 300 ticks (5 seconds)

    private readonly BaseSprenController controller;

    public SprenParticleSystem(SprenType sprenType, int mapID) {
        SprenType = sprenType;
        controller = SprenControllerRegistry.GetController(sprenType)!;
        IsStatic = sprenType.IsNatureSpren();
        ValidCells = [];
        SpawnAreaMesh = null; // Will be created on main thread
        ParticleSystem = null; // Will be created on main thread
        MapID = mapID;
    }

    private int MapID { get; }

    public SprenType SprenType { get; }
    public UnityEngine.ParticleSystem ParticleSystem { get; private set; }
    public List<IntVec3> ValidCells { get; private set; }
    public List<IntVec3> ActiveSpawnCells { get; } = new List<IntVec3>();
    public Mesh SpawnAreaMesh { get; private set; }
    public bool IsStatic { get; }
    public float LastUpdateTime { get; set; }
    public int LastActiveCellsRefreshTick { get; set; }
    public int LastParticleEmissionTick { get; set; }

    public void InitializeOnMainThread() {
        if (SpawnAreaMesh == null) {
            SpawnAreaMesh = new Mesh();
            SpawnAreaMesh.name = $"SprenMesh_{SprenType}";
            SpawnAreaMesh.MarkDynamic(); // Mark as frequently updated
        }

        if (ParticleSystem == null) {
            ParticleSystem = CreateParticleSystem(SprenType, MapID);
        }
    }

    private UnityEngine.ParticleSystem CreateParticleSystem(SprenType type, int mapID) {
        UnityEngine.ParticleSystem particleSystem = Builder.CreateLesserSprenParticleSystem(mapID);

        // Ensure particle system is at world origin
        particleSystem.transform.position = Vector3.zero;

        // Customize particle system based on spren type
        ConfigureForSprenType(particleSystem, type);

        // Ensure it's in world space
        UnityEngine.ParticleSystem.MainModule main = particleSystem.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        return particleSystem;
    }

    private void ConfigureForSprenType(UnityEngine.ParticleSystem ps, SprenType type) {
        UnityEngine.ParticleSystem.MainModule main = ps.main;
        UnityEngine.ParticleSystem.ColorOverLifetimeModule colorModule = ps.colorOverLifetime;

        // Set colors based on spren type
        Color baseColor = GetSprenColor(type);
        main.startColor = baseColor;

        // Configure size based on type
        float sizeMultiplier = GetSprenSizeMultiplier(type);
        main.startSize = main.startSize.constant * sizeMultiplier;

        // Configure emission rate based on type
        UnityEngine.ParticleSystem.EmissionModule emission = ps.emission;
        float emissionMultiplier = GetEmissionRateMultiplier(type);
        emission.rateOverTime = emission.rateOverTime.constant * emissionMultiplier;
    }

    private Color GetSprenColor(SprenType type) {
        return type switch {
            SprenType.Wavespren => new Color(0.3f, 0.5f, 0.8f, 0.7f), // Blue
            SprenType.Riverspren => new Color(0.2f, 0.4f, 0.9f, 0.7f), // Deeper blue
            SprenType.Rockspren => new Color(0.5f, 0.4f, 0.3f, 0.8f), // Brown
            SprenType.Sandspren => new Color(0.9f, 0.8f, 0.5f, 0.6f), // Sandy yellow
            SprenType.Grassspren => new Color(0.3f, 0.7f, 0.3f, 0.7f), // Green
            SprenType.Windspren => new Color(0.9f, 0.9f, 1f, 0.4f), // White/transparent
            SprenType.Flamespren => new Color(1f, 0.4f, 0.1f, 0.9f), // Orange-red
            SprenType.Joyspren => new Color(1f, 0.9f, 0.3f, 0.8f), // Golden yellow
            SprenType.Fearspren => new Color(0.2f, 0.1f, 0.3f, 0.8f), // Dark purple
            SprenType.Angerspren => new Color(0.9f, 0.1f, 0.1f, 0.9f), // Red
            SprenType.Deathspren => new Color(0.1f, 0.1f, 0.1f, 0.9f), // Black
            SprenType.Lifespren => new Color(0.1f, 0.9f, 0.1f, 0.8f), // Bright green
            SprenType.Decayspren => new Color(0.4f, 0.3f, 0.2f, 0.7f), // Dark brown
            _ => new Color(0.7f, 0.7f, 0.7f, 0.7f), // Default gray
        };
    }

    private float GetSprenSizeMultiplier(SprenType type) {
        return type switch {
            SprenType.Rockspren => 0.4f, // Small rocky particles (reduced by half)
            SprenType.Windspren => 0.5f, // Reduced by half
            SprenType.Deathspren => 0.3f, // Reduced by half
            SprenType.Lifespren => 0.45f, // Reduced by half
            SprenType.Flamespren => 0.35f, // Reduced by half
            _ => 0.4f, // Reduced by half
        };
    }

    private float GetEmissionRateMultiplier(SprenType type) {
        return type switch {
            SprenType.Windspren => 0.08f, // Reduced by 90%
            SprenType.Flamespren => 0.12f, // Reduced by 90%
            SprenType.Grassspren => 0.06f, // Reduced by 90%
            SprenType.Deathspren => 0.03f, // Reduced by 90%
            _ => 0.04f, // Reduced by 90%
        };
    }

    public void UpdateValidCells(List<IntVec3> newCells) {
        ValidCells = newCells;
    }

    public void UpdateMesh(MeshManager meshManager) {
        if (ParticleSystem == null || SpawnAreaMesh == null) {
            Logger.Error(
                $"[Spren] Cannot update mesh - ParticleSystem: {ParticleSystem != null}, Mesh: {SpawnAreaMesh != null}"
            );
            return;
        }

        if (ValidCells.Count == 0) {
            ParticleSystem.Stop();
            return;
        }

        // Initialize active spawn cells if this is the first time
        if (ActiveSpawnCells.Count == 0) {
            DetermineActiveSpawnCells();
            LastActiveCellsRefreshTick = Find.TickManager?.TicksGame ?? 0;
        }

        // Disable mesh-based emission since we're doing manual positioning
        UnityEngine.ParticleSystem.ShapeModule shapeModule = ParticleSystem.shape;
        shapeModule.enabled = false;

        UpdateParticleCount();

        // Emit random particles for each active spawn cell
        EmitParticlesForActiveCells();
        LastParticleEmissionTick = Find.TickManager?.TicksGame ?? 0;
    }

    private void DetermineActiveSpawnCells() {
        int previousCount = ActiveSpawnCells.Count;
        ActiveSpawnCells.Clear();

        float spawnChance = controller?.cellSpawnChance ?? 0.05f;

        foreach (IntVec3 cell in ValidCells) {
            if (Random.value < spawnChance) {
                ActiveSpawnCells.Add(cell);
            }
        }

        // Always log the result, especially when no active cells are selected
        Logger.Verbose(
            $"[Spren] {SprenType}: {ActiveSpawnCells.Count}/{ValidCells.Count} active cells (spawn chance: {spawnChance:P1})"
        );

        // Update debug overlay with active spawn cells
        SprenDebugOverlay.UpdateActiveSpawnCells(SprenType, ActiveSpawnCells);
    }

    private void UpdateParticleCount() {
        UnityEngine.ParticleSystem.MainModule main = ParticleSystem.main;
        UnityEngine.ParticleSystem.EmissionModule emission = ParticleSystem.emission;

        // Calculate expected particle count based on active spawn cells and controller settings
        int maxParticlesPerCell = controller?.maxParticlesPerCell ?? 7;
        int expectedParticles = ActiveSpawnCells.Count * maxParticlesPerCell; // Max possible particles
        main.maxParticles = Mathf.Max(10, expectedParticles); // Minimum of 10 particles

        // Disable automatic emission since we're doing manual emission
        emission.rateOverTime = 0f; // No automatic emission
    }

    private void EmitParticlesForActiveCells() {
        if (ActiveSpawnCells.Count == 0) return;

        int totalParticlesToEmit = 0;
        List<UnityEngine.ParticleSystem.EmitParams> emitParamsList = new List<UnityEngine.ParticleSystem.EmitParams>();

        // Get particle count range from controller
        int minParticles = controller?.minParticlesPerCell ?? 2;
        int maxParticles = controller?.maxParticlesPerCell ?? 7;

        // Create specific emit parameters for each cell
        foreach (IntVec3 cell in ActiveSpawnCells) {
            int particlesForThisCell = Random.Range(minParticles, maxParticles + 1);
            totalParticlesToEmit += particlesForThisCell;

            // Emit multiple particles at this specific cell location
            for (int i = 0; i < particlesForThisCell; i++) {
                UnityEngine.ParticleSystem.EmitParams emitParams = new UnityEngine.ParticleSystem.EmitParams();

                // Set position to cell center with small random offset
                Vector3 cellCenter = cell.ToVector3Shifted();
                Vector3 randomOffset = new Vector3(
                    Random.Range(-0.4f, 0.4f),
                    0f,
                    Random.Range(-0.4f, 0.4f)
                );
                emitParams.position = cellCenter + randomOffset;

                // Set random velocity
                emitParams.velocity = new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(0.1f, 0.3f),
                    Random.Range(-0.5f, 0.5f)
                );

                emitParamsList.Add(emitParams);
            }
        }

        // Emit all particles with their specific parameters
        foreach (UnityEngine.ParticleSystem.EmitParams emitParams in emitParamsList) {
            ParticleSystem.Emit(emitParams, 1);
        }

        /*Logger.Verbose(
            $"[Spren] Emitted {totalParticlesToEmit} particles for {ActiveSpawnCells.Count} active cells (avg {(float)totalParticlesToEmit / ActiveSpawnCells.Count:F1} per cell)"
        );*/
    }

    public bool ShouldUpdateDynamic() {
        return !IsStatic && Time.time - LastUpdateTime > UPDATE_INTERVAL;
    }

    public bool ShouldReEmitParticles() {
        return ActiveSpawnCells.Count > 0 &&
               Find.TickManager?.TicksGame - LastParticleEmissionTick >= PARTICLE_EMISSION_INTERVAL;
    }

    public void ReEmitParticles() {
        if (ParticleSystem != null && ActiveSpawnCells.Count > 0) {
            EmitParticlesForActiveCells();
            LastParticleEmissionTick = Find.TickManager?.TicksGame ?? 0;
        }
    }

    public bool ShouldRefreshActiveCells() {
        return ValidCells.Count > 0 &&
               Find.TickManager?.TicksGame - LastActiveCellsRefreshTick >= ACTIVE_CELLS_REFRESH_INTERVAL;
    }

    public void RefreshActiveCells() {
        if (ValidCells.Count > 0) {
            DetermineActiveSpawnCells();
            LastActiveCellsRefreshTick = Find.TickManager?.TicksGame ?? 0;
        }
    }

    public void Destroy() {
        StateHandler.DestroyParticleSystem(ParticleSystem);
        if (SpawnAreaMesh != null) {
            Object.Destroy(SpawnAreaMesh);
        }
    }
}