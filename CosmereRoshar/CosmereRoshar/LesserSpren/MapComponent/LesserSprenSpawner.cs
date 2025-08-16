using Cosmere.Roshar.Debug;
using Cosmere.Roshar.LesserSpren.ParticleSystem;
using Cosmere.Roshar.LesserSpren.SprenControllers;
using Verse;
using Logger = Cosmere.Foundation.Logger;

namespace Cosmere.Roshar.LesserSpren.MapComponent;

public class LesserSprenSpawner : Verse.MapComponent {
    private const float ParticleAlpha = 2.5f;

    private const float
        DynamicUpdateInterval = GenTicks.TickRareInterval; // Update dynamic spren every 250 ticks (about 4 seconds)

    private readonly List<BaseSprenController> allControllers = [];

    private readonly int mapID;
    private readonly Dictionary<SprenType, MeshManager> meshManagers = new Dictionary<SprenType, MeshManager>();
    private readonly List<SprenType> pendingInitialization = [];

    private readonly Dictionary<SprenType, SprenParticleSystem> sprenSystems =
        new Dictionary<SprenType, SprenParticleSystem>();

    private bool initialized;
    private int lastDynamicUpdateTick;

    public LesserSprenSpawner(Map map) : base(map) {
        mapID = map.GetHashCode();
        LongEventHandler.ExecuteWhenFinished(InitializeMapSystems);
    }

    public override void MapComponentDraw() {
        SprenDebugOverlay.DrawOverlay();
    }

    public override void MapComponentTick() {
        base.MapComponentTick();

        // Always process pending initializations, even before fully initialized
        // This allows the background task to queue systems for main thread init

        // Initialize any pending spren systems on main thread
        if (pendingInitialization.Count > 0) {
            foreach (SprenType sprenType in pendingInitialization.ToList()) {
                if (sprenSystems.TryGetValue(sprenType, out SprenParticleSystem? system)) {
                    system.InitializeOnMainThread();

                    if (system.ParticleSystem != null) {
                        ColorManager.SetParticleAlpha(system.ParticleSystem, ParticleAlpha);
                        StateHandler.RestoreParticleSystemState(system.ParticleSystem);

                        // Update the mesh now that particle system exists
                        if (meshManagers.TryGetValue(sprenType, out MeshManager? meshManager)) {
                            system.UpdateMesh(meshManager);
                        }

                        // Update debug overlay with actual particle positions
                        SprenDebugOverlay.UpdateActualParticles(sprenType, system.ParticleSystem);

                        // Force the particle system to start playing
                        system.ParticleSystem.Play();
                    } else {
                        Logger.Error($"[Spren] Failed to initialize particle system for {sprenType}");
                    }
                }
            }

            pendingInitialization.Clear();
        }

        // Don't do anything else until initialized
        if (!initialized) return;

        // Update all spren controllers periodically
        if (Find.TickManager.TicksGame - lastDynamicUpdateTick >= DynamicUpdateInterval) {
            UpdateAllSprenControllers();
            lastDynamicUpdateTick = Find.TickManager.TicksGame;

            // Update debug overlay with current particle positions for all active systems
            if (SprenDebugOverlay.ShowOverlay && SprenDebugOverlay.ShowActualParticles) {
                foreach (KeyValuePair<SprenType, SprenParticleSystem> kvp in sprenSystems) {
                    if (kvp.Value.ParticleSystem != null) {
                        SprenDebugOverlay.UpdateActualParticles(kvp.Key, kvp.Value.ParticleSystem);
                    }
                }
            }
        }

        // Check for particle re-emission and active cell refresh for all systems (both static and dynamic)
        foreach (KeyValuePair<SprenType, SprenParticleSystem> kvp in sprenSystems) {
            // Check if we need to refresh active spawn cells (every 1800 ticks)
            if (kvp.Value.ShouldRefreshActiveCells()) {
                kvp.Value.RefreshActiveCells();
            }

            // Check if we need to re-emit particles (every 300 ticks)
            if (kvp.Value.ShouldReEmitParticles()) {
                kvp.Value.ReEmitParticles();
            }
        }
    }

