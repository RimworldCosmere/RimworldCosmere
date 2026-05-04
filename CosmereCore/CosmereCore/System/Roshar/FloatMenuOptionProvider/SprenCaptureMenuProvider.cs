using System;
using Cosmere.Core.Comp.Thing;
using Cosmere.System.Roshar.Comp.Thing;
using Cosmere.System.Roshar.LesserSpren.CaptureSystem;
using Cosmere.System.Roshar.LesserSpren.ParticleSystem;
using Cosmere.System.Roshar.LesserSpren.SprenController;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.FloatMenuOptionProvider;

public class SprenCaptureMenuProvider : RimWorld.FloatMenuOptionProvider {
    private static readonly List<SprenType> AllSprenTypes =
        Enum.GetValues(typeof(SprenType)).Cast<SprenType>().ToList();

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

    private static List<FloatMenuOption> GetSprenCaptureOptionsForCell(IntVec3 cell, Map map, Pawn pawn) {
        List<FloatMenuOption> options = [];

        List<BaseSprenController> capturableSpren = LesserSprenCaptureSystem.GetCapturableSprenWithinRadius(
            cell,
            map
        );

        if (capturableSpren.Count == 0) {
            return [];
        }

        List<ThingWithComps> suitableGems = FindSuitableGemsForSprenTypes(pawn, capturableSpren);

        if (suitableGems.Count == 0) {
            return [];
        }

        ThingWithComps suitableGem = suitableGems.First();
        bool gemInInventory = pawn.inventory?.innerContainer?.Contains(suitableGem) == true;

        foreach (BaseSprenController? controller in capturableSpren) {
            SprenType sprenType = controller.sprenType;

            string sprenName = controller.GetLocalizedName();

            if (!LesserSprenCaptureSystem.CanGemCaptureSpren(suitableGem, sprenType)) {
                continue;
            }

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

            if (!pawn.CanReach(cell, PathEndMode.OnCell, Danger.Deadly)) {
                options.Add(
                    new FloatMenuOption(
                        "SprenCapture_CannotReach".Translate(sprenName),
                        null
                    )
                );
                continue;
            }

            string optionText = gemInInventory
                ? "SprenCapture_CaptureSpren".Translate(sprenName)
                : "SprenCapture_CaptureSprenPickupGem".Translate(sprenName, suitableGem.Label);

            options.Add(new FloatMenuOption(optionText, () => GiveSprenCaptureJob(pawn, cell, sprenType, suitableGem)));
        }

        return options;
    }

    private static List<ThingWithComps> FindSuitableGemsForSprenTypes(
        Pawn pawn,
        List<BaseSprenController> controllers
    ) {
        List<ThingWithComps> suitableGems = [];

        ThingWithComps? inventoryGem = FindSuitableGemInInventory(pawn, controllers.Select(x => x.sprenType).ToList());
        if (inventoryGem != null) {
            suitableGems.Add(inventoryGem);
            return suitableGems;
        }

        List<ThingWithComps> mapGems = FindSuitableGemsOnMapForSprenTypes(
            pawn,
            controllers.Select(x => x.sprenType).ToList()
        );
        suitableGems.AddRange(mapGems);

        return suitableGems;
    }

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

    private static List<ThingWithComps> FindSuitableGemsOnMapForSprenTypes(Pawn pawn, List<SprenType> sprenTypes) {
        if (pawn.Map == null) return [];

        List<ThingWithComps> availableGems = pawn.Map.listerThings.AllThings
            .Where(thing => thing is ThingWithComps gem &&
                            IsGemSuitableForSprenTypes(gem, sprenTypes) &&
                            pawn.CanReach(thing, PathEndMode.ClosestTouch, Danger.Deadly) &&
                            !thing.IsForbidden(pawn)
            )
            .Cast<ThingWithComps>()
            .ToList();

        return availableGems.OrderBy(gem => pawn.Position.DistanceTo(gem.Position)).ToList();
    }

    private static bool IsGemSuitableForSprenTypes(ThingWithComps gem, List<SprenType> sprenTypes) {
        SprenContainer? sprenContainer = gem.TryGetComp<SprenContainer>();
        if (sprenContainer is null or { hasCapturedSpren: true }) return false;

        InvestitureHolder? investiture = gem.TryGetComp<InvestitureHolder>();
        if (investiture == null || investiture.currentInvestiture <= 0) return false;

        return Enumerable.Any(sprenTypes, sprenType => LesserSprenCaptureSystem.CanGemCaptureSpren(gem, sprenType));
    }

    private static void GiveSprenCaptureJob(Pawn pawn, IntVec3 cell, SprenType sprenType, ThingWithComps gem) {
        Verse.AI.Job captureJob = JobMaker.MakeJob(JobDefOf.Cosmere_Roshar_CaptureSpren, cell, gem);
        captureJob.targetC = new LocalTargetInfo(new IntVec3((int)sprenType, 0, 0));
        captureJob.count = 1;
        pawn.jobs.TryTakeOrderedJob(captureJob);
    }
}
