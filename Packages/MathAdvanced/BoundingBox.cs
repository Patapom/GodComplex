using System;

namespace SharpMath
{
/// <summary>
/// Here is the orientation of the bouding-box in its own radix:
///                                                              
///         o--------o                                               
///        /:       /|          Y (TOP)                          
///       / :      / |          |                                    
///      o--------M  |          |                    M is the MAX component
///      |  :     |  |          |                    m is the MIN component
///      |  m.....|..o          o------X (RIGHT)                     
///      | '      | /          /                                     
///      |'       |/          /                                      
///      o--------o          Z (FRONT)                               
///
///
/// </summary>
    [System.Diagnostics.DebuggerDisplay("Min = ({m_Min.x}, {m_Min.y}, {m_Min.z}) Max = ({m_Max.x}, {m_Max.y}, {m_Max.z})")]
    public class BoundingBox
	{
		#region	CONSTANTS

		internal static float	EPSILON = float.Epsilon;	// Use the Global class to modify this epsilon

		#endregion

		#region	FIELDS

		public float3				m_Min, m_Max;

		#endregion

		#region PROPERTIES

		public float			DimX
		{
			get { return m_Max.x - m_Min.x; }
		}

		public float			DimY
		{
			get { return m_Max.y - m_Min.y; }
		}

		public float			DimZ
		{
			get { return m_Max.z - m_Min.z; }
		}

		public float3			Dim
		{
			get { return new float3( m_Max.x - m_Min.x, m_Max.y - m_Min.y, m_Max.z - m_Min.z ); }
		}

		public float3			Center
		{
			get { return new float3( .5f * (m_Min.x + m_Max.x), .5f * (m_Min.y + m_Max.y), .5f * (m_Min.z + m_Max.z) ); }
		}

		public static BoundingBox	Empty
		{
			get { return new BoundingBox( +float.MaxValue, +float.MaxValue, +float.MaxValue, -float.MaxValue, -float.MaxValue, -float.MaxValue ); }
		}

		#endregion

		#region	METHODS

		// Constructors/Destructor
		public					BoundingBox	()																					{ m_Min = new float3(); m_Max = new float3(); }
		public					BoundingBox	( float _MinX, float _MinY, float _MinZ, float _MaxX, float _MaxY, float _MaxZ )	{ m_Min = new float3( _MinX, _MinY, _MinZ ); m_Max = new float3( _MaxX, _MaxY, _MaxZ ); }
		public					BoundingBox	( float3 _Min, float3 _Max ) 															{ m_Min = _Min; m_Max = _Max; }

		public override string		ToString()
		{
			return	"[" + m_Min + "] - [" + m_Max + "]";
		}

		public BoundingBox		Set			( float3 _Min, float3 _Max )			{ m_Min = _Min; m_Max = _Max; return this; }

		// Helpers
		public void				Grow( float3 _GrowSize )						{ m_Min -= .5f * _GrowSize; m_Max += .5f * _GrowSize; }
		public void				Add( float3 _Point )							{ m_Min.Min( _Point ); m_Max.Max( _Point ); }

			// Grows the current bbox using another transformed bbox
		public void				Grow( BoundingBox _BBox, float4x4 _Transform )
		{
			float3[]	Corners = _BBox.Transform( _Transform );
			for ( int CornerIndex=0; CornerIndex < 8; CornerIndex++ )
				Grow( Corners[CornerIndex] );
		}

		// Transforms the box by the given matrix and returns 8 points corresponding to the eight transformed corners
		public float3[]			Transform( float4x4 _Transform )
		{
			float3[]	Corners = new float3[8]
			{
				new float3( 0, 0, 0 ),
				new float3( 1, 0, 0 ),
				new float3( 1, 1, 0 ),
				new float3( 0, 1, 0 ),
				new float3( 0, 0, 1 ),
				new float3( 1, 0, 1 ),
				new float3( 1, 1, 1 ),
				new float3( 0, 1, 1 ),
			};

			float3	D = Dim;

			float3[]	Result = new float3[8];
			for ( int CornerIndex=0; CornerIndex < 8; CornerIndex++ )
				Result[CornerIndex] = (float3) (new float4( m_Min + Corners[CornerIndex] * D, 1.0f ) * _Transform);

			return	Result;
		}

