using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Cosmere.Lightweave.Runtime;
using RimWorld;
using UnityEngine;
using Verse;

namespace Cosmere.Lightweave.LoadColony;

public static class SaveSidecar {
    public const string Extension = ".lwmeta.json";

    public static string PathFor(string saveFilePath) {
        string dir = Path.GetDirectoryName(saveFilePath) ?? string.Empty;
        string name = Path.GetFileNameWithoutExtension(saveFilePath);
        return Path.Combine(dir, name + Extension);
    }

    public static SaveSidecarData? Read(string saveFilePath) {
        string path = PathFor(saveFilePath);
        if (!File.Exists(path)) {
            return null;
        }

        try {
            string text = File.ReadAllText(path);
            return Parse(text);
        }
        catch (Exception ex) {
            LightweaveLog.Warning("SaveSidecar read failed for " + path + ": " + ex.Message);
            return null;
        }
    }

    public static void Write(string saveFilePath, SaveSidecarData data) {
        string path = PathFor(saveFilePath);
        try {
            string json = Serialize(data);
            File.WriteAllText(path, json);
        }
        catch (Exception ex) {
            LightweaveLog.Warning("SaveSidecar write failed for " + path + ": " + ex.Message);
        }
    }

    public static void Delete(string saveFilePath) {
        string path = PathFor(saveFilePath);
        if (File.Exists(path)) {
            try {
                File.Delete(path);
            }
            catch (Exception ex) {
                LightweaveLog.Warning("SaveSidecar delete failed for " + path + ": " + ex.Message);
            }
        }
    }

    public static SaveSidecarData CaptureFromCurrentGame() {
        Map? map = Find.CurrentMap;
        Game game = Current.Game;

        int colonists = 0;
        int animals = 0;
        int moodAvg = 0;
        float wealth = 0f;
        string biome = string.Empty;
        string climate = string.Empty;
        if (map != null) {
            colonists = map.mapPawns?.FreeColonistsCount ?? 0;
            wealth = map.wealthWatcher?.WealthTotal ?? 0f;
            biome = map.Biome?.LabelCap ?? string.Empty;
            climate = ResolveClimate(map);
            animals = CountColonyAnimals(map);
            moodAvg = AverageColonyMoodPercent(map);
        }

        int daysSurvived = (int)((game?.tickManager?.TicksGame ?? 0) / 60000);
        float threatScale = Find.Storyteller?.difficulty?.threatScale ?? 1f;
        string colonyName = Find.World?.info?.name ?? game?.World?.info?.name ?? string.Empty;
        bool permadeath = game?.Info?.permadeathMode ?? false;

        return new SaveSidecarData {
            Version = 1,
            ColonyName = colonyName,
            ColonistCount = colonists,
            AnimalCount = animals,
            Wealth = wealth,
            MoodAveragePercent = moodAvg,
            DaysSurvived = daysSurvived,
            Quadrum = ResolveQuadrum(map),
            InGameYear = ResolveYear(map),
            Biome = biome,
            Climate = climate,
            ThreatScale = threatScale,
            ActiveThreat = string.Empty,
            Permadeath = permadeath,
            CapturedAtUtc = DateTime.UtcNow,
            ScreenshotBase64 = string.Empty,
        };
    }


    public static void UpdateScreenshot(string saveFilePath, string base64) {
        SaveSidecarData? existing = Read(saveFilePath);
        if (existing == null) {
            return;
        }
        existing.ScreenshotBase64 = base64;
        Write(saveFilePath, existing);
    }

    private static int CountColonyAnimals(Map map) {
        try {
            int count = 0;
            IReadOnlyList<Pawn>? all = map.mapPawns?.AllPawnsSpawned;
            if (all == null) return 0;
            for (int i = 0; i < all.Count; i++) {
                Pawn p = all[i];
                if (p?.RaceProps?.Animal == true && p.Faction != null && p.Faction.IsPlayer) {
                    count++;
                }
            }
            return count;
        }
        catch {
            return 0;
        }
    }

    private static int AverageColonyMoodPercent(Map map) {
        try {
            IReadOnlyList<Pawn>? colonists = map.mapPawns?.FreeColonists;
            if (colonists == null || colonists.Count == 0) return 0;
            float total = 0f;
            int counted = 0;
            for (int i = 0; i < colonists.Count; i++) {
                Pawn p = colonists[i];
                if (p?.needs?.mood != null) {
                    total += p.needs.mood.CurLevelPercentage;
                    counted++;
                }
            }
            if (counted == 0) return 0;
            return Mathf.RoundToInt(total / counted * 100f);
        }
        catch {
            return 0;
        }
    }

    private static string ResolveClimate(Map map) {
        try {
            float temp = GenTemperature.GetTemperatureForCell(map.Center, map);
            return temp.ToString("0", CultureInfo.InvariantCulture) + "°C";
        }
        catch {
            return string.Empty;
        }
    }

    private static string ResolveQuadrum(Map? map) {
        if (map == null) {
            return string.Empty;
        }
        try {
            return GenLocalDate.Season(map).LabelCap();
        }
        catch {
            return string.Empty;
        }
    }

    private static int ResolveYear(Map? map) {
        if (map == null) {
            return 5500;
        }
        try {
            return GenLocalDate.Year(map);
        }
        catch {
            return 5500;
        }
    }

    private static string Serialize(SaveSidecarData data) {
        StringBuilder sb = new StringBuilder(512);
        sb.Append('{');
        AppendField(sb, "version", data.Version, first: true);
        AppendField(sb, "colonyName", data.ColonyName);
        AppendField(sb, "colonistCount", data.ColonistCount);
        AppendField(sb, "animalCount", data.AnimalCount);
        AppendField(sb, "wealth", data.Wealth);
        AppendField(sb, "moodAveragePercent", data.MoodAveragePercent);
        AppendField(sb, "daysSurvived", data.DaysSurvived);
        AppendField(sb, "quadrum", data.Quadrum);
        AppendField(sb, "inGameYear", data.InGameYear);
        AppendField(sb, "biome", data.Biome);
        AppendField(sb, "climate", data.Climate);
        AppendField(sb, "threatScale", data.ThreatScale);
        AppendField(sb, "activeThreat", data.ActiveThreat);
        AppendField(sb, "permadeath", data.Permadeath);
        AppendField(sb, "capturedAtUtc", data.CapturedAtUtc.ToString("o", CultureInfo.InvariantCulture));
        if (!string.IsNullOrEmpty(data.ScreenshotBase64)) {
            AppendField(sb, "screenshotBase64", data.ScreenshotBase64);
        }
        sb.Append('}');
        return sb.ToString();
    }

    private static void AppendField(StringBuilder sb, string key, object value, bool first = false) {
        if (!first) {
            sb.Append(',');
        }
        sb.Append('"').Append(key).Append("\":");
        switch (value) {
            case string s:
                sb.Append('"').Append(EscapeJson(s)).Append('"');
                break;
            case bool b:
                sb.Append(b ? "true" : "false");
                break;
            case int i:
                sb.Append(i.ToString(CultureInfo.InvariantCulture));
                break;
            case float f:
                sb.Append(f.ToString("0.##", CultureInfo.InvariantCulture));
                break;
            default:
                sb.Append('"').Append(EscapeJson(value?.ToString() ?? string.Empty)).Append('"');
                break;
        }
    }

    private static string EscapeJson(string s) {
        return s
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }

    private static SaveSidecarData? Parse(string json) {
        if (string.IsNullOrWhiteSpace(json)) {
            return null;
        }

        SaveSidecarParser parser = new SaveSidecarParser(json);
        return parser.Parse();
    }
}
