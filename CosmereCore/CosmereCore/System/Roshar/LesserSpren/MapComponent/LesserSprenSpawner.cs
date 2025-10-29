using System;
using System.Text;
using Cosmere.System.Roshar.Debug;
using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using Cosmere.System.Roshar.LesserSpren.SprenController;
using HarmonyLib;
using Verse;
using Verse.Profile;

namespace Cosmere.System.Roshar.LesserSpren.MapComponent;

[HarmonyPatch]
public class LesserSprenSpawner(Map map) : Verse.MapComponent(map) {
    private static readonly List<SprenType> PendingInitialization = [];

    private static readonly Dictionary<SprenType, SprenParticleSystem> SprenSystems =
        new Dictionary<SprenType, SprenParticleSystem>();

    private bool initialized;
    private int lastDynamicUpdateTick;

    private int mapID = map.GetHashCode();

    [HarmonyPatch(typeof(MemoryUtility), nameof(MemoryUtility.ClearAllMapsAndWorld))]
    [HarmonyPostfix]
    public static void OnClearAllMapsAndWorld() {
        CleanupAllSystems();
    }

    public override void MapComponentDraw() {
        SprenDebugOverlay.DrawOverlay();

        // Always process pending initializations, even before fully initialized
        // This allows the background task to queue systems for main thread init

        // Initialize any pending spren systems on main thread
        if (PendingInitialization.Count > 0) {
            foreach (SprenType sprenType in PendingInitialization.ToList()) {
                if (!SprenSystems.TryGetValue(sprenType, out SprenParticleSystem? sprenSystem)) continue;

                sprenSystem.Initialize();
                PendingInitialization.Remove(sprenType);
                return;
            }

            PendingInitialization.Clear();
        }

        // Handle particle re-emission during draw so it works even when paused
        if (!initialized) return;

        foreach (SprenParticleSystem? sprenSystem in SprenSystems.Values.Where(s => s.ShouldReEmitParticles())) {
            sprenSystem.EmitParticlesForActiveCells();
        }
    }

    public override void MapComponentTick() {
        base.MapComponentTick();

        // Don't do anything else until initialized
        if (!initialized) return;

        UpdateAllSprenControllers();
    }

    private void UpdateAllSprenControllers() {
        // Update ALL controllers (both static and dynamic need periodic refreshes)
        foreach (BaseSprenController controller in SprenControllerRegistry.enabledControllers) {
            SprenType sprenType = controller.sprenType;

            // Update controller's cells (handles timing internally)
            if (!controller.UpdateInfo(map)) {
                continue;
            }

            SprenParticleSystem? sprenSystem;
            if (controller.activeSpawnInfo.Count > 0) {
                // Create or update the spren system
                if (!SprenSystems.ContainsKey(sprenType)) {
                    CreateSprenSystem(sprenType);
                }

                sprenSystem = SprenSystems[sprenType];

                // Only update if particle system is initialized
                if (sprenSystem?.particleSystem == null) continue;

                sprenSystem.UpdateParticles();
                sprenSystem.particleSystem.gameObject.SetActive(true);
                sprenSystem.particleSystem.Play();
            } else if (SprenSystems.TryGetValue(sprenType, out sprenSystem)) {
                if (sprenSystem.particleSystem == null) continue;
                sprenSystem.particleSystem.gameObject.SetActive(false);
                sprenSystem.particleSystem.Stop();
            }
        }
    }

    private void CreateSprenSystem(SprenType sprenType) {
        SprenParticleSystem system = new SprenParticleSystem(sprenType, mapID);
        SprenSystems[sprenType] = system;

        // Mark for initialization on main thread
        PendingInitialization.Add(sprenType);
    }

    public override void MapGenerated() {
        base.MapGenerated();

        mapID = map.GetHashCode();
        initialized = false;
        lastDynamicUpdateTick = 0;

        LongEventHandler.ExecuteWhenFinished(InitializeMapSystems);
    }

