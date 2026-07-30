using Cosmere.Core.BetaHub;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

/// <summary>
///     Covers the arithmetic in <see cref="FeedbackButtonLayout" />, in particular the shift
///     that keeps the plates clear of the learning helper readout.
/// </summary>
[TestClass]
public class FeedbackButtonLayoutTests {
    private const float LearningReadoutLeftEdge = 208f;

    [TestMethod]
    public void WithoutTheLearningHelperTheButtonsTakeTheCorner() {
        Assert.AreEqual(2560f - 208f, FeedbackButtonLayout.ButtonX(2560f, false), 0.001f);
    }

    [TestMethod]
    public void WithTheLearningHelperTheButtonsShiftAFullColumnLeft() {
        Assert.AreEqual(2560f - 416f, FeedbackButtonLayout.ButtonX(2560f, true), 0.001f);
    }

    /// <summary>
    ///     The learning helper readout's own left edge, mirrored from LearningReadout's layout.
    /// </summary>
    [TestMethod]
    public void TheShiftedButtonsNeverOverlapTheLearningReadout() {
        float screenWidth = 1920f;
        float right = FeedbackButtonLayout.ButtonX(screenWidth, true) + FeedbackButtonLayout.ButtonWidth;
        Assert.IsTrue(right <= screenWidth - LearningReadoutLeftEdge, $"right edge {right} ran into the readout");
    }

    [TestMethod]
    public void TheShiftIsExactlyOneButtonPlusOneMargin() {
        float shifted = FeedbackButtonLayout.ButtonX(2560f, true);
        float plain = FeedbackButtonLayout.ButtonX(2560f, false);
        Assert.AreEqual(FeedbackButtonLayout.ButtonWidth + FeedbackButtonLayout.EdgeMargin, plain - shifted, 0.001f);
    }

    [TestMethod]
    public void TheSecondButtonSitsBelowTheFirstWithAGap() {
        Assert.AreEqual(FeedbackButtonLayout.TopMargin, FeedbackButtonLayout.ButtonY(0), 0.001f);
        Assert.AreEqual(
            FeedbackButtonLayout.TopMargin + FeedbackButtonLayout.ButtonHeight + FeedbackButtonLayout.ButtonGap,
            FeedbackButtonLayout.ButtonY(1),
            0.001f
        );
    }

    /// <summary>
    ///     Even at a narrow resolution with the learning helper visible, the plates must stay
    ///     on screen rather than going negative.
    /// </summary>
    [TestMethod]
    public void ANarrowScreenStillPlacesTheButtonsOnScreen() {
        Assert.IsTrue(FeedbackButtonLayout.ButtonX(1280f, true) >= 0f);
    }
}
