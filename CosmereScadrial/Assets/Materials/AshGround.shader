Shader "Custom/AshGround"
{
    Properties
    {
        _AshColor ("Ash Color Shallow", Color) = (0.42, 0.36, 0.31, 1)
        _AshColorDeep ("Ash Color Deep", Color) = (0.13, 0.12, 0.12, 1)
        _AshSeverity ("Severity", Range(0, 1)) = 1
        _MaxGroundOpacity ("Max Opacity", Range(0, 1)) = 0.8

        _DriftScale ("Drift Noise Scale", Range(0.01, 1)) = 0.09
        _GrainScale ("Grain Noise Scale", Range(0.1, 8)) = 2.5
        _CreepSharpness ("Creep Edge Sharpness", Range(1, 16)) = 6
        _CoverageGain ("Coverage Gain", Range(1, 3)) = 1.35
        _NoiseInfluence ("Noise Influence", Range(0, 1)) = 0.75
    }

    SubShader
    {
        // 2401, matching RimWorld's own Custom/Snow. Above terrain, below pawns and items.
        // Geometry+50 puts it under the terrain layers, which then paint over it entirely.
        Tags { "Queue" = "Geometry+401" "RenderType" = "Transparent" "IgnoreProjector" = "True" }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _AshColor;
            fixed4 _AshColorDeep;
            float _AshSeverity;
            float _MaxGroundOpacity;
            float _DriftScale;
            float _GrainScale;
            float _CreepSharpness;
            float _CoverageGain;
            float _NoiseInfluence;

            struct appdata
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                fixed4 color : COLOR;
                float2 world : TEXCOORD0;
            };

            // The section mesh carries verts and tris only, no UV channel, and the verts are
            // already world space. Everything spatial has to come off vertex.xz.
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.world = mul(unity_ObjectToWorld, v.vertex).xz;
                return o;
            }

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float valueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = hash21(i);
                float b = hash21(i + float2(1, 0));
                float c = hash21(i + float2(0, 1));
                float d = hash21(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float depth = i.color.a;
                float jitter = i.color.r;

                float drift = valueNoise(i.world * _DriftScale);
                float grain = valueNoise(i.world * _GrainScale);
                float n = lerp(drift, grain, 0.25);

                // Ash creeps outward from the noise valleys as depth rises rather than the whole
                // map brightening together. Coverage saturates before depth does, so the deep end
                // reads as solid ground rather than permanent patchiness.
                float coverage = saturate(depth * _CoverageGain);
                float edge = saturate((coverage - n * _NoiseInfluence + jitter * 0.06) * _CreepSharpness);

                fixed3 col = lerp(_AshColor.rgb, _AshColorDeep.rgb, saturate(depth * 1.2));
                col *= 0.92 + grain * 0.16;

                return fixed4(col, edge * _AshSeverity * _MaxGroundOpacity);
            }
            ENDCG
        }
    }

    Fallback Off
}
