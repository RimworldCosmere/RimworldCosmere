using Cosmere.Core.Settings.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

[TestClass]
public class SettingValueMathTests {
    [TestMethod]
    public void ClampFiniteRejectsZeroNegativeAndNonFiniteValues() {
        Assert.AreEqual(2500f, SettingValueMath.ClampFinite(0f, 5000f, 2500f, 60000f));
        Assert.AreEqual(2500f, SettingValueMath.ClampFinite(-1f, 5000f, 2500f, 60000f));
        Assert.AreEqual(5000f, SettingValueMath.ClampFinite(float.NaN, 5000f, 2500f, 60000f));
        Assert.AreEqual(5000f, SettingValueMath.ClampFinite(float.PositiveInfinity, 5000f, 2500f, 60000f));
        Assert.AreEqual(5000f, SettingValueMath.ClampFinite(float.NegativeInfinity, 5000f, 2500f, 60000f));
        Assert.AreEqual(60000f, SettingValueMath.ClampFinite(90000f, 5000f, 2500f, 60000f));
    }

    [TestMethod]
    public void RaisingRangeMinimumAlsoRaisesMaximum() {
        int minimum = 5;
        int maximum = 7;

        SettingValueMath.SetOrderedMinimum(20, ref minimum, ref maximum, 1, 30);

        Assert.AreEqual(20, minimum);
        Assert.AreEqual(20, maximum);
    }

    [TestMethod]
    public void LoweringRangeMaximumAlsoLowersMinimum() {
        int minimum = 5;
        int maximum = 7;

        SettingValueMath.SetOrderedMaximum(3, ref minimum, ref maximum, 1, 30);

        Assert.AreEqual(3, minimum);
        Assert.AreEqual(3, maximum);
    }

    [TestMethod]
    public void NahelEditsPreserveMinimumAverageMaximumOrder() {
        float minimum = 864000f;
        float average = 1728000f;
        float maximum = 13824000f;

        SettingValueMath.SetOrderedMinimum(2000000f, ref minimum, ref average, ref maximum, 2500f, 36000000f);
        Assert.AreEqual(2000000f, minimum);
        Assert.AreEqual(2000000f, average);
        Assert.AreEqual(13824000f, maximum);

        SettingValueMath.SetOrderedMaximum(1500000f, ref minimum, ref average, ref maximum, 2500f, 36000000f);
        Assert.AreEqual(1500000f, minimum);
        Assert.AreEqual(1500000f, average);
        Assert.AreEqual(1500000f, maximum);

        SettingValueMath.SetOrderedAverage(1000000f, ref minimum, ref average, ref maximum, 2500f, 36000000f);
        Assert.AreEqual(1000000f, minimum);
        Assert.AreEqual(1000000f, average);
        Assert.AreEqual(1500000f, maximum);
    }
}
