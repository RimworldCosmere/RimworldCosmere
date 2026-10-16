#version 450

// A ring on a mote quad, plus a wave that sweeps in or out behind it leaving a fading trail.
// Everything is procedural off the quad uv, so there is no _MainTex and no _MainTex_ST.
// _Speed is only ever read for its sign: zero freezes the ring, negative sweeps outward.

layout(location = 1) in vec2 vUv;

layout(location = 0) out vec4 outColor;

layout(set = 2, binding = 0) uniform MaterialUniforms
{
    vec4 _Color;         // @color @default = vec4(1.0, 1.0, 1.0, 1.0)
    vec4 _Color2;        // @color @default = vec4(1.0, 1.0, 1.0, 0.2)
    float _Radius;       // @default = 1.0
    float _BorderWidth;  // @default = 0.002
    float _TrailLength;  // @default = 0.05
    float _PulseTime;    // @default = 5.0
    float _Speed;        // @default = 1.0
    float _GameSeconds;  // global, set by TickManager
} u;

void main()
{
    // The quad is symmetric about its centre, so flipV would not change any distance here.
    vec2 centered = vUv - 0.5;
    float dist = length(centered);

    float radius = u._Radius * 0.5;
    float inner = radius - u._BorderWidth;
    float outer = radius + u._BorderWidth;

    bool inStaticRing = dist >= inner && dist <= outer;

    // Frozen: the ring is all there is.
    if (u._Speed == 0.0) {
        outColor = inStaticRing ? u._Color2 : vec4(0.0);

        return;
    }

    float time = mod(u._GameSeconds / u._PulseTime, 1.0);
    float waveProgress = u._Speed > 0.0 ? time : 1.0 - time;

    float waveRadius = radius * waveProgress;

    // The trail shortens as the wave reaches the rim, so it tucks in rather than clipping.
    float trailLength = mix(u._TrailLength, u._TrailLength * (1.0 - waveProgress), waveProgress);

    float behindWave = u._Speed > 0.0 ? waveRadius - dist : dist - waveRadius;
    bool inTrail = behindWave > 0.0 && behindWave < trailLength && dist < radius;

    if (inStaticRing) {
        outColor = u._Color2;
    } else if (inTrail) {
        // trailLength is non zero here, since inTrail required behindWave below it.
        float trailAlpha = (1.0 - behindWave / trailLength) * (1.0 - time);

        outColor = vec4(u._Color.rgb, trailAlpha * u._Color.a);
    } else {
        outColor = vec4(0.0);
    }
}
