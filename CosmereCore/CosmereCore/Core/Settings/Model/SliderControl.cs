using global::System;

namespace Cosmere.Core.Settings.Model;

public sealed record SliderControl : SettingControl {
    public SliderControl(
        Func<float> get,
        Action<float> set,
        float @default,
        float min,
        float max,
        float? step,
        Func<float, string> format
    ) {
        Get = get;
        Set = set;
        Default = @default;
        Min = min;
        Max = max;
        Step = step;
        Format = format;
    }

    public Func<float> Get { get; }

    public Action<float> Set { get; }

    public float Default { get; }

    public float Min { get; }

    public float Max { get; }

    public float? Step { get; }

    public Func<float, string> Format { get; }

    public float Value {
        get => Get();
        set => Set(SettingValueMath.ClampFinite(value, Default, Min, Max));
    }

    public override bool IsDefault => Value == Default;

    public override void Reset() {
        Value = Default;
    }
}
