Shader "World Political Map 2D/Scenic 16K" {

	Properties {
		_TexTL ("Tex TL", 2D) = "white" {}
		_TexTR ("Tex TR", 2D) = "white" {}
		_TexBL ("Tex BL", 2D) = "white" {}
		_TexBR ("Tex BR", 2D) = "white" {}
		_NormalMap ("Normal Map", 2D) = "bump" {}
		_BumpAmount ("Bump Amount", Range(0, 1)) = 0.5
		_CloudMap ("Cloud Map", 2D) = "black" {}
		_CloudSpeed ("Cloud Speed", Range(-1, 1)) = -0.04
		_CloudAlpha ("Cloud Alpha", Range(0, 1)) = 1
		_CloudShadowStrength ("Cloud Shadow Strength", Range(0, 1)) = 0.2
		_CloudElevation ("Cloud Elevation", Range(0.001, 0.1)) = 0.003
	}
	
	Subshader {
		Tags { "RenderType"="Opaque" }
		Offset 2,2
		
			CGPROGRAM
				#pragma surface surf Unlit vertex:vert

				sampler2D _TexTL;
				sampler2D _TexTR;
				sampler2D _TexBL;
				sampler2D _TexBR;
				sampler2D _NormalMap;
				sampler2D _CloudMap;
				float _BumpAmount;
				float _CloudSpeed;
				float _CloudAlpha;
				float _CloudShadowStrength;
				float _CloudElevation;
				
				inline fixed4 LightingUnlit (SurfaceOutput s, fixed3 lightDir, fixed3 viewDir, fixed atten) {
					fixed4 c = fixed4(1,1,1,1);
					float d = 1.0 - 0.5 * saturate (dot(s.Normal, viewDir) + _BumpAmount - 1);
					c.rgb = s.Albedo * d;
					c.a = s.Alpha;
					return c;
				}
				
				struct Input {
					float2 uv_NormalMap;
					float3 normal;
					float3 viewDir;
				};
				
				 void vert (inout appdata_full v, out Input o) {
				 	UNITY_INITIALIZE_OUTPUT(Input,o);
				 	o.normal = v.normal;
				 }
				
				void surf (Input IN, inout SurfaceOutput o) {
					o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap));;
					half4 earth;
					// compute Earth pixel color
					if (IN.uv_NormalMap.y>0.5) {
						float2 uv = float2(IN.uv_NormalMap.x * 2.0, IN.uv_NormalMap.y * 1.9996);
						if (uv.x<1.0) {
							earth = tex2D (_TexTL, uv);
						} else {
							earth = tex2D (_TexTR, float2(uv.x - 1.0, uv.y));
						}
					} else {
						float2 uv = float2(IN.uv_NormalMap.x * 2.0, (IN.uv_NormalMap.y - 0.5) * 2.002);
						if (uv.x<1.0) {
							earth = tex2D (_TexBL, uv);	
						} else {
							earth = tex2D (_TexBR, float2(uv.x - 1.0, uv.y));	
						}
					}
					o.Alpha = earth.a;
					
					float2 t = float2(_Time[0] * _CloudSpeed, 0);
					float2 disp = -IN.viewDir * _CloudElevation;
					
					half3 cloud = tex2D (_CloudMap, IN.uv_NormalMap + t - disp);
					half3 shadows = tex2D (_CloudMap, IN.uv_NormalMap + t + float2(0.998,0) + disp) * _CloudShadowStrength;
					shadows *= saturate (dot(o.Normal, IN.viewDir));
					o.Albedo = earth.rgb + (cloud.rgb - clamp(shadows.rgb, shadows.rgb, 1-cloud.rgb)) * _CloudAlpha ;

				
				}
			
			ENDCG
	}
}