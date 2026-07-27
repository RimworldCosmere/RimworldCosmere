using Cosmere.System.Roshar.Surgebinding.Ability.Illumination;
using HarmonyLib;
using Verse.Profile;

namespace Cosmere.System.Roshar.Patch.Surgebinding;

[HarmonyPatch(typeof(MemoryUtility), nameof(MemoryUtility.ClearAllMapsAndWorld))]
public static class LightweavingDecoyStateClearerPatch {
    [HarmonyPostfix]
    public static void Postfix() {
        LightweavingDecoyRegistry.Clear();
    }
}
