#include "Global.hlsl"
#include "Scene/Scene.hlsl"

Texture2D<float4>		_tex_Albedo : register(t0);
Texture2D<float4>		_tex_Normal : register(t1);
Texture2D<float3>		_tex_Emissive : register(t2);
Texture2D<float>		_tex_Depth : register(t3);
Texture2D<float3>		_tex_MotionVectors_Scatter : register(t4);
Texture2D<float2>		_tex_MotionVectors_Gather : register(t5);

Texture2D<float4>		_tex_Radiance0 : register(t8);
Texture2D<float4>		_tex_Radiance1 : register(t9);
Texture2D<float4>		_tex_BentCone : register(t10);
Texture2D<float4>		_tex_FinalRender : register(t11);

// Push/pull
Texture2D<float4>		_tex_SourceRadiance_PUSH : register(t12);
Texture2D<float4>		_tex_SourceRadiance_PULL : register(t13);

// Split buffers
Texture2DArray<float>	_tex_splitDepth : register(t20);
Texture2DArray<float2>	_tex_splitNormal : register(t21);	// Camera-space normal vectors
Texture2DArray<float4>	_tex_splitRadiance : register(t22);
Texture2DArray<float4>	_tex_splitIrradiance : register(t23);
Texture2DArray<float4>	_tex_splitBentCone : register(t24);

Texture2D<float>		_tex_DepthStencil : register(t30);


float4	VS( float4 __Position : SV_POSITION ) : SV_POSITION { return __Position; }

float	Depth2Weight( float _depth ) {
	return _depth < 1e-3 ? 0.0	// Keep uninitialized values as invalid
						 : smoothstep( 0.0, 1.0, _depth ) * smoothstep( 100.0, 40.0, _depth );	// Otherwise, we maximize the weights of samples whose depth is between 1 and 40 meters
}

