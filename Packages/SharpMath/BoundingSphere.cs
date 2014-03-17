using System;

namespace WMath
{
	/// <summary>
	/// </summary>
    [System.Diagnostics.DebuggerDisplay("Center = ({m_Center.x}, {m_Center.y}, {m_Center.z}) Radius = m_Radius")]
    public class BoundingSphere
	{
		#region	CONSTANTS

		internal static float	EPSILON = float.Epsilon;	// Use the Global class to modify this epsilon

		#endregion

		#region	FIELDS

		public Point			m_Center;
		public float			m_Radius;

		#endregion

		#region PROPERTIES

		public Point			Center
		{
			get { return m_Center; }
			set { m_Center = value; }
		}

		public float			Radius
		{
			get { return m_Radius; }
			set { m_Radius = value; }
		}

		public static BoundingSphere	Empty
		{
			get { return new BoundingSphere( 0, 0, 0, 0 ); }
		}

		#endregion

		#region	METHODS

		// Constructors/Destructor
		public					BoundingSphere	()												{ m_Center = new Point(); m_Radius = 0.0f; }
		public					BoundingSphere	( float _X, float _Y, float _Z, float _Radius )	{ m_Center = new Point( _X, _Y, _Z ); m_Radius = _Radius; }
		public					BoundingSphere	( Point _Center, float _Radius ) 				{ m_Center = _Center; m_Radius = _Radius; }

		public override string		ToString()
		{
			return	"[" + m_Center + "] R=" + m_Radius;
		}

		// Logic operators
		public static bool			operator==( BoundingSphere _Op0, BoundingSphere _Op1 )		{ return (_Op0.m_Center == _Op1.m_Center) && (_Op0.m_Radius == _Op1.m_Radius); }
		public static bool			operator!=( BoundingSphere _Op0, BoundingSphere _Op1 )		{ return (_Op0.m_Center != _Op1.m_Center) || (_Op0.m_Radius != _Op1.m_Radius); }

		#endregion
	};
}
