namespace Cosmere.Core.BetaHub;

/// <summary>
///     Decides whether the in-game feedback UI exists at all.
/// </summary>
public static class BetaHubGate {
    public const string BetaMarker = "-beta.";

    public static bool IsBetaRevision(string? revision) {
        return !string.IsNullOrEmpty(revision) && revision!.Contains(BetaMarker);
    }

    public static bool ShouldShow(string? revision, string? token, bool enabledInSettings) {
        return IsBetaRevision(revision) && !string.IsNullOrEmpty(token) && enabledInSettings;
    }
}
