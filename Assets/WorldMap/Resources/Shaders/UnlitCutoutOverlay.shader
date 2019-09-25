Shader "World Political Map 2D/Unlit Cutout Overlay" {
 
Properties
    {
        _Cutoff ("Alpha Cutoff" , Range(0, 1)) = 0.075
       _MainTex ("Texture", 2D) = ""
    }
 
SubShader
    {
        Tags {
         "Queue" = "Transparent+2" 
        }
            
        Alphatest Greater [_Cutoff]
        Lighting Off
//        ZTest Always
        Pass
        {
            SetTexture[_MainTex] { Combine texture, texture * primary}
        }
    }
}