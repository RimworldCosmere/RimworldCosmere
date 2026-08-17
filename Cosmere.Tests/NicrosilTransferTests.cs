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
            return dir!.FullName;
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

        XElement? amount = def!.Descendants("maxAmount").FirstOrDefault();
        Assert.IsNotNull(amount, $"{defName} declares no maxAmount");

        return float.Parse(amount!.Value);
    }

    private static float BeuFor(string defName) {
        return CapacityOf(defName) * ScadrialMetallurgyConstants.NicrosilBeuPerCharge;
    }

    [TestMethod]
    public void AFullBandCarriesAPawnPastTheFirstHeightening() {
        Assert.IsTrue(
            BeuFor("Cosmere_Scadrial_Thing_MetalmindBand") > FirstHeightening,
            "a full nicrosil band should clear the 1st Heightening"
        );
    }

    [TestMethod]
    public void ABraceletDoesNot() {
        Assert.IsTrue(
            BeuFor("Cosmere_Scadrial_Thing_MetalmindBracelet") < FirstHeightening,
            "a bracelet should fall short of the 1st Heightening"
        );
    }

    [TestMethod]
    public void OnlyALegendaryBandReachesTheSecond() {
        Assert.IsTrue(BeuFor("Cosmere_Scadrial_Thing_MetalmindBand") < SecondHeightening);
        Assert.IsTrue(
            BeuFor("Cosmere_Scadrial_Thing_MetalmindBand") * 2f > SecondHeightening,
            "quality 2.0 doubles capacity, which should clear the 2nd"
        );
    }

    // A nicrosilmind reads brighter to bronze than a steelmind holding the same charge,
    // because its contents are Investiture. That is the point, not a rounding error.
    [TestMethod]
    public void NicrosilConvertsHigherThanAnOrdinaryMetalmind() {
        Assert.IsTrue(
            ScadrialMetallurgyConstants.NicrosilBeuPerCharge >
            ScadrialMetallurgyConstants.BreathEquivalentUnitsPerMetalmindUnit
        );
    }
}
