using Concord;
using Cosmere.Core.Tab;
using RimWorld;
using Verse;

namespace Cosmere.Core.Patch.InnerStorage;

[Patch]
public abstract class StorageTabPatch : Verse.Thing {
    [Inject(At.Return, nameof(GetInspectTabs))]
    private void AfterGetInspectTabs(ControlHandle<IEnumerable<InspectTabBase>> ch) {
        Verse.Thing self = this;
        ch.ReturnValue = WithStorageTabs(ch.ReturnValue, self);
    }

    private static IEnumerable<InspectTabBase> WithStorageTabs(
        IEnumerable<InspectTabBase>? values,
        Verse.Thing instance
    ) {
        if (values != null) {
            foreach (InspectTabBase tab in values) {
                yield return tab;
            }
        }

        if (instance is not Pawn pawn) yield break;
        if (pawn.apparel == null) yield break;

        HashSet<string> seenTabNames = [];
        List<Apparel> apparel = pawn.apparel.WornApparel;
        for (int i = 0; i < apparel.Count; i++) {
            if (!apparel[i].TryGetComp(out Comp.Thing.InnerStorage comp)) continue;
            if (!seenTabNames.Add(comp.props.tabName)) continue;

            yield return new ITab_StorageWithInventory(comp.props.tabName, comp);
        }
    }
}
