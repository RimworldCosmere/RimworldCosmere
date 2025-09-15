// ========================================
// CutoutAdvanced Shader
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

Shader "Unlit/CutoutAdvanced" {
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
        Name "CutoutAdvanced"
        Tags {
            "IGNOREPROJECTOR"="true"
            "QUEUE"="Transparent-100"
            "RenderType"="Transparent"
        }
        Pass {
            Name ""
            Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
            ZClip On
            ZWrite Off
            Cull Off

            Tags {
                "IGNOREPROJECTOR"="true"
                "QUEUE"="Transparent-100"
                "RenderType"="Transparent"
            }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            // Shader variants for blend modes
            #pragma multi_compile _ BLEND_LINEAR BLEND_SMOOTH BLEND_SHARP BLEND_STEP
            
            // Shader variants for mask features
            #pragma multi_compile _ USE_WEAR_MASK
            #pragma multi_compile _ USE_GLOW_MASK  
            #pragma multi_compile _ USE_SPECIAL_MASK

            sampler2D _MainTex;
            sampler2D _ColorMaskTex;
            sampler2D _WearMaskTex;
            sampler2D _GlowMaskTex;
            sampler2D _SpecialMaskTex;
            float _BlendStrength;
            float _ColorCount;
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
            
            // Pre-computed optimization values
            float4 _HighlightParams; // x: highlightSize, y: power, z: intensity, w: unused
            float2 _RimLightCenter;  // Pre-computed rim light center
            
            uniform float4 _Colors[32];
            uniform float _MetallicValues[32];
            uniform float _SmoothnessValues[32];

            struct appdata {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD;
            };

            struct v2f {
                float4 position : SV_POSITION;
                float2 texcoord : TEXCOORD;
                float4 color : COLOR;
            };

            v2f vert(appdata v) {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord.xy;
                o.color = v.color;
                return o;
            }

            float4 frag(v2f inp) : SV_Target {
                float2 uv = inp.texcoord.xy;
                float4 baseColor = tex2D(_MainTex, uv);

                // Do NOT clip alpha early - do all calculations first like CutoutComplex

                // Early exit for black pixels only (not alpha)
                if (all(baseColor.rgb < float3(0.001, 0.001, 0.001))) {
                    return baseColor;
                }

                float4 colorSample = tex2D(_ColorMaskTex, uv);
                float4 wearSample = float4(0,0,0,1);
                float4 glowSample = float4(0,0,0,1);
                float4 specialSample = float4(0,0,0,1);
                
                #ifdef USE_WEAR_MASK
                    wearSample = tex2D(_WearMaskTex, uv);
                #endif
                #ifdef USE_GLOW_MASK
                    glowSample = tex2D(_GlowMaskTex, uv);
                #endif
                #ifdef USE_SPECIAL_MASK
                    specialSample = tex2D(_SpecialMaskTex, uv);
                #endif
                

                
                // EXTRACT DATA FROM SEPARATE MASKS
                float colorIndex = colorSample.r;           // Red channel of color mask
                float wearAmount = float(0.0);
                float glowStrength = float(0.0);
                float specialZone = float(0.0);
                
                #ifdef USE_WEAR_MASK
                    wearAmount = wearSample.r;             // Red channel of wear mask
                #endif
                #ifdef USE_GLOW_MASK
                    glowStrength = glowSample.r;           // Red channel of glow mask
                #endif
                #ifdef USE_SPECIAL_MASK
                    specialZone = specialSample.r;         // Red channel of special mask
                #endif
                                
                float safeIndex = min(colorIndex, 1.0 - (1.0 / _ColorCount + 1e-5));
                float lutIndex = safeIndex * _ColorCount;
                int idxA = (int)floor(lutIndex);

                float4 lutColor;
                float metallic;
                float smoothness;

                // Cache array values to reduce lookups
                float4 colorA = _Colors[idxA];
                float metallicA = _MetallicValues[idxA];
                float smoothnessA = _SmoothnessValues[idxA];
                
                // Compile-time blend mode optimization
                #if defined(BLEND_LINEAR) || defined(BLEND_SMOOTH) || defined(BLEND_SHARP) || defined(BLEND_STEP)
                    int idxB = (int)clamp(idxA + 1, 0, _ColorCount - 1);
                    float4 colorB = _Colors[idxB];
                    float metallicB = _MetallicValues[idxB];
                    float smoothnessB = _SmoothnessValues[idxB];
                    
                    float blend = saturate(frac(lutIndex) * _BlendStrength);
                    
                    float blendFinal;
                    #ifdef BLEND_LINEAR
                        blendFinal = blend;
                    #elif BLEND_SMOOTH
                        blendFinal = smoothstep(0, 1, blend);
                    #elif BLEND_SHARP
                        blendFinal = blend * blend;
                    #elif BLEND_STEP
                        blendFinal = step(float(0.5), blend);
                    #endif
                    
                    lutColor = lerp(colorA, colorB, blendFinal);
                    metallic = lerp(metallicA, metallicB, blendFinal);
                    smoothness = lerp(smoothnessA, smoothnessB, blendFinal);
                #else
                    // No blending
                    lutColor = colorA;
                    metallic = metallicA;
                    smoothness = smoothnessA;
                #endif
                
                // Apply base color with vertex color like CutoutComplex
                float4 result = baseColor * lutColor;

                // APPLY WEAR/DAMAGE if enabled
                float wearIntensity = float(0.0);
                #ifdef USE_WEAR_MASK
                    if (wearAmount > float(0.01)) {
                        // If wear mask is at maximum brightness (255/255 = 1.0), always show wear at full intensity
                        if (wearAmount >= float(0.99)) {
                            wearIntensity = wearAmount; // Permanent full wear
                        } else {
                            // Normal wear application based on current wear level
                            wearIntensity = _CurrentWearLevel * wearAmount;
                        }
                        
                        float3 wornColor = result.rgb * _WearDarkness;
                        result.rgb = lerp(result.rgb, wornColor, wearIntensity);
                        
                        // Worn metals lose their shine
                        metallic *= (float(1.0) - wearIntensity * float(0.7));
                        smoothness *= (float(1.0) - wearIntensity * float(0.5));
                    }
                #endif

                // FAKE METALLIC EFFECTS
                // 1. Metallic surfaces are more contrasty and slightly desaturated
                float3 grayscale = Luminance(result.rgb);
                result.rgb = lerp(result.rgb, grayscale, metallic * float(0.2) * _MaterialIntensity);
                
                // 2. Add contrast for metallic materials
                float contrast = lerp(float(1.0), float(1.3), metallic * _MaterialIntensity);
                result.rgb = saturate((result.rgb - float(0.5)) * contrast + float(0.5));
                
                // 3. Fake "shine" based on position - metallic surfaces have hot spots
                // Consolidate wear reduction calculation to avoid redundancy
                float wearReduction = float(1.0) - wearIntensity;
                float2 shinePos = frac(uv * float(3.0));
                float shine = pow(max(float(0.0), float(1.0) - length(shinePos - float(0.5)) * float(2.0)), float(4.0));
                result.rgb += shine * metallic * smoothness * float(0.3) * _MaterialIntensity * wearReduction;
                
                // FAKE SMOOTHNESS EFFECTS - Using pre-computed values for performance
                // 1. Smooth surfaces have tighter, brighter highlights
                float highlightSize = lerp(_HighlightParams.x, _HighlightParams.x * float(2.67), smoothness); // 3.0 to 8.0
                float highlight = pow(saturate(sin(uv.x * highlightSize) * 
                                              sin(uv.y * highlightSize)), 
                                     lerp(float(1.0), _HighlightParams.y, smoothness));
                
                // 2. Combine highlight with metallic for final shine
                result.rgb += highlight * smoothness * metallic * _HighlightParams.z * _MaterialIntensity * wearReduction;
                
                // 3. Smooth non-metallic surfaces (like plastic) get a subtle rim light - using pre-computed center
                float rimLight = pow(float(1.0) - saturate(dot(_RimLightCenter, uv - float(0.5))), float(2.0));
                result.rgb += rimLight * smoothness * (float(1.0) - metallic) * float(0.1) * _MaterialIntensity;

                // APPLY GLOW EFFECTS if enabled - AFTER material effects
                #ifdef USE_GLOW_MASK
                    if (glowStrength > float(0.01)) {
                        float actualGlowIntensity;
                        // If glow mask is at maximum brightness (255/255 = 1.0), always glow at full intensity
                        if (glowStrength >= float(0.99)) {
                            actualGlowIntensity = glowStrength; // Permanent full glow
                        } else {
                            // Normal glow application based on current glow level
                            actualGlowIntensity = _CurrentGlowLevel * glowStrength;
                        }
                        result.rgb += _GlowColor.rgb * actualGlowIntensity * _GlowIntensity;
                    }
                #endif

                // SPECIAL ZONES if enabled
                #ifdef USE_SPECIAL_MASK
                    if (specialZone > float(0.01)) {
                        if (specialZone > float(0.8)) {
                            // Gem areas - extra sparkle
                            float sparkle = frac(sin(dot(uv * float(100.0), float2(12.9898, 78.233))) * float(43758.5453));
                            result.rgb += sparkle * float(0.5);
                        } else if (specialZone > float(0.5)) {
                            // Rune areas - pulsing glow
                            float pulse = sin(_Time.y * float(3.0)) * float(0.5) + float(0.5);
                            result.rgb += _GlowColor.rgb * pulse * float(0.3);
                        } else if (specialZone > float(0.3)) {
                            // Metal inlay - extra metallic shine
                            result.rgb += shine * float(0.5);
                        }
                    }
                #endif

                // Alpha clipping at the very end like CutoutComplex
                if (result.a < 0.5) {
                    discard;
                }

                return result;
            }
            ENDCG
        }
    }
    Fallback "Transparent/Diffuse"
}