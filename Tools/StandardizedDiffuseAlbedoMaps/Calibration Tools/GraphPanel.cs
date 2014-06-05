using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace StandardizedDiffuseAlbedoMaps
{
	public partial class GraphPanel : Panel
	{
		private Bitmap		m_Bitmap = null;
		private Pen			m_PenMainAxes = new Pen( Color.Black, 1.0f );
		private Pen			m_PenProbeAxes = new Pen( Color.FromArgb( 200, 200, 200 ), 1.0f );
		private Pen			m_PenProbeIdeal = new Pen( Color.FromArgb( 180, 180, 180 ), 1.0f );
		private Brush		m_BrushProbeIdeal = new SolidBrush( Color.FromArgb( 180, 180, 180 ) );
		private Pen			m_PenProbeMeasured = new Pen( Color.FromArgb( 0, 0, 0 ), 1.0f );
		private Brush		m_BrushProbeMeasured = new SolidBrush( Color.FromArgb( 0, 0, 0 ) );


		private CameraCalibration	m_CameraCalibration = null;
		public CameraCalibration	Calibration
		{
			get { return m_CameraCalibration; }
			set {
				m_CameraCalibration = value;
				UpdateGraph();
			}
		}

		private bool	m_UseLagrange = false;
		public bool		UseLagrange
		{
			get { return m_UseLagrange; }
			set {
				m_UseLagrange = value;
				UpdateGraph();
			}
		}

		public GraphPanel( IContainer container )
		{
			container.Add( this );
			InitializeComponent();
			OnSizeChanged( EventArgs.Empty );
		}

		public void		UpdateGraph()
		{
			if ( m_Bitmap == null )
				return;

			int		W = m_Bitmap.Width;
			int		H = m_Bitmap.Height;

			using ( Graphics G = Graphics.FromImage( m_Bitmap ) )
			{
				using ( SolidBrush B = new SolidBrush( BackColor ) )
					G.FillRectangle( B, 0, 0, W, H );

				int	X0 = 10;
				int	X1 = W - 10;
				int	Y0 = H - 10;
				int	Y1 = 10;

				// Draw ideal probes line
				G.DrawLine( m_PenProbeIdeal, X0, Y0, X1, Y1 );

				if ( m_CameraCalibration != null )
				{
					// Draw ideal probe reflectances
					CameraCalibration.Probe[]	Probes = m_CameraCalibration.m_Reflectances;
					for ( int i=0; i < Probes.Length; i++ )
					{
						float	X = X0 + (X1 - X0) * Probes[i].StandardReflectance;
						float	Y = Y0 + (Y1 - Y0) * Probes[i].StandardReflectance;
						G.FillEllipse( m_BrushProbeIdeal, X-2, Y-2, 4, 4 );
					}

					// Draw measured probe reflectances
					if ( m_UseLagrange )
					{
						float	PreviousY = Y0;
						for ( int X=X0+1; X <= X1; X++ )
						{
							float	x = (float) (X - X0) / (X1 - X0);
							float	y = 0.0f;
							for ( int i=0; i < Probes.Length; i++ )
							{
								float	Product = 1.0f;
								for ( int j=0; j < Probes.Length; j++ )
									if ( j != i )
										Product *= (x - Probes[j].StandardReflectance) / (Probes[i].StandardReflectance - Probes[j].StandardReflectance);
								y += Probes[i].m_LuminanceNormalized * Product;
							}

							float	Y = Y0 + y * (Y1 - Y0);

							G.DrawLine( m_PenProbeMeasured, X-1, PreviousY, X, Y );
							PreviousY = Y;
						}
					}
					else
					{
						for ( int i=1; i < Probes.Length; i++ )
						{
							float	Xs = X0 + (X1 - X0) * Probes[i-1].StandardReflectance;
							float	Ys = Y0 + (Y1 - Y0) * Probes[i-1].m_LuminanceNormalized;
							float	Xe = X0 + (X1 - X0) * Probes[i].StandardReflectance;
							float	Ye = Y0 + (Y1 - Y0) * Probes[i].m_LuminanceNormalized;
							G.DrawLine( m_PenProbeMeasured, Xs, Ys, Xe, Ye );
						}
					}

					for ( int i=0; i < Probes.Length; i++ )
					{
						float	X = X0 + (X1 - X0) * Probes[i].StandardReflectance;
						float	Y = Y0 + (Y1 - Y0) * Probes[i].m_LuminanceNormalized;
						G.FillEllipse( m_BrushProbeMeasured, X-2, Y-2, 4, 4 );
					}

					G.DrawString( "ISO = " + m_CameraCalibration.m_CameraShotInfos.m_ISOSpeed.ToString(), Font, Brushes.Black, X0 + 8, Y1 + 0 );
					G.DrawString( "Shutter Speed = " + (m_CameraCalibration.m_CameraShotInfos.m_ShutterSpeed < 1.0f ? "1/"+((int)(1.0f/m_CameraCalibration.m_CameraShotInfos.m_ShutterSpeed)).ToString( "G4" ) : m_CameraCalibration.m_CameraShotInfos.m_ShutterSpeed.ToString( "G4" )), Font, Brushes.Black, X0 + 8, Y1 + 1*8 );
					G.DrawString( "Aperture= " + m_CameraCalibration.m_CameraShotInfos.m_Aperture.ToString( "G4" ), Font, Brushes.Black, X0 + 8, Y1 + 2*8 );
				}

				// Draw main axes
				G.DrawLine( m_PenMainAxes, X0, 0, X0, H );
				G.DrawLine( m_PenMainAxes, 0, Y0, W, Y0 );
			}

			Invalidate();
		}

		protected override void OnSizeChanged( EventArgs e )
		{
			if ( m_Bitmap != null )
				m_Bitmap.Dispose();
			m_Bitmap = null;

			if ( Width > 0 && Height > 0 )
				m_Bitmap = new Bitmap( Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb );

			UpdateGraph();

			base.OnSizeChanged( e );
		}

		protected override void OnPaintBackground( PaintEventArgs e )
		{
//			base.OnPaintBackground( e );
		}

		protected override void OnPaint( PaintEventArgs e )
		{
			base.OnPaint( e );

			if ( m_Bitmap != null )
				e.Graphics.DrawImage( m_Bitmap, 0, 0 );

			if ( m_CameraCalibration == null )
				return;

			int	X0 = 10;
			int	X1 = Width - 10;
			int	Y0 = Height - 10;
			int	Y1 = 10;

			Point	P = PointToClient( Control.MousePosition );
			float	x = (float) (P.X - X0) / (X1 - X0);
			float	StandardReflectance = 0.02f + (0.99f - 0.02f) * x;
			for ( int i=1; i < m_CameraCalibration.m_Reflectances.Length; i++ )
			{
				if ( StandardReflectance < m_CameraCalibration.m_Reflectances[i-1].StandardReflectance || StandardReflectance > m_CameraCalibration.m_Reflectances[i].StandardReflectance )
					continue;

				float	t = (StandardReflectance - m_CameraCalibration.m_Reflectances[i-1].StandardReflectance) / (m_CameraCalibration.m_Reflectances[i].StandardReflectance - m_CameraCalibration.m_Reflectances[i-1].StandardReflectance);
				float	Y = m_CameraCalibration.m_Reflectances[i-1].m_LuminanceMeasured + t * (m_CameraCalibration.m_Reflectances[i].m_LuminanceMeasured - m_CameraCalibration.m_Reflectances[i-1].m_LuminanceMeasured);

				e.Graphics.DrawString( "Y=" + Y.ToString( "G4" ), Font, Brushes.Black, P.X + 6, P.Y - 16);
				e.Graphics.DrawString( "Reflectance=" + (100.0f * StandardReflectance).ToString( "G4" ) + "%", Font, Brushes.Black, P.X + 6, P.Y - 8 );
				break;
			}
		}

		protected override void OnMouseMove( MouseEventArgs e )
		{
			Invalidate();
			base.OnMouseMove( e );
		}
	}
}
