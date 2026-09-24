using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Every site below is Verse-bound, so these read the source: the test host has no Verse.AI,
///     no CompGlower and no HediffSet.
/// </summary>
[TestClass]
public class SonarBlockerDefectTests {
    private static readonly string[] WorkGivers = [
        Path.Combine("System", "Roshar", "WorkGiver", "SoulcastWorkGiver.cs"),
        Path.Combine("System", "Scadrial", "WorkGiver", "ClearAsh.cs"),
    ];

    private static readonly string[] Glowers = [
        Path.Combine("System", "Roshar", "Comp", "Thing", "SphereGlower.cs"),
        Path.Combine("System", "Roshar", "Comp", "Thing", "StormlightBuildingGlower.cs"),
    ];

    private static string CoreRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not locate CosmereScadrial/Defs above the test output directory.");
            return Path.Combine(dir!.FullName, "CosmereCore", "CosmereCore");
        }
    }

    /// <summary>
    ///     `PathEndMode => PathEndMode.Touch` dodges self-recursion only by the C# identical-names
    ///     rule. One edit on the right side and work scanning overflows the stack.
    /// </summary>
    [TestMethod]
    public void WorkGiverPathEndModeNamesTheEnumNotItself() {
        foreach (string relative in WorkGivers) {
            string source = SourceOf(relative);
            Match property = Regex.Match(source, @"override PathEndMode PathEndMode\s*=>\s*([^;]+);");

            Assert.IsTrue(property.Success, $"{relative} no longer declares a PathEndMode property.");
            StringAssert.StartsWith(
                property.Groups[1].Value.Trim(),
                "Verse.AI.PathEndMode.",
                $"{relative} returns an unqualified PathEndMode. Write Verse.AI.PathEndMode.<value> so the " +
                "property cannot read as a recursive call on itself."
            );
        }
    }

    /// <summary>
    ///     Both glowers derive radius and colour from stored Investiture, so an accessor that drops
    ///     `value` on the floor reads as a bug. Override the getter alone instead.
    /// </summary>
    [TestMethod]
    public void GlowerPropertiesDoNotDeclareAnAccessorThatDiscardsValue() {
        foreach (string relative in Glowers) {
            string source = SourceOf(relative);

            Assert.IsFalse(
                Regex.IsMatch(source, @"\bset\s*\{\s*\}"),
                $"{relative} declares an empty set accessor. Drop the setter and override only the getter."
            );
        }
    }

    /// <summary>
    ///     A conduit is wall trim, not a node with network totals to show. It answers for itself,
    ///     so the base never has to know its own subclasses.
    /// </summary>
    [TestMethod]
    public void StormlightNodeDoesNotTypeTestItsOwnSubclass() {
        Assert.IsFalse(
            Regex.IsMatch(
                SourceOf(Path.Combine("System", "Roshar", "Comp", "Thing", "StormlightNode.cs")),
                @"\bis\s+StormlightConduit\b"
            ),
            "StormlightNode type-tests StormlightConduit. Override the member on the conduit instead."
        );

        Assert.IsTrue(
            Regex.IsMatch(
                SourceOf(Path.Combine("System", "Roshar", "Comp", "Thing", "StormlightConduit.cs")),
                @"override\s+string\??\s+CompInspectStringExtra\s*\(\s*\)"
            ),
            "StormlightConduit must override CompInspectStringExtra, or it inherits the node's network readout."
        );
    }

    /// <summary>
    ///     Concatenating an empty list yields the same sequence, so a count check around the
    ///     caravan gizmos buys nothing and reads as an unreachable branch.
    /// </summary>
    [TestMethod]
    public void PortalCaravanGizmosAreConcatenatedUnconditionally() {
        string source = SourceOf(
            Path.Combine("System", "Roshar", "Surgebinding", "Ability", "Transportation", "Portal.cs")
        );

        Assert.IsFalse(
            Regex.IsMatch(source, @"if\s*\(\s*extra\.Count\s*>\s*0\s*\)"),
            "Portal guards the gizmo concat on extra.Count. Concat on an empty list is already a no-op."
        );
        Assert.IsTrue(
            Regex.IsMatch(source, @"ch\.ReturnValue\s*=\s*ch\.ReturnValue\.Concat\(\s*extra\s*\)"),
            "Portal must still append its caravan gizmos to the return value."
        );
    }

    /// <summary>
    ///     An unchecked cast throws InvalidCastException mid-ability when another mod swaps the
    ///     def's hediffClass. A type test reports the miss instead.
    /// </summary>
    [TestMethod]
    public void SurgebindingHediffLookupTypeChecksBeforeItCasts() {
        string source = SourceOf(
            Path.Combine("System", "Roshar", "Surgebinding", "Util", "SurgebindingHediffUtility.cs")
        );

        Assert.IsFalse(
            Regex.IsMatch(source, @"\(\s*SurgebindingHediff\s*\)\s*uncastHediff"),
            "TryGetHediff casts to SurgebindingHediff unchecked. Use an `is` pattern so a foreign " +
            "hediffClass reports a miss instead of throwing."
        );
        Assert.IsTrue(
            Regex.IsMatch(source, @"uncastHediff\s+is\s+not\s+SurgebindingHediff"),
            "TryGetHediff must type-check the hediff it found before handing it back."
        );
    }

    private static string SourceOf(string relative) {
        string path = Path.Combine(CoreRoot, relative);
        Assert.IsTrue(File.Exists(path), $"Expected a source file at {path}");
        return File.ReadAllText(path);
    }
}
