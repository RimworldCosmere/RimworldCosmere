using Concord;
using Cosmere.Core.Tab;
using Cosmere.Core.UI.Model;
using Verse;

namespace Cosmere.Core.Patch;

[Patch]
public abstract class InvestitureTabPatch : Verse.Thing {
    private static ITab_Investiture? cachedTab;

    [Inject(At.Return, nameof(GetInspectTabs))]
    private void AfterGetInspectTabs(ControlHandle<IEnumerable<InspectTabBase>> ch) {
        Verse.Thing self = this;
        ch.ReturnValue = WithInvestitureTab(ch.ReturnValue, self);
    }

    private static IEnumerable<InspectTabBase> WithInvestitureTab(
        IEnumerable<InspectTabBase>? values,
        Verse.Thing instance
    ) {
        bool alreadyHasTab = false;
        if (values != null) {
            foreach (InspectTabBase tab in values) {
                if (tab is ITab_Investiture) alreadyHasTab = true;
                yield return tab;
            }
        }

        if (alreadyHasTab) yield break;
        if (instance is not Pawn pawn) yield break;

        // The Codex used to appear only for Invested pawns. Connection is chrome above the
        // system switcher and every pawn has one, so the tab now opens for anyone whose
        // Connection the player is entitled to read.
        if (!ShowsConnection(pawn)) yield break;

        cachedTab ??= new ITab_Investiture();
        yield return cachedTab;
    }

    /// <summary>
    ///     Whose Connection the player may read: their own colonists and their prisoners. Every
    ///     pawn has a Connection, but a raider's is not the player's business.
    /// </summary>
    private static bool ShowsConnection(Pawn pawn) {
        if (!pawn.RaceProps.Humanlike) return false;

        return pawn.IsColonist || pawn.IsSlaveOfColony || pawn.IsPrisonerOfColony;
    }
}
