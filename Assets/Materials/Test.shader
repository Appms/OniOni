Shader "Aubergine/Object/Unlit/Texture" {
Properties {
_MainTex("Base (RGB)", 2D) = "white" {}
}

SubShader {
Tags { "RenderType" = "Opaque" }
LOD 100

Pass {
Name "BASE"
Tags { "LightMode" = "Always" }

Fog { Mode off }

CGPROGRAM
#pragma exclude_renderers xbox360 ps3 flash
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
#include "UnityCG.cginc"

sampler2D _MainTex;
float4 _MainTex_ST;

struct a2v {
float4 vertex : POSITION;
float2 texcoord : TEXCOORD0;
};

struct v2f {
float4 pos : SV_POSITION;
half2 texcoord : TEXCOORD0;
};

v2f vert(a2v v) {
v2f o;
o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
return o;
}

fixed4 frag(v2f i) : COLOR {
fixed4 col = tex2D(_MainTex, i.texcoord);
return col;
}
ENDCG 
}

Pass {
Name "ShadowCaster"
Tags { "LightMode" = "ShadowCaster" }

Fog {Mode Off}
ZWrite On ZTest LEqual Cull Off
Offset 1, 1

CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
#pragma multi_compile_shadowcaster
#include "UnityCG.cginc"

struct v2f {
V2F_SHADOW_CASTER;
};

v2f vert(appdata_base v) {
v2f o;
TRANSFER_SHADOW_CASTER(o)
return o;
}

fixed4 frag(v2f i) : COLOR {
SHADOW_CASTER_FRAGMENT(i)
}
ENDCG 
}

//Editor needs this pass according to builtin shaders
Pass {
Name "ShadowCollector"
Tags { "LightMode" = "ShadowCollector" }

Fog {Mode Off}
ZWrite On ZTest LEqual

CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
#pragma multi_compile_shadowcollector

#define SHADOW_COLLECTOR_PASS
#include "UnityCG.cginc"

struct a2v {
float4 vertex : POSITION;
};

struct v2f {
V2F_SHADOW_COLLECTOR;
};

v2f vert(a2v v) {
v2f o;
TRANSFER_SHADOW_COLLECTOR(o)
return o;
}

fixed4 frag(v2f i) : COLOR {
SHADOW_COLLECTOR_FRAGMENT(i)
}
ENDCG 
}
}

//For old graphics cards
SubShader {
Tags { "RenderType" = "Opaque" }
LOD 100

Pass {
Lighting Off
Fog { Mode off }

SetTexture [_MainTex] { combine texture } 
}
}

Fallback Off
}
