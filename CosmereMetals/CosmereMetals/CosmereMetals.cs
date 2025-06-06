using CosmereFramework;
using HarmonyLib;
using Verse;
using static CosmereFramework.CosmereFramework;
using Log = CosmereFramework.Log;

namespace CosmereMetals {
    public class CosmereMetals(ModContentPack content) : Mod(content) {
        public static CosmereMetals CosmereMetalsMod => LoadedModManager.GetMod<CosmereMetals>();
    }

    [StaticConstructorOnStartup]
    public static class ModStartup {
        static ModStartup() {
            Startup.Initialize("Cryptiklemur.Cosmere.Metals");
        }
    }
}