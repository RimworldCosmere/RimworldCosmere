using Cosmere.System.Roshar.Dialog;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Guards the sphere filter geometry: a scroll view that once ran past the window, a view rect
///     that ignored how many spheres there were, and a close button pinned to the wrong origin.
/// </summary>
/// <remarks>
///     The math lives on SphereFilterLayout rather than on the window so it can be checked without
///     UnityEngine.Rect - the test host has no Unity assemblies.
/// </remarks>
[TestClass]
public class SphereFilterTests {
    private const float WindowX = 18f;
    private const float WindowY = 18f;
    private const float WindowWidth = 400f;
    private const float WindowHeight = 500f;

    [TestMethod]
    public void ScrollViewStaysInsideTheWindow() {
        float bottom = SphereFilterLayout.ScrollY(WindowY) + SphereFilterLayout.ScrollHeight(WindowHeight);

        Assert.IsTrue(bottom <= WindowY + WindowHeight);
    }

    [TestMethod]
    public void ScrollViewStartsBelowTheHeader() {
        Assert.AreEqual(WindowY + SphereFilterLayout.HeaderHeight, SphereFilterLayout.ScrollY(WindowY));
    }

    [TestMethod]
    public void ScrollViewClearsTheCloseButton() {
        float bottom = SphereFilterLayout.ScrollY(WindowY) + SphereFilterLayout.ScrollHeight(WindowHeight);

        Assert.IsTrue(bottom <= SphereFilterLayout.CloseY(WindowY, WindowHeight));
    }

    [TestMethod]
    public void ScrollHeightNeverGoesNegativeInATinyWindow() {
        Assert.AreEqual(0f, SphereFilterLayout.ScrollHeight(10f));
    }

    [TestMethod]
    public void ViewHeightScalesWithSphereCount() {
        Assert.AreEqual(0f, SphereFilterLayout.ViewHeight(0));
        Assert.AreEqual(SphereFilterLayout.RowHeight * 3f, SphereFilterLayout.ViewHeight(3));
        Assert.AreEqual(SphereFilterLayout.RowHeight * 40f, SphereFilterLayout.ViewHeight(40));
    }

    [TestMethod]
    public void EveryRowFitsInsideTheViewRect() {
        const int count = 40;
        float lastRowBottom = ((count - 1) * SphereFilterLayout.RowHeight) + SphereFilterLayout.RowHeight;

        Assert.IsTrue(lastRowBottom <= SphereFilterLayout.ViewHeight(count));
    }

    [TestMethod]
    public void ViewWidthReservesTheScrollbar() {
        Assert.AreEqual(
            WindowWidth - SphereFilterLayout.ScrollbarWidth,
            SphereFilterLayout.ViewWidth(WindowWidth)
        );
    }

    [TestMethod]
    public void CloseButtonStaysInsideAnOffsetWindow() {
        float x = SphereFilterLayout.CloseX(WindowX, WindowWidth);
        float y = SphereFilterLayout.CloseY(WindowY, WindowHeight);

        Assert.IsTrue(x >= WindowX);
        Assert.IsTrue(x + SphereFilterLayout.CloseWidth <= WindowX + WindowWidth);
        Assert.IsTrue(y >= WindowY);
        Assert.IsTrue(y + SphereFilterLayout.CloseHeight <= WindowY + WindowHeight);
    }

    [TestMethod]
    public void CloseButtonTracksTheWindowOrigin() {
        Assert.AreEqual(
            SphereFilterLayout.CloseX(0f, WindowWidth) + WindowX,
            SphereFilterLayout.CloseX(WindowX, WindowWidth)
        );
        Assert.AreEqual(
            SphereFilterLayout.CloseY(0f, WindowHeight) + WindowY,
            SphereFilterLayout.CloseY(WindowY, WindowHeight)
        );
    }
}
