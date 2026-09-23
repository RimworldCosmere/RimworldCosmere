using Cosmere.Core.Framework;
using Cosmere.Core.Settings;

namespace Cosmere.Core.BetaHub;

/// <summary>
///     Endpoint addresses for the Cosmere BetaHub project, and the one place that answers
///     whether the feedback UI should exist in this build.
/// </summary>
public static class BetaHubConfig {
    public const string ProjectId = "pr-4628785616";
    public const string BaseUrl = "https://app.betahub.io";
    public const string SourceTag = "rimworld_ingame";

    public static string ProjectUrl => $"{BaseUrl}/projects/{ProjectId}";

    public static string IssuesUrl => $"{ProjectUrl}/issues.json";

    public static string FeatureRequestsUrl => $"{ProjectUrl}/feature_requests.json";

    public static string LogFilesUrl(string issueId) => $"{ProjectUrl}/issues/{issueId}/log_files.json";

    public static string ScreenshotsUrl(string issueId) => $"{ProjectUrl}/issues/{issueId}/screenshots.json";

    public static bool ShouldShowFeedbackUi => BetaHubGate.ShouldShow(
        BuildInfo.Revision,
        BetaHubToken.Value,
        Mod.GetModSettings<CoreModSettings>().showFeedbackButtons
    );
}
