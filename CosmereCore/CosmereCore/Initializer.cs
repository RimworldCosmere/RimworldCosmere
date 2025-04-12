using HarmonyLib;
using Verse;

namespace CosmereCore {
    [StaticConstructorOnStartup]
    public static class Initializer {
        static Initializer() {
            new Harmony("cryptiklemur.cosmere.core").PatchAll();
            Log.Message("[Cosmere] Harmony patches applied.");
        }
    }
}