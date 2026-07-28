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

        IReadOnlyList<IInvestitureProvider> all = InvestitureProviderRegistry.All;
        bool invested = false;
        for (int i = 0; i < all.Count; i++) {
            if (all[i].IsInvested(pawn)) {
                invested = true;
                break;
            }
        }

        if (!invested) yield break;

        cachedTab ??= new ITab_Investiture();
        yield return cachedTab;
    }
}
