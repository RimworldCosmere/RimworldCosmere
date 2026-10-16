using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     The dock and the wheel are the two surfaces a player touches every session, so a dead
///     hit area or a silent hover there is felt more than anywhere else in the mod.
/// </summary>
[TestClass]
public class DockAndRadialAffordanceTests {
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

    private static string Core(params string[] parts) {
        return File.ReadAllText(Path.Combine([RepoRoot, "CosmereCore", "CosmereCore", .. parts]));
    }

    private static string DockWindow => Core("Core", "UI", "Dock", "InvestitureDockWindow.cs");

    /// <summary>
    ///     ContractedBy shrank a 24px button to a 14px target, and the icon kept its full size,
    ///     so the dead ring around it looked clickable.
    /// </summary>
    [TestMethod]
    public void TheCollapseButtonUsesItsWholeRect() {
        Assert.DoesNotContain(
            "ButtonImage(pinRect.ContractedBy",
            DockWindow,
            "Contracting the rect leaves a ring that looks clickable and is not."
        );
        Assert.Contains(
            "Widgets.ButtonImage(pinRect, DockTex.Chevron",
            DockWindow,
            "The collapse button takes the full pinRect."
        );
    }

    /// <summary>
    ///     The dock folds, it does not close, and vanilla's X says the opposite.
    /// </summary>
    [TestMethod]
    public void TheDockFoldsWithItsOwnIconRatherThanVanillasCloseX() {
        Assert.DoesNotContain(
            "TexButton.CloseXSmall",
            DockWindow,
            "CloseXSmall promises the panel goes away. It folds."
        );

        string tex = Core("Core", "UI", "Dock", "DockTex.cs");
        Assert.Contains("public static readonly Texture2D Chevron", tex, "The mod owns the glyph.");
        Assert.Contains(
            "row 0 is the bottom",
            tex,
            "Texture2D row 0 is the bottom; without that the chevron renders upside down."
        );
    }

    /// <summary>
    ///     Every other clickable row in the mod answers on hover. The ribbons did not.
    /// </summary>
    [TestMethod]
    public void TheCollapsedRibbonsAnswerOnHover() {
        Assert.Contains("Widgets.DrawHighlightIfMouseover(ribbon)", DockWindow, "Ribbons highlight.");
        Assert.Contains("MouseoverSounds.DoRegion(ribbon)", DockWindow, "Ribbons click audibly.");
    }

    /// <summary>
    ///     Wedges are angular, not rects, so DoRegion cannot reach them. The tick has to fire on
    ///     the hovered index changing instead.
    /// </summary>
    [TestMethod]
    public void TheWheelTicksWhenTheHoveredWedgeChanges() {
        string radial = Core("Core", "UI", "Radial", "RadialWindow.cs");

        Assert.Contains("int hoveredBefore = state.HoveredIndex;", radial, "The previous wedge is kept.");
        Assert.Contains(
            "state.HoveredIndex >= 0 && state.HoveredIndex != hoveredBefore",
            radial,
            "Only a change onto a real wedge ticks, or leaving the wheel would rattle."
        );
    }

    /// <summary>
    ///     Shift-drag was only ever documented on the collapsed ribbon, so pinning the dock open
    ///     lost the one place it was written down.
    /// </summary>
    [TestMethod]
    public void ShiftDragIsDocumentedInBothDockStates() {
        XDocument dock = XDocument.Load(Path.Combine(
            RepoRoot, "CosmereCore", "Languages", "English", "Keyed", "Dock.xml"
        ));

        string Value(string key) {
            return dock.Descendants(key).Single().Value;
        }

        Assert.Contains("Shift-drag", Value("CC_Dock_Rail_Tip"), "Collapsed already said so.");
        Assert.Contains("Shift-drag", Value("CC_Dock_Collapse"), "Expanded has to say so too.");
    }
}
