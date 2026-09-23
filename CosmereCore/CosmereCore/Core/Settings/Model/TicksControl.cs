using global::System;
using RimWorld;

namespace Cosmere.Core.Settings.Model;

public sealed record TicksControl : SettingControl {
    public TicksControl(
        Func<float> get,
        Action<float> set,
        float @default,
        float minTicks,
        float maxTicks,
        TickUnit editUnit
    ) {
        Get = get;
        Set = set;
        Default = @default;
        MinTicks = minTicks;
        MaxTicks = maxTicks;
        EditUnit = editUnit;
    }

    public Func<float> Get { get; }

    public Action<float> Set { get; }

    public float Default { get; }

    public float MinTicks { get; }

    public float MaxTicks { get; }

    public TickUnit EditUnit { get; }

    public float Value {
        get => Get();
        set => Set(SettingValueMath.ClampFinite(value, Default, MinTicks, MaxTicks));
    }

    public string DisplayText => ((int)Value).ToStringTicksToPeriod();

    public override bool IsDefault => Value == Default;

    public override void Reset() {
        Value = Default;
    }
}
