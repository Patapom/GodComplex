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
		public struct	CB_Input
		{
			public float4x4		World2Proj;
			public float4x4		Proj2World;
		}

		#endregion

		#region FIELDS

		private RegistryKey					m_AppKey;
		private string						m_ApplicationPath;

		private Device						m_Device = new Device();

		private ComputeShader				m_CS = null;
		private Shader						m_PS = null;
		private ConstantBuffer<CB_Input>	m_CB = null;

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

			m_Device.Init( viewportPanel.Handle, Width, Height, false, true );
			m_Device.Clear( Color.SkyBlue );

			Reg( m_CS = new ComputeShader( m_Device, new ShaderFile( new System.IO.FileInfo( @"Shaders/Test/TestCompute.hlsl" ) ), "CS", null ) );
			Reg( m_PS = new Shader( m_Device, new ShaderFile( new System.IO.FileInfo( @"Shaders/Test/TestFullscreenQuad.hlsl" ) ), VERTEX_FORMAT.Pt4, "VS", "PS", null ) );
			Reg( m_CB = new ConstantBuffer<CB_Input>( m_Device, 0 ) );
		}

		protected override void OnClosing( CancelEventArgs e )
		{
			foreach ( IDisposable D in m_Disposables )
				D.Dispose();

			m_Device.Exit();
//			m_Device = null;

			base.OnClosing( e );
		}

		private void	Render()
		{
			// Setup camera matrix
			Matrix4x4	Camera2World = new Matrix4x4();
			Camera2World.MakeLookAt( new WMath.Point( 0.0f, 0.0f, 4.0f ), new WMath.Point( 0, 0, 0 ), new Vector( 0, 1, 0 ) );

			Matrix4x4	Camera2Proj = new Matrix4x4();
			Camera2Proj.MakeProjectionPerspective( 60.0f * (float) Math.PI / 180.0f, (float) Width / Height, 0.1f, 10.0f );

			Matrix4x4	World2Proj = Camera2World.Inverse * Camera2Proj;
			Matrix4x4	Proj2World = World2Proj.Inverse;

			m_CB.m.World2Proj.FromMatrix4( World2Proj );
			m_CB.m.Proj2World.FromMatrix4( Proj2World );
			m_CB.UpdateData();

			// Setup default render target as UAV & render using the compute shader


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
