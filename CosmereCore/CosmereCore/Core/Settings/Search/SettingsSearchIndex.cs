using global::System;
using global::System.Collections.Generic;
using global::System.Text;

namespace Cosmere.Core.Settings.Search;

public sealed class SettingsSearchIndex {
    private static readonly IReadOnlyList<SettingsSearchResult> EmptyResults = [];

    private readonly IReadOnlyList<SettingsSearchDocument> documents;
    private readonly IReadOnlyList<string> haystacks;
    private readonly IReadOnlyList<bool> searchable;

    public SettingsSearchIndex(IReadOnlyList<SettingsSearchDocument> sourceDocuments) {
        List<SettingsSearchDocument> indexedDocuments = new List<SettingsSearchDocument>(sourceDocuments.Count);
        List<string> normalizedHaystacks = new List<string>(sourceDocuments.Count);
        List<bool> documentSearchability = new List<bool>(sourceDocuments.Count);

        foreach (SettingsSearchDocument document in sourceDocuments) {
            StringBuilder haystack = new StringBuilder();
            AppendNormalized(haystack, document.SystemName);
            AppendNormalized(haystack, document.SectionName);
            AppendNormalized(haystack, document.Label);
            AppendNormalized(haystack, document.Description);
            indexedDocuments.Add(document);
            normalizedHaystacks.Add(haystack.ToString());

            bool sectionVisible = document.SourceSection?.IsVisible ?? true;
            bool descriptorVisible = document.SourceDescriptor?.IsVisible ?? true;
            documentSearchability.Add(sectionVisible && descriptorVisible);
        }

        documents = indexedDocuments;
        haystacks = normalizedHaystacks;
        searchable = documentSearchability;
    }

    public IReadOnlyList<SettingsSearchResult> Search(string query) {
        if (string.IsNullOrWhiteSpace(query)) return EmptyResults;

        List<SettingsSearchResult> results = new List<SettingsSearchResult>();

        for (int index = 0; index < documents.Count; index++) {
            if (!searchable[index]) continue;
            if (haystacks[index].IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0) continue;

            results.Add(new SettingsSearchResult(documents[index]));
        }

        return results;
    }

    public IReadOnlyDictionary<string, int> CountBySystem(string query) {
        Dictionary<string, int> counts = new Dictionary<string, int>();

        if (string.IsNullOrWhiteSpace(query)) return counts;

        for (int index = 0; index < documents.Count; index++) {
            if (!searchable[index]) continue;
            if (haystacks[index].IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0) continue;

            string systemKey = documents[index].SystemKey;
            if (counts.TryGetValue(systemKey, out int count)) counts[systemKey] = count + 1;
            else counts.Add(systemKey, 1);
        }

        return counts;
    }

    private static void AppendNormalized(StringBuilder haystack, string? text) {
        if (string.IsNullOrEmpty(text)) return;

        bool inTag = false;

        foreach (char character in text!) {
            if (character == '<') {
                inTag = true;
                continue;
            }

            if (character == '>') {
                inTag = false;
                continue;
            }

            if (!inTag) haystack.Append(character);
        }

        haystack.Append(' ');
    }
}
