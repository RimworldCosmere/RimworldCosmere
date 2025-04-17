using System.Collections.Generic;
using System.Linq;
using CosmereCore.Pages;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereCore.Patches {
    [HarmonyPatch(typeof(PageUtility), nameof(PageUtility.StitchedPages))]
    public static class InsertShardSelection {
        private static void Prefix(ref IEnumerable<Page> pages) {
            var list = pages.ToList();

            if (list.Any(p => p is SelectShards)) {
                return;
            }

            list.Insert(1, new SelectShards());
            pages = list;
        }
    }
}