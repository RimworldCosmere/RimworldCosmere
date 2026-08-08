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

    public Pawn? Held => held;

    public void Hold(Pawn kandra) {
        held = kandra;
    }

    public void Release() {
        held = null;
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
    }
}
