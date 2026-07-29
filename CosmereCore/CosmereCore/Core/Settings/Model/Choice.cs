namespace Cosmere.Core.Settings.Model;

public sealed class Choice {
    public Choice(string value, string labelKey) {
        Value = value;
        LabelKey = labelKey;
    }

    public string Value { get; }

    public string LabelKey { get; }
}
