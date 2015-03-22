using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RendererManaged;

namespace Nuaj.Cirrus.Utility
{
	/// <summary>
	/// The Camera class doesn't wrap any DirectX component per-se but helps a lot to handle
	///  basic displacement and projections
	///  
	/// NOTES :
	/// _ The projection float4x4 is Left Handed
	/// _ The Local2World float4x4 is left handed (all other matrices in Nuaj are right handed !)
	/// 
	/// A typical camera float4x4 looks like this :
	/// 
	///     Y (Up)
	///     ^
	///     |    Z (At)
	///     |   /
	///     |  /
	///     | /
	///     |/
	///     o---------> X (Right)
	/// 
	/// </summary>
	public class Camera
	{
		#region FIELDS

		protected float4x4	m_Camera2World = float4x4.Identity;	// Transform float4x4
		protected float4x4	m_World2Camera = float4x4.Identity;
		protected float4x4	m_Camera2Proj = float4x4.Identity;	// Projection float4x4
		protected float4x4	m_World2Proj = float4x4.Identity;	// Transform + Projection float4x4

		protected float		m_Near = 0.0f;
		protected float		m_Far = 0.0f;
		protected float		m_AspectRatio = 0.0f;

		// Perspective/Orthogonal informations
		protected bool		m_bIsPerspective = true;
		protected float		m_PerspFOV = 0.0f;
		protected float		m_OrthoHeight = 0.0f;

		protected bool		m_bActive = false;

		protected float4	m_CachedCameraData = new float4();

		#endregion

		#region PROPERTIES

		/// <summary>
		/// Gets or sets the camera transform (CAMERA => WORLD)
		/// </summary>
		public float4x4			Camera2World
		{
			get { return m_Camera2World; }
			set
			{
				m_Camera2World = value;
				m_World2Camera = value.Inverse;
				m_World2Proj = m_World2Camera * m_Camera2Proj;

				// Notify of the change
				if ( CameraTransformChanged != null )
					CameraTransformChanged( this, EventArgs.Empty );
			}
		}

		/// <summary>
		/// Gets the inverse camera transform (WORLD => CAMERA)
		/// </summary>
		public float4x4			World2Camera
		{
			get { return m_World2Camera; }
		}

		/// <summary>
		/// Gets the projection transform
		/// </summary>
		[System.ComponentModel.Browsable( false )]
		public float4x4			Camera2Proj
		{
			get { return m_Camera2Proj; }
			private set
			{
				m_Camera2Proj = value;
				m_World2Proj = m_World2Camera * m_Camera2Proj;

				// Notify of the change
				if ( CameraProjectionChanged != null )
					CameraProjectionChanged( this, EventArgs.Empty );
			}
		}

		/// <summary>
		/// Gets the world to projection transform
		/// </summary>
		[System.ComponentModel.Browsable( false )]
		public float4x4			World2Proj	{ get { return m_World2Proj; } }

		/// <summary>
		/// Gets the near clip plane distance
		/// </summary>
		public float			Near		{ get { return m_Near; } set { m_Far = value; RebuildProjection(); } }

		/// <summary>
		/// Gets the far clip plane distance
		/// </summary>
		public float			Far			{ get { return m_Far; } set { m_Far = value; RebuildProjection(); } }

		/// <summary>
		/// Gets the aspect ratio
		/// </summary>
		public float			AspectRatio	{ get { return m_AspectRatio; } }

		/// <summary>
		/// Tells if the camera was initialized as a perspective camera
		/// </summary>
		public bool				IsPerspective	{ get { return m_bIsPerspective; } }

		/// <summary>
		/// Gets the vertical FOV value used for perspective init
		/// </summary>
		public float			PerspectiveFOV	{ get { return m_PerspFOV; } }

		/// <summary>
		/// Gets the vertical height value used for orthographic init
		/// </summary>
		public float			OrthographicHeight	{ get { return m_OrthoHeight; } }

		public float3			Right		{ get { return (float3) m_Camera2World.r0; } }
		public float3			Up			{ get { return (float3) m_Camera2World.r1; } }
		public float3			At			{ get { return (float3) m_Camera2World.r2; } }
		public float3			Position	{ get { return (float3) m_Camera2World.r3; } }

		public event EventHandler	CameraTransformChanged;
		public event EventHandler	CameraProjectionChanged;

		#endregion

		#region METHODS

		/// <summary>
		/// Creates a default camera
		/// </summary>
		public	Camera()
		{
		}

