Shader "Custom/StylizedSkybox"
{
    Properties
    {
        [Header(Colors)]
        _ColorTop ("Top Color", Color) = (0.2, 0.4, 1, 1)
        _ColorHorizon ("Horizon Color", Color) = (0.8, 0.9, 1, 1)
        _ColorBottom ("Bottom Color", Color) = (0.1, 0.1, 0.3, 1)
        
        [Header(Blending)]
        _Exponent ("Horizon Smoothness", Range(0.1, 5.0)) = 1.0
        _HorizonLevel ("Horizon Level", Range(-1.0, 1.0)) = 0.0
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 texcoord : TEXCOORD0;
            };

            fixed4 _ColorTop;
            fixed4 _ColorHorizon;
            fixed4 _ColorBottom;
            float _Exponent;
            float _HorizonLevel;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.vertex.xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 viewDir = normalize(i.texcoord);
                float y = viewDir.y - _HorizonLevel;

                // Mieszanie kolorów (Lerp)
                float blend = abs(y);
                blend = pow(blend, _Exponent); // Wyg³adzanie horyzontu

                fixed4 col;

                if (y > 0)
                {
                    // Góra nieba (Horyzont -> Góra)
                    col = lerp(_ColorHorizon, _ColorTop, blend);
                }
                else
                {
                    // Dó³ nieba (Horyzont -> Ziemia) - TO NAPRAWIA CZARN¥ DZIURÊ!
                    col = lerp(_ColorHorizon, _ColorBottom, blend);
                }

                return col;
            }
            ENDCG
        }
    }
}