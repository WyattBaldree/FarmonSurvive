Shader "Custom/Underwater" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Depth ("Depth", Range(0,1)) = 0.5
        _Refraction ("Refraction", Range(0,1)) = 0.1
        _FogColor ("Fog Color", Color) = (0.5,0.5,0.5,1)
        _FogDensity ("Fog Density", Range(0,1)) = 0.05
    }

    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Opaque"}

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _Depth;
            float _Refraction;
            float4 _FogColor;
            float _FogDensity;
            sampler2D _CameraDepthTexture;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float4 col = tex2D(_MainTex, i.uv);
                float depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, UNITY_PROJ_COORD(i.vertex)));
                float factor = depth < _Depth ? _Refraction : 1.0;
                col.rgb = lerp(col.rgb, _FogColor.rgb, factor);
                col.a = 1.0 - pow(1.0 - col.a, _FogDensity * depth * 30.0);
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
