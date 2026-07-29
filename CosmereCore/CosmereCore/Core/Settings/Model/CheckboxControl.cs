using global::System;

namespace Cosmere.Core.Settings.Model;

public sealed record CheckboxControl : SettingControl {
    public CheckboxControl(Func<bool> get, Action<bool> set, bool @default) {
        Get = get;
        Set = set;
        Default = @default;
    }

    public Func<bool> Get { get; }

    public Action<bool> Set { get; }

    public bool Default { get; }

    public bool Value {
        get => Get();
        set => Set(value);
    }

    public override bool IsDefault => Value == Default;

    public override void Reset() {
        Value = Default;
    }
}
