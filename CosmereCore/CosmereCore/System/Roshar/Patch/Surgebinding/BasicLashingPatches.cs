using Concord;
using Cosmere.System.Roshar.Surgebinding.Ability.Gravitation;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Cosmere.System.Roshar.Patch.Surgebinding;

[Patch]
public abstract class BasicLashingFloatPatch : Pawn {
    private const float FloatHeight = 0.5f;

    [Inject(At.Return, nameof(DrawPos))]
    private void AfterDrawPos(ControlHandle<Vector3> ch) {
        Pawn self = this;
        if (!BasicLashing.FlyingPawns.Contains(self)) return;

        ch.ReturnValue += new Vector3(0f, 0f, FloatHeight);
    }
}

[Patch]
public abstract class BasicLashingMeleeBlockPatch : Verb_MeleeAttack {
    [Inject(At.Head, nameof(TryCastShot))]
    private Control BeforeTryCastShot(ControlHandle<bool> ch) {
        Verb_MeleeAttack self = this;
        if (self.CurrentTarget.Thing is not Pawn targetPawn) return Control.Continue;
        if (!BasicLashing.FlyingPawns.Contains(targetPawn)) return Control.Continue;

        Pawn? attacker = self.CasterPawn;
        if (attacker == null) return Control.Continue;
        if (BasicLashing.FlyingPawns.Contains(attacker)) return Control.Continue;

        ch.ReturnValue = false;
        return Control.Cancel;
    }
}
