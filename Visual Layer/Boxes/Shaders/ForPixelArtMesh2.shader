Shader "Playtime Painter/Pixel Art/ForPixelArtMesh2"{
	Properties{
		_MainTex("_MainTex", 2D) = "white" {}
		[NoScaleOffset]_Bump("_bump", 2D) = "bump" {}
		_Smudge("_smudge", 2D) = "gray" {}
		[NoScaleOffset]_BumpEx("_bumpEx", 2D) = "bump" {}
		_BumpDetail("_bumpDetail", 2D) = "bump" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
	}

	Category{
		Tags{
			"IgnoreProjector" = "True"
			"RenderType" = "Opaque"
			"LightMode" = "ForwardBase"
			"Queue" = "Geometry"
		}

				//Blend SrcAlpha OneMinusSrcAlpha

		SubShader{
			Pass{

				CGPROGRAM
				#include "UnityLightingCommon.cginc" 
				#include "Lighting.cginc"
				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
				#include "Assets/Tools/Playtime Painter/Shaders/quizcanners_cg.cginc"
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fwdbase
				#pragma multi_compile ___ USE_NOISE_TEXTURE

			

				sampler2D _MainTex;
				sampler2D _Bump;
				sampler2D _BumpDetail;
				sampler2D _BumpEx;
				sampler2D _Smudge;
				float4 _MainTex_TexelSize;
				uniform float4 _MainTex_ST;
				float _Glossiness;

				struct v2f {
					float4 pos : SV_POSITION;
					float3 worldPos : TEXCOORD0;
					float3 normal : TEXCOORD1;
					float2 texcoord : TEXCOORD2;
					float3 viewDir: TEXCOORD3;
					float4 hold : TEXCOORD4;
					float2 perfuv : TEXCOORD5;
					SHADOW_COORDS(6)
					float4 wTangent : TEXCOORD7;
					float3 tangentViewDir : TEXCOORD8;

				};

				v2f vert(appdata_full v) {
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					o.normal.xyz = UnityObjectToWorldNormal(v.normal);
					o.pos = UnityObjectToClipPos(v.vertex);
					o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
					o.viewDir.xyz = WorldSpaceViewDir(v.vertex);
			
					o.wTangent.xyz = UnityObjectToWorldDir(v.tangent.xyz);
					o.wTangent.w = v.tangent.w * unity_WorldTransformParams.w;
					TRANSFER_SHADOW(o);

					o.perfuv.xy = (floor(v.texcoord.zw*_MainTex_TexelSize.z) + 0.5)* _MainTex_TexelSize.x;

					o.texcoord.xy = v.texcoord.xy*_MainTex_ST.xy + _MainTex_ST.zw;



					o.tangentViewDir = GetParallax(v.tangent, v.normal, v.vertex);

					float2 up = (v.texcoord.zw)*_MainTex_TexelSize.z;
					float2 bord = up;
					up = floor(up);
					bord = bord - up - 0.5;
					float2 hold = bord * 2;
					hold *= _MainTex_TexelSize.x;
					up = (up + 0.5)* _MainTex_TexelSize.x;

					float4 c = tex2Dlod(_MainTex, float4(up, 0, 0));
					float4 contact = tex2Dlod(_MainTex, float4(up + float2(hold.x, 0), 0, 0));
					float4 contact2 = tex2Dlod(_MainTex, float4(up + float2(0, hold.y), 0, 0));
					float4 contact3 = tex2Dlod(_MainTex, float4(up + float2(hold.x, hold.y), 0, 0));

					hold *= _MainTex_TexelSize.z / 5.5;

					bord = abs(bord);

					float4 difff = abs(contact - c);
					float xsame = saturate((0.1 - (difff.r + difff.g + difff.b + difff.a)) * 165800);
					difff = abs(contact2 - c);
					float ysame = saturate((0.1 - (difff.r + difff.g + difff.b + difff.a)) * 165800);
					difff = abs(contact3 - c);
					float ddiff = saturate((0.05 - (difff.r + difff.g + difff.b + difff.a)) * 165800);

					float diag = saturate((1 - ddiff)*xsame*ysame * 165800);

					o.hold.z = diag;

					o.hold.w = 1 - diag;

					o.hold.x = 1 - saturate(xsame);
					o.hold.y = 1 - saturate(ysame);

					return o;
				}


				float4 frag(v2f i) : COLOR{

				
				
					i.tangentViewDir = normalize(i.tangentViewDir);

					i.tangentViewDir.xy /= (i.tangentViewDir.z + 0.42);

					//return float4 (i.tangentViewDir.xy, 0, 1);


					float3 viewDir = normalize(i.viewDir.xyz);

					float2 off = (i.texcoord.xy - i.perfuv.xy);

					float2 bumpUV = off * _MainTex_TexelSize.zw;

					float4 c = tex2Dlod(_MainTex, float4(i.perfuv.xy, 0, 0));

					float2 border = (abs(float2(bumpUV.x, bumpUV.y)) - 0.4) * 10;
					float bord = max(0, max(border.x*i.hold.x, border.y*i.hold.y)*i.hold.w + i.hold.z*min(border.x, border.y));

					bord *= bord;

					float deBord = 1 - bord;


					bumpUV.x = bumpUV.x*max(i.hold.x, i.hold.z);
					bumpUV.y = bumpUV.y*max(i.hold.y, i.hold.z);

					bumpUV *= 0.98;
					bumpUV += 0.5;

					float2 parallax = i.tangentViewDir.xy;

					bumpUV += parallax * 0.08;


					


					float3 nn = UnpackNormal(tex2Dlod(_Bump, float4(bumpUV, 0, 0)) *(1 - i.hold.z) + tex2Dlod(_BumpEx, float4(bumpUV, 0, 0)) *(i.hold.z));

					float smudge = tex2D(_Smudge, i.texcoord.xy * 8).r;

					float3 nn2 = UnpackNormal(tex2D(_BumpDetail, i.texcoord.xy*((1.5-c.a)*8 + nn.xy
					
						)));
					nn += nn2 * smudge * (1.5-c.a) * 0.3;

					nn = normalize(nn);

					float gloss = _Glossiness * c.a;

					float2 sat = abs(off) * 128 * _MainTex_TexelSize.zw;
					float2 pixuv = i.perfuv.xy + off * min(1, sat*0.03) ;


					float4 light = (tex2Dlod(_MainTex, float4(pixuv
						+ nn.xy*_MainTex_TexelSize.xy*(deBord)
						-parallax * 0.03
						
						, 0, 0)));

			
					//return light;

					float4 col = c;

					float shadow = SHADOW_ATTENUATION(i);

					float ambientBlock = bord;

					float3 worldNormal = i.normal.xyz;

					applyTangent(worldNormal, nn, i.wTangent);

					float dotprod = max(0, dot(worldNormal, viewDir.xyz));
					float fernel = 1.5 - dotprod;

					float smoothness = gloss;

					float deSmoothness = 1 - gloss;

					shadow = saturate((shadow * 2 - ambientBlock));

					float diff = saturate(dot(worldNormal, _WorldSpaceLightPos0.xyz));
					diff = saturate(diff - ambientBlock * 4 * (1 - diff));

					float3 ambientCol = ShadeSH9(float4(worldNormal, 1));

					_LightColor0 *= shadow;

					float deUp = (1 - nn.z);

					light.rgb *= (_LightColor0.rgb * (1 + light.a - diff)
						+ ambientCol.rgb) * deUp;

				


					col.rgb = (
						col.rgb * (light.rgb * deSmoothness + _LightColor0 * diff + ambientCol)
						+ light * smoothness * shadow *  deUp
					
						) * deBord 
						;

					float3 halfDirection = normalize(viewDir.xyz + _WorldSpaceLightPos0.xyz);

					float NdotH = max(0.01, (dot(worldNormal, halfDirection)));// *pow(smoothness + 0.2, 8);

					col.rgb += pow(NdotH, 4096 * smudge * gloss) * gloss * gloss * 8 * _LightColor0 * diff * (1.1- gloss) ;

					float3 bgr = col.gbr + col.brg;
					bgr *= bgr;

					col.rgb += bgr * 0.1;

					#if USE_NOISE_TEXTURE
						float4 noise = tex2Dlod(_Global_Noise_Lookup, float4(i.texcoord.xy * 13.5 + float2(_SinTime.w, _CosTime.w) * 32, 0, 0));
						#ifdef UNITY_COLORSPACE_GAMMA
							col.rgb += (noise.rgb - 0.5)*0.02;
						#else
							col.rgb += (noise.rgb - 0.5)*0.0025;
						#endif
					#endif

					return col;
				}
				ENDCG

			}
			UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
		}
		Fallback "Legacy Shaders/Transparent/VertexLit"
	}
}