    private void UpdateAllSprenControllers() {
        // Update ALL controllers (both static and dynamic need periodic refreshes)
        foreach (BaseSprenController controller in allControllers) {
            SprenType sprenType = controller.sprenType;

            // Update controller's cells (handles timing internally)
            controller.UpdateCells(map);

            if (controller.validSpawnCells.Count > 0) {
                // Create or update the spren system
                if (!sprenSystems.ContainsKey(sprenType)) {
                    CreateSprenSystem(sprenType, controller);
                } else {
                    // Update existing system with new cells
                    SprenParticleSystem? system = sprenSystems[sprenType];
                    system.UpdateValidCells(controller.validSpawnCells);

                    // Update debug overlay for dynamic spren
                    SprenDebugOverlay.UpdateValidCells(sprenType, controller.validSpawnCells);
                }

                if (!meshManagers.TryGetValue(sprenType, out MeshManager? value)) continue;
                SprenParticleSystem? sprenSystem = sprenSystems[sprenType];

                // Only update if particle system is initialized
                if (sprenSystem.ParticleSystem != null) {
                    sprenSystem.UpdateMesh(value);
                    StateHandler.SetParticleSystemState(sprenSystem.ParticleSystem, true);

                    // Update debug overlay with actual particle positions
                    SprenDebugOverlay.UpdateActualParticles(sprenType, sprenSystem.ParticleSystem);
                }
            } else if (sprenSystems.TryGetValue(sprenType, out SprenParticleSystem? system)) {
                if (system.ParticleSystem != null) {
                    StateHandler.SetParticleSystemState(system.ParticleSystem, false);

                    // Clear particle positions in debug overlay when no active spren
                    SprenDebugOverlay.UpdateActualParticles(sprenType, system.ParticleSystem);
                }
            }
        }
    }

    private void CreateSprenSystem(SprenType sprenType, BaseSprenController controller) {
        SprenParticleSystem system = new SprenParticleSystem(sprenType, mapID);
        sprenSystems[sprenType] = system;

        // Update system with controller's cells
        system.UpdateValidCells(controller.validSpawnCells);

        MeshManager meshManager = new MeshManager(
            map,
            pos => controller.validSpawnCells.Contains(pos.ToIntVec3())
        );
        meshManagers[sprenType] = meshManager;

        // Update debug overlay
        SprenDebugOverlay.UpdateValidCells(sprenType, controller.validSpawnCells);

        // Mark for initialization on main thread
        pendingInitialization.Add(sprenType);
    }


    public override void MapRemoved() {
        base.MapRemoved();

        // Clean up all spren systems
        foreach (SprenParticleSystem? system in sprenSystems.Values) {
            system.Destroy();
        }

        sprenSystems.Clear();
        meshManagers.Clear();
    }

    private void InitializeMapSystems() {
        if (initialized) return;

        // Get all enabled controllers
        allControllers.AddRange(SprenControllerRegistry.GetEnabledControllers());
        Logger.Info($"[Spren] Initializing {allControllers.Count} spren controllers");

        // Initialize cells for all controllers
        foreach (BaseSprenController controller in allControllers) {
            Logger.Info($"[Spren] Initializing cells for {controller.sprenType}");
            controller.InitializeCells(map);

            // Create spren system if controller has valid cells
            if (controller.validSpawnCells.Count > 0) {
                CreateSprenSystem(controller.sprenType, controller);
            }
        }

        initialized = true;
        Logger.Info("[Spren] Map systems initialized");
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref initialized, "initialized");
        Scribe_Values.Look(ref lastDynamicUpdateTick, "lastDynamicUpdateTick");

        // We'll recreate particle systems on load rather than trying to save them
        if (Scribe.mode != LoadSaveMode.PostLoadInit) return;
        if (initialized) {
            InitializeMapSystems();
        }
    }
}