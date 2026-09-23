using System.Reflection;
using Cosmere.Core.Settings;
using Verse;

namespace Cosmere.Core;

public sealed class CosmereSettings : ModSettings {
    public override void ExposeData() {
        foreach (CosmereModSettings modSettings in Core.Mod.cosmereSettings) {
            modSettings.ExposeData();
        }
    }

    public static bool TryGetRaw(string modId, string key, out object? value) {
        CosmereModSettings? modSettings =
            Core.Mod.cosmereSettings.FirstOrDefault(m => modId.Equals($"Cosmere.{m.Name}"));

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
