using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     DefDatabase is keyed on the concrete type, so Copper exists twice: a MetalDef and the
///     MetallicArtsMetalDef inheriting it. They share a defName and nothing else.
/// </summary>
[TestClass]
public class MetalDefIdentityTests {
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

    private static IEnumerable<(string path, string text)> SourceFiles() {
        string root = Path.Combine(RepoRoot, "CosmereCore", "CosmereCore");

        return Directory
            .EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .Where(p => !p.EndsWith(".generated.cs", StringComparison.Ordinal))
            .Select(p => (Path.GetRelativePath(RepoRoot, p), File.ReadAllText(p)));
    }

    /// <summary>
    ///     Both defs answer to the same name, so the name is the only thing worth comparing.
    /// </summary>
    [TestMethod]
    public void IsOneOfComparesNamesRatherThanReferences() {
        string ext = File.ReadAllText(Path.Combine(
            RepoRoot, "CosmereCore", "CosmereCore", "Core", "Extension", "DefExtension.cs"
        ));

        Assert.IsTrue(
            ext.Contains("def.defName == other.defName"),
            "IsOneOf has to match on defName. Reference equality cannot see across two DefDatabases."
        );
        Assert.IsFalse(
            ext.Contains("defs.Any(def.Equals)"),
            "defs.Any(def.Equals) is what made the duralumin exemption dead code."
        );
    }

    /// <summary>
    ///     The metals a Metalborn gene carries are always MetallicArtsMetalDefs, so testing one
    ///     against the plain MetalDefOf answers false forever.
    /// </summary>
    [TestMethod]
    public void NothingComparesAMetalbornMetalAgainstThePlainMetalDefOf() {
        // comparing defName to defName is the correct form, so it is not an offender
        Regex suspect = new Regex(
            @"(==|!=|\.Equals\(|IsOneOf\()\s*MetalDefOf\.[A-Za-z]+(?![A-Za-z]|\.defName)",
            RegexOptions.Compiled
        );

        List<string> offenders = [];
        foreach ((string path, string text) in SourceFiles()) {
            string[] lines = text.Split('\n');
            foreach (Match match in suspect.Matches(text)) {
                int lineNumber = text.Take(match.Index).Count(c => c == '\n') + 1;
                string line = lines[lineNumber - 1].Trim();

                // both of these really are resolved out of DefDatabase<MetalDef>
                if (line.Contains("mind.Metal") || line.Contains("x.Equals(MetalDefOf.Aluminum)")) continue;

                offenders.Add($"{path}:{lineNumber}  {line}");
            }
        }

        Assert.AreEqual(
            0,
            offenders.Count,
            "Compare against MetallicArtsMetalDefOf, or on defName:\n" + string.Join("\n", offenders)
        );
    }

    /// <summary>
    ///     Duralumin and nicrosil never consume a vial, and lerasium refuses an existing Mistborn.
    ///     Both exemptions were silently off.
    /// </summary>
    [TestMethod]
    public void TheExemptionsPointAtTheDefsTheirCallersActuallyHold() {
        string gene = File.ReadAllText(Path.Combine(
            RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "Gene", "Allomancer.cs"
        ));
        Assert.IsTrue(
            gene.Contains("IsOneOf(MetallicArtsMetalDefOf.Duralumin, MetallicArtsMetalDefOf.Nicrosil)"),
            "Metalborn.metal is a MetallicArtsMetalDef, so the exemption has to name those defs."
        );

        string pawnExt = File.ReadAllText(Path.Combine(
            RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "Extension", "PawnExtension.cs"
        ));
        Assert.IsTrue(
            pawnExt.Contains("metal.defName == MetalDefOf.Lerasium.defName"),
            "CanUseMetal takes a MetalDef but every caller hands it a MetallicArtsMetalDef."
        );
    }
}
