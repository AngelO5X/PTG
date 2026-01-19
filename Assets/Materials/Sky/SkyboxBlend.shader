Shader "Skybox/PanoramicBlendedWithFog"
{
    Properties
    {
        _Tint ("Tint Color", Color) = (.5, .5, .5, .5)
        _Gamma ("Gamma", Float) = 1.0
        _Exposure ("Exposure", Range(0, 8)) = 1.0
        _Rotation ("Rotation", Range(0, 360)) = 0
        
        [NoScaleOffset] _MainTex ("Skybox A (Base)", 2D) = "grey" {}
        [NoScaleOffset] _SecTex ("Skybox B (Target)", 2D) = "grey" {}
        _Blend ("Blend Factor", Range(0, 1)) = 0
        
        [Header(Horizon Fog)]
        _FogHeight ("Fog Height", Range(0, 1)) = 0.2
        _FogFill ("Fog Bottom Fill", Range(0, 1)) = 1.0 
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

            sampler2D _MainTex;
            sampler2D _SecTex;
            half4 _Tint;
            half _Exposure;
            half _Rotation;
            half _Blend;
            half _Gamma;
            half _FogHeight;
            half _FogFill;
            
            struct appdata_t
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 texcoord : TEXCOORD0;
            };

            float3 RotateAroundYInDegrees (float3 vertex, float degrees)
            {
                float alpha = degrees * UNITY_PI / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float3(mul(m, vertex.xz), vertex.y).xzy;
            }

            v2f vert (appdata_t v)
            {
                v2f o;
                float3 rotated = RotateAroundYInDegrees(v.vertex.xyz, _Rotation);
                o.vertex = UnityObjectToClipPos(rotated);
                o.texcoord = v.vertex.xyz;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float3 dir = normalize(i.texcoord);
                
                float2 uv = float2(atan2(dir.z, dir.x) / (2 * UNITY_PI) + 0.5, 1.0 - acos(dir.y) / UNITY_PI);
                
                half4 texA = tex2D(_MainTex, uv);
                half4 texB = tex2D(_SecTex, uv);
                
                half3 finalSky = lerp(texA.rgb, texB.rgb, _Blend);
                
                float h = dir.y; 
                float fogFactor = 0;
                
                if (h > 0) {
                    fogFactor = 1.0 - smoothstep(0.0, _FogHeight, h);
                } else {
                    fogFactor = _FogFill;
                }

                finalSky = lerp(finalSky, unity_FogColor.rgb, fogFactor);

                finalSky = finalSky * _Tint.rgb * unity_ColorSpaceDouble.rgb;
                finalSky *= _Exposure;
                finalSky = pow(max(finalSky, 0.0), _Gamma);
                
                return half4(finalSky, 1);
            }
            ENDCG
        }
    }
    Fallback Off
}