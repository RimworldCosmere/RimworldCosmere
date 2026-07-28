using System;
using Concord;
using Cosmere.System.Roshar.Comp.Thing;
using RimWorld;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.Patch.Spren;

[Patch]
public abstract class BondedSprenDraftGizmoPatch : Pawn_DraftController {
    protected BondedSprenDraftGizmoPatch(Pawn pawn) : base(pawn) { }

    [Inject(At.Return, nameof(ShowDraftGizmo))]
    private void AfterShowDraftGizmo(ControlHandle<bool> ch) {
        if (!ch.ReturnValue) return;
        if (pawn?.TryGetComp<SprenBond>() == null) return;
        ch.ReturnValue = false;
    }
}

[Patch]
public abstract class BondedSprenThreatDisabledPatch : Pawn {
    [Inject(At.Return, nameof(ThreatDisabled))]
    private void AfterThreatDisabled(ControlHandle<bool> ch) {
        if (ch.ReturnValue) return;
        Pawn self = this;
        if (self.TryGetComp<SprenBond>() == null) return;
        ch.ReturnValue = true;
    }
}

[Patch]
public abstract class BondedSprenStartJobFilterPatch : Pawn_JobTracker {
    private static readonly HashSet<string> BlockedJobDefNames = new HashSet<string>(StringComparer.Ordinal) {
        "Equip",
        "Wear",
        "DropEquipment",
        "RemoveApparel",
        "TakeInventory",
        "HaulToCell",
        "HaulToContainer",
        "AttackMelee",
        "AttackStatic",
        "FlagFromMortar",
        "Hunt",
        "Flee",
        "FleeAndCower",
        "PredatorHunt",
    };

    protected BondedSprenStartJobFilterPatch(Pawn newPawn) : base(newPawn) { }

    [Inject(At.Head, nameof(StartJob))]
    private Control BeforeStartJob(Verse.AI.Job newJob) {
        if (newJob?.def == null) return Control.Continue;
        if (!BlockedJobDefNames.Contains(newJob.def.defName)) return Control.Continue;
        if (pawn?.TryGetComp<SprenBond>() == null) return Control.Continue;

        return Control.Cancel;
    }
}
