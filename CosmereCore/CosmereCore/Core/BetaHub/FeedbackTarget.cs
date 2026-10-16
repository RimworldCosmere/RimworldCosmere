namespace Cosmere.Core.BetaHub;

/// <summary>
///     Which mod a report is about. One BetaHub project covers all three, so this becomes
///     the custom field the dashboard filters on.
/// </summary>
public enum FeedbackTarget {
    Unknown,
    Core,
    Scadrial,
    Roshar,
}
