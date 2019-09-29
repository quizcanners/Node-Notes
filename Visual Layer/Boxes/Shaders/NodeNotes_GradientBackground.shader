Shader "Playtime Painter/Effects/GradientBackground" {
	Properties{
	}
	
	Category{
		Tags{
			"Queue" = "Background"
			"IgnoreProjector" = "True"
			"RenderType" = "Opaque"
		}

		ColorMask RGB
		Cull Off
		ZWrite Off
		ZTest Off

		SubShader{
			Pass{

				CGPROGRAM
				#include "NodeNotesShaders.cginc"
				#include "UnityCG.cginc"
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fwdbase
				#pragma multi_compile ___ USE_NOISE_TEXTURE

				struct v2f {
					float4 pos : SV_POSITION;
					float3 worldPos : TEXCOORD0;
					float3 normal : TEXCOORD1;
					float2 texcoord : TEXCOORD2;
					float3 viewDir: TEXCOORD4;
					float4 screenPos : TEXCOORD5;
					float4 color: COLOR;
				};

				v2f vert(appdata_full v) {
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					o.normal.xyz = UnityObjectToWorldNormal(v.normal);
					o.pos = UnityObjectToClipPos(v.vertex);
					o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
					o.viewDir.xyz = WorldSpaceViewDir(v.vertex);
					o.texcoord = v.texcoord.xy;
					o.screenPos = ComputeScreenPos(o.pos);
					o.color = v.color;
					return o;
				}

				float4 _BG_GRAD_COL_1;
				float4 _BG_GRAD_COL_2;
				float4 _BG_CENTER_COL;

				float4 frag(v2f i) : COLOR{

					float2 screenUV = i.screenPos.xy / i.screenPos.w;

					float clickPower = PowerFromClick(screenUV);

					float grad = DarkBrightGradient(screenUV, 1, clickPower);

					float2 off = screenUV - 0.5;
					off.x *= _ScreenParams.x / _ScreenParams.y;
					off *= off;

					i.viewDir.xyz = normalize(i.viewDir.xyz);

					#ifdef UNITY_COLORSPACE_GAMMA
					_BG_GRAD_COL_1.rgb *= _BG_GRAD_COL_1.rgb;
					_BG_GRAD_COL_2.rgb *= _BG_GRAD_COL_2.rgb;
					_BG_CENTER_COL.rgb *= _BG_CENTER_COL.rgb;
					#endif

					float4 col = _BG_GRAD_COL_1 * screenUV.y + _BG_GRAD_COL_2 * (1 - screenUV.y);

					//_ScreenParams.xy

					float center = saturate(1 - (off.x + off.y));

					center = center*_BG_CENTER_COL.a;
	
					col.rgb = col.rgb * (1 - center) + _BG_CENTER_COL.rgb*center;

					#ifdef UNITY_COLORSPACE_GAMMA
					col.rgb = sqrt(o.color.rgb);
					#endif

					col.rgb += grad * _BG_CENTER_COL.rgb * clickPower * _BG_CENTER_COL.a;

					#if USE_NOISE_TEXTURE
						float4 noise = tex2Dlod(_Global_Noise_Lookup, float4(i.texcoord.xy * 13.5 + float2(_SinTime.w, _CosTime.w) * 32, 0, 0));
						#ifdef UNITY_COLORSPACE_GAMMA
							col.rgb += (noise.rgb - 0.5)*0.02;
						#else
							col.rgb += (noise.rgb - 0.5)*0.0025;
						#endif
					#endif

					return saturate(col);
				}
				ENDCG

			}
		}
		Fallback "Legacy Shaders/Transparent/VertexLit"
	}
}
