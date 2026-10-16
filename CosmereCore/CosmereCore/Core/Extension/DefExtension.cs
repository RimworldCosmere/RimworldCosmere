namespace Cosmere.Core.Extension;

public static class DefExtension {
    /// <summary>
    ///     Compares by defName, not by reference. DefDatabase is keyed on the concrete type, so a
    ///     MetallicArtsMetalDef and the MetalDef it inherits are two objects sharing one defName.
    /// </summary>
    /// <returns>True when one of the given defs shares this def's defName.</returns>
    public static bool IsOneOf(this Verse.Def def, params Verse.Def[] defs) {
        return defs.Any(other => other != null && def.defName == other.defName);
    }

    public static IEnumerable<string> ParseDefName(this Verse.Def def) {
        return def.defName.Split('_');
    }
}
