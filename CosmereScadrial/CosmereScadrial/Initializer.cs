using HarmonyLib;
using Verse;
using Log = CosmereFramework.Log;

namespace CosmereScadrial {
    [StaticConstructorOnStartup]
    public static class Initializer {
        static Initializer() {
            new Harmony("cryptiklemur.cosmere.scadrial").PatchAll();
            Log.Verbose("Harmony patches applied.");
        }
    }
}