using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using Cosmere.System.Scadrial.Grid;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The vent's throw is tuned from the def, and every way it can be tuned wrong fails quietly:
///     an unknown metal is logged and dropped, a radius past the plume never buries, and a missing
///     field silently falls back to whatever the comp was compiled with.
/// </summary>
[TestClass]
public class AshVentThrowTests {
    private const string VentDefName = "Cosmere_Scadrial_Thing_AshVent";
    private const string CompClass = "Cosmere.System.Scadrial.Comp.Thing.CompProperties_AshVent";
    private const string FirstMetalKey = "CS_AshVent_FirstMetal";

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

    private static XElement VentComp() {
        foreach (XElement def in DefsOfType("ThingDef", Path.Combine(RepoRoot, "CosmereScadrial", "Defs"))) {
            if (def.Element("defName")?.Value.Trim() != VentDefName) continue;

            foreach (XElement li in def.Element("comps")?.Elements("li") ?? []) {
                if (li.Attribute("Class")?.Value.Trim() == CompClass) return li;
            }

            Assert.Fail($"{VentDefName} carries no {CompClass}, so it breathes no ash and throws no metal.");
        }

        Assert.Fail($"No ThingDef named {VentDefName} under CosmereScadrial/Defs.");
        return null;
    }

    private static IEnumerable<XElement> DefsOfType(string element, string dir) {
        foreach (string path in Directory.GetFiles(dir, "*.xml", SearchOption.AllDirectories)) {
            XElement? root = XDocument.Load(path).Root;
            if (root == null) continue;

            foreach (XElement def in root.Elements(element)) {
                yield return def;
            }
        }
    }

    /// <summary>
    ///     Every field the comp reads has to be written out, or tuning the throw means a rebuild.
    ///     The comp's own defaults are the trap: an omitted field looks tuned and is not.
    /// </summary>
    [TestMethod]
    public void EveryThrowFieldIsSetInTheDefRatherThanLeftToTheCompDefault() {
        XElement comp = VentComp();

        foreach (string field in new[] { "throwsPerDay", "lumpsPerThrow", "throwRadius", "metals", "clearRadius" }) {
            Assert.IsNotNull(comp.Element(field), $"{field} is not on the vent def, so it cannot be tuned without a rebuild");
        }
    }

    [TestMethod]
    public void TheThrowRateAndStackAreBothPositive() {
        XElement comp = VentComp();

        float perDay = float.Parse(comp.Element("throwsPerDay")!.Value.Trim(), CultureInfo.InvariantCulture);
        int lumps = int.Parse(comp.Element("lumpsPerThrow")!.Value.Trim());

        Assert.IsTrue(perDay > 0f, $"throwsPerDay is {perDay}, so the vent banks a throw that never comes due");
        Assert.IsTrue(lumps > 0, $"lumpsPerThrow is {lumps}, so a throw lands a stack of nothing");
    }

    /// <summary>
    ///     A lump outside the plume only ever gets the map's baseline fall, which takes long enough
    ///     that the burial the whole feature turns on stops being a pressure the player can feel.
    /// </summary>
    [TestMethod]
    public void LumpsLandInsideTheVentsOwnPlume() {
        int radius = int.Parse(VentComp().Element("throwRadius")!.Value.Trim());

        Assert.IsTrue(radius > 0, $"throwRadius is {radius}, so every lump lands on the vent itself");
        Assert.IsTrue(
            radius <= AshPlume.RadiusCells,
            $"throwRadius {radius} reaches past the plume's {AshPlume.RadiusCells} cells, where no vent ash falls"
        );
    }

    /// <summary>
    ///     Cadmium and friends exist as MetallicArtsMetalDefs too, and the comp resolves against
    ///     ThingDef alone - a name that only carries the other kind is logged and dropped at load.
    /// </summary>
    [TestMethod]
    public void EveryThrownMetalNamesALoadedThingDef() {
        HashSet<string> things = [];
        foreach (string mod in new[] { "CosmereCore", "CosmereScadrial" }) {
            foreach (XElement def in DefsOfType("ThingDef", Path.Combine(RepoRoot, mod, "Defs"))) {
                string? name = def.Element("defName")?.Value.Trim();
                if (name != null) things.Add(name);
            }
        }

        int counted = 0;
        foreach (XElement li in VentComp().Element("metals")!.Elements("li")) {
            string metal = li.Value.Trim();
            Assert.IsTrue(things.Contains(metal), $"the vent throws '{metal}', which is no ThingDef in Core or Scadrial");
            counted++;
        }

        Assert.IsTrue(counted > 0, "the vent lists no metals, so ThrowOnce returns before it places anything");
    }

    /// <summary>
    ///     The comp lives in Core and the key lives in Scadrial. The game merges the two, but only
    ///     while both are loaded, and a missing key prints its own name at the player.
    /// </summary>
    [TestMethod]
    public void TheFirstMetalMessageHasAKeyToReadFrom() {
        string path = Path.Combine(RepoRoot, "CosmereScadrial", "Languages", "English", "Keyed", "Messages.xml");
        Assert.IsTrue(File.Exists(path), "CosmereScadrial has no keyed Messages.xml");

        XElement? key = XDocument.Load(path).Root?.Element(FirstMetalKey);
        Assert.IsNotNull(key, $"{FirstMetalKey} is not defined, so the message shows the player its own key name");
        Assert.IsTrue(key.Value.Trim().Length > 0, $"{FirstMetalKey} is empty");
        Assert.IsFalse(key.Value.Contains('{'), $"{FirstMetalKey} carries a placeholder, but nothing passes it an argument");
    }
}
