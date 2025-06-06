using CosmereFramework;
using Verse;

namespace CosmereCore {
    public class CosmereCore(ModContentPack content) : Mod(content) {
        public static CosmereCore CosmereCoreMod => LoadedModManager.GetMod<CosmereCore>();
    }

    [StaticConstructorOnStartup]
    public static class ModStartup {
        static ModStartup() {
            Startup.Initialize("Cryptiklemur.Cosmere.Core");
        }
    }
}