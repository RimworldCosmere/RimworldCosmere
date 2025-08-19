using System;
using Cosmere.Core.Comp.Thing;
using Cosmere.Roshar.Comp.Thing;
using Cosmere.Roshar.LesserSpren.CaptureSystem;
using Cosmere.Roshar.LesserSpren.ParticleSystem;
using Cosmere.Roshar.LesserSpren.SprenControllers;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.Roshar.FloatMenuOptionProvider;

public class SprenCaptureMenuProvider : RimWorld.FloatMenuOptionProvider {
    protected override bool Drafted => true;
    protected override bool Undrafted => true;
    protected override bool Multiselect => false;

    public override IEnumerable<FloatMenuOption> GetOptions(FloatMenuContext context) {
        if (context.FirstSelectedPawn == null) yield break;

        List<FloatMenuOption> options = GetSprenCaptureOptionsForCell(
            context.ClickedCell,
            context.map,
            context.FirstSelectedPawn
        );

        foreach (FloatMenuOption option in options) {
            yield return option;
        }
    }

    /// <summary>
    ///     Get float menu options for capturing spren at a specific cell
    /// </summary>
    private static List<FloatMenuOption> GetSprenCaptureOptionsForCell(IntVec3 cell, Map map, Pawn pawn) {
        List<FloatMenuOption> options = [];

        // Get all capturable spren within radius of this cell first
        List<BaseSprenController> capturableSpren = LesserSprenCaptureSystem.GetCapturableSprenWithinRadius(
            cell,
            map
        );

        if (capturableSpren.Count == 0) {
            return [];
        }

        // Check if pawn has access to gems that can capture any of these spren
        List<ThingWithComps> suitableGems = FindSuitableGemsForSprenTypes(pawn, capturableSpren);

        if (suitableGems.Count == 0) {
            // Show which spren are available but can't be captured
            string sprenList = string.Join(", ", capturableSpren.Select(s => s.GetLocalizedName()));
            options.Add(
                new FloatMenuOption(
                    "SprenCapture_NoSuitableGem".Translate(sprenList),
                    null
                )
            );
            return options;
        }

        // Use the best suitable gem (prioritize inventory, then closest)
        ThingWithComps suitableGem = suitableGems.First();
        bool gemInInventory = FindSuitableGemInInventory(pawn) == suitableGem;

        // Create option for each capturable spren type
        foreach (BaseSprenController? controller in capturableSpren) {
            SprenType sprenType = controller.sprenType;

            string sprenName = controller.GetLocalizedName();

            // Check if the gem can capture this specific spren type
            if (!LesserSprenCaptureSystem.CanGemCaptureSpren(suitableGem, sprenType)) {
                options.Add(
                    new FloatMenuOption(
                        "SprenCapture_IncompatibleGem".Translate(sprenName),
                        null
                    )
                );
                continue;
            }

            // Check if gem has investiture
            InvestitureHolder? investiture = suitableGem.TryGetComp<InvestitureHolder>();
            if (investiture == null || investiture.currentInvestiture <= 0) {
                options.Add(
                    new FloatMenuOption(
                        "SprenCapture_GemNeedsStormlight".Translate(sprenName),
                        null
                    )
                );
                continue;
            }

            // Check if pawn can reach the area
            if (!pawn.CanReach(cell, PathEndMode.OnCell, Danger.Deadly)) {
                options.Add(
                    new FloatMenuOption(
                        "SprenCapture_CannotReach".Translate(sprenName),
                        null
                    )
                );
                continue;
            }

            // Create working capture option
            string optionText = gemInInventory
                ? "SprenCapture_CaptureSpren".Translate(sprenName)
                : "SprenCapture_CaptureSprenPickupGem".Translate(sprenName, suitableGem.Label);

            options.Add(
                new FloatMenuOption(
                    optionText,
                    () => GiveSprenCaptureJob(pawn, cell, map, sprenType, suitableGem)
                )
            );
        }

        // Add general "capture any spren" option if multiple types available
        if (capturableSpren.Count > 1) {
            string generalOptionText = gemInInventory
                ? "SprenCapture_CaptureAnySuitable".Translate()
                : "SprenCapture_CaptureAnySuitablePickupGem".Translate(suitableGem.Label);

            options.Add(
                new FloatMenuOption(
                    generalOptionText,
                    () => GiveGeneralSprenCaptureJob(pawn, cell, map, suitableGem)
                )
            );
        }

        return options;
    }

