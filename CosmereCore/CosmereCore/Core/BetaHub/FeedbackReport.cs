namespace Cosmere.Core.BetaHub;

public sealed class FeedbackReport {
    public FeedbackKind Kind;
    public string Title = string.Empty;
    public string Description = string.Empty;
    public string StepsToReproduce = string.Empty;
    public FeedbackTarget Target = FeedbackTarget.Unknown;
    public string? DiscordUsername;
    public string? VideoUrl;
    public bool IncludeScreenshot = true;
}
