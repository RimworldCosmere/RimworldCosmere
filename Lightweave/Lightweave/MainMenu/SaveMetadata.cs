using System;
using System.IO;
using Cosmere.Lightweave.Settings;
using Verse;

namespace Cosmere.Lightweave.MainMenu;

public static class SaveMetadata {
    public sealed class LatestSave {
        public string FileName { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public DateTime LastWriteTime { get; init; }
        public bool Permadeath { get; init; }
        public LoadColony.SaveSidecarData? Sidecar { get; init; }
    }

    private static LatestSave? cached;
    private static DateTime cachedAt;
    private const double CacheSeconds = 5.0;

    public static LatestSave? Get() {
        if (!LightweaveMod.Settings.ParseSaveMetadata) {
            return null;
        }

        if (cached != null && (DateTime.UtcNow - cachedAt).TotalSeconds < CacheSeconds) {
            return cached;
        }

        try {
            string folder = GenFilePaths.SavedGamesFolderPath;
            if (!Directory.Exists(folder)) {
                cached = null;
                cachedAt = DateTime.UtcNow;
                return null;
            }

            FileInfo? newest = null;
            foreach (string path in Directory.GetFiles(folder, "*.rws")) {
                FileInfo fi = new FileInfo(path);
                if (newest == null || fi.LastWriteTime > newest.LastWriteTime) {
                    newest = fi;
                }
            }

            if (newest == null) {
                cached = null;
            }
            else {
                string fileName = Path.GetFileNameWithoutExtension(newest.Name);
                LoadColony.SaveSidecarData? sidecar = LoadColony.SaveSidecar.Read(newest.FullName);
                string display = !string.IsNullOrEmpty(sidecar?.ColonyName) ? sidecar!.ColonyName : fileName;
                cached = new LatestSave {
                    FileName = fileName,
                    DisplayName = display,
                    LastWriteTime = newest.LastWriteTime,
                    Permadeath = sidecar?.Permadeath ?? false,
                    Sidecar = sidecar,
                };
            }
        }
        catch (Exception ex) {
            Log.WarningOnce("Lightweave save scan failed: " + ex, 0x4C57_5341);
            cached = null;
        }

        cachedAt = DateTime.UtcNow;
        return cached;
    }

    public static string FormatRelative(DateTime when) {
        TimeSpan span = DateTime.Now - when;
        if (span.TotalMinutes < 1) {
            return "CL_MainMenu_Time_JustNow".Translate();
        }
        if (span.TotalHours < 1) {
            int m = Math.Max(1, (int)span.TotalMinutes);
            return "CL_MainMenu_Time_MinutesAgo".Translate(m.Named("COUNT"));
        }
        if (span.TotalDays < 1) {
            int h = Math.Max(1, (int)span.TotalHours);
            return "CL_MainMenu_Time_HoursAgo".Translate(h.Named("COUNT"));
        }
        int d = Math.Max(1, (int)span.TotalDays);
        return "CL_MainMenu_Time_DaysAgo".Translate(d.Named("COUNT"));
    }
}
