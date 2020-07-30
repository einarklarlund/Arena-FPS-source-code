Shader "My Shaders/Surface Dither Color Set"{
	//show values to edit in inspector
	Properties{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
        _Color1 ("Dither Color 1", Color) = (0, 0, 0, 1)
        _Color2 ("Dither Color 2", Color) = (1, 1, 1, 1)
		_Smoothness ("Smoothness", Range(0, 1)) = 0
		_Metallic ("Metalness", Range(0, 1)) = 0
		[HDR] _Emission ("Emission", color) = (0,0,0)
	}

	SubShader {
		//the material is completely non-transparent and is rendered at the same time as the other opaque geometry
		Tags{ "RenderType"="Opaque" "Queue"="Geometry"}

		CGPROGRAM

		//the shader is a surface shader, meaning that it will be extended by unity in the background to have fancy lighting and other features
		//our surface shader function is called surf and we use our custom lighting model
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

		//texture and tint of color
		sampler2D _MainTex;
		float4 _Color;
		float4 _Color1;
		float4 _Color2;

		//lighting properties
		half _Smoothness;
		half _Metallic;
		half3 _Emission;

		//remapping of distance
		float _MinDistance;
		float _MaxDistance;

		//input struct which is automatically filled by unity
		struct Input {
			float2 uv_MainTex;
			
		};

        float rand_1_05(in float2 uv) {
            float2 noise = (frac(sin(dot(uv ,float2(12.9898,78.233)*2.0)) * 43758.5453));
            return abs(noise.x + noise.y) + 0.1;
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
			float4 texColor = tex2D(_MainTex, i.uv_MainTex);
			// o.Albedo = texColor.rgb * _Color;

			//value from the dither
            float ditherValue = rand_1_05(rand_1_05(i.uv_MainTex) * rand(_Time));
            
            //combine dither pattern with texture value to get final result
            float ditheredValue = step(ditherValue, texColor);
            float4 col = lerp(_Color1, _Color2, ditheredValue);
            o.Albedo = _Color * col;
            o.Albedo = col;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			o.Emission = _Emission;
            o.Alpha = texColor.a;
		}
		ENDCG
	}
	FallBack "Standard"
}