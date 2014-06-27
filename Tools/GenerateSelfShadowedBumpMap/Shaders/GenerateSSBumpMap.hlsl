////////////////////////////////////////////////////////////////////////////////
// SSBumpMap generator
// This compute shader will generate the directional and ambient occlusion over a specific texel
//	and store the result into a target UAV
////////////////////////////////////////////////////////////////////////////////

cbuffer	CBInput : register( b0 )
{

}

Texture2D<float>		Source : register( t0 );
RWTexture2D<float4>		Target : register( u0 );

[numthreads( 1024, 1, 1 )]
void	CS( uint3 _GroupID : SV_GROUPID, uint3 _GroupThreadID : SV_GROUPTHREADID )
{
	if ( _GroupThreadID.x == 0 )
	{
		float	H0 = Source.Load( int3( _GroupID.xy, 0 ) );
		Target[_GroupID.xy] = float4( H0 * float3( 1, 0.5, 0.25 ), 1 );
	}
}