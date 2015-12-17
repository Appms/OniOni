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
		_SpecularBias("Specular Bias", Range(0.1,1)) = 0
		_SpecularStrength("Specular Intensity", Range(0,1)) = 0.1
		_Smoothness("Smoothness", Range(1,100)) = 1
		_DissolveTexture("Dissolve Texture", 2D) = "white" {}
		_DissolveFactor("Dissolve Factor", Range(0,1)) = 0
		_DissolveGradient("Dissolve Gradient", 2D) = "white" {}
		_GradientWidth("Gradient Width", Float) = 2
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

			float _H;
			float _S;
			float _B;
			float _ShadowBias;
			float _ShadowStrength;
			float _ShadowBrightness;
			float _ShadowMaps;
			float _SpecularBias;
			float _SpecularStrength;
			float _Smoothness;
			sampler2D _DissolveTexture;
			sampler2D _DissolveGradient;
			float _DissolveFactor;
			float _GradientWidth;

			float4 _LightColor0;

			struct a2v {
				float4 pos : POSITION;
				float3 normal : NORMAL;
				float4 color : COLOR;
				float4 uvs : TEXCOORD0;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				float3 normal : NORMAL;
				float4 color : COLOR0;
				float3 position : COLOR1;
				float4 uvs : TEXCOORD2;

				LIGHTING_COORDS(0,1)
			};

			struct HSBColor {
				float h;
				float s;
				float b;
				float a;
			};

			HSBColor RGB2HSB(float4 color) {
				HSBColor ret;
				ret.h = ret.s = ret.b = 0;
				ret.a = color.a;

				float r = color.r;
				float g = color.g;
				float b = color.b;

				float M = max(r, max(g, b));

				if (M <= 0) return ret;

				float m = min(r, min(g, b));
				float dif = M - m;

				if (M > m) {
					if (g == M) ret.h = (b - r) / dif * 60 + 120;
					else if (b == M) ret.h = (r - g) / dif * 60 + 240;
					else if (b > g) ret.h = (g - b) / dif * 60 + 360;
					else ret.h = (g - b) / dif * 60;

					if (ret.h < 0) ret.h = ret.h + 360;
				}

				else ret.h = 0;

				ret.h = ret.h / 360;
				ret.s = dif / M;
				ret.b = M;

				return ret;
			}

			float4 HSB2RGB(HSBColor color) {
				float r = color.b;
				float g = color.b;
				float b = color.b;

				if (color.s != 0) {
					float M = clamp(color.b, 0, 1);
					float dif = clamp(color.b * color.s, 0, 1);
					float m = color.b - dif;

					float h = frac(color.h) * 360;

					if (h < 60) {
						r = M;
						g = h * dif / 60 + m;
						b = m;
					}
					else if (h < 120) {
						r = -(h - 120) * dif / 60 + m;
						g = M;
						b = m;
					}
					else if (h < 180) {
						r = m;
						g = M;
						b = (h - 120) * dif / 60 + m;
					}
					else if (h < 240) {
						r = m;
						g = -(h - 240) * dif / 60 + m;
						b = M;
					}
					else if (h < 300) {
						r = (h - 240) * dif / 60 + m;
						g = m;
						b = M;
					}
					else if (h <= 360) {
						r = M;
						g = m;
						b = -(h - 360) * dif / 60 + m;
					}
					else {
						r = g = b = 0;
					}
				}

				return float4(clamp(r, 0, 1), clamp(g, 0, 1), clamp(b, 0, 1), color.a);
			}

			HSBColor complementary(HSBColor color) {
				HSBColor complementary = color;
				complementary.h += 0.5;
				return complementary;
			}

			HSBColor maxSaturate(HSBColor color) {
				HSBColor sat = color;
				sat.s = 1;
				return sat;
			}

			HSBColor lighten(HSBColor color, float light) {
				HSBColor lighten = color;
				lighten.b += light;
				return lighten;
			}

			float remap(float value, float l1, float h1, float l2, float h2){
				return l2 + (value - l1) * (h2 - l2) / (h1 - l1);
			}
			
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
				float4 specular = lerp(HSB2RGB(baseColor), HSB2RGB(specularColor), step(_SpecularBias, pow(max(0, dot(reflect(L, N), E)), _Smoothness)));

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
