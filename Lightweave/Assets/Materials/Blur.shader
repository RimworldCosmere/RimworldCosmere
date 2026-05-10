Shader "Lightweave/Blur"
{
    Properties
    {
        _BlurSize ("Blur Size (pixels)", Range(0, 16)) = 4
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

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.grabPos.xy / i.grabPos.w;
                float2 step = float2(_LW_GrabH_TexelSize.x * _BlurSize, 0);

                fixed4 sum  = tex2D(_LW_GrabH, uv)              * 0.227027;
                sum += tex2D(_LW_GrabH, uv + step * 1.0)        * 0.1945946;
                sum += tex2D(_LW_GrabH, uv - step * 1.0)        * 0.1945946;
                sum += tex2D(_LW_GrabH, uv + step * 2.0)        * 0.1216216;
                sum += tex2D(_LW_GrabH, uv - step * 2.0)        * 0.1216216;
                sum += tex2D(_LW_GrabH, uv + step * 3.0)        * 0.054054;
                sum += tex2D(_LW_GrabH, uv - step * 3.0)        * 0.054054;
                sum += tex2D(_LW_GrabH, uv + step * 4.0)        * 0.016216;
                sum += tex2D(_LW_GrabH, uv - step * 4.0)        * 0.016216;
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

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.grabPos.xy / i.grabPos.w;
                float2 step = float2(0, _LW_GrabV_TexelSize.y * _BlurSize);

                fixed4 sum  = tex2D(_LW_GrabV, uv)              * 0.227027;
                sum += tex2D(_LW_GrabV, uv + step * 1.0)        * 0.1945946;
                sum += tex2D(_LW_GrabV, uv - step * 1.0)        * 0.1945946;
                sum += tex2D(_LW_GrabV, uv + step * 2.0)        * 0.1216216;
                sum += tex2D(_LW_GrabV, uv - step * 2.0)        * 0.1216216;
                sum += tex2D(_LW_GrabV, uv + step * 3.0)        * 0.054054;
                sum += tex2D(_LW_GrabV, uv - step * 3.0)        * 0.054054;
                sum += tex2D(_LW_GrabV, uv + step * 4.0)        * 0.016216;
                sum += tex2D(_LW_GrabV, uv - step * 4.0)        * 0.016216;

                return sum * _Color;
            }
            ENDCG
        }
    }
}
