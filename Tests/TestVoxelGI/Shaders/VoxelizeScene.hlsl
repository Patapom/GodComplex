//////////////////////////////////////////////////////////////////////////////////////////////
// Voxelizes a distance field scene
//////////////////////////////////////////////////////////////////////////////////////////////
//
#include "Global.hlsl"
#include "DistanceField.hlsl"
#include "Voxel.hlsl"

RWTexture3D< float4 >	_Tex_VoxelScene_Albedo : register(u0);
RWTexture3D< float4 >	_Tex_VoxelScene_Normal : register(u1);
RWTexture3D< float4 >	_Tex_VoxelScene_Lighting : register(u2);

[numthreads( 16, 16, 1 )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {

	uint3	voxelIndex = _DispatchThreadID;
	float3	wsVoxelCenter = VOXEL_MIN + (voxelIndex + 0.5) * VOXEL_SIZE;

	// Estimate distance field at center position
	float2	distance = Map( wsVoxelCenter );
	if ( abs( distance.x ) > 0.5 * VOXEL_DIAG_SIZE ) {
		// Empty
		_Tex_VoxelScene_Albedo[voxelIndex] = 0.0;
//		_Tex_VoxelScene_Normal[voxelIndex] = float4( 0.5.xxx, 0 );	// UNORM
		_Tex_VoxelScene_Normal[voxelIndex] = float4( 0.0.xxx, 0 );	// SNORM
		_Tex_VoxelScene_Lighting[voxelIndex] = 0.0;
		return;
	}

	// Estimate albedo and normal
	float3	albedo = Albedo( wsVoxelCenter, distance.y );
			albedo = sqrt( albedo );		// Store as sqrt() to simulate sRGB (sRGB is forbidden when buffer is set as UAV)
	float3	wsNormal = Normal( wsVoxelCenter );
	_Tex_VoxelScene_Albedo[voxelIndex] = float4( albedo, 1.0 );
//	_Tex_VoxelScene_Normal[voxelIndex] = float4( 0.5 * (1.0 + wsNormal ), 0 );	// UNORM
	_Tex_VoxelScene_Normal[voxelIndex] = float4( wsNormal, 0 );					// SNORM

	// Compute lighting
	float3	wsLightPos = CORNELL_LIGHT_POS;
	float3	wsScenePos = wsVoxelCenter + 0.5 * VOXEL_SIZE * wsNormal;	// TODO: Properly offset position to the surface of the voxel
	float3	wsLight = wsLightPos - wsScenePos;
	float	distance2Light = length( wsLight );
			wsLight /= distance2Light;

	float2	shadowDistance = Trace( wsScenePos, wsLight, 0.0, 100 );
	float	shadow = smoothstep( 0.95, 1.0, shadowDistance.x / distance2Light );
			shadow *= saturate( wsLight.y );	// saturate( -dot( wsLight, float3( 0, -1, 0 ) ) ) assuming the light is emitting toward the bottom

	// Compute lighting
	float3	sceneRadiance = (INVPI * albedo) * saturate( dot( wsNormal, wsLight ) ) * shadow * LIGHT_ILLUMINANCE / (distance2Light * distance2Light);
	_Tex_VoxelScene_Lighting[voxelIndex] = float4( sceneRadiance, 0.0 );
}

Texture3D< float4 >		_Tex_SourceVoxelScene_Albedo : register(t0);
Texture3D< float4 >		_Tex_SourceVoxelScene_Normal : register(t1);
Texture3D< float4 >		_Tex_SourceVoxelScene_Lighting : register(t2);

[numthreads( 16, 16, 1 )]
void	CS_Mip( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID, uint3 _DispatchThreadID : SV_DISPATCHTHREADID ) {
	uint3	targetVoxelIndex = _DispatchThreadID;
	uint3	sourceVoxelIndex = targetVoxelIndex << 1;

	// Process pre-multiplied albedo with care since we encoded some sort of sRGB curve
	float4	albedo000 = _Tex_SourceVoxelScene_Albedo[sourceVoxelIndex];	sourceVoxelIndex.x++;
	float4	albedo001 = _Tex_SourceVoxelScene_Albedo[sourceVoxelIndex];	sourceVoxelIndex.y++;
	float4	albedo011 = _Tex_SourceVoxelScene_Albedo[sourceVoxelIndex];	sourceVoxelIndex.x--;
	float4	albedo010 = _Tex_SourceVoxelScene_Albedo[sourceVoxelIndex];	sourceVoxelIndex.y--; 	sourceVoxelIndex.z++;
	float4	albedo100 = _Tex_SourceVoxelScene_Albedo[sourceVoxelIndex];	sourceVoxelIndex.x++;
	float4	albedo101 = _Tex_SourceVoxelScene_Albedo[sourceVoxelIndex];	sourceVoxelIndex.y++;
	float4	albedo111 = _Tex_SourceVoxelScene_Albedo[sourceVoxelIndex];	sourceVoxelIndex.x--;
	float4	albedo110 = _Tex_SourceVoxelScene_Albedo[sourceVoxelIndex]; sourceVoxelIndex.y--; 	sourceVoxelIndex.z--;
	float	invAlpha000 = albedo000.w > 0.0 ? 1.0 / albedo000.w : 0.0;
	float	invAlpha001 = albedo001.w > 0.0 ? 1.0 / albedo001.w : 0.0;
	float	invAlpha011 = albedo011.w > 0.0 ? 1.0 / albedo011.w : 0.0;
	float	invAlpha010 = albedo010.w > 0.0 ? 1.0 / albedo010.w : 0.0;
	float	invAlpha100 = albedo100.w > 0.0 ? 1.0 / albedo100.w : 0.0;
	float	invAlpha101 = albedo101.w > 0.0 ? 1.0 / albedo101.w : 0.0;
	float	invAlpha111 = albedo111.w > 0.0 ? 1.0 / albedo111.w : 0.0;
	float	invAlpha110 = albedo110.w > 0.0 ? 1.0 / albedo110.w : 0.0;
		// Un-premultiply
	albedo000.xyz *= invAlpha000;
	albedo001.xyz *= invAlpha001;
	albedo011.xyz *= invAlpha011;
	albedo010.xyz *= invAlpha010;
	albedo100.xyz *= invAlpha100;
	albedo101.xyz *= invAlpha101;
	albedo111.xyz *= invAlpha111;
	albedo110.xyz *= invAlpha110;
		// Un-sRGB
	albedo000.xyz *= albedo000.xyz;
	albedo001.xyz *= albedo001.xyz;
	albedo011.xyz *= albedo011.xyz;
	albedo010.xyz *= albedo010.xyz;
	albedo100.xyz *= albedo100.xyz;
	albedo101.xyz *= albedo101.xyz;
	albedo111.xyz *= albedo111.xyz;
	albedo110.xyz *= albedo110.xyz;
	float4	albedo = 0.125 * (albedo000 + albedo001 + albedo011 + albedo010 + albedo100 + albedo101 + albedo111 + albedo110);
			albedo.xyz = sqrt( albedo.xyz );	// Re-sRGB
			albedo.xyz *= albedo.w;				// Re-premultiply

	_Tex_VoxelScene_Albedo[targetVoxelIndex] = albedo;

	// Process pre-multiplied normal vectors
	float4	normal000 = _Tex_SourceVoxelScene_Normal[sourceVoxelIndex];	sourceVoxelIndex.x++;
	float4	normal001 = _Tex_SourceVoxelScene_Normal[sourceVoxelIndex];	sourceVoxelIndex.y++;
	float4	normal011 = _Tex_SourceVoxelScene_Normal[sourceVoxelIndex];	sourceVoxelIndex.x--;
	float4	normal010 = _Tex_SourceVoxelScene_Normal[sourceVoxelIndex];	sourceVoxelIndex.y--; 	sourceVoxelIndex.z++;
	float4	normal100 = _Tex_SourceVoxelScene_Normal[sourceVoxelIndex];	sourceVoxelIndex.x++;
	float4	normal101 = _Tex_SourceVoxelScene_Normal[sourceVoxelIndex];	sourceVoxelIndex.y++;
	float4	normal111 = _Tex_SourceVoxelScene_Normal[sourceVoxelIndex];	sourceVoxelIndex.x--;
	float4	normal110 = _Tex_SourceVoxelScene_Normal[sourceVoxelIndex]; sourceVoxelIndex.y--; 	sourceVoxelIndex.z--;

// 		// Un-pack (UNORM)
// 	normal000.xyz = 2.0 * normal000.xyz - 1.0;
// 	normal001.xyz = 2.0 * normal001.xyz - 1.0;
// 	normal011.xyz = 2.0 * normal011.xyz - 1.0;
// 	normal010.xyz = 2.0 * normal010.xyz - 1.0;
// 	normal100.xyz = 2.0 * normal100.xyz - 1.0;
// 	normal101.xyz = 2.0 * normal101.xyz - 1.0;
// 	normal111.xyz = 2.0 * normal111.xyz - 1.0;
// 	normal110.xyz = 2.0 * normal110.xyz - 1.0;
		// Un-premultiply
	normal000 *= invAlpha000;
	normal001 *= invAlpha001;
	normal011 *= invAlpha011;
	normal010 *= invAlpha010;
	normal100 *= invAlpha100;
	normal101 *= invAlpha101;
	normal111 *= invAlpha111;
	normal110 *= invAlpha110;

	float4	normal = 0.125 * (normal000 + normal001 + normal011 + normal010 + normal100 + normal101 + normal111 + normal110);
	float	normalLength = length( normal.xyz );
			normal.xyz *= normalLength > 0.0 ? 1.0 / normalLength : 0.0;
			normal *= albedo.w;						// Re-premultiply
//			normal.xyz = 0.5 * (1.0 + normal.xyz);	// Re-pack (UNORM)

	_Tex_VoxelScene_Normal[targetVoxelIndex] = normal;

	// Average lighting
	float4	lighting000 = _Tex_SourceVoxelScene_Lighting[sourceVoxelIndex];	sourceVoxelIndex.x++;
	float4	lighting001 = _Tex_SourceVoxelScene_Lighting[sourceVoxelIndex];	sourceVoxelIndex.y++;
	float4	lighting011 = _Tex_SourceVoxelScene_Lighting[sourceVoxelIndex];	sourceVoxelIndex.x--;
	float4	lighting010 = _Tex_SourceVoxelScene_Lighting[sourceVoxelIndex];	sourceVoxelIndex.y--; 	sourceVoxelIndex.z++;
	float4	lighting100 = _Tex_SourceVoxelScene_Lighting[sourceVoxelIndex];	sourceVoxelIndex.x++;
	float4	lighting101 = _Tex_SourceVoxelScene_Lighting[sourceVoxelIndex];	sourceVoxelIndex.y++;
	float4	lighting111 = _Tex_SourceVoxelScene_Lighting[sourceVoxelIndex];	sourceVoxelIndex.x--;
	float4	lighting110 = _Tex_SourceVoxelScene_Lighting[sourceVoxelIndex];	sourceVoxelIndex.y--; 	sourceVoxelIndex.z--;
	_Tex_VoxelScene_Lighting[targetVoxelIndex] = 0.125 * (lighting000 + lighting001 + lighting011 + lighting010 + lighting100 + lighting101 + lighting111 + lighting110);

// _Tex_VoxelScene_Lighting[targetVoxelIndex] = float4( targetVoxelIndex / 63.0, 0 );
}