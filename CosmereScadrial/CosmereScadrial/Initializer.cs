using HarmonyLib;
using Verse;

namespace CosmereScadrial {
    [StaticConstructorOnStartup]
    public static class Initializer {
        static Initializer() {
            new Harmony("cryptiklemur.cosmere.scadrial").PatchAll();
            Log.Message("[Cosmere] Harmony patches applied.");
        }
    }
}