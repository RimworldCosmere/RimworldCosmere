namespace Cosmere.Core.BetaHub;

public enum SubmitOutcome {
    Success,
    RateLimited,
    Rejected,
    Unreachable,
}

/// <summary>
///     Maps an HTTP status onto what the player is told. The error body's own status field is
///     documented as unreliable, so only the status line drives control flow.
/// </summary>
public static class BetaHubStatusMapper {
    private const string ReleasePermissionMarker = "permission to create releases";

    public static SubmitOutcome Map(long httpStatus) {
        if (httpStatus >= 200 && httpStatus < 400) return SubmitOutcome.Success;

        return httpStatus switch {
            403 => SubmitOutcome.RateLimited,
            422 => SubmitOutcome.Rejected,
            _ => SubmitOutcome.Unreachable,
        };
    }

    /// <summary>
    ///     Whether a 403 is the release-permission one rather than the daily cap.
    /// </summary>
    /// <remarks>
    ///     Both share a status code, so this is the one place the error body is read. It only
    ///     picks between two recovery paths, never between success and failure, so leaning on
    ///     an unstable message here cannot turn a failed report into a reported success.
    /// </remarks>
    public static bool IsMissingReleasePermission(long httpStatus, string? body) {
        return httpStatus == 403
               && body != null
               && body.IndexOf(ReleasePermissionMarker, global::System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public static string MessageKey(SubmitOutcome outcome) {
        return outcome switch {
            SubmitOutcome.Success => "CC_BetaHub_Result_Success",
            SubmitOutcome.RateLimited => "CC_BetaHub_Result_RateLimited",
            SubmitOutcome.Rejected => "CC_BetaHub_Result_Rejected",
            _ => "CC_BetaHub_Result_Unreachable",
        };
    }
}
