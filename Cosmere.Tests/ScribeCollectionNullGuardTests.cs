using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Scribe_Collections.Look leaves a collection null when the save has no such node, which
///     is every save written without Cosmere. DormantConnection then NREd on every pawn tick.
/// </summary>
[TestClass]
public class ScribeCollectionNullGuardTests {
    /// <summary>
    ///     Reads as unguarded but is not a defect: two locals AshPlume handles, and one field
    ///     that is nullable by design. The list may shrink, never grow.
    /// </summary>
    private static readonly HashSet<string> KnownUnguarded = [
        "System/Scadrial/Comp/Map/AshDepthTracker.cs|banked",
        "System/Scadrial/Comp/Thing/CompAshVent.cs|banked",
        "Core/Quest/Objective/QuestPart_CosmereChoice.cs|options",
    ];

    private static readonly Regex LookCall = new(@"Scribe_Collections\.Look\(\s*ref\s+(\w+)");

    private static string SourceRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not locate CosmereScadrial/Defs above the test output directory.");
            return Path.Combine(dir!.FullName, "CosmereCore", "CosmereCore");
        }
    }

    [TestMethod]
    public void EveryScribedCollectionIsNullGuarded() {
        List<string> offenders = FindUnguarded().Except(KnownUnguarded).ToList();

        Assert.AreEqual(
            0,
            offenders.Count,
            "Scribe_Collections.Look without a null guard. Loading a save that lacks the node " +
            "leaves the field null and the next dereference throws:\n" + string.Join("\n", offenders) +
            "\nAdd `field ??= [];` after the Look call."
        );
    }

    [TestMethod]
    public void TheKnownUnguardedListHasNotGoneStale() {
        List<string> fixedSites = KnownUnguarded.Except(FindUnguarded()).ToList();

        Assert.AreEqual(
            0,
            fixedSites.Count,
            "These sites now have a guard, so drop them from KnownUnguarded:\n" +
            string.Join("\n", fixedSites)
        );
    }

    private static List<string> FindUnguarded() {
        List<string> found = [];

        foreach (string file in Directory.EnumerateFiles(SourceRoot, "*.cs", SearchOption.AllDirectories)) {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")) continue;
            if (file.EndsWith(".generated.cs", StringComparison.Ordinal)) continue;

            string[] lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++) {
                Match match = LookCall.Match(lines[i]);
                if (!match.Success) continue;

                string field = match.Groups[1].Value;
                string window = string.Join("\n", lines.Skip(i + 1).Take(6));
                if (Regex.IsMatch(window, $@"\b{Regex.Escape(field)}\s*(\?\?=|=\s*new)")) continue;

                string relative = Path.GetRelativePath(SourceRoot, file).Replace(Path.DirectorySeparatorChar, '/');
                found.Add($"{relative}|{field}");
            }
        }

        return found;
    }
}
