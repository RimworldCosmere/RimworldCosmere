using Cosmere.Core.Settings.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmere.Tests;

[TestClass]
public class RosharNahelIntervalTests {
    private const float MinimumTicks = 2500f;
    private const float MaximumTicks = 36000000f;
    private const float DefaultAverageTicks = 1728000f;
    private const float DefaultMinimumTicks = 864000f;
    private const float DefaultMaximumTicks = 13824000f;

    [TestMethod]
    public void NahelIntervalControlsRejectZeroNegativeAndNonfiniteInput() {
        float[] invalidValues = [
            0f,
            -1f,
            float.NaN,
            float.PositiveInfinity,
            float.NegativeInfinity,
        ];

        foreach (float invalidValue in invalidValues) {
            NahelIntervalControls intervals = new();
            intervals.Minimum.Value = invalidValue;
            Assert.IsGreaterThanOrEqualTo(MinimumTicks, intervals.Minimum.Value);

            intervals = new NahelIntervalControls();
            intervals.Average.Value = invalidValue;
            Assert.IsGreaterThanOrEqualTo(MinimumTicks, intervals.Average.Value);

            intervals = new NahelIntervalControls();
            intervals.Maximum.Value = invalidValue;
            Assert.IsGreaterThanOrEqualTo(MinimumTicks, intervals.Maximum.Value);
        }
    }

    [TestMethod]
    public void NahelIntervalControlsPreserveOrderedTripleAfterEditsInBothDirections() {
        NahelIntervalControls intervals = new();
        intervals.Minimum.Value = 2000000f;
        AssertOrdered(intervals);

        intervals = new NahelIntervalControls();
        intervals.Minimum.Value = 500000f;
        AssertOrdered(intervals);

        intervals = new NahelIntervalControls();
        intervals.Average.Value = 20000000f;
        AssertOrdered(intervals);

        intervals = new NahelIntervalControls();
        intervals.Average.Value = 500000f;
        AssertOrdered(intervals);

        intervals = new NahelIntervalControls();
        intervals.Maximum.Value = 1500000f;
        AssertOrdered(intervals);

        intervals = new NahelIntervalControls();
        intervals.Maximum.Value = 20000000f;
        AssertOrdered(intervals);
    }

    [TestMethod]
    public void NahelIntervalControlsKeepRosharDefaults() {
        NahelIntervalControls intervals = new();

        Assert.AreEqual(DefaultMinimumTicks, intervals.Minimum.Default);
        Assert.AreEqual(DefaultMinimumTicks, intervals.Minimum.Value);
        Assert.AreEqual(DefaultAverageTicks, intervals.Average.Default);
        Assert.AreEqual(DefaultAverageTicks, intervals.Average.Value);
        Assert.AreEqual(DefaultMaximumTicks, intervals.Maximum.Default);
        Assert.AreEqual(DefaultMaximumTicks, intervals.Maximum.Value);
    }

    private static void AssertOrdered(NahelIntervalControls intervals) {
        Assert.IsLessThanOrEqualTo(intervals.Minimum.Value, intervals.Average.Value);
        Assert.IsLessThanOrEqualTo(intervals.Average.Value, intervals.Maximum.Value);
    }

    private sealed class NahelIntervalControls {
        private float average = DefaultAverageTicks;
        private float maximum = DefaultMaximumTicks;
        private float minimum = DefaultMinimumTicks;

        public NahelIntervalControls() {
            Average = new TicksControl(
                () => average,
                updated => SettingValueMath.SetOrderedAverage(
                    updated,
                    ref minimum,
                    ref average,
                    ref maximum,
                    MinimumTicks,
                    MaximumTicks
                ),
                DefaultAverageTicks,
                MinimumTicks,
                MaximumTicks,
                TickUnit.Days
            );
            Minimum = new TicksControl(
                () => minimum,
                updated => SettingValueMath.SetOrderedMinimum(
                    updated,
                    ref minimum,
                    ref average,
                    ref maximum,
                    MinimumTicks,
                    MaximumTicks
                ),
                DefaultMinimumTicks,
                MinimumTicks,
                MaximumTicks,
                TickUnit.Days
            );
            Maximum = new TicksControl(
                () => maximum,
                updated => SettingValueMath.SetOrderedMaximum(
                    updated,
                    ref minimum,
                    ref average,
                    ref maximum,
                    MinimumTicks,
                    MaximumTicks
                ),
                DefaultMaximumTicks,
                MinimumTicks,
                MaximumTicks,
                TickUnit.Days
            );
        }

        public TicksControl Average { get; }

        public TicksControl Maximum { get; }

        public TicksControl Minimum { get; }
    }
}
