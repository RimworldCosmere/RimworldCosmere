namespace Cosmere.Core.Settings.Layout;

public sealed record SettingSectionPlacement {
    public SettingSectionPlacement(string key, int column, float x, float y, float width, float height) {
        Key = key;
        Column = column;
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public string Key { get; }

    public int Column { get; }

    public float X { get; }

    public float Y { get; }

    public float Width { get; }

    public float Height { get; }
}
