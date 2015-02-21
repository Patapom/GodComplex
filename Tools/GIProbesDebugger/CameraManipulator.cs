using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using RendererManaged;

namespace GIProbesDebugger
{
	/// <summary>
	/// This is a little camera manipulator helper that you can bind to a control
	/// Use left button to rotate, middle to pan and right/wheel to zoom
	/// Use Shift to switch to "Unreal Editor first person mode"
	/// </summary>
	public class CameraManipulator
	{
		#region	CONSTANTS

		// The minimal distance we can't go under (decreasing any further would push the target along with the camera)
		protected const float	MIN_TARGET_DISTANCE					= 0.1f;

		// The MAX denormalized distance
		// NOTE: The MAX normalized distance is deduced from this value and the zoom acceleration)
		protected const float	TARGET_DISTANCE_DENORMALIZED_MAX	= 100.0f;	// At MAX normalized distance, denormalized distance should equal to this

		// The power at which the denormalized distance should increase
		protected const float	TARGET_DISTANCE_POWER				= 4.0f;

		#endregion

		#region NESTED TYPES

		public delegate bool	EnableMouseActionEventHandler( MouseEventArgs _e );

		#endregion

		#region FIELDS

		protected Control		m_Control = null;
		protected Camera		m_Camera = null;

		// Camera manipulation parameters
		protected float			m_ManipulationRotationSpeed			= 1.0f;
		protected float			m_ManipulationPanSpeed				= 1.0f;
		protected float			m_ManipulationZoomSpeed				= 0.8f;
		protected float			m_ManipulationZoomAcceleration		= 0.8f;

		// Target object matrix
		protected float4x4		m_CameraTransform					= float4x4.Identity;
		protected float			m_CameraTargetDistance				= 5.0f;

		// Camera motion
		protected MouseButtons	m_ButtonsDown						= MouseButtons.None;
		protected float4x4		m_ButtonDownTransform				= float4x4.Identity;
		protected float4x4		m_ButtonDownTargetObjectMatrix		= float4x4.Identity;
		protected float4x4		m_InvButtonDownTargetObjectMatrix	= float4x4.Identity;
		protected float2		m_ButtonDownMousePosition			= float2.Zero;
		protected float			m_ButtonDownCameraTargetDistance	= 0.0f;
		protected bool			m_bRotationEnabled					= true;
		protected float			m_NormalizedTargetDistance			= 5.0f;
		protected float			m_ButtonDownNormalizedTargetDistance = 0.0f;
		protected bool			m_bPushingTarget					= false;

		protected bool			m_bLastManipulationWasFirstPerson	= false;

		#endregion

		#region PROPERTIES

		public Camera		ManipulatedCamera
		{
			get { return m_Camera; }
			set { m_Camera = value; CameraTransform = m_CameraTransform; }
		}

		protected float4x4	CameraTransform
		{
			get { return m_CameraTransform; }
			set
			{
				m_CameraTransform = value;
				if ( m_Camera == null )
					return;

				float4x4	Result = value;
			    Result.r0 = -Result.r0;

				m_Camera.Camera2World = Result;
			}
		}

		protected float		CameraTargetDistance
		{
			get { return m_CameraTargetDistance; }
			set
			{
				float4x4	TargetMat = TargetObjectMatrix;		// Get current target matrix before changing distance
				float4x4	Temp = CameraTransform;				// Get current camera matrix before changing distance

				m_CameraTargetDistance = value;

				// Move the camera along its axis to match the new distance
				Temp.r3 =  TargetMat.r3 - m_CameraTargetDistance * Temp.r2;

				CameraTransform = Temp;

				m_NormalizedTargetDistance = NormalizeTargetDistance( m_CameraTargetDistance );
			}
		}

		protected float4x4	TargetObjectMatrix
		{
			get
			{
				float4x4	CamMat = CameraTransform;
				float4x4	TargetMat = float4x4.Identity;
						TargetMat.r3 =  CamMat.r3  + m_CameraTargetDistance * CamMat.r2;

				return	TargetMat;
			}
			set
			{
				float4x4	CamMat = CameraTransform;
						CamMat.r3 = value.r3 - m_CameraTargetDistance * value.r2;

 				CameraTransform = CamMat;
			}
		}

		protected bool		FirstPersonKeyDown
		{
			get { return (Control.ModifierKeys & Keys.Shift) != 0; }
		}

