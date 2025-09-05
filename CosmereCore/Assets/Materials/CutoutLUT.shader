Shader "Unlit/Cutout_LUT" {
    Properties {
        _MainTex ("Main Texture", 2D) = "white" {}
        _LUTTex ("LUT Mask Texture", 2D) = "white" {}
        _BlendStrength ("Blend Strength", Range(0, 2)) = 0
        _ColorCount ("Color Count", Float) = 32
        _FallbackColor ("Fallback Color", Color) = (1, 1, 1, 1)
        _BlendMode ("Blend Mode", Float) = 2
        _MaterialIntensity ("Material Effect Intensity", Range(0, 1)) = 1
        
        // Multi-channel properties
        [Toggle] _UseGreenChannel ("Use Wear (Green)", Float) = 0
        [Toggle] _UseBlueChannel ("Use Glow (Blue)", Float) = 0
        [Toggle] _UseAlphaChannel ("Use Special Zones (Alpha)", Float) = 0
        _WearDarkness ("Wear Darkness", Range(0, 1)) = 0.4
        _GlowColor ("Glow Color", Color) = (1, 1, 1, 1)
        _GlowIntensity ("Glow Intensity", Range(0, 3)) = 2
    }
    SubShader {
        Tags {
            "IGNOREPROJECTOR"="true"
            "QUEUE"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
            ZClip On
            ZWrite Off
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            sampler2D _LUTTex;
            float _BlendStrength;
            float _ColorCount;
            float4 _FallbackColor;
            float _BlendMode;
            float _MaterialIntensity;
            
            // Multi-channel variables
            float _UseGreenChannel;
            float _UseBlueChannel;
            float _UseAlphaChannel;
            float _WearDarkness;
            float4 _GlowColor;
            float _GlowIntensity;
            
            uniform float4 _Colors[32];
            uniform float _MetallicValues[32];
            uniform float _SmoothnessValues[32];

            struct appdata {
                float4 vertex : POSITION;
                float4 texcoord : TEXCOORD;
            };

            struct v2f {
                float4 position : SV_POSITION;
                float2 texcoord : TEXCOORD;
            };

            v2f vert(appdata v) {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord.xy;
                return o;
            }

            float4 frag(v2f inp) : SV_Target {
                float4 baseColor = tex2D(_MainTex, inp.texcoord.xy);

                // Early exit for fully transparent pixels
                if (baseColor.a < 0.01) {
                    discard;
                }
                
                // Early exit for black pixels
                if (all(baseColor.rgb < 0.001)) {
                    return baseColor;
                }

                float4 lutSample = tex2D(_LUTTex, inp.texcoord.xy);

                if (lutSample.a < 0.01 && _UseAlphaChannel < 0.5) {
                    return baseColor * _FallbackColor;
                }
                
                // EXTRACT DATA FROM ALL CHANNELS
                float colorIndex = lutSample.r;      // Red: Color palette index
                float wearAmount = lutSample.g;      // Green: Wear/damage
                float glowStrength = lutSample.b;    // Blue: Glow intensity
                float specialZone = lutSample.a;     // Alpha: Special zones
                                
                float lutIndex = colorIndex * (_ColorCount);
                int idxA = (int)clamp(floor(lutIndex), 0.0, _ColorCount - 1.0);

                float4 lutColor;
                float metallic;
                float smoothness;

                if (_BlendMode < 0.5) {
                    // Mode 0: None - no blending
                    lutColor = _Colors[idxA];
                    metallic = _MetallicValues[idxA];
                    smoothness = _SmoothnessValues[idxA];
                } else {
                    // All blend modes
                    int idxB = (int)clamp(idxA + 1, 0.0, _ColorCount - 1.0);
                    float blend = saturate(frac(lutIndex) * _BlendStrength);
                    float blendFinal;
                    
                    if (_BlendMode < 1.5) {
                        // Mode 1: Linear
                        blendFinal = blend;
                    } else if (_BlendMode < 2.5) {
                        // Mode 2: Smooth
                        blendFinal = smoothstep(0, 1, blend);
                    } else if (_BlendMode < 3.5) {
                        // Mode 3: Sharp
                        blendFinal = blend * blend;
                    } else {
                        // Mode 4: Step
                        blendFinal = step(0.5, blend);
                    }
                    
                    lutColor = lerp(_Colors[idxA], _Colors[idxB], blendFinal);
                    metallic = lerp(_MetallicValues[idxA], _MetallicValues[idxB], blendFinal);
                    smoothness = lerp(_SmoothnessValues[idxA], _SmoothnessValues[idxB], blendFinal);
                }
                
                // Apply base color
                float4 result = baseColor * lutColor;

                // APPLY WEAR/DAMAGE (Green channel) if enabled
                if (_UseGreenChannel > 0.5 && wearAmount > 0.01) {
                    float3 wornColor = result.rgb * _WearDarkness;
                    result.rgb = lerp(result.rgb, wornColor, wearAmount);
                    // Worn metals lose their shine
                    metallic *= (1.0 - wearAmount * 0.7);
                    smoothness *= (1.0 - wearAmount * 0.5);
                }

                // FAKE METALLIC EFFECTS
                // 1. Metallic surfaces are more contrasty and slightly desaturated
                float3 grayscale = dot(result.rgb, float3(0.299, 0.587, 0.114));
                result.rgb = lerp(result.rgb, grayscale, metallic * 0.2 * _MaterialIntensity);
                
                // 2. Add contrast for metallic materials
                float contrast = lerp(1.0, 1.3, metallic * _MaterialIntensity);
                result.rgb = saturate((result.rgb - 0.5) * contrast + 0.5);
                
                // 3. Fake "shine" based on position - metallic surfaces have hot spots
                float wearReduction = (_UseGreenChannel > 0.5) ? (1.0 - wearAmount) : 1.0;
                float2 shinePos = frac(inp.texcoord * 3.0);
                float shine = pow(max(0, 1.0 - length(shinePos - 0.5) * 2.0), 4.0);
                result.rgb += shine * metallic * smoothness * 0.3 * _MaterialIntensity * wearReduction;
                
                // FAKE SMOOTHNESS EFFECTS  
                // 1. Smooth surfaces have tighter, brighter highlights
                float highlightSize = lerp(3.0, 8.0, smoothness);
                float highlight = pow(saturate(sin(inp.texcoord.x * highlightSize) * 
                                              sin(inp.texcoord.y * highlightSize)), 
                                     lerp(1.0, 4.0, smoothness));
                
                // 2. Combine highlight with metallic for final shine
                result.rgb += highlight * smoothness * metallic * 0.2 * _MaterialIntensity * wearReduction;
                
                // 3. Smooth non-metallic surfaces (like plastic) get a subtle rim light
                float rimLight = pow(1.0 - saturate(dot(float2(0.5, 0.5), inp.texcoord - 0.5)), 2.0);
                result.rgb += rimLight * smoothness * (1.0 - metallic) * 0.1 * _MaterialIntensity;

                // APPLY GLOW EFFECTS (Blue channel) if enabled - AFTER material effects
                if (_UseBlueChannel > 0.5 && glowStrength > 0.01) {
                    result.rgb += _GlowColor.rgb * glowStrength * _GlowIntensity;
                }

                // SPECIAL ZONES (Alpha channel) if enabled
                if (_UseAlphaChannel > 0.5 && specialZone > 0.01) {
                    if (specialZone > 0.8) {
                        // Gem areas - extra sparkle
                        float sparkle = frac(sin(dot(inp.texcoord * 100.0, float2(12.9898, 78.233))) * 43758.5453);
                        result.rgb += sparkle * 0.5;
                    } else if (specialZone > 0.5) {
                        // Rune areas - pulsing glow
                        float pulse = sin(_Time.y * 3.0) * 0.5 + 0.5;
                        result.rgb += _GlowColor.rgb * pulse * 0.3;
                    } else if (specialZone > 0.3) {
                        // Metal inlay - extra metallic shine
                        result.rgb += shine * 0.5;
                    }
                }

                return result;
            }
            ENDCG
        }
    }
    Fallback "Transparent/Diffuse"
}