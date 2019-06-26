Shader "World Political Map 2D/Unlit Texture 16K" {
 
Properties {
    _Color ("Color", Color) = (1,1,1)
	_TexTL ("Tex TL", 2D) = "white" {}
	_TexTR ("Tex TR", 2D) = "white" {}
	_TexBL ("Tex BL", 2D) = "white" {}
	_TexBR ("Tex BR", 2D) = "white" {}
}
 
SubShader {
  	Tags { "RenderType"="Opaque" }
		Lighting Off
		Offset 5,2
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				
				#include "UnityCG.cginc"
				
				sampler2D _TexTL;
				sampler2D _TexTR;
				sampler2D _TexBL;
				sampler2D _TexBR;
				
				struct v2f {
					float4 pos : SV_POSITION;
					half2 uv : TEXCOORD0;
				};
        
				v2f vert (appdata_base v) {
					v2f o;
					o.pos 				= UnityObjectToClipPos(v.vertex);
					o.uv				= v.texcoord;
					return o;
				 }
				
				fixed4 frag (v2f i) : COLOR {
					// compute Earth pixel color
					if (i.uv.x<0.5) {
						if (i.uv.y>0.5) {
							return tex2D (_TexTL, i.uv.xy * 2.0.xx);
						} else {
							return tex2D (_TexBL, half2(i.uv.x * 2.0f, (i.uv.y-0.5) * 2.0));	
						}
					} else {
						if (i.uv.y>0.5) {
							return tex2D (_TexTR, half2((i.uv.x - 0.5) * 2.0f, i.uv.y * 2.0));
						} else {
							return tex2D (_TexBR, half2((i.uv.x - 0.5) * 2.0f, (i.uv.y-0.5) * 2.0));	
						}
					}
				}
			
			ENDCG
		}
	}
}