		public event EnableMouseActionEventHandler	EnableMouseAction;

		#endregion

		#region METHODS

		public	CameraManipulator()
		{
		}

		public void		Attach( Control _Control, Camera _Camera )
		{
			m_Control = _Control;
			m_Camera = _Camera;
			m_Control.MouseDown += new MouseEventHandler( Control_MouseDown );
			m_Control.MouseUp += new MouseEventHandler( Control_MouseUp );
			m_Control.MouseMove += new MouseEventHandler( Control_MouseMove );
			m_Control.MouseWheel += new MouseEventHandler( Control_MouseWheel );
		}

		public void		Detach( Control _Control )
		{
			m_Control.MouseDown += new MouseEventHandler( Control_MouseDown );
			m_Control.MouseUp += new MouseEventHandler( Control_MouseUp );
			m_Control.MouseMove += new MouseEventHandler( Control_MouseMove );
			m_Control.MouseWheel += new MouseEventHandler( Control_MouseWheel );
			m_Control = null;
			m_Camera = null;
		}

		public void		InitializeCamera( float3 _Position, float3 _Target, float3 _Up )
		{
			// Build the camera matrix
			float3	At = _Target - _Position;
			if ( At.LengthSquared > 1e-2f )
			{	// Normal case
				m_CameraTargetDistance = At.Length;
				At /= m_CameraTargetDistance;
			}
			else
			{	// Special bad case
				m_CameraTargetDistance = 0.01f;
				At = new float3( 0.0f, 0.0f, -1.0f );
			}

			float3		Ortho = _Up.Cross( At ).Normalized;

			float4x4	CameraMat = float4x4.Identity;
						CameraMat.r3 = new float4( _Position, 1.0f );
						CameraMat.r2 = new float4( At, 0.0f );
						CameraMat.r0 = new float4( Ortho, 0.0f );
						CameraMat.r1 = new float4( At.Cross( Ortho ), 0.0f );

			CameraTransform = CameraMat;

			// Setup the normalized target distance
			m_NormalizedTargetDistance = NormalizeTargetDistance( m_CameraTargetDistance );
		}

		protected float2	ComputeNormalizedScreenPosition( int _X, int _Y, float _fCameraAspectRatio )
		{
			return new float2( _fCameraAspectRatio * (2.0f * (float) _X - m_Control.Width) / m_Control.Width, 1.0f - 2.0f * (float) _Y / m_Control.Height );
		}

		protected float		GetDenormalizationFactor()
		{
			float	fMaxDeNormalizedDistance = TARGET_DISTANCE_DENORMALIZED_MAX / m_ManipulationZoomSpeed;			// Here, we reduce the max denormalized distance based on the zoom speed
			float	fMaxNormalizedDistance = fMaxDeNormalizedDistance * (1.0f - m_ManipulationZoomAcceleration);	// This line deduces the max normalized distance from the max denormalized distance

			return	fMaxDeNormalizedDistance / (float) Math.Pow( fMaxNormalizedDistance, TARGET_DISTANCE_POWER );
		}

		protected float		NormalizeTargetDistance( float _fDeNormalizedTargetDistance )
		{
			return	(float) Math.Pow( _fDeNormalizedTargetDistance / GetDenormalizationFactor(), 1.0 / TARGET_DISTANCE_POWER );
		}

		protected float		DeNormalizeTargetDistance( float _fNormalizedTargetDistance )
		{
			return	GetDenormalizationFactor() * (float) Math.Pow( _fNormalizedTargetDistance, TARGET_DISTANCE_POWER );
		}

		/// <summary>
		/// Converts an angle+axis into a plain rotation matrix
		/// </summary>
		/// <param name="_Angle"></param>
		/// <param name="_Axis"></param>
		/// <returns></returns>
		protected float4x4	AngleAxis2Matrix( float _Angle, float3 _Axis )
		{
			// Convert into a quaternion
			float3	qv = (float) System.Math.Sin( .5f * _Angle ) * _Axis;
			float	qs = (float) System.Math.Cos( .5f * _Angle );

			// Then into a matrix
			float	xs, ys, zs, wx, wy, wz, xx, xy, xz, yy, yz, zz;

// 			Quat	q = new Quat( _Source );
// 			q.Normalize();		// A cast to a matrix only works with normalized quaternions!

			xs = 2.0f * qv.x;	ys = 2.0f * qv.y;	zs = 2.0f * qv.z;

			wx = qs * xs;		wy = qs * ys;		wz = qs * zs;
			xx = qv.x * xs;	xy = qv.x * ys;	xz = qv.x * zs;
			yy = qv.y * ys;	yz = qv.y * zs;	zz = qv.z * zs;

			float4x4	Ret = float4x4.Identity;

			Ret.r0.x = 1.0f -	yy - zz;
			Ret.r0.y =			xy + wz;
			Ret.r0.z =			xz - wy;

			Ret.r1.x =			xy - wz;
			Ret.r1.y = 1.0f -	xx - zz;
			Ret.r1.z =			yz + wx;

			Ret.r2.x =			xz + wy;
			Ret.r2.y =			yz - wx;
			Ret.r2.z = 1.0f -	xx - yy;

			return	Ret;
		}

