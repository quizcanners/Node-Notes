
#include "UnityCG.cginc"
#include "UnityLightingCommon.cginc" 
#include "Lighting.cginc"
#include "AutoLight.cginc"

sampler2D _Global_Noise_Lookup;
float4 _Global_Noise_Lookup_TexelSize;

float _NodeNotes_Time;

sampler2D _Global_Water_Particles_Mask_L;
sampler2D _Global_Water_Particles_Mask_D;


inline float4 DarkBrightGradient(float2 screenUV, float alpha) {
	 
	float t = _NodeNotes_Time;

	screenUV *= _ScreenParams.xy * 0.001;

	float val = t * 15 + screenUV.x * 3 + screenUV.y * 8;

	float portion = saturate((cos(val) + 1)*0.5);
	float dePortion = 1 - portion;

	float2 offA = screenUV * 1.3;
	float2 offB = screenUV * 1.1;

	float blur = max(0, alpha) * 4 + 0.001;

	alpha = saturate(alpha);

	float deAlpha = 1 - alpha;

	float offset = alpha * 0.1;

	float4 darker = tex2Dlod(_Global_Water_Particles_Mask_D, float4(offA*4 + offset + float2(t*(0.13), 0), 0, blur));

	float4 darkerB = tex2Dlod(_Global_Water_Particles_Mask_D, float4(offB*4 - offset + float2(0, t*0.1), 0, blur));

	float dmix = darker * darkerB;

	float4 brighter = tex2Dlod(_Global_Water_Particles_Mask_L, float4(offA - float2(t*0.07, 0), 0, blur));
	//*portion
	float4 brighterB = tex2Dlod(_Global_Water_Particles_Mask_L, float4(offB - float2(0, t*0.09), 0, blur));
	//*dePortion;

	float mix = brighter * brighterB;

	brighter = ((brighter * portion + brighterB * dePortion)*deAlpha
		
		+ (darker * portion + darkerB * dePortion))*alpha
		+ 
		mix * mix  + dmix * dmix + 
		(mix*dmix)*(pow(blur,10))

		;

	//brighter = brighter * uvy + darker * (1- uvy);

	return brighter;

}

