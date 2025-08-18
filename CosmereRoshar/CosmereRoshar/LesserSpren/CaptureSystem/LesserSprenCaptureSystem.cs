using System;
using Cosmere.Core.Comp.Thing;
using Cosmere.Resources.Def;
using Cosmere.Resources.DefModExtension;
using Cosmere.Roshar.Comp.Thing;
using Cosmere.Roshar.LesserSpren.ParticleSystem;
using Cosmere.Roshar.LesserSpren.SprenControllers;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Roshar.LesserSpren.CaptureSystem;

/// <summary>
///     System for capturing lesser spren particles into gemstones
///     Configuration for each spren type is stored in their respective controllers
/// </summary>
public static class LesserSprenCaptureSystem {
    public const float DefaultCaptureRadius = 3f;

    /// <summary>
    ///     Check if a gem can capture a specific spren type
    /// </summary>
    public static bool CanGemCaptureSpren(ThingWithComps gem, SprenType sprenType) {
        if (gem?.Stuff == null) return false;

        // Check if gem already has a captured spren (using the new system)
        SprenContainer? sprenContainer = gem.TryGetComp<SprenContainer>();
        if (sprenContainer != null && sprenContainer.hasCapturedSpren) return false;

        // Get the controller for this spren type
        BaseSprenController controller = SprenControllerRegistry.GetController(sprenType)!;
        if (!controller.canBeCaptured) return false;

        // Check if this gem type is compatible
        GemDef? gemDef = gem.Stuff.GetModExtension<GemsLinked>().Gems.FirstOrDefault();
        if (gemDef == null) return false;

        return controller.compatibleGemTypes.Contains(gemDef);
    }

    public static bool TryCaptureSprenWithInfo(IntVec3 position, Map map, ThingWithComps gem, SprenType sprenType) {
        BaseSprenController? controller = SprenControllerRegistry.GetController(sprenType);
        SprenSpawnInformation? info =
            controller?.activeSpawnInfo.FirstOrDefault(info => info.position.Equals(position));

        return info != null && TryCaptureSprenWithInfo(info, map, gem, sprenType);
    }

    /// <summary>
    ///     Try to capture a spren particle at a specific position
    /// </summary>
    public static bool TryCaptureSprenWithInfo(
        SprenSpawnInformation info,
        Map map,
        ThingWithComps gem,
        SprenType targetSprenType
    ) {
        // Validate gem can capture this spren type
        if (!CanGemCaptureSpren(gem, targetSprenType)) {
            return false;
        }

        // Check if there are active spren particles at this position
        if (!IsSprenActive(info, map, targetSprenType)) {
            return false;
        }

        // Get investiture component
        InvestitureHolder? investiture = gem.TryGetComp<InvestitureHolder>();
        if (investiture == null || investiture.currentInvestiture <= 0) {
            return false;
        }

        // Calculate capture probability based on gem quality and controller settings
        float captureChance = CalculateCaptureChance(gem, targetSprenType);

        if (Rand.Chance(captureChance)) {
            // Perform capture
            return PerformCapture(gem, targetSprenType, info, map);
        }

        // Drain some investiture on failed attempt
        investiture.currentInvestitureSelf = investiture.maxInvestitureSelf * .1f;
        return false;
    }

    /// <summary>
    ///     Check if a specific spren type is active within radius of a position
    /// </summary>
    private static bool IsSprenActiveWithinRadius(IntVec3 position, Map map, SprenType sprenType, float radius) {
        BaseSprenController? controller = SprenControllerRegistry.GetController(sprenType);
        if (controller == null || !controller.isEnabled) return false;

        return controller.activeSpawnInfo.Any(info =>
            map.Equals(info.map) && position.DistanceTo(info.position!.Value) <= radius
        );
    }

    /// <summary>
    ///     Check if a specific spren type is active at a position
    /// </summary>
    private static bool IsSprenActive(SprenSpawnInformation info, Map map, SprenType sprenType) {
        BaseSprenController? controller = SprenControllerRegistry.GetController(sprenType);
        if (controller == null || !controller.isEnabled) return false;

        return controller.activeSpawnInfo.Contains(info);
    }

    /// <summary>
    ///     Check if a specific spren type is active at a position
    /// </summary>
    private static bool IsSprenActive(IntVec3 position, Map map, SprenType sprenType) {
        BaseSprenController? controller = SprenControllerRegistry.GetController(sprenType);
        if (controller == null || !controller.isEnabled) return false;

        return controller.activeSpawnInfo.Any(info => info.position.Equals(position));
    }

    /// <summary>
    ///     Calculate capture chance based on gem quality and spren rarity
    /// </summary>
    private static float CalculateCaptureChance(ThingWithComps gem, SprenType sprenType) {
        // Base chance
        float baseChance = 0.5f;

        // Get rarity multiplier from controller
        BaseSprenController controller = SprenControllerRegistry.GetController(sprenType)!;
        float rarityMultiplier = controller.captureRarityMultiplier;

        // Additional factors could include gem quality, etc.
        CompQuality? quality = gem.TryGetComp<CompQuality>();
        float qualityMultiplier = quality != null ? 0.5f + (int)quality.Quality * 0.1f : 1f;

        return Mathf.Clamp01(baseChance * rarityMultiplier * qualityMultiplier);
    }

