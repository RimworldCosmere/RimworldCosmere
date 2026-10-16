using System;
using Cosmere.Core.Comp.Thing;
using Cosmere.Core.Def;
using Cosmere.Core.DefModExtension;
using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using Cosmere.System.Roshar.LesserSpren.SprenController;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.LesserSpren.CaptureSystem;

/// <summary>
///     System for capturing lesser spren particles into gemstones.
///     Configuration for each spren type is stored in their respective controllers.
/// </summary>
public static class LesserSprenCaptureSystem {
    public const float DefaultCaptureRadius = 3f;

    public static bool CanGemCaptureSpren(ThingWithComps gem, SprenType sprenType) {
        if (gem?.Stuff == null) return false;

        SprenContainer? sprenContainer = gem.TryGetComp<SprenContainer>();
        if (sprenContainer != null && sprenContainer.HasCapturedSpren) return false;

        BaseSprenController controller = SprenControllerRegistry.GetController(sprenType)!;
        if (!controller.CanBeCaptured) return false;

        GemDef? gemDef = gem.Stuff.GetModExtension<GemsLinked>()?.Gems.FirstOrDefault();
        if (gemDef == null) return false;

        return controller.CompatibleGemTypes.Contains(gemDef);
    }

    public static bool AttemptCaptureSpren(IntVec3 position, Map map, ThingWithComps gem, SprenType sprenType) {
        BaseSprenController? controller = SprenControllerRegistry.GetController(sprenType);
        SprenSpawnInformation? info =
            controller?.ActiveSpawnInfo.FirstOrDefault(info => info.position.Equals(position));

        return info != null && AttemptCaptureSpren(info, map, gem, sprenType);
    }

    /// <summary>
    ///     Try to capture a spren particle. On failure, drains 90% of the gem's investiture.
    /// </summary>
    /// <returns></returns>
    public static bool AttemptCaptureSpren(
        SprenSpawnInformation info,
        Map map,
        ThingWithComps gem,
        SprenType targetSprenType
    ) {
        if (!CanGemCaptureSpren(gem, targetSprenType)) {
            return false;
        }

        if (!IsSprenActive(info, targetSprenType)) {
            return false;
        }

        InvestitureHolder? investiture = gem.TryGetComp<InvestitureHolder>();
        if (investiture == null || investiture.CurrentInvestiture <= 0) {
            return false;
        }

        float captureChance = CalculateCaptureChance(gem, targetSprenType);

        if (Rand.Chance(captureChance)) {
            return PerformCapture(gem, targetSprenType, info);
        }

        investiture.CurrentInvestitureSelf = investiture.maxInvestitureSelf * .1f;
        return false;
    }

    private static bool IsSprenActiveWithinRadius(IntVec3 position, Map map, SprenType sprenType, float radius) {
        BaseSprenController? controller = SprenControllerRegistry.GetController(sprenType);
        if (controller == null || !controller.IsEnabled) return false;

        foreach (SprenSpawnInformation info in controller.ActiveSpawnInfo) {
            if (info.map != map) continue;
            if (position.Equals(info.position!.Value)) return true;
            if (position.DistanceTo(info.position.Value) <= radius) return true;
        }

        return false;
    }

    private static bool IsSprenActive(SprenSpawnInformation info, SprenType sprenType) {
        BaseSprenController? controller = SprenControllerRegistry.GetController(sprenType);
        if (controller == null || !controller.IsEnabled) return false;

        return controller.ActiveSpawnInfo.Contains(info);
    }

    private static bool IsSprenActive(IntVec3 position, SprenType sprenType) {
        BaseSprenController? controller = SprenControllerRegistry.GetController(sprenType);
        if (controller == null || !controller.IsEnabled) return false;

        return controller.ActiveSpawnInfo.Any(info => info.position.Equals(position));
    }

    private static float CalculateCaptureChance(ThingWithComps gem, SprenType sprenType) {
        const float baseChance = .75f;

        BaseSprenController controller = SprenControllerRegistry.GetController(sprenType)!;
        float rarityMultiplier = controller.CaptureRarityMultiplier;

        CompQuality? quality = gem.TryGetComp<CompQuality>();
        float qualityMultiplier = quality != null ? 0.5f + (int)quality.Quality * 0.1f : 1f;

        return Mathf.Clamp01(baseChance * rarityMultiplier * qualityMultiplier);
    }