    /// <summary>
    ///     Find suitable gems for capturing specific spren types (inventory first, then map)
    /// </summary>
    private static List<ThingWithComps> FindSuitableGemsForSprenTypes(
        Pawn pawn,
        List<BaseSprenController> controllers
    ) {
        List<ThingWithComps> suitableGems = [];

        // First check pawn's inventory
        ThingWithComps? inventoryGem = FindSuitableGemInInventory(pawn, controllers.Select(x => x.sprenType).ToList());
        if (inventoryGem != null) {
            suitableGems.Add(inventoryGem);
            return suitableGems; // Prioritize inventory gems
        }

        // Then check available gems on the map
        List<ThingWithComps> mapGems = FindSuitableGemsOnMapForSprenTypes(
            pawn,
            controllers.Select(x => x.sprenType).ToList()
        );
        suitableGems.AddRange(mapGems);

        return suitableGems;
    }

    /// <summary>
    ///     Find a suitable gem for spren capture (inventory first, then map) - Legacy method
    /// </summary>
    private static ThingWithComps? FindSuitableGemForCapture(Pawn pawn) {
        // First check pawn's inventory
        ThingWithComps? inventoryGem = FindSuitableGemInInventory(pawn);
        if (inventoryGem != null) return inventoryGem;

        // Then check available gems on the map
        return FindSuitableGemOnMap(pawn);
    }

    /// <summary>
    ///     Find a suitable gem in pawn's inventory for capturing specific spren types
    /// </summary>
    private static ThingWithComps? FindSuitableGemInInventory(Pawn pawn, List<SprenType> sprenTypes) {
        if (pawn.inventory?.innerContainer == null) return null;

        foreach (Verse.Thing thing in pawn.inventory.innerContainer) {
            if (thing is not ThingWithComps gem) continue;

            if (IsGemSuitableForSprenTypes(gem, sprenTypes)) {
                return gem;
            }
        }

        return null;
    }

    /// <summary>
    ///     Find a suitable gem in pawn's inventory for spren capture
    /// </summary>
    private static ThingWithComps? FindSuitableGemInInventory(Pawn pawn) {
        if (pawn.inventory?.innerContainer == null) return null;

        foreach (Verse.Thing thing in pawn.inventory.innerContainer) {
            if (thing is not ThingWithComps gem) continue;

            if (IsGemSuitableForCapture(gem)) {
                return gem;
            }
        }

        return null;
    }

    /// <summary>
    ///     Find suitable gems on the map for capturing specific spren types
    /// </summary>
    private static List<ThingWithComps> FindSuitableGemsOnMapForSprenTypes(Pawn pawn, List<SprenType> sprenTypes) {
        if (pawn.Map == null) return [];

        // Look for gems within reasonable hauling distance
        List<ThingWithComps> availableGems = pawn.Map.listerThings.AllThings
            .Where(thing => thing is ThingWithComps gem &&
                            IsGemSuitableForSprenTypes(gem, sprenTypes) &&
                            pawn.CanReach(thing, PathEndMode.ClosestTouch, Danger.Deadly) &&
                            !thing.IsForbidden(pawn)
            )
            .Cast<ThingWithComps>()
            .ToList();

        // Sort by distance (closest first)
        return availableGems.OrderBy(gem => pawn.Position.DistanceTo(gem.Position)).ToList();
    }

