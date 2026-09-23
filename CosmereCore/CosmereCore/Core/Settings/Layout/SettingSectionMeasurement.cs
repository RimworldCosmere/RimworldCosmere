using global::System;
using global::System.Collections.Generic;

namespace Cosmere.Core.Settings.Layout;

public sealed record SettingSectionMeasurement {
    public SettingSectionMeasurement(string key, float height) {
        Key = key;
        Height = height;
    }

    public string Key { get; }

    public float Height { get; }
}