    private static bool PerformCapture(ThingWithComps gem, SprenType sprenType, SprenSpawnInformation info) {
        SprenContainer? sprenContainer = gem.TryGetComp<SprenContainer>();
        if (sprenContainer == null) {
            return false;
        }

        sprenContainer.CaptureSpren(sprenType);

        RemoveSprenWithInfo(info, sprenType);

        Messages.Message(
            $"Successfully captured a {sprenType} in the {gem.Label}!",
            gem,
            MessageTypeDefOf.PositiveEvent
        );

        return true;
    }

    private static void RemoveSprenWithInfo(SprenSpawnInformation info, SprenType sprenType) {
        BaseSprenController? controller = SprenControllerRegistry.GetController(sprenType);
        if (controller == null || !controller.IsEnabled) return;

        controller.RemoveActiveSpawnInfo(info);
    }

    public static List<BaseSprenController> GetCapturableSprenWithinRadius(
        IntVec3 position,
        Map map,
        float radius = DefaultCaptureRadius
    ) {
        List<BaseSprenController> capturableSpren = [];

        foreach (SprenType sprenType in Enum.GetValues(typeof(SprenType))) {
            BaseSprenController? controller = SprenControllerRegistry.GetController(sprenType);
            if (controller is { CanBeCaptured: true } && IsSprenActiveWithinRadius(position, map, sprenType, radius)) {
                capturableSpren.Add(controller);
            }
        }

        return capturableSpren;
    }

    public static List<SprenType> GetCapturableSprenAtPosition(IntVec3 position, Map map) {
        List<SprenType> capturableSpren = [];

        foreach (SprenType sprenType in Enum.GetValues(typeof(SprenType))) {
            BaseSprenController? controller = SprenControllerRegistry.GetController(sprenType);
            if (controller is { CanBeCaptured: true } && IsSprenActive(position, sprenType)) {
                capturableSpren.Add(sprenType);
            }
        }

        return capturableSpren;
    }

    public static bool TryCaptureSprenWithinRadius(
        IntVec3 position,
        Map map,
        ThingWithComps gem,
        SprenType targetSprenType,
        float radius = DefaultCaptureRadius
    ) {
        if (!CanGemCaptureSpren(gem, targetSprenType)) {
            return false;
        }

        SprenSpawnInformation? closestSpren =
            FindClosestSprenWithinRadius(position, targetSprenType, radius);
        if (closestSpren == null) {
            return false;
        }

        return AttemptCaptureSpren(closestSpren, map, gem, targetSprenType);
    }

    public static bool TryCaptureAnySprenWithinRadius(
        IntVec3 position,
        Map map,
        ThingWithComps gem,
        float radius = DefaultCaptureRadius
    ) {
        List<BaseSprenController> availableSpren = GetCapturableSprenWithinRadius(position, map, radius);

        availableSpren.Sort((a, b) => a.CaptureRarityMultiplier.CompareTo(b.CaptureRarityMultiplier));

        return availableSpren.Where(c => CanGemCaptureSpren(gem, c.SprenType))
            .Any(c => TryCaptureSprenWithinRadius(position, map, gem, c.SprenType, radius));
    }

    public static bool TryCaptureAnySprenAtPosition(IntVec3 position, Map map, ThingWithComps gem) {
        List<SprenType> availableSpren = GetCapturableSprenAtPosition(position, map);

        availableSpren.Sort((a, b) => {
            BaseSprenController controllerA = SprenControllerRegistry.GetController(a)!;
            BaseSprenController controllerB = SprenControllerRegistry.GetController(b)!;
            return controllerA.CaptureRarityMultiplier.CompareTo(controllerB.CaptureRarityMultiplier);
        }
        );

        return availableSpren.Where(sprenType => CanGemCaptureSpren(gem, sprenType))
            .Any(sprenType => AttemptCaptureSpren(position, map, gem, sprenType));
    }

    private static SprenSpawnInformation? FindClosestSprenWithinRadius(
        IntVec3 position,
        SprenType sprenType,
        float radius
    ) {
        BaseSprenController? controller = SprenControllerRegistry.GetController(sprenType);
        if (controller == null || !controller.IsEnabled) return null;

        SprenSpawnInformation? closestInfo = null;
        float closestDistance = float.MaxValue;

        foreach (SprenSpawnInformation? activeInfo in controller.ActiveSpawnInfo) {
            float distance = position.DistanceTo(activeInfo.position!.Value);
            if (!(distance <= radius) || !(distance < closestDistance)) continue;
            closestDistance = distance;
            closestInfo = activeInfo;
        }

        return closestInfo;
    }
}
