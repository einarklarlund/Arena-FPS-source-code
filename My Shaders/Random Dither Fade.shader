Shader "My Shaders/Random Dither Fade"{
    //show values to edit in inspector
    Properties{
        _MainTex ("Texture", 2D) = "white" {}
        _DitherPattern ("Dithering Pattern", 2D) = "white" {}
        _MinDistance ("Minimum Fade Distance", Float) = 0
        _MaxDistance ("Maximum Fade Distance", Float) = 1
        _Color1 ("Dither Color 1", Color) = (0, 0, 0, 1)
        _Color2 ("Dither Color 2", Color) = (1, 1, 1, 1)
    }

    SubShader {
        //the material is completely non-transparent and is rendered at the same time as the other opaque geometry
        Tags{ "RenderType"="Opaque" "Queue"="Geometry"}

        CGPROGRAM

        //the shader is a surface shader, meaning that it will be extended by unity in the background to have fancy lighting and other features
        //our surface shader function is called surf and we use the default PBR lighting model
        #pragma surface surf Standard
        #pragma target 3.0

        //texture and transforms of the texture
        sampler2D _MainTex;
		float4 _Color1;
		float4 _Color2;


        //The dithering pattern
        sampler2D _DitherPattern;
        float4 _DitherPattern_TexelSize;

        //remapping of distance
        float _MinDistance;
        float _MaxDistance;

        //input struct which is automatically filled by unity
        struct Input {
            float2 uv_MainTex;
            float4 screenPos;
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

        //the surface shader function which sets parameters the lighting function then uses
        void surf (Input i, inout SurfaceOutputStandard o) {
            //read texture and write it to diffuse color
            float3 texColor = tex2D(_MainTex, i.uv_MainTex);
            o.Albedo = texColor.rgb;

            //get relative distance from the camera
            float relDistance = i.screenPos.w;
            relDistance = relDistance - _MinDistance;
            relDistance = relDistance / (_MaxDistance - _MinDistance);

            //value from the dither pattern
            // float ditherValue = rand_1_05(rand_1_05(i.uv_MainTex) * rand(_Time)) - relDistance;
            float ditherValue =  relDistance;
            //combine dither pattern with texture value to get final result
            float ditheredValue = step(ditherValue, texColor);
            float4 col = lerp(_Color1, _Color2, ditheredValue);
            o.Albedo = col;
        }
        ENDCG
    }
    FallBack "Standard"
}
            // fixed4 frag(v2f i) : SV_TARGET{
            //     //texture value the dithering is based on
            //     float4 texColor = tex2D(_MainTex, i.uv).r;

            //     //get relative distance from the camera
            //     float relDistance = i.screenPosition.w;
            //     relDistance = relDistance - _MinDistance;
            //     relDistance = relDistance / (_MaxDistance - _MinDistance);

            //     //value from the dither pattern
            //     float ditherValue = rand_1_05(rand_1_05(i.uv) * rand(_Time)) + relDistance;

            //     // if(relDistance > _MaxDistance)  {
            //     //     ditherValue = 1;
            //     // }
            //     // else {
            //     //     ditherValue = (ditherValue + relDistance/_MaxDistance)/(ditherValue + 1);
            //     // }

            //     //combine dither pattern with texture value to get final result
            //     float ditheredValue = step(ditherValue, texColor);
            //     float4 col = lerp(_Color1, _Color2, ditheredValue);
            //     return col;
            // }
