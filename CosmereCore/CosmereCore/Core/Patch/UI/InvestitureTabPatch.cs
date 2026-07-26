using Cosmere.Core.Tab;
using Cosmere.Core.UI.Model;
using HarmonyLib;
using Verse;

namespace Cosmere.Core.Patch;

[HarmonyPatch(typeof(Verse.Thing), nameof(Verse.Thing.GetInspectTabs))]
public static class InvestitureTabPatch {
    private static ITab_Investiture? cachedTab;

    private static IEnumerable<InspectTabBase> Postfix(IEnumerable<InspectTabBase>? values, Verse.Thing __instance) {
        bool alreadyHasTab = false;
        if (values != null) {
            foreach (InspectTabBase tab in values) {
                if (tab is ITab_Investiture) alreadyHasTab = true;
                yield return tab;
            }
        }

        if (alreadyHasTab) yield break;
        if (__instance is not Pawn pawn) yield break;

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