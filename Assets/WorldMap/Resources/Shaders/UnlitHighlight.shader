Shader "World Political Map 2D/Unlit Highlight" {
Properties {
    _Color ("Color", Color) = (1,1,1,0.5)
}
SubShader {
    Tags {
        "Queue"="Geometry+5"
        "IgnoreProjector"="True"
        "RenderType"="Transparent"
    }
			//Set up blending and other operations
			Cull Off			// Must be off to draw all highlight rectangles
			ZWrite Off			//On | Off - Z coordinates from pixel positions will be written to the Z/Depth buffer
//			ZTest Always
			Offset -5, -5
			Color [_Color]
			Fog { Mode Off }
			Blend SrcAlpha OneMinusSrcAlpha 	//SrcFactor DstFactor (also:, SrcFactorA DstFactorA) = One | Zero | SrcColor | SrcAlpha | DstColor | DstAlpha | OneMinusSrcColor | OneMinusSrcAlpha | OneMinusDstColor | OneMinusDstAlpha - Blending between shader output and the backbuffer will use blend mode 'Solid'
			Pass {
			}
	}	
}
