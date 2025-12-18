Shader "Custom/TerrainBlend"
{
    Properties
    {
        _SandTex ("Sand", 2D) = "white" {}
        _GrassTex ("Grass", 2D) = "white" {}
        _RockTex ("Rock", 2D) = "white" {}
        _ShoreTex ("Shore", 2D) = "white" {}

        _SandHeight ("Sand Height", Float) = 2
        _GrassHeight ("Grass Height", Float) = 6
        _BlendStrength ("Blend Strength", Float) = 2

        _WaterHeight ("Water Height", Float) = 1.2
        _ShoreWidth ("Shore Width", Float) = 0.8
        _ShoreTiling ("Shore Tiling", Float) = 0.08
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 300

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _SandTex;
        sampler2D _GrassTex;
        sampler2D _RockTex;
        sampler2D _ShoreTex;

        float _SandHeight;
        float _GrassHeight;
        float _BlendStrength;

        float _WaterHeight;
        float _ShoreWidth;
        float _ShoreTiling;

        struct Input
        {
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float height = IN.worldPos.y;

            float sandW = saturate(1 - smoothstep(_SandHeight, _SandHeight + _BlendStrength, height));
            float grassW = saturate(
                smoothstep(_SandHeight, _GrassHeight, height) *
                (1 - smoothstep(_GrassHeight, _GrassHeight + _BlendStrength, height))
            );
            float rockW = saturate(smoothstep(_GrassHeight, _GrassHeight + _BlendStrength, height));

            float shoreW = saturate(1 - abs(height - _WaterHeight) / _ShoreWidth);

            float2 uv = IN.worldPos.xz * 0.05;
            float2 shoreUV = IN.worldPos.xz * _ShoreTiling;

            float3 sand = tex2D(_SandTex, uv).rgb;
            float3 grass = tex2D(_GrassTex, uv).rgb;
            float3 rock = tex2D(_RockTex, uv).rgb;
            float3 shore = tex2D(_ShoreTex, shoreUV).rgb;

            float3 baseColor =
                sand * sandW +
                grass * grassW +
                rock * rockW;

            o.Albedo = lerp(baseColor, shore, shoreW);
            o.Smoothness = 0.35;
        }
        ENDCG
    }
}
