using System;

namespace SharpMath
{
	/// <summary>
	/// Summary description for Ray.
	/// </summary>
    [System.Diagnostics.DebuggerDisplay("Pos = ({m_Pos.x}, {m_Pos.y}, {m_Pos.z}) Aim = ({m_Aim.x}, {m_Aim.y}, {m_Aim.z}) Length = {m_Length} Marched = {m_MarchedLength}")]
    public class Ray {
		#region FIELDS

		private float3				m_Pos				= new float3();													// The origin of the ray
		private float3				m_Aim				= new float3();													// The direction of the ray from the origin
		private float				m_Length			= float.MaxValue;												// The dynamic length of the ray
		private float				m_MarchedLength		= 0.0f;															// The marched length of the ray (accumulated distances)
		private object				m_Datum				= 0;															// General purpose datum associated to the ray

		#endregion

		#region PROPERTIES

		public float3				Pos
		{
			get	{ return m_Pos; }
			set { m_Pos = value; }
		}

		public float3				Aim
		{
			get	{ return m_Aim; }
			set { m_Aim = value; }
		}

		public float				Length
		{
			get	{ return m_Length; }
			set { m_Length = value; }
		}

		public float				MarchedLength
		{
			get	{ return m_MarchedLength; }
			set { m_MarchedLength = value; }
		}

		public object				Datum
		{
			get	{ return m_Datum; }
			set { m_Datum = value; }
		}

		#endregion

		#region METHODS

		public					Ray				()																									{}
		public					Ray				( Ray _Source )																						{ Set( _Source ); }
		public					Ray				( float3 _Pos, float3 _Aim )																		{ m_Pos = _Pos; m_Aim = _Aim; }
		public					Ray				( float3 _Pos, float3 _Aim, float _fLength ): this( _Pos, _Aim )									{ m_Length = _fLength; }
		public					Ray				( float3 _Pos, float3 _Aim, float _fLength, float _fMarchedLength ) : this( _Pos, _Aim, _fLength )	{ m_MarchedLength = _fMarchedLength; }

		// Helpers
		public Ray				March			( float _fDelta )										{ m_Pos += _fDelta * m_Aim; m_Length -= _fDelta; m_MarchedLength += _fDelta; return this; }
		public Ray				GoToHit			()														{ return March( m_Length ); }
		public float3			GetHitPos		()														{ return m_Pos + m_Length * m_Aim; }
		public Ray				Bend			( float3 _Axis, float _fBendFactor )					{ m_Aim += _fBendFactor * m_Aim.Dot(_Axis) * _Axis; m_Aim.Normalize(); return this; }
		public Ray				TurnAboutOrtho	( float3 _Axis, float _fAngle ) {
			float	fCos = m_Aim.Dot( _Axis );
			float	fSin = (float) System.Math.Sqrt( 1.0f - fCos * fCos );

			float3	Z = m_Aim.Cross( _Axis );
			float3	X = _Axis.Cross( Z );
			float3	Temp = new float3( (float) (fSin * System.Math.Cos( _fAngle ) - fCos * System.Math.Sin( _fAngle )), (float) (fSin * System.Math.Sin( _fAngle ) + fCos * System.Math.Cos( _fAngle )), 0.0f );

			m_Aim = Temp.x * X + Temp.y * _Axis;

			return	this;
		}


		// Assignment operators
		public void				Set				( Ray _r )												{ m_Pos = _r.m_Pos; m_Aim = _r.m_Aim; m_Length = _r.m_Length; m_MarchedLength = _r.m_MarchedLength; m_Datum = _r.m_Datum; }

		// Arithmetic operators
		public static Ray		operator-		( Ray _Op0 )											{ return new Ray( _Op0.m_Pos, -_Op0.m_Aim, _Op0.m_Length, _Op0.m_MarchedLength ); }
		public static Ray		operator*		( Ray _Op0, float3x3 _Op1 )								{ return new Ray( _Op0.m_Pos * _Op1, _Op0.m_Aim * _Op1, _Op0.m_Length, _Op0.m_MarchedLength ); }
		public static Ray		operator*		( Ray _Op0, float4x4 _Op1 )								{ return new Ray( (float3) (new float4( _Op0.m_Pos, 1 ) * _Op1), (float3) (new float4( _Op0.m_Aim, 0 ) * _Op1), _Op0.m_Length, _Op0.m_MarchedLength ); }

		#endregion
	}
}
