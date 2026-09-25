using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The oath ladder is the progression readout in the Surgebinding dock body, and the
///     ceiling sentence is the aside beside it rather than the caption it used to be.
/// </summary>
[TestClass]
public class OathLadderWiringTests {
    private static string Section {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereCore", "CosmereCore"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not locate CosmereCore above the test output directory.");

            return File.ReadAllText(Path.Combine(
                dir!.FullName,
                "CosmereCore",
                "CosmereCore",
                "System",
                "Roshar",
                "UI",
                "SurgebindingDockSection.cs"
            ));
        }
    }

    /// <summary>
    ///     Comments in this file describe the caption row the ladder replaced, so a plain
    ///     search reads the old layout back as if it were still drawn.
    /// </summary>
    private static string CodeOnly(string source) {
        string[] lines = source.Split('\n');
        List<string> kept = [];

        for (int i = 0; i < lines.Length; i++) {
            string trimmed = lines[i].TrimStart();
            if (trimmed.StartsWith("//") || trimmed.StartsWith('*')) continue;

            kept.Add(lines[i]);
        }

        return string.Join("\n", kept);
    }

    [TestMethod]
    public void LadderIsDrawn() {
        Assert.Contains(
            "OathLadder.Draw",
            CodeOnly(Section),
            "The dock body has to draw the ladder, not just reserve height for it."
        );
    }

    [TestMethod]
    public void CeilingSentenceIsRightAligned() {
        string code = CodeOnly(Section);
        int start = code.IndexOf("CeilingCaption(", StringComparison.Ordinal);
        Assert.IsGreaterThan(0, start, "The ceiling sentence is still drawn somewhere.");

        int end = code.IndexOf(");", start, StringComparison.Ordinal);
        Assert.IsGreaterThan(start, end, "The ceiling sentence's draw call has to close.");

        Assert.Contains(
            "TextAnchor.MiddleRight",
            code.Substring(start, end - start),
            "The ceiling sentence is the right-hand aside under the ladder, not the left caption."
        );
    }
}
