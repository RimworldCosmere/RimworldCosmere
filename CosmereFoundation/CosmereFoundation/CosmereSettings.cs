using System.Reflection;
using Cosmere.Foundation.Settings;
using Verse;

namespace Cosmere.Foundation;

public sealed class CosmereSettings : ModSettings {
    public override void ExposeData() {
        foreach (CosmereModSettings modSettings in Foundation.Mod.cosmereSettings) {
            modSettings.ExposeData();
        }
    }

    public static bool TryGetRaw(string modId, string key, out object? value) {
        CosmereModSettings? modSettings =
            Foundation.Mod.cosmereSettings.FirstOrDefault(m => m.GetType().Assembly.GetName().Name.Contains(modId));

        value = null;
        if (modSettings == null) return false;

        FieldInfo? field = modSettings.GetType()
            .GetField(key, BindingFlags.Public | BindingFlags.Instance);
        if (field == null) {
            return false;
        }

        value = field.GetValue(modSettings);
        return true;
    }
}