#nullable disable
using System;

namespace Cosmere.Foundation.Attribute;

public enum ProfileMode {
    Instrumentation = 0, // Full enter/exit timing
    Sampling = 1, // Bernoulli sampling
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class Profile(
    string label = null,
    ProfileMode mode = ProfileMode.Instrumentation,
    float sampleProbability = 0.01f,
    string category = null,
    bool aggregate = true
)
    : System.Attribute {
    public string label { get; } = label;
    public ProfileMode mode { get; } = mode;
    public float sampleProbability { get; } = sampleProbability;
    public string category { get; } = category;
    public bool aggregate { get; } = aggregate;
}