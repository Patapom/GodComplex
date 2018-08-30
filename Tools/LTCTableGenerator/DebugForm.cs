using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SharpMath;
using ImageUtility;

namespace LTCTableGenerator
{
	public partial class DebugForm : Form
	{
		LTC[,]			m_results = null;
		public LTC[,]	Results {
			get { return m_results; }
			set { m_results = value; }
		}

		ImageFile		m_bitmap0;
		ImageFile		m_bitmap1;
		ImageFile		m_bitmap2;
		ImageFile		m_bitmap3;

		public DebugForm()
		{
			InitializeComponent();

			m_bitmap0 = new ImageFile( (uint) panelOutput0.Width, (uint) panelOutput0.Height, PIXEL_FORMAT.RGBA8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
			m_bitmap1 = new ImageFile( (uint) panelOutput1.Width, (uint) panelOutput1.Height, PIXEL_FORMAT.RGBA8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
			m_bitmap2 = new ImageFile( (uint) panelOutput2.Width, (uint) panelOutput2.Height, PIXEL_FORMAT.RGBA8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
			m_bitmap3 = new ImageFile( (uint) panelOutput3.Width, (uint) panelOutput3.Height, PIXEL_FORMAT.RGBA8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="_roughnessIndex"></param>
		public void	DebugRoughness( int _roughnessIndex, string _topic ) {
			
			Text = "Debug Roughness" + _topic;

			Plot( _roughnessIndex, 0, 0, m_bitmap0, panelOutput0, label0 );
			Plot( _roughnessIndex, 2, 0, m_bitmap1, panelOutput1, label1 );
			Plot( _roughnessIndex, 0, 2, m_bitmap2, panelOutput2, label2 );
			Plot( _roughnessIndex, 2, 2, m_bitmap3, panelOutput3, label3 );
		}

		void	Plot( int _roughnessIndex, int _row, int _column, ImageFile _image, PanelOutput _panel, Label _label ) {
			_image.Clear( float4.One );

			// Determine range first
			int		W = m_results.GetLength(0);
			float[]	values = new float[W];
			float	min = float.MaxValue;
			float	max = -float.MaxValue;
			for ( int X=0; X < W; X++ ) {
				LTC		ltc = m_results[_roughnessIndex,X];
				double	term = ltc.invM[_row,_column];
						term /= ltc.invM[1,1];	// Normalize by central term
				values[X] = (float) term;
				min = Math.Min( min, values[X] );
				max = Math.Max( max, values[X] );
			}
			float2	rangeY = float2.UnitY;
			rangeY.x = Math.Min( rangeY.x, min );
			rangeY.y = Math.Max( rangeY.y, max );

			// Plot graph and update UI
			float2	rangeX = new float2( 0, 1 );
			_image.PlotGraph( new float4( 0, 0, 0, 1 ), rangeX, rangeY, ( float _X ) => {
				int	X = Math.Min( W-1, (int) (W * _X) );
				return values[X];
			} );
			_image.PlotAxes( float4.UnitW, rangeX, rangeY, 0.1f, 0.1f * (rangeY.y - rangeY.x) );
			_label.Text = "Coefficient m" + _row + "" + _column + " - Range [" + rangeY.x + ", " + rangeY.y + "]";
			_panel.m_bitmap = _image.AsBitmap;
			_panel.Refresh();
		}
	}
}
