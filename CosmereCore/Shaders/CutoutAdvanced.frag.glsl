#version 450

// Palette swapping with wear, glow and special-zone masks, each greyscale in its red channel.
// Levels are bands of that channel, so 4 wear levels means 0-63, 64-127, 128-191, 192-255. A band
// at full brightness is permanent rather than level driven.

layout(location = 1) in vec2 vUv;

layout(location = 0) out vec4 outColor;

layout(set = 2, binding = 0) uniform MaterialUniforms
{
    vec4 _MainTex_ST;      // @default = vec4(0.0, 0.0, 1.0, 1.0)
    vec4 _GlowColor;       // @color @default = vec4(1.0, 1.0, 1.0, 1.0)
    vec4 _HighlightParams; // @default = vec4(3.0, 8.0, 0.2, 0.0)
    vec4 _Time;            // global, published by MapRenderFeature
    vec2 _RimLightCenter;  // @default = vec2(0.0, 1.0)
    float _BlendStrength;  // @default = 0.0
    float _MaterialIntensity; // @default = 1.0
    float _WearLevelCount;    // @default = 4.0
    float _GlowLevelCount;    // @default = 4.0
    float _CurrentWearLevel;  // @default = 0.0
    float _CurrentGlowLevel;  // @default = 0.0
    float _WearDarkness;      // @default = 0.4
    float _GlowIntensity;     // @default = 2.0
} u;

layout(set = 2, binding = 1) uniform sampler2D _MainTex;
layout(set = 2, binding = 2) uniform sampler2D _ColorMaskTex;   // @default = white
layout(set = 2, binding = 3) uniform sampler2D _WearMaskTex;    // @default = black
layout(set = 2, binding = 4) uniform sampler2D _GlowMaskTex;    // @default = black
layout(set = 2, binding = 5) uniform sampler2D _SpecialMaskTex; // @default = black
layout(std430, set = 2, binding = 6) readonly buffer _PaletteColors { vec4 paletteColors[]; };
layout(std430, set = 2, binding = 7) readonly buffer _MetallicValues { float metallicValues[]; };
layout(std430, set = 2, binding = 8) readonly buffer _SmoothnessValues { float smoothnessValues[]; };
// length() reports the allocated buffer, which the runtime may pad; the real count rides in
// its own array because a scalar in MaterialUniforms does not arrive intact.
layout(std430, set = 2, binding = 9) readonly buffer _PaletteCount { float paletteCount[]; };

float Luminance(vec3 c) { return dot(c, vec3(0.22, 0.707, 0.071)); }

