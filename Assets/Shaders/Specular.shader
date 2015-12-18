Shader "OniOni/Specular"
{
	Properties
	{
		_H("Hue",Range(-0.5,0.5)) = 0
		_S("Saturation",Range(-1,1)) = 0
		_B("Color",Range(-1,1)) = 0
		_ShadowBias("Shadowv Bias", Range(-1,1)) = 0
		_ShadowStrength("Shadow Strength", Range(0,1)) = 1
		_ShadowBrightness("Shadow Brightness", Range(0,1)) = 1
		[MaterialToggle]_ShadowMaps("Use Shadow Maps", Float) = 0
		_SpecularStrength("Specular Strength", Range(0,1)) = 0
		_Smoothness("Smoothness", Range(1,100)) = 1
		_SpecularBias("Specular Bias", Range(0.1,1)) = 0.1
	}
	SubShader
	{
		Tags {	"RenderType"="Opaque" }
		LOD 200

		Pass
		{
			Tags{
				"LightMode" = "ForwardBase"
			}

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			#pragma multi_compile_fwdbase
			#include "AutoLight.cginc"

			#include "OniOniCG.cginc"

			float _H;
			float _S;
			float _B;
			float _ShadowBias;
			float _ShadowStrength;
			float _ShadowBrightness;
			float _ShadowMaps;
			float _SpecularStrength;
			float _Smoothness;
			float _SpecularBias;

			float4 _LightColor0;
			
			v2f vert (a2v IN)
			{
				v2f OUT;

				OUT.pos = mul(UNITY_MATRIX_MVP, IN.pos);
				OUT.normal = IN.normal;
				OUT.color = IN.color;
				OUT.uvs = IN.uvs;

				OUT.position = mul(_Object2World, IN.pos).xyz;

				TRANSFER_VERTEX_TO_FRAGMENT(OUT);

				return OUT;
			}
			
			float4 frag (v2f IN, float id : INSTANCEID) : SV_Target
			{
				HSBColor baseColor = RGB2HSB(IN.color);
				baseColor.h += _H;
				baseColor.s += _S;
				baseColor.b += _B;
				HSBColor complementaryColor = complementary(baseColor);
				HSBColor specularColor = lighten(baseColor, _SpecularStrength);

				float3 N = normalize(UnityObjectToWorldNormal(IN.normal));
				float3 E = -normalize(UnityWorldSpaceViewDir(IN.position));
				float3 L = normalize(UnityWorldSpaceLightDir(IN.position));
				float attenuation = LIGHT_ATTENUATION(IN);

				float4 diffuse = lerp(HSB2RGB(baseColor), HSB2RGB(complementaryColor) * _ShadowBrightness, _ShadowStrength);
				float4 specular = lerp(HSB2RGB(baseColor), HSB2RGB(specularColor), step(_SpecularBias, pow(max(0, dot(reflect(L, N), E)), _Smoothness)));

				float4 finalColor = float4(0, 0, 0, 1);
				finalColor.rgb = lerp(diffuse, specular, min(step(_ShadowBias, dot(N,L)), _ShadowMaps * attenuation + 1 * (1 - _ShadowMaps)));

				return finalColor;
			}
			ENDCG
		}
	}
	Fallback "VertexLit"
}
