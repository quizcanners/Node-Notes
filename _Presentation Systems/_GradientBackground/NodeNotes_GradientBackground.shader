Shader "Node Notes/Effects/GradientBackground" {
	Properties{
	}
	
	Category{
		Tags{
			"Queue" = "Background"
			"RenderType" = "Opaque"
		}

		ColorMask RGB
		Cull Off
		ZWrite Off
		ZTest Off
		Blend One Zero//SrcAlpha OneMinusSrcAlpha

		SubShader{
			Pass{

				CGPROGRAM
				#include "Assets/NodeNotes/_Presentation Systems/NodeNotesShaders.cginc"
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fwdbase
				#pragma multi_compile __ RT_MOTION_TRACING

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

				uniform float4 _BG_GRAD_COL_1;
				uniform float4 _BG_GRAD_COL_2;
				uniform float4 _BG_CENTER_COL;
				uniform sampler2D _Global_Noise_Lookup;
				uniform float4 _Global_Noise_Lookup_TexelSize;
				uniform sampler2D _RayTracing_TargetBuffer;
				uniform float4 _RayTracing_TargetBuffer_ScreenFillAspect;
				uniform float _RayTraceTransparency;


	

				float4 frag(v2f i) : COLOR{

					float2 screenUV = i.screenPos.xy / i.screenPos.w;

					float clickPower = PowerFromClick(screenUV);

					float grad = DarkBrightGradient(screenUV, clickPower);

					float2 off = screenUV - 0.5;
					off.x *= _NodeNotes_MousePosition.w; // Same as _ScreenParams.x / _ScreenParams.y;
					off *= off;

					i.viewDir.xyz = normalize(i.viewDir.xyz);

					
					#ifdef UNITY_COLORSPACE_GAMMA
					_BG_GRAD_COL_1.rgb *= _BG_GRAD_COL_1.rgb;
					_BG_GRAD_COL_2.rgb *= _BG_GRAD_COL_2.rgb;
					_BG_CENTER_COL.rgb *= _BG_CENTER_COL.rgb;
					#endif
					

					float clickEffect = grad * clickPower;

					float up = saturate((screenUV.y - 0.5) * (1 - clickEffect*0.5) + 0.5);

					float4 col = _BG_GRAD_COL_1 * up + _BG_GRAD_COL_2 * (1 - up);

					float center = smoothstep(2.5 , 0 ,(off.x + off.y) + clickEffect);

#if RT_MOTION_TRACING
					float2 pixOff = _RayTracing_TargetBuffer_ScreenFillAspect.zw * 1.5;

					float4 rayTrace = 0;
					float4 blur;
					float same;

					#define R(kernel) blur = tex2Dlod( _RayTracing_TargetBuffer, float4(screenUV + kernel* pixOff  ,0,0)); rayTrace.rgb = max(rayTrace.rgb, blur.rgb * 0.55); //same = 1 - min(1, abs(blur.a - col.a)*0.01); rayTrace.rgb = max(rayTrace.rgb, blur.rgb * same * 0.55)

					R(float2(-1, 0));
					R(float2(1, 0));
					R(float2(0, -1));
					R(float2(0, 1));
					R(float2(1, 1));
					R(float2(-1, -1));
					R(float2(1, -1));
					R(float2(-1, 1)); 
#else
					float4 rayTrace = tex2Dlod(_RayTracing_TargetBuffer, float4(screenUV, 0, 0));
#endif

					float rt = 1 * center;
					col = rayTrace * rt +col * (1 - rt);

					//center *= center*_BG_CENTER_COL.a;
	
					//col.rgb = col.rgb * (1 - center) + _BG_CENTER_COL.rgb*center;

					
					#ifdef UNITY_COLORSPACE_GAMMA
					col.rgb = sqrt(col.rgb);
					#endif
					
					
					float4 noise = tex2Dlod(_Global_Noise_Lookup, float4(i.texcoord.xy * 13.5 + float2(_SinTime.w, _CosTime.w) * 32, 0, 0));
					#ifdef UNITY_COLORSPACE_GAMMA
					col.rgb += col.rgb*(noise.rgb - 0.5)*0.06;
					#else
					col.rgb += col.rgb*(noise.rgb - 0.5)*0.2;
					#endif
				
					return saturate(col);
				}
				ENDCG

			}
		}
		Fallback "Legacy Shaders/Transparent/VertexLit"
	}
}
