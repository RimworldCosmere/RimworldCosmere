using Cosmere.Framework.Settings;
using Verse;

namespace Cosmere.Framework;

public abstract class CosmereMod<TSettings> : Verse.Mod where TSettings : CosmereModSettings, new() {
    public CosmereMod(ModContentPack content, string mod) : base(content) {
        LongEventHandler.QueueLongEvent(
            () => Startup.Initialize($"CryptikLemur.Cosmere.{mod}"),
            "LoadCosmereMod",
            true,
            null
        );
    }

    public static TSettings Settings => Mod.GetModSettings<TSettings>();

    public override string SettingsCategory() {
        return "Cosmere";
    }
}