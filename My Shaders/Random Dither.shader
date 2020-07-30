Shader "My Shaders/Random Dither"{
    //values to edit in inspector
    Properties{
        //declare texture in global scope, set default color to white
        _MainTex ("Texture", 2D) = "white" {}
        _Color1 ("Dither Color 1", Color) = (0, 0, 0, 1)
        _Color2 ("Dither Color 2", Color) = (0, 0, 0, 1)
        _UseColor2 ("Use color 2? (set to 0 or 1)", Int) = 0
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
            //scale/translation of maintex. the tiling of the texture will be saved in first 2 parameters and the the offset of the texture will be saved in the last 2 parameters
			float4 _MainTex_ST;
            //dither variables
            float4 _Color1;
            float4 _Color2;
            float _UseColor2;

            //the object data that's put into the vertex shader
            struct appdata{
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            //the data that's used to generate fragments and can be read by the fragment shader
            struct v2f{
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };


            float rand_1_05(in float2 uv) {
                float2 noise = (frac(sin(dot(uv ,float2(12.9898,78.233)*2.0)) * 43758.5453));
                return abs(noise.x + noise.y);
            }
            
            float rand(float seed) {
                float r1 = (seed * 7919 % 3943 + 1)/4500;
                float r2 = (seed * 4993 % 6197 + 1)/6500;
                float r3 = (seed * 5039 % 1093 + 1)/1500;
                return (r1 + r2 + r3) / 3;
            }

            float2 rand_2_0004(in float2 uv)
            {
                float noiseX = (frac(sin(dot(uv, float2(12.9898,78.233)      )) * 43758.5453));
                float noiseY = (frac(sin(dot(uv, float2(12.9898,78.233) * 2.0)) * 43758.5453));
                return float2(noiseX, noiseY) * 0.004 + 0.1;
            }

            //the vertex shader
            v2f vert(appdata v){
                v2f o;
                //convert the vertex positions from object space to clip space so they can be rendered
                o.position = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            //the fragment shader
            fixed4 frag(v2f i) : SV_TARGET{
                //texture value the dithering is based on
                float4 texColor = tex2D(_MainTex, i.uv).r;

                //value from the dither pattern
                float ditherValue = rand_2_0004(i.uv.x*_Time);

                //combine dither pattern with texture value to get final result
                float ditheredValue = step(ditherValue, texColor);
                float4 secondColor;
                if(_UseColor2 == 0) {
                    secondColor = texColor;
                }
                else {
                    secondColor = _Color2;
                }
                float4 col = lerp(_Color1, secondColor, ditheredValue);
                return col;
            }
            ENDCG
        }
    }
}
