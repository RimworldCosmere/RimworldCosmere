using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Cosmere.System.Roshar.Dialog;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Filmstrip geometry: the sigil rail stays centred and the arrow columns stay off it.
/// </summary>
[TestClass]
public class RadiantOrderFilmstripTests {
    private const float SpacingUnit = 16f;
    private const float WindowWidth = SpacingUnit * 65f;
    private const float FooterPadding = SpacingUnit;
    private const float RailLeftEdge = FooterPadding;
    private const float RailRightEdge = WindowWidth - FooterPadding;
    private const float RailCenterX = (RailLeftEdge + RailRightEdge) / 2f;

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

    [TestMethod]
    public void RailWidthCountsDotsAndGaps() {
        Assert.AreEqual(0f, RadiantFilmstripLayout.RailWidth(0), 0.001f);
        Assert.AreEqual(RadiantFilmstripLayout.DotSize, RadiantFilmstripLayout.RailWidth(1), 0.001f);

        for (int count = 2; count <= 12; count++) {
            float expected = count * RadiantFilmstripLayout.DotSize
                             + (count - 1) * RadiantFilmstripLayout.DotGap;
            Assert.AreEqual(expected, RadiantFilmstripLayout.RailWidth(count), 0.001f, $"count {count}");
        }
    }

    [TestMethod]
    public void DotGroupIsCentredForEveryCount() {
        for (int count = 1; count <= 12; count++) {
            float first = RadiantFilmstripLayout.DotCenterX(RailCenterX, count, 0);
            float last = RadiantFilmstripLayout.DotCenterX(RailCenterX, count, count - 1);

            Assert.AreEqual(RailCenterX, (first + last) / 2f, 0.001f, $"count {count} is off centre");
        }
    }

    [TestMethod]
    public void EveryDotLandsInsideTheRail() {
        for (int count = 1; count <= 12; count++) {
            for (int selected = 0; selected < count; selected++) {
                for (int i = 0; i < count; i++) {
                    float half = Half(i, selected);
                    float center = RadiantFilmstripLayout.DotCenterX(RailCenterX, count, i);

                    Assert.IsTrue(
                        center - half >= RailLeftEdge,
                        $"dot {i} of {count} (selected {selected}) runs off the left of the rail"
                    );
                    Assert.IsTrue(
                        center + half <= RailRightEdge,
                        $"dot {i} of {count} (selected {selected}) runs off the right of the rail"
                    );
                    Assert.IsTrue(
                        half * 2f <= RadiantFilmstripLayout.RailHeight() + 0.001f,
                        $"dot {i} of {count} is taller than the rail"
                    );
                }
            }
        }
    }

    [TestMethod]
    public void EnlargedDotNeverTouchesItsNeighbour() {
        for (int count = 2; count <= 12; count++) {
            for (int selected = 0; selected < count; selected++) {
                for (int i = 0; i < count - 1; i++) {
                    float leftEdge = RadiantFilmstripLayout.DotCenterX(RailCenterX, count, i)
                                     + Half(i, selected);
                    float rightEdge = RadiantFilmstripLayout.DotCenterX(RailCenterX, count, i + 1)
                                      - Half(i + 1, selected);

                    Assert.IsTrue(rightEdge > leftEdge, $"dots {i} and {i + 1} of {count} overlap");
                }
            }
        }
    }

    [TestMethod]
    public void ArrowColumnsClearTheRail() {
        for (int count = 1; count <= 12; count++) {
            float railLeft = RadiantFilmstripLayout.RailLeft(RailCenterX, count);
            float rightArrow = RadiantFilmstripLayout.RightArrowLeft(0f, WindowWidth);

            Assert.IsTrue(
                railLeft >= RadiantFilmstripLayout.ArrowColumnWidth,
                $"the left arrow column covers the first dot at count {count}"
            );
            Assert.IsTrue(
                rightArrow >= railLeft + RadiantFilmstripLayout.RailWidth(count),
                $"the right arrow column covers the last dot at count {count}"
            );
        }
    }

    [TestMethod]
    public void ArrowColumnsStopAtTheRail() {
        const float contentTop = 256f;
        const float railTop = 900f;

        float height = RadiantFilmstripLayout.ArrowColumnHeight(contentTop, railTop);

        Assert.AreEqual(railTop - contentTop, height, 0.001f);
        Assert.AreEqual(railTop, contentTop + height, 0.001f, "an arrow column must end where the rail begins");
        Assert.AreEqual(
            0f,
            RadiantFilmstripLayout.ArrowColumnHeight(contentTop, contentTop - 50f),
            0.001f,
            "a footer taller than the window must not give the column a negative height"
        );
    }

    [TestMethod]
    public void RightArrowColumnSitsOnTheRightEdge() {
        float left = RadiantFilmstripLayout.RightArrowLeft(20f, WindowWidth);

        Assert.AreEqual(20f + WindowWidth, left + RadiantFilmstripLayout.ArrowColumnWidth, 0.001f);
    }

