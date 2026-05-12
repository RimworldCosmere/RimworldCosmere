using System;

namespace Cosmere.Lightweave.LoadColony;

public sealed class SaveSidecarData {
    public int Version { get; init; } = 1;
    public string ColonyName { get; init; } = string.Empty;
    public int ColonistCount { get; init; }
    public int AnimalCount { get; init; }
    public float Wealth { get; init; }
    public int MoodAveragePercent { get; init; }
    public int DaysSurvived { get; init; }
    public string Quadrum { get; init; } = string.Empty;
    public int InGameYear { get; init; }
    public string Biome { get; init; } = string.Empty;
    public string Climate { get; init; } = string.Empty;
    public float ThreatScale { get; init; } = 1f;
    public string ActiveThreat { get; init; } = string.Empty;
    public bool Permadeath { get; init; }
    public DateTime CapturedAtUtc { get; init; }
    public string ScreenshotBase64 { get; set; } = string.Empty;
}
