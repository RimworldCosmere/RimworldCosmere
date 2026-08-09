using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Cosmere.System.Scadrial.Kandra;

public class CompProperties_KandraShapePair : CompProperties {
    public CompProperties_KandraShapePair() {
        compClass = typeof(CompKandraShapePair);
    }
}

/// <summary>
///     The kandra that is currently wearing this animal.
/// </summary>
/// <remarks>
///     Holding the pawn here rather than leaving it on the map is what makes the swap reversible.
///     It is despawned but not destroyed, so its skills, hediffs, relationships and remembered
///     forms all survive the trip and come back with it.
///     <para>
///         It is held in a <see cref="ThingOwner" /> rather than a plain field so the game can
///         still find it. A despawned pawn with no holder has a null <c>MapHeld</c>, and anything
///         that starts from the map - the social tab most visibly - sees nobody at all.
///     </para>
/// </remarks>
public class CompKandraShapePair : ThingComp, IThingHolder {
    private ThingOwner<Pawn> inside;

    /// <summary>What the kandra was holding and wearing, so it goes back the same way.</summary>
    private List<Verse.Thing> wasEquipped = [];
    private List<Verse.Thing> wasWorn = [];

    public CompKandraShapePair() {
        inside = new ThingOwner<Pawn>(this, true);
    }

    public Pawn? Held => inside.Count > 0 ? inside[0] : null;

    public List<Verse.Thing> WasEquipped => wasEquipped;

    public List<Verse.Thing> WasWorn => wasWorn;

    /// <summary>
    ///     The shape itself, so the kandra's holder chain reaches the map it is standing on.
    /// </summary>
    /// <remarks>
    ///     Explicit, because ThingComp already has a ParentHolder that means something else: the
    ///     holder of the shape, which is nothing at all while the shape is walking around.
    /// </remarks>
    IThingHolder IThingHolder.ParentHolder => (IThingHolder)parent;

    public void GetChildHolders(List<IThingHolder> outChildren) {
        ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
    }

    public ThingOwner GetDirectlyHeldThings() {
        return inside;
    }

    public void RememberGear(IEnumerable<Verse.Thing> equipped, IEnumerable<Verse.Thing> worn) {
        wasEquipped = [.. equipped];
        wasWorn = [.. worn];
    }

    public void Hold(Pawn kandra) {
        inside.TryAddOrTransfer(kandra, false);
    }

    public void Release() {
        Pawn? kandra = Held;
        if (kandra != null) inside.Remove(kandra);

        wasEquipped = [];
        wasWorn = [];
    }

    /// <summary>
    ///     Killing the animal kills the kandra inside it.
    /// </summary>
    /// <remarks>
    ///     The alternative is a colonist who quietly stops existing when their dog dies, with no
    ///     corpse and no death letter. Better to put the body on the ground where the player can
    ///     see what happened.
    /// </remarks>
    public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null) {
        base.Notify_Killed(prevMap, dinfo);

        Pawn? kandra = Held;
        if (kandra == null) return;

        inside.Remove(kandra);

        GenSpawn.Spawn(kandra, parent.Position, prevMap);
        kandra.Kill(dinfo);
    }

    public override IEnumerable<Verse.Gizmo> CompGetGizmosExtra() {
        if (Held == null) yield break;
        if (parent.Faction != Faction.OfPlayer) yield break;

        yield return new Command_Action {
            defaultLabel = "CS_Kandra_LeaveAnimal".Translate(),
            defaultDesc = "CS_Kandra_LeaveAnimalDesc".Translate(),
            icon = TexCommand.Draft,
            action = () => KandraAnimalShape.Revert((Pawn)parent),
        };
    }

    public override string CompInspectStringExtra() {
        Pawn? kandra = Held;
        if (kandra == null) return string.Empty;

        return "CS_Kandra_WearingAnimal".Translate(kandra.Name?.ToStringShort.Named("PAWN") ?? string.Empty.Named("PAWN"))
            .Resolve();
    }

    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_Deep.Look(ref inside, "inside", this);
        inside ??= new ThingOwner<Pawn>(this, true);

        // By reference: the things themselves live in this pawn's inventory.
        Scribe_Collections.Look(ref wasEquipped, "wasEquipped", LookMode.Reference);
        Scribe_Collections.Look(ref wasWorn, "wasWorn", LookMode.Reference);
        wasEquipped ??= [];
        wasWorn ??= [];
    }
}
