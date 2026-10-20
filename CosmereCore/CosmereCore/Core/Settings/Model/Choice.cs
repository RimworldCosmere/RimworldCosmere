namespace Cosmere.Core.Settings.Model;

public sealed class Choice {
    public Choice(string value, string labelKey) : this(value, labelKey, null, false) { }

    public static Choice Keyed(string value, string labelKey) {
        return new Choice(value, labelKey);
    }

    public static Choice Literal(string value, string displayText) {
        return new Choice(value, null, displayText, true);
    }

    private Choice(string value, string? labelKey, string? literalDisplayText, bool isLiteral) {
        Value = value;
        LabelKey = labelKey;
        LiteralDisplayText = literalDisplayText;
        IsLiteral = isLiteral;
    }

    public string Value { get; }

    public string? LabelKey { get; }

    public string? LiteralDisplayText { get; }

    public bool IsLiteral { get; }
}
