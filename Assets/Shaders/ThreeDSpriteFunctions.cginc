#if !defined(ThreeDSpriteFunctions_INCLUDED)
#define ThreeDSpriteFunctions_INCLUDED

half4 _MainTex_TexelSize;
        
fixed4 _OutlineColor;
float _OutlineWidth;
            
float _WhiteOut;

struct Input
{
    float2 uv_MainTex;
    fixed4 color;
};

void surfFunc(Input IN, inout SurfaceOutput o)
{
    fixed4 c = SampleSpriteTexture(IN.uv_MainTex) * IN.color;


    float3 whiteMinusRgb = float3(1, 1, 1) - c.rgb;
                    //increase the color by _WhiteOut
    c.rgb = c.rgb + (whiteMinusRgb * _WhiteOut);

    o.Albedo = c.rgb * c.a;
    o.Alpha = c.a;

                    //#define DIV_SQRT_2 0.70710678118
    float2 directions[4] = { float2(1, 0), float2(0, 1), float2(-1, 0), float2(0, -1) };
                    //float2(DIV_SQRT_2, DIV_SQRT_2), float2(-DIV_SQRT_2, DIV_SQRT_2),
                    //float2(-DIV_SQRT_2, -DIV_SQRT_2), float2(DIV_SQRT_2, -DIV_SQRT_2)};


    float2 sampleDistance = _MainTex_TexelSize.xy * _OutlineWidth;

                    //generate border
    float maxAlpha = 0;
    for (uint index = 0; index < 4; index++)
    {
        float2 sampleUV = IN.uv_MainTex + directions[index] * sampleDistance;
        maxAlpha = max(maxAlpha, tex2D(_MainTex, sampleUV).a);
    }

                    //apply border
    c.rgb = lerp(_OutlineColor.rgb, c.rgb, c.a);
    c.a = max(c.a, maxAlpha);
            
                    // Don't draw anything below a certain alpha
                    // This allows us to hide the transparent portions of sprites
                    // without enabling z-write off and alpha:fade which prevents
                    // the material from interacting (going behind) other Transparent
                    // materials.
    clip(c.a - 0.001);

    o.Albedo = c.rgb * c.a;
    o.Alpha = c.a;
}

void vertFunc(inout appdata_full v, out Input o)
{
    v.vertex = UnityFlipSprite(v.vertex, _Flip);

#if defined(PIXELSNAP_ON)
                v.vertex = UnityPixelSnap (v.vertex);
#endif

    UNITY_INITIALIZE_OUTPUT(Input, o);
    o.color = v.color * _Color * _RendererColor;
}

#endif