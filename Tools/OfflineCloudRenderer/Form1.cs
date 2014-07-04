using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

using WMath;
using RendererManaged;

namespace OfflineCloudRenderer
{
	public partial class Form1 : Form
	{
		#region NESTED TYPES

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		public struct	CB_Camera
		{
			public float4		CameraData;		// X=tan(FOV_H/2) Y=tan(FOV_V/2) Z=Near W=Far
			public float4x4		Camera2World;
			public float4x4		World2Camera;
			public float4x4		Camera2Proj;
			public float4x4		Proj2Camera;
			public float4x4		World2Proj;
			public float4x4		Proj2World;
		}

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		public struct	CB_Render
		{
			public float4		TargetDimensions;	// XY=Target dimensions, ZW=1/XY
		}

		#endregion

		#region FIELDS

		private RegistryKey					m_AppKey;
		private string						m_ApplicationPath;

		private Device						m_Device = new Device();

		private ComputeShader				m_CS = null;
		private Shader						m_PS = null;
		private ConstantBuffer<CB_Camera>	m_CB_Camera = null;
		private ConstantBuffer<CB_Render>	m_CB_Render = null;

		private List<IDisposable>			m_Disposables = new List<IDisposable>();

		#endregion

		#region METHODS

		public Form1()
		{
			InitializeComponent();
			viewportPanel.Device = m_Device;

 			m_AppKey = Registry.CurrentUser.CreateSubKey( @"Software\GodComplex\OfflineCloudsRenderer" );
			m_ApplicationPath = System.IO.Path.GetDirectoryName( Application.ExecutablePath );
		}

		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );

			m_Device.Init( viewportPanel.Handle, false, true );
			m_Device.Clear( Color.SkyBlue );

			Reg( m_CS = new ComputeShader( m_Device, new ShaderFile( new System.IO.FileInfo( @"Shaders/Test/TestCompute.hlsl" ) ), "CS", null ) );
			Reg( m_PS = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( @"Shaders/DisplayDistanceField.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", "PS", null ) );
			Reg( m_CB_Camera = new ConstantBuffer<CB_Camera>( m_Device, 0 ) );
			Reg( m_CB_Render = new ConstantBuffer<CB_Render>( m_Device, 8 ) );
		}

		protected override void OnClosing( CancelEventArgs e )
		{
			foreach ( IDisposable D in m_Disposables )
				D.Dispose();

			m_Device.Exit();
//			m_Device = null;

			base.OnClosing( e );
		}

		/// <summary>
		/// Computes and updates the camera constant buffer
		/// </summary>
		/// <param name="_Position"></param>
		/// <param name="_Target"></param>
		/// <param name="_FOV"></param>
		/// <param name="_Near"></param>
		/// <param name="_Far"></param>
		private void	UpdateCameraMatrices( float3 _Position, float3 _Target, float3 _Up, float _FOV, float _AspectRatio, float _Near, float _Far )
		{
			float	TanHalfFOV = (float) Math.Tan( 0.5 * _FOV );
			m_CB_Camera.m.CameraData = new float4( _AspectRatio * TanHalfFOV, TanHalfFOV, _Near, _Far );

			m_CB_Camera.m.Camera2World.MakeLookAt( _Position, _Target, _Up );
			m_CB_Camera.m.World2Camera = m_CB_Camera.m.Camera2World.Inverse;
			m_CB_Camera.m.Camera2Proj.MakeProjectionPerspective( _FOV, _AspectRatio, _Near, _Far );
			m_CB_Camera.m.Proj2Camera = m_CB_Camera.m.Camera2Proj.Inverse;

//float4x4	Test = m_CB_Camera.m.Camera2Proj * m_CB_Camera.m.Proj2Camera;

			m_CB_Camera.m.World2Proj = m_CB_Camera.m.World2Camera * m_CB_Camera.m.Camera2Proj;
			m_CB_Camera.m.Proj2World = m_CB_Camera.m.Proj2Camera * m_CB_Camera.m.Camera2World;
			m_CB_Camera.UpdateData();
		}

		private void	Render()
		{
			UpdateCameraMatrices( new float3( 0.0f, 0.0f, 4.0f ), new float3( 0, 0, 0 ), new float3( 0, 1, 0 ), 60.0f * (float) Math.PI / 180.0f, (float) viewportPanel.Width / viewportPanel.Height, 0.1f, 10.0f );

			// Setup default render target as UAV & render using the compute shader
			m_CB_Render.m.TargetDimensions = new float4( viewportPanel.Width, viewportPanel.Height, 1.0f / viewportPanel.Width, 1.0f / viewportPanel.Height );
			m_CB_Render.UpdateData();

			// Render a fullscreen quad
			m_Device.SetRenderTarget( m_Device.DefaultTarget, null );
			m_Device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );
			m_Device.RenderFullscreenQuad( m_PS );

			// Refresh
			viewportPanel.Invalidate();
		}

		#region Helpers

		private string	GetRegKey( string _Key, string _Default )
		{
			string	Result = m_AppKey.GetValue( _Key ) as string;
			return Result != null ? Result : _Default;
		}
		private void	SetRegKey( string _Key, string _Value )
		{
			m_AppKey.SetValue( _Key, _Value );
		}

		private float	GetRegKeyFloat( string _Key, float _Default )
		{
			string	Value = GetRegKey( _Key, _Default.ToString() );
			float	Result;
			float.TryParse( Value, out Result );
			return Result;
		}

		private int		GetRegKeyInt( string _Key, float _Default )
		{
			string	Value = GetRegKey( _Key, _Default.ToString() );
			int		Result;
			int.TryParse( Value, out Result );
			return Result;
		}

		private DialogResult	MessageBox( string _Text )
		{
			return MessageBox( _Text, MessageBoxButtons.OK );
		}
		private DialogResult	MessageBox( string _Text, Exception _e )
		{
			return MessageBox( _Text + _e.Message, MessageBoxButtons.OK, MessageBoxIcon.Error );
		}
		private DialogResult	MessageBox( string _Text, MessageBoxButtons _Buttons )
		{
			return MessageBox( _Text, _Buttons, MessageBoxIcon.Information );
		}
		private DialogResult	MessageBox( string _Text, MessageBoxIcon _Icon )
		{
			return MessageBox( _Text, MessageBoxButtons.OK, _Icon );
		}
		private DialogResult	MessageBox( string _Text, MessageBoxButtons _Buttons, MessageBoxIcon _Icon )
		{
			return System.Windows.Forms.MessageBox.Show( this, _Text, "Cloud Renderer", _Buttons, _Icon );
		}

		/// <summary>
		/// Registers a disposable that will get disposed on form closing
		/// </summary>
		/// <param name="_Disposable"></param>
		private void	Reg( IDisposable _Disposable )
		{
			m_Disposables.Add( _Disposable );
		}

		#endregion

		#endregion

		#region EVENT HANDLERS

		private void viewportPanel_MouseDown( object sender, MouseEventArgs e )
		{
			Render();
		}

		#endregion
	}
}