    [TestMethod]
    public void FilmstripKeysAreDefinedInRosharKeyed() {
        Dictionary<string, string> keys = RosharKeys();

        AssertKey(keys, "CRO_RadiantOrder_Position", "{INDEX}", "{TOTAL}");
        AssertKey(keys, "CRO_RadiantOrder_Sigil_Blocked", "{ORDER}", "{REASON}");
    }

    [TestMethod]
    public void EdgeArrowsReuseTheExistingNavKeys() {
        Dictionary<string, string> keys = RosharKeys();

        Assert.IsTrue(keys.ContainsKey("CRO_RadiantOrder_Nav_Previous"));
        Assert.IsTrue(keys.ContainsKey("CRO_RadiantOrder_Nav_Next"));
    }

    private static float Half(int index, int selectedIndex) {
        float ring = index == selectedIndex ? RadiantFilmstripLayout.SelectedRingWidth : 0f;

        return RadiantFilmstripLayout.DotSizeAt(index, selectedIndex) / 2f + ring;
    }

    private static void AssertKey(Dictionary<string, string> keys, string key, params string[] placeholders) {
        Assert.IsTrue(keys.ContainsKey(key), $"{key} is not defined under CosmereRoshar/Languages/English/Keyed.");

        for (int i = 0; i < placeholders.Length; i++) {
            Assert.IsTrue(
                keys[key].Contains(placeholders[i]),
                $"{key} is missing the {placeholders[i]} placeholder."
            );
        }
    }

    /// <summary>Every key in the folder, because the game merges the folder rather than one file.</summary>
    private static Dictionary<string, string> RosharKeys() {
        Dictionary<string, string> keys = new Dictionary<string, string>(StringComparer.Ordinal);
        string folder = Path.Combine(RepoRoot, "CosmereRoshar", "Languages", "English", "Keyed");

        foreach (string file in Directory.GetFiles(folder, "*.xml", SearchOption.AllDirectories)) {
            XElement? root = XDocument.Load(file).Root;
            if (root == null) continue;

            foreach (XElement element in root.Elements()) {
                Assert.IsFalse(
                    keys.ContainsKey(element.Name.LocalName),
                    $"{element.Name.LocalName} is defined twice in the Roshar Keyed folder."
                );
                keys[element.Name.LocalName] = element.Value;
            }
        }

        return keys;
    }

    /// <summary>
    ///     UI.RotateAroundPivot rescales a pivot that is already in UI space, so anything rotated
    ///     through it drifts at UI scale above 1 and looks perfect at 1.0.
    /// </summary>
    [TestMethod]
    public void NoUiCodeRotatesAtDrawTime() {
        string root = Path.Combine(RepoRoot, "CosmereCore", "CosmereCore");
        List<string> offenders = [];

        foreach (string file in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)) {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")) continue;

            foreach (string line in File.ReadAllLines(file)) {
                string trimmed = line.Trim();
                if (trimmed.StartsWith("//", StringComparison.Ordinal)) continue;
                if (trimmed.StartsWith("///", StringComparison.Ordinal)) continue;

                if (trimmed.Contains("Widgets.DrawTextureRotated", StringComparison.Ordinal)
                    || trimmed.Contains("UI.RotateAroundPivot", StringComparison.Ordinal)) {
                    offenders.Add($"{Path.GetRelativePath(RepoRoot, file)}  {trimmed}");
                }
            }
        }

        Assert.AreEqual(
            0,
            offenders.Count,
            "Generate the glyph already turned, or call GUIUtility.RotateAroundPivot:\n"
            + string.Join("\n", offenders)
        );
    }

    /// <summary>
    ///     The arrows point the way they travel, and the dock folds downward.
    /// </summary>
    [TestMethod]
    public void TheChevronSheetsAreGeneratedPreTurned() {
        string tex = File.ReadAllText(Path.Combine(
            RepoRoot, "CosmereCore", "CosmereCore", "Core", "UI", "Dock", "DockTex.cs"
        ));

        foreach (string name in new[] { "Chevron", "ChevronLeft", "ChevronRight" }) {
            Assert.IsTrue(
                tex.Contains($"Texture2D {name} = BuildChevron(", StringComparison.Ordinal),
                $"{name} has to come out of the generator already facing the right way."
            );
        }

        string dialog = File.ReadAllText(Path.Combine(
            RepoRoot, "CosmereCore", "CosmereCore", "System", "Roshar", "Dialog", "Dialog_ChooseRadiantOrder.cs"
        ));
        Assert.IsTrue(
            dialog.Contains("DockTex.ChevronLeft : DockTex.ChevronRight", StringComparison.Ordinal),
            "The arrows pick a sheet rather than rotating one."
        );
    }
}
