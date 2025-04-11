using CosmereCore.Tabs;
using CosmereScadrial.Inspector;
using HarmonyLib;
using Verse;

namespace CosmereScadrial
{
    [StaticConstructorOnStartup]
    public static class Initializer
    {
        static Initializer()
        {
            var harmony = new Harmony("cryptiklemur.cosmere.scadrial");
            harmony.PatchAll();
            Log.Message("[Cosmere] Harmony patches applied.");
            InvestitureTabRegistry.Register(InvestitureTab_Scadrial.Draw);
        }
    }
}