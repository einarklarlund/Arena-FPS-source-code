Shader "Custom/Ordered Dither Surface"{
    //show values to edit in inspector
    Properties{
        [Header(Base Parameters)]
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
        _Specular ("Specular Color", Color) = (1,1,1,1)
        [HDR] _Emission ("Emission", color) = (0 ,0 ,0 , 1)

        [Header(Dither parameters)]
        _DitherPattern ("Dithering Pattern", 2D) = "white" {}
        _Color1 ("Dither Color 1", Color) = (0, 0, 0, 1)
        _Color2 ("Dither Color 2", Color) = (1, 1, 1, 1)

        [Header(Lighting Parameters)]
        _ShadowTint ("Shadow Color", Color) = (0.5, 0.5, 0.5, 1)
        [IntRange]_StepAmount ("Shadow Steps", Range(1, 16)) = 2
        _StepWidth ("Step Size", Range(0, 1)) = 0.25
        _SpecularSize ("Specular Size", Range(0, 1)) = 0.1
        _SpecularFalloff ("Specular Falloff", Range(0, 2)) = 1
    }

    SubShader {
        //the material is completely non-transparent and is rendered at the same time as the other opaque geometry
        Tags{ "RenderType"="Opaque" "Queue"="Geometry"}

        CGPROGRAM

        //the shader is a surface shader, meaning that it will be extended by unity in the background to have fancy lighting and other features
        //our surface shader function is called surf and we use the default PBR lighting model
        #pragma surface surf Stepped fullforwardshadows
        #pragma target 3.0

        //texture and transforms of the texture, color and lighting
        sampler2D _MainTex;
        fixed4 _Color;
        half3 _Emission;
        fixed4 _Specular;

        //The dithering pattern
        sampler2D _DitherPattern;
        float4 _DitherPattern_TexelSize;

        //Dither colors
        float4 _Color1;
        float4 _Color2;

        //input struct which is automatically filled by unity
        struct Input {
            float2 uv_MainTex;
            float4 screenPos;
        };
        
        //toon lighting properties
        float3 _ShadowTint;
        float _StepWidth;
        float _StepAmount;
        float _SpecularSize;
        float _SpecularFalloff;

        struct ToonSurfaceOutput{
            fixed3 Albedo;
            half3 Emission;
            fixed3 Specular;
            fixed Alpha;
            fixed3 Normal;
        };

        //our lighting function. Will be called once per light
        float4 LightingStepped(ToonSurfaceOutput s, float3 lightDir, half3 viewDir, float shadowAttenuation){
            //how much does the normal point towards the light?
            float towardsLight = dot(s.Normal, lightDir);

            //stretch values so each whole value is one step
            towardsLight = towardsLight / _StepWidth;
            //make steps harder
            float lightIntensity = floor(towardsLight);

            // calculate smoothing in first pixels of the steps and add smoothing to step, raising it by one step
            // (that's fine because we used floor previously and we want everything to be the value above the floor value, 
            // for example 0 to 1 should be 1, 1 to 2 should be 2 etc...)
            float change = fwidth(towardsLight);
            float smoothing = smoothstep(0, change, frac(towardsLight));
            lightIntensity = lightIntensity + smoothing;

            // bring the light intensity back into a range where we can use it for color
            // and clamp it so it doesn't do weird stuff below 0 / above one
            lightIntensity = lightIntensity / _StepAmount;
            lightIntensity = saturate(lightIntensity);

        #ifdef USING_DIRECTIONAL_LIGHT
            //for directional lights, get a hard vut in the middle of the shadow attenuation
            float attenuationChange = fwidth(shadowAttenuation) * 0.5;
            float shadow = smoothstep(0.5 - attenuationChange, 0.5 + attenuationChange, shadowAttenuation);
        #else
            //for other light types (point, spot), put the cutoff near black, so the falloff doesn't affect the range
            float attenuationChange = fwidth(shadowAttenuation);
            float shadow = smoothstep(0, attenuationChange, shadowAttenuation);
        #endif
            lightIntensity = lightIntensity * shadow;

            //calculate how much the surface points points towards the reflection direction
            float3 reflectionDirection = reflect(lightDir, s.Normal);
            float towardsReflection = dot(viewDir, -reflectionDirection);

            //make specular highlight all off towards outside of model
            float specularFalloff = dot(viewDir, s.Normal);
            specularFalloff = pow(specularFalloff, _SpecularFalloff);
            towardsReflection = towardsReflection * specularFalloff;

            //make specular intensity with a hard corner
            float specularChange = fwidth(towardsReflection);
            float specularIntensity = smoothstep(1 - _SpecularSize, 1 - _SpecularSize + specularChange, towardsReflection);
            //factor inshadows
            specularIntensity = specularIntensity * shadow;

            float4 color;
            //calculate final color
            color.rgb = s.Albedo * lightIntensity * _LightColor0.rgb;
            color.rgb = lerp(color.rgb, s.Specular * _LightColor0.rgb, saturate(specularIntensity));

            color.a = s.Alpha;
            return color;
        }

        //the surface shader function which sets parameters the lighting function then uses
        void surf (Input i, inout ToonSurfaceOutput o) {
            //read texture and write it to diffuse color
            float4 texColor = tex2D(_MainTex, i.uv_MainTex);
            texColor *= _Color;
            // _DitherPattern_TexelSize.x = 0.25;
            // _DitherPattern_TexelSize.y = 0.25;
            //value from the dither pattern
            float2 screenPos = i.screenPos.xy / i.screenPos.w;
            float2 ditherCoordinate = screenPos * _ScreenParams.xy * _DitherPattern_TexelSize.xy;
            float ditherValue = tex2D(_DitherPattern, ditherCoordinate).r;

            float ditheredValue = step(ditherValue, texColor);
            float4 col = lerp(_Color1, _Color2, ditheredValue);

            o.Albedo = col;

            float3 shadowColor = col.rgb * _ShadowTint;
            o.Emission = _Emission + shadowColor;
            o.Specular = _Specular;
        }
        ENDCG
    }
    FallBack "Standard"
}