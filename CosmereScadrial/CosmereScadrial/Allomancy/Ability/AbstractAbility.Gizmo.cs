using System.Collections.Generic;
using CosmereScadrial.Extension;

namespace CosmereScadrial.Allomancy.Ability;

public abstract partial class AbstractAbility {
    public override bool GizmoDisabled(out string reason) {
        if (!atLeastBurning) return base.GizmoDisabled(out reason);

        reason = "";
        return false;
    }

    public override IEnumerable<Verse.Command> GetGizmos() {
        yield break;
    }

    public new bool GizmosVisible() {
        if (!base.GizmosVisible()) return false;

        if (!def.defName.Contains("Cosmere_Scadrial_Ability_Compound")) return true;

        return pawn.genes.HasAllomanticGeneForMetal(metal) && pawn.genes.HasFeruchemicGeneForMetal(metal);
    }
}