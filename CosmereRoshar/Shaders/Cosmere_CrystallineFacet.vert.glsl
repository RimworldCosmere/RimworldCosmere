#version 450

// Rockspren shell. Only job beyond the standard transform is a small vertical bob, so the block
// below is the leading prefix of CrystallineFacet.frag's block - same trick PlantWindSway.vert uses.

layout(location = 0) in vec3 inPosition; // @vertex Position required
layout(location = 2) in vec4 inColor; // @vertex Color optional @default = vec4(1.0, 1.0, 1.0, 1.0)
layout(location = 3) in vec4 inUv0; // @vertex TexCoord0 optional @default = vec4(0.0, 0.0, 0.0, 0.0)

layout(push_constant) uniform Push { mat4 modelMatrix; } push;

layout(location = 0) out vec4 vColor;
layout(location = 1) out vec2 vUv;

layout(set = 0, binding = 0) uniform VertexUniforms
{
    mat4 ViewProj;
} uVertex;

layout(set = 2, binding = 0) uniform MaterialUniforms
{
    vec4 _MainTex_ST;     // @default = vec4(0.0, 0.0, 1.0, 1.0)
    vec4 _Color;          // @color @default = vec4(0.7, 0.6, 0.5, 0.8)
    vec4 _EdgeColor;      // @color @default = vec4(0.9, 0.8, 0.6, 1.0)
    vec4 _EmissionColor;  // @color @default = vec4(0.8, 0.7, 0.5, 0.9)
    vec4 _Time;           // global, published by MapRenderFeature
    float _FacetSize;      // @default = 8.0
    float _FacetSharpness; // @default = 0.8
    float _FacetOffset;    // @default = 0.5
    float _RefractStrength; // @default = 0.03
    float _FresnelPower;    // @default = 2.0
    float _PulseSpeed;      // @default = 0.5
    float _PulseIntensity;  // @default = 0.3
    float _NoiseScale;      // @default = 1.0
    float _NoiseSpeed;      // @default = 0.2
    float _Alpha;           // @default = 0.85
} u;

void main()
{
    float time = u._Time.y * u._NoiseSpeed;
    vec3 pos = inPosition + vec3(0.0, sin(time + inPosition.x * 2.0) * 0.01, 0.0);

    gl_Position = uVertex.ViewProj * push.modelMatrix * vec4(pos, 1.0);
    vColor = inColor;
    vUv = inUv0.xy;
}
