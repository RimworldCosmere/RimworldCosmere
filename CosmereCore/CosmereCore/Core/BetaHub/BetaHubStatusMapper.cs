namespace Cosmere.Core.BetaHub;

public enum SubmitOutcome {
    Success,
    RateLimited,
    Rejected,
    Unreachable,
}

/// <summary>
///     Maps an HTTP status onto what the player is told. The error body's own status field is
///     documented as unreliable, so only the status line is read.
/// </summary>
public static class BetaHubStatusMapper {
    public static SubmitOutcome Map(long httpStatus) {
        if (httpStatus >= 200 && httpStatus < 400) return SubmitOutcome.Success;

        return httpStatus switch {
            403 => SubmitOutcome.RateLimited,
            422 => SubmitOutcome.Rejected,
            _ => SubmitOutcome.Unreachable,
        };
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
