Shader"Moonflow/Lensflare"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        
        Tags
        {
            "RenderType" = "Transparent" 
            "IgnoreProjector" = "True" 
            "PreviewType" = "Plane" 
            "PerformanceChecks" = "False" 
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100

        Pass
        {
            Blend One One
            ZWrite Off
            ZTest Off
            
            HLSLPROGRAM
            #pragma multi_compile_instancing
            #pragma multi_compile _ _STEREO_MULTIVIEW_ON _STEREO_INSTANCING_ON
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/core.hlsl"

            struct appdata
            {
               float4 vertex : POSITION;
               float2 uv : TEXCOORD0;
               half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
               float4 vertex : SV_POSITION;
               float2 uv : TEXCOORD0;
               half4 color : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Texture2D _MainTex;
            SamplerState sampler_MainTex;
            float4 _MainTex_ST;

            // Moved to use ComputeShader
            // Texture2D _CameraDepthTexture;
            // SamplerState sampler_CameraDepthTexture;

            half4 _FlareScreenPos;

            v2f vert (appdata v)
            {
                v2f o;
                // ✅ Use per-eye view-projection matrix (Core.hlsl defines this)
            #if defined(USING_STEREO_MATRICES)
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float4x4 stereoVP = UNITY_MATRIX_VP;
                stereoVP = unity_StereoMatrixVP[unity_StereoEyeIndex];
                o.vertex = mul(stereoVP, float4(worldPos, 1.0));
            #else
                o.vertex = TransformObjectToHClip(v.vertex);
            #endif

                o.uv.xy = v.uv;
                o.color = v.color;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
            #if defined(USING_STEREO_MATRICES)
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
            #endif
                // half depthMask = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, _FlareScreenPos.xy).r;
                // half depthTex = LinearEyeDepth(depthMask, _ZBufferParams);
                // half needRender = lerp(saturate(depthTex - _FlareScreenPos.z), 1 - ceil(depthMask), _FlareScreenPos.w);
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy)/* * needRender*/ * i.color;
                return col;
            }
            ENDHLSL
        }
    }
}
