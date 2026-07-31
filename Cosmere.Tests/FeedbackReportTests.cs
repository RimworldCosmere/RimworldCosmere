using System.Collections.Generic;
using Cosmere.Core.BetaHub;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Covers the minimum length rules the server enforces, and the exact form keys it expects.
/// </summary>
/// <remarks>
///     The two minimums and both parameter roots were read off the live project rather than
///     the docs, which do not state them. Changing them here without re-probing will produce 422s.
/// </remarks>
[TestClass]
public class FeedbackReportTests {
    [TestMethod]
    public void BugsNeedFiftyCharactersAndSuggestionsNeedEighty() {
        Assert.AreEqual(50, FeedbackValidator.MinDescriptionLength(FeedbackKind.Bug));
        Assert.AreEqual(80, FeedbackValidator.MinDescriptionLength(FeedbackKind.Suggestion));
    }

    [TestMethod]
    public void OneCharacterShortIsNotSubmittable() {
        Assert.IsFalse(FeedbackValidator.IsSubmittable(FeedbackKind.Bug, new string('a', 49)));
        Assert.IsFalse(FeedbackValidator.IsSubmittable(FeedbackKind.Suggestion, new string('a', 79)));
    }

    [TestMethod]
    public void ExactlyTheMinimumIsSubmittable() {
        Assert.IsTrue(FeedbackValidator.IsSubmittable(FeedbackKind.Bug, new string('a', 50)));
        Assert.IsTrue(FeedbackValidator.IsSubmittable(FeedbackKind.Suggestion, new string('a', 80)));
    }

    /// <summary>
    ///     Whitespace padding must not count, or the counter tells the player they are done
    ///     and the server disagrees.
    /// </summary>
    [TestMethod]
    public void SurroundingWhitespaceDoesNotCount() {
        string padded = "   " + new string('a', 48) + "   ";

        Assert.IsFalse(FeedbackValidator.IsSubmittable(FeedbackKind.Bug, padded));
    }

    [TestMethod]
    public void TheCounterReportsCharactersStillNeeded() {
        Assert.AreEqual(10, FeedbackValidator.RemainingCharacters(FeedbackKind.Bug, new string('a', 40)));
        Assert.AreEqual(0, FeedbackValidator.RemainingCharacters(FeedbackKind.Bug, new string('a', 90)));
        Assert.AreEqual(50, FeedbackValidator.RemainingCharacters(FeedbackKind.Bug, null));
    }

    [TestMethod]
    public void BugsAndSuggestionsUseDifferentParameterRoots() {
        Assert.AreEqual("issue", BetaHubFormEncoder.ParameterRoot(FeedbackKind.Bug));
        Assert.AreEqual("feature_request", BetaHubFormEncoder.ParameterRoot(FeedbackKind.Suggestion));
    }

    [TestMethod]
    public void ABugEncodesEveryFieldUnderTheIssueRoot() {
        List<KeyValuePair<string, string>> form = BetaHubFormEncoder.Encode(SampleBug(), SampleFacts());

        Assert.AreEqual("Pewter burn does nothing", Find(form, "issue[title]"));
        Assert.AreEqual("Scadrial", Find(form, "issue[custom][mod]"));
        Assert.AreEqual("2.0.0-beta.23", Find(form, "issue[release_label]"));
        Assert.AreEqual("cryptik", Find(form, "issue[custom][discord]"));
        Assert.AreEqual("rimworld_ingame", Find(form, "issue[source]"));
        StringAssert.Contains(Find(form, "issue[unformatted_steps_to_reproduce]"), "Burn pewter");
    }

    /// <summary>
    ///     A bug carries its build context in an attached log file, so the description must
    ///     stay exactly what the player typed.
    /// </summary>
    [TestMethod]
    public void ABugDescriptionIsNotDecorated() {
        List<KeyValuePair<string, string>> form = BetaHubFormEncoder.Encode(SampleBug(), SampleFacts());

        Assert.AreEqual(SampleBug().Description, Find(form, "issue[description]"));
    }

