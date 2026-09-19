using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     How a designed eye is stored and carried: by palette name, through all three halves of a
///     reshape, with sentinels for the two things null cannot say.
/// </summary>
[TestClass]
public class KandraEyeStorageTests {
    private static string RepoRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not find the repo root from the test output directory.");

            return dir!.FullName;
        }
    }

    private static string KandraDir =>
        Path.Combine(RepoRoot, "CosmereCore", "CosmereCore", "System", "Scadrial", "Kandra");

    private static string CompSource => File.ReadAllText(Path.Combine(KandraDir, "CompKandraForms.cs"));

    private static string FormSource => File.ReadAllText(Path.Combine(KandraDir, "KandraForm.cs"));

    /// <summary>Every pending value a reshape carries, and the field each one lands in.</summary>
    private static readonly (string pending, string what)[] PendingFields = [
        ("pendingMaterial", "the material"),
        ("pendingGender", "the gender"),
        ("pendingHair", "the hair"),
        ("pendingHairColour", "the hair colour"),
        ("pendingEyeColour", "the eye colour"),
        ("pendingEyeColourTwo", "the second eye colour"),
        ("pendingIrisSize", "the iris size"),
        ("pendingEyeLight", "the eye light"),
    ];

    private static readonly string[] EyePendingFields = [
        "pendingEyeColour",
        "pendingEyeColourTwo",
        "pendingIrisSize",
        "pendingEyeLight",
    ];

    private static readonly string[] EyeFormFields = [
        "eyeColourName",
        "eyeColourTwoName",
        "irisSizeName",
        "eyeLightName",
    ];

    [TestMethod]
    public void AFieldOneHalfOfTheReshapeForgetsNeverLandsOrNeverLeaves() {
        string comp = CompSource;
        string begin = WithLocalCalls(comp, "BeginReshape");
        string commit = WithLocalCalls(comp, "CommitReshape");
        string cancel = WithLocalCalls(comp, "CancelReshape");

        foreach ((string pending, string what) in PendingFields) {
            Assert.IsTrue(
                begin.Contains(pending, StringComparison.Ordinal),
                $"BeginReshape never records {pending}, so {what} the player picked is gone before "
                + "the job even starts."
            );

            Assert.IsTrue(
                commit.Contains(pending, StringComparison.Ordinal),
                $"CommitReshape never reads {pending}, so {what} never lands on the true body. The "
                + "player waits out the job and nothing changes."
            );

            Assert.IsTrue(
                cancel.Contains(pending, StringComparison.Ordinal),
                $"CancelReshape never clears {pending}, so {what} from an abandoned reshape rides in "
                + "the save forever and commits itself on the next unrelated job."
            );
        }
    }

    [TestMethod]
    public void AnEyeStoredAsAnythingButANameFreezesOrLosesItself() {
        string comp = CompSource;

        foreach (string field in EyePendingFields) {
            Assert.IsTrue(
                Regex.IsMatch(comp, $@"private\s+string\?\s+{field}\s*;"),
                $"CompKandraForms.{field} is not a string?. A nullable struct through "
                + "Scribe_Values.Look is not proven to round-trip on this comp, and a stored Color "
                + "freezes a hex the palette may retune or drop."
            );

            Assert.IsTrue(
                Regex.IsMatch(comp, $@"Scribe_Values\.Look\(\s*ref\s+{field}\b"),
                $"CompKandraForms.{field} is not scribed with Scribe_Values.Look, so a reshape in "
                + "flight across a save dies half-applied."
            );
        }
    }

    [TestMethod]
    public void WithoutASentinelOddEyesAndTheLightGoOnAndNeverOff() {
        string commit = WithLocalCalls(CompSource, "CommitReshape");

        Assert.IsTrue(
            commit.Contains("EyeColourNone", StringComparison.Ordinal),
            "CommitReshape does not handle KandraAppearance.EyeColourNone for the second eye "
            + "colour. Null already means \"the player changed nothing\", so without the sentinel odd "
            + "eyes can be turned on and never off."
        );

        Assert.IsTrue(
            commit.Contains("EyeLightOff", StringComparison.Ordinal),
            "CommitReshape does not handle KandraAppearance.EyeLightOff for the eye light. Null "
            + "already means \"the player changed nothing\", so without the sentinel a lit eye can "
            + "never be put out."
        );

        Assert.IsFalse(
            commit.Contains("\"off\"", StringComparison.Ordinal)
            || commit.Contains("\"none\"", StringComparison.Ordinal),
            "CommitReshape spells a sentinel out as a literal. Retuning the const in "
            + "KandraAppearance would then leave the two files disagreeing."
        );
    }

    [TestMethod]
    public void AnEyeFieldTheSaveDropsComesBackAsSomeoneElsesColour() {
        string form = FormSource;
        string expose = MethodBody(form, "ExposeData");

        foreach (string field in EyeFormFields) {
            Assert.IsTrue(
                Regex.IsMatch(expose, $@"Scribe_Values\.Look\(\s*ref\s+{field}\b"),
                $"KandraForm.{field} is not scribed in ExposeData, so the designed eye is gone on "
                + "reload and the kandra comes back with whatever the fallback draws."
            );
        }

        Assert.IsFalse(
            Regex.IsMatch(form, @"(?m)^\s*public\s+Color\s+eyeColour(Two)?\s*;"),
            "KandraForm caches an eye Color again. The name is the only storage: a cached colour is "
            + "transparent black whenever odd eyes are off, and stale once the palette retunes."
        );
    }

    [TestMethod]
    public void SyncingAnEyeOntoPawnStoryWouldRepaintADisguiseTheSameWayHairDid() {
        foreach ((string file, string source) in new[] {
                     ("KandraForm.cs", FormSource),
                     ("CompKandraForms.cs", CompSource),
                 }) {
            Match write = Regex.Match(source, @"story[\w.?]*\.\w*[Ee]ye\w*\s*=");
            Assert.IsFalse(
                write.Success,
                $"{file} writes an eye value onto pawn.story ({write.Value}). Eyes are read off the "
                + "form at render time on purpose; a sync path reintroduces the class of bug the hair "
                + "build hit, where a worn disguise got repainted with the true body's colour."
            );

            Assert.IsFalse(
                source.Contains("SyncTrueBodyEyeColour", StringComparison.Ordinal),
                $"{file} declares or calls SyncTrueBodyEyeColour. The hair needed that hop because "
                + "the map pawn renders from story.HairColor. Eyes have no story field to drift out "
                + "of sync with, so the method can only blow a kandra's cover."
            );
        }
    }

    [TestMethod]
    public void AnUnlitEyeReadsBackAsLitAtSteady() {
        string path = Path.Combine(
            RepoRoot,
            "CosmereCore",
            "CosmereCore",
            "System",
            "Scadrial",
            "Util",
            "KandraAppearance.cs"
        );
        string body = MethodBody(File.ReadAllText(path), "EyeLightStrengthFor");

        Assert.IsTrue(
            Regex.IsMatch(body, @"name is null or EyeLightOff\s*\)\s*return 1f;"),
            "EyeLightStrengthFor does not send null and \"off\" to 1f. Neither is a row in the "
            + "table, so both fall through to the steady fallback and every undesigned or "
            + "put-out kandra reads back lit."
        );
    }

    /// <summary>A method body plus the bodies of the same class's methods it calls straight.</summary>
    private static string WithLocalCalls(string source, string method) {
        string body = MethodBody(source, method);
        string all = body;
        foreach (Match call in Regex.Matches(body, @"(?<![.\w])(\w+)\(\s*\)\s*;")) {
            string name = call.Groups[1].Value;
            if (name == method) continue;
            int declared = source.IndexOf($" {name}() {{", StringComparison.Ordinal);
            if (declared < 0) continue;

            int end = source.IndexOf("\n    }", declared, StringComparison.Ordinal);
            if (end > declared) all += source[declared..end];
        }

        return all;
    }

    private static string MethodBody(string source, string method) {
        int start = source.IndexOf($" {method}(", StringComparison.Ordinal);
        Assert.IsTrue(start >= 0, $"No {method} is declared where this test expects it.");

        int end = source.IndexOf("\n    }", start, StringComparison.Ordinal);
        Assert.IsTrue(end > start, $"Could not read the body of {method}.");

        return source[start..end];
    }
}
