using Concord;
using Cosmere.System.Scadrial.Kandra;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Patch.Kandra;

/// <summary>
///     Stops the game offering a bionic arm to something with paws.
/// </summary>
/// <remarks>
///     Fixing the Torso lookups made surgery work on a shaped kandra, which is what lets a spike
///     come out of one. It also opened the full human operations list on a wolf. The whole list is
///     hidden rather than filtered: a partly-filtered human surgery menu on a dog reads worse than
///     an empty one, and the kandra can be told to change back first.
/// </remarks>
[Patch(typeof(HealthCardUtility))]
public static class ShapedSurgeryPatch {
    [Inject(At.Head, "DrawMedOperationsTab")]
    private static Control BeforeDrawMedOperationsTab(
        Verse.Thing thingForMedBills,
        float curY,
        ControlHandle<float> ch
    ) {
        if (thingForMedBills is not Pawn pawn) return Control.Continue;
        if (KandraShapeGraphicUtility.WornKind(pawn) == null) return Control.Continue;

        // hand back curY unchanged when skipping the draw, or the health tabs layout collapses below it
        ch.ReturnValue = curY;

        return Control.Cancel;
    }
}

/// <summary>
///     Stops the auto-dress job putting a parka on a wolf.
/// </summary>
/// <remarks>
///     One static covering fourteen callers, <c>JobGiver_OptimizeApparel</c> among them. Only new
///     apparel is affected; anything already worn went into the pack when the shape went on.
/// </remarks>
[Patch(typeof(ApparelUtility))]
public static class ShapedApparelPatch {
    [Inject(At.Return, nameof(ApparelUtility.HasPartsToWear))]
    private static void AfterHasPartsToWear(Pawn p, ThingDef apparel, ControlHandle<bool> ch) {
        if (!ch.ReturnValue) return;
        if (KandraShapeGraphicUtility.WornKind(p) == null) return;

        ch.ReturnValue = false;
    }
}
