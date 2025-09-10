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
            half _BlendStrength;
            half _ColorCount;
            half4 _FallbackColor;
            half _MaterialIntensity;
            
            // Multi-channel variables
            half _UseWearMask;
            half _UseGlowMask;
            half _UseSpecialMask;
            half _WearLevelCount;
            half _GlowLevelCount;
            half _CurrentWearLevel;
            half _CurrentGlowLevel;
            half _WearDarkness;
            half4 _GlowColor;
            half _GlowIntensity;
            
            // Pre-computed optimization values
            half4 _HighlightParams; // x: highlightSize, y: power, z: intensity, w: unused
            half2 _RimLightCenter;  // Pre-computed rim light center
            
            uniform half4 _Colors[32];
            uniform half _MetallicValues[32];
            uniform half _SmoothnessValues[32];

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

            half4 frag(v2f inp) : SV_Target {
                half2 uv = inp.texcoord.xy;
                half4 baseColor = tex2D(_MainTex, uv);

                // Early exit for fully transparent pixels
                clip(baseColor.a - half(0.01));
                
                // Early exit for black pixels
                if (all(baseColor.rgb < half3(0.001, 0.001, 0.001))) {
                    return baseColor;
                }

                // Sample mask textures conditionally to avoid unnecessary texture reads
                half4 colorSample = tex2D(_ColorMaskTex, uv);
                half4 wearSample = half4(0,0,0,1);
                half4 glowSample = half4(0,0,0,1);
                half4 specialSample = half4(0,0,0,1);
                
                #ifdef USE_WEAR_MASK
                    wearSample = tex2D(_WearMaskTex, uv);
                #endif
                #ifdef USE_GLOW_MASK
                    glowSample = tex2D(_GlowMaskTex, uv);
                #endif
                #ifdef USE_SPECIAL_MASK
                    specialSample = tex2D(_SpecialMaskTex, uv);
                #endif
                

                if (colorSample.a < half(0.01)) {
                    return baseColor * _FallbackColor;
                }
                
                // EXTRACT DATA FROM SEPARATE MASKS
                half colorIndex = colorSample.r;           // Red channel of color mask
                half wearAmount = half(0.0);
                half glowStrength = half(0.0);
                half specialZone = half(0.0);
                
                #ifdef USE_WEAR_MASK
                    wearAmount = wearSample.r;             // Red channel of wear mask
                #endif
                #ifdef USE_GLOW_MASK
                    glowStrength = glowSample.r;           // Red channel of glow mask
                #endif
                #ifdef USE_SPECIAL_MASK
                    specialZone = specialSample.r;         // Red channel of special mask
                #endif
                                
                half safeIndex = min(colorIndex, 1.0 - (1.0 / _ColorCount + 1e-5));
                half lutIndex = safeIndex * _ColorCount;
                int idxA = (int)floor(lutIndex);

                half4 lutColor;
                half metallic;
                half smoothness;

                // Cache array values to reduce lookups
                half4 colorA = _Colors[idxA];
                half metallicA = _MetallicValues[idxA];
                half smoothnessA = _SmoothnessValues[idxA];
                
                // Compile-time blend mode optimization
                #if defined(BLEND_LINEAR) || defined(BLEND_SMOOTH) || defined(BLEND_SHARP) || defined(BLEND_STEP)
                    int idxB = (int)clamp(idxA + 1, 0, _ColorCount - 1);
                    half4 colorB = _Colors[idxB];
                    half metallicB = _MetallicValues[idxB];
                    half smoothnessB = _SmoothnessValues[idxB];
                    
                    half blend = saturate(frac(lutIndex) * _BlendStrength);
                    
                    half blendFinal;
                    #ifdef BLEND_LINEAR
                        blendFinal = blend;
                    #elif BLEND_SMOOTH
                        blendFinal = smoothstep(0, 1, blend);
                    #elif BLEND_SHARP
                        blendFinal = blend * blend;
                    #elif BLEND_STEP
                        blendFinal = step(half(0.5), blend);
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
                
                // Apply base color
                half4 result = baseColor * lutColor;

                // APPLY WEAR/DAMAGE if enabled
                half wearIntensity = half(0.0);
                #ifdef USE_WEAR_MASK
                    if (wearAmount > half(0.01)) {
                        // Simple wear application - apply wear where mask is present and wear level is active
                        wearIntensity = _CurrentWearLevel * wearAmount;
                        half3 wornColor = result.rgb * _WearDarkness;
                        result.rgb = lerp(result.rgb, wornColor, wearIntensity);
                        
                        // Worn metals lose their shine
                        metallic *= (half(1.0) - wearIntensity * half(0.7));
                        smoothness *= (half(1.0) - wearIntensity * half(0.5));
                    }
                #endif

                // FAKE METALLIC EFFECTS
                // 1. Metallic surfaces are more contrasty and slightly desaturated
                half3 grayscale = Luminance(result.rgb);
                result.rgb = lerp(result.rgb, grayscale, metallic * half(0.2) * _MaterialIntensity);
                
                // 2. Add contrast for metallic materials
                half contrast = lerp(half(1.0), half(1.3), metallic * _MaterialIntensity);
                result.rgb = saturate((result.rgb - half(0.5)) * contrast + half(0.5));
                
                // 3. Fake "shine" based on position - metallic surfaces have hot spots
                // Consolidate wear reduction calculation to avoid redundancy
                half wearReduction = half(1.0) - wearIntensity;
                half2 shinePos = frac(uv * half(3.0));
                half shine = pow(max(half(0.0), half(1.0) - length(shinePos - half(0.5)) * half(2.0)), half(4.0));
                result.rgb += shine * metallic * smoothness * half(0.3) * _MaterialIntensity * wearReduction;
                
                // FAKE SMOOTHNESS EFFECTS - Using pre-computed values for performance
                // 1. Smooth surfaces have tighter, brighter highlights
                half highlightSize = lerp(_HighlightParams.x, _HighlightParams.x * half(2.67), smoothness); // 3.0 to 8.0
                half highlight = pow(saturate(sin(uv.x * highlightSize) * 
                                              sin(uv.y * highlightSize)), 
                                     lerp(half(1.0), _HighlightParams.y, smoothness));
                
                // 2. Combine highlight with metallic for final shine
                result.rgb += highlight * smoothness * metallic * _HighlightParams.z * _MaterialIntensity * wearReduction;
                
                // 3. Smooth non-metallic surfaces (like plastic) get a subtle rim light - using pre-computed center
                half rimLight = pow(half(1.0) - saturate(dot(_RimLightCenter, uv - half(0.5))), half(2.0));
                result.rgb += rimLight * smoothness * (half(1.0) - metallic) * half(0.1) * _MaterialIntensity;

                // APPLY GLOW EFFECTS if enabled - AFTER material effects
                #ifdef USE_GLOW_MASK
                    if (glowStrength > half(0.01)) {
                        // Simple glow application - glow where mask is present and glow level is active
                        half actualGlowIntensity = _CurrentGlowLevel * glowStrength;
                        result.rgb += _GlowColor.rgb * actualGlowIntensity * _GlowIntensity;
                    }
                #endif

                // SPECIAL ZONES if enabled
                #ifdef USE_SPECIAL_MASK
                    if (specialZone > half(0.01)) {
                        if (specialZone > half(0.8)) {
                            // Gem areas - extra sparkle
                            half sparkle = frac(sin(dot(uv * half(100.0), half2(12.9898, 78.233))) * half(43758.5453));
                            result.rgb += sparkle * half(0.5);
                        } else if (specialZone > half(0.5)) {
                            // Rune areas - pulsing glow
                            half pulse = sin(_Time.y * half(3.0)) * half(0.5) + half(0.5);
                            result.rgb += _GlowColor.rgb * pulse * half(0.3);
                        } else if (specialZone > half(0.3)) {
                            // Metal inlay - extra metallic shine
                            result.rgb += shine * half(0.5);
                        }
                    }
                #endif

                return result;
            }
            ENDCG
        }
    }
    Fallback "Transparent/Diffuse"
}