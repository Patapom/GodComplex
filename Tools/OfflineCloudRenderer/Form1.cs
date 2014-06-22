using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using RendererManaged;

namespace OfflineCloudRenderer
{
	public partial class Form1 : Form
	{
		private Device				m_Device = new Device();

		private ComputeShader		m_Shader = null;

		private List<IDisposable>	m_Disposables = new List<IDisposable>();

		public Form1()
		{
			InitializeComponent();
		}

		protected override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );

			m_Device.Init( Handle, Width, Height, false, true );
			m_Device.Clear( Color.SkyBlue );

			Reg( m_Shader = new ComputeShader( m_Device, new ShaderFile( new System.IO.FileInfo( @"Shaders/Test/TestCompute.hlsl" ) ), "CS", null ) );
		}

		protected override void OnClosing( CancelEventArgs e )
		{
			foreach ( IDisposable D in m_Disposables )
				D.Dispose();

			m_Device.Exit();
			m_Device = null;

			base.OnClosing( e );
		}

		private void	Reg( IDisposable _Disposable )
		{
			m_Disposables.Add( _Disposable );
		}

		protected override void OnPaintBackground( PaintEventArgs e )
		{
//			base.OnPaintBackground( e );
		}

		protected override void OnPaint( PaintEventArgs e )
		{
//			base.OnPaint( e );

			if ( m_Device != null )
				m_Device.Present();
		}
	}
}
