Shader "Custom/FlowingParticleStream"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0.3, 0.8, 0.4, 0.7)
        _StreamColor ("Stream Color", Color) = (0.8, 1.0, 0.6, 1.0)
        _ParticleColor ("Particle Color", Color) = (1.0, 1.0, 0.8, 0.9)
        
        _FlowPattern ("Flow Pattern", Int) = 0  // 0=Grass, 1=Wind, 2=Spiral
        _FlowSpeed ("Flow Speed", Range(0.1, 5)) = 1.5
        _FlowDensity ("Flow Density", Range(1, 20)) = 8
        _FlowWidth ("Stream Width", Range(0.01, 0.2)) = 0.05
        
        _ParticleCount ("Particle Count", Range(2, 15)) = 6
        _ParticleSize ("Particle Size", Range(0.005, 0.05)) = 0.02
        _ParticleSpeed ("Particle Speed", Range(0.5, 3)) = 1.2
        
        _AnimationSpeed ("Animation Speed", Range(0.1, 3)) = 1.0
        _Intensity ("Intensity", Range(0.1, 2)) = 1.0
        _FadeEdges ("Fade Edges", Range(0, 1)) = 0.8
        
        _Alpha ("Overall Alpha", Range(0, 1)) = 0.8
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
            float4 _StreamColor;
            float4 _ParticleColor;
            
            int _FlowPattern;
            float _FlowSpeed;
            float _FlowDensity;
            float _FlowWidth;
            
            float _ParticleCount;
            float _ParticleSize;
            float _ParticleSpeed;
            
            float _AnimationSpeed;
            float _Intensity;
            float _FadeEdges;
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
                float4 color : COLOR;
            };
            
            float hash21(float2 p)
            {
                p = frac(p * float2(233.34, 851.73));
                p += dot(p, p + 23.45);
                return frac(p.x * p.y);
            }
            
            float2 hash22(float2 p)
            {
                p = frac(p * float2(127.1, 311.7));
                p = -1.0 + 2.0 * frac(p * 43758.5453123);
                return p;
            }
            
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                
                return lerp(lerp(hash21(i + float2(0,0)), hash21(i + float2(1,0)), f.x),
                           lerp(hash21(i + float2(0,1)), hash21(i + float2(1,1)), f.x), f.y);
            }
            
            float2 getFlowDirection(float2 uv, float time)
            {
                float2 direction = float2(0, 1);
                
                if (_FlowPattern == 0) // Grass - gentle swaying
                {
                    float swayAmount = sin(time * _AnimationSpeed + uv.x * 3.14159) * 0.3;
                    direction = float2(swayAmount, 1.0 - abs(swayAmount) * 0.5);
                    direction = normalize(direction);
                }
                else if (_FlowPattern == 1) // Wind - flowing curves
                {
                    float2 noiseOffset = float2(time * _AnimationSpeed * 0.3, 0);
                    float n = noise((uv + noiseOffset) * 3);
                    float angle = n * 3.14159 * 2 + time * _AnimationSpeed;
                    direction = float2(cos(angle), sin(angle));
                }
                else if (_FlowPattern == 2) // Spiral - rotating flow
                {
                    float2 center = uv - 0.5;
                    float angle = atan2(center.y, center.x) + time * _AnimationSpeed;
                    float radius = length(center);
                    direction = float2(-sin(angle), cos(angle)) * (1.0 - radius);
                    direction = normalize(direction + float2(radius * 0.3, 0));
                }
                
                return direction;
            }
            
            float getStreamIntensity(float2 uv, float2 flowDir, float time)
            {
                float streamIntensity = 0.0;
                
                for(int i = 0; i < 8; i++)
                {
                    if(i >= _FlowDensity) break;
                    
                    float streamOffset = float(i) / _FlowDensity;
                    float2 streamStart = float2(streamOffset, 0) + hash22(float2(i, 0)) * 0.1;
                    
                    float2 currentPos = streamStart;
                    float segmentLength = 0.1;
                    
                    for(int j = 0; j < 10; j++)
                    {
                        float t = float(j) * segmentLength;
                        float2 segmentDir = getFlowDirection(currentPos, time);
                        float2 nextPos = currentPos + segmentDir * segmentLength;
                        
                        float2 lineDir = nextPos - currentPos;
                        float2 toPoint = uv - currentPos;
                        
                        float proj = dot(toPoint, lineDir) / dot(lineDir, lineDir);
                        proj = saturate(proj);
                        
                        float2 closest = currentPos + lineDir * proj;
                        float dist = length(uv - closest);
                        
                        if(dist < _FlowWidth)
                        {
                            float lineAlpha = 1.0 - (dist / _FlowWidth);
                            streamIntensity = max(streamIntensity, lineAlpha);
                        }
                        
                        currentPos = nextPos;
                        if(currentPos.y > 1.0) break;
                    }
                }
                
                return streamIntensity;
            }
            
            float getParticleIntensity(float2 uv, float time)
            {
                float particleIntensity = 0.0;
                
                for(int i = 0; i < 15; i++)
                {
                    if(i >= _ParticleCount) break;
                    
                    float particleId = float(i);
                    float2 randomSeed = float2(particleId * 17.3, particleId * 31.7);
                    
                    float2 startPos = float2(hash21(randomSeed), hash21(randomSeed + 1.0));
                    float particleTime = frac(time * _ParticleSpeed * _AnimationSpeed + hash21(randomSeed + 2.0));
                    
                    float2 currentPos = startPos;
                    float step = particleTime;
                    
                    for(int j = 0; j < 20; j++)
                    {
                        float t = float(j) * 0.05;
                        if(t > step) break;
                        
                        float2 flowDir = getFlowDirection(currentPos, time + t);
                        currentPos += flowDir * 0.05;
                        
                        if(currentPos.x < 0 || currentPos.x > 1 || currentPos.y < 0 || currentPos.y > 1)
                            break;
                    }
                    
                    float dist = length(uv - currentPos);
                    if(dist < _ParticleSize)
                    {
                        float alpha = 1.0 - (dist / _ParticleSize);
                        alpha = pow(alpha, 2);
                        particleIntensity = max(particleIntensity, alpha);
                    }
                }
                
                return particleIntensity;
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                float4 texColor = tex2D(_MainTex, i.uv);
                float time = _Time.y * _AnimationSpeed; // Use Unity's built-in _Time instead of _GameSeconds
                
                float2 centeredUV = i.uv - 0.5;
                float distFromCenter = length(centeredUV);
                
                float2 flowDir = getFlowDirection(i.uv, time);
                float streamIntensity = getStreamIntensity(i.uv, flowDir, time);
                float particleIntensity = getParticleIntensity(i.uv, time);
                
                float totalIntensity = max(streamIntensity, particleIntensity);
                
                // Get base texture and color
                float3 baseColor = texColor.rgb * _Color.rgb * i.color.rgb;
                float baseAlpha = texColor.a * _Color.a * i.color.a * _Alpha;
                
                // Calculate flow effects
                float3 streamContrib = _StreamColor.rgb * streamIntensity * _Intensity;
                float3 particleContrib = _ParticleColor.rgb * particleIntensity * _Intensity;
                float3 effectsColor = (streamContrib + particleContrib) * i.color.rgb;
                
                // Blend base texture with flow effects
                float3 finalColor;
                float alpha;
                
                if (totalIntensity > 0.05) {
                    // Where there are effects, blend them with the base
                    finalColor = lerp(baseColor, baseColor + effectsColor, totalIntensity);
                    alpha = max(baseAlpha, totalIntensity * _Alpha * i.color.a);
                } else {
                    // Where there are no effects, just show the base texture
                    finalColor = baseColor;
                    alpha = baseAlpha;
                }
                
                float edgeFade = 1.0 - pow(distFromCenter * 2.0, _FadeEdges);
                edgeFade = saturate(edgeFade);
                alpha *= edgeFade;
                
                return float4(finalColor, alpha);
            }
            ENDCG
        }
    }
    
    FallBack "Transparent/Diffuse"
}