		public bool				IsHitBy( Ray _Ray )
		{
			// ********** TEST RAY INTERSECTION **********
			bool	bHitX = false, bHitY = false, bHitZ = false;
			float	fDistX = float.MaxValue, fDistY = float.MaxValue, fDistZ = float.MaxValue;
			float3	HitPosition;

			// Left  right intersection
			if ( System.Math.Abs( _Ray.Aim.x ) > EPSILON )
			{
				fDistX = ( _Ray.Aim.x > 0.0f ? System.Math.Min( m_Max.x - _Ray.Pos.x, m_Min.x - _Ray.Pos.x ) : System.Math.Max( m_Max.x - _Ray.Pos.x, m_Min.x - _Ray.Pos.x ) ) / _Ray.Aim.x;
				if ( fDistX >= 0.0f )
				{
					HitPosition = _Ray.Pos + _Ray.Aim * fDistX;
					if ( HitPosition.y >= m_Min.y && HitPosition.y <= m_Max.y && HitPosition.z >= m_Min.z && HitPosition.z <= m_Max.z )
						bHitX = true;
				}
			}

			// Top  bottom intersection
			if ( System.Math.Abs( _Ray.Aim.y ) > EPSILON )
			{
				fDistY = ( _Ray.Aim.y > 0.0f ? System.Math.Min( m_Max.y - _Ray.Pos.y, m_Min.y - _Ray.Pos.y ) : System.Math.Max( m_Max.y - _Ray.Pos.y, m_Min.y - _Ray.Pos. y) ) / _Ray.Aim.y;
				if ( fDistY >= 0.0f )
				{
					HitPosition = _Ray.Pos + _Ray.Aim * fDistY;
					if ( HitPosition.x >= m_Min.x && HitPosition.x <= m_Max.x && HitPosition.z >= m_Min.z && HitPosition.z <= m_Max.z )
						bHitY = true;
				}
			}

			// Front  back intersection
			if ( System.Math.Abs( _Ray.Aim.z ) > EPSILON )
			{
				fDistZ = ( _Ray.Aim.z > 0.0f ? System.Math.Min( m_Max.z - _Ray.Pos.z, m_Min.z - _Ray.Pos.z ) : System.Math.Max( m_Max.z - _Ray.Pos.z, m_Min.z - _Ray.Pos.z ) ) / _Ray.Aim.z;
				if ( fDistZ >= 0.0f )
				{
					HitPosition = _Ray.Pos + _Ray.Aim * fDistZ;
					if ( HitPosition.x >= m_Min.x && HitPosition.x <= m_Max.x && HitPosition.y >= m_Min.y && HitPosition.y <= m_Max.y )
						bHitZ = true;
				}
			}

			if ( !bHitX && !bHitY && !bHitZ )
				return	false;

			if ( fDistX < fDistY )
			{
				if ( fDistX < fDistZ )
				{
					if ( fDistX > _Ray.Length )
						return	false;		// Ray is too short!
					_Ray.Length = fDistX;
					_Ray.Datum = _Ray.Aim.x > 0.0f ? 1 : 0;
				}
				else
				{
					if ( fDistZ > _Ray.Length )
						return	false;		// Ray is too short!
					_Ray.Length = fDistZ;
					_Ray.Datum = _Ray.Aim.z > 0.0f ? 5 : 4;
				}
			}
			else
			{
				if ( fDistY < fDistZ )
				{
					if ( fDistY > _Ray.Length )
						return	false;		// Ray is too short!
					_Ray.Length = fDistY;
					_Ray.Datum = _Ray.Aim.y > 0.0f ? 3 : 2;
				}
				else
				{
					if ( fDistZ > _Ray.Length )
						return	false;		// Ray is too short!
					_Ray.Length = fDistZ;
					_Ray.Datum = _Ray.Aim.z > 0.0f ? 5 : 4;
				}
			}
		
			return	true;
		}
		public bool				Intersect( Ray _Ray )
		{
			// ********** TEST RAY INTERSECTION (ONLY THE FACT OF INTERSECTION IS IMPORTANT THIS TIME) **********
			float	fDistX = float.MaxValue, fDistY = float.MaxValue, fDistZ = float.MaxValue;
			float3	HitPosition;

			// Left  right intersection
			if ( System.Math.Abs( _Ray.Aim.x ) > EPSILON )
			{
				fDistX = ( _Ray.Aim.x > 0.0f ? System.Math.Min( m_Max.x - _Ray.Pos.x, m_Min.x - _Ray.Pos.x ) : System.Math.Max( m_Max.x - _Ray.Pos.x, m_Min.x - _Ray.Pos.x ) ) / _Ray.Aim.x;
				if ( fDistX >= 0.0f )
				{
					HitPosition = _Ray.Pos + _Ray.Aim * fDistX;
					if ( HitPosition.y >= m_Min.y && HitPosition.y <= m_Max.y && HitPosition.z >= m_Min.z && HitPosition.z <= m_Max.z )
						return	true;
				}
			}

			// Top  bottom intersection
			if ( System.Math.Abs( _Ray.Aim.y ) > EPSILON )
			{
				fDistY = ( _Ray.Aim.y > 0.0f ? System.Math.Min( m_Max.y - _Ray.Pos.y, m_Min.y - _Ray.Pos.y ) : System.Math.Max( m_Max.y - _Ray.Pos.y, m_Min.y - _Ray.Pos. y) ) / _Ray.Aim.y;
				if ( fDistY >= 0.0f )
				{
					HitPosition = _Ray.Pos + _Ray.Aim * fDistY;
					if ( HitPosition.x >= m_Min.x && HitPosition.x <= m_Max.x && HitPosition.z >= m_Min.z && HitPosition.z <= m_Max.z )
						return	true;
				}
			}

			// Front  back intersection
			if ( System.Math.Abs( _Ray.Aim.z ) > EPSILON )
			{
				fDistZ = ( _Ray.Aim.z > 0.0f ? System.Math.Min( m_Max.z - _Ray.Pos.z, m_Min.z - _Ray.Pos.z ) : System.Math.Max( m_Max.z - _Ray.Pos.z, m_Min.z - _Ray.Pos.z ) ) / _Ray.Aim.z;
				if ( fDistZ >= 0.0f )
				{
					HitPosition = _Ray.Pos + _Ray.Aim * fDistZ;
					if ( HitPosition.x >= m_Min.x && HitPosition.x <= m_Max.x && HitPosition.y >= m_Min.y && HitPosition.y <= m_Max.y )
						return	true;
				}
			}

			return	false;
		}
		public bool				IsOutsideHitBy( Ray _Ray )
		{
			float	fDistance;
			float3	HitPosition;

			// Left | right intersection
			if ( System.Math.Abs( _Ray.Aim.x ) > EPSILON )
			{
				fDistance = ( _Ray.Aim.x > 0.0f ? m_Min.x - _Ray.Pos.x : m_Max.x - _Ray.Pos.x ) / _Ray.Aim.x;
				if ( fDistance >= 0.0f && fDistance <= _Ray.Length )
				{
					HitPosition = _Ray.Pos + _Ray.Aim * fDistance;
					if ( HitPosition.y >= m_Min.y && HitPosition.y <= m_Max.y && HitPosition.z >= m_Min.z && HitPosition.z <= m_Max.z )
					{
						_Ray.Length = fDistance;
						_Ray.Datum = _Ray.Aim.x > 0.0f ? 0 : 1;

						return	true;
					}
				}
				else
					_Ray.Datum = 6;			// We notify the caller it didn't hit because of distance
			}

			// Top | bottom intersection
			if ( System.Math.Abs( _Ray.Aim.y ) > EPSILON )
			{
				fDistance = ( _Ray.Aim.y > 0.0f ? m_Min.y - _Ray.Pos.y : m_Max.y - _Ray.Pos.y ) / _Ray.Aim.y;
				if ( fDistance >= 0.0f && fDistance <= _Ray.Length )
				{
					HitPosition = _Ray.Pos + _Ray.Aim * fDistance;
					if ( HitPosition.x >= m_Min.x && HitPosition.x <= m_Max.x && HitPosition.z >= m_Min.z && HitPosition.z <= m_Max.z )
					{
						_Ray.Length = fDistance;
						_Ray.Datum = _Ray.Aim.y > 0.0f ? 2 : 3;

						return	true;
					}
				}
				else
					_Ray.Datum = 6;			// We notify the caller it didn't hit because of distance
			}

			// Front | back intersection
			if ( System.Math.Abs( _Ray.Aim.z ) > EPSILON )
			{
				fDistance = ( _Ray.Aim.z > 0.0f ? m_Min.z - _Ray.Pos.z : m_Max.z - _Ray.Pos.z ) / _Ray.Aim.z;
				if ( fDistance >= 0.0f && fDistance <= _Ray.Length )
				{
					HitPosition = _Ray.Pos + _Ray.Aim * fDistance;
					if ( HitPosition.x >= m_Min.x && HitPosition.x <= m_Max.x && HitPosition.y >= m_Min.y && HitPosition.y <= m_Max.y )
					{
						_Ray.Length = fDistance;
						_Ray.Datum = _Ray.Aim.z > 0.0f ? 4 : 5;

						return	true;
					}
				}
				else
					_Ray.Datum = 6;			// We notify the caller it didn't hit because of distance
			}

			return	false;
		}
		public bool				IsInsideHitBy( Ray _Ray )
		{
			float	fDistance;
			float3	HitPosition;

			// Left | right intersection
			if ( System.Math.Abs( _Ray.Aim.x ) > EPSILON )
			{
				fDistance = ( _Ray.Aim.x < 0.0f ? m_Min.x - _Ray.Pos.x : m_Max.x - _Ray.Pos.x ) / _Ray.Aim.x;
				if ( fDistance >= 0.0f && fDistance <= _Ray.Length )
				{
					HitPosition = _Ray.Pos + _Ray.Aim * fDistance;
					if ( HitPosition.y >= m_Min.y && HitPosition.y <= m_Max.y && HitPosition.z >= m_Min.z && HitPosition.z <= m_Max.z )
					{
						_Ray.Length = fDistance;
						_Ray.Datum = _Ray.Aim.x > 0.0f ? 1 : 0;

						return	true;
					}
				}
			}

			// Top | bottom intersection
			if ( System.Math.Abs( _Ray.Aim.y ) > EPSILON )
			{
				fDistance = ( _Ray.Aim.y < 0.0f ? m_Min.y - _Ray.Pos.y : m_Max.y - _Ray.Pos.y ) / _Ray.Aim.y;
				if ( fDistance >= 0.0f && fDistance <= _Ray.Length )
				{
					HitPosition = _Ray.Pos + _Ray.Aim * fDistance;
					if ( HitPosition.x >= m_Min.x && HitPosition.x <= m_Max.x && HitPosition.z >= m_Min.z && HitPosition.z <= m_Max.z )
					{
						_Ray.Length = fDistance;
						_Ray.Datum = _Ray.Aim.y > 0.0f ? 3 : 2;

						return	true;
					}
				}
			}

			// Front | back intersection
			if ( System.Math.Abs( _Ray.Aim.z ) > EPSILON )
			{
				fDistance = ( _Ray.Aim.z < 0.0f ? m_Min.z - _Ray.Pos.z : m_Max.z - _Ray.Pos.z ) / _Ray.Aim.z;
				if ( fDistance >= 0.0f && fDistance <= _Ray.Length )
				{
					HitPosition = _Ray.Pos + _Ray.Aim * fDistance;
					if ( HitPosition.x >= m_Min.x && HitPosition.x <= m_Max.x && HitPosition.y >= m_Min.y && HitPosition.y <= m_Max.y )
					{
						_Ray.Length = fDistance;
						_Ray.Datum = _Ray.Aim.z > 0.0f ? 5 : 4;

						return	true;
					}
				}
			}

			return	false;
		}
		public float3			Rationalize( float3 _Source ) {
			float3	D = _Source - m_Min;
			return new float3( D.x / (m_Max.x - m_Min.x), D.y / (m_Max.y - m_Min.y), D.z / (m_Max.z - m_Min.z) );
		}

