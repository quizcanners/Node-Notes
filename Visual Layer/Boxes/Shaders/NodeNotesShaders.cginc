
#include "UnityCG.cginc"
#include "UnityLightingCommon.cginc" 
#include "Lighting.cginc"
#include "AutoLight.cginc"



float _NodeNotes_Time;

sampler2D _Global_Water_Particles_Mask_L;
sampler2D _Global_Water_Particles_Mask_D;
sampler2D _NodeNotes_SpiralMask;

float4 _NodeNotes_MousePosition; // w is Screen.Width/Screen.Height
float4 _NodeNotes_MouseDerrived; // x - One Directional Speed

inline float PowerFromClick(float2 screenUV) {
	float2 fromMouse = (screenUV - _NodeNotes_MousePosition.xy);
	fromMouse.x *= _NodeNotes_MousePosition.w;

	float radius = max(0 , 1 - length(fromMouse) * 4);

	radius *= radius;

	return saturate(radius * radius * _NodeNotes_MousePosition.z);
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


inline float DarkBrightGradient(float2 screenUV, float pressPower) {

	


	float t = _NodeNotes_Time;

	float2 clickOff = (screenUV - _NodeNotes_MousePosition.xy);

	float power = max(0, 1 - length(clickOff) * 3);


	screenUV.x *= _NodeNotes_MousePosition.w; // 1.5;


	//return length(clickOff);

	float2 clickOff1 = clickOff * power * _NodeNotes_MouseDerrived.x * 0.3;

	float val = t * 15 + screenUV.x * 3 + screenUV.y * 8;

	float portion = saturate((cos(val + pressPower * 8) + 1)*0.5);
	float dePortion = 1 - portion;

	float2 offA = screenUV * 1.3;
	float2 offB = (screenUV) * 1.1;


	float brighter = tex2Dlod(_Global_Water_Particles_Mask_L, float4(offA - clickOff1 - float2(t*0.07, 0), 0, dePortion * 2)).r;

	float brighterB = tex2Dlod(_Global_Water_Particles_Mask_L, float4(offB - clickOff1 - float2(0, t*0.09), 0, portion * 2)).r;

	float mix = brighter * brighterB;

	float darker = tex2Dlod(_Global_Water_Particles_Mask_D, float4(offA*2.5 - clickOff1 + float2(t*(0.13), 0), 0, dePortion * 2)).r;

	float darkerB = tex2Dlod(_Global_Water_Particles_Mask_D, float4(offB * 4 - clickOff1 + float2(0, t*0.1), 0, portion * 2)).r;

	float dmix = darker * darkerB;

	darker = darker * portion + darkerB * dePortion + dmix * dmix;

	brighter = (brighter * portion + brighterB * dePortion) +
		mix * mix * mix
		+
		darker
		;


	return brighter * (1 + pressPower * (1.5 + sin(pressPower * 10 + _NodeNotes_Time * 100)));

}
