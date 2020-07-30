Shader "My Shaders/Screenspace Surface" {
	Properties {
        _Color ("Tint", Color) = (0, 0, 0, 1)
        _MainTex ("Texture", 2D) = "white" {}
        _Smoothness ("Smoothness", Range(0, 1)) = 0
        _Metallic ("Metalness", Range(0, 1)) = 0
        [HDR] _Emission ("Emission", Color) = (0,0,0,1)
	}
	SubShader {
		Tags{ "RenderType"="Opaque" "Queue"="Geometry"}

		CGPROGRAM

		sampler2D _MainTex;
        //uniform variables
        float4 _MainTex_ST;
		fixed4 _Color;
        half _Smoothness;
        half _Metallic;
        half3 _Emission;

        //declare the kind of shader and the methods used
		#pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        //holds all of the information that we need to set the color of our surface
        struct Input {
            //delcaring screenPos to the surface input struct will make unity generate code that fills it with the correct data
            float4 screenPos;
        };

        //we use a surface function instead of a fragment function
        void surf (Input i, inout SurfaceOutputStandard o) {
            //take the first two components of the screen position and divide them by the last one. This counteracts the perspective correction the GPU automatically performs on interpolators
            float2 textureCoordinate = i.screenPos.xy / i.screenPos.w;
            //calculate apsect ratio to unstretch texture
            float aspect = _ScreenParams.x / _ScreenParams.y;
            textureCoordinate.x = textureCoordinate.x * aspect;
            textureCoordinate = TRANSFORM_TEX(textureCoordinate, _MainTex);
            
            fixed4 col = tex2D(_MainTex, textureCoordinate);
            col *= _Color;
            o.Albedo = col.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Emission = _Emission;
        }

		ENDCG
	}
	FallBack "Standard"
}