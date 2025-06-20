using CosmereFramework;
using Verse;

namespace CosmereResources;

public class CosmereResources(ModContentPack content) : Mod(content) {
    public static CosmereResources cosmereResourcesMod => LoadedModManager.GetMod<CosmereResources>();
}

[StaticConstructorOnStartup]
public static class ModStartup {
    static ModStartup() {
        Startup.Initialize("Cryptiklemur.Cosmere.Metals");
    }
}