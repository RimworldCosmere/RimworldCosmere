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
/// </remarks>
public class CompKandraShapePair : ThingComp {
    private Pawn? held;

    /// <summary>What the kandra was holding and wearing, so it goes back the same way.</summary>
    private List<Verse.Thing> wasEquipped = [];
    private List<Verse.Thing> wasWorn = [];

    public Pawn? Held => held;

    public List<Verse.Thing> WasEquipped => wasEquipped;

    public List<Verse.Thing> WasWorn => wasWorn;

    public void RememberGear(IEnumerable<Verse.Thing> equipped, IEnumerable<Verse.Thing> worn) {
        wasEquipped = [.. equipped];
        wasWorn = [.. worn];
    }

    public void Hold(Pawn kandra) {
        held = kandra;
    }

    public void Release() {
        held = null;
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

        if (held == null) return;

        Pawn kandra = held;
        held = null;

        GenSpawn.Spawn(kandra, parent.Position, prevMap);
        kandra.Kill(dinfo);
    }

    public override IEnumerable<Verse.Gizmo> CompGetGizmosExtra() {
        if (held == null) yield break;
        if (parent.Faction != Faction.OfPlayer) yield break;

        yield return new Command_Action {
            defaultLabel = "CS_Kandra_LeaveAnimal".Translate(),
            defaultDesc = "CS_Kandra_LeaveAnimalDesc".Translate(),
            icon = TexCommand.Draft,
            action = () => KandraAnimalShape.Revert((Pawn)parent),
        };
    }

    public override string CompInspectStringExtra() {
        if (held == null) return string.Empty;

        return "CS_Kandra_WearingAnimal".Translate(held.Name?.ToStringShort.Named("PAWN") ?? string.Empty.Named("PAWN"))
            .Resolve();
    }

    public override void PostExposeData() {
        base.PostExposeData();
        Scribe_Deep.Look(ref held, "heldKandra");

        // By reference: the things themselves live in this pawn's inventory.
        Scribe_Collections.Look(ref wasEquipped, "wasEquipped", LookMode.Reference);
        Scribe_Collections.Look(ref wasWorn, "wasWorn", LookMode.Reference);
        wasEquipped ??= [];
        wasWorn ??= [];
    }
}
