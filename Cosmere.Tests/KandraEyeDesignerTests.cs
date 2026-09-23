using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Guards the finished Eyes tab: the four values the designer holds, the two sentinels that
///     turn a choice off, and the summary line the preview plate prints beside the portrait.
/// </summary>
[TestClass]
public class KandraEyeDesignerTests {
    private static readonly string[] EyesLabelKeys = [
        "CS_Kandra_IrisHeader",
        "CS_Kandra_OddEyesHeader",
        "CS_Kandra_OddEyesToggle",
        "CS_Kandra_OddEyesDesc",
        "CS_Kandra_EyeLightHeader",
        "CS_Kandra_EyeLightToggle",
        "CS_Kandra_EyeLightDesc",
    ];

    private static readonly string[] SummaryKeys = [
        "CS_Kandra_SummaryName",
        "CS_Kandra_SummaryBuild",
        "CS_Kandra_SummaryHair",
        "CS_Kandra_SummaryEyes",
        "CS_Kandra_SummaryEyesOdd",
        "CS_Kandra_SummaryEyesLit",
        "CS_Kandra_SummaryEyesOddLit",
    ];

    private static readonly string[] SummaryEyeKeys = [
        "CS_Kandra_SummaryEyes",
        "CS_Kandra_SummaryEyesOdd",
        "CS_Kandra_SummaryEyesLit",
        "CS_Kandra_SummaryEyesOddLit",
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

    private static string DialogPath => KandraSource("Dialog_KandraForms.cs");

    private static string EyesPath => KandraSource("Dialog_KandraForms.Eyes.cs");

    private static string DialogSource => File.ReadAllText(DialogPath);

    private static string EyesSource {
        get {
            Assert.IsTrue(
                File.Exists(EyesPath),
                "Dialog_KandraForms.Eyes.cs does not exist, so the Eyes tab's right column draws nothing."
            );

            return File.ReadAllText(EyesPath);
        }
    }

    /// <summary>A value the dialog holds but never sends is a control that silently does nothing.</summary>
    [TestMethod]
    public void AnEyeValueTheDesignerHoldsButNeverSendsIsAControlThatDoesNothing() {
        Match commit = Regex.Match(DialogSource, @"forms\.BeginReshape\(([^)]*)\)");
        Assert.IsTrue(commit.Success, "The designer no longer commits through BeginReshape.");

        string args = commit.Groups[1].Value;

        Assert.AreEqual(
            8,
            args.Split(',').Length,
            "BeginReshape takes eight values and the designer sends " + args.Split(',').Length
            + ". Every eye value the player picked has to travel with the reshape."
        );

        foreach (string part in new[] { "EyeColour", "ColourTwo", "Iris", "Light" }) {
            Assert.IsTrue(
                args.Contains(part, StringComparison.OrdinalIgnoreCase),
                "No argument to BeginReshape mentions " + part + ", so that pick never leaves the window."
            );
        }
    }

    /// <summary>Null means "changed nothing", so an off switch that sends it can never turn anything off.</summary>
    [TestMethod]
    public void TurningOddEyesOrTheLightOffHasToSendASentinelRatherThanNull() {
        string source = DialogSource + EyesSource;

        foreach (string sentinel in new[] { "KandraAppearance.EyeColourNone", "KandraAppearance.EyeLightOff" }) {
            StringAssert.Contains(
                source,
                sentinel,
                "The designer never sends " + sentinel
                + ". Null reads downstream as \"the player changed nothing\", so the player can turn this on and never off."
            );
        }
    }

    /// <summary>An unseeded value means opening the designer and pressing Reshape wipes an old choice.</summary>
    [TestMethod]
    public void EveryEyeValueOpensOnWhatTheKandraAlreadyIs() {
        string begin = Body("private void BeginDesign");

        foreach (string field in new[] { "eyeColourName", "eyeColourTwoName", "irisSizeName", "eyeLightName" }) {
            StringAssert.Contains(
                begin,
                field,
                "BeginDesign never reads " + field + " off the true body, so accepting the designer untouched wipes it."
            );
        }
    }

    /// <summary>The portrait pawn has no CompKandraForms, so anything not passed in cannot be drawn.</summary>
    [TestMethod]
    public void ThePreviewIsHandedEveryEyeValueBecauseItCanReadNoneOfThemItself() {
        Match call = Regex.Match(Body("private void DrawPreviewPlate"), @"DrawPortrait\(([^)]*)\)");
        Assert.IsTrue(call.Success, "The preview plate no longer draws a portrait.");

        string args = call.Groups[1].Value;

        foreach (string part in new[] { "EyeColour", "ColourTwo", "Iris", "Light" }) {
            Assert.IsTrue(
                args.Contains(part, StringComparison.OrdinalIgnoreCase),
                "The preview portrait is handed no " + part
                + ", and the stand-in pawn has no CompKandraForms to read it from."
            );
        }
    }

    /// <summary>A label written in the pane is a label that stays English in every other language.</summary>
    [TestMethod]
    public void EveryLabelInTheEyesPaneComesFromAKeyThatIsActuallyDefined() {
        string source = EyesSource;
        XElement root = XDocument.Load(GeneralKeyedPath).Root!;

        foreach (string key in EyesLabelKeys) {
            StringAssert.Contains(source, key, "The eyes pane no longer labels itself from " + key + ".");

            Assert.IsNotNull(
                root.Element(key),
                key + " is referenced by the eyes pane but missing from General.xml, so the player reads the key."
            );
        }
    }

    /// <summary>An unreferenced variant means one combination prints a different line than the player expects.</summary>
    [TestMethod]
    public void EverySummaryLineIsDefinedAndEveryEyeVariantIsReachable() {
        XElement root = XDocument.Load(GeneralKeyedPath).Root!;

        foreach (string key in SummaryKeys) {
            Assert.IsNotNull(root.Element(key), key + " is missing from General.xml, so the summary prints the key.");
        }

        string source = DialogSource;

        foreach (string key in SummaryEyeKeys) {
            Assert.IsTrue(
                Regex.IsMatch(source, "\"" + key + "\"(?!\\w)"),
                "The plate never reaches " + key
                + ", so that combination of odd eyes and light prints someone else's line."
            );
        }
    }

    /// <summary>Adding a row to a table should not need a code change, and a wide iris was cut once already.</summary>
    [TestMethod]
    public void TheIrisAndLightPickersReadTheirTablesRatherThanCountingThemselves() {
        string source = EyesSource;

        foreach (string table in new[] { "KandraAppearance.AllIrisSizes", "KandraAppearance.AllEyeLights" }) {
            StringAssert.Contains(
                source,
                table,
                "The eyes pane writes its own rows instead of reading " + table + "."
            );
        }
    }

    /// <summary>A colour written here dodges the palette the rest of the designer is checked against.</summary>
    [TestMethod]
    public void TheEyesPaneInventsNoColourOfItsOwnAndLoadsNoTextureWhileDrawing() {
        string source = EyesSource;

        foreach (Match match in Regex.Matches(source, @"new Color\s*\(|#[0-9a-fA-F]{6}")) {
            Assert.Fail(
                "Dialog_KandraForms.Eyes writes its own colour at offset " + match.Index
                + ". Every colour here comes from BaseWindow or KandraAppearance."
            );
        }

        Assert.IsFalse(
            source.Contains("ContentFinder", StringComparison.Ordinal),
            "The eyes pane loads a texture inside a draw call, which hits the disk every frame."
        );
    }

    /// <summary>The whole canvas in a 24px chip put Small and Standard a fifth of a pixel apart.</summary>
    [TestMethod]
    public void TheIrisChipCropsHardEnoughForTheSizesToTellThemselvesApart() {
        string source = EyesSource;

        StringAssert.Contains(
            source,
            "DrawTextureWithTexCoords",
            "The iris chip draws the whole canvas again, so every size renders as the same speck."
        );

        float canvas = Constant(source, "IrisCanvasSize");
        float window = Constant(source, "IrisChipWindow");
        float chip = Constant(source, "IrisChipSize");

        Assert.AreEqual(
            PngWidth(IrisArtPath),
            (int)canvas,
            "IrisCanvasSize no longer matches the shipped iris art, so the crop lands somewhere else."
        );

        // The painted marks measure six canvas pixels across at Small and eight at Standard.
        float difference = (8f - 6f) / window * chip;

        Assert.IsTrue(
            difference >= 1f,
            "Small and Standard differ by " + difference + " chip pixels, so the player picks blind."
        );
    }

    private static float Constant(string source, string name) {
        Match match = Regex.Match(source, @"const float " + name + @" = ([0-9.]+)f;");
        Assert.IsTrue(match.Success, "Dialog_KandraForms.Eyes no longer declares " + name + ".");

        return float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
    }

    /// <summary>Width lives in the IHDR chunk, eight bytes past the signature plus length and type.</summary>
    private static int PngWidth(string path) {
        byte[] header = new byte[24];
        using (FileStream file = File.OpenRead(path)) {
            Assert.AreEqual(header.Length, file.Read(header, 0, header.Length), path + " is not a PNG.");
        }

        return (header[16] << 24) | (header[17] << 16) | (header[18] << 8) | header[19];
    }

    private static string IrisArtPath => Path.Combine(
        RepoRoot,
        "CosmereScadrial",
        "Assets",
        "Textures",
        "Things",
        "Pawn",
        "Humanlike",
        "HeadAttachments",
        "KandraEyes",
        "Kandra_Eyes_Male_Standard_Left_south.png"
    );

    private static string GeneralKeyedPath => Path.Combine(
        RepoRoot,
        "CosmereScadrial",
        "Languages",
        "English",
        "Keyed",
        "General.xml"
    );

    private static string KandraSource(string file) => Path.Combine(
        RepoRoot,
        "CosmereCore",
        "CosmereCore",
        "System",
        "Scadrial",
        "Kandra",
        file
    );

    private static string Body(string signature) {
        string source = DialogSource;
        int start = source.IndexOf(signature, StringComparison.Ordinal);
        Assert.IsTrue(start >= 0, "Dialog_KandraForms no longer declares " + signature + ".");

        int end = source.IndexOf("\n    }", start, StringComparison.Ordinal);
        Assert.IsTrue(end > start, "Could not read the body of " + signature + ".");

        return source[start..end];
    }
}
