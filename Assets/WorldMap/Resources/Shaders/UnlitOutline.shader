// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "World Political Map 2D/Unlit Outline" {
 
Properties {
    _Color ("Color", Color) = (1,1,1,1)
}
 
SubShader {
    Tags {
       "Queue"="Geometry+301"
       "RenderType"="Opaque"
  	}
  	ZWrite Off
    Pass {
    	CGPROGRAM
		#pragma vertex vert	
		#pragma fragment frag
		#include "UnityCG.cginc"				

		fixed4 _Color;

		struct AppData {
			float4 vertex : POSITION;
		};
		
		void vert(inout AppData v) {
			v.vertex = UnityObjectToClipPos(v.vertex);
			#if UNITY_REVERSED_Z
			v.vertex.z+= 0.001;
			#else
			v.vertex.z-=0.001;
			#endif
		}
		
		fixed4 frag(AppData i) : COLOR {
			return _Color;					
		}
			
		ENDCG
    }
    
   // SECOND STROKE ***********
 
    Pass {
    	CGPROGRAM
		#pragma vertex vert	
		#pragma fragment frag
		#include "UnityCG.cginc"				

		fixed4 _Color;

		struct AppData {
			float4 vertex : POSITION;
		};

		void vert(inout AppData v) {
			float4x4 projectionMatrix = UNITY_MATRIX_P;
			float d = projectionMatrix[1][1];
 			float distanceFromCameraToVertex = UnityObjectToViewPos(v.vertex).z; // mul( UNITY_MATRIX_MV, v.vertex ).z;
 			//The check here is for wether the camera is orthographic or perspective
 			float frustumHeight = projectionMatrix[3][3] == 1 ? 2/d : 2.0*-distanceFromCameraToVertex*(1/d);
 			float metersPerPixel = frustumHeight/_ScreenParams.y;
 			metersPerPixel /= unity_ObjectToWorld[0][0];
 			
 			v.vertex.x += metersPerPixel;
			v.vertex = UnityObjectToClipPos(v.vertex);
			#if UNITY_REVERSED_Z
			v.vertex.z+= 0.001; //0.002; 
			#else
			v.vertex.z-=0.001; //0.002; 
			#endif
		}
		
		fixed4 frag(AppData i) : COLOR {
			return _Color;						//Output RGBA color
		}
			
		ENDCG
    }
    
      // THIRD STROKE ***********
 
    Pass {
    	CGPROGRAM
		#pragma vertex vert	
		#pragma fragment frag
		#include "UnityCG.cginc"				

		fixed4 _Color;
		
		struct AppData {
			float4 vertex : POSITION;
		};		
		
		void vert(inout AppData v) {
			float4x4 projectionMatrix = UNITY_MATRIX_P;
			float d = projectionMatrix[1][1];
 			float distanceFromCameraToVertex = UnityObjectToViewPos(v.vertex).z; //mul( UNITY_MATRIX_MV, v.vertex ).z;
 			//The check here is for wether the camera is orthographic or perspective
 			float frustumHeight = projectionMatrix[3][3] == 1 ? 2/d : 2.0*-distanceFromCameraToVertex*(1/d);
 			float metersPerPixel = frustumHeight/_ScreenParams.y;
			metersPerPixel /= unity_ObjectToWorld[1][1];

 			v.vertex.y += metersPerPixel;
			v.vertex = UnityObjectToClipPos(v.vertex);
			#if UNITY_REVERSED_Z
			v.vertex.z+= 0.001; //0.002; 
			#else
			v.vertex.z-=0.001; //0.002; 
			#endif
		}
		
		fixed4 frag(AppData i) : COLOR {
			return fixed4(_Color);						//Output RGBA color
		}
			
		ENDCG
    }
    
       
      // FOURTH STROKE ***********
 
    Pass {
    	CGPROGRAM
		#pragma vertex vert	
		#pragma fragment frag
		#include "UnityCG.cginc"				

		fixed4 _Color;

		struct AppData {
			float4 vertex : POSITION;
		};		
		
		void vert(inout AppData v) {
			float4x4 projectionMatrix = UNITY_MATRIX_P;
			float d = projectionMatrix[1][1];
 			float distanceFromCameraToVertex = UnityObjectToViewPos(v.vertex).z; //mul( UNITY_MATRIX_MV, v.vertex ).z;
 			//The check here is for wether the camera is orthographic or perspective
 			float frustumHeight = projectionMatrix[3][3] == 1 ? 2/d : 2.0*-distanceFromCameraToVertex*(1/d);
 			float metersPerPixel = frustumHeight/_ScreenParams.y;
			metersPerPixel /= unity_ObjectToWorld[0][0];
 			
 			v.vertex.x-= metersPerPixel;
			v.vertex = UnityObjectToClipPos(v.vertex);
			#if UNITY_REVERSED_Z
			v.vertex.z+= 0.001; //0.002; 
			#else
			v.vertex.z-=0.001; //0.002; 
			#endif
		}
		
		fixed4 frag(AppData i) : COLOR {
			return _Color;						//Output RGBA color
		}
			
		ENDCG
    }
    
    // FIFTH STROKE ***********
 
    Pass {
    	CGPROGRAM
		#pragma vertex vert	
		#pragma fragment frag
		#include "UnityCG.cginc"				

		fixed4 _Color;

		struct AppData {
			float4 vertex : POSITION;
		};
				
		void vert(inout AppData v) {
			float4x4 projectionMatrix = UNITY_MATRIX_P;
			float d = projectionMatrix[1][1];
 			float distanceFromCameraToVertex = UnityObjectToViewPos(v.vertex).z; //mul( UNITY_MATRIX_MV, v.vertex ).z;
 			//The check here is for wether the camera is orthographic or perspective
 			float frustumHeight = projectionMatrix[3][3] == 1 ? 2/d : 2.0*-distanceFromCameraToVertex*(1/d);
 			float metersPerPixel = frustumHeight/_ScreenParams.y;
 			metersPerPixel /= unity_ObjectToWorld[1][1];
 			
 			v.vertex.y-= metersPerPixel;
			v.vertex = UnityObjectToClipPos(v.vertex);
			#if UNITY_REVERSED_Z
			v.vertex.z+= 0.001; //0.002; 
			#else
			v.vertex.z-=0.001; //0.002; 
			#endif
		}
		
		fixed4 frag(AppData i) : COLOR {
			return _Color;						//Output RGBA color
		}
			
		ENDCG
    }
}
}
