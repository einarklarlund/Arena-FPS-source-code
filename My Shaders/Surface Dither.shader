Shader "My Shaders/Surface Dither"{
	//show values to edit in inspector
	Properties{
		_MainTex ("Texture", 2D) = "white" {}
        _Color1 ("Dither Color 1", Color) = (0, 0, 0, 1)
        _Color2 ("Dither Color 2", Color) = (0, 0, 0, 1)
        _UseColor2 ("Use color 2? (set to 0 or 1)", Int) = 0
	}

	SubShader {
		//the material is completely non-transparent and is rendered at the same time as the other opaque geometry
		Tags{ "RenderType"="Opaque" "Queue"="Geometry"}

		CGPROGRAM

		//the shader is a surface shader, meaning that it will be extended by unity in the background to have fancy lighting and other features
		//our surface shader function is called surf and we use our custom lighting model
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

		//texture and dither vars
		sampler2D _MainTex;
		float4 _Color1;
		float4 _Color2;
		float _UseColor2;

		//input struct which is automatically filled by unity
		struct Input {
			float2 uv_MainTex;
		};

		float rand_1_05(in float2 uv)
		{
			float2 noise = (frac(sin(dot(uv ,float2(12.9898,78.233)*2.0)) * 43758.5453));
			return abs(noise.x + noise.y) * 0.5;
		}

		float2 rand_2_10(in float2 uv) {
			float noiseX = (frac(sin(dot(uv, float2(12.9898,78.233) * 2.0)) * 43758.5453));
			float noiseY = sqrt(1 - noiseX * noiseX);
			return float2(noiseX, noiseY);
		}

		float2 rand_2_0004(in float2 uv)
		{
			float noiseX = (frac(sin(dot(uv, float2(12.9898,78.233)      )) * 43758.5453));
			float noiseY = (frac(sin(dot(uv, float2(12.9898,78.233) * 2.0)) * 43758.5453));
			return float2(noiseX, noiseY) * 0.004 + 0.1;
		}

		float noise(float4 val) {
			float v = val;	
			// ensure reasonable range
			v = frac(v) + frac(v*1e4) + frac(v*1e-4);
			// seed
			v += float4(0.12345, 0.6789, 0.314159, 0.271828);
			// more iterations => more random
			v = frac(v*dot(v, v)*123.456);
			v = frac(v*dot(v, v)*123.456);
			return v;
		}

		//the surface shader function which sets parameters the lighting function then uses
		void surf (Input i, inout SurfaceOutputStandard o) {
			//read texture and write it to diffuse color
			float4 texColor = tex2D(_MainTex, i.uv_MainTex);
			// o.Albedo = texColor.rgb * _Color;

			//value from the dither
            // float ditherValue = rand_2_10(i.uv_MainTex.x*_Time);
            // float ditherValue = rand(_Time);
			// float pos = i.uv_MainTex.xy + _Time * 1500 + 50;
			float t = _Time * 13.0;
			float4 ditherIn = float4(i.uv_MainTex.x, i.uv_MainTex.y, t, 0.0);
			float ditherValue = rand_1_05(noise(ditherIn));
            

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
            // o.Albedo = _Color * col;
            o.Albedo = col;
		}
		ENDCG
	}
	FallBack "Standard"
}