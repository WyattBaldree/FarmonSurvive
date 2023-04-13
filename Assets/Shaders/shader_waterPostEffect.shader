Shader "Custom/WaterPostEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Tint("Tint", Color) = (1, 1, 1, 1)
        _Speed("Speed", float) = 0.1
        _ScaleX("Scale X", float) = 0.1
        _ScaleY("Scale Y", float) = 0.1
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float4 _Tint;
            float _Speed;
            float _ScaleX;
            float _ScaleY;

            fixed4 frag (v2f i) : SV_Target
            {
                //speed = 18
                //scale = 80
                fixed4 col = tex2D(_MainTex, i.uv + float2(0, sin((i.vertex.x + _Time[1] * _Speed) * _ScaleY)) * _ScaleX);
                // just invert the colors
                return col * _Tint;
            }
            ENDCG
        }
    }
}
