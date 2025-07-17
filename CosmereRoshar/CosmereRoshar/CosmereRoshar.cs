#nullable disable
using CosmereFramework;
using Verse;

namespace CosmereRoshar;

public class CosmereRoshar(ModContentPack content) : Mod(content);

[StaticConstructorOnStartup]
public static class ModStartup {
    static ModStartup() {
        Startup.Initialize("Cryptiklemur.Cosmere.Roshar");
    }
}