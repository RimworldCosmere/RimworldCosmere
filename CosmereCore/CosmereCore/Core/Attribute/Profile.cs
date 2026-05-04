using System;

namespace Cosmere.Core.Attribute;

public enum ProfileMode {
    Instrumentation = 0, // Full enter/exit timing
    Sampling = 1, // Bernoulli sampling
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class Profile(
    string? label = null,
    ProfileMode mode = ProfileMode.Instrumentation,
    float sampleProbability = 0.01f,
    string? category = null,
    bool aggregate = true
)
    : global::System.Attribute {
    public string? Label { get; } = label;
    public ProfileMode Mode { get; } = mode;
    public float SampleProbability { get; } = sampleProbability;
    public string? Category { get; } = category;
    public bool Aggregate { get; } = aggregate;
}