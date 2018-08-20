Shader "NodeBook/CircleShader" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
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
	#pragma multi_compile_instancing
	#pragma target 3.0

		float4 _Color;
	float4 l0pos;
	float4 l0col;
	float4 l1pos;
	float4 l1col;
	float4 l2pos;
	float4 l2col;
	float4 l3pos;
	float4 l3col;
	float4 _ClickLight;

	struct v2f {
		float4 pos : SV_POSITION;
		float3 worldPos : TEXCOORD0;
		float3 normal : TEXCOORD1;
		float2 texcoord : TEXCOORD2;
		float3 viewDir: TEXCOORD4;
		float4 screenPos : TEXCOORD5;
		//float4 color: COLOR;
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
		//o.color = v.color;

		return o;
	}


	inline void PointLightTransparent(inout float3 directLight,
		float3 vec, float3 viewDir, float4 lcol, float alpha)
	{

		float len = length(vec);
		vec /= len;

		float dott = abs(dot(viewDir, vec));

		float power = pow(dott, 8 * (1 + alpha));

		directLight += lcol.rgb*power;

	}


	float4 frag(v2f i) : COLOR{

		float2 off = i.texcoord - 0.5;
		off *= off;

		i.viewDir.xyz = normalize(i.viewDir.xyz);

	//	float2 duv = (i.screenPos.xy / i.screenPos.w)*float2(1,2);
		float alpha = saturate(saturate((1 - (off.x + off.y) * 4)));

		//float4 col = i.color;

		float4 col = _Color; //.rgb = 1;

		col.a *= alpha;

		//float3 scatter = 0;
		//float3 directLight = 0;

		// Point Lights

		/*PointLightTransparent(directLight, i.worldPos.xyz - l0pos.xyz,
			i.viewDir.xyz, l0col, alpha);

		PointLightTransparent(directLight, i.worldPos.xyz - l1pos.xyz,
			i.viewDir.xyz,  l1col, alpha);

		PointLightTransparent(directLight, i.worldPos.xyz - l2pos.xyz,
			i.viewDir.xyz,  l2col, alpha);

		PointLightTransparent(directLight, i.worldPos.xyz - l3pos.xyz,
			i.viewDir.xyz, l3col.w, alpha);*/


		//col.rgb *= (directLight);//*pow(col.a, 4);

	

		//float3 mix = col.gbr + col.brg;
		//col.rgb += mix * mix*0.02;

		return col;

	}
		ENDCG

	}
	}
		Fallback "Legacy Shaders/Transparent/VertexLit"
	}

}

