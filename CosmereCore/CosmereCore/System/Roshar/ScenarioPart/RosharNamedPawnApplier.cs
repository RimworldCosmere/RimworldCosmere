using System;
using Cosmere.Core;
using Cosmere.Core.ScenarioPart;
using Cosmere.System.Roshar.Extension;
using Verse;

namespace Cosmere.System.Roshar.ScenarioPart;

public sealed class RosharNamedPawnApplier : INamedPawnApplier {
    public void Apply(Pawn pawn, NamedPawnDef template) {
        if (template.radiantOrder == null) return;

        GeneDef? orderGeneDef = DefDatabase<GeneDef>.GetNamedSilentFail(template.radiantOrder);
        if (orderGeneDef == null) return;

        try {
            pawn.genes?.TryAddRadiantOrder(orderGeneDef, template.idealLevel);
        } catch (Exception ex) {
            Log.Warn($"NamedPawnApplier: Failed to add radiant order: {ex}");
        }
    }
}
