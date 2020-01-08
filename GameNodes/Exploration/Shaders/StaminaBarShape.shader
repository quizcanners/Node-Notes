﻿Shader "Node Notes/StaminaBarShape"
{
	Properties{
		  _MainTex("Albedo (RGB)", 2D) = "white" {}
	}

	SubShader{

		Tags{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		ColorMask RGB
		Cull Off
		ZWrite Off
		ZTest Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass{

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			#pragma target 3.0

			struct v2f {
				float4 pos : 		SV_POSITION;
				float3 worldPos : 	TEXCOORD0;
				float3 normal : 	TEXCOORD1;
				float2 texcoord : 	TEXCOORD2;
				float3 viewDir: 	TEXCOORD3;
				float4 screenPos : 	TEXCOORD4;
				float4 color: 		COLOR;
			};


			uniform float4 _MainTex_ST;
			sampler2D _MainTex;

			float _NodeNotes_StaminaCurve;
			float _NodeNotesStaminaPortion;
			float _NodeNotesStaminaPortion_Prev;

			v2f vert(appdata_full v) {
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);

				o.normal.xyz = UnityObjectToWorldNormal(v.normal);
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.viewDir.xyz = WorldSpaceViewDir(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.screenPos = ComputeScreenPos(o.pos);
				o.color = v.color;
				return o;
			}

			float4 frag(v2f o) : COLOR{

				float2 off = abs(o.texcoord.xy - 0.5)*2;

			


				float2 duv = o.screenPos.xy / o.screenPos.w;

				float4 col = o.color * tex2D(_MainTex, o.texcoord.xy);

				off.x = 1 - off.x;

				float actual = pow(off.x, 1 + _NodeNotes_StaminaCurve);

				float center = length(float2(1-actual ,off.y)) * 2;

				center = pow(center, 8);
				center = saturate(center);

				float thickness = pow(actual, 2)*0.8 + 0.2*off.x;

			//_NodeNotesStaminaPortion_Prev

				float used = saturate((_NodeNotesStaminaPortion - o.texcoord.x) * 100);

				float previous = max(0, saturate((_NodeNotesStaminaPortion_Prev - o.texcoord.x) * 100) - used * 100);

				float shape = max(0, thickness - off.y);

				col.a = shape * max(used, previous);

				float splits =  max(0, 0.17 * actual - 1 + abs(((actual * 10) % 1) - 0.5) * 2);

				col.rgb = float3(1, 1, 0) * previous + col.rgb * (1 - previous);

				float outline = (max(0, 0.05 - col.a)) * 200;

				col.a = saturate((outline * (1-previous) + previous*64 ) * col.a + splits * 8 * shape) *center;


				

				return col;
			}
			ENDCG
		}
		}
			Fallback "Legacy Shaders/Transparent/VertexLit"

				//CustomEditor "CircleDrawerGUI"
}