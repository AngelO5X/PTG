Shader "Custom/Water"
{
    Properties
    {
        _Color ("Water Color", Color) = (0.1, 0.4, 0.6, 0.8)
        _WaveStrength ("Wave Strength", Float) = 0.15
        _WaveSpeed ("Wave Speed", Float) = 1
        _WaveScale ("Wave Scale", Float) = 0.8
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        CGPROGRAM
        #pragma surface surf Standard alpha

        float4 _Color;
        float _WaveStrength;
        float _WaveSpeed;
        float _WaveScale;

        struct Input
        {
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float wave =
                sin(IN.worldPos.x * _WaveScale + _Time.y * _WaveSpeed) +
                cos(IN.worldPos.z * _WaveScale + _Time.y * _WaveSpeed);

            wave *= _WaveStrength;

            o.Albedo = _Color.rgb;
            o.Alpha = _Color.a;
            o.Emission = wave * 0.1;
            o.Smoothness = 0.9;
        }
        ENDCG
    }
}
