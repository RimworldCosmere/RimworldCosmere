#version 450

// Ash lying on the ground, drawn by a section layer at the same queue as vanilla snow.
// Vertex colour carries the data: alpha is ash depth, red is a per-cell jitter. Everything
// else is value noise over world XZ, so there are no textures and no texture coords to flip.

layout(location = 0) in vec4 vColor; // r = per-cell jitter, a = ash depth
layout(location = 1) in vec2 vUv;    // object-space XZ

layout(location = 0) out vec4 outColor;

layout(set = 2, binding = 0) uniform MaterialUniforms
{
    vec4 _AshColor;         // @color @default = vec4(0.42, 0.36, 0.31, 1.0)
    vec4 _AshColorDeep;     // @color @default = vec4(0.13, 0.12, 0.12, 1.0)
    float _AshSeverity;     // @default = 1.0
    float _MaxGroundOpacity;// @default = 0.8
    float _DriftScale;      // @default = 0.09
    float _GrainScale;      // @default = 2.5
    float _CreepSharpness;  // @default = 6.0
    float _CoverageGain;    // @default = 1.35
    float _NoiseInfluence;  // @default = 0.75
} u;

float hash21(vec2 p)
{
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);

    return fract(p.x * p.y);
}

float valueNoise(vec2 p)
{
    vec2 i = floor(p);
    vec2 f = fract(p);
    f = f * f * (3.0 - 2.0 * f);

    float a = hash21(i);
    float b = hash21(i + vec2(1.0, 0.0));
    float c = hash21(i + vec2(0.0, 1.0));
    float d = hash21(i + vec2(1.0, 1.0));

    return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
}

void main()
{
    float depth = vColor.a;
    float jitter = vColor.r;

    float drift = valueNoise(vUv * u._DriftScale);
    float grain = valueNoise(vUv * u._GrainScale);
    float n = mix(drift, grain, 0.25);

    // Ash creeps out of the noise valleys as depth rises, instead of the whole map brightening
    // together. Coverage saturates before depth does, so the deep end reads as solid ground.
    float coverage = clamp(depth * u._CoverageGain, 0.0, 1.0);
    float edge = clamp((coverage - n * u._NoiseInfluence + jitter * 0.06) * u._CreepSharpness, 0.0, 1.0);

    vec3 col = mix(u._AshColor.rgb, u._AshColorDeep.rgb, clamp(depth * 1.2, 0.0, 1.0));
    col *= 0.92 + grain * 0.16;

    outColor = vec4(col, edge * u._AshSeverity * u._MaxGroundOpacity);
}
