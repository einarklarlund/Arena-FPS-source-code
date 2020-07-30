Shader "My Shaders/Screenspace Unlit"{
    //values to edit in inspector
    Properties{
        _Color("Color", Color) = (0, 0, 0, 1)
        //declare texture in global scope, set default color to white
        _MainTex ("Texture", 2D) = "white" {}
    }
    
    // ...


    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            //include useful shader functions
            #include "UnityCG.cginc"

            //define vertex and fragment shader
            #pragma vertex vert
            #pragma fragment frag

            //texture
            sampler2D _MainTex;
            //tint of the texture
            fixed4 _Color;
            //scale/translation of maintex. the tiling of the texture will be saved in first 2 parameters and the the offset of the texture will be saved in the last 2 parameters
			float4 _MainTex_ST;

            //the object data that's put into the vertex shader
            struct appdata{
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            //the data that's used to generate fragments and can be read by the fragment shader
            struct v2f{
                float4 position : SV_POSITION;
                float4 screenPosition : TEXCOORD0;
            };

            //the vertex shader
            v2f vert(appdata v){
                v2f o;
                //convert the vertex positions from object space to clip space so they can be rendered
                o.position = UnityObjectToClipPos(v.vertex);
                //get the screen position from the clip space position
                o.screenPosition = ComputeScreenPos(o.position);
                return o;
            }

            //the fragment shader
            fixed4 frag(v2f i) : SV_TARGET{
                //take the first two components of the screen position and divide them by the last one. This counteracts the perspective correction the GPU automatically performs on interpolators
                float2 textureCoordinate = i.screenPosition.xy / i.screenPosition.w;
                //calculate apsect ratio to unstretch texture
                float aspect = _ScreenParams.x / _ScreenParams.y;
                textureCoordinate.x = textureCoordinate.x * aspect;
                textureCoordinate = TRANSFORM_TEX(textureCoordinate, _MainTex);
                fixed4 col = tex2D(_MainTex, textureCoordinate);
                col *= _Color;
                return col;
            }
            ENDCG
        }
    }
}
