using System;

namespace Cosmere.Core.BetaHub;

/// <summary>
///     Mirrors min_description_length and min_suggestion_length from the BetaHub project so
///     the dialog can refuse a submit the server would reject with a 422.
/// </summary>
public static class FeedbackValidator {
    public const int MinBugDescriptionLength = 50;
    public const int MinSuggestionDescriptionLength = 80;

    public static int MinDescriptionLength(FeedbackKind kind) {
        return kind == FeedbackKind.Suggestion ? MinSuggestionDescriptionLength : MinBugDescriptionLength;
    }

    public static int RemainingCharacters(FeedbackKind kind, string? description) {
        int length = description?.Trim().Length ?? 0;

        return Math.Max(0, MinDescriptionLength(kind) - length);
    }

    public static bool IsSubmittable(FeedbackKind kind, string? description) {
        return RemainingCharacters(kind, description) == 0;
    }

    public static bool IsTitlePresent(string? title) {
        return !string.IsNullOrWhiteSpace(title);
    }

    /// <summary>
    ///     Steps are required on a bug and do not exist on a suggestion.
    /// </summary>
    public static bool AreStepsPresent(FeedbackKind kind, string? steps) {
        return kind != FeedbackKind.Bug || !string.IsNullOrWhiteSpace(steps);
    }

    public static bool IsComplete(FeedbackKind kind, string? title, string? description, string? steps) {
        return IsTitlePresent(title)
               && IsSubmittable(kind, description)
               && AreStepsPresent(kind, steps);
    }
}
