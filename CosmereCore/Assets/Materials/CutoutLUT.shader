// ========================================
// CutoutLUT Shader
// ========================================
// Multi-texture masking shader for palette swapping, wear, glow, and special effects.
// Supports configurable levels for progressive wear and glow states.
//
// TEXTURE REQUIREMENTS:
// - MainTex: Base diffuse texture
// - ColorMaskTex: Grayscale mask for palette color selection (red channel used)
// - WearMaskTex: Grayscale mask for wear zones (red channel used)
// - GlowMaskTex: Grayscale mask for glow zones (red channel used)
// - SpecialMaskTex: Grayscale mask for special effect zones (red channel used)
//
// LEVEL SYSTEM:
// - Artist paints masks with grayscale ranges (0-255 divided by level count)
// - Current level parameters control which zones are active
// - Example: 4 wear levels = zones 0-63, 64-127, 128-191, 192-255
// ========================================

Shader "Unlit/Cutout_LUT" {
    Properties {
        // ========================================
        // BASE TEXTURES
        // ========================================
        _MainTex ("Main Texture", 2D) = "white" {}
        _ColorMaskTex ("Color Mask (R)", 2D) = "white" {}
        _WearMaskTex ("Wear Mask", 2D) = "black" {}
        _GlowMaskTex ("Glow Mask", 2D) = "black" {}
        _SpecialMaskTex ("Special Zones Mask", 2D) = "black" {}
        
        // ========================================
        // COLOR PALETTE SYSTEM
        // ========================================
        _BlendStrength ("Blend Strength", Range(0, 2)) = 0
        _ColorCount ("Color Count", Float) = 32
        _FallbackColor ("Fallback Color", Color) = (1, 1, 1, 1)
        _BlendMode ("Blend Mode", Float) = 2
        _MaterialIntensity ("Material Effect Intensity", Range(0, 1)) = 1
        
        // ========================================
        // MASK CONFIGURATION
        // ========================================
        [Toggle] _UseWearMask ("Use Wear Mask", Float) = 0
        [Toggle] _UseGlowMask ("Use Glow Mask", Float) = 0
        [Toggle] _UseSpecialMask ("Use Special Zones Mask", Float) = 0
        
        // Level counts: How many levels the artist painted (1-32)
        _WearLevelCount ("Wear Level Count", Range(1, 32)) = 4
        _GlowLevelCount ("Glow Level Count", Range(1, 32)) = 4
        
        // Current states: What level of effect to show
        _CurrentWearLevel ("Current Wear Level", Range(0, 1)) = 0
        _CurrentGlowLevel ("Current Glow Level", Range(0, 3)) = 0
        
        // ========================================
        // EFFECT PARAMETERS
        // ========================================
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
            sampler2D _ColorMaskTex;
            sampler2D _WearMaskTex;
            sampler2D _GlowMaskTex;
            sampler2D _SpecialMaskTex;
            float _BlendStrength;
            float _ColorCount;
            float4 _FallbackColor;
            float _BlendMode;
            float _MaterialIntensity;
            
            // Multi-channel variables
            float _UseWearMask;
            float _UseGlowMask;
            float _UseSpecialMask;
            float _WearLevelCount;
            float _GlowLevelCount;
            float _CurrentWearLevel;
            float _CurrentGlowLevel;
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

                // Sample separate mask textures
                float4 colorSample = tex2D(_ColorMaskTex, inp.texcoord.xy);
                float4 wearSample = tex2D(_WearMaskTex, inp.texcoord.xy);
                float4 glowSample = tex2D(_GlowMaskTex, inp.texcoord.xy);
                float4 specialSample = tex2D(_SpecialMaskTex, inp.texcoord.xy);
                
                // If ColorMaskTex sample equals MainTex sample, it means no separate color mask was provided
                // (Unity defaults to using the same texture when none is specified)
                // In this case, use inverted MainTex as the color mask
                if (all(abs(colorSample.rgb - baseColor.rgb) < 0.001)) {
                    // Use inverted main texture as the color mask
                    colorSample = float4(1.0 - baseColor.rgb, baseColor.a);
                }

                if (colorSample.a < 0.01) {
                    return baseColor * _FallbackColor;
                }
                
                // EXTRACT DATA FROM SEPARATE MASKS
                float colorIndex = colorSample.r;           // Red channel of color mask
                float wearAmount = wearSample.r;            // Red channel of wear mask
                float glowStrength = glowSample.r;          // Red channel of glow mask  
                float specialZone = specialSample.r;        // Red channel of special mask
                                
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

                // APPLY WEAR/DAMAGE if enabled
                if (_UseWearMask > 0.5 && wearAmount > 0.01) {
                    // Convert wear mask value to damage zone using wear level count
                    int damageZone = (int)(wearAmount * _WearLevelCount);
                    
                    // Convert current wear level to zone using wear level count
                    int currentWearZone = (int)(_CurrentWearLevel * _WearLevelCount);
                    
                    // Only apply wear if current wear level >= this zone's level
                    if (currentWearZone >= damageZone) {
                        // Use the actual wear level as intensity
                        float wearIntensity = _CurrentWearLevel;
                        float3 wornColor = result.rgb * _WearDarkness;
                        result.rgb = lerp(result.rgb, wornColor, wearIntensity);
                        
                        // Worn metals lose their shine
                        metallic *= (1.0 - wearIntensity * 0.7);
                        smoothness *= (1.0 - wearIntensity * 0.5);
                    }
                }

                // FAKE METALLIC EFFECTS
                // 1. Metallic surfaces are more contrasty and slightly desaturated
                float3 grayscale = dot(result.rgb, float3(0.299, 0.587, 0.114));
                result.rgb = lerp(result.rgb, grayscale, metallic * 0.2 * _MaterialIntensity);
                
                // 2. Add contrast for metallic materials
                float contrast = lerp(1.0, 1.3, metallic * _MaterialIntensity);
                result.rgb = saturate((result.rgb - 0.5) * contrast + 0.5);
                
                // 3. Fake "shine" based on position - metallic surfaces have hot spots
                float wearReduction = 1.0;
                if (_UseWearMask > 0.5 && wearAmount > 0.01) {
                    int damageZone = (int)(wearAmount * _WearLevelCount);
                    int currentWearZone = (int)(_CurrentWearLevel * _WearLevelCount);
                    if (currentWearZone >= damageZone) {
                        float wearIntensity = _CurrentWearLevel;
                        wearReduction = (1.0 - wearIntensity);
                    }
                }
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

                // APPLY GLOW EFFECTS if enabled - AFTER material effects
                if (_UseGlowMask > 0.5 && glowStrength > 0.01) {
                    // Convert glow mask value to glow zone using glow level count
                    int glowZone = (int)(glowStrength * _GlowLevelCount);
                    
                    // Only apply glow if current glow level >= this zone's level
                    if (_CurrentGlowLevel >= glowZone) {
                        // Calculate glow intensity based on current glow level and level count
                        float actualGlowIntensity = (_CurrentGlowLevel + 1.0) / _GlowLevelCount;
                        result.rgb += _GlowColor.rgb * actualGlowIntensity * _GlowIntensity;
                    }
                }

                // SPECIAL ZONES if enabled
                if (_UseSpecialMask > 0.5 && specialZone > 0.01) {
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