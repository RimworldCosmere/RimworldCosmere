using System.Collections.Generic;
using System.Linq;
using CosmereCore.Pages;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CosmereCore.Patches
{
    [HarmonyPatch(typeof(PageUtility), nameof(PageUtility.StitchedPages))]
    public static class Patch_InsertShardSelection
    {
        private static void Prefix(ref IEnumerable<Page> pages)
        {
            var list = pages.ToList();

            if (list.Any(p => p is Page_SelectShards))
                return;

            list.Insert(1, new Page_SelectShards());
            pages = list;
        }
    }
}