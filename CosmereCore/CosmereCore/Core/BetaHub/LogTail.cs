namespace Cosmere.Core.BetaHub;

/// <summary>
///     Trims a log down to its most recent characters, cutting on a line boundary so the
///     first line of the result is not a fragment.
/// </summary>
public static class LogTail {
    public const int DefaultMaxChars = 256 * 1024;

    public static string Take(string content, int maxChars) {
        if (string.IsNullOrEmpty(content)) return string.Empty;
        if (content.Length <= maxChars) return content;

        string tail = content.Substring(content.Length - maxChars);
        int firstNewline = tail.IndexOf('\n');

        return firstNewline < 0 ? tail : tail.Substring(firstNewline + 1);
    }
}
