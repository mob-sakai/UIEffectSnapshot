Shader "Hidden/UIEffectSnapshot"
{
	Properties
	{
		[PerRendererData] _MainTex("Main Texture", 2D) = "white" {}
	}

	SubShader
	{
		ZTest Always
		Cull Off
		ZWrite Off
		Fog{ Mode off }

		Pass
		{
			Name "Effect-Base"

		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#pragma multi_compile __ GRAYSCALE SEPIA NEGA PIXEL
			#pragma multi_compile __ ADD SUBTRACT FILL

			#include "UnityCG.cginc"
			#include "UI-Effect.cginc"

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			half4 _EffectFactor;
			fixed4 _ColorFactor;

			v2f_img vert(appdata_img v)
			{
				v2f_img o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				#if UNITY_UV_STARTS_AT_TOP
				o.uv.y = 1 - o.uv.y;
				#endif
				return o;
			}

			fixed4 frag(v2f_img IN) : SV_Target
			{
				half effectFactor = _EffectFactor.x;
				fixed4 colorFactor = _ColorFactor;

				#if PIXEL
				half2 pixelScale = max(2, (1 - effectFactor) * _MainTex_TexelSize.zw);
				IN.uv = round(IN.uv * pixelScale) / pixelScale;
				#endif

				half4 color = tex2D(_MainTex, IN.uv);

				#if defined (UI_TONE)
				color = ApplyToneEffect(color, effectFactor);
				#endif

				color = ApplyColorEffect(color, colorFactor);

				color.a = 1;
				return color;
			}
		ENDCG
		}


		Pass
		{
			Name "Effect-Blur"

		CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_blur
			#pragma target 2.0

			#pragma multi_compile __ FASTBLUR MEDIUMBLUR DETAILBLUR

			#include "UnityCG.cginc"
			#include "UI-Effect.cginc"

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			half4 _EffectFactor;

			fixed4 frag_blur(v2f_img IN) : SV_Target
			{
				half2 blurFactor = _EffectFactor.xy;
				half4 color = Tex2DBlurring1D(_MainTex, IN.uv, blurFactor * _MainTex_TexelSize.xy);
				color.a = 1;
				return color;
			}
		ENDCG
		}
	}
}
