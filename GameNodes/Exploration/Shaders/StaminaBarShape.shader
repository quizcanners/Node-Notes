Shader "Node Notes/StaminaBarShape"
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

				float thickness = pow(off.x, 8)*0.8 + 0.2*off.x;

				col.a = max(0, thickness - off.y)* saturate((col.a - o.texcoord.x*0.99)*100);

				float splits =  max(0, 0.1 * thickness - 1 + abs(((thickness * 10) % 1) - 0.5) * 2) * 10;

				col.a = ( max(0, 0.05 - col.a) + splits) * col.a * 400;

				return col;
			}
			ENDCG
		}
		}
			Fallback "Legacy Shaders/Transparent/VertexLit"

				//CustomEditor "CircleDrawerGUI"
}