void main()
{
    vec2 uv = vUv * u._MainTex_ST.zw + u._MainTex_ST.xy;
    vec4 baseColor = texture(_MainTex, uv);

    // A black pixel carries no palette index, so there is nothing to look up.
    if (all(lessThan(baseColor.rgb, vec3(0.001)))) {
        outColor = baseColor;

        return;
    }

    float colorIndex = texture(_ColorMaskTex, uv).r;
    float wearAmount = 0.0;
    float glowStrength = 0.0;
    float specialZone = 0.0;

#ifdef USE_WEAR_MASK
    wearAmount = texture(_WearMaskTex, uv).r;
#endif
#ifdef USE_GLOW_MASK
    glowStrength = texture(_GlowMaskTex, uv).r;
#endif
#ifdef USE_SPECIAL_MASK
    specialZone = texture(_SpecialMaskTex, uv).r;
#endif

    float colorCount = paletteCount[0];
    float safeIndex = min(colorIndex, 1.0 - (1.0 / colorCount + 1e-5));
    float lutIndex = safeIndex * colorCount;
    int idxA = clamp(int(floor(lutIndex)), 0, int(colorCount) - 1);

    vec4 lutColor;
    float metallic;
    float smoothness;

    vec4 colorA = paletteColors[idxA];

#if defined(BLEND_LINEAR) || defined(BLEND_SMOOTH) || defined(BLEND_SHARP) || defined(BLEND_STEP)
    int idxB = min(idxA + 1, int(colorCount) - 1);
    vec4 colorB = paletteColors[idxB];

    float blend = clamp(fract(lutIndex) * u._BlendStrength, 0.0, 1.0);
    float blendFinal;

    #ifdef BLEND_LINEAR
    blendFinal = blend;
    #elif defined(BLEND_SMOOTH)
    blendFinal = smoothstep(0.0, 1.0, blend);
    #elif defined(BLEND_SHARP)
    blendFinal = blend * blend;
    #else
    blendFinal = step(0.5, blend);
    #endif

    lutColor = mix(colorA, colorB, blendFinal);
    metallic = mix(metallicValues[idxA], metallicValues[idxB], blendFinal);
    smoothness = mix(smoothnessValues[idxA], smoothnessValues[idxB], blendFinal);
#else
    lutColor = colorA;
    metallic = metallicValues[idxA];
    smoothness = smoothnessValues[idxA];
#endif

    vec4 result = baseColor * lutColor;
    float wearIntensity = 0.0;

#ifdef USE_WEAR_MASK
    if (wearAmount > 0.01) {
        // A mask painted at full brightness is permanent wear rather than a level.
        wearIntensity = wearAmount >= 0.99 ? wearAmount : u._CurrentWearLevel * wearAmount;

        result.rgb = mix(result.rgb, result.rgb * u._WearDarkness, wearIntensity);

        // Worn metal stops being shiny.
        metallic *= 1.0 - wearIntensity * 0.7;
        smoothness *= 1.0 - wearIntensity * 0.5;
    }
#endif

    // Metal reads as desaturated and contrasty before any highlight lands on it.
    result.rgb = mix(result.rgb, vec3(Luminance(result.rgb)), metallic * 0.2 * u._MaterialIntensity);

    float contrast = mix(1.0, 1.3, metallic * u._MaterialIntensity);
    result.rgb = clamp((result.rgb - 0.5) * contrast + 0.5, 0.0, 1.0);

    float wearReduction = 1.0 - wearIntensity;
    vec2 shinePos = fract(uv * 3.0);
    float shine = pow(max(0.0, 1.0 - length(shinePos - 0.5) * 2.0), 4.0);
    result.rgb += shine * metallic * smoothness * 0.3 * u._MaterialIntensity * wearReduction;

    // Smoother surfaces get a tighter, brighter highlight.
    float highlightSize = mix(u._HighlightParams.x, u._HighlightParams.x * 2.67, smoothness);
    float highlight = pow(
        clamp(sin(uv.x * highlightSize) * sin(uv.y * highlightSize), 0.0, 1.0),
        mix(1.0, u._HighlightParams.y, smoothness)
    );
    result.rgb += highlight * smoothness * metallic * u._HighlightParams.z * u._MaterialIntensity * wearReduction;

    // Smooth non metal, plastic rather than steel, catches a rim light instead.
    float rimLight = pow(1.0 - clamp(dot(u._RimLightCenter, uv - 0.5), 0.0, 1.0), 2.0);
    result.rgb += rimLight * smoothness * (1.0 - metallic) * 0.1 * u._MaterialIntensity;

#ifdef USE_GLOW_MASK
    if (glowStrength > 0.01) {
        float glow = glowStrength >= 0.99 ? glowStrength : u._CurrentGlowLevel * glowStrength;
        result.rgb += u._GlowColor.rgb * glow * u._GlowIntensity;
    }
#endif

#ifdef USE_SPECIAL_MASK
    if (specialZone > 0.8) {
        // Gems sparkle.
        result.rgb += fract(sin(dot(uv * 100.0, vec2(12.9898, 78.233))) * 43758.5453) * 0.5;
    } else if (specialZone > 0.5) {
        // Runes pulse.
        result.rgb += u._GlowColor.rgb * (sin(u._Time.y * 3.0) * 0.5 + 0.5) * 0.3;
    } else if (specialZone > 0.3) {
        // Inlaid metal takes the highlight harder.
        result.rgb += shine * 0.5;
    }
#endif

    // Clipped last, so the masks above run on the pixels that survive rather than before them.
    if (result.a < 0.5) {
        discard;
    }

    outColor = result;
}
