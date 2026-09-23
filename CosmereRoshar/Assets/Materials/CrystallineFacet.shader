Shader "Custom/CrystallineFacet"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0.7, 0.6, 0.5, 0.8)
        _EdgeColor ("Edge Color", Color) = (0.9, 0.8, 0.6, 1.0)
        _EmissionColor ("Emission Color", Color) = (0.8, 0.7, 0.5, 0.9)
        
        _FacetSize ("Facet Size", Range(2, 20)) = 8
        _FacetSharpness ("Facet Sharpness", Range(0.1, 2)) = 0.8
        _FacetOffset ("Facet Offset", Range(0, 1)) = 0.5
        
        _RefractStrength ("Refraction Strength", Range(0, 0.1)) = 0.03
        _FresnelPower ("Fresnel Power", Range(0.5, 5)) = 2
        
        _PulseSpeed ("Pulse Speed", Range(0, 2)) = 0.5
        _PulseIntensity ("Pulse Intensity", Range(0, 1)) = 0.3
        
        _NoiseScale ("Noise Scale", Range(0.1, 5)) = 1
        _NoiseSpeed ("Noise Animation Speed", Range(0, 2)) = 0.2
        
        _Alpha ("Overall Alpha", Range(0, 1)) = 0.85
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _EdgeColor;
            float4 _EmissionColor;
            
            float _FacetSize;
            float _FacetSharpness;
            float _FacetOffset;
            
            float _RefractStrength;
            float _FresnelPower;
            
            float _PulseSpeed;
            float _PulseIntensity;
            
            float _NoiseScale;
            float _NoiseSpeed;
            
            float _Alpha;
            
            float _GameSeconds;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
                float4 color : COLOR;
            };
            
            float hash3(float3 p)
            {
                p = frac(p * 0.1031);
                p += dot(p, p.yzx + 33.33);
                return frac((p.x + p.y) * p.z);
            }
            
            float noise3d(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                
                return lerp(
                    lerp(
                        lerp(hash3(i + float3(0,0,0)), hash3(i + float3(1,0,0)), f.x),
                        lerp(hash3(i + float3(0,1,0)), hash3(i + float3(1,1,0)), f.x),
                        f.y
                    ),
                    lerp(
                        lerp(hash3(i + float3(0,0,1)), hash3(i + float3(1,0,1)), f.x),
                        lerp(hash3(i + float3(0,1,1)), hash3(i + float3(1,1,1)), f.x),
                        f.y
                    ),
                    f.z
                );
            }
            
            float3 voronoi3d(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);
                
                float minDist = 1.0;
                float3 minPoint = float3(0,0,0);
                
                for(int x = -1; x <= 1; x++)
                {
                    for(int y = -1; y <= 1; y++)
                    {
                        for(int z = -1; z <= 1; z++)
                        {
                            float3 neighbor = float3(x, y, z);
                            float3 randomOffset = float3(
                                hash3(i + neighbor),
                                hash3(i + neighbor + 17.0),
                                hash3(i + neighbor + 31.0)
                            );
                            
                            float3 diff = neighbor + randomOffset * _FacetOffset - f;
                            float dist = length(diff);
                            
                            if(dist < minDist)
                            {
                                minDist = dist;
                                minPoint = i + neighbor + randomOffset * _FacetOffset;
                            }
                        }
                    }
                }
                
                return float3(minDist, minPoint.xy);
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                
                float time = _GameSeconds * _NoiseSpeed;
                float3 animatedPos = v.vertex.xyz + float3(0, sin(time + v.vertex.x * 2) * 0.01, 0);
                
                o.vertex = UnityObjectToClipPos(float4(animatedPos, 1));
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, float4(animatedPos, 1)).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(_WorldSpaceCameraPos - o.worldPos);
                o.color = v.color;
                
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                float4 texColor = tex2D(_MainTex, i.uv);
                
                float2 centeredUV = i.uv - 0.5;
                float distFromCenter = length(centeredUV);
                
                // Create blocky facets like bismuth
                float2 facetCoord = i.uv * _FacetSize;
                
                float2 facetCell = floor(facetCoord);
                float2 facetUV = frac(facetCoord);
                
                float minDist = 1.0;
                for(int x = -1; x <= 1; x++)
                {
                    for(int y = -1; y <= 1; y++)
                    {
                        float2 neighbor = float2(x, y);
                        float2 randomOffset = float2(
                            hash3(float3(facetCell + neighbor, 0)),
                            hash3(float3(facetCell + neighbor, 17))
                        ) * 0.5 + 0.5;
                        
                        float2 diff = neighbor + randomOffset - facetUV;
                        float dist = length(diff);
                        minDist = min(minDist, dist);
                    }
                }
                
                // Make edges sharper and more geometric for bismuth look
                float facetEdge = 1.0 - smoothstep(0.0, _FacetSharpness * 0.3, minDist);
                
                // Add cell-based variation for bismuth-like stepped appearance
                float cellHash = hash3(float3(facetCell, 0));
                
                float fresnel = pow(1.0 - distFromCenter, _FresnelPower);
                
                float2 refractOffset = centeredUV * _RefractStrength * facetEdge;
                float4 refractedTex = tex2D(_MainTex, i.uv + refractOffset);
                
                float animNoise = noise3d(float3(i.uv * _NoiseScale, _GameSeconds * _NoiseSpeed));
                
                // Create more dramatic color stepping for bismuth-like appearance
                float3 baseColor = lerp(
                    i.color.rgb * _Color.rgb * texColor.rgb,
                    i.color.rgb * _EdgeColor.rgb,
                    facetEdge
                );
                
                // Add metallic/iridescent bismuth-like color variation
                float3 bismuthColors = lerp(
                    float3(0.4, 0.3, 0.6), // Purple-blue
                    float3(0.8, 0.7, 0.4), // Gold-yellow
                    cellHash
                );
                baseColor = lerp(baseColor, bismuthColors * i.color.rgb, facetEdge * 0.4);
                
                baseColor = lerp(baseColor, i.color.rgb * _EmissionColor.rgb, fresnel * 0.3);
                baseColor = lerp(baseColor, refractedTex.rgb * baseColor, 0.2);
                
                // Reduce noise for cleaner geometric look
                baseColor *= (1.0 + animNoise * 0.1);
                
                // Respect particle system alpha for smooth fade in/out
                float particleAlpha = i.color.a;
                
                // Calculate shader effects alpha (but don't multiply with particle alpha yet)
                float shaderAlpha = _Alpha * _Color.a * texColor.a;
                shaderAlpha *= lerp(0.6, 1.0, facetEdge);
                shaderAlpha *= (1.0 + fresnel * 0.3);
                
                // Final alpha respects particle system animation
                float alpha = particleAlpha * shaderAlpha;
                
                return float4(baseColor, alpha);
            }
            ENDCG
        }
    }
    
    FallBack "Transparent/Diffuse"
}