using System;
using System.Text;
using Concord;
using Cosmere.Core.Def;
using Cosmere.Core.Util;
using Cosmere.System.Roshar.Dev;
using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using Cosmere.System.Roshar.LesserSpren.SprenController;
using RimWorld.Planet;
using Verse;
using Verse.Profile;

namespace Cosmere.System.Roshar.LesserSpren.MapComponent;

[Patch(typeof(MemoryUtility))]
public static class LesserSprenSpawnerStateClearer {
    [Inject(At.Return, nameof(MemoryUtility.ClearAllMapsAndWorld))]
    private static void AfterClearAllMapsAndWorld() {
        LesserSprenSpawner.OnClearAllMapsAndWorld();
    }
}

public class LesserSprenSpawner(Map map) : Verse.MapComponent(map) {
    private static readonly List<SprenType> PendingInitialization = [];

    private static readonly Dictionary<SprenType, SprenParticleSystem> SprenSystems = [];

    private bool initialized;
    private int lastDynamicUpdateTick;

    private int mapID = map.GetHashCode();

    public static void OnClearAllMapsAndWorld() {
        CleanupAllSystems();
    }

    public override void MapComponentDraw() {
        // spren are Honor's and Cultivation's; a Scadrial map should not be growing any
        if (!FeatureUtility.IsActive(FeatureDefOf.Cosmere_Feature_Spren)) return;
        SprenDebugOverlay.DrawOverlay();

        // process pending inits first: background scanners queue work here while the map is still loading
        if (PendingInitialization.Count > 0) {
            foreach (SprenType sprenType in PendingInitialization.ToList()) {
                if (!SprenSystems.TryGetValue(sprenType, out SprenParticleSystem? sprenSystem)) continue;

                sprenSystem.Initialize();
                PendingInitialization.Remove(sprenType);
                return;
            }

            PendingInitialization.Clear();
        }

        if (!initialized) return;

        // Re-emission runs during Draw so particles continue while paused.
        foreach (SprenParticleSystem? sprenSystem in SprenSystems.Values.Where(s => s.ShouldReEmitParticles())) {
            sprenSystem.EmitParticlesForActiveCells();
        }
    }

    public override void MapComponentUpdate() {
        // spren are Honor's and Cultivation's; a Scadrial map should not be growing any
        if (!FeatureUtility.IsActive(FeatureDefOf.Cosmere_Feature_Spren)) return;
        base.MapComponentUpdate();

        bool onWorldMap = WorldRendererUtility.WorldRendered;
        foreach (SprenParticleSystem? sprenSystem in SprenSystems.Values) {
            sprenSystem.SetVisible(!onWorldMap);
        }
    }

    public override void MapComponentTick() {
        // spren are Honor's and Cultivation's; a Scadrial map should not be growing any
        if (!FeatureUtility.IsActive(FeatureDefOf.Cosmere_Feature_Spren)) return;
        base.MapComponentTick();

        if (!initialized) return;

        UpdateAllSprenControllers();
    }

    private void UpdateAllSprenControllers() {
        foreach (BaseSprenController controller in SprenControllerRegistry.EnabledControllers) {
            SprenType sprenType = controller.SprenType;

            if (!controller.UpdateInfo(map)) {
                continue;
            }

            SprenParticleSystem? sprenSystem;
            if (controller.ActiveSpawnInfo.Count > 0) {
                if (!SprenSystems.ContainsKey(sprenType)) {
                    CreateSprenSystem(sprenType);
                }

                sprenSystem = SprenSystems[sprenType];

                if (sprenSystem?.ParticleSystem == null) continue;

                sprenSystem.UpdateParticles();
                sprenSystem.ParticleSystem.gameObject.SetActive(true);
                sprenSystem.ParticleSystem.Play();
            } else if (SprenSystems.TryGetValue(sprenType, out sprenSystem)) {
                if (sprenSystem.ParticleSystem == null) continue;
                sprenSystem.ParticleSystem.gameObject.SetActive(false);
                sprenSystem.ParticleSystem.Stop();
            }
        }
    }

