Shader "TokMart/AssetBake"
{
 Properties { _MainTex ("Texture", 2D) = "white" {} }
 SubShader { Tags { "RenderType"="Opaque" } Cull Off Pass {
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct appdata {float4 vertex:POSITION;float3 normal:NORMAL;float2 uv:TEXCOORD0;};
 struct v2f {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;float shade:TEXCOORD1;};
 sampler2D _MainTex;
 v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.shade=.62+.38*max(0,dot(UnityObjectToWorldNormal(v.normal),normalize(float3(-.4,1,-.7))));return o;}
 fixed4 frag(v2f i):SV_Target{fixed4 c=tex2D(_MainTex,i.uv);return fixed4(c.rgb*i.shade,1);}
 ENDCG
 } }
}
