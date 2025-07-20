using Cosmere.Framework;
using Verse;

namespace Cosmere.Resources;

public class Mod(ModContentPack content) : Verse.Mod(content) {
    public static Mod modMod => LoadedModManager.GetMod<Mod>();
}

[StaticConstructorOnStartup]
public static class ModStartup {
    static ModStartup() {
        Startup.Initialize("Cryptiklemur.Cosmere.Metals");
    }
}