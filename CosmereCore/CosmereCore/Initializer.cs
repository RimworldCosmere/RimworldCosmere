using HarmonyLib;
using Verse;

namespace CosmereCore
{
    [StaticConstructorOnStartup]
    public static class Initializer
    {
        static Initializer()
        {
            var harmony = new Harmony("cryptiklemur.cosmere.core");
            harmony.PatchAll();
            Log.Message("[Cosmere] Harmony patches applied.");
        }
    }
}