    /// <summary>
    ///     Find a suitable gem on the map for spren capture
    /// </summary>
    private static ThingWithComps? FindSuitableGemOnMap(Pawn pawn) {
        if (pawn.Map == null) return null;

        // Look for gems within reasonable hauling distance
        List<Verse.Thing> availableGems = pawn.Map.listerThings.AllThings
            .Where(thing => thing is ThingWithComps gem &&
                            IsGemSuitableForCapture(gem) &&
                            pawn.CanReach(thing, PathEndMode.ClosestTouch, Danger.Deadly) &&
                            !thing.IsForbidden(pawn)
            )
            .ToList();

        // Return closest gem
        if (availableGems.Count <= 0) return null;

        Verse.Thing? closestGem = GenClosest.ClosestThing_Global_Reachable(
            pawn.Position,
            pawn.Map,
            availableGems,
            PathEndMode.ClosestTouch,
            TraverseParms.For(pawn)
        );
        return closestGem as ThingWithComps;
    }

    /// <summary>
    ///     Check if a gem is suitable for capturing specific spren types
    /// </summary>
    private static bool IsGemSuitableForSprenTypes(ThingWithComps gem, List<SprenType> sprenTypes) {
        // Check if it has CompSprenContainer and no captured spren
        SprenContainer? sprenContainer = gem.TryGetComp<SprenContainer>();
        if (sprenContainer is null or { hasCapturedSpren: true }) return false;

        // Check if it has investiture
        InvestitureHolder? investiture = gem.TryGetComp<InvestitureHolder>();
        if (investiture == null || investiture.currentInvestiture <= 0) return false;

        // Check if this gem can capture any of the specified spren types
        return Enumerable.Any(sprenTypes, sprenType => LesserSprenCaptureSystem.CanGemCaptureSpren(gem, sprenType));
    }

    /// <summary>
    ///     Check if a gem is suitable for spren capture
    /// </summary>
    private static bool IsGemSuitableForCapture(ThingWithComps gem) {
        // Check if it has CompSprenContainer and no captured spren
        SprenContainer? sprenContainer = gem.TryGetComp<SprenContainer>();
        if (sprenContainer is { hasCapturedSpren: true }) return false;

        // Check if it has investiture
        InvestitureHolder? investiture = gem.TryGetComp<InvestitureHolder>();
        if (investiture == null || investiture.currentInvestiture <= 0) return false;

        // Check if any spren type can be captured with this gem
        return Enum.GetValues(typeof(SprenType))
            .Cast<SprenType>()
            .Any(sprenType => LesserSprenCaptureSystem.CanGemCaptureSpren(gem, sprenType));
    }

    /// <summary>
    ///     Give the pawn a job to capture a specific spren type
    /// </summary>
    private static void GiveSprenCaptureJob(Pawn pawn, IntVec3 cell, Map map, SprenType sprenType, ThingWithComps gem) {
        Verse.AI.Job captureJob = JobMaker.MakeJob(Defs.Cosmere_Roshar_CaptureSpren, cell, gem);
        captureJob.targetC = new LocalTargetInfo(new IntVec3((int)sprenType, 0, 0)); // Store spren type in targetC.x
        captureJob.count = 1; // Gem count for StartCarryThing
        pawn.jobs.TryTakeOrderedJob(captureJob);
    }

    /// <summary>
    ///     Give the pawn a job to capture any suitable spren
    /// </summary>
    private static void GiveGeneralSprenCaptureJob(Pawn pawn, IntVec3 cell, Map map, ThingWithComps gem) {
        Verse.AI.Job captureJob = JobMaker.MakeJob(Defs.Cosmere_Roshar_CaptureSpren, cell, gem);
        captureJob.targetC = new LocalTargetInfo(new IntVec3(-1, 0, 0)); // -1 indicates "any spren type"
        captureJob.count = 1; // Gem count for StartCarryThing
        pawn.jobs.TryTakeOrderedJob(captureJob);
    }
}