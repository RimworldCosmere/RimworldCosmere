using Cosmere.Core.Settings.Model;
using Cosmere.Core.Settings.Search;
using global::System.Collections;
using global::System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

[TestClass]
public class SettingsSearchIndexTests {
    [TestMethod]
    public void SearchMatchesSystemSectionLabelAndDescriptionCaseInsensitively() {
        SettingsSearchIndex index = new SettingsSearchIndex([
            new SettingsSearchDocument(
                "Roshar", "Roshar", "nahel", "Nahel bonds", "average", "Average time", "Spren seek someone to bond"
            ),
            new SettingsSearchDocument(
                "Scadrial", "Scadrial", "mists", "Incidents", "frequency", "Mist frequency", "How often the mists appear"
            ),
        ]);

        Assert.AreEqual(1, index.Search("roshar").Count);
        Assert.AreEqual(1, index.Search("NAHEL").Count);
        Assert.AreEqual(1, index.Search("average time").Count);
        Assert.AreEqual(1, index.Search("someone to bond").Count);
    }

    [TestMethod]
    public void BlankQueryReturnsNoSearchModeResults() {
        SettingsSearchIndex index = new SettingsSearchIndex([
            new SettingsSearchDocument("Roshar", "Roshar", "nahel", "Nahel bonds", "average", "Average time", "Spren seek someone to bond"),
        ]);

        Assert.AreEqual(0, index.Search(string.Empty).Count);
        Assert.AreEqual(0, index.Search("   ").Count);
    }

    [TestMethod]
    public void SearchPreservesSystemSectionAndDescriptorDeclarationOrder() {
        SettingsSearchIndex index = new SettingsSearchIndex([
            new SettingsSearchDocument("Roshar", "Roshar", "first", "First section", "first", "First mist setting", string.Empty),
            new SettingsSearchDocument("Scadrial", "Scadrial", "second", "Second section", "second", "Second mist setting", string.Empty),
            new SettingsSearchDocument("Scadrial", "Scadrial", "third", "Third section", "third", "Third mist setting", string.Empty),
        ]);

        IReadOnlyList<SettingsSearchResult> results = index.Search("mist");

        Assert.AreEqual("first", results[0].Document.SettingKey);
        Assert.AreEqual("second", results[1].Document.SettingKey);
        Assert.AreEqual("third", results[2].Document.SettingKey);
    }

    [TestMethod]
    public void RichTextTagsDoNotAffectMatching() {
        SettingsSearchIndex index = new SettingsSearchIndex([
            new SettingsSearchDocument(
                "Roshar", "Roshar", "stormlight", "<b>Stormlight</b> settings", "reserve", "<color=#99DDFF>Stormlight reserve</color>", "<i>Available Light</i>"
            ),
        ]);

        Assert.AreEqual(1, index.Search("stormlight reserve").Count);
        Assert.AreEqual(1, index.Search("available light").Count);
    }

    [TestMethod]
    public void UnclosedOpeningBracketRemainsSearchable() {
        SettingsSearchIndex index = new SettingsSearchIndex([
            new SettingsSearchDocument("Roshar", "Roshar", "general", "General", "value", "Value must be <5", string.Empty),
        ]);

        Assert.AreEqual(1, index.Search("5").Count);
    }

    [TestMethod]
    public void StrayClosingBracketRemainsSearchable() {
        SettingsSearchIndex index = new SettingsSearchIndex([
            new SettingsSearchDocument("Roshar", "Roshar", "general", "General", "value", "Value > 5", string.Empty),
        ]);

        Assert.AreEqual(1, index.Search(">").Count);
    }

    [TestMethod]
    public void EmptyRichTextTagIsStrippedWithoutCorruptingText() {
        SettingsSearchIndex index = new SettingsSearchIndex([
            new SettingsSearchDocument("Roshar", "Roshar", "general", "General", "value", "Storm<>light reserve", string.Empty),
        ]);

        Assert.AreEqual(1, index.Search("stormlight reserve").Count);
    }

    [TestMethod]
    public void CountBySystemReturnsGroupedCount() {
        SettingsSearchIndex index = new SettingsSearchIndex([
            new SettingsSearchDocument("Roshar", "Roshar", "storms", "Storms", "frequency", "Mist frequency", string.Empty),
            new SettingsSearchDocument("Scadrial", "Scadrial", "mists", "Mists", "frequency", "Mist frequency", string.Empty),
            new SettingsSearchDocument("Scadrial", "Scadrial", "mists", "Mists", "duration", "Mist duration", string.Empty),
        ]);

        IReadOnlyDictionary<string, int> counts = index.CountBySystem("mist");

        Assert.AreEqual(1, counts["Roshar"]);
        Assert.AreEqual(2, counts["Scadrial"]);
    }

    [TestMethod]
    public void HiddenSourceDescriptorsAreExcludedFromSearch() {
        bool value = false;
        bool isVisible = false;
        CheckboxControl control = new CheckboxControl(() => value, updated => value = updated, false);
        SettingDescriptor descriptor = new SettingDescriptor("hidden", "Settings.Hidden", null, control, () => isVisible);
        SettingSection section = new SettingSection("general", "Settings.General", [descriptor]);
        SettingsSearchDocument document = new SettingsSearchDocument(
            "Roshar", "Roshar", "general", "General", "hidden", "Hidden setting", string.Empty, section, descriptor
        );
        SettingsSearchIndex index = new SettingsSearchIndex([document]);

        Assert.AreEqual(0, index.Search("hidden").Count);
        Assert.AreEqual(0, index.CountBySystem("hidden").Count);

        isVisible = true;

        Assert.AreEqual(0, index.Search("hidden").Count);
        Assert.AreEqual(0, index.CountBySystem("hidden").Count);
    }

    [TestMethod]
    public void ResultsRetainTheirSourceReferences() {
        bool value = false;
        CheckboxControl control = new CheckboxControl(() => value, updated => value = updated, false);
        SettingDescriptor descriptor = new SettingDescriptor("visible", "Settings.Visible", null, control);
        SettingSection section = new SettingSection("general", "Settings.General", [descriptor]);
        SettingsSearchDocument document = new SettingsSearchDocument(
            "Roshar", "Roshar", "general", "General", "visible", "Visible setting", string.Empty, section, descriptor
        );
        SettingsSearchIndex index = new SettingsSearchIndex([document]);

        SettingsSearchResult result = index.Search("visible")[0];

        Assert.AreSame(section, result.Document.SourceSection);
        Assert.AreSame(descriptor, result.Document.SourceDescriptor);
    }
}
