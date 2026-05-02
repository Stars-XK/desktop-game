Shader "Hidden/PhotoModeFilter"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Tint ("Tint", Color) = (1,1,1,1)
        _Saturation ("Saturation", Float) = 1
        _Contrast ("Contrast", Float) = 1
        _VigStrength ("Vignette Strength", Float) = 0.2
        _VigSmooth ("Vignette Smooth", Float) = 0.4
        _BlurStrength ("Blur Strength", Float) = 0.25
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _Tint;
            float _Saturation;
            float _Contrast;
            float _VigStrength;
            float _VigSmooth;
            float _BlurStrength;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float3 ApplySaturation(float3 c, float s)
            {
                float l = dot(c, float3(0.299, 0.587, 0.114));
                return lerp(float3(l, l, l), c, s);
            }

            float3 ApplyContrast(float3 c, float k)
            {
                return (c - 0.5) * k + 0.5;
            }

            float Vignette(float2 uv, float strength, float smoothness)
            {
                float2 p = uv * 2.0 - 1.0;
                float r = dot(p, p);
                float v = smoothstep(1.0 - smoothness, 1.0, r);
                return 1.0 - v * strength;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 px = _MainTex_TexelSize.xy * _BlurStrength;
                float3 c0 = tex2D(_MainTex, i.uv).rgb;
                float3 c1 = tex2D(_MainTex, i.uv + float2(px.x, 0)).rgb;
                float3 c2 = tex2D(_MainTex, i.uv + float2(-px.x, 0)).rgb;
                float3 c3 = tex2D(_MainTex, i.uv + float2(0, px.y)).rgb;
                float3 c4 = tex2D(_MainTex, i.uv + float2(0, -px.y)).rgb;
                float3 blur = (c0 * 0.40 + (c1 + c2 + c3 + c4) * 0.15);

                float3 col = blur * _Tint.rgb;
                col = ApplySaturation(col, _Saturation);
                col = ApplyContrast(col, _Contrast);

                float vig = Vignette(i.uv, _VigStrength, _VigSmooth);
                col *= vig;

                return float4(saturate(col), 1.0);
            }
            ENDCG
        }
    }
}

