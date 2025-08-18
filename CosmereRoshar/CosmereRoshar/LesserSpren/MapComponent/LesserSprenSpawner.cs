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

        // Handle particle re-emission during draw so it works even when paused
        if (!initialized) return;
        foreach (KeyValuePair<SprenType, SprenParticleSystem> kvp in sprenSystems) {
            if (kvp.Value.ShouldReEmitParticles()) {
                kvp.Value.EmitParticlesForActiveCells();
            }
        }
    }

    public override void MapComponentTick() {
        base.MapComponentTick();

        // Always process pending initializations, even before fully initialized
        // This allows the background task to queue systems for main thread init

        // Initialize any pending spren systems on main thread
        if (pendingInitialization.Count > 0) {
            foreach (SprenType sprenType in pendingInitialization.ToList()) {
                if (!sprenSystems.TryGetValue(sprenType, out SprenParticleSystem? system)) continue;

                system.InitializeOnMainThread();

                if (system.particleSystem != null) {
                    ColorManager.SetParticleAlpha(system.particleSystem, ParticleAlpha);
                    StateHandler.RestoreParticleSystemState(system.particleSystem);

                    // Update the mesh now that particle system exists
                    if (meshManagers.TryGetValue(sprenType, out MeshManager? meshManager)) {
                        system.UpdateMesh(meshManager);
                    }


                    // Force the particle system to start playing
                    system.particleSystem.Play();
                } else {
                    Logger.Error($"[Spren] Failed to initialize particle system for {sprenType}");
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
        }
    }

    private void UpdateAllSprenControllers() {
        // Update ALL controllers (both static and dynamic need periodic refreshes)
        foreach (BaseSprenController controller in allControllers) {
            SprenType sprenType = controller.sprenType;

            // Update controller's cells (handles timing internally)
            controller.UpdateInfo(map);

            if (controller.validSpawnInfo.Count > 0) {
                // Create or update the spren system
                if (!sprenSystems.ContainsKey(sprenType)) {
                    CreateSprenSystem(sprenType, controller);
                }

                if (!meshManagers.TryGetValue(sprenType, out MeshManager? value)) continue;
                SprenParticleSystem? sprenSystem = sprenSystems[sprenType];

                // Only update if particle system is initialized
                if (sprenSystem.particleSystem != null) {
                    sprenSystem.UpdateMesh(value);
                    StateHandler.SetParticleSystemState(sprenSystem.particleSystem, true);
                }
            } else if (sprenSystems.TryGetValue(sprenType, out SprenParticleSystem? system)) {
                if (system.particleSystem != null) {
                    StateHandler.SetParticleSystemState(system.particleSystem, false);
                }
            }
        }
    }

    private void CreateSprenSystem(SprenType sprenType, BaseSprenController controller) {
        SprenParticleSystem system = new SprenParticleSystem(sprenType, mapID);
        sprenSystems[sprenType] = system;


        MeshManager meshManager = new MeshManager(
            map,
            pos => controller.validSpawnInfo.Any(info => info.position.Equals(pos.ToIntVec3()))
        );
        meshManagers[sprenType] = meshManager;


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
        // Initialize cells for all controllers
        foreach (BaseSprenController controller in allControllers) {
            controller.InitializeInfo(map);

            // Create spren system if controller has valid cells
            if (controller.validSpawnInfo.Count > 0) {
                CreateSprenSystem(controller.sprenType, controller);
            }
        }

        initialized = true;
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