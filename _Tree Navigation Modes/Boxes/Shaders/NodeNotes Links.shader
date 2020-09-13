Shader "Node Notes/Links"
{
	Properties{
		[PerRendererData]_MainTex("Albedo (RGB)", 2D) = "white" {}
	}
		Category{
			Tags{
				"Queue" = "Geometry"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"

			}

			ColorMask RGB
			Cull Off
			ZWrite Off
			ZTest Off
			Blend One One // OneMinusSrcAlpha //One OneMinusSrcAlpha//

		SubShader{
			Pass{

				CGPROGRAM

			
					#include "Assets/Node-Notes/_Presentation Systems/NodeNotesShaders.cginc"

				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_instancing


				struct v2f {
					float4 pos : SV_POSITION;
					float2 texcoord : TEXCOORD0;
					float4 screenPos :	TEXCOORD4;
					float4 color: COLOR;
				};



				v2f vert(appdata_full v) {
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					o.pos =				UnityObjectToClipPos(v.vertex);
					o.texcoord.xy =		v.texcoord.xy;
					o.color =			v.color;

					o.screenPos =		ComputeScreenPos(o.pos);

					return o;
				}


				float4 frag(v2f o) : COLOR{
					
					float2 sUV = o.screenPos.xy / o.screenPos.w;

					float clickPower = PowerFromClick(sUV);

					float off = o.texcoord.y - 0.5;

					float a = 1 - abs(off)*2;

					a *= a;

					float4 col = o.color;

					col.a *= a * (0.1 + o.texcoord.x);

					float grad = DarkBrightGradient(sUV, clickPower);

					col.rgb *= grad * (saturate(3-(col.r + col.g + col.b))) * min(col.a, 1); //asd0asdasd saturate(grad.rgb)*col.rgb;

					col.rgb *= col.rgb * 4;

				

					col.a = 1;

					//col.rgb = max(o.color.rgb, col.rgb);

					return col;
				}
				ENDCG
			}
		}
		Fallback "Legacy Shaders/Transparent/VertexLit"
	}
}
