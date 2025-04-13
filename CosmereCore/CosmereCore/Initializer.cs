using HarmonyLib;
using Verse;
using Log = CosmereFramework.Log;

namespace CosmereCore {
    [StaticConstructorOnStartup]
    public static class Initializer {
        static Initializer() {
            new Harmony("cryptiklemur.cosmere.core").PatchAll();
            Log.Verbose("Harmony patches applied.");
        }
    }
}