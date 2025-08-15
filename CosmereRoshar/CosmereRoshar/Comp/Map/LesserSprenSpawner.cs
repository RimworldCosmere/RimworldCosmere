using System.Threading.Tasks;
using Cosmere.Roshar.Debug;
using Cosmere.Roshar.ParticleSystem.LesserSpren;
using Cosmere.Roshar.ParticleSystem.LesserSpren.SprenControllers;
using Verse;
using Logger = Cosmere.Foundation.Logger;

namespace Cosmere.Roshar.Comp.Map;

public class LesserSprenSpawner : MapComponent {
    private const float ParticleAlpha = 2.5f;

    private const float
        DynamicUpdateInterval = GenTicks.TickRareInterval; // Update dynamic spren every 250 ticks (about 4 seconds)

    private readonly int mapID;
    private readonly Dictionary<SprenType, MeshManager> meshManagers = new Dictionary<SprenType, MeshManager>();
    private readonly List<SprenType> pendingInitialization = new List<SprenType>();

    private readonly Dictionary<SprenType, SprenParticleSystem> sprenSystems =
        new Dictionary<SprenType, SprenParticleSystem>();

    private CellValidator cellValidator;
    private Task fetchTask;
    private bool initialized;
    private int lastDynamicUpdateTick;

    public LesserSprenSpawner(Verse.Map map) : base(map) {
        mapID = map.GetHashCode();
        LongEventHandler.ExecuteWhenFinished(InitializeMapSystems);
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

        // Update dynamic spren periodically
        if (Find.TickManager.TicksGame - lastDynamicUpdateTick >= DynamicUpdateInterval) {
            UpdateDynamicSpren();
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

        if (fetchTask is { IsCompleted: false }) return;
    }

    private void UpdateDynamicSpren() {
        IEnumerable<BaseSprenController> dynamicControllers = SprenControllerRegistry.GetEnabledDynamicControllers();

        foreach (BaseSprenController controller in dynamicControllers) {
            SprenType sprenType = controller.sprenType;

            List<IntVec3> cells = cellValidator.GetDynamicSprenCells(sprenType);

            if (cells.Count > 0) {
                // Create or update the spren system
                if (!sprenSystems.ContainsKey(sprenType)) {
                    CreateSprenSystem(sprenType);
                }

                SprenParticleSystem? system = sprenSystems[sprenType];
                system.UpdateValidCells(cells);

                // Update debug overlay for dynamic spren
                SprenDebugOverlay.UpdateValidCells(sprenType, cells);

                if (!meshManagers.TryGetValue(sprenType, out MeshManager? value)) continue;

                // Only update if particle system is initialized
                if (system.ParticleSystem != null) {
                    system.UpdateMesh(value);
                    StateHandler.SetParticleSystemState(system.ParticleSystem, true);

                    // Update debug overlay with actual particle positions
                    SprenDebugOverlay.UpdateActualParticles(sprenType, system.ParticleSystem);
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

    private void CreateSprenSystem(SprenType sprenType) {
        SprenParticleSystem system = new SprenParticleSystem(sprenType, mapID);
        sprenSystems[sprenType] = system;

        MeshManager meshManager = new MeshManager(
            map,
            pos => cellValidator.IsCellValidForSprenType(pos.ToIntVec3(), sprenType)
        );
        meshManagers[sprenType] = meshManager;

        // Mark for initialization on main thread
        pendingInitialization.Add(sprenType);
    }

    private void FetchAllCells() {
        fetchTask = Task.Run(() => {
                // Cache static nature spren cells
                cellValidator.CacheStaticSprenCells();

                // Initialize static spren systems
                IEnumerable<BaseSprenController> enabledNatureControllers =
                    SprenControllerRegistry.GetEnabledNatureControllers();

                foreach (BaseSprenController controller in enabledNatureControllers) {
                    SprenType sprenType = controller.sprenType;

                    List<IntVec3> cells = cellValidator.GetCellsForSprenType(sprenType);
                    if (cells.Count <= 0) continue;
                    CreateSprenSystem(sprenType);
                    SprenParticleSystem? system = sprenSystems[sprenType];
                    system.UpdateValidCells(cells);

                    // Don't update mesh here - particle system doesn't exist yet!
                    // Mesh will be updated on main thread after initialization

                    // Update debug overlay
                    SprenDebugOverlay.UpdateValidCells(sprenType, cells);
                }
            }
        );
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

        cellValidator = new CellValidator(map);

        // Initialize static nature spren after a delay
        FetchAllCells();

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