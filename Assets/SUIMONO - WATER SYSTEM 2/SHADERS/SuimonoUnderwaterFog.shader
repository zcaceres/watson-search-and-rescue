// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/SuimonoUnderwaterFog" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
}

	CGINCLUDE

	#pragma target 3.0
	#include "UnityCG.cginc"

	sampler2D _suimono_TransTex;
	//sampler2D _suimono_CausticTex;
	//sampler2D _suimono_underwaterMaskTex;
	sampler2D _underwaterDistort;
	sampler2D _distort1Mask;
	sampler2D _distort2Mask;
	//sampler2D _bgReflectTex;

	uniform sampler2D _MainTex;
	uniform sampler2D_float _CameraDepthTexture;
	uniform float4 _HeightParams;
	uniform float4 _DistanceParams;
	
	int4 _SceneFogMode;
	float4 _SceneFogParams;
	uniform float4 _MainTex_TexelSize;
	
	// for fast world space reconstruction
	uniform float4x4 _FrustumCornersWS;
	uniform float4 _CameraWS;
	float _Suimono_IsUnderwater;
	float _lightFactor;
	float4 _suimono_lightColor;
	half4 _BlurOffsets;

	float4 _underwaterColor;
	float4 _cameraBGColor;
	float _distortAmt;
	float _distortSpeed;
	float _distortScale;
	float _trans1;
	float _trans2;
	float _dropOffx;
	float _dropOffy;
	float _showScreenMask = 0.0;
	//float _useUnderSurfaceView = 0.0;
	float _enableUnderwater;
	float _Suimono_isLinear;
	float _blur;
	float _hDepth;




	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		float2 uv_depth : TEXCOORD1;
		float4 interpolatedRay : TEXCOORD2;
		float3 lightDir : TEXCOORD3;
		half2 taps[4] : TEXCOORD4;
	};
	


	v2f vert (appdata_tan v){
		v2f o;
		half index = v.vertex.z;
		v.vertex.z = 0.1;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;
		o.uv_depth = v.texcoord.xy;

		#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0)
			o.uv.y = 1-o.uv.y;
		#endif				
		
		o.interpolatedRay = _FrustumCornersWS[(int)index];
		o.interpolatedRay.w = index;
		
		//get light direction
		o.lightDir = WorldSpaceLightDir(v.vertex);

		//blur
		o.taps[0] = o.uv + _MainTex_TexelSize * _BlurOffsets.xy;
		o.taps[1] = o.uv - _MainTex_TexelSize * _BlurOffsets.xy;
		o.taps[2] = o.uv + _MainTex_TexelSize * _BlurOffsets.xy * half2(1,-1);
		o.taps[3] = o.uv - _MainTex_TexelSize * _BlurOffsets.xy * half2(1,-1);

		return o;
	}
	

	// Applies one of standard fog formulas, given fog coordinate (i.e. distance)
	half ComputeFogFactor (float coord){
		float fogFac = 0.0;
		if (_SceneFogMode.x == 1) // linear
		{
			// factor = (end-z)/(end-start) = z * (-1/(end-start)) + (end/(end-start))
			fogFac = coord * _SceneFogParams.z + _SceneFogParams.w;
		}
		if (_SceneFogMode.x == 2) // exp
		{
			// factor = exp(-density*z)
			fogFac = _SceneFogParams.y * coord; fogFac = exp2(-fogFac);
		}
		if (_SceneFogMode.x == 3) // exp2
		{
			// factor = exp(-(density*z)^2)
			fogFac = _SceneFogParams.x * coord; fogFac = exp2(-fogFac*fogFac);
		}
		return saturate(fogFac);
	}

	// Distance-based fog
	float ComputeDistance (float3 camDir, float zdepth)
	{
		float dist; 
		if (_SceneFogMode.y == 1)
			dist = length(camDir);
		else
			dist = zdepth * _ProjectionParams.z;
		// Built-in fog starts at near plane, so match that by
		// subtracting the near value. Not a perfect approximation
		// if near plane is very large, but good enough.
		dist -= _ProjectionParams.y;
		return dist;
	}

	// Linear half-space fog, from https://www.terathon.com/lengyel/Lengyel-UnifiedFog.pdf
	float ComputeHalfSpace (float3 wsDir)
	{
		float3 wpos = _CameraWS + wsDir;
		float FH = _HeightParams.x;
		float3 C = _CameraWS;
		float3 V = wsDir;
		float3 P = wpos;
		float3 aV = _HeightParams.w * V;
		float FdotC = _HeightParams.y;
		float k = _HeightParams.z;
		float FdotP = P.y-FH;
		float FdotV = wsDir.y;
		float c1 = k * (FdotP + FdotC);
		float c2 = (1-2*k) * FdotP;
		float g = min(c2, 0.0);
		g = -length(aV) * (c1 - g * g / abs(FdotV+1.0e-5f));
		return g;
	}




	float ComputeDepth(half2 dUV, float3 ray, bool distance, bool height, half dist)
	{
		half fFac = 0.0;
		half rDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,dUV);

		half dp = Linear01Depth(rDepth);
		float3 wDir = dp * ray;

		// Compute fog distance
		half gd = dist;
		if (distance)
			gd += ComputeDistance (wDir, dp);
		if (height)
			gd += ComputeHalfSpace (wDir);

		// Compute fog amount
		fFac = ComputeFogFactor (max(0.0,gd));
		fFac = 1.0-fFac;

		if (rDepth >= 0.999999)
			fFac = 1.0;

		return saturate(fFac);
	}




	half4 ComputeFog (v2f i, bool distance, bool height) : SV_Target{

		half3 distortTex = UnpackNormal(tex2D(_underwaterDistort,i.uv*_distortScale+float2(_Time.x*_distortSpeed,-_Time.x*0.5*_distortSpeed)));
		half2 dUV = i.uv;
		dUV.x += (distortTex.x * _distortAmt);

		half4 waterColor = tex2D(_suimono_TransTex, i.uv);
		half4 origColor = tex2D(_MainTex, i.uv);
		half4 sceneColor = tex2D(_MainTex, dUV);
		half4 underwaterScene = tex2D(_suimono_TransTex,dUV);
		half4 origSceneColor = tex2D(_MainTex, i.uv);


		//underwater MASK
		half4 underwaterMask = half4(0,0,1,1);


		//original depth gen
		float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,dUV);
		float dpth = Linear01Depth(rawDepth);
		float4 wsDir = dpth * i.interpolatedRay;
		float4 wsPos = _CameraWS + wsDir;

		// Compute fog distance
		float g = _DistanceParams.x;
		if (distance)
			g += ComputeDistance (wsDir, dpth);
		if (height)
			g += ComputeHalfSpace (wsDir);

		// Compute fog amount
		half fogFac = ComputeFogFactor (max(0.0,g));
		half fogFac2 = 0;

		fogFac = 1.0-fogFac;

		if (rawDepth >= 0.999999)
			fogFac = 1.0;


		//reflected depth gen
		rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,dUV);
		dpth = Linear01Depth(rawDepth);
		wsDir = dpth * i.interpolatedRay;
		wsPos = _CameraWS + wsDir;


		// Compute fog distance
		g = _DistanceParams.x;
		if (distance)
			g += ComputeDistance (wsDir, dpth);
		if (height)
			g += ComputeHalfSpace (wsDir);


		// Compute fog amount
		fogFac2 = ComputeFogFactor (max(0.0,g));
		fogFac2 = 1.0-fogFac2;

		if (rawDepth >= 0.999999)
			fogFac2 = 1.0;




		//compute background fog color
		_underwaterColor = _cameraBGColor;
		_underwaterColor.rgb = _underwaterColor.rgb * lerp(2.2,1.0,_Suimono_isLinear);
		half3 fogCola = _underwaterColor.rgb;

		//depth color influence
		half depthFaca = saturate(lerp(0.3,1.6,dot(normalize(wsDir),half3(0,-1,0))));
		fogCola = lerp(fogCola,fogCola*0.1,depthFaca);

		//light color influence
		half lightFaca = saturate(lerp(0.0,0.8,dot(normalize(wsDir),i.lightDir))-depthFaca);
		fogCola = lerp(fogCola,fogCola*2,lightFaca);
		half lightFac2a = saturate(lerp(-5.0,0.5,dot(normalize(wsDir),i.lightDir))-depthFaca)*2;
		fogCola = lerp(fogCola,fogCola*1.4,lightFac2a);

		half3 depthCol = fogCola.rgb*_suimono_lightColor*_lightFactor;

		//darken underwater fog based on depth
		depthCol = lerp(depthCol,depthCol*0.05,_hDepth);



		//Blur Distance
		half fogFac3 = ComputeDepth(dUV,i.interpolatedRay, distance,height,_DistanceParams.x);
		_blur = lerp(0,4,ComputeDepth(dUV,i.interpolatedRay, distance,height,-5));

		half2 uUv =  dUV + lerp(0,(_MainTex_TexelSize * _BlurOffsets.xy),_blur);
		half3 color = lerp(tex2D(_MainTex,uUv).rgb, depthCol, ComputeDepth(uUv,i.interpolatedRay, distance,height,_DistanceParams.x) );
		uUv =  dUV - lerp(0,(_MainTex_TexelSize * _BlurOffsets.xy),_blur);
		color += lerp(tex2D(_MainTex,uUv).rgb, depthCol, ComputeDepth(uUv,i.interpolatedRay, distance,height,_DistanceParams.x) );

		uUv =  dUV + lerp(0,(_MainTex_TexelSize * _BlurOffsets.xy * half2(1,-1)),_blur);
		color += lerp(tex2D(_MainTex,uUv).rgb, depthCol, ComputeDepth(uUv,i.interpolatedRay, distance,height,_DistanceParams.x) );
		uUv =  dUV - lerp(0,(_MainTex_TexelSize * _BlurOffsets.xy * half2(1,-1)),_blur);
		color += lerp(tex2D(_MainTex,uUv).rgb, depthCol, ComputeDepth(uUv,i.interpolatedRay, distance,height,_DistanceParams.x) );

		uUv =  dUV + lerp(0,(_MainTex_TexelSize * _BlurOffsets.xy * half2(-1,1)),_blur);
		color += lerp(tex2D(_MainTex,uUv).rgb, depthCol, ComputeDepth(uUv,i.interpolatedRay, distance,height,_DistanceParams.x) );
		uUv =  dUV - lerp(0,(_MainTex_TexelSize * _BlurOffsets.xy * half2(-1,1)),_blur);
		color += lerp(tex2D(_MainTex,uUv).rgb, depthCol, ComputeDepth(uUv,i.interpolatedRay, distance,height,_DistanceParams.x) );

		uUv =  dUV + lerp(0,(_MainTex_TexelSize * (_BlurOffsets.xy*0.5)),_blur);
		color += lerp(tex2D(_MainTex,uUv).rgb, depthCol, ComputeDepth(uUv,i.interpolatedRay, distance,height,_DistanceParams.x) );
		uUv =  dUV - lerp(0,(_MainTex_TexelSize * (_BlurOffsets.xy*0.5)),_blur);
		color += lerp(tex2D(_MainTex,uUv).rgb, depthCol, ComputeDepth(uUv,i.interpolatedRay, distance,height,_DistanceParams.x) );
		uUv =  dUV + lerp(0,(_MainTex_TexelSize * (_BlurOffsets.xy*0.5) * half2(0.5,-0.5)),_blur);
		color += lerp(tex2D(_MainTex,uUv).rgb, depthCol, ComputeDepth(uUv,i.interpolatedRay, distance,height,_DistanceParams.x) );
		uUv =  dUV - lerp(0,(_MainTex_TexelSize * (_BlurOffsets.xy*0.5) * half2(-0.5,0.5)),_blur);
		color += lerp(tex2D(_MainTex,uUv).rgb, depthCol, ComputeDepth(uUv,i.interpolatedRay, distance,height,_DistanceParams.x) );

		sceneColor.rgb = color * 0.1;



		//FINAL COLOR
		half4 retCol;
		retCol.a = 1.0;




		half3 bgCol = underwaterScene.rgb;
		if (_Suimono_IsUnderwater == 1.0){
			bgCol = sceneColor;
		}


		bgCol = lerp(underwaterScene.rgb,bgCol,underwaterMask.b);




		//blend underwater fog
		retCol.rgb = lerp(bgCol.rgb, depthCol.rgb * _suimono_lightColor * _lightFactor, fogFac3);

		half3 underCol = retCol.rgb;

		retCol.rgb = lerp(origSceneColor.rgb,retCol.rgb,saturate(underwaterMask.g+lerp(0,underwaterMask.b,_Suimono_IsUnderwater)));



		//screen effects
		half3 tCol = lerp(half3(1,1,1), half3(0.1,0.6,0.8), 0.2);
		half2 sUV = i.uv;
		half3 baseTex = UnpackNormal(tex2D(_underwaterDistort,i.uv*0.4-float2(_Time.x*1.3,_Time.x*1.3)));
		half baseDis = i.uv.x + (baseTex.x*0.05);

		half3 screenTex = UnpackNormal(tex2D(_underwaterDistort,i.uv*0.1+float2(sUV.x*0.1+baseDis,_Time.x*1)));
		half distort1Mask = tex2D(_distort1Mask,i.uv*0.1+float2(i.uv.x*0.1+baseDis,_Time.x*1)).r;
		sUV.x = sUV.x-lerp((screenTex.x*0.08),0.0,0.0);

		half4 distort1Color = tex2D(_MainTex, sUV);

		half trans1Time = saturate(_trans1);
		distort1Mask = saturate(lerp(0.1-trans1Time,11 - (11 * trans1Time),distort1Mask));

		retCol.rgb = lerp(retCol.rgb,retCol.rgb*0.9*tCol,distort1Mask);
		retCol.rgb = lerp(retCol.rgb,distort1Color.rgb,distort1Mask*1.2);


		//drops
		half trans2Time = saturate(_trans2);

		half2 rUV = i.uv;
		half distort2Mask = tex2D(_distort2Mask,i.uv*float2(0.4,0.23)+float2(_dropOffx,_Time.x*0.11+_dropOffy)).r;
		rUV.x = rUV.x-(distort2Mask*0.1);
		half4 distort2Color = tex2D(_MainTex, rUV);
		distort2Mask = saturate(lerp(0.2-trans2Time,2 - (2 * trans2Time),distort2Mask));
		retCol.rgb = lerp(retCol.rgb,retCol.rgb*0.9*tCol,distort2Mask);
		retCol.rgb = lerp(retCol.rgb,lerp(distort2Color.rgb,lerp(distort2Color.rgb,distort1Color.rgb,distort2Mask),0.1),distort2Mask*1.2);

		half2 rUV2 = i.uv;
		half distort3Mask = tex2D(_distort2Mask,i.uv*float2(0.8,0.6)+float2(_dropOffx,_Time.x*0.075+_dropOffy)).r;
		rUV2.x = rUV2.x-(distort3Mask*0.1);
		half4 distort3Color = tex2D(_MainTex, rUV2);
		distort3Mask = saturate(lerp(0.2-trans2Time,2 - (2 * trans2Time),distort3Mask));
		retCol.rgb = lerp(retCol.rgb,retCol.rgb*0.9*tCol,distort3Mask);
		retCol.rgb = lerp(retCol.rgb,lerp(distort3Color.rgb,lerp(distort3Color.rgb,distort1Color.rgb,distort3Mask),0.1),distort3Mask*1.2);


		//blend effects based on underwater mask
		half3 overCol = retCol.rgb;
		overCol.rgb = lerp(retCol.rgb,retCol.rgb,underwaterMask.g);
		retCol.rgb = lerp(overCol.rgb,underCol.rgb,saturate(underwaterMask.g));

		retCol.rgb = lerp(retCol.rgb,underwaterMask.rgb,_showScreenMask);

		//set blank underwater
		retCol.rgb = lerp(origColor.rgb,retCol.rgb,_enableUnderwater);

		return retCol;
	}

ENDCG




SubShader
{
	ZTest Always Cull Off ZWrite Off Fog { Mode Off }

	// 0: distance + height
	Pass
	{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		half4 frag (v2f i) : SV_Target { return ComputeFog (i, true, true); }
		ENDCG
	}
	// 1: distance
	Pass
	{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		half4 frag (v2f i) : SV_Target { return ComputeFog (i, true, false); }
		ENDCG
	}
	// 2: height
	Pass
	{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		half4 frag (v2f i) : SV_Target { return ComputeFog (i, false, true); }
		ENDCG
	}


}


Fallback "Diffuse"

}
