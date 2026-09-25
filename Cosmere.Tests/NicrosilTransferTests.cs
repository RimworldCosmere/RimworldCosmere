using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Cosmere.System.Scadrial;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     A nicrosilmind converts at its own rate because its contents are Investiture rather than
///     an attribute. These check what that rate is worth in Heightenings, read off the defs.
/// </summary>
[TestClass]
public class NicrosilTransferTests {
    private const int FirstHeightening = 100;
    private const int SecondHeightening = 200;

    private static string RepoRoot {
        get {
            DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereScadrial", "Defs", "Feruchemy"))) {
                dir = dir.Parent;
            }

            Assert.IsNotNull(dir, "Could not locate CosmereScadrial/Defs/Feruchemy.");
            return dir.FullName;
        }
    }

    private static float CapacityOf(string defName) {
        string path = Path.Combine(
            RepoRoot, "CosmereScadrial", "Defs", "Feruchemy", "Things", "Metalminds", "Things.xml"
        );

        XElement? def = XDocument.Load(path).Root!
            .Elements("ThingDef")
            .FirstOrDefault(d => (string?)d.Element("defName") == defName);

        Assert.IsNotNull(def, $"{defName} not found");

        XElement? amount = def.Descendants("maxAmount").FirstOrDefault();
        Assert.IsNotNull(amount, $"{defName} declares no maxAmount");

        return float.Parse(amount.Value);
    }

    private static float BeuFor(string defName) {
        return CapacityOf(defName) * ScadrialMetallurgyConstants.NicrosilBeuPerCharge;
    }

    [TestMethod]
    public void AFullBandCarriesAPawnPastTheFirstHeightening() {
        Assert.IsGreaterThan(
            FirstHeightening,
            BeuFor("Cosmere_Scadrial_Thing_MetalmindBand"),
            "a full nicrosil band should clear the 1st Heightening"
        );
    }

    [TestMethod]
    public void ABraceletDoesNot() {
        Assert.IsLessThan(
            FirstHeightening,
            BeuFor("Cosmere_Scadrial_Thing_MetalmindBracelet"),
            "a bracelet should fall short of the 1st Heightening"
        );
    }

    [TestMethod]
    public void OnlyALegendaryBandReachesTheSecond() {
        Assert.IsLessThan(SecondHeightening, BeuFor("Cosmere_Scadrial_Thing_MetalmindBand"));
        Assert.IsGreaterThan(
            SecondHeightening,
            BeuFor("Cosmere_Scadrial_Thing_MetalmindBand") * 2f,
            "quality 2.0 doubles capacity, which should clear the 2nd"
        );
    }

    // Nicrosilminds read brighter because they hold Investiture, not an attribute.
    [TestMethod]
    public void NicrosilConvertsHigherThanAnOrdinaryMetalmind() {
        Assert.IsGreaterThan(
            ScadrialMetallurgyConstants.BreathEquivalentUnitsPerMetalmindUnit,
            ScadrialMetallurgyConstants.NicrosilBeuPerCharge
        );
    }

    // The four tests above pin the consequences; this pins the number they depend on.
    [TestMethod]
    public void TheConversionIsTheRateTheHeighteningsWereChosenAgainst() {
        Assert.AreEqual(0.125f, ScadrialMetallurgyConstants.NicrosilBeuPerCharge);
    }
}