		/// <summary>
		/// Extracts Euler angles from a rotation matrix
		/// </summary>
		/// <param name="_Matrix"></param>
		/// <returns></returns>
		protected float3	GetEuler( float4x4 _Matrix )
		{
			float3	Ret = new float3();
			float	fSinY = Math.Min( +1.0f, Math.Max( -1.0f, _Matrix.r0.z ) ),
					fCosY = (float) Math.Sqrt( 1.0f - fSinY*fSinY );

			if ( _Matrix.r0.x < 0.0 && _Matrix.r2.z < 0.0 )
				fCosY = -fCosY;

			if ( (float) Math.Abs( fCosY ) > float.Epsilon )
			{
				Ret.x = (float)  Math.Atan2( _Matrix.r1.z / fCosY, _Matrix.r2.z / fCosY );
				Ret.y = (float) -Math.Atan2( fSinY, fCosY );
				Ret.z = (float)  Math.Atan2( _Matrix.r0.y / fCosY, _Matrix.r0.x / fCosY );
			}
			else
			{
				Ret.x = (float)  Math.Atan2( -_Matrix.r2.y, _Matrix.r1.y );
				Ret.y = (float) -Math.Asin( fSinY );
				Ret.z = 0.0f;
			}

			return	Ret;
		}

		#endregion

		#region EVENT HANDLERS

		void Control_MouseDown( object sender, MouseEventArgs e )
		{
			if ( EnableMouseAction != null && !EnableMouseAction( e ) )
				return;	// Don't do anything

			m_ButtonsDown |= e.Button;		// Add this button

			// Keep a track of the mouse and camera states when button was pressed
			m_ButtonDownTransform = CameraTransform;
			m_ButtonDownTargetObjectMatrix = TargetObjectMatrix;
			m_InvButtonDownTargetObjectMatrix = m_ButtonDownTargetObjectMatrix.Inverse;
			m_ButtonDownMousePosition = ComputeNormalizedScreenPosition( e.X, e.Y, (float) m_Control.Width / m_Control.Height );
			m_ButtonDownCameraTargetDistance = CameraTargetDistance;
			m_ButtonDownNormalizedTargetDistance = NormalizeTargetDistance( m_ButtonDownCameraTargetDistance );
		}

		void Control_MouseUp( object sender, MouseEventArgs e )
		{
			m_ButtonsDown = MouseButtons.None;	// Remove all buttons

			// Update the mouse and camera states when button is released
			m_ButtonDownTransform = CameraTransform;
			m_ButtonDownTargetObjectMatrix = TargetObjectMatrix;
			m_ButtonDownMousePosition = ComputeNormalizedScreenPosition( e.X, e.Y, (float) m_Control.Width / m_Control.Height );
			m_ButtonDownCameraTargetDistance = CameraTargetDistance;
			m_ButtonDownNormalizedTargetDistance = NormalizeTargetDistance( m_ButtonDownCameraTargetDistance );
		}

