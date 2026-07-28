using System;
using Concord;
using Cosmere.System.Roshar.Comp.Thing;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Cosmere.System.Roshar.Patch.Spren;

[Patch]
public abstract class BondedSprenColonistBarPatch : ColonistBar {
    [InjectField("cachedEntries")]
    private readonly List<Entry> cachedEntries = null!;

    [Inject(At.Return, "CheckRecacheEntries")]
    private void AfterCheckRecacheEntries() {
        List<Entry>? entries = cachedEntries;
        if (entries == null) return;

        for (int i = entries.Count - 1; i >= 0; i--) {
            Pawn? pawn = entries[i].pawn;
            if (pawn != null && SprenPatchUtil.IsSprenOrDecoy(pawn)) entries.RemoveAt(i);
        }
    }
}

[Patch]
public abstract class BondedSprenHostilityPatch : Pawn_PlayerSettings {
    [InjectField("pawn")]
    private readonly Pawn trackedPawn = null!;

    protected BondedSprenHostilityPatch(Pawn pawn) : base(pawn) { }

    [Inject(At.Return, nameof(UsesConfigurableHostilityResponse))]
    private void AfterUsesConfigurableHostilityResponse(ControlHandle<bool> ch) {
        if (trackedPawn?.TryGetComp<SprenBond>() != null) {
            ch.ReturnValue = false;
        }
    }
}

[Patch(typeof(CaravanFormingUtility))]
public static class BondedSprenCaravanPatch {
    [Inject(At.Return, nameof(CaravanFormingUtility.AllSendablePawns))]
    private static void AfterAllSendablePawns(ControlHandle<List<Pawn>> ch) {
        SprenPatchUtil.RemoveSprenAndDecoys(ch.ReturnValue);
    }
}

[Patch]
public abstract class BondedSprenDisableWorkPatch : Pawn {
    [Inject(At.Return, nameof(GetDisabledWorkTypes))]
    private void AfterGetDisabledWorkTypes(ControlHandle<List<WorkTypeDef>> ch) {
        Pawn self = this;
        if (self.TryGetComp<SprenBond>() == null) return;

        List<WorkTypeDef> allTypes = DefDatabase<WorkTypeDef>.AllDefsListForReading;
        for (int i = 0; i < allTypes.Count; i++) {
            if (!ch.ReturnValue.Contains(allTypes[i])) {
                ch.ReturnValue.Add(allTypes[i]);
            }
        }
    }
}

[Patch]
public abstract class BondedSprenWorkTabPatch : PawnTable {
    [InjectField("cachedPawns")]
    private readonly List<Pawn> cachedPawns = null!;

    protected BondedSprenWorkTabPatch(
        PawnTableDef def,
        Func<IEnumerable<Pawn>> pawnsGetter,
        int uiWidth,
        int uiHeight
    ) : base(def, pawnsGetter, uiWidth, uiHeight) { }

    [Inject(At.Return, "RecachePawns")]
    private void AfterRecachePawns() {
        List<Pawn>? pawns = cachedPawns;
        if (pawns == null) return;

        SprenPatchUtil.RemoveSprenAndDecoys(pawns);
    }
}
