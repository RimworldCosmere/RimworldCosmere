using Cosmere.Core.InspectorTab;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Cosmere.Core.Patch.InnerStorage;

[HarmonyPatch]
public class PatchStorageTab {
    [HarmonyPatch(typeof(Verse.Thing), nameof(Verse.Thing.GetInspectTabs))]
    [HarmonyPostfix]
    public static IEnumerable<InspectTabBase> PostfixGetInspectTabs(
        IEnumerable<InspectTabBase> values,
        Verse.Thing __instance
    ) {
        foreach (InspectTabBase tab in values) {
            yield return tab;
        }

        if (__instance is not Pawn pawn) yield break;

        foreach (Apparel apparel in pawn.apparel.WornApparel) {
            if (!apparel.TryGetComp(out Comp.Thing.InnerStorage comp)) continue;

            yield return new StorageWithInventory(comp.props.tabName, comp);
        }
    }
}