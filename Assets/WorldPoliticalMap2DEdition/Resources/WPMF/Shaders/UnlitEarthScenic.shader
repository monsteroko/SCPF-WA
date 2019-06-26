Shader "World Political Map 2D/Scenic" {

	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
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
		
			CGPROGRAM
				#pragma surface surf Unlit vertex:vert


				sampler2D _MainTex;
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
					float2 uv_MainTex;
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
					half4 earth = tex2D (_MainTex, IN.uv_MainTex);
					o.Alpha = earth.a;
					
					fixed2 t = fixed2(_Time[0] * _CloudSpeed, 0);
					fixed2 disp = -IN.viewDir * _CloudElevation;
					
					half3 cloud = tex2D (_CloudMap, IN.uv_MainTex + t - disp);
					half3 shadows = tex2D (_CloudMap, IN.uv_MainTex + t + fixed2(0.998,0) + disp) * _CloudShadowStrength;
					shadows *= saturate (dot(o.Normal, IN.viewDir));
					o.Albedo = earth.rgb + (cloud.rgb - clamp(shadows.rgb, shadows.rgb, 1-cloud.rgb)) * _CloudAlpha ;
				}
			
			ENDCG
	}
}