		void Control_MouseMove( object sender, MouseEventArgs e )
		{
			if ( EnableMouseAction != null && !EnableMouseAction( e ) )
				return;	// Don't do anything

			float4x4	CameraMatrixBeforeBaseCall = CameraTransform;

//			base.OnMouseMove( e );

			m_Control.Focus();

			float2	MousePos = ComputeNormalizedScreenPosition( e.X, e.Y, (float) m_Control.Width / m_Control.Height );

			// Check for FIRST PERSON switch
			if ( m_bLastManipulationWasFirstPerson ^ FirstPersonKeyDown )
			{	// There was a switch so we need to copy the current matrix and make it look like the button was just pressed...
				Control_MouseDown( sender, e );
			}
			m_bLastManipulationWasFirstPerson = FirstPersonKeyDown;

			if ( !FirstPersonKeyDown )
			{
				//////////////////////////////////////////////////////////////////////////
				// MAYA MANIPULATION MODE
				//////////////////////////////////////////////////////////////////////////
				//
				switch ( m_ButtonsDown )
				{
						// ROTATE
					case	MouseButtons.Left:
					{
						if ( !m_bRotationEnabled )
							break;	// Rotation is disabled!

						float	fAngleX = (MousePos.y - m_ButtonDownMousePosition.y) * 2.0f * (float) Math.PI * m_ManipulationRotationSpeed;
						float	fAngleY = (MousePos.x - m_ButtonDownMousePosition.x) * 2.0f * (float) Math.PI * m_ManipulationRotationSpeed;

						float4		AxisX = m_ButtonDownTransform.r0;
						float4x4	Rot = AngleAxis2Matrix( fAngleX, -new float3( AxisX.x, AxisX.y, AxisX.z ) )
										* AngleAxis2Matrix( fAngleY, new float3( 0f, -1.0f, 0.0f ) );

						float4x4	Rotated = m_ButtonDownTransform * m_InvButtonDownTargetObjectMatrix * Rot * TargetObjectMatrix;

						CameraTransform = Rotated;

						break;
					}

						// DOLLY => Simply translate along the AT axis
					case	MouseButtons.Right:
					case	MouseButtons.Left | MouseButtons.Middle:
					{
						float	fTrans = m_ButtonDownMousePosition.x - m_ButtonDownMousePosition.y - MousePos.x + MousePos.y;

						m_NormalizedTargetDistance = m_ButtonDownNormalizedTargetDistance + 4.0f * m_ManipulationZoomSpeed * fTrans;
						float	fTargetDistance = Math.Sign( m_NormalizedTargetDistance ) * DeNormalizeTargetDistance( m_NormalizedTargetDistance );
						if ( fTargetDistance > MIN_TARGET_DISTANCE )
						{	// Okay! We're far enough so we can reduce the distance anyway
							CameraTargetDistance = fTargetDistance;
							m_bPushingTarget = false;
						}
						else
						{	// Too close! Let's move the camera forward and clamp the target distance... That will push the target along.
							m_CameraTargetDistance = MIN_TARGET_DISTANCE;
							m_NormalizedTargetDistance = NormalizeTargetDistance( m_CameraTargetDistance );

							if ( !m_bPushingTarget )
							{
								m_ButtonDownNormalizedTargetDistance = m_NormalizedTargetDistance;
								fTrans = 0.0f;
								m_bPushingTarget = true;
							}

							m_ButtonDownMousePosition = MousePos;

							float4x4	DollyCam = CameraTransform;
									DollyCam.r3 = DollyCam.r3 - 2.0f * m_ManipulationZoomSpeed * fTrans * DollyCam.r2;

							CameraTransform = DollyCam;
						}
						break;
					}

						// PAN
					case	MouseButtons.Middle:
					{
						float2	Trans = new float2(	-(MousePos.x - m_ButtonDownMousePosition.x),
														MousePos.y - m_ButtonDownMousePosition.y
													);

						float		fTransFactor = m_ManipulationPanSpeed * Math.Max( 2.0f, m_CameraTargetDistance );

						// Make the camera pan
						float4x4	PanCam = m_ButtonDownTransform;
									PanCam.r3 = m_ButtonDownTransform.r3
											  - fTransFactor * Trans.x * m_ButtonDownTransform.r0
											  - fTransFactor * Trans.y * m_ButtonDownTransform.r1;

						CameraTransform = PanCam;
						break;
					}
				}
			}
			else
			{
				//////////////////////////////////////////////////////////////////////////
				// UNREAL MANIPULATION MODE
				//////////////////////////////////////////////////////////////////////////
				//
				switch ( m_ButtonsDown )
				{
					// TRANSLATE IN THE ZX PLANE (WORLD SPACE)
					case	MouseButtons.Left :
					{
						float	fTransFactor = m_ManipulationPanSpeed * System.Math.Max( 4.0f, CameraTargetDistance );

						// Compute translation in the view direction
					    float4	Trans = CameraMatrixBeforeBaseCall.r2;
								Trans.y = 0.0f;
						if ( Trans.LengthSquared < 1e-4f )
						{	// Better use Y instead...
						    Trans = CameraMatrixBeforeBaseCall.r1;
							Trans.y = 0.0f;
						}

						Trans = Trans.Normalized;

						float4	NewPosition = CameraMatrixBeforeBaseCall.r3 + Trans * fTransFactor * (MousePos.y - m_ButtonDownMousePosition.y);

						m_ButtonDownMousePosition.y = MousePos.y;	// The translation is a cumulative operation...

						// Compute rotation about the the Y WORLD axis
						float		fAngleY = (m_ButtonDownMousePosition.x - MousePos.x) * 2.0f * (float) Math.PI * m_ManipulationRotationSpeed * 0.2f;	// [PATAPATCH] Multiplied by 0.2 as it's REALLY too sensitive otherwise!
						if ( m_ButtonDownTransform.r1.y < 0.0f )
							fAngleY = -fAngleY;		// Special "head down" case...

						float4x4	RotY = float4x4.RotationY( fAngleY );

						float4x4	FinalMatrix = m_ButtonDownTransform;
								FinalMatrix.r3 = float4.Zero;	// Clear translation...
								FinalMatrix = FinalMatrix * RotY;
								FinalMatrix.r3 = NewPosition;

						CameraTransform = FinalMatrix;

						break;
					}

					// ROTATE ABOUT CAMERA
					case	MouseButtons.Right :
					{
						float		fAngleY = (m_ButtonDownMousePosition.x - MousePos.x) * 2.0f * (float) Math.PI * m_ManipulationRotationSpeed;
						float		fAngleX = (m_ButtonDownMousePosition.y - MousePos.y) * 2.0f * (float) Math.PI * m_ManipulationRotationSpeed;

						float3		Euler = GetEuler( m_ButtonDownTransform );
						float4x4	CamRotYMatrix = float4x4.RotationY( fAngleY + Euler.y );
						float4x4	CamRotXMatrix = float4x4.RotationX( fAngleX + Euler.x );
						float4x4	CamRotZMatrix = float4x4.RotationZ( Euler.z );

						float4x4	RotateMatrix = CamRotXMatrix * CamRotYMatrix * CamRotZMatrix;

						RotateMatrix.r3 = CameraTransform.r3;
						CameraTransform = RotateMatrix;

						break;
					}

						// Translate in the ( Z-world Y-camera ) plane
					case	MouseButtons.Middle :
					case	MouseButtons.Left | MouseButtons.Right:
					{
						float		fTransFactor = m_ManipulationPanSpeed * System.Math.Max( 4.0f, CameraTargetDistance );

						float4		NewPosition =	m_ButtonDownTransform.r3 + fTransFactor *
													( (MousePos.y - m_ButtonDownMousePosition.y) * float4.UnitY
													+ (m_ButtonDownMousePosition.x - MousePos.x) * m_ButtonDownTransform.r0 );

						float4x4	NewMatrix = m_ButtonDownTransform;
								NewMatrix.r3 =  NewPosition ;

						CameraTransform = NewMatrix;

						break;
					}
				}
			}
		}

		void Control_MouseWheel( object sender, MouseEventArgs e )
		{
			if ( EnableMouseAction != null && !EnableMouseAction( e ) )
				return;	// Don't do anything

			m_NormalizedTargetDistance -= 0.004f * m_ManipulationZoomSpeed * e.Delta;
			float	fTargetDistance = DeNormalizeTargetDistance( m_NormalizedTargetDistance );
			if ( fTargetDistance > MIN_TARGET_DISTANCE )
			{	// Okay! We're far enough so we can reduce the distance anyway
				CameraTargetDistance = fTargetDistance;

				// Update "cached" data
				Control_MouseDown( sender, e );
			}
			else
			{
				// Too close! Let's move the camera forward without changing the target distance...
				m_CameraTargetDistance = MIN_TARGET_DISTANCE;
				m_NormalizedTargetDistance = NormalizeTargetDistance( m_CameraTargetDistance );

				float4x4	DollyCam = CameraTransform;
						DollyCam.r3 = DollyCam.r3 + 0.004f * m_ManipulationZoomSpeed * e.Delta * DollyCam.r2;

				CameraTransform = DollyCam;

				// Update "cached" data
				Control_MouseDown( sender, e );
			}
		}

		#endregion
	}
}
