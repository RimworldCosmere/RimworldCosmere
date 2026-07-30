namespace Cosmere.Core.BetaHub;

/// <summary>
///     Turns a report into the form fields BetaHub expects. Bugs and suggestions are separate
///     resources with different parameter roots, and only bugs accept attachments.
/// </summary>
public static class BetaHubFormEncoder {
    public static string ParameterRoot(FeedbackKind kind) {
        return kind == FeedbackKind.Suggestion ? "feature_request" : "issue";
    }

    public static List<KeyValuePair<string, string>> Encode(FeedbackReport report, DiagnosticsFacts facts) {
        string root = ParameterRoot(report.Kind);
        List<KeyValuePair<string, string>> form = [];

        Add(form, $"{root}[title]", report.Title);

        string description = report.Kind == FeedbackKind.Suggestion
            ? report.Description + DiagnosticsText.BuildInlineFooter(facts)
            : report.Description;
        Add(form, $"{root}[description]", description);

        if (report.Kind == FeedbackKind.Bug) {
            Add(form, $"{root}[unformatted_steps_to_reproduce]", report.StepsToReproduce);
        }

        Add(form, $"{root}[custom][mod]", report.Target.ToString());
        Add(form, $"{root}[release_label]", facts.Revision);
        Add(form, $"{root}[discord_username]", report.DiscordUsername);
        Add(form, $"{root}[source]", BetaHubConfig.SourceTag);

        return form;
    }

    private static void Add(List<KeyValuePair<string, string>> form, string key, string? value) {
        if (string.IsNullOrWhiteSpace(value)) return;

        form.Add(new KeyValuePair<string, string>(key, value!));
    }
}
