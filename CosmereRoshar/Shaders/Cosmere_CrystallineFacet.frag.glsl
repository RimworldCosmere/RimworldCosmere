#version 450

// Bismuth-style stepped crystal for rockspren. Facets are a 2D Worley cell distance over the quad's
// own uv, so the pattern rides the sprite rather than the world. Everything is procedural; _MainTex
// only tints and supplies the refraction sample.

layout(location = 0) in vec4 vColor;
layout(location = 1) in vec2 vUv;

layout(location = 0) out vec4 outColor;

layout(set = 2, binding = 0) uniform MaterialUniforms
{
    vec4 _MainTex_ST;     // @default = vec4(0.0, 0.0, 1.0, 1.0)
    vec4 _Color;          // @color @default = vec4(0.7, 0.6, 0.5, 0.8)
    vec4 _EdgeColor;      // @color @default = vec4(0.9, 0.8, 0.6, 1.0)
    vec4 _EmissionColor;  // @color @default = vec4(0.8, 0.7, 0.5, 0.9)
    vec4 _Time;           // global, published by MapRenderFeature
    float _FacetSize;      // @default = 8.0
    float _FacetSharpness; // @default = 0.8
    float _FacetOffset;    // @default = 0.5 (unused by the shipped effect; kept so C# can still set it)
    float _RefractStrength; // @default = 0.03
    float _FresnelPower;    // @default = 2.0
    float _PulseSpeed;      // @default = 0.5 (set by RocksprenController, unused by the shipped effect)
    float _PulseIntensity;  // @default = 0.3 (set by RocksprenController, unused by the shipped effect)
    float _NoiseScale;      // @default = 1.0
    float _NoiseSpeed;      // @default = 0.2
    float _Alpha;           // @default = 0.85
} u;

layout(set = 2, binding = 1) uniform sampler2D _MainTex;

float hash3(vec3 p)
{
    p = fract(p * 0.1031);
    p += dot(p, p.yzx + 33.33);

    return fract((p.x + p.y) * p.z);
}

float noise3d(vec3 p)
{
    vec3 i = floor(p);
    vec3 f = fract(p);
    f = f * f * (3.0 - 2.0 * f);

    return mix(
        mix(
            mix(hash3(i + vec3(0.0, 0.0, 0.0)), hash3(i + vec3(1.0, 0.0, 0.0)), f.x),
            mix(hash3(i + vec3(0.0, 1.0, 0.0)), hash3(i + vec3(1.0, 1.0, 0.0)), f.x),
            f.y
        ),
        mix(
            mix(hash3(i + vec3(0.0, 0.0, 1.0)), hash3(i + vec3(1.0, 0.0, 1.0)), f.x),
            mix(hash3(i + vec3(0.0, 1.0, 1.0)), hash3(i + vec3(1.0, 1.0, 1.0)), f.x),
            f.y
        ),
        f.z
    );
}

void main()
{
    vec2 texUv = vUv * u._MainTex_ST.zw + u._MainTex_ST.xy;
    vec4 texColor = texture(_MainTex, texUv);

    vec2 centeredUV = vUv - 0.5;
    float distFromCenter = length(centeredUV);

    vec2 facetCoord = vUv * u._FacetSize;
    vec2 facetCell = floor(facetCoord);
    vec2 facetUV = fract(facetCoord);

    float minDist = 1.0;

    for (int x = -1; x <= 1; x++)
    {
        for (int y = -1; y <= 1; y++)
        {
            vec2 neighbor = vec2(float(x), float(y));
            vec2 randomOffset = vec2(
                hash3(vec3(facetCell + neighbor, 0.0)),
                hash3(vec3(facetCell + neighbor, 17.0))
            ) * 0.5 + 0.5;

            minDist = min(minDist, length(neighbor + randomOffset - facetUV));
        }
    }

    // Tight falloff off the cell boundary is what reads as a cleaved face rather than a blob.
    float facetEdge = 1.0 - smoothstep(0.0, u._FacetSharpness * 0.3, minDist);
    float cellHash = hash3(vec3(facetCell, 0.0));

    float fresnel = pow(1.0 - distFromCenter, u._FresnelPower);

    vec2 refractOffset = centeredUV * u._RefractStrength * facetEdge;
    vec4 refractedTex = texture(_MainTex, texUv + refractOffset * u._MainTex_ST.zw);

    float animNoise = noise3d(vec3(vUv * u._NoiseScale, u._Time.y * u._NoiseSpeed));

    vec3 baseColor = mix(
        vColor.rgb * u._Color.rgb * texColor.rgb,
        vColor.rgb * u._EdgeColor.rgb,
        facetEdge
    );

    // Per cell purple-to-gold shift, the iridescence bismuth is known for.
    vec3 bismuthColors = mix(vec3(0.4, 0.3, 0.6), vec3(0.8, 0.7, 0.4), cellHash);
    baseColor = mix(baseColor, bismuthColors * vColor.rgb, facetEdge * 0.4);

    baseColor = mix(baseColor, vColor.rgb * u._EmissionColor.rgb, fresnel * 0.3);
    baseColor = mix(baseColor, refractedTex.rgb * baseColor, 0.2);
    baseColor *= 1.0 + animNoise * 0.1;

    float shaderAlpha = u._Alpha * u._Color.a * texColor.a;
    shaderAlpha *= mix(0.6, 1.0, facetEdge);
    shaderAlpha *= 1.0 + fresnel * 0.3;

    // Particle system alpha stays a separate factor so fade in and out survive the effect math.
    outColor = vec4(baseColor, vColor.a * shaderAlpha);
}
