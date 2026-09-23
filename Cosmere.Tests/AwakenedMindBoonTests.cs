using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The boon sold Stormlight and its hediff sold paperwork, and neither named the doubled
///     Investiture absorption that is the largest thing either one grants.
/// </summary>
[TestClass]
public class AwakenedMindBoonTests {
    private const string BoonName = "NW_Boon_AwakenedMind";
    private const string HediffName = "Cosmere_Roshar_Hediff_NW_BoonPassive_AwokenMind";

    [TestMethod]
    public void TheBoonKeepsItsNameAndItsHediff() {
        XElement boon = Boon();

        Assert.AreEqual(HediffName, boon.Element("hediff")?.Value);
        Assert.IsFalse(Read("BoonDefs.xml").Contains("NW_Boon_Psylink"), "the legacy psylink name came back");

        // the name says awakened mind, so nothing here may quietly become a Royalty psylink grant
        Assert.IsNull(boon.Element("psylinkBoost"));
    }

    [TestMethod]
    public void BothHalvesNameWhatTheyActuallyGrant() {
        string boon = Boon().Element("description")!.Value;
        XElement hediff = Hediff();
        string text = hediff.Element("description")!.Value;

        // the boon grants Connection, so its own text has to carry Cultivation and the opened mind
        StringAssert.Contains(boon, "Cultivation");
        StringAssert.Contains(boon, "Patterns surface");

        // the hediff doubles absorption and speeds research; the description used to admit only one
        StringAssert.Contains(text, "Investiture");
        StringAssert.Contains(text, "Research");

        XElement stage = hediff.Element("stages")!.Elements("li").First();
        Assert.IsNotNull(stage.Element("statFactors")?.Element("Cosmere_InvestitureAbsorption"));
        Assert.IsNotNull(stage.Element("statOffsets")?.Element("ResearchSpeed"));
    }

    private static XElement Boon() {
        return Defs("BoonDefs.xml")
            .Elements()
            .Single(d => d.Element("defName")?.Value == BoonName);
    }

    private static XElement Hediff() {
        return Defs("HediffDefs.xml")
            .Elements("HediffDef")
            .Single(d => d.Element("defName")?.Value == HediffName);
    }

    private static XElement Defs(string file) {
        return XDocument.Parse(Read(file)).Root!;
    }

    private static string Read(string file) {
        DirectoryInfo? dir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "CosmereRoshar"))) {
            dir = dir.Parent;
        }

        Assert.IsNotNull(dir);

        return File.ReadAllText(Path.Combine(dir.FullName, "CosmereRoshar", "Defs", "Nightwatcher", file));
    }
}
