using System.Reflection;
using CosmereFramework.Settings;
using Verse;

namespace CosmereFramework;

public sealed class CosmereSettings : ModSettings {
    public override void ExposeData() {
        foreach (CosmereModSettings modSettings in CosmereFramework.cosmereSettings) {
            modSettings.ExposeData();
        }
    }

    public static bool TryGetRaw(string modId, string key, out object? value) {
        CosmereModSettings? modSettings =
            CosmereFramework.cosmereSettings.FirstOrDefault(m => m.GetType().Assembly.GetName().Name.Contains(modId));

        value = null;
        if (modSettings == null) return false;

        FieldInfo? field = modSettings.GetType()
            .GetField(key, BindingFlags.Public | BindingFlags.Instance);
        if (field == null)
            return false;

        value = field.GetValue(modSettings);
        return true;
    }
}