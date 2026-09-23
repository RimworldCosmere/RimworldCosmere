namespace Cosmere.Core.Settings.Search;

public sealed record SettingsSearchResult {
    public SettingsSearchResult(SettingsSearchDocument document) {
        Document = document;
    }

    public SettingsSearchDocument Document { get; }
}
