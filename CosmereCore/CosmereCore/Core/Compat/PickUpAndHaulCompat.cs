using System;
using System.Reflection;
using Concord;
using Cosmere.Core.Comp.Thing;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.Core.Compat;

/// <summary>
///     Teaches Pick Up And Haul about our <see cref="InnerStorage" /> containers.
/// </summary>
/// <remarks>
///     PUAH's <c>WorkGiver_HaulToInventory.JobOnThing</c> carries its own copy of vanilla's
///     destination dispatch instead of calling <c>HaulAIUtility.HaulToStorageJob</c>, so the patch
///     that teaches vanilla about our storage never reaches it. Its copy only knows an
///     <c>ISlotGroupParent</c> or a <c>Thing</c> with an inner container; ours is a ThingComp, so it
///     logged "Don't know how to handle HaulToStorageJob" and returned null. The pawn picked the
///     gems up, got no job, and stood there holding them - BetaHub #19.
///     <para>
///         PUAH already falls back to <c>HaulToStorageJob</c> in several places, and that path is
///         patched and works, so this just sends our destinations down it too rather than
///         rebuilding the job here.
///     </para>
///     <para>
///         Soft: resolved by name, and does nothing when PUAH is absent.
///     </para>
/// </remarks>
[StaticConstructorOnStartup]
public static class PickUpAndHaulCompat {
    private const string WorkGiverTypeName = "PickUpAndHaul.WorkGiver_HaulToInventory, PickUpAndHaul";

    static PickUpAndHaulCompat() {
        MethodInfo? target = ResolveJobOnThing();
        if (target == null) return;

        MethodInfo injection = typeof(PickUpAndHaulCompat).GetMethod(
            nameof(BeforeJobOnThing),
            BindingFlags.NonPublic | BindingFlags.Static
        )!;

        try {
            Patcher.Patch(target, injection, At.Head);
            Log.Info("Pick Up And Haul found; its hauler now understands Cosmere storage.");
        } catch (Exception e) {
            Log.Error($"Could not teach Pick Up And Haul about Cosmere storage: {e.Message}");
        }
    }

    private static MethodInfo? ResolveJobOnThing() {
        Type? workGiver = Type.GetType(WorkGiverTypeName, false);
        if (workGiver == null) return null;

        return workGiver.GetMethod(
            "JobOnThing",
            BindingFlags.Instance | BindingFlags.Public,
            null,
            [typeof(Pawn), typeof(Verse.Thing), typeof(bool)],
            null
        );
    }

    private static Control BeforeJobOnThing(Pawn pawn, Verse.Thing thing, bool forced, ControlHandle<Job> ch) {
        if (pawn?.Map == null || thing == null) return Control.Continue;

        StoragePriority priority = StoreUtility.CurrentStoragePriorityOf(thing, forced);
        if (!StoreUtility.TryFindBestBetterStorageFor(
                thing,
                pawn,
                pawn.Map,
                priority,
                pawn.Faction,
                out _,
                out IHaulDestination destination
            )) {
            return Control.Continue;
        }

        if (destination is not InnerStorage) return Control.Continue;

        ch.ReturnValue = HaulAIUtility.HaulToStorageJob(pawn, thing, forced)!;
        return Control.Cancel;
    }
}
