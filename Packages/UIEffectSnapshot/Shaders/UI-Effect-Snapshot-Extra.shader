Shader "Hidden/UI Effect Snapshot Extra"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}

        _Scale ("Scale", Range (0.01, 2)) = 1.1

        _VignetteIntensity ("Intensity", Range (0, 1)) = 0.3

        _DistortionIntensity ("Intensity", Range (0, 1)) = 0.5

        _NoiseIntensity ("Intensity", Range (0, 1)) = 0.1

        [PowerSlider(10.0)] _ScanningLineFrequency ("Frequency", Range (100, 5000)) = 1000
        _ScanningLineIntensity ("Intensity", Range (0, 0.5)) = 0.1

        _RgbShiftIntensity ("Intensity", Range (0, 1)) = 0.3
        _RgbShiftOffsetX ("OffsetX", Range (-1, 1)) = 1
        _RgbShiftOffsetY ("OffsetY", Range (-1, 1)) = 1
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma target 2.0

            #pragma shader_feature VIGNETTE
            #pragma shader_feature DISTORTION
            #pragma shader_feature NOISE
            #pragma shader_feature SCANNING_LINE
            #pragma shader_feature RGB_SHIFT

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Scale;
            float _VignetteIntensity;
            float _DistortionIntensity;
            float _ScanningLineFrequency;
            float _ScanningLineIntensity;
            float _NoiseIntensity;
            float _RgbShiftIntensity;
            float _RgbShiftOffsetX;
            float _RgbShiftOffsetY;
            fixed4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                half2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv.xy = v.texcoord.xy;
                o.uv.zw = o.pos.xy;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 pos = i.uv.zw;
                float2 uv = i.uv.xy;

                // ==== Distortion ====
                #if DISTORTION
                    float2 h = pos / 2;
                    float r2 = h.x * h.x + h.y * h.y;
                    float f = 1.0 + r2 * (_DistortionIntensity * sqrt(r2));
                    uv = f / _Scale * h + 0.5;
                    uv.y = 1 - uv.y;
                #endif

                half4 col = tex2D(_MainTex, uv);

                // ==== RGB Shift ====
                #if RGB_SHIFT
                    half2 offset = _RgbShiftIntensity * half2(_RgbShiftOffsetX, _RgbShiftOffsetY) * _MainTex_TexelSize.xy * 10;
                    col.r = tex2D(_MainTex, uv + offset).r;
                    col.b = tex2D(_MainTex, uv - offset).b;
                #endif

                // ==== Noise ====
                #if NOISE
                    float x = i.uv.x * i.uv.y * _Time.x * 1000 + 100;
                    float dx = fmod( fmod( x, 13.0 ) * fmod( x, 123.0 ), 0.01 );
                    half3 noise = col.rgb * ((dx * 100) - 0.5);
                    col.rgb += noise * _NoiseIntensity;
                #endif

                // ==== ScanningLine ====
                #if SCANNING_LINE
                    half2 sc = half2( sin( i.uv.y * _ScanningLineFrequency ), cos( i.uv.xy.y * _ScanningLineFrequency ) );
                    col.rgb += half3( sc.x, sc.y, sc.x ) * _ScanningLineIntensity;
                #endif

                // ==== Vignette ====
                #if VIGNETTE
                    col.rgb -= dot(pos, pos) * _VignetteIntensity;
                #endif

                // Check outside
                fixed inner = step(0, uv.x) * step(uv.x, 1) * step(0, uv.y) * step(uv.y, 1);
                return lerp(fixed4(0, 0, 0, 1), col, inner);
            }
            ENDCG
        }
    }
    CustomEditor "Coffee.UIExtensions.Editors.UIEffectSnapshotExtraShaderGUI"

}
