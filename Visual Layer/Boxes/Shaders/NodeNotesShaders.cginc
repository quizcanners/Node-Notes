
#include "UnityCG.cginc"
#include "UnityLightingCommon.cginc" 
#include "Lighting.cginc"
#include "AutoLight.cginc"

sampler2D _Global_Noise_Lookup;
float4 _Global_Noise_Lookup_TexelSize;

float _NodeNotes_Time;

sampler2D _Global_Water_Particles_Mask_L;
sampler2D _Global_Water_Particles_Mask_D;
sampler2D _NodeNotes_SpiralMask;

float4 _NodeNotes_MousePosition;


inline float PowerFromClick(float2 screenUV) {
	float2 fromMouse = (screenUV - _NodeNotes_MousePosition.xy);
	fromMouse.x *= _NodeNotes_MousePosition.w;
	return saturate((1 - length(fromMouse) * 6) * _NodeNotes_MousePosition.z);
}

inline float GetCirculingSparkles(float2 uv, float offset, float pressPower) {

	float angle = _NodeNotes_Time * 20 + offset;

	float2 off = (uv - 0.5) / 2;
	float2 rotUV = off;
	float si = sin(angle);
	float co = cos(angle);

	float tx = rotUV.x;
	float ty = rotUV.y;
	rotUV.x = (co * tx) - (si * ty);
	rotUV.y = (si * tx) + (co * ty);

	rotUV += 0.5;

	float rays = tex2D(_NodeNotes_SpiralMask, rotUV).r * (1 - pressPower) + pressPower;



	return rays;

}


inline float DarkBrightGradient(float2 screenUV, float alpha, float pressPower) {

	//float pressPower = 0;

	float t = _NodeNotes_Time;

	screenUV.x *= 1.5;

	float val = t * 15 + screenUV.x * 3 + screenUV.y * 8;

	float portion = saturate((cos(val + pressPower * 8) + 1)*0.5);
	float dePortion = 1 - portion;

	float2 offA = screenUV * 1.3;
	float2 offB = screenUV * 1.1;

	float blur = alpha * 2;

	float darker = tex2Dlod(_Global_Water_Particles_Mask_D, float4(offA*2.5 + float2(t*(0.13), 0), 0, blur)).r;

	float darkerB = tex2Dlod(_Global_Water_Particles_Mask_D, float4(offB * 4 + float2(0, t*0.1), 0, blur)).r;

	float dmix = darker * darkerB;

	darker = darker * portion + darkerB * dePortion + dmix * dmix;

	float brighter = tex2Dlod(_Global_Water_Particles_Mask_L, float4(offA - float2(t*0.07, 0), 0, blur)).r;
	//*portion
	float brighterB = tex2Dlod(_Global_Water_Particles_Mask_L, float4(offB - float2(0, t*0.09), 0, blur)).r;
	//*dePortion;

	float mix = brighter * brighterB;

	brighter = (brighter * portion + brighterB * dePortion) +
		mix * mix * mix
		+
		darker
		;

	//brighter = brighter * uvy + darker * (1- uvy);

	return brighter * brighter * (1 + pressPower * (1.5 + sin(pressPower * 10 + t * 100)))  * alpha;

}
