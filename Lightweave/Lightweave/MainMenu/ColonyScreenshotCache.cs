using System;
using System.Collections.Generic;
using Cosmere.Lightweave.Runtime;
using UnityEngine;

namespace Cosmere.Lightweave.MainMenu;

internal static class ColonyScreenshotCache {
    private static readonly Dictionary<string, Texture2D?> Cache = new Dictionary<string, Texture2D?>();

    public static Texture2D? GetOrLoad(SaveMetadata.LatestSave? save) {
        if (save?.Sidecar == null || string.IsNullOrEmpty(save.Sidecar.ScreenshotBase64)) {
            return null;
        }

        string key = save.FileName + "|" + save.LastWriteTime.Ticks.ToString();
        if (Cache.TryGetValue(key, out Texture2D? cached)) {
            return cached;
        }

        Texture2D? tex = null;
        try {
            byte[] bytes = Convert.FromBase64String(save.Sidecar.ScreenshotBase64);
            tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
            if (!tex.LoadImage(bytes)) {
                UnityEngine.Object.Destroy(tex);
                tex = null;
            }
            else {
                tex.filterMode = FilterMode.Bilinear;
            }
        }
        catch (Exception ex) {
            LightweaveLog.Warning("Failed to decode colony screenshot for " + save.FileName + ": " + ex.Message);
            tex = null;
        }

        Cache[key] = tex;
        return tex;
    }
}
