Shader "Custom/TransparentShield" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _EdgeColor ("Edge Color", Color) = (0,0,1,1)
        _Glossiness ("Glossiness", Range(0.0, 1.0)) = 0.5
        _Metallic ("Metallic", Range(0.0, 1.0)) = 0.0
        _Emission ("Emission", Range(0.0, 1.0)) = 0.0
        _FresnelMultiplier ("Fresnel Multiplier", Range(0.01, 2.0)) = 1.0
    }

    SubShader {
        Tags {"RenderType"="Transparent"}

        CGPROGRAM
        #pragma surface surf Standard alpha:fade

        sampler2D _MainTex;
        fixed4 _Color;
        fixed4 _EdgeColor;
        float _Glossiness;
        float _Metallic;
        float _Emission;
        float _FresnelMultiplier;

        struct Input {
            float2 uv_MainTex;
            float3 worldNormal;
            float3 viewDir;
            float3 viewPos;
            float4 screenPos;
            INTERNAL_DATA
        };

        void surf (Input IN, inout SurfaceOutputStandard o) {
            //get the dot product between the normal and the view direction
            float fresnel = dot(IN.worldNormal, IN.viewDir) / _FresnelMultiplier;
            //clamp the value between 0 and 1 so we don't get dark artefacts at the backside
            fresnel = saturate(1 - fresnel);

            // Sample alpha from _MainTex texture and use it to clamp the alpha value of the shader
            fixed4 mainTexColor = tex2D(_MainTex, IN.uv_MainTex);
            float alphaClamp = mainTexColor.a;

            // Create a gradient that fades from the edge color to the main color
            fixed4 col = lerp(_Color, _EdgeColor, fresnel);

            // Set the albedo color and transparency
            o.Albedo = col.rgb;
            o.Alpha = col.a * alphaClamp;

            o.Emission = _Emission + fresnel;

            // Set the metallic and glossiness properties
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}