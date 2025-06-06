using CosmereCore.Tabs;
using CosmereFramework;
using CosmereScadrial.Inspector;
using Verse;

namespace CosmereScadrial {
    public class CosmereScadrial : Mod {
        public CosmereScadrial(ModContentPack content) : base(content) {
            InvestitureTabRegistry.Register(InvestitureTabScadrial.Draw);
        }

        public static CosmereScadrial CosmereScadrialMod => LoadedModManager.GetMod<CosmereScadrial>();
    }

    [StaticConstructorOnStartup]
    public static class ModStartup {
        static ModStartup() {
            Startup.Initialize("Cryptiklemur.Cosmere.Scadrial");
        }
    }
}