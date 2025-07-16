Shader "Custom/CirclePulse"
{
    Properties
    {
        _Radius ("Radius", Range(0, 1)) = 1
        _BorderWidth ("Border Width", Range(0, 0.01)) = 0.002
        _Color ("Color", Color) = (1,1,1,1)
        _Color2 ("Color2", Color) = (1,1,1,0.2)
        _PulseTime ("Pulse Time", Float) = 5
        _Speed ("Speed", Float) = 1
        _TrailLength ("Trail Length", Range(0, 0.5)) = 0.05
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            float _Radius;
            float _BorderWidth;
            float _TrailLength;
            float _PulseTime;
            float _Speed;
            float4 _Color;
            float4 _Color2;
            
            float _GameSeconds;
             
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float get_wave_progress(float time, float speed)
            {
                return lerp(-0.5, 1.0, speed > 0 ? time : 1.0 - time);
            }

            float calculate_trail_alpha(float behind_wave, float trail_length, float time)
            {
                return (1.0 - behind_wave / trail_length) * (1.0 - time);
            }
            
            float4 frag (v2f i) : SV_Target
            {
                float2 centered_uv = i.uv - 0.5;
                float distance = length(centered_uv);

                float radius = lerp(0, 0.5, _Radius);
                float inner = radius - _BorderWidth;
                float outer = radius + _BorderWidth;
                
                // Handle frozen state early
                if (_Speed == 0.0)
                {
                    if (distance < inner || distance > outer)
                        return float4(0,0,0,0);
                    return _Color2;
                }
                
                // Calculate time and wave position
                float time = fmod(_GameSeconds / _PulseTime, 1.0);
                float wave_progress =  _Speed > 0 ? time : 1.0 - time;
                
                float wave_inner = inner * wave_progress;
                float wave_outer = outer * wave_progress;

                float current_wave_radius = radius * wave_progress;
                float scaled_trail_length = lerp(_TrailLength, _TrailLength * (1-wave_progress), wave_progress);
                
                // Static ring (always visible)
                bool in_static_ring = distance >= inner && distance <= outer;
                
                // Dynamic trail logic
                float behind_wave = _Speed > 0 ? current_wave_radius - distance : distance - current_wave_radius;
                bool in_trail = behind_wave > 0.0 && behind_wave < scaled_trail_length && distance < radius;
    
                
                //float behind_wave = _Speed > 0 ? wave_outer - distance : distance - wave_outer;
                //bool in_trail = behind_wave > 0.0 && behind_wave < _TrailLength && distance < inner;
                
                if (in_static_ring)
                {
                    return _Color2;
                }
                else if (in_trail)
                {
                    float trail_alpha = (1.0 - behind_wave / scaled_trail_length) * (1.0 - time);
                    return float4(_Color.rgb, trail_alpha * _Color.a);
                }
                return float4(0,0,0,0);
            }
            ENDCG
        }
    }
}