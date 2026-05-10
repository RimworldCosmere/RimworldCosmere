using System;
using System.Globalization;
using System.Text;

namespace Cosmere.Lightweave.LoadColony;

internal sealed class SaveSidecarParser {
    private readonly string source;
    private int index;

    public SaveSidecarParser(string source) {
        this.source = source;
        index = 0;
    }

    public SaveSidecarData? Parse() {
        SkipWhitespace();
        if (!Match('{')) {
            return null;
        }

        SaveSidecarData data = new SaveSidecarData();
        int version = 1;
        string colonyName = string.Empty;
        int colonistCount = 0;
        int animalCount = 0;
        float wealth = 0f;
        int moodAveragePercent = 0;
        int daysSurvived = 0;
        string quadrum = string.Empty;
        int inGameYear = 0;
        string biome = string.Empty;
        string climate = string.Empty;
        float threatScale = 1f;
        string activeThreat = string.Empty;
        bool permadeath = false;
        DateTime capturedAt = default;
        string screenshotBase64 = string.Empty;

        SkipWhitespace();
        if (Peek() != '}') {
            while (true) {
                SkipWhitespace();
                string key = ReadString();
                SkipWhitespace();
                if (!Match(':')) {
                    return null;
                }
                SkipWhitespace();
                switch (key) {
                    case "version":
                        version = ReadInt();
                        break;
                    case "colonyName":
                        colonyName = ReadString();
                        break;
                    case "colonistCount":
                        colonistCount = ReadInt();
                        break;
                    case "animalCount":
                        animalCount = ReadInt();
                        break;
                    case "wealth":
                        wealth = ReadFloat();
                        break;
                    case "moodAveragePercent":
                        moodAveragePercent = ReadInt();
                        break;
                    case "daysSurvived":
                        daysSurvived = ReadInt();
                        break;
                    case "quadrum":
                        quadrum = ReadString();
                        break;
                    case "inGameYear":
                        inGameYear = ReadInt();
                        break;
                    case "biome":
                        biome = ReadString();
                        break;
                    case "climate":
                        climate = ReadString();
                        break;
                    case "threatScale":
                        threatScale = ReadFloat();
                        break;
                    case "activeThreat":
                        activeThreat = ReadString();
                        break;
                    case "permadeath":
                        permadeath = ReadBool();
                        break;
                    case "capturedAtUtc":
                        string raw = ReadString();
                        DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out capturedAt);
                        break;
                    case "screenshotBase64":
                        screenshotBase64 = ReadString();
                        break;
                    default:
                        SkipValue();
                        break;
                }
                SkipWhitespace();
                if (!Match(',')) {
                    break;
                }
            }
        }
        SkipWhitespace();
        if (!Match('}')) {
            return null;
        }

        return new SaveSidecarData {
            Version = version,
            ColonyName = colonyName,
            ColonistCount = colonistCount,
            AnimalCount = animalCount,
            Wealth = wealth,
            MoodAveragePercent = moodAveragePercent,
            DaysSurvived = daysSurvived,
            Quadrum = quadrum,
            InGameYear = inGameYear,
            Biome = biome,
            Climate = climate,
            ThreatScale = threatScale,
            ActiveThreat = activeThreat,
            Permadeath = permadeath,
            CapturedAtUtc = capturedAt,
            ScreenshotBase64 = screenshotBase64,
        };
    }

    private char Peek() {
        return index < source.Length ? source[index] : '\0';
    }

    private bool Match(char c) {
        if (index < source.Length && source[index] == c) {
            index++;
            return true;
        }
        return false;
    }

    private void SkipWhitespace() {
        while (index < source.Length && char.IsWhiteSpace(source[index])) {
            index++;
        }
    }

    private string ReadString() {
        if (!Match('"')) {
            return string.Empty;
        }
        StringBuilder sb = new StringBuilder();
        while (index < source.Length && source[index] != '"') {
            if (source[index] == '\\' && index + 1 < source.Length) {
                char next = source[index + 1];
                switch (next) {
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 't': sb.Append('\t'); break;
                    case '"': sb.Append('"'); break;
                    case '\\': sb.Append('\\'); break;
                    default: sb.Append(next); break;
                }
                index += 2;
            }
            else {
                sb.Append(source[index]);
                index++;
            }
        }
        Match('"');
        return sb.ToString();
    }

    private int ReadInt() {
        int start = index;
        if (Peek() == '-') {
            index++;
        }
        while (index < source.Length && char.IsDigit(source[index])) {
            index++;
        }
        if (index == start) {
            return 0;
        }
        int.TryParse(source.Substring(start, index - start), NumberStyles.Integer, CultureInfo.InvariantCulture, out int value);
        return value;
    }

    private float ReadFloat() {
        int start = index;
        if (Peek() == '-') {
            index++;
        }
        while (index < source.Length && (char.IsDigit(source[index]) || source[index] == '.')) {
            index++;
        }
        if (index == start) {
            return 0f;
        }
        float.TryParse(source.Substring(start, index - start), NumberStyles.Float, CultureInfo.InvariantCulture, out float value);
        return value;
    }

    private bool ReadBool() {
        if (index + 4 <= source.Length && source.Substring(index, 4) == "true") {
            index += 4;
            return true;
        }
        if (index + 5 <= source.Length && source.Substring(index, 5) == "false") {
            index += 5;
            return false;
        }
        return false;
    }

    private void SkipValue() {
        SkipWhitespace();
        char c = Peek();
        if (c == '"') {
            ReadString();
        }
        else if (c == 't' || c == 'f') {
            ReadBool();
        }
        else if (c == '-' || char.IsDigit(c)) {
            ReadFloat();
        }
        else {
            while (index < source.Length && source[index] != ',' && source[index] != '}') {
                index++;
            }
        }
    }
}
