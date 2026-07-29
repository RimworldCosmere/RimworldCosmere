using global::System;

namespace Cosmere.Core.Settings.Model;

public sealed record IntRangeControl : SettingControl {
    public IntRangeControl(
        Func<int> getMin,
        Action<int> setMin,
        Func<int> getMax,
        Action<int> setMax,
        int defaultMin,
        int defaultMax,
        int hardMin,
        int hardMax
    ) {
        GetMin = getMin;
        SetMin = setMin;
        GetMax = getMax;
        SetMax = setMax;
        DefaultMin = defaultMin;
        DefaultMax = defaultMax;
        HardMin = hardMin;
        HardMax = hardMax;
    }

    public Func<int> GetMin { get; }

    public Action<int> SetMin { get; }

    public Func<int> GetMax { get; }

    public Action<int> SetMax { get; }

    public int DefaultMin { get; }

    public int DefaultMax { get; }

    public int HardMin { get; }

    public int HardMax { get; }

    public int Minimum {
        get => GetMin();
        set {
            int minimum = Minimum;
            int maximum = Maximum;
            SettingValueMath.SetOrderedMinimum(value, ref minimum, ref maximum, HardMin, HardMax);
            SetMin(minimum);
            SetMax(maximum);
        }
    }

    public int Maximum {
        get => GetMax();
        set {
            int minimum = Minimum;
            int maximum = Maximum;
            SettingValueMath.SetOrderedMaximum(value, ref minimum, ref maximum, HardMin, HardMax);
            SetMin(minimum);
            SetMax(maximum);
        }
    }

    public override bool IsDefault => Minimum == DefaultMin && Maximum == DefaultMax;

    public override void Reset() {
        Minimum = DefaultMin;
        Maximum = DefaultMax;
    }
}
