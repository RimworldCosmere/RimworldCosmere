using Cosmere.Core.BetaHub;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Covers log trimming and the shape of the attached diagnostics body.
/// </summary>
[TestClass]
public class DiagnosticsTextTests {
    [TestMethod]
    public void ShortLogsArePassedThroughUntouched() {
        Assert.AreEqual("line one\nline two", LogTail.Take("line one\nline two", 1000));
    }

    [TestMethod]
    public void AnEmptyLogProducesAnEmptyTail() {
        Assert.AreEqual(string.Empty, LogTail.Take(string.Empty, 1000));
        Assert.AreEqual(string.Empty, LogTail.Take(null!, 1000));
    }

    /// <summary>
    ///     A tail that starts mid-line reads as a corrupt log, so the partial first line goes.
    /// </summary>
    [TestMethod]
    public void TheTailStartsOnALineBoundary() {
        string log = "aaaaaaaaaa\nbbbbbbbbbb\ncccccccccc";
        string tail = LogTail.Take(log, 25);

        Assert.IsTrue(tail.StartsWith("bbbbbbbbbb") || tail.StartsWith("cccccccccc"), tail);
        Assert.IsFalse(tail.Contains("aaaaaaaaaa"), tail);
    }

    [TestMethod]
    public void TheTailNeverExceedsTheCap() {
        string log = new string('x', 5000);
        Assert.IsTrue(LogTail.Take(log, 100).Length <= 100);
    }

    /// <summary>
    ///     A single line longer than the cap has no boundary to cut on. It must still
    ///     return something rather than an empty string.
    /// </summary>
    [TestMethod]
    public void ASingleOverlongLineStillProducesOutput() {
        string tail = LogTail.Take(new string('x', 500), 100);

        Assert.AreEqual(100, tail.Length);
    }

    [TestMethod]
    public void TheBodyCarriesEverySection() {
        DiagnosticsFacts facts = SampleFacts();
        string body = DiagnosticsText.Build(facts, "log line one\nlog line two");

        StringAssert.Contains(body, "2.0.0-beta.23");
        StringAssert.Contains(body, "76561198000000000");
        StringAssert.Contains(body, "Cryptik");
        StringAssert.Contains(body, "1.6.4518");
        StringAssert.Contains(body, "Linux");
        StringAssert.Contains(body, "CosmereCore");
        StringAssert.Contains(body, "log line two");
    }

    [TestMethod]
    public void TheInlineFooterOmitsTheLogAndTheSteamId() {
        string footer = DiagnosticsText.BuildInlineFooter(SampleFacts());

        StringAssert.Contains(footer, "2.0.0-beta.23");
        StringAssert.Contains(footer, "CosmereCore");
        Assert.IsFalse(footer.Contains("76561198000000000"), footer);
    }

    private static DiagnosticsFacts SampleFacts() {
        return new DiagnosticsFacts {
            Revision = "2.0.0-beta.23",
            BuildTime = "2026-07-29T20:42:23.110Z",
            SteamId = "76561198000000000",
            SteamPersona = "Cryptik",
            GameVersion = "1.6.4518",
            OperatingSystem = "Linux 6.9",
            GraphicsDevice = "AMD Radeon",
            SystemMemoryMb = 32768,
            ActiveMods = ["CosmereCore 2.0.0-beta.23", "Harmony 2.3.3"],
        };
    }
}
