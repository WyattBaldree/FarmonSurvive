// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Custom/3DSpriteWithOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
        _OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineWidth ("Outline Width", Range(0, 10)) = 1

        ///Used to gradually make the sprite whiter.
        _WhiteOut ("White Out", Range(0.0, 1.0)) = 0
    }

    SubShader
    {
        


            Tags
            {
                "Queue"="Transparent"
                "IgnoreProjector"="True"
                "RenderType"="Transparent"
                "PreviewType"="Plane"
                "CanUseSpriteAtlas"="True"
            }

            Lighting On
            //ZWrite Off
            Blend One OneMinusSrcAlpha

            //Cull the back on the first pass
            Cull Back

            CGPROGRAM
            #pragma surface surf Lambert vertex:vert noinstancing //alpha:fade
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"
            #include "ThreeDSpriteFunctions.cginc"

            void vert (inout appdata_full v, out Input o)
            {
                vertFunc(v, o);
            }

            void surf(Input IN, inout SurfaceOutput o)
            {
                surfFunc (IN, o);
            }
            
            ENDCG

            //Cull the front on the second pass
            Cull Front

            CGPROGRAM
            #pragma surface surf Lambert vertex:vert noinstancing //alpha:fade
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc" 
            #include "ThreeDSpriteFunctions.cginc"

            void vert (inout appdata_full v, out Input o)
            {
                vertFunc(v, o);

                //Flip the normals to fix the lighting on the second pass.
                v.normal = -v.normal;
            }

            void surf(Input IN, inout SurfaceOutput o)
            {
                surfFunc (IN, o);
            }

            ENDCG
        
    }

//Fallback "Transparent/VertexLit"
}
