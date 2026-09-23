using System;
using System.IO;
using Cosmere.Core.Framework;
using RimWorld;
using Steamworks;
using UnityEngine;
using Verse;
using Verse.Steam;

namespace Cosmere.Core.BetaHub;

/// <summary>
///     Reads the running game's build, hardware, mod list and log into a DiagnosticsFacts.
/// </summary>
public static class DiagnosticsBundle {
    public static DiagnosticsFacts Collect() {
        return new DiagnosticsFacts {
            Revision = BuildInfo.Revision,
            BuildTime = BuildInfo.BuildTime,
            SteamId = ReadSteamId(),
            SteamPersona = ReadSteamPersona(),
            GameVersion = VersionControl.CurrentVersionStringWithRev,
            OperatingSystem = SystemInfo.operatingSystem,
            GraphicsDevice = SystemInfo.graphicsDeviceName,
            SystemMemoryMb = SystemInfo.systemMemorySize,
            ActiveMods = ReadActiveMods(),
        };
    }

    public static string ReadLogTail() {
        try {
            string path = Application.consoleLogPath;
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return "(no log file found)";

            // The game holds the log open for writing, so a plain File.ReadAllText fails.
            using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using StreamReader reader = new StreamReader(stream);

            return LogTail.Take(reader.ReadToEnd(), LogTail.DefaultMaxChars);
        } catch (Exception ex) {
            Log.Warn($"Could not read the log for a BetaHub report: {ex.Message}");
            return "(log unreadable)";
        }
    }

    private static List<string> ReadActiveMods() {
        List<string> mods = [];
        foreach (ModContentPack pack in LoadedModManager.RunningModsListForReading) {
            mods.Add($"{pack.Name} [{pack.PackageId}]");
        }

        return mods;
    }

    private static string ReadSteamId() {
        try {
            return SteamManager.Initialized ? SteamUser.GetSteamID().m_SteamID.ToString() : "(not on steam)";
        } catch (Exception) {
            return "(unavailable)";
        }
    }

    private static string ReadSteamPersona() {
        try {
            return SteamManager.Initialized ? SteamFriends.GetPersonaName() : "(not on steam)";
        } catch (Exception) {
            return "(unavailable)";
        }
    }
}
