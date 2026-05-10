using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Cosmere.Lightweave.Runtime;
using RimWorld;
using Verse;

namespace Cosmere.Lightweave.LoadColony;

public static class SaveStatusInspector {
    public sealed class SaveStatus {
        public string FileName { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public DateTime LastWriteTime { get; init; }
        public long FileSizeBytes { get; init; }
        public string GameVersion { get; init; } = string.Empty;
        public SaveCompatibility Compatibility { get; init; }
        public ModMatchKind ModMatch { get; init; }
        public List<string> MissingModNames { get; init; } = new List<string>();
        public List<string> ExtraModNames { get; init; } = new List<string>();
        public SaveSidecarData? Sidecar { get; init; }
        public bool HasSidecar => Sidecar != null;
    }

    public enum SaveCompatibility {
        Unknown,
        Match,
        DifferentBuild,
        DifferentVersion,
        Incompatible,
    }

    public enum ModMatchKind {
        Unknown,
        Match,
        Mismatch,
    }

    private static readonly ConcurrentDictionary<string, SaveStatus> Cache = new ConcurrentDictionary<string, SaveStatus>();

    public static void InvalidateAll() {
        Cache.Clear();
    }

    public static void Invalidate(string fileName) {
        Cache.TryRemove(fileName, out _);
    }

    public static SaveStatus Inspect(SaveFileInfo file) {
        string key = file.FileInfo.FullName;
        if (Cache.TryGetValue(key, out SaveStatus cached) && cached.LastWriteTime == file.LastWriteTime) {
            return cached;
        }

        SaveStatus status = Compute(file);
        Cache[key] = status;
        return status;
    }

    private static SaveStatus Compute(SaveFileInfo file) {
        string fileName = Path.GetFileNameWithoutExtension(file.FileName);
        long size = 0;
        try {
            size = file.FileInfo.Length;
        }
        catch {
        }

        string version = string.Empty;
        SaveCompatibility compat = SaveCompatibility.Unknown;
        ModMatchKind match = ModMatchKind.Unknown;
        List<string> missing = new List<string>();
        List<string> extra = new List<string>();

        try {
            version = ScribeMetaHeaderUtility.GameVersionOf(file.FileInfo) ?? string.Empty;
            if (!string.IsNullOrEmpty(version)) {
                compat = ResolveCompatibility(version);
            }
        }
        catch (Exception ex) {
            LightweaveLog.Warning("Inspect version failed for " + fileName + ": " + ex.Message);
        }

        try {
            ReadModList(file.FileInfo, out List<string> ids, out List<string> names);
            EvaluateMods(ids, names, missing, extra);
            match = missing.Count == 0 && extra.Count == 0 ? ModMatchKind.Match : ModMatchKind.Mismatch;
        }
        catch (Exception ex) {
            LightweaveLog.Warning("Inspect mods failed for " + fileName + ": " + ex.Message);
        }

        SaveSidecarData? sidecar = SaveSidecar.Read(file.FileInfo.FullName);

        return new SaveStatus {
            FileName = fileName,
            DisplayName = sidecar?.ColonyName ?? fileName,
            LastWriteTime = file.LastWriteTime,
            FileSizeBytes = size,
            GameVersion = version,
            Compatibility = compat,
            ModMatch = match,
            MissingModNames = missing,
            ExtraModNames = extra,
            Sidecar = sidecar,
        };
    }

    private static SaveCompatibility ResolveCompatibility(string version) {
        try {
            int major = VersionControl.MajorFromVersionString(version);
            int minor = VersionControl.MinorFromVersionString(version);
            int build = VersionControl.BuildFromVersionString(version);

            if (major != VersionControl.CurrentMajor || minor != VersionControl.CurrentMinor) {
                if (BackCompatibility.IsSaveCompatibleWith(version)) {
                    return SaveCompatibility.DifferentVersion;
                }
                return SaveCompatibility.Incompatible;
            }
            if (build != VersionControl.CurrentBuild) {
                return SaveCompatibility.DifferentBuild;
            }
            return SaveCompatibility.Match;
        }
        catch {
            return SaveCompatibility.Unknown;
        }
    }

    private static void ReadModList(FileInfo fileInfo, out List<string> ids, out List<string> names) {
        ids = new List<string>();
        names = new List<string>();
        try {
            Scribe.loader.InitLoadingMetaHeaderOnly(fileInfo.FullName);
            ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.Map, logVersionConflictWarning: false);
            if (ScribeMetaHeaderUtility.loadedModIdsList != null) {
                ids.AddRange(ScribeMetaHeaderUtility.loadedModIdsList);
            }
            if (ScribeMetaHeaderUtility.loadedModNamesList != null) {
                names.AddRange(ScribeMetaHeaderUtility.loadedModNamesList);
            }
        }
        finally {
            Scribe.loader.FinalizeLoading();
        }
    }

    private static void EvaluateMods(List<string> savedIds, List<string> savedNames, List<string> missing, List<string> extra) {
        HashSet<string> activeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (ModContentPack pack in LoadedModManager.RunningModsListForReading) {
            activeIds.Add(pack.PackageId);
        }

        HashSet<string> savedSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < savedIds.Count; i++) {
            string id = savedIds[i];
            string display = i < savedNames.Count ? savedNames[i] : id;
            savedSet.Add(id);
            if (!activeIds.Contains(id)) {
                missing.Add(display);
            }
        }

        foreach (ModContentPack pack in LoadedModManager.RunningModsListForReading) {
            if (!savedSet.Contains(pack.PackageId)) {
                extra.Add(pack.Name);
            }
        }
    }
}
