using CosmereFramework.Settings;
using Verse;

namespace CosmereFramework;

public sealed class CosmereSettings : ModSettings {
    public override void ExposeData() {
        foreach (CosmereModSettings modSettings in CosmereFramework.cosmereSettings) {
            modSettings.ExposeData();
        }
    }
}