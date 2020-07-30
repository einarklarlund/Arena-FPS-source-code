Shader "Custom/Laser Shader" {
Properties {
    [Header(Base parameters)]
    _TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
    _MainTex ("Particle Texture", 2D) = "white" {}
    _InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0

    [Header(Dither parameters)]
    _DitherPattern ("Dithering Pattern", 2D) = "white" {}
    _Color1 ("Dither Color 1", Color) = (0, 0, 0, 1)
    _Color2 ("Dither Color 2", Color) = (1, 1, 1, 1)
}

Category {
    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
    Blend SrcAlpha OneMinusSrcAlpha
    ColorMask RGB
    Cull Off Lighting Off ZWrite Off

    SubShader {
        Pass {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_particles
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _TintColor;

            //Dither colors
            float4 _Color1;
            float4 _Color2;

            //The dithering pattern
            sampler2D _DitherPattern;
            float4 _DitherPattern_TexelSize;

            struct appdata_t {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float4 texcoords : TEXCOORD0;
                float texcoordBlend : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float2 texcoord2 : TEXCOORD1;
                fixed blend : TEXCOORD2;
                UNITY_FOG_COORDS(3)
                float4 projPos : TEXCOORD4;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _MainTex_ST;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.projPos = ComputeScreenPos (o.vertex);
                COMPUTE_EYEDEPTH(o.projPos.z);
                o.color = v.color * _TintColor;
                o.texcoord = TRANSFORM_TEX(v.texcoords.xy,_MainTex);
                o.texcoord2 = TRANSFORM_TEX(v.texcoords.zw,_MainTex);
                o.blend = v.texcoordBlend;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            float _InvFade;

            fixed4 frag (v2f i) : SV_Target
            {
                #ifdef SOFTPARTICLES_ON
                float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
                float partZ = i.projPos.z;
                float fade = saturate (_InvFade * (sceneZ-partZ));
                i.color.a *= fade;
                #endif

                fixed4 colA = tex2D(_MainTex, i.texcoord);
                fixed4 colB = tex2D(_MainTex, i.texcoord2);
                fixed4 baseCol = 2.0f * i.color * lerp(colA, colB, i.blend);
                baseCol.a = saturate(baseCol.a); // alpha should not have double-brightness applied to it, but we can't fix that legacy behavior without breaking everyone's effects, so instead clamp the output to get sensible HDR behavior (case 967476)

                UNITY_APPLY_FOG(i.fogCoord, baseCol);

                float2 screenPos = i.projPos.xy / i.projPos.w;
                float2 ditherCoordinate = screenPos * _ScreenParams.xy * _DitherPattern_TexelSize.xy;
                float ditherValue = tex2D(_DitherPattern, ditherCoordinate).r;

                float ditheredValue = step(ditherValue, baseCol);
                float4 col = lerp(_Color1, _Color2, ditheredValue);

                return col;
            }
            ENDCG
        }
    }
}
}