    private void CreateSprenSystem(SprenType sprenType) {
        SprenParticleSystem system = new SprenParticleSystem(sprenType, mapID);
        SprenSystems[sprenType] = system;

        PendingInitialization.Add(sprenType);
    }

    public override void MapGenerated() {
        // spren are Honor's and Cultivation's; a Scadrial map should not be growing any
        if (!FeatureUtility.IsActive(FeatureDefOf.Cosmere_Feature_Spren)) return;
        base.MapGenerated();

        mapID = map.GetHashCode();
        initialized = false;
        lastDynamicUpdateTick = 0;

        LongEventHandler.ExecuteWhenFinished(InitializeMapSystems);
    }

    private static void CleanupAllSystems() {
        foreach (SprenParticleSystem? system in SprenSystems.Values) {
            system.Destroy();
        }

        foreach (BaseSprenController controller in SprenControllerRegistry.EnabledControllers) {
            controller.ResetForNewMap();
        }

        SprenSystems.Clear();
        PendingInitialization.Clear();
    }

    private void InitializeMapSystems() {
        if (initialized) return;

        foreach (BaseSprenController controller in SprenControllerRegistry.EnabledControllers) {
            controller.InitializeInfo(map);

            if (controller.ValidSpawnInfo.Count > 0) {
                CreateSprenSystem(controller.SprenType);
            }
        }

        initialized = true;
    }

    public override void ExposeData() {
        base.ExposeData();
        Scribe_Values.Look(ref initialized, "initialized");
        Scribe_Values.Look(ref lastDynamicUpdateTick, "lastDynamicUpdateTick");

        // Particle systems are recreated post-load rather than serialized.
        if (Scribe.mode != LoadSaveMode.PostLoadInit) return;

        if (initialized) {
            initialized = false;
        }
    }

    public string DebugStringAt(IntVec3 position) {
        StringBuilder info = new StringBuilder();

        bool foundValidSpren = false;

        foreach (SprenType sprenType in Enum.GetValues<SprenType>()) {
            BaseSprenController? controller = SprenControllerRegistry.GetController(sprenType);
            if (controller == null || !controller.IsEnabled) continue;
            if (AppendSprenDebug(info, controller, sprenType, position)) foundValidSpren = true;
        }

        return !foundValidSpren ? string.Empty : info.ToString();
    }

    private bool AppendSprenDebug(
        StringBuilder info,
        BaseSprenController controller,
        SprenType sprenType,
        IntVec3 position
    ) {
        // Get actual spawn information for this position
        SprenSpawnInformation? positionSpawnInfo = controller.GetSprenSpawnInformation(position, map);
        bool isValidAtPosition = positionSpawnInfo != null;
        bool isInActiveCells = controller.ActiveSpawnInfo.Any(i => i.position == position);
        bool isInValidCells = controller.ValidSpawnInfo.Any(i => i.position == position);

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

        if (!shouldShow) return false;

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

        return true;
    }

    private static void AppendControllerSettings(StringBuilder info, BaseSprenController controller) {
        info.AppendLine($"  Spawn Chance: {controller.CellSpawnChance:P1}");
        info.AppendLine($"  Particles/Cell: {controller.MinParticlesPerCell}-{controller.MaxParticlesPerCell}");
        info.AppendLine($"  Color: {controller.SprenColor.ColoredColor()}");
        info.AppendLine($"  Size Mult: {controller.SprenSizeMultiplier.ColoredBreakpoints()}x");
        info.AppendLine($"  Emission Mult: {controller.EmissionRateMultiplier.ColoredBreakpoints()}x");
        info.AppendLine($"  Speed Mult: {controller.SprenSpeedMultiplier.ColoredBreakpoints()}x");
    }
}
