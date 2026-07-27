using Cosmere.Core.Tab;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.Core.Patch.InnerStorage;

[HarmonyPatch]
public static class StorageTabPatch {
    [HarmonyPatch(typeof(Verse.Thing), nameof(Verse.Thing.GetInspectTabs))]
    [HarmonyPostfix]
    public static IEnumerable<InspectTabBase> PostfixGetInspectTabs(
        IEnumerable<InspectTabBase>? values,
        Verse.Thing __instance
    ) {
        if (values != null) {
            foreach (InspectTabBase tab in values) {
                yield return tab;
            }
        }

        if (__instance is not Pawn pawn) yield break;
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
