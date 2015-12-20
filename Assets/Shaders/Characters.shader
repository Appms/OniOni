Shader "OniOni/Characters"
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
		_SpecularTexture("Specular Texture", 2D) = "white" {}
		_SpecularBias("Specular Bias", Range(0.1,1)) = 0.1
		_DissolveTexture("Dissolve Texture", 2D) = "white" {}
		_DissolveFactor("Dissolve Factor", Range(0,1)) = 0
		_DissolveGradient("Dissolve Gradient", 2D) = "white" {}
		_GradientWidth("Gradient Width", Float) = 2
		[MaterialToggle]_Hurt("Hurt", Float) = 0
		[MaterialToggle]_Push("Push Buff", Float) = 0
		[MaterialToggle]_Atk("Attack Buff", Float) = 0
		[MaterialToggle]_Def("Defense Buff", Float) = 0
		[MaterialToggle]_Speed("Speed Buff", Float) = 0
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
			sampler2D _SpecularTexture;
			float _SpecularBias;
			sampler2D _DissolveTexture;
			sampler2D _DissolveGradient;
			float _DissolveFactor;
			float _GradientWidth;
			float _Hurt;
			float _Push;
			float _Atk;
			float _Def;
			float _Speed;

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
				float specularStrength = tex2D(_SpecularTexture, IN.uvs).r;
				float smoothness = tex2D(_SpecularTexture, IN.uvs).g * 100.0;

				HSBColor baseColor = RGB2HSB(IN.color);
				baseColor.h += _H;
				baseColor.s += _S;
				baseColor.b += _B;

				if(_Hurt > 0.0) baseColor.h = 0.0;
				if(_Push > 0.0) baseColor.h = lerp(lerp(baseColor.h, 0.15, 0.5), 0.15, tex2D(_DissolveTexture, IN.uvs + _Time.xy).r);
				if(_Atk > 0.0) baseColor.h = lerp(lerp(baseColor.h, 0.0, 0.5), 0.0, tex2D(_DissolveTexture, IN.uvs + _Time.xy).r);
				if(_Def > 0.0) baseColor.h = lerp(lerp(baseColor.h, 0.33, 0.5), 0.33, tex2D(_DissolveTexture, IN.uvs + _Time.xy).r);
				if(_Speed > 0.0) baseColor.h = lerp(lerp(baseColor.h, 0.66, 0.5), 0.66, tex2D(_DissolveTexture, IN.uvs + _Time.xy).r);

				HSBColor complementaryColor = complementary(baseColor);
				HSBColor specularColor = lighten(baseColor, specularStrength);

				float noise = tex2D(_DissolveTexture, IN.uvs).r;
				float dissolve = remap(_DissolveFactor, 0,1,-1,1);
				float clipFactor = 1 - saturate(noise + dissolve);
				float clipGlow = 1 - saturate(remap(clipFactor, 0, 1, -_GradientWidth, _GradientWidth));

				HSBColor dissolveColor = RGB2HSB(tex2D(_DissolveGradient, float2(clamp(clipGlow,0,1),0.5)));
				dissolveColor.h += _H;
				clip(clipFactor - 0.5f);

				float3 N = normalize(UnityObjectToWorldNormal(IN.normal));
				float3 E = -normalize(UnityWorldSpaceViewDir(IN.position));
				float3 L = normalize(UnityWorldSpaceLightDir(IN.position));
				float attenuation = LIGHT_ATTENUATION(IN);

				float4 diffuse = lerp(HSB2RGB(baseColor), HSB2RGB(complementaryColor) * _ShadowBrightness, _ShadowStrength);
				float4 specular = lerp(HSB2RGB(baseColor), HSB2RGB(specularColor), step(_SpecularBias, pow(max(0, dot(reflect(L, N), E)), smoothness)));

				float4 finalColor = float4(0, 0, 0, 1);
				finalColor.rgb = lerp(diffuse, specular, min(step(_ShadowBias, dot(N,L)), _ShadowMaps * attenuation + 1 * (1 - _ShadowMaps)));

				finalColor = lerp(finalColor, finalColor + HSB2RGB(dissolveColor), clipGlow);

				return finalColor;
			}
			ENDCG
		}
	}
	Fallback "VertexLit"
}