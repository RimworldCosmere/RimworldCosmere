using global::System;

namespace Cosmere.Core.Settings.Model;

public sealed record ButtonControl : SettingControl {
    public ButtonControl(string labelKey, Action onClick, Func<string?> statusKey) {
        LabelKey = labelKey;
        OnClick = onClick;
        StatusKey = statusKey;
    }

    public string LabelKey { get; }

    public Action OnClick { get; }

    public Func<string?> StatusKey { get; }

    public override bool IsDefault => true;

    public override void Reset() {
    }
}