    /// <summary>
    ///     Perform the actual capture
    /// </summary>
    private static bool PerformCapture(ThingWithComps gem, SprenType sprenType, SprenSpawnInformation info, Map map) {
        // Get or create the spren container component
        SprenContainer? sprenContainer = gem.TryGetComp<SprenContainer>();
        if (sprenContainer == null) {
            return false;
        }

        // Store the captured spren
        sprenContainer.CaptureSpren(sprenType);

        // Remove the spren from the active spawn position
        RemoveSprenWithInfo(info, map, sprenType);

        // Show message to player
        Messages.Message(
            $"Successfully captured a {sprenType} in the {gem.Label}!",
            gem,
            MessageTypeDefOf.PositiveEvent
        );

        return true;
    }

    /// <summary>
    ///     Remove a spren from its spawn position after capture
    /// </summary>
    private static void RemoveSprenWithInfo(SprenSpawnInformation info, Map map, SprenType sprenType) {
        BaseSprenController? controller = SprenControllerRegistry.GetController(sprenType);
        if (controller == null || !controller.isEnabled) return;

        controller.activeSpawnInfo.Remove(info);

        // The spawner will handle particle system refresh on its next update cycle
    }

    /// <summary>
    ///     Get all spren types that can be captured within radius of a position
    /// </summary>
    public static List<SprenType> GetCapturableSprenWithinRadius(
        IntVec3 position,
        Map map,
        float radius = DefaultCaptureRadius
    ) {
        List<SprenType> capturableSpren = [];

        foreach (SprenType sprenType in Enum.GetValues(typeof(SprenType))) {
            BaseSprenController? controller = SprenControllerRegistry.GetController(sprenType);
            if (controller is { canBeCaptured: true } && IsSprenActiveWithinRadius(position, map, sprenType, radius)) {
                capturableSpren.Add(sprenType);
            }
        }

        return capturableSpren;
    }

    /// <summary>
    ///     Get all spren types that can be captured at a specific position
    /// </summary>
    public static List<SprenType> GetCapturableSprenAtPosition(IntVec3 position, Map map) {
        List<SprenType> capturableSpren = [];

        foreach (SprenType sprenType in Enum.GetValues(typeof(SprenType))) {
            BaseSprenController? controller = SprenControllerRegistry.GetController(sprenType);
            if (controller is { canBeCaptured: true } && IsSprenActive(position, map, sprenType)) {
                capturableSpren.Add(sprenType);
            }
        }

        return capturableSpren;
    }

    /// <summary>
    ///     Try to capture a spren within radius of a position
    /// </summary>
    public static bool TryCaptureSprenWithinRadius(
        IntVec3 position,
        Map map,
        ThingWithComps gem,
        SprenType targetSprenType,
        float radius = DefaultCaptureRadius
    ) {
        // Validate gem can capture this spren type
        if (!CanGemCaptureSpren(gem, targetSprenType)) {
            return false;
        }

        // Find closest spren of this type within radius
        SprenSpawnInformation? closestSpren =
            FindClosestSprenWithinRadius(position, map, targetSprenType, radius);
        if (closestSpren == null) {
            return false;
        }

        // Try to capture at the closest position
        return TryCaptureSprenWithInfo(closestSpren, map, gem, targetSprenType);
    }

    /// <summary>
    ///     Try to capture any available spren within radius with a gem
    /// </summary>
    public static bool TryCaptureAnySprenWithinRadius(
        IntVec3 position,
        Map map,
        ThingWithComps gem,
        float radius = DefaultCaptureRadius
    ) {
        List<SprenType> availableSpren = GetCapturableSprenWithinRadius(position, map, radius);

        // Try to capture spren in order of preference (rarer spren first)
        availableSpren.Sort((a, b) => {
                BaseSprenController controllerA = SprenControllerRegistry.GetController(a)!;
                BaseSprenController controllerB = SprenControllerRegistry.GetController(b)!;
                return controllerA.captureRarityMultiplier.CompareTo(controllerB.captureRarityMultiplier);
            }
        );

        return availableSpren.Where(sprenType => CanGemCaptureSpren(gem, sprenType))
            .Any(sprenType => TryCaptureSprenWithinRadius(position, map, gem, sprenType, radius));
    }

    /// <summary>
    ///     Try to capture any available spren at a position with a gem
    /// </summary>
    public static bool TryCaptureAnySprenAtPosition(IntVec3 position, Map map, ThingWithComps gem) {
        List<SprenType> availableSpren = GetCapturableSprenAtPosition(position, map);

        // Try to capture spren in order of preference (rarer spren first)
        availableSpren.Sort((a, b) => {
                BaseSprenController controllerA = SprenControllerRegistry.GetController(a)!;
                BaseSprenController controllerB = SprenControllerRegistry.GetController(b)!;
                return controllerA.captureRarityMultiplier.CompareTo(controllerB.captureRarityMultiplier);
            }
        );

        return availableSpren.Where(sprenType => CanGemCaptureSpren(gem, sprenType))
            .Any(sprenType => TryCaptureSprenWithInfo(position, map, gem, sprenType));
    }

    /// <summary>
    ///     Find the closest spren of a specific type within radius
    /// </summary>
    private static SprenSpawnInformation? FindClosestSprenWithinRadius(
        IntVec3 position,
        Map map,
        SprenType sprenType,
        float radius
    ) {
        BaseSprenController? controller = SprenControllerRegistry.GetController(sprenType);
        if (controller == null || !controller.isEnabled) return null;

        SprenSpawnInformation? closestInfo = null;
        float closestDistance = float.MaxValue;

        foreach (SprenSpawnInformation? activeInfo in controller.activeSpawnInfo) {
            float distance = position.DistanceTo(activeInfo.position!.Value);
            if (!(distance <= radius) || !(distance < closestDistance)) continue;
            closestDistance = distance;
            closestInfo = activeInfo;
        }

        return closestInfo;
    }
}