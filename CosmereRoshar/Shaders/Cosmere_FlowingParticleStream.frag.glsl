#version 450

// Grasspren and windspren shell. Streams are polylines marched through a flow field and measured by
// point-to-segment distance; motes are single points marched along the same field. Both are traced
// from scratch for every pixel, so the loop bounds are compile time constants and the _FlowDensity /
// _ParticleCount uniforms only cut the count down, never up.

layout(location = 0) in vec4 vColor;
layout(location = 1) in vec2 vUv;

layout(location = 0) out vec4 outColor;

layout(set = 2, binding = 0) uniform MaterialUniforms
{
    vec4 _MainTex_ST;     // @default = vec4(0.0, 0.0, 1.0, 1.0)
    vec4 _Color;          // @color @default = vec4(0.3, 0.8, 0.4, 0.7)
    vec4 _StreamColor;    // @color @default = vec4(0.8, 1.0, 0.6, 1.0)
    vec4 _ParticleColor;  // @color @default = vec4(1.0, 1.0, 0.8, 0.9)
    vec4 _Time;           // global, published by MapRenderFeature
    int _FlowPattern;      // @default = 0 (0 = grass, 1 = wind, 2 = spiral)
    float _FlowSpeed;      // @default = 1.5 (set by the spren controllers, unused by the shipped effect)
    float _FlowDensity;    // @default = 8.0
    float _FlowWidth;      // @default = 0.05
    float _ParticleCount;  // @default = 6.0
    float _ParticleSize;   // @default = 0.02
    float _ParticleSpeed;  // @default = 1.2
    float _AnimationSpeed; // @default = 1.0
    float _Intensity;      // @default = 1.0
    float _FadeEdges;      // @default = 0.8
    float _Alpha;          // @default = 0.8
} u;

layout(set = 2, binding = 1) uniform sampler2D _MainTex;

const int MaxStreams = 8;
const int StreamSegments = 10;
const int MaxParticles = 15;
const int ParticleSteps = 20;

float hash21(vec2 p)
{
    p = fract(p * vec2(233.34, 851.73));
    p += dot(p, p + 23.45);

    return fract(p.x * p.y);
}

vec2 hash22(vec2 p)
{
    p = fract(p * vec2(127.1, 311.7));

    return -1.0 + 2.0 * fract(p * 43758.5453123);
}

float valueNoise(vec2 p)
{
    vec2 i = floor(p);
    vec2 f = fract(p);
    f = f * f * (3.0 - 2.0 * f);

    return mix(
        mix(hash21(i + vec2(0.0, 0.0)), hash21(i + vec2(1.0, 0.0)), f.x),
        mix(hash21(i + vec2(0.0, 1.0)), hash21(i + vec2(1.0, 1.0)), f.x),
        f.y
    );
}

vec2 getFlowDirection(vec2 uv, float time)
{
    vec2 direction = vec2(0.0, 1.0);

    if (u._FlowPattern == 0)
    {
        // Grass: upright, swaying side to side.
        float swayAmount = sin(time * u._AnimationSpeed + uv.x * 3.14159) * 0.3;
        direction = normalize(vec2(swayAmount, 1.0 - abs(swayAmount) * 0.5));
    }
    else if (u._FlowPattern == 1)
    {
        // Wind: heading comes straight off a scrolling noise field, so streams curve.
        float n = valueNoise((uv + vec2(time * u._AnimationSpeed * 0.3, 0.0)) * 3.0);
        float angle = n * 3.14159 * 2.0 + time * u._AnimationSpeed;
        direction = vec2(cos(angle), sin(angle));
    }
    else if (u._FlowPattern == 2)
    {
        // Spiral: tangent to the centre, weakening outward, with a nudge that keeps it from stalling.
        vec2 center = uv - 0.5;
        float angle = atan(center.y, center.x) + time * u._AnimationSpeed;
        float radius = length(center);
        direction = vec2(-sin(angle), cos(angle)) * (1.0 - radius);
        direction = normalize(direction + vec2(radius * 0.3, 0.0));
    }

    return direction;
}

float getStreamIntensity(vec2 uv, float time)
{
    float streamIntensity = 0.0;

    for (int i = 0; i < MaxStreams; i++)
    {
        if (float(i) >= u._FlowDensity) { break; }

        float streamOffset = float(i) / u._FlowDensity;
        vec2 currentPos = vec2(streamOffset, 0.0) + hash22(vec2(float(i), 0.0)) * 0.1;

        const float segmentLength = 0.1;

        for (int j = 0; j < StreamSegments; j++)
        {
            vec2 nextPos = currentPos + getFlowDirection(currentPos, time) * segmentLength;

            vec2 lineDir = nextPos - currentPos;
            float proj = clamp(dot(uv - currentPos, lineDir) / dot(lineDir, lineDir), 0.0, 1.0);
            float dist = length(uv - (currentPos + lineDir * proj));

            if (dist < u._FlowWidth)
            {
                streamIntensity = max(streamIntensity, 1.0 - dist / u._FlowWidth);
            }

            currentPos = nextPos;

            if (currentPos.y > 1.0) { break; }
        }
    }

    return streamIntensity;
}

float getParticleIntensity(vec2 uv, float time)
{
    float particleIntensity = 0.0;

    for (int i = 0; i < MaxParticles; i++)
    {
        if (float(i) >= u._ParticleCount) { break; }

        vec2 randomSeed = vec2(float(i) * 17.3, float(i) * 31.7);
        vec2 currentPos = vec2(hash21(randomSeed), hash21(randomSeed + 1.0));

        // Each mote runs its own looping clock, so they do not all restart together.
        float travel = fract(time * u._ParticleSpeed * u._AnimationSpeed + hash21(randomSeed + 2.0));

        for (int j = 0; j < ParticleSteps; j++)
        {
            if (float(j) * 0.05 > travel) { break; }

            currentPos += getFlowDirection(currentPos, time + float(j) * 0.05) * 0.05;

            if (currentPos.x < 0.0 || currentPos.x > 1.0 || currentPos.y < 0.0 || currentPos.y > 1.0)
            {
                break;
            }
        }

        float dist = length(uv - currentPos);

        if (dist < u._ParticleSize)
        {
            float a = 1.0 - dist / u._ParticleSize;
            particleIntensity = max(particleIntensity, a * a);
        }
    }

    return particleIntensity;
}

void main()
{
    vec4 texColor = texture(_MainTex, vUv * u._MainTex_ST.zw + u._MainTex_ST.xy);
    float time = u._Time.y * u._AnimationSpeed;

    float distFromCenter = length(vUv - 0.5);

    float streamIntensity = getStreamIntensity(vUv, time);
    float particleIntensity = getParticleIntensity(vUv, time);
    float totalIntensity = max(streamIntensity, particleIntensity);

    vec3 baseColor = texColor.rgb * u._Color.rgb * vColor.rgb;
    float baseAlpha = texColor.a * u._Color.a * vColor.a * u._Alpha;

    vec3 streamContrib = u._StreamColor.rgb * streamIntensity * u._Intensity;
    vec3 particleContrib = u._ParticleColor.rgb * particleIntensity * u._Intensity;

    vec3 finalColor = baseColor + (streamContrib + particleContrib) * vColor.rgb * totalIntensity;

    // max() keeps pow off a zero base, which is undefined rather than zero in GLSL.
    float edgeFade = clamp(1.0 - pow(max(distFromCenter * 2.0, 1e-5), u._FadeEdges), 0.0, 1.0);

    outColor = vec4(finalColor, baseAlpha * edgeFade);
}