float3	PS( float4 __Position : SV_POSITION ) : SV_TARGET0 {
	float2	UV = __Position.xy / _resolution;
	float3	csView = BuildCameraRay( UV );
	float	Z2Distance = length( csView );
			csView /= Z2Distance;
	float3	wsView = mul( float4( csView, 0 ), _camera2World ).xyz;
	float3	wsPos = _camera2World[3].xyz + Z2Distance * _ZNearFar_Q_Z.y * _tex_Depth[__Position.xy] * wsView;

//return 0.1 * wsPos;

	//////////////////////////////////////////////
	// Render full result
	return _tex_FinalRender[__Position.xy].xyz;
	//////////////////////////////////////////////


#if 0	// DEBUG BENT CONE

	// Face-cam
	float3	wsRight = normalize( cross( wsView, _camera2World[1].xyz ) );
	float3	wsUp = cross( wsRight, wsView );
	float3	wsAt = -wsView;
//float3	N = float3( dot( wsNormal, wsRight ), dot( wsNormal, wsUp ), -dot( wsNormal, wsView ) );	// Camera-space normal

	#if USE_RECOMPOSED_BUFFER
		float4	csBentConeDev = _tex_BentCone[__Position.xy];
	#else
		uint2	pixelIndex = uint2( floor( __Position.xy ) );
		uint2	subPixelIndex = pixelIndex & 3;
				pixelIndex >>= 2;
		float4	csBentConeDev = _tex_splitBentCone[uint3( pixelIndex, (subPixelIndex.y << 2) + subPixelIndex.x )];
	#endif

	float	cosConeAngle = length( csBentConeDev.xyz );
	float3	csBentCone = csBentConeDev.xyz / cosConeAngle;

cosConeAngle *= cosConeAngle;	// Now stored as sqrt!

	float	coneAngle = acos( cosConeAngle );
	float	stdDeviationAO = csBentConeDev.w;

	float3	wsBentCone = csBentCone.x * wsRight + csBentCone.y * wsUp + csBentCone.z * wsAt;


//return csBentConeDev.xyz;

//return 1.0 - cosConeAngle;	// a.k.a. the ambient occlusion
//return coneAngle * 2.0 / PI;
//return stdDeviationAO;
//return csBentCone;
return wsBentCone;
return 0.5 * (1.0 + wsBentCone);
return csBentConeDev.xyz;	// Show RAW value
#endif

#if 0	// DEBUG PUSH/PULL
	float4	V = (_flags & 0x100U)
			? _tex_SourceRadiance_PULL.SampleLevel( LinearClamp, __Position.xy / _resolution, _debugMipIndex )
			: _tex_SourceRadiance_PUSH.SampleLevel( LinearClamp, __Position.xy / _resolution, _debugMipIndex );
//			? _tex_SourceRadiance_PULL.mips[_debugMipIndex][__Position.xy]
//			: _tex_SourceRadiance_PUSH.mips[_debugMipIndex][__Position.xy];

//	if ( _debugMipIndex == 0 )
//		V.w = Depth2Weight( V.w );
//	return 0.05 * V.w;
//	return V.xyz / V.w;
	return V.xyz;
#endif

#if 0	// DEBUG SPLIT BUFFERS
	uint2	pixelPosition = uint2( floor( __Position.xy ) );

	#if 1
		// Recomposed buffers
		uint3	samplingPos = uint3( pixelPosition >> 2, ((pixelPosition.y & 3) << 2) | (pixelPosition.x & 3) );
//uint3	samplingPos = uint3( pixelPosition % (uint2(_resolution) >> 2), 0*uint(4*(4 * pixelPosition.y / _resolution.y) + (4 * pixelPosition.x / _resolution.x)) );
	#else
		// Individual buffers
		uint3	samplingPos = uint3( pixelPosition >> 2, _debugMipIndex );
	#endif

	// Source buffers
	float	splitZ = _tex_splitDepth[samplingPos];
	float3	splitN;
			splitN.xy = _tex_splitNormal[samplingPos];
			splitN.z = sqrt( 1.0 - dot(splitN.xy,splitN.xy) );
//	float3	splitL = _tex_splitRadiance.mips[_debugMipIndex][samplingPos].xyz;
	float3	splitL = _tex_splitRadiance[samplingPos].xyz;

	// Ugly resulting split buffers
	float3	splitE = _tex_splitIrradiance[samplingPos].xyz;
	float4	splitBentNormal = _tex_splitBentCone[samplingPos];



#if 0
	// Debug filter
//	float2	UV0 = _mouseUVs.zw;
	float2	UV0 = _mouseUVs.xy;
	uint2	ssCentralLocation = uint2( UV0 * _resolution );
	uint2	subPixel0 = ssCentralLocation & 3;
			ssCentralLocation >>= 2;
	uint3	samplingPos0 = uint3( ssCentralLocation, (subPixel0.y << 2) + subPixel0.x );
	float	_centralZ = _tex_splitDepth[samplingPos0];
	float3	_lcsCentralNormal;
	_lcsCentralNormal.xy = _tex_splitNormal[samplingPos0];
	_lcsCentralNormal.z = sqrt( 1.0 - dot( _lcsCentralNormal.xy, _lcsCentralNormal.xy ) );

//	float2	UV1 = _mouseUVs.xy;
	float2	UV1 = pixelPosition.xy / _resolution;
	uint2	ssCurrentLocation = uint2( UV1 * _resolution );
	uint2	subPixel1 = ssCurrentLocation & 3;
			ssCurrentLocation >>= 2;
	uint3	samplingPos1 = uint3( ssCurrentLocation, (subPixel1.y << 2) + subPixel1.x );
	float	_currentZ = _tex_splitDepth[samplingPos1];
	float3	lcsCurrentNormal;
	lcsCurrentNormal.xy = _tex_splitNormal[samplingPos1];
	lcsCurrentNormal.z = sqrt( 1.0 - dot( lcsCurrentNormal.xy, lcsCurrentNormal.xy ) );

	////////////////////////////////////////////////////////////////////////////////////
	float3	csView0 = BuildCameraRay( UV0 );
	float3	csPos0 = _centralZ * csView0;
			csView0 = normalize( csView0 );
	float3	csAt0 = -csView0;
	float3	csRight0 = normalize( cross( csAt0, float3( 0, 1, 0 ) ) );
	float3	csUp0 = cross( csRight0, csAt0 );
	float3	csNormal0 = _lcsCentralNormal.x * csRight0 + _lcsCentralNormal.y * csUp0 + _lcsCentralNormal.z * csAt0;

	float3	csView1 = BuildCameraRay( UV1 );
	float3	csPos1 = _currentZ * csView1;
			csView1 = normalize( csView1 );
	float3	csAt1 = -csView1;
	float3	csRight1 = normalize( cross( csAt1, float3( 0, 1, 0 ) ) );
	float3	csUp1 = cross( csRight1, csAt1 );
	float3	csNormal1 = lcsCurrentNormal.x * csRight1 + lcsCurrentNormal.y * csUp1 + lcsCurrentNormal.z * csAt1;



//float3	wsView1 = mul( float4( csView1, 0.0 ), _camera2World ).xyz;
//float3	wsRight1 = normalize( cross( wsView1, _camera2World[1].xyz ) );
//float3	wsUp1 = cross( wsRight1, wsView1 );
//float3	wsAt1 = -wsView1;
//return wsUp1;
//return wsRight1;
//return wsView1;

//return _centralZ;
//return _currentZ;
//return float3( UV1, 0 );
//return csNormal1;

	// Our criterion is that current position and normal must see our central position to contribute...
	float3	csToCentralPosition = csPos0 - csPos1;
	float	distance2CentralPosition = length( csToCentralPosition );
			csToCentralPosition /= distance2CentralPosition;

	const float2	toleranceMin = float2( -0.01, -0.02 );
	const float2	toleranceMax = float2( -0.2, -0.4 );
	float	verticality = smoothstep( 0.2, 0.8, saturate( -dot( csToCentralPosition, csNormal0 ) ) );
	float2	tolerance = lerp( toleranceMin, toleranceMax, verticality );	// We grow more tolerant

//return -tolerance.x;
return verticality;
return saturate( -dot( csToCentralPosition, csNormal0 ) );

	float	fade = smoothstep( tolerance.y, tolerance.x, dot( csToCentralPosition, csNormal1 ) );
	float	radianceFade = 1.0
						 - saturate( 0.5 * distance2CentralPosition )	// Dot product fade out is fully effective after 2 meters
						 * pow2( 1.0 - saturate( dot( csToCentralPosition, csNormal1 ) ) );

//return distance2CentralPosition;
//return dot( csToCentralPosition, csNormal1 );
	return fade;
return smoothstep( -_debugValues.y, -_debugValues.x, dot( csToCentralPosition, csNormal1 ) );
return smoothstep( 2*_debugValues.y-1, 2*_debugValues.x-1, dot( csToCentralPosition, csNormal1 ) );

#endif



//	return 4.0 * splitZ;
//	return 0.5 * (1.0 + splitN);
	return float3( 0.5 * (1.0 + splitN.xy), 0 );
//	return splitL;

	// Results
//	return splitE;
//	return splitBentNormal.w;	// AO
	return 0.5 * (1.0 + splitBentNormal.xyz);	// Bent normal
#endif


//return 1.0 * _tex_ShadowMap.SampleLevel( LinearClamp, float3( __Position.xy / _resolution, _debugMipIndex ), 0.0 );	// Debug shadow map
//return 1.0 * _tex_ShadowMapDirectional.SampleLevel( LinearClamp, float3( __Position.xy / _resolution, 0 ), 0.0 );	// Debug shadow map

// Show G-Buffer
//return _tex_Emissive[__Position.xy].xyz;
//return _tex_Radiance1[__Position.xy].xyz;
//return _tex_Radiance0[__Position.xy].xyz;
//return _tex_Albedo[__Position.xy].xyz;
//return _tex_Albedo[__Position.xy].w;	// Show F0
return _tex_Normal[__Position.xy].xyz;
//return _tex_Normal[__Position.xy].w;	// Show roughness
return 0.5 * (1.0 + _tex_Normal[__Position.xy].xyz);

//return 1.0 * _tex_Depth.SampleLevel( LinearClamp, __Position.xy / _resolution, _debugMipIndex );

//float	Zproj = _tex_DepthStencil[__Position.xy];
//float	Z_NEAR = 0.01;
//float	Q = Z_FAR / (Z_FAR - Z_NEAR);
//return 0.1 * (-Q * Z_NEAR / (Zproj - Q));

return 4.0 * _tex_Depth.mips[_debugMipIndex][__Position.xy];
//return _tex_MotionVectors_Scatter[__Position.xy];
return float3( abs( _tex_MotionVectors_Gather[__Position.xy] ), 0 );
return float3( 0.5 * (1.0+_tex_MotionVectors_Gather[__Position.xy]), 0 );
}