    /// <summary>
    ///     A suggestion cannot carry an attachment, so its build context is appended instead.
    /// </summary>
    [TestMethod]
    public void ASuggestionDescriptionGainsABuildFooter() {
        FeedbackReport report = SampleSuggestion();
        List<KeyValuePair<string, string>> form = BetaHubFormEncoder.Encode(report, SampleFacts());
        string description = Find(form, "feature_request[description]");

        StringAssert.StartsWith(description, report.Description);
        StringAssert.Contains(description, "2.0.0-beta.23");
    }

    [TestMethod]
    public void ASuggestionSendsNoStepsToReproduce() {
        List<KeyValuePair<string, string>> form = BetaHubFormEncoder.Encode(SampleSuggestion(), SampleFacts());

        Assert.IsNull(FindOrNull(form, "feature_request[unformatted_steps_to_reproduce]"));
    }

    [TestMethod]
    public void EmptyOptionalFieldsAreOmittedEntirely() {
        FeedbackReport report = SampleBug();
        report.Title = string.Empty;
        report.DiscordUsername = null;
        report.StepsToReproduce = string.Empty;

        List<KeyValuePair<string, string>> form = BetaHubFormEncoder.Encode(report, SampleFacts());

        Assert.IsNull(FindOrNull(form, "issue[title]"));
        Assert.IsNull(FindOrNull(form, "issue[custom][discord]"));
        Assert.IsNull(FindOrNull(form, "issue[unformatted_steps_to_reproduce]"));
    }

    [TestMethod]
    public void AnUnknownTargetStillSendsTheCustomField() {
        FeedbackReport report = SampleBug();
        report.Target = FeedbackTarget.Unknown;

        Assert.AreEqual("Unknown", Find(BetaHubFormEncoder.Encode(report, SampleFacts()), "issue[custom][mod]"));
    }

    [TestMethod]
    public void ATitleIsRequired() {
        Assert.IsFalse(FeedbackValidator.IsTitlePresent(null));
        Assert.IsFalse(FeedbackValidator.IsTitlePresent(string.Empty));
        Assert.IsFalse(FeedbackValidator.IsTitlePresent("   "));
        Assert.IsTrue(FeedbackValidator.IsTitlePresent("Pewter burn does nothing"));
    }

    [TestMethod]
    public void StepsAreRequiredOnABug() {
        Assert.IsFalse(FeedbackValidator.AreStepsPresent(FeedbackKind.Bug, null));
        Assert.IsFalse(FeedbackValidator.AreStepsPresent(FeedbackKind.Bug, "  "));
        Assert.IsTrue(FeedbackValidator.AreStepsPresent(FeedbackKind.Bug, "Burn pewter"));
    }

    /// <summary>
    ///     A suggestion has no steps field at all, so an absent value must not block it.
    /// </summary>
    [TestMethod]
    public void StepsAreNotRequiredOnASuggestion() {
        Assert.IsTrue(FeedbackValidator.AreStepsPresent(FeedbackKind.Suggestion, null));
        Assert.IsTrue(FeedbackValidator.AreStepsPresent(FeedbackKind.Suggestion, string.Empty));
    }

    [TestMethod]
    public void ABugIsCompleteOnlyWithTitleDescriptionAndSteps() {
        string description = new string('a', 50);

        Assert.IsFalse(FeedbackValidator.IsComplete(FeedbackKind.Bug, null, description, "steps"));
        Assert.IsFalse(FeedbackValidator.IsComplete(FeedbackKind.Bug, "t", new string('a', 49), "steps"));
        Assert.IsFalse(FeedbackValidator.IsComplete(FeedbackKind.Bug, "t", description, null));
        Assert.IsTrue(FeedbackValidator.IsComplete(FeedbackKind.Bug, "t", description, "steps"));
    }

    [TestMethod]
    public void ASuggestionIsCompleteWithTitleAndDescriptionAlone() {
        Assert.IsTrue(FeedbackValidator.IsComplete(FeedbackKind.Suggestion, "t", new string('a', 80), null));
        Assert.IsFalse(FeedbackValidator.IsComplete(FeedbackKind.Suggestion, "t", new string('a', 79), null));
        Assert.IsFalse(FeedbackValidator.IsComplete(FeedbackKind.Suggestion, "  ", new string('a', 80), null));
    }

