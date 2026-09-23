using global::System;
using global::System.Collections.Generic;

namespace Cosmere.Core.Settings.Model;

public sealed record ChoiceControl : SettingControl {
    public ChoiceControl(
        Func<string?> get,
        Action<string?> set,
        string? @default,
        Func<IReadOnlyList<Choice>> options,
        bool allowNone
    ) {
        Get = get;
        Set = set;
        Default = @default;
        Options = options;
        AllowNone = allowNone;
    }

    public Func<string?> Get { get; }

    public Action<string?> Set { get; }

    public string? Default { get; }

    public Func<IReadOnlyList<Choice>> Options { get; }

    public bool AllowNone { get; }

    public string? Value {
        get => Get();
        set => Set(value);
    }

    public override bool IsDefault => Value == Default;

    public override void Reset() {
        Value = Default;
    }
}
