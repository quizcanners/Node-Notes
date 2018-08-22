Shader "NodeNotes/UI/Nebula_Button" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_Courners("Rounding Courners", Range(0,0.9)) = 0.5
		_Stretch("Edge Courners", Vector) = (0,0,0,0)
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
	float _Courners;
	//float4 _ClickLight;
	float4 _Stretch;

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

		//float2 off = i.texcoord - 0.5;
		//off *= off;

		i.viewDir.xyz = normalize(i.viewDir.xyz);

		//float alpha = saturate(saturate((1 - (off.x + off.y) * 4))*8 );

		float4 col = _Color; 
		
		float2 uv = i.texcoord.xy - 0.5;
		uv = abs(uv) * 2;
		//_Stretch
		uv -= _Stretch;
		float2 upStretch = 1 - _Stretch;
		uv = max(0, uv) / upStretch;

		//_Courners
		uv = uv - _Courners;
		float flattened = saturate(uv.x*uv.y * 2048);
		float upscale = 1 - _Courners;
		uv = max(0, uv) / upscale;


		uv *= uv;
		float rad = (uv.x + uv.y);
		float trim = saturate((1 - rad) * 20 *(1 - _Courners));
		col.a *= trim;


		//col.a *= alpha;

		return col;

	}
		ENDCG

	}
	}
		Fallback "Legacy Shaders/Transparent/VertexLit"
	}

}

