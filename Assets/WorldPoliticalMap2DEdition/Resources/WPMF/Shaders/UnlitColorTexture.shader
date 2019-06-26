Shader "World Political Map 2D/Unlit Color Texture" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white"
    }
    Category {
       Tags {
       	"Queue"="Geometry"
       	"RenderType"="Opaque"
       }
       Lighting On
       ZWrite On
       Cull Back
       Offset 1,1
       SubShader {
            Material {
               Emission [_Color]
            }
            Pass {
               SetTexture [_MainTex] {
                      Combine Texture * Primary, Texture * Primary
                }
            }
        } 
    }
}