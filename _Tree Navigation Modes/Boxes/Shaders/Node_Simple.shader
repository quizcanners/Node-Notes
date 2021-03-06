﻿Shader "Node Notes/UI/Node_Simple" {
	Properties{
		_MainTex_Current("Main Texture", 2D) = "white" {}
		_Next_MainTex("Transitioning to", 2D) = "white" {}
		_Transition("Transition", Range(0,1)) = 0

		_ProjTexPos("Screen Space Position", Vector) = (0,0,0,0)
		_Color("Color", Color) = (1,1,1,1)
		_TextureFadeIn("ShowTexture", Range(0,1)) = 0
		_Courners("Rounding Courners", Range(0,0.9)) = 0.5
		_Blur("Blur", Range(0,1)) = 0
		_Selected("Selected", Range(0,1)) = 0
		_Stretch("Edge Courners", Vector) = (0,0,0,0)

		[Toggle(_CLAMP)] blabla("Clamp", Float) = 0  
	}

	Category{
		Tags{
			"Queue" = "AlphaTest"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		Cull Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		SubShader{

			Pass{

				CGPROGRAM

				#include "UnityCG.cginc"
					#include "Assets/Node-Notes/_Presentation Systems/NodeNotesShaders.cginc"

				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fwdbase
				#pragma multi_compile_instancing
				#pragma target 3.0
				#pragma multi_compile ____  _CLAMP

				sampler2D _MainTex_Current;
				float4 _MainTex_Current_TexelSize;
				sampler2D _Next_MainTex;
				float4 _Next_MainTex_TexelSize;
				float _Transition;

				float4 _ProjTexPos;
				float4 _Color;

				float _Courners;
				float4 _Stretch;
				float _Blur;
				float _TextureFadeIn;
				float _Selected;
				float4 _ClickLight;

				struct v2f {
					float4 pos : SV_POSITION;
					float3 worldPos : TEXCOORD0;
					float3 normal : TEXCOORD1;
					float2 texcoord : TEXCOORD2;
					float3 viewDir: TEXCOORD4;
					float4 screenPos : TEXCOORD5;
					#if _CLAMP
					float2 mainTexScale : TEXCOORD6;
					#endif
				};

	
				v2f vert(appdata_full v) {
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					o.normal.xyz = UnityObjectToWorldNormal(v.normal);
					o.pos = UnityObjectToClipPos(v.vertex);
					o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
					o.viewDir.xyz = WorldSpaceViewDir(v.vertex);
					o.texcoord = (v.texcoord.xy - 0.5) * 1.2 + 0.5;
					o.screenPos = ComputeScreenPos(o.pos);

					#if _CLAMP
					float relation = (_MainTex_Current_TexelSize.w*(1-_Stretch.y)) / (_MainTex_Current_TexelSize.z*(1 - _Stretch.x))* (1-_Transition)
						+ (_Next_MainTex_TexelSize.w*(1 - _Stretch.y)) / (_Next_MainTex_TexelSize.z*(1 - _Stretch.x))* (_Transition);
					o.mainTexScale.x = min(1, relation);
					o.mainTexScale.y = min(1, 1 / relation);
					#endif

					return o;
				}

				float4 frag(v2f i) : COLOR{

					const float PI2 = 3.14159 * 2;

					float2 screenUV = i.screenPos.xy / i.screenPos.w;

					float clickPower = PowerFromClick(screenUV);

					float circle = GetCirculingSparkles(i.texcoord.xy, 0, clickPower);


					_Courners = max(0, _Courners - clickPower - _Selected*(1 - circle));

					#if _CLAMP
					float2 texUV = ((i.texcoord.xy - 0.5) * i.mainTexScale.xy) + 0.5;
					#else
				

					float4 texelSize = _MainTex_Current_TexelSize * (1 - _Transition)
						+ _Next_MainTex_TexelSize * _Transition;

					float2 inPix = (screenUV - _ProjTexPos.xy)*_ScreenParams.xy;

					float2 texUV = inPix * texelSize.xy *_ProjTexPos.z;
					texUV += 0.5;
					texUV += texelSize.xy*0.5*fmod(texelSize.zw, 2);

					#endif

					float4 col = tex2Dlod(_MainTex_Current, float4(texUV, 0, 0))*(1 - _Transition) + tex2Dlod(_Next_MainTex, float4(texUV, 0, 0))*_Transition;

					i.viewDir.xyz = normalize(i.viewDir.xyz);

					col.rgb = _Color.rgb * (1- _TextureFadeIn) + col.rgb* _TextureFadeIn;
					col.a = _Color.a;

					_Blur = max(_Blur, 1 - col.a);

					float2 uv = i.texcoord.xy - 0.5;


					//  Sparkle prepare

					/*float angle = atan2(-uv.x, -uv.y) + 0.001;
					angle = saturate(max(angle,PI2 - max(0, -angle)- max(0, angle * 999999)) / PI2);

					angle = abs(frac(_Time.x * 8) - angle);
					float above = max(0, angle - 0.5);
					angle = saturate((min(angle, 0.5 - above) - 0.25) * 4);*/

		
					uv = abs(uv) * 2;

					//_Stretch
					uv -= _Stretch;
					float2 upStretch = 1 - _Stretch;
					uv = max(0, uv) / upStretch;

					//_Courners & Trim
					uv = uv - _Courners;
					float flattened = saturate(uv.x*uv.y * 2048);
					float upscale = 1 - _Courners;
					uv = max(0, uv) / upscale;
					uv *= uv;
					float rad = (uv.x + uv.y);
					float trim = (1 - rad) * (10 * (2 - _Blur)) *(1 - _Courners);
					col *= saturate(trim);

					col.a +=saturate(trim + 1)*0.2;

					// Sparkle 2

					float width = max(0, rad - 0.1)*max(0, 1 - rad);

					float4 grad = DarkBrightGradient(screenUV,  clickPower);

			
					//return circle;

					//
					//width = pow(width, 16);

					circle = (circle*_Selected*4 + clickPower) * width * grad;

					return saturate(col + circle*normalize(col)*16);// float4(circle* normalize(col.rgb) * 2, circle));

				}
				ENDCG

			}
		}
		Fallback "Legacy Shaders/Transparent/VertexLit"
	}
}

