#include "Includes/global.hlsl"
#include "Includes/DistanceFieldHelpers.hlsl"

Texture2DArray<float4>	_TexSource : register(t0);

static const float	TAN_HALF_FOV = 0.57735026918962576450914878050196;	// tan( 60° / 2 )


//----------------------------------------------------------------------

float2 map( in float3 pos ) {

	float2 res = float2( INFINITY, -1 );

    res = opU( res, float2( sdEllipsoid( pos-float3( -1.0,0.35,-2.0), float3(0.15, 0.2, 0.05) ), 28 ) );

	res = opU( res, float2( 0.7*sdSphere(    pos-float3(-2.0,0.25,-2.0), 0.2 ) + 
										     0.03*sin(50.0*pos.x)*sin(50.0*pos.y)*sin(50.0*pos.z), 
											39.37 ) );
        
    return res;
}

float4 render( in float2 _pixelPos, in float _sceneDistance, in float3 ro, in float3 rd, bool _useModel, out float _distance ) { 
	float2	res = castRay( ro, rd );
	float	t = res.x;
	float	m = res.y;
	_distance = t;
	if ( m <= -0.5 || t >= _sceneDistance ) {
		return float4( 0, 0, 0, 1 );
	}

	float3 pos = ro + t*rd;
	float3 nor = calcNormal( pos );
	float3 ref = reflect( rd, nor );
        
	// material        
	float3	albedo = 0.45 + 0.4*cos( float3(0.08,0.05,0.1)*(m-1.0) );
	float	roughness = 0.5 + 0.5 * sin( 0.1 * (m-1.0) );
		
	if ( m < 1.5 ) {
		// Floor
		float f = abs( fmod( floor(5.0*pos.z) + floor(5.0*pos.x), 2.0 ) );
		albedo = 0.4 + 0.1*f;
		roughness = 0.4 + 0.6 * abs( f );
	}

	// lighting        
	float	occ = calcAO( pos, nor );
	float3	lig = normalize( float3(-0.6, 0.7, -0.5) );
	float3	lig2 = normalize(float3(-lig.x,0.0,-lig.z));	// For backlighting
	float3	lightIntensity = 1.2 * float3(1.0,0.85,0.55);
	float3	lightIntensity2 = 0.3;// * float3(1.0,0.85,0.55);

	float	amb = saturate( 0.5+0.5*nor.y );
	float	LdotN = saturate( dot( nor, lig ) );
	float	LdotN2 = saturate( dot( nor, lig2 ) ) * saturate( 1.0-pos.y );
	float	dom = smoothstep( -0.1, 0.1, ref.y );
	float	fre = pow( saturate( 1.0+dot(nor,rd) ), 2.0 );
	float	spe = pow( saturate( dot( ref, lig ) ), 16.0 );
        
	float	shadow = softshadow( pos, lig, 0.02, 2.5 );
	float	shadow_ref = softshadow( pos, ref, 0.02, 2.5 );
	dom *= shadow_ref;

	// Add rough diffuse model
	float3	dif2 = 0.0;
//	if ( _useModel ) {
//
//		roughness *= _DebugParm;
//		roughness = max( 1e-3, roughness );
//
//		float	shadow2 = lerp( 1.0, shadow, saturate( 10.0 * (LdotN-0.2) ) );	// This removes shadowing on back faces
//				shadow2 = 1.0 - shadow2;
//				shadow2 = pow2( shadow2 );	// ^2
//				shadow2 = pow2( shadow2 );	// ^4
//				shadow2 = pow2( shadow2 );	// ^8
//				shadow2 = pow2( shadow2 );	// ^16
//				shadow2 = 1.0 - shadow2;
////				shadow2 *= saturate( 0.2 + 0.8 * LdotN );	// Larger L.N, eating into the backfaces
//
//		dif2 = ComputeDiffuseModel( lig, -rd, roughness, albedo ) * lightIntensity * shadow * LdotN;
//
//		// 2nd light
//		shadow2 = lerp( 1.0, shadow, saturate( 10.0 * (LdotN2-0.2) ) );	// This removes shadowing on back faces
//		shadow2 = 1.0 - shadow2;
//		shadow2 = pow2( shadow2 );	// ^2
//		shadow2 = pow2( shadow2 );	// ^4
//		shadow2 = pow2( shadow2 );	// ^8
//		shadow2 = pow2( shadow2 );	// ^16
//		shadow2 = 1.0 - shadow2;
////		shadow2 *= saturate( 0.2 + 0.8 * LdotN2 );	// Larger L.N, eating into the backfaces
//
//		dif2 += ComputeDiffuseModel( lig2, -ref, roughness, albedo ) * lightIntensity2 * shadow * LdotN2;
//	}

	float3 lin = float3(0,0,0);
	lin += lightIntensity * shadow * LdotN;
	lin += lightIntensity * shadow * spe * LdotN;
	lin += 0.20*amb*float3(0.50,0.70,1.00)*occ;
	lin += 0.30*dom*float3(0.50,0.70,1.00)*occ;
	lin += lightIntensity2 * LdotN2*float3(0.25,0.25,0.25)*occ;
	lin += 0.40*fre*float3(1.00,1.00,1.00)*occ;

	float3	col = 0;//float3(0.7, 0.9, 1.0) + rd.y*0.8;	// Sky color
	col = albedo * lin;

	col += dif2;

	// Add some fog
	col = lerp( col, float3(0.8,0.9,1.0), 1.0-exp( -0.002*t*t ) );

if ( _DebugFlags & 8 ) {
	col = dif2;
} else if ( _DebugFlags & 1 ) {
	col = roughness;
}

	return float4( saturate( col ), 0.0 );
}

float3	PS( VS_IN _In ) : SV_TARGET0 {
	float2	UV = _In.__Position.xy / iResolution.xy;

	#if 1
		float	AspectRatio = iResolution.x / iResolution.y;
		float	pixelRadius = 2.0 * SQRT2 * TAN_HALF_FOV / iResolution.y;
		float3	csView = normalize( float3( AspectRatio * TAN_HALF_FOV * (2.0 * UV.x - 1.0), TAN_HALF_FOV * (1.0 - 2.0 * UV.y), 1.0 ) );
		float3	ro = _Camera2World[3].xyz;
		float3	rd = mul( float4( csView, 0.0 ), _Camera2World ).xyz;
	#else
		float2	p = -1.0+2.0*UV;
				p.x *= iResolution.x / iResolution.y;

		float2 mo = 0;//iMouse.xy / iResolution.xy;
		 
		float time = 15.0 + iGlobalTime;

		// camera	
		float3 ro = float3( -0.5+3.5*cos(0.1*time + 6.0*mo.x), 1.0 + 2.0*mo.y, 0.5 + 3.5*sin(0.1*time + 6.0*mo.x) );
		float3 ta = float3( -0.5, -0.4, 0.5 );
	
		// camera-to-world transformation
		float3x3 ca = setCamera( ro, ta, 0.0 );

		// ray direction
		float3	rd = mul( normalize( float3(p.x, -p.y, 2.0) ), ca );
	#endif

	bool	useModel = _DebugFlags & 2;// && UV.x > _MousePosition.x;


	float	backgroundDistance = _TexSource[uint3( _In.__Position.xy, 1 )].x;
	float3	backgroundColor = _TexSource[uint3( _In.__Position.xy, 0 )].xyz;

	float	distance;
	float4	Color = render( _In.__Position.xy, backgroundDistance, ro, rd, useModel, distance );

	return backgroundColor.xyz * Color.w + Color.xyz;
}
