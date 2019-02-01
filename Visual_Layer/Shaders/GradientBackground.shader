Shader "Playtime Painter/Effects/GradientBackground" {
	Properties{
		_MainTex("Noise Texture (RGB)", 2D) = "white" {}
	}
	
	Category{
		Tags{
			"Queue" = "Background"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		ColorMask RGB
		Cull Off
		ZWrite Off
		ZTest Off
		Blend SrcAlpha One //MinusSrcAlpha

		SubShader{
			Pass{

				CGPROGRAM

				#include "UnityCG.cginc"

				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fwdbase
				#pragma multi_compile_instancing
				#pragma target 3.0

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
				sampler2D _MainTex;

				float4 frag(v2f i) : COLOR{

					float2 off = i.texcoord - 0.5;
					off.y *= _ScreenParams.y / _ScreenParams.x;
					off *= off;

					i.viewDir.xyz = normalize(i.viewDir.xyz);
					float2 duv = i.screenPos.xy / i.screenPos.w;

					float4 noise = tex2Dlod(_MainTex,float4(duv*5,0,0));

					float noisy = (noise.r - 0.5) * 0.2;

					duv.y *= (1 + noisy);

					float4 col = _BG_GRAD_COL_1 * duv.y + _BG_GRAD_COL_2 * (1 - duv.y);

					//_ScreenParams.xy

					float center = saturate(1 - (off.x + off.y) + +noisy);

					center = pow(center, 4 )*_BG_CENTER_COL.a;
	
					col.rgb = col.rgb * (1 - center) + _BG_CENTER_COL.rgb*center;

					float3 mix = col.gbr + col.brg;
					mix *= mix;
					col.rgb += mix * 0.05;

					return col;
				}
				ENDCG

			}
		}
		Fallback "Legacy Shaders/Transparent/VertexLit"
	}
}
