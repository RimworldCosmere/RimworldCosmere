Shader "Lightweave/Blur"
{
    Properties
    {
        _BlurSize ("Blur Size (pixels)", Range(0, 64)) = 12
        _Color ("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" }
        ZWrite Off
        Cull Off
        Lighting Off

        GrabPass
        {
            "_LW_GrabH"
        }

        Pass
        {
            Blend One Zero
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _LW_GrabH;
            float4 _LW_GrabH_TexelSize;
            float _BlurSize;

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 vertex : SV_POSITION; float4 grabPos : TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                return o;
            }

            float hash21(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.grabPos.xy / i.grabPos.w;
                float jitter = 0.9 + 0.2 * hash21(i.vertex.xy);
                float2 step = float2(_LW_GrabH_TexelSize.x * (_BlurSize / 3.0) * jitter, 0);

                fixed4 sum  = tex2D(_LW_GrabH, uv) * 0.1597;
                sum += (tex2D(_LW_GrabH, uv + step) + tex2D(_LW_GrabH, uv - step)) * 0.1474;
                sum += (tex2D(_LW_GrabH, uv + step * 2.0) + tex2D(_LW_GrabH, uv - step * 2.0)) * 0.1159;
                sum += (tex2D(_LW_GrabH, uv + step * 3.0) + tex2D(_LW_GrabH, uv - step * 3.0)) * 0.0777;
                sum += (tex2D(_LW_GrabH, uv + step * 4.0) + tex2D(_LW_GrabH, uv - step * 4.0)) * 0.0444;
                sum += (tex2D(_LW_GrabH, uv + step * 5.0) + tex2D(_LW_GrabH, uv - step * 5.0)) * 0.0216;
                sum += (tex2D(_LW_GrabH, uv + step * 6.0) + tex2D(_LW_GrabH, uv - step * 6.0)) * 0.0090;
                sum += (tex2D(_LW_GrabH, uv + step * 7.0) + tex2D(_LW_GrabH, uv - step * 7.0)) * 0.0032;
                sum += (tex2D(_LW_GrabH, uv + step * 8.0) + tex2D(_LW_GrabH, uv - step * 8.0)) * 0.0010;
                sum.a = 1.0;
                return sum;
            }
            ENDCG
        }

        GrabPass
        {
            "_LW_GrabV"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _LW_GrabV;
            float4 _LW_GrabV_TexelSize;
            float _BlurSize;
            float4 _Color;

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 vertex : SV_POSITION; float4 grabPos : TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                return o;
            }

            float hash21(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.grabPos.xy / i.grabPos.w;
                float jitter = 0.9 + 0.2 * hash21(i.vertex.yx);
                float2 step = float2(0, _LW_GrabV_TexelSize.y * (_BlurSize / 3.0) * jitter);

                fixed4 sum  = tex2D(_LW_GrabV, uv) * 0.1597;
                sum += (tex2D(_LW_GrabV, uv + step) + tex2D(_LW_GrabV, uv - step)) * 0.1474;
                sum += (tex2D(_LW_GrabV, uv + step * 2.0) + tex2D(_LW_GrabV, uv - step * 2.0)) * 0.1159;
                sum += (tex2D(_LW_GrabV, uv + step * 3.0) + tex2D(_LW_GrabV, uv - step * 3.0)) * 0.0777;
                sum += (tex2D(_LW_GrabV, uv + step * 4.0) + tex2D(_LW_GrabV, uv - step * 4.0)) * 0.0444;
                sum += (tex2D(_LW_GrabV, uv + step * 5.0) + tex2D(_LW_GrabV, uv - step * 5.0)) * 0.0216;
                sum += (tex2D(_LW_GrabV, uv + step * 6.0) + tex2D(_LW_GrabV, uv - step * 6.0)) * 0.0090;
                sum += (tex2D(_LW_GrabV, uv + step * 7.0) + tex2D(_LW_GrabV, uv - step * 7.0)) * 0.0032;
                sum += (tex2D(_LW_GrabV, uv + step * 8.0) + tex2D(_LW_GrabV, uv - step * 8.0)) * 0.0010;

                return sum * _Color;
            }
            ENDCG
        }
    }
}
