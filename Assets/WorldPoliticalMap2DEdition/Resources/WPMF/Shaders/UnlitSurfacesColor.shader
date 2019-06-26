Shader "World Political Map 2D/Unlit Surface Single Color" {
 
Properties {
    _Color ("Color", Color) = (1,1,1)
}
 
SubShader {
    Color  [_Color]
        Tags {
        "Queue"="Geometry+1"
        "RenderType"="Opaque"
    	}
//    Offset 1,1
    ZWrite Off
    Pass {
    }
}
 
}
