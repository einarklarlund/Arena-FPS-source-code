Shader "My Shaders/Random Dither Color Set"{
    //values to edit in inspector
    Properties{
        _Color("Color", Color) = (0, 0, 0, 1)
        //declare texture in global scope, set default color to white
        _MainTex ("Texture", 2D) = "white" {}
        _DitherPattern ("Dithering Pattern", 2D) = "white" {}
        _Color1 ("Dither Color 1", Color) = (0, 0, 0, 1)
        _Color2 ("Dither Color 2", Color) = (1, 1, 1, 1)
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
            //dither variables
            sampler2D _DitherPattern;
            float4 _Color1;
            float4 _Color2;

            //the object data that's put into the vertex shader
            struct appdata{
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            //the data that's used to generate fragments and can be read by the fragment shader
            struct v2f{
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPosition : TEXCOORD1;
            };


            float rand_1_05(in float2 uv) {
                float2 noise = (frac(sin(dot(uv ,float2(12.9898,78.233)*2.0)) * 43758.5453));
                return abs(noise.x + noise.y);
            }
            
            float rand(float seed) {
                float x = seed * 7919 % 353;
                return x+1/354;
            }

            //the vertex shader
            v2f vert(appdata v){
                v2f o;
                //convert the vertex positions from object space to clip space so they can be rendered
                o.position = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPosition = ComputeScreenPos(o.position);
                return o;
            }

            //the fragment shader
            fixed4 frag(v2f i) : SV_TARGET{
                //texture value the dithering is based on
                float4 texColor = tex2D(_MainTex, i.uv).r;

                //value from the dither pattern
                float ditherValue = rand_1_05(i.uv * rand(_Time));

                //combine dither value with texture value to get final result
                float ditheredValue = step(ditherValue, texColor);
                float4 col = lerp(_Color1, _Color2, ditheredValue);
                return col;
            }
            ENDCG
        }
    }
}
