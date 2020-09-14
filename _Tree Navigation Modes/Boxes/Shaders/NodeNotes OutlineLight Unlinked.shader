﻿Shader "Node Notes/UI/Rounded Unlinked Outline Light"
{
	Properties{
		[PerRendererData]_MainTex("Albedo (RGB)", 2D) = "black" {}
		_Edges("Sharpness", Range(0.02,2)) = 0.5
		_Thickness("Thinnesss", Range(0.02,0.3)) = 1
		[Toggle(TRIMMED)] trimmed("Trimmed Corners", Float) = 0
		[Toggle(FADE)] faded("Fade", Float) = 0
		_FadeEdge("Fade Sharpness", Range(0,200)) = 10
	}
	Category{
		Tags{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PixelPerfectUI" = "FadePosition"
			"SpriteRole" = "Hide"
			"PerEdgeData" = "Unlinked"
		}

		ColorMask RGB
		Cull Off
		ZWrite Off
		ZTest Off
		Blend SrcAlpha OneMinusSrcAlpha

		SubShader{
			Pass{

				CGPROGRAM

			
					#include "Assets/Node-Notes/_Presentation Systems/NodeNotesShaders.cginc"

				#pragma vertex vert
				#pragma fragment frag

				#pragma multi_compile_instancing
				#pragma shader_feature __ TRIMMED
				#pragma shader_feature __ FADE

				struct v2f {
					float4 pos : SV_POSITION;
					float4 texcoord : TEXCOORD0;
					float4 projPos : TEXCOORD1;
					float4 precompute : TEXCOORD2;
					float2 offUV : TEXCOORD3;
					float4 screenPos :	TEXCOORD4;
					#if FADE
					float4 fade : TEXCOORD5;
					
					#endif
					float4 color: COLOR;
				};

				float _FadeEdge;
				float _Edges;
				float _Thickness;

				v2f vert(appdata_full v) {
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					o.pos =				UnityObjectToClipPos(v.vertex);
					o.texcoord.xy =		v.texcoord.xy;
					o.color =			v.color;

					o.texcoord.zw =		v.texcoord1.xy;
					o.texcoord.w *= 0.99; // v.texcoord1.xy;
					o.texcoord.z =		0;
					o.projPos.xy =		v.normal.xy;
					o.projPos.zw =		max(0, float2(v.texcoord1.x, -v.texcoord1.x));
					o.screenPos =		ComputeScreenPos(o.pos);

					#if FADE
						
						o.fade = float4(v.texcoord2.xy, v.texcoord3.xy);
					#endif

					#if TRIMMED
						o.texcoord.w *= 0.9f;
					#endif

					o.precompute.w =	1 / (1.0001 - o.texcoord.w);
					o.precompute.xy =	1 / (1.0001 - o.projPos.zw);
					o.precompute.z =	(1 + _Edges * 32);

					o.offUV.xy =		(o.texcoord.xy - 0.5) * 2;

					return o;
				}


				float4 frag(v2f o) : COLOR{
					
					float dx = abs(ddx(o.texcoord.x));
					float dy = abs(ddy(o.texcoord.y));
					float mip = (dx + dy) * 200;

					_Edges /= 1 + mip * mip; //LOD

					float4 _ProjTexPos = o.projPos;
					float _Courners = o.texcoord.w;
					float deCourners = 1 - _Courners;
					float something = o.precompute.w;
					float2 uv = abs(o.offUV);
				
					float _Blur = (1 - o.color.a);
					uv = max(0, uv - _ProjTexPos.zw) * o.precompute.xy;

					float2 forFade = uv;

					uv = max(0, uv - _Courners) * something;

					#if TRIMMED

						float dist = (uv.x + uv.y);

						dist = dist * (deCourners * 0.7) + deCourners * 0.25 + _Courners*0.9;
					
					#else
						float dist = dot(uv, uv);
						forFade *= forFade;
					#endif

					float exterior = 15;

					float fade = max(forFade.x, forFade.y);

					float a =  1 - max(fade, dist);

			
					a = max(0, a * _Thickness);

					float uvy = saturate(a * 8 *(1 + _Edges));

					float outside = saturate((1 - uvy) * 2);
						
					float alpha = o.color.a;

					alpha *= min(1, outside *
						min(a * _Edges  * (1 - _Blur)*exterior, 1)//*(2 - _Edges)
						*(3 - uvy));

					float2 sUV = o.screenPos.xy / o.screenPos.w;

					float clickPower = PowerFromClick(sUV);

					alpha = saturate(alpha*1.5);

					float grad = DarkBrightGradient(sUV, clickPower);

					#if FADE

						float edge = 1 -
							saturate((sUV.x - o.fade.x) * _FadeEdge)
							* saturate((sUV.y - o.fade.y) * _FadeEdge)
							* saturate((o.fade.z - sUV.x) * _FadeEdge)
							* saturate((o.fade.w - sUV.y) * _FadeEdge)
							;

						edge *= edge;

						alpha *= 1 - edge;

					#endif

						float4 col = o.color;
						col.a = alpha;

						col.rgb *= grad * 128 * alpha * alpha;


					return col;
				}
				ENDCG
			}
		}
		Fallback "Legacy Shaders/Transparent/VertexLit"
	}
}
