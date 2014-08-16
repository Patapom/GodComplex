using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using WMath;

namespace OfflineCloudRenderer
{
	public struct	float2
	{
		public float	x, y;
		public void	FromVector2( Vector2D a )	{ x = a.x; y = a.y; }
	}

	public struct	float3
	{
		public float	x, y, z;
		public void	FromVector3( Vector a )		{ x = a.x; y = a.y; z = a.z; }
	}

	public struct	float4
	{
		public float	x, y, z, w;
		public void	FromVector4( Vector4D a )	{ x = a.x; y = a.y; z = a.z; w = a.w; }
	}

	public struct	float4x4
	{
		public float4	r0;
		public float4	r1;
		public float4	r2;
		public float4	r3;
		public void	FromMatrix4( Matrix4x4 a )	{ r0.FromVector4( a.GetRow0() ); r1.FromVector4( a.GetRow1() ); r2.FromVector4( a.GetRow2() ); r3.FromVector4( a.GetRow3() ); }
	}
}