    private static void CleanupAllSystems() {
        // Clean up all spren systems
        foreach (SprenParticleSystem? system in SprenSystems.Values) {
            system.Destroy();
        }

        // Reset all controllers for new map
        foreach (BaseSprenController controller in SprenControllerRegistry.enabledControllers) {
            controller.ResetForNewMap();
        }

        SprenSystems.Clear();
        PendingInitialization.Clear();
    }

    private void InitializeMapSystems() {
        if (initialized) return;

        // Get all enabled controllers
        // Initialize cells for all controllers
        foreach (BaseSprenController controller in SprenControllerRegistry.enabledControllers) {
            controller.InitializeInfo(map);

            // Create spren system if controller has valid cells
            if (controller.validSpawnInfo.Count > 0) {
                CreateSprenSystem(controller.sprenType);
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

        // Reset initialization on load to wait for map to fully load again
        if (initialized) {
            initialized = false;
        }
    }

    public string DebugStringAt(IntVec3 position) {
        StringBuilder info = new StringBuilder();

        bool foundValidSpren = false;

        foreach (SprenType sprenType in Enum.GetValues(typeof(SprenType))) {
            BaseSprenController? controller = SprenControllerRegistry.GetController(sprenType);
            if (controller == null || !controller.isEnabled) continue;

            // Get actual spawn information for this position
            SprenSpawnInformation? positionSpawnInfo = controller.GetSprenSpawnInformation(position, map);
            bool isValidAtPosition = positionSpawnInfo != null;
            bool isInActiveCells = controller.activeSpawnInfo.Any(info => info.position == position);
            bool isInValidCells = controller.validSpawnInfo.Any(info => info.position == position);

            bool shouldShow = isValidAtPosition || isInActiveCells || isInValidCells;

            if (controller is StaticSprenController) {
                // For nature spren, also check if it would be valid even if not currently active
                shouldShow = isValidAtPosition || isInValidCells;
            } else {
                // For dynamic spren, check dynamic cells
                List<SprenSpawnInformation> dynamicCells = controller.GetDynamicSpawnInfo(map);
                bool isInDynamicCells = dynamicCells.Any(i => i.position == position);
                shouldShow = shouldShow || isInDynamicCells;
            }

            if (!shouldShow) continue;

            foundValidSpren = true;
            info.AppendLine($"\n{sprenType} ({(controller is StaticSprenController ? "Static" : "Dynamic")}):");

            info.AppendLine($"  Valid at Position: {isValidAtPosition.ColoredBool()}");
            info.AppendLine($"  In Active Cells: {isInActiveCells.ColoredBool()}");
            info.AppendLine($"  In Valid Cells: {isInValidCells.ColoredBool()}");

            if (controller is not StaticSprenController) {
                List<SprenSpawnInformation> dynamicCells = controller.GetDynamicSpawnInfo(map);
                bool isInDynamicCells = dynamicCells.Any(i => i.position == position);
                info.AppendLine($"  In Dynamic Cells: {isInDynamicCells.ColoredBool()}");
            }

            // Show actual spawn information if available
            if (positionSpawnInfo != null) {
                info.AppendLine($"  Actual Spawn Chance: {positionSpawnInfo.spawnChance:P2}");
            }

            AppendControllerSettings(info, controller);

            string controllerDebug = controller.DebugStringAt(position);
            if (!string.IsNullOrEmpty(controllerDebug)) info.Append(controllerDebug);
        }

        return !foundValidSpren ? "" : info.ToString();
    }

    private void AppendControllerSettings(StringBuilder info, BaseSprenController controller) {
        info.AppendLine($"  Spawn Chance: {controller.cellSpawnChance:P1}");
        info.AppendLine($"  Particles/Cell: {controller.minParticlesPerCell}-{controller.maxParticlesPerCell}");
        info.AppendLine($"  Color: {controller.sprenColor.ColoredColor()}");
        info.AppendLine($"  Size Mult: {controller.sprenSizeMultiplier.ColoredBreakpoints()}x");
        info.AppendLine($"  Emission Mult: {controller.emissionRateMultiplier.ColoredBreakpoints()}x");
        info.AppendLine($"  Speed Mult: {controller.sprenSpeedMultiplier.ColoredBreakpoints()}x");
    }
}