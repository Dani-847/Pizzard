Shader "Pizzard/SpriteGaussianBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Range(0, 20)) = 5.0
        _Color ("Tint", Color) = (1,1,1,1)
        _Iterations ("Blur Iterations", Range(1, 4)) = 3
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float _BlurSize;
            float _Iterations;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // 13-tap Gaussian kernel along one axis
            fixed4 blur13(sampler2D tex, float2 uv, float2 dir)
            {
                fixed4 col = fixed4(0,0,0,0);
                // Offsets and weights for a 13-tap 1D Gaussian (sigma ~4)
                col += tex2D(tex, uv + dir * -6.0) * 0.002216;
                col += tex2D(tex, uv + dir * -5.0) * 0.008764;
                col += tex2D(tex, uv + dir * -4.0) * 0.026995;
                col += tex2D(tex, uv + dir * -3.0) * 0.064759;
                col += tex2D(tex, uv + dir * -2.0) * 0.120985;
                col += tex2D(tex, uv + dir * -1.0) * 0.176033;
                col += tex2D(tex, uv)               * 0.199471;
                col += tex2D(tex, uv + dir *  1.0) * 0.176033;
                col += tex2D(tex, uv + dir *  2.0) * 0.120985;
                col += tex2D(tex, uv + dir *  3.0) * 0.064759;
                col += tex2D(tex, uv + dir *  4.0) * 0.026995;
                col += tex2D(tex, uv + dir *  5.0) * 0.008764;
                col += tex2D(tex, uv + dir *  6.0) * 0.002216;
                return col;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                int iters = max(1, (int)_Iterations);

                fixed4 col = fixed4(0,0,0,0);

                // Accumulate multiple passes at different scales for a deep, soft blur
                for (int p = 0; p < 4; p++)
                {
                    if (p >= iters) break;
                    float spread = _BlurSize * (1.0 + p * 0.7);
                    float2 dirH = float2(_MainTex_TexelSize.x * spread, 0);
                    float2 dirV = float2(0, _MainTex_TexelSize.y * spread);

                    // Two-pass separable: horizontal then vertical
                    // Since we can't do true multi-pass in one shader pass,
                    // we blend both axes per iteration
                    col += blur13(_MainTex, uv, dirH) * 0.5;
                    col += blur13(_MainTex, uv, dirV) * 0.5;
                }
                col /= (float)iters;

                return col * _Color;
            }
            ENDCG
        }
    }
}
