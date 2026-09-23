using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Roshar's dialogs and comps were full of hardcoded English. Both guards here fail the
///     moment that comes back: one catches a raw literal at a call site, the other catches a
///     CRO_ key that no Keyed file defines, which the game renders as the key name itself.
/// </summary>
[TestClass]
public class RosharLocalizationTests {
    private static readonly string[] GuardedFiles = [
        "System/Roshar/Dialog/Dialog_GemheartExpedition.cs",
        "System/Roshar/Dialog/Dialog_RadiantOrderDialogBase.cs",
        "System/Roshar/Dialog/Dialog_RadiantOrderInfoDialog.cs",
        "System/Roshar/Dialog/Dialog_ChooseRadiantOrder.cs",
        "System/Roshar/Comp/Fabrials/BasicFabrial.cs",
        "System/Roshar/Comp/Fabrials/FabrialPowerGenerator.cs",
        "System/Roshar/Comp/Thing/SprenContainer.cs",
        "System/Roshar/Surgebinding/Ability/Gravitation/BasicLashing.cs",
        "System/Roshar/Surgebinding/Ability/Transformation/Soulcast.cs",
        "System/Roshar/Surgebinding/Ability/Transportation/Portal.cs",
    ];

    /// <summary>
    ///     First argument after the rect, so a literal in that slot is the visible label.
    /// </summary>
    private static readonly Regex PlayerFacingLiteral = new Regex(
        @"(?:Widgets\.(?:Label|ButtonText|CheckboxLabeled)|TooltipHandler\.TipRegion)\s*\([^,)]+,\s*""[A-Z]"
        + @"|Messages\.Message\s*\(\s*""[A-Z]"
        + @"|new FloatMenuOption\s*\(\s*""[A-Z]"
        + @"|default(?:Label|Desc)\s*=\s*""[A-Z]",
        RegexOptions.Compiled
    );

    private static readonly Regex CroKeyReference = new Regex(@"""(CRO_[A-Za-z0-9_]+)""", RegexOptions.Compiled);

    private static string RepoRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereRoshar", "Languages"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not locate CosmereRoshar/Languages above the test output directory.");
            return dir.FullName;
        }
    }

    private static string SourcePath(string relative) {
        return Path.Combine(RepoRoot, "CosmereCore", "CosmereCore", relative.Replace('/', Path.DirectorySeparatorChar));
    }

    [TestMethod]
    public void EveryGuardedFileExists() {
        foreach (string relative in GuardedFiles) {
            Assert.IsTrue(File.Exists(SourcePath(relative)), $"{relative} moved. Update GuardedFiles.");
        }
    }

    [TestMethod]
    public void NoPlayerFacingCallTakesARawEnglishLiteral() {
        List<string> offenders = [];

        foreach (string relative in GuardedFiles) {
            string[] lines = File.ReadAllLines(SourcePath(relative));
            for (int i = 0; i < lines.Length; i++) {
                if (lines[i].Contains(".Translate(")) continue;
                if (!PlayerFacingLiteral.IsMatch(lines[i])) continue;

                offenders.Add($"{relative}:{i + 1}: {lines[i].Trim()}");
            }
        }

        Assert.AreEqual(
            0,
            offenders.Count,
            "Player-facing text must come from a CRO_ key:\n" + string.Join("\n", offenders)
        );
    }

    [TestMethod]
    public void EveryReferencedCroKeyIsDefined() {
        HashSet<string> defined = [];
        string keyedDir = Path.Combine(RepoRoot, "CosmereRoshar", "Languages", "English", "Keyed");
        Assert.IsTrue(Directory.Exists(keyedDir), $"Keyed folder is missing at {keyedDir}.");

        foreach (string path in Directory.GetFiles(keyedDir, "*.xml", SearchOption.AllDirectories)) {
            foreach (XElement element in XDocument.Load(path).Root?.Elements() ?? []) {
                defined.Add(element.Name.LocalName);
            }
        }

        List<string> missing = [];
        foreach (string relative in GuardedFiles) {
            string source = File.ReadAllText(SourcePath(relative));
            foreach (Match match in CroKeyReference.Matches(source)) {
                string key = match.Groups[1].Value;
                if (!defined.Contains(key)) missing.Add($"{relative}: {key}");
            }
        }

        Assert.AreEqual(
            0,
            missing.Count,
            "Referenced keys with no definition render as the key text in game:\n" + string.Join("\n", missing)
        );
    }

    /// <summary>
    ///     Glyph icons carry no meaning for a screen reader and vanish in some fonts, so status
    ///     words replaced them. The tick mark must not come back.
    /// </summary>
    [TestMethod]
    public void IdealStatusUsesWordsNotGlyphs() {
        string source = File.ReadAllText(SourcePath("System/Roshar/Dialog/Dialog_RadiantOrderDialogBase.cs"));

        Assert.IsFalse(source.Contains('✓'), "A check glyph is back in the Ideals timeline.");
    }
}
