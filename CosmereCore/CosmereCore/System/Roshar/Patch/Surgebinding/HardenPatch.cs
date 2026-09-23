using Concord;
using Cosmere.System.Roshar.Surgebinding.Ability.Tension;
using UnityEngine;
using Verse;

namespace Cosmere.System.Roshar.Patch.Surgebinding;

[Patch]
public abstract class HardenMaxHpPatch : Verse.Thing {
    [Inject(At.Return, nameof(MaxHitPoints))]
    private void AfterMaxHitPoints(ControlHandle<int> ch) {
        Verse.Thing self = this;
        if (self is not Building building) return;
        if (!Harden.TryGetHardenMultiplier(building, out float multiplier)) return;

        ch.ReturnValue = Mathf.RoundToInt(ch.ReturnValue * (1f + multiplier));
    }
}
