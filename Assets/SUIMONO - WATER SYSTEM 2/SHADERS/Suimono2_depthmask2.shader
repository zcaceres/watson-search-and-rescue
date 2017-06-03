// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Suimono2/DepthMask2" {

SubShader {

	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparency" }
	Cull Back Lighting Off ZWrite Off
		
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf NoLight vertex:vert
		#include "UnityCG.cginc"

		sampler2D _suimono_TransTex;

        struct Input {
            float4 projPos;
        };


	    void vert (inout appdata_full v, out Input o) {
	    	UNITY_INITIALIZE_OUTPUT(Input,o);
	    	float4 pos = UnityObjectToClipPos(v.vertex);
	        o.projPos = ComputeScreenPos(pos);
			COMPUTE_EYEDEPTH(o.projPos.z);
	    }


		half4 LightingNoLight (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten){
			return half4(s.Albedo*0.93,s.Alpha);
		}


	    void surf (Input IN, inout SurfaceOutput o) {
			//get screen Textures
			o.Albedo = tex2Dproj(_suimono_TransTex, UNITY_PROJ_COORD(IN.projPos)).rgb;
	        o.Alpha = 1.0;
	    }
		ENDCG 
	}

	fallback "Standard"
}