		/// <summary>
		/// Creates a perspective projection float4x4 for the camera
		/// </summary>
		/// <param name="_FOV"></param>
		/// <param name="_AspectRatio"></param>
		/// <param name="_Near"></param>
		/// <param name="_Far"></param>
		public void		CreatePerspectiveCamera( float _FOV, float _AspectRatio, float _Near, float _Far )
		{
			m_Near = _Near;
			m_Far = _Far;
			m_AspectRatio = _AspectRatio;
			m_PerspFOV = _FOV;

			float4x4	Temp = new float4x4();
			Temp.MakeProjectionPerspective( _FOV, _AspectRatio, _Near, _Far );
			this.Camera2Proj = Temp;
			m_bIsPerspective = true;

			// Build camera data
			m_CachedCameraData.x = (float) Math.Tan( 0.5 * m_PerspFOV );
			m_CachedCameraData.y = m_AspectRatio;
			m_CachedCameraData.z = m_Near;
			m_CachedCameraData.w = m_Far;
		}

// 		/// <summary>
// 		/// Creates an orthogonal projection float4x4 for the camera
// 		/// </summary>
// 		/// <param name="_Height"></param>
// 		/// <param name="_AspectRatio"></param>
// 		/// <param name="_Near"></param>
// 		/// <param name="_Far"></param>
// 		public void		CreateOrthoCamera( float _Height, float _AspectRatio, float _Near, float _Far )
// 		{
// 			m_Near = _Near;
// 			m_Far = _Far;
// 			m_AspectRatio = _AspectRatio;
// 			m_OrthoHeight = _Height;
// 			m_Frustum = Frustum.FromOrtho( _Height, _AspectRatio, _Near, _Far );
// 			this.Camera2Proj = float4x4.OrthoLH( _AspectRatio * _Height, _Height, _Near, _Far );
// 			m_bIsPerspective = false;
// 
// 			// Build camera data
// 			m_CachedCameraData.X = 0.5f * _Height;
// 			m_CachedCameraData.Y = m_AspectRatio;
// 			m_CachedCameraData.Z = m_Near;
// 			m_CachedCameraData.W = m_Far;
// 		}

		/// <summary>
		/// Rebuilds the camera projection data after a change
		/// </summary>
		protected void	RebuildProjection()
		{
			if ( m_bIsPerspective )
				CreatePerspectiveCamera( m_PerspFOV, m_AspectRatio, m_Near, m_Far );
// 			else
// 				CreateOrthoCamera( m_OrthoHeight, m_AspectRatio, m_Near, m_Far );
		}

		/// <summary>
		/// Makes the camera look at the specified target from the specified eye position
		/// </summary>
		/// <param name="_Eye"></param>
		/// <param name="_Target"></param>
		/// <param name="_Up"></param>
		public void		LookAt( float3 _Eye, float3 _Target, float3 _Up )
		{
			float4x4	Temp = new float4x4();
						Temp.MakeLookAtCamera( _Eye, _Target, _Up );
			this.Camera2World = Temp;
		}

		/// <summary>
		/// Projects a 3D point in 2D
		/// </summary>
		/// <param name="_Position"></param>
		/// <returns></returns>
		public float2	ProjectPoint( float3 _Position )
		{
			float4	Temp = new float4( _Position, 1.0f ) * m_World2Proj;
					Temp *= 1.0f / Temp.w;
			return new float2( Temp.x, Temp.y );
		}

		/// <summary>
		/// Projects a 3D vector in 2D
		/// </summary>
		/// <param name="_Position"></param>
		/// <returns></returns>
		public float2	ProjectVector( float3 _Vector )
		{
			float4	Temp = new float4( _Vector, 0.0f ) * m_World2Proj;
					Temp *= 1.0f / Temp.w;
			return new float2( Temp.x, Temp.y );
		}

// 		/// <summary>
// 		/// Builds a camera ray in WORLD space
// 		/// </summary>
// 		/// <param name="_X">The normalized X coordinate in [0,1] (0 is left screen border and 1 is right screen border)</param>
// 		/// <param name="_Y">The normalized Y coordinate in [0,1] (0 is top screen border and 1 is bottom screen border)</param>
// 		/// <param name="_Position"></param>
// 		/// <param name="_Direction"></param>
// 		public void		BuildWorldRay( float _X, float _Y, out float3 _Position, out float3 _Direction )
// 		{
// 			float3	P, V;
// 			BuildCameraRay( _X, _Y, out P, out V );
// 
// 			_Position = float3.TransformCoordinate( P, m_Camera2World );
// 			_Direction = float3.TransformNormal( V, m_Camera2World );
// 		}

		/// <summary>
		/// Builds a camera ray in CAMERA space
		/// </summary>
		/// <param name="_X">The normalized X coordinate in [0,1] (0 is left screen border and 1 is right screen border)</param>
		/// <param name="_Y">The normalized Y coordinate in [0,1] (0 is top screen border and 1 is bottom screen border)</param>
		/// <param name="_Position"></param>
		/// <param name="_Direction"></param>
		public void		BuildCameraRay( float _X, float _Y, out float3 _Position, out float3 _Direction )
		{
			if ( m_bIsPerspective )
			{
				_Position = float3.Zero;
				_Direction = new float3(
						(2.0f * _X - 1.0f) * m_AspectRatio * (float) Math.Tan( 0.5f * m_PerspFOV ),
						(1.0f - 2.0f * _Y) * (float) Math.Tan( 0.5f * m_PerspFOV ),
						1.0f
					).Normalized;
			}
			else
			{
				_Direction = float3.UnitZ;
				_Position = new float3(
						(_X - 0.5f) * m_AspectRatio * m_OrthoHeight,
						(0.5f - _Y) * m_OrthoHeight,
						0.0f
					);
			}
		}

		#endregion
	}
}
