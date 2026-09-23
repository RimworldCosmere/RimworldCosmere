using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The form designer printed a material's save key instead of its label, and drew the true
///     body with the disguise's gender while a kandra was wearing one.
/// </summary>
[TestClass]
public class KandraTrueBodyRepairTests {
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

    private static string DialogSourcePath => Path.Combine(
        RepoRoot,
        "CosmereCore",
        "CosmereCore",
        "System",
        "Scadrial",
        "Kandra",
        "Dialog_KandraForms.cs"
    );

    private static string DialogSource => File.ReadAllText(DialogSourcePath);

    [TestMethod]
    public void TheMaterialRowUnderAPortraitIsTranslatedRatherThanItsSaveKey() {
        string body = MethodBody(DialogSource, "private void DrawMaterial(");

        Assert.IsTrue(
            WithHelpers(DialogSource, body).Contains("labelKey", StringComparison.Ordinal),
            "DrawMaterial labels the material without reaching a labelKey, so the save key "
            + "(\"pewter\") lands on screen instead of its label."
        );
        AssertNoRawMaterialNameLabel(body, "DrawMaterial");
    }

    [TestMethod]
    public void TheLabelUnderTheNowPortraitIsTranslatedRatherThanItsSaveKey() {
        string body = MethodBody(DialogSource, "private string CurrentTrueLabel(");

        Assert.IsTrue(
            WithHelpers(DialogSource, body).Contains("labelKey", StringComparison.Ordinal),
            "CurrentTrueLabel returns the material's save key, which the reshape rail prints verbatim."
        );
        AssertNoRawMaterialNameLabel(body, "CurrentTrueLabel");
    }

    [TestMethod]
    public void ADisguisedKandraStillSeesItsOwnGenderInTheTrueBodyPortrait() {
        string source = DialogSource;
        string body = MethodBody(source, "private void DrawTrueBody(");

        int disguise = body.IndexOf("pawn.gender", StringComparison.Ordinal);
        int own = body.IndexOf("form.gender", StringComparison.Ordinal);
        bool fallbackPrefersTheForm = disguise < 0 || (own >= 0 && own < disguise);

        Assert.IsTrue(
            fallbackPrefersTheForm || EveryTrueBodyCallPassesAGender(source),
            "DrawTrueBody falls back to pawn.gender, which is the disguise's gender while a "
            + "kandra wears one, and no caller passes the real one. The Now pane draws the "
            + "wrong silhouette."
        );
    }

    [TestMethod]
    public void EveryKandraKeyTheDialogAsksForExistsInTheKeyedFolder() {
        HashSet<string> defined = Directory
            .EnumerateFiles(
                Path.Combine(RepoRoot, "CosmereScadrial", "Languages", "English", "Keyed"),
                "*.xml"
            )
            .SelectMany(file => XDocument.Load(file).Root?.Elements() ?? [])
            .Select(element => element.Name.LocalName)
            .ToHashSet(StringComparer.Ordinal);

        foreach (Match used in Regex.Matches(DialogSource, @"""(CS_Kandra_[A-Za-z0-9_]+)""\s*\.Translate")) {
            Assert.IsTrue(
                defined.Contains(used.Groups[1].Value),
                $"Dialog_KandraForms translates {used.Groups[1].Value}, which no Keyed file defines, "
                + "so the key name itself prints on screen."
            );
        }
    }

    private static void AssertNoRawMaterialNameLabel(string body, string method) {
        Assert.IsFalse(
            Regex.IsMatch(body, @"(Widgets\.Label\([^;]*,\s*name\s*\)|return\s+[^;]*\.name\s*;)"),
            $"{method} puts the value destructured out of TrueBodyMaterialFor on screen. That is "
            + "the internal save key, not a label."
        );
    }

    /// <summary>The body plus any same-file helper it calls, so delegating still counts as reaching a key.</summary>
    private static string WithHelpers(string source, string body) {
        IEnumerable<Match> helpers = Regex.Matches(source, @"private (?:static )?\w+\??\s+(\w+)\(")
            .Where(match => Regex.IsMatch(body, $@"\b{match.Groups[1].Value}\("));

        return body + string.Join(string.Empty, helpers.Select(match => BodyAt(source, match.Index)));
    }

    private static bool EveryTrueBodyCallPassesAGender(string source) {
        IEnumerable<Match> calls = Regex.Matches(source, @"(DrawTrueBody|DrawPortrait)\(")
            .Where(match => !IsDeclaration(source, match.Index));

        return calls.All(match => Arguments(source, match.Index + match.Length).Contains("gender", StringComparison.Ordinal));
    }

    private static bool IsDeclaration(string source, int index) {
        int lineStart = source.LastIndexOf('\n', index) + 1;

        return source[lineStart..index].Contains("private", StringComparison.Ordinal);
    }

    /// <summary>Text between the call's parentheses, so a call broken over lines still reads whole.</summary>
    private static string Arguments(string source, int afterOpenParen) {
        int depth = 1;
        int i = afterOpenParen;
        while (i < source.Length && depth > 0) {
            if (source[i] == '(') depth++;
            if (source[i] == ')') depth--;
            i++;
        }

        return source[afterOpenParen..(i - 1)];
    }

    private static string MethodBody(string source, string signature) {
        int start = source.IndexOf(signature, StringComparison.Ordinal);
        Assert.IsTrue(start >= 0, $"Dialog_KandraForms no longer declares {signature}.");

        return BodyAt(source, start);
    }

    [TestMethod]
    public void AReshapeThatNeverFinishesDoesNotLeaveItsDesignInTheSave() {
        string comp = File.ReadAllText(
            Path.Combine(
                RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "Kandra", "CompKandraForms.cs"
            )
        );

        int cancel = comp.IndexOf("public void CancelReshape", StringComparison.Ordinal);
        Assert.IsTrue(cancel >= 0, "CompKandraForms has no CancelReshape, so a dropped reshape cannot be cleaned up.");

        string body = BodyAt(comp, cancel);
        foreach (string field in new[] { "pendingMaterial", "pendingGender" }) {
            StringAssert.Contains(
                body,
                field + " = null",
                $"CancelReshape leaves {field} set, so a reshape the pawn never finished rides in the save forever."
            );
        }

        string job = File.ReadAllText(
            Path.Combine(
                RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "JobDriver", "KandraChangeShape.cs"
            )
        );

        // Drafted, downed or attacked mid-rearrange all end the job without ever reaching Change.
        Assert.IsTrue(
            job.Contains("AddFinishAction", StringComparison.Ordinal)
            && job.Contains("CancelReshape", StringComparison.Ordinal),
            "KandraChangeShape does not clear a pending reshape when the job ends early."
        );

        string gizmo = File.ReadAllText(
            Path.Combine(
                RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "Gene", "BodyAbsorption.cs"
            )
        );

        int start = gizmo.IndexOf("private void StartChange", StringComparison.Ordinal);
        Assert.IsTrue(start >= 0, "BodyAbsorption no longer starts the change job.");

        string starter = BodyAt(gizmo, start);
        Assert.IsTrue(
            starter.Contains("TryTakeOrderedJob", StringComparison.Ordinal)
            && starter.Contains("CancelReshape", StringComparison.Ordinal),
            "StartChange throws away the result of TryTakeOrderedJob. A refused job runs no toil, so "
            + "nothing fires the finish action and the design stays pending with no way to reach it."
        );
    }

    private static string BodyAt(string source, int start) =>
        source[start..source.IndexOf("\n    }", start, StringComparison.Ordinal)];
}
