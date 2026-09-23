using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Codex content providers are registered once as singletons, so a scroll position parked on
///     one of them is shared by every pawn. Scroll position belongs on CodexState, which the tab
///     keeps per pawn. These read the source because Widgets.BeginScrollView and CodexState are
///     both Unity-bound and the test host has no UnityEngine.
/// </summary>
[TestClass]
public class CodexScrollIsPerPawnTests {
    private static readonly string[] Renderers = [
        Path.Combine("Core", "UI", "Codex", "AutocastSubtabRenderer.cs"),
        Path.Combine("System", "Scadrial", "UI", "AllomancyCodexContent.cs"),
        Path.Combine("System", "Scadrial", "UI", "FeruchemyCodexContent.cs"),
        Path.Combine("System", "Roshar", "UI", "SurgebindingCodexContent.cs"),
    ];

    private static string RepoRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not locate CosmereScadrial/Defs above the test output directory.");
            return dir.FullName;
        }
    }

    private static string SourceOf(string relative) {
        string path = Path.Combine(RepoRoot, "CosmereCore", "CosmereCore", relative);
        Assert.IsTrue(File.Exists(path), $"Expected a codex renderer at {path}");
        return File.ReadAllText(path);
    }

    [TestMethod]
    public void NoCodexRendererHoldsAScrollPositionOfItsOwn() {
        foreach (string relative in Renderers) {
            Match field = Regex.Match(
                SourceOf(relative),
                @"(?:private|protected|internal|public)\s+(?:static\s+)?(?:readonly\s+)?Vector2\s+(\w+)"
            );

            Assert.IsFalse(
                field.Success,
                $"{relative} declares a Vector2 field '{field.Groups[1].Value}'. Providers are singletons in " +
                "InvestitureProviderRegistry and the renderer is static, so that field is one per system rather " +
                "than one per pawn: scrolling pawn A's list leaves pawn B opening mid-scroll. Put it on CodexState."
            );
        }
    }

    [TestMethod]
    public void EveryCodexScrollViewReadsItsPositionFromCodexState() {
        foreach (string relative in Renderers) {
            foreach (Match call in Regex.Matches(SourceOf(relative), @"BeginScrollView\([^;]*?,\s*ref\s+([\w.]+)\s*,")) {
                string target = call.Groups[1].Value;
                Assert.IsTrue(
                    target.StartsWith("state.", StringComparison.Ordinal),
                    $"{relative} scrolls against '{target}' instead of a CodexState field."
                );
            }
        }
    }

    [TestMethod]
    public void CodexStateCarriesAScrollFieldForEverySubtabThatScrolls() {
        string source = SourceOf(Path.Combine("Core", "UI", "Codex", "CodexState.cs"));

        foreach (string field in new[] { "ProgressionScroll", "AutocastScroll" }) {
            Assert.IsTrue(
                Regex.IsMatch(source, $@"public\s+Vector2\s+{field}\b"),
                $"CodexState is missing {field}, so that subtab has nowhere per-pawn to keep its scroll position."
            );
        }
    }
}