    /// <summary>
    ///     discord_username is dropped by BetaHub unless the caller is an anonymous FormUser,
    ///     which a project token is not. Verified against the live project.
    /// </summary>
    [TestMethod]
    public void TheDiscordNameGoesInACustomFieldNotDiscordUsername() {
        List<KeyValuePair<string, string>> form = BetaHubFormEncoder.Encode(SampleBug(), SampleFacts());

        Assert.IsNull(FindOrNull(form, "issue[discord_username]"));
        Assert.AreEqual("cryptik", Find(form, "issue[custom][discord]"));
    }

    [TestMethod]
    public void ABugSendsDeviceInfoForBetaHubToParse() {
        List<KeyValuePair<string, string>> form = BetaHubFormEncoder.Encode(SampleBug(), SampleFacts());

        StringAssert.Contains(Find(form, "issue[extras][device_info][value]"), "Linux 6.9");
        Assert.AreEqual("optional", Find(form, "issue[extras][device_info][validation_mode]"));
    }

    /// <summary>
    ///     Only bugs were probed against the live API, so suggestions deliberately do not send it.
    /// </summary>
    [TestMethod]
    public void ASuggestionSendsNoDeviceInfo() {
        List<KeyValuePair<string, string>> form = BetaHubFormEncoder.Encode(SampleSuggestion(), SampleFacts());

        Assert.IsNull(FindOrNull(form, "feature_request[extras][device_info][value]"));
    }

    [TestMethod]
    public void AVideoLinkRidesInACustomField() {
        FeedbackReport report = SampleBug();
        report.VideoUrl = "https://youtu.be/abc123";

        Assert.AreEqual(
            "https://youtu.be/abc123",
            Find(BetaHubFormEncoder.Encode(report, SampleFacts()), "issue[custom][video_url]")
        );
    }

    [TestMethod]
    public void AnAbsentVideoLinkIsOmitted() {
        Assert.IsNull(FindOrNull(BetaHubFormEncoder.Encode(SampleBug(), SampleFacts()), "issue[custom][video_url]"));
    }

    [TestMethod]
    public void AVideoLinkIsAppendedToTheDescriptionSoItRenders() {
        FeedbackReport report = SampleBug();
        report.VideoUrl = "https://youtu.be/abc123";

        string description = Find(BetaHubFormEncoder.Encode(report, SampleFacts()), "issue[description]");

        StringAssert.StartsWith(description, report.Description);
        StringAssert.Contains(description, "https://youtu.be/abc123");
    }

    private static string Find(List<KeyValuePair<string, string>> form, string key) {
        string? found = FindOrNull(form, key);
        Assert.IsNotNull(found, $"form had no key {key}");

        return found!;
    }

    private static string? FindOrNull(List<KeyValuePair<string, string>> form, string key) {
        foreach (KeyValuePair<string, string> pair in form) {
            if (pair.Key == key) return pair.Value;
        }

        return null;
    }

    private static FeedbackReport SampleBug() {
        return new FeedbackReport {
            Kind = FeedbackKind.Bug,
            Title = "Pewter burn does nothing",
            Description = "Burning pewter gives no strength bonus at all, the stat page is unchanged.",
            StepsToReproduce = "Burn pewter\nOpen the stat page",
            Target = FeedbackTarget.Scadrial,
            DiscordUsername = "cryptik",
        };
    }

    private static FeedbackReport SampleSuggestion() {
        return new FeedbackReport {
            Kind = FeedbackKind.Suggestion,
            Title = "Let allomancers queue burns",
            Description = "It would be good to queue a sequence of metal burns ahead of time instead of clicking each one.",
            Target = FeedbackTarget.Scadrial,
        };
    }

    private static DiagnosticsFacts SampleFacts() {
        return new DiagnosticsFacts {
            Revision = "2.0.0-beta.23",
            GameVersion = "1.6.4518",
            OperatingSystem = "Linux 6.9",
            ActiveMods = ["CosmereCore 2.0.0-beta.23"],
        };
    }
}
