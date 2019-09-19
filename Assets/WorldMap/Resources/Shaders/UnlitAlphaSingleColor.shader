Shader "World Political Map 2D/Unlit Alpha Single Color" {
 
Properties {
    _Color ("Color", Color) = (1,1,1,0.5)
}
 
SubShader {
	Tags {
        "Queue"="Transparent+1"
        "RenderType"="Transparent"
    }
    Color [_Color]
   	Blend SrcAlpha OneMinusSrcAlpha
   	Offset -1, -1
    Pass {
 	
    }
}
 
}
