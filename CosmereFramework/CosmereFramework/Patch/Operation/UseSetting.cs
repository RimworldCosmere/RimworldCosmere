#nullable disable
using System;
using System.Xml;
using CosmereFramework.Extension;
using Verse;

namespace CosmereFramework.Patch.Operation;

public class UseSetting : PatchOperation {
    public PatchOperation apply;
    public string expect;
    public string key;
    public string modId;

    protected override bool ApplyWorker(XmlDocument xml) {
        if (!CosmereSettings.TryGetRaw(modId, key, out object settingValue)) {
            Log.Warning($"[Cosmere] PatchOperationUseSetting: Missing setting '{key}' in mod '{modId}'");
            return false;
        }

        string valueStr = settingValue switch {
            bool b => b.ToString().ToLowerInvariant(),
            _ => settingValue?.ToString() ?? "",
        };

        if (!string.Equals(valueStr, expect, StringComparison.OrdinalIgnoreCase))
            return true;

        return apply.ReplaceTokens(key, valueStr).Apply(xml);
    }
}