using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Cosmere.System.Scadrial.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     A mask's filtration only means something next to the rate it fights. Above the break-even
///     fraction a masked pawn heals at the vent mouth, which is a different feature from slowing.
/// </summary>
[TestClass]
public class AshMaskDefTests {
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

    private static float ClothMaskFiltration {
        get {
            string path = Path.Combine(RepoRoot, "CosmereScadrial", "Defs", "Things", "Apparel", "AshMask.xml");
            Assert.IsTrue(File.Exists(path), $"AshMask.xml is missing at {path}.");

            XElement? offset = XDocument.Load(path).Root?.Elements("ThingDef")
                .FirstOrDefault(d => d.Element("defName")?.Value == "Cosmere_Scadrial_Apparel_AshMask")
                ?.Element("equippedStatOffsets")
                ?.Element("Cosmere_Scadrial_Stat_AshFiltration");

            Assert.IsNotNull(offset, "The ash mask no longer grants Cosmere_Scadrial_Stat_AshFiltration.");

            return float.Parse(offset.Value, CultureInfo.InvariantCulture);
        }
    }

    private static float GasMaskFiltration {
        get {
            string path = Path.Combine(RepoRoot, "CosmereScadrial", "Patches", "GasMask_AshFiltration.xml");
            Assert.IsTrue(File.Exists(path), $"GasMask_AshFiltration.xml is missing at {path}.");

            XElement? offset = XDocument.Load(path).Descendants("Cosmere_Scadrial_Stat_AshFiltration")
                .FirstOrDefault();

            Assert.IsNotNull(offset, "The gas mask patch no longer grants Cosmere_Scadrial_Stat_AshFiltration.");

            return float.Parse(offset.Value, CultureInfo.InvariantCulture);
        }
    }

    [TestMethod]
    public void TheClothMaskSlowsAshLungWithoutStoppingIt() {
        float delta = AshLungMath.SeverityDeltaPerHour(1f, ClothMaskFiltration);

        Assert.IsTrue(
            delta > 0f,
            $"filtration {ClothMaskFiltration} gives {delta}/hour at the mouth - a masked pawn heals there, "
            + "which makes the cloth mask immunity rather than a delay."
        );
    }

    [TestMethod]
    public void TheClothMaskIsStillWorthWearing() {
        float bare = AshLungMath.SeverityDeltaPerHour(1f, 0f);
        float masked = AshLungMath.SeverityDeltaPerHour(1f, ClothMaskFiltration);

        Assert.IsTrue(masked < bare, $"masked {masked}/hour is no better than bare {bare}/hour.");
    }

    [TestMethod]
    public void TheGasMaskStopsAshLungOutright() {
        float delta = AshLungMath.SeverityDeltaPerHour(1f, GasMaskFiltration);

        Assert.IsTrue(
            delta <= 0f,
            $"filtration {GasMaskFiltration} gives {delta}/hour at the mouth - the gas mask is meant to be "
            + "the one that stops it."
        );
    }

    [TestMethod]
    public void TheGasMaskBeatsTheClothMask() {
        Assert.IsTrue(
            GasMaskFiltration > ClothMaskFiltration,
            $"gas mask {GasMaskFiltration} should filter more than the cloth mask {ClothMaskFiltration}."
        );
    }
}
