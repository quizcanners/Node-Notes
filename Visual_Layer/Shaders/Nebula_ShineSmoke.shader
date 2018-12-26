Shader "NodeNotes/Effects/ShineSmoke" {
	Properties{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_SmokyTex("Noise Smoke (RGB)", 2D) = "white" {}
	}

	Category{
		Tags{
			"Queue" = "AlphaTest"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		Cull Off
		ZWrite Off
		Blend SrcAlpha One //MinusSrcAlpha 

		SubShader{
			Pass{

				CGPROGRAM

				#include "UnityCG.cginc"

				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fog
				#pragma multi_compile_fwdbase

				uniform sampler2D _MainTex;
				sampler2D _SmokyTex;
				sampler2D _Nebula_BG;
				float4 _Nebula_Pos;

				float4 l0pos;
				float4 l0col;
				float4 l1pos;
				float4 l1col;
				float4 l2pos;
				float4 l2col;
				float4 l3pos;
				float4 l3col;


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
					o.normal.xyz = UnityObjectToWorldNormal(v.normal);
					o.pos = UnityObjectToClipPos(v.vertex);
					o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
					o.viewDir.xyz = WorldSpaceViewDir(v.vertex);
					o.texcoord = v.texcoord.xy;
					o.screenPos = ComputeScreenPos(o.pos);
					o.color = v.color;

					return o;
				}


				inline void PointLightTransparent(inout float3 scatter, inout float3 directLight, float3 vec, float3 viewDir, float4 lcol, float amb) 
				{

					float len = length(vec);
					vec /= len;

					float lensq = len * len;
		
					float dott = dot(viewDir, vec);

					float power = pow(
						max(0.01, dott), 128*(0.5+ amb));

					float frontLight = max(0, -dott);

					float3 distApprox = lcol.rgb / lensq;

					scatter += (distApprox * (1 + frontLight))*(1+lcol.a);
					directLight += lcol.rgb*power;

				}

				float4 frag(v2f i) : COLOR{

					float2 scp = i.screenPos.xy / i.screenPos.w;

					float4 screenMask = tex2Dlod(_MainTex, float4( scp ,0,0));

					float2 sPos = ((i.worldPos.xz - _Nebula_Pos.xz)+16)/32 ;

					float4 mask = tex2Dlod(_Nebula_BG, float4(sPos,0,0));

					float toClick = mask.r;

					float2 off = i.texcoord - 0.5;
					float2 orOff = off;
					float2 rotUV = off;
					off *= off;

					i.viewDir.xyz = normalize(i.viewDir.xyz);

					float alpha = max(0, (1 - (off.x + off.y) * 4));

					float angle = toClick * 0.1 + _Time.x;// +screenMask.b * 2;
					float si = sin(angle);
					float co = cos(angle);

					float tx = rotUV.x;
					float ty = rotUV.y;
					rotUV.x = (co * tx) - (si * ty);
					rotUV.y = (si * tx) + (co * ty);

					rotUV += 0.5f;

					float bluring =   sin(angle*10 + screenMask.b + i.viewDir.z);

					float4 col = tex2Dlod(_MainTex, float4(rotUV,0, (1 - toClick) * 2 + bluring));

					float swaprb = saturate((bluring+1)*0.25);

					col.rb = col.rb*(1 - swaprb) + col.br *(swaprb);

					angle = -_Time.x;
					si = sin(angle);
					co = cos(angle);

					tx = orOff.x;
					ty = orOff.y;
					float2 rotUV2;
					rotUV2.x = (co * tx) - (si * ty);
					rotUV2.y = (si * tx) + (co * ty);

					rotUV2 += 0.5;

					float4 col2 = tex2Dlod(_MainTex, float4(rotUV2,0, (1 - toClick) * 2 - bluring)).gbra;

					float alp = saturate((col.g - col2.g + (abs((((_Time.x+col.r)*2) % 2)-1) -0.5)*0.3   ) *2);

					col = col *  alp + col2 * (1 - alp);

					float4 smokyCol = tex2D(_SmokyTex, rotUV);

					col = (col * (1 - toClick) + smokyCol * toClick)*
						i.color;

					col.a *= alpha;

					float ambientBlock = pow((1.01 - col.a), col.a*32*(1 + screenMask.r))*512 * col.g;

					float3 scatter = 0;
					float3 directLight = 0;

					PointLightTransparent(scatter, directLight, i.worldPos.xyz - l0pos.xyz,
						i.viewDir.xyz, l0col, ambientBlock);

					PointLightTransparent(scatter, directLight, i.worldPos.xyz - l1pos.xyz,
						i.viewDir.xyz,  l1col, ambientBlock);

					PointLightTransparent(scatter, directLight, i.worldPos.xyz - l2pos.xyz,
						i.viewDir.xyz,  l2col, ambientBlock);

					PointLightTransparent(scatter, directLight, i.worldPos.xyz - l3pos.xyz,
						i.viewDir.xyz, l3col, ambientBlock);

					col.rgb *= (directLight*ambientBlock * 1024 
						+scatter
						)*pow(col.a, 2+ toClick*3);

					col.rgb *= col.a;

					float3 mix = col.gbr + col.brg;
					col.rgb += mix * mix*0.02;

					return col;

				}
				ENDCG
			}
		}
		Fallback "Legacy Shaders/Transparent/VertexLit"
	}
}