		public float3[]			GetCorners()
		{
			float3[]	Result = new float3[8];
			for ( int i=0; i < 8; i++ )
			{
				Result[i] = m_Min + new float3( (i&1) * DimX, ((i>>1)&1) * DimY, ((i>>2)&1) * DimZ );
			}

			return Result;
		}

		// Arithmetic operators
		public static BoundingBox	operator+( BoundingBox _Op0, float3 _Op1 )							{ return new BoundingBox( _Op0.m_Min + _Op1, _Op0.m_Max + _Op1 ); }
		public static BoundingBox	operator*( BoundingBox _Op0, float _Op1 )							{ return new BoundingBox( _Op0.m_Min * _Op1, _Op0.m_Max * _Op1 ); }
		public static BoundingBox	operator/( BoundingBox _Op0, float _Op1 )							{ _Op1 = 1.0f / _Op1; return new BoundingBox( _Op0.m_Min * _Op1, _Op0.m_Max * _Op1 ); }
		public static BoundingBox	operator|( BoundingBox _Op0, BoundingBox _Op1 )						{ return new BoundingBox( System.Math.Min(_Op0.m_Min.x,_Op1.m_Min.x), System.Math.Min(_Op0.m_Min.y,_Op1.m_Min.y), System.Math.Min(_Op0.m_Min.z,_Op1.m_Min.z), System.Math.Max(_Op0.m_Max.x,_Op1.m_Max.x), System.Math.Max(_Op0.m_Max.y,_Op1.m_Max.y), System.Math.Max(_Op0.m_Max.z,_Op1.m_Max.z) ); }
		public static BoundingBox	operator&( BoundingBox _Op0, BoundingBox _Op1 )						{ return new BoundingBox( System.Math.Max(_Op0.m_Min.x,_Op1.m_Min.x), System.Math.Max(_Op0.m_Min.y,_Op1.m_Min.y), System.Math.Max(_Op0.m_Min.z,_Op1.m_Min.z), System.Math.Min(_Op0.m_Max.x,_Op1.m_Max.x), System.Math.Min(_Op0.m_Max.y,_Op1.m_Max.y), System.Math.Min(_Op0.m_Max.z,_Op1.m_Max.z) ); }

		// Logic operators
		public static bool			operator==( BoundingBox _Op0, BoundingBox _Op1 )					{ return ( _Op0.m_Min == _Op1.m_Min ) && ( _Op0.m_Max == _Op1.m_Max ); }
		public static bool			operator!=( BoundingBox _Op0, BoundingBox _Op1 )					{ return ( _Op0.m_Min != _Op1.m_Min ) || ( _Op0.m_Max != _Op1.m_Max ); }

		#endregion
	};
}
