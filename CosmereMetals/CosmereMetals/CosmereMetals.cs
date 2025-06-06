using CosmereFramework;
using Verse;

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