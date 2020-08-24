﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using UIUtility;
using Renderer;
using SharpMath;

namespace GenerateTranslucencyMap
{
	public partial class ViewerForm : Form {
		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		private struct	CBDisplay {
			public uint		_Width;
			public uint		_Height;
			public float	_Time;
			public uint		_Flags;
			public float3	_Light;
			public float	_Thickness_mm;
			public float3	_CameraPos;
			public float	_Size_mm;
			public float3	_CameraTarget;
			public float	_IOR;
			public float3	_CameraUp;
			public float	_Sigma_t;
		}

		private GeneratorForm				m_Owner;

		private ConstantBuffer<CBDisplay>	m_CB_Display;
		private Shader						m_PS_Display;

		private DateTime					m_StartTime = DateTime.Now;

		private float3						m_LightPos = new float3( 0, 0.1f, 0 );

		// Manipulation
		private MouseButtons				m_ButtonsDown = MouseButtons.None;
		private Point						m_ButtonDownMousePos;
		private float3						m_ButtonDownLightPos;

		private Camera						m_Camera = new Camera();
		private CameraManipulator			m_Manipulator = new CameraManipulator();

		private Device		Device {
			get { return m_Owner.m_Device; }
		}

		public ViewerForm( GeneratorForm _Owner )
		{
			InitializeComponent();
			m_Owner = _Owner;
		}

		public void Init()
		{
			#if !DEBUG
				using ( ScopedForceShadersLoadFromBinary scope = new ScopedForceShadersLoadFromBinary() )
			#endif
			{
				m_PS_Display = new Shader( Device, new System.IO.FileInfo( "./Shaders/Display.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS" );
			}

			m_CB_Display = new ConstantBuffer<CBDisplay>( Device, 0 );
			m_CB_Display.m._Width = (uint) Width;
			m_CB_Display.m._Height = (uint) Height;

			// Setup camera
			m_Camera.CreatePerspectiveCamera( (float) (60.0 * Math.PI / 180.0), (float) Width / Height, 0.01f, 100.0f );
			m_Manipulator.Attach( this, m_Camera );
			m_Manipulator.InitializeCamera( new float3( 0, -0.1f, 0.1f ), new float3( 0, 0, 0 ), float3.UnitY );
			m_Manipulator.ManipulationPanSpeed = 0.1f;
			m_Camera.CameraTransformChanged += new EventHandler( Camera_CameraTransformChanged );
			m_Manipulator.EnableMouseAction += new CameraManipulator.EnableMouseActionEventHandler( m_Manipulator_EnableMouseAction );
			Camera_CameraTransformChanged( m_Manipulator, EventArgs.Empty );

			Application.Idle += new EventHandler( Application_Idle );
		}

		bool	m_exiting = false;
		public void Exit() {
			m_CB_Display.Dispose();
			m_PS_Display.Dispose();
			m_exiting = true;
		}

		bool m_Manipulator_EnableMouseAction( MouseEventArgs _e )
		{
			return Control.ModifierKeys != Keys.Shift;
		}

		void Camera_CameraTransformChanged( object sender, EventArgs e )
		{
			m_CB_Display.m._CameraPos = m_Manipulator.CameraPosition;
			m_CB_Display.m._CameraTarget = m_Manipulator.TargetPosition;
			m_CB_Display.m._CameraUp = (float3) m_Camera.Camera2World.r1;
		}

		void Application_Idle( object sender, EventArgs e )
		{
			if ( !Visible )
				return;

			// Update constants
			DateTime	CurrentTime = DateTime.Now;
			m_CB_Display.m._Time = (float) (CurrentTime - m_StartTime).TotalSeconds;

//			m_CB_Display.m._Light = new float3( m_LightDistance * (float) (Math.Sin( m_LightTheta ) * Math.Sin( m_LightPhi )), m_LightDistance * (float) Math.Cos( m_LightTheta ), m_LightDistance * (float) (Math.Sin( m_LightTheta ) * Math.Cos( m_LightPhi )) );
			m_CB_Display.m._Light = m_LightPos;

// 			const float	CAMERA_ANGLE = 45.0f * (float) Math.PI / 180.0f;
// 			m_CB_Display.m._CameraPos = new float3( 0.0f, m_CameraDistance * (float) Math.Sin( CAMERA_ANGLE ), m_CameraDistance * (float) Math.Cos( CAMERA_ANGLE ) );

			m_CB_Display.m._Thickness_mm = m_Owner.Thickness_mm;
			m_CB_Display.m._Size_mm = m_Owner.TextureSize_mm;
			m_CB_Display.m._IOR = m_Owner.IOR;
			m_CB_Display.m._Sigma_t = m_Owner.Sigma_t;
			m_CB_Display.UpdateData();

			// Render
			Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );
			Device.SetRenderTarget( Device.DefaultTarget, null );

			if ( m_PS_Display.Use() ) {

				m_CB_Display.m._Flags &= ~1U;
 				if ( m_Owner.m_TextureTargets[0][0] != null ) {

					if ( m_Owner.m_TextureSourceThickness != null )
						m_Owner.m_TextureSourceThickness.SetPS( 0 );
//					if ( m_Owner.m_TextureTarget0 != null )
//						m_Owner.m_TextureTarget0.SetPS( 0 );

 					if ( m_Owner.m_TextureSourceNormal != null )
 						m_Owner.m_TextureSourceNormal.SetPS( 1 );
 					if ( m_Owner.m_TextureSourceTransmittance != null )
 						m_Owner.m_TextureSourceTransmittance.SetPS( 2 );
 					if ( m_Owner.m_TextureSourceAlbedo != null )
 						m_Owner.m_TextureSourceAlbedo.SetPS( 3 );
 					if ( m_Owner.m_TextureSourceVisibility != null )
 						m_Owner.m_TextureSourceVisibility.SetPS( 4 );

 					m_Owner.m_TextureTargets[0][0].SetPS( 5 );
 					m_Owner.m_TextureTargets[1][0].SetPS( 6 );
 					m_Owner.m_TextureTargets[2][0].SetPS( 7 );
 					m_Owner.m_TextureTargetCombined.SetPS( 8 );

					m_CB_Display.m._Flags |= 1U;
				}

				Device.RenderFullscreenQuad( m_PS_Display );
			}

			Device.Present( false );
		}

