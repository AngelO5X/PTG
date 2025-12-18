Shader "Custom/WaterWithFoam"
{
    Properties
    {
        _Color ("Water Color", Color) = (0.1,0.4,0.6,0.5)
        _WaveStrength ("Wave Strength", Float) = 0.15
        _WaveSpeed ("Wave Speed", Float) = 1
        _WaveScale ("Wave Scale", Float) = 0.8

        _WaterHeight ("Water Height", Float) = 1.2
        _DepthFade ("Depth Fade", Float) = 0.05

        _FoamTex ("Foam Texture", 2D) = "white" {}
        _FoamTiling ("Foam Tiling", Float) = 0.5
        _FoamStrength ("Foam Strength", Float) = 0.7

        _FoamMask ("Foam Mask", Float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        CGPROGRAM
        #pragma surface surf Standard alpha

        sampler2D _FoamTex;

        float4 _Color;
        float _WaveStrength;
        float _WaveSpeed;
        float _WaveScale;

        float _WaterHeight;
        float _DepthFade;

        float _FoamTiling;
        float _FoamStrength;

        float _FoamMask;

        struct Input
        {
            float3 worldPos;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // fale
            float wave = sin(IN.worldPos.x * _WaveScale + _Time.y * _WaveSpeed) +
                         cos(IN.worldPos.z * _WaveScale + _Time.y * _WaveSpeed);
            wave *= _WaveStrength;

            // alfa zale¿na od g³êbokoœci
            float depth = _WaterHeight - IN.worldPos.y;
            float alpha = saturate(_Color.a - depth * _DepthFade);

            // pianka tylko przy powierzchni i wg maski
            float2 foamUV = IN.worldPos.xz * _FoamTiling + wave * 0.5;
            float foamSample = tex2D(_FoamTex, foamUV).r;
            float foam = foamSample * saturate(1.0 - depth / 0.5) * _FoamStrength * saturate(_FoamMask);

            // kolor koñcowy
            float3 finalColor = lerp(_Color.rgb, float3(1,1,1), foam);

            o.Albedo = finalColor;
            o.Alpha = alpha;
            o.Emission = wave * 0.1;
            o.Smoothness = 0.9;
        }
        ENDCG
    }
}