		protected override void OnFormClosing( FormClosingEventArgs e )
		{
			e.Cancel = !m_exiting;
			Visible = false;	// Only hide...
			base.OnFormClosing( e );
		}

		private void ViewerForm_MouseDown( object sender, MouseEventArgs e )
		{
			m_ButtonsDown |= e.Button;
			m_ButtonDownMousePos = e.Location;
			m_ButtonDownLightPos = m_LightPos;
		}

		private void ViewerForm_MouseUp( object sender, MouseEventArgs e )
		{
			m_ButtonsDown &= ~e.Button;
		}

		private void ViewerForm_MouseMove( object sender, MouseEventArgs e )
		{
			float	Dx = e.X - m_ButtonDownMousePos.X;
			float	Dy = e.Y - m_ButtonDownMousePos.Y;

			if ( m_Manipulator_EnableMouseAction( e ) )
				return;	// Let the camera manipulator handle things

			const float	MOVE_SPEED = 2.0f;
			if ( (m_ButtonsDown & MouseButtons.Left) != 0 ) {
				float3	CamX = (float3) m_Camera.Camera2World.r0;
				float3	CamZ = (m_Camera.Camera2World.r2.y > 0.0f ? -1.0f : 1.0f) * float3.UnitY.Cross( CamX ).Normalized;	// Invert sign if viewing up...
				m_LightPos = m_ButtonDownLightPos + (MOVE_SPEED * Dx / Width) * CamX - (MOVE_SPEED * Dy / Height) * CamZ;

			} else if ( (m_ButtonsDown & MouseButtons.Middle) != 0 ) {
				m_LightPos = m_ButtonDownLightPos - (MOVE_SPEED * Dy / Height) * float3.UnitY;
				m_LightPos.y = Math.Max( 0.01f, m_LightPos.y );
			}

// 			if ( (m_ButtonsDown & MouseButtons.Left) != 0 ) {
// 				// Move light
// 				m_LightPhi = m_ButtonDownPhi + Dx * 2.0f * (float) Math.PI / Width;
// 				m_LightTheta = m_ButtonDownTheta + Dy * 0.5f * (float) Math.PI / Height;
// 				m_LightTheta = Math.Max( 0.0f, Math.Min( 0.4999f * (float) Math.PI, m_LightTheta ) );
// 
// 			} else if ( (m_ButtonsDown & MouseButtons.Right) != 0 ) {
// 				// Move camera
// 				const float	SCALE_SPEED = 0.005f;
// 				float	ScaleFactor = Dx > 0.0f ? (1.0f + SCALE_SPEED * Dx) : 1.0f / (1.0f - SCALE_SPEED * Dx);
// 				m_CameraDistance = m_ButtonDownCameraDistance * ScaleFactor;
// 			}
		}

		private void ViewerForm_KeyDown( object sender, KeyEventArgs e )
		{
			if ( e.KeyCode == Keys.Return )
				m_CB_Display.m._Flags ^= 2U;
			else if ( e.KeyCode == Keys.Back )
				m_CB_Display.m._Flags ^= 4U;
		}
	}
}
