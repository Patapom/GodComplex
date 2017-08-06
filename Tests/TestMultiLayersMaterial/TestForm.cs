//#define	CUBIC_SPLINES
#define	POWERS

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nuaj.Cirrus.Utility;

using SharpMath;
using ImageUtility;

namespace TestMultiLayersMaterial
{
	public partial class TestForm : Form {

		const uint	LAYERS_COUNT = 4;
		const uint	MASK_SIZE = 512;
		const int	BRUSH_HALF_SIZE = 50;
		const uint	LEVEL_BUCKETS_COUNT = 4096;

		float4[][,]		m_layers = new float4[LAYERS_COUNT][,];

		float[,]		m_mask = new float[MASK_SIZE,MASK_SIZE];
//		float[,]		m_buttonDownMask = new float[MASK_SIZE,MASK_SIZE];
		ImageFile		m_imageMask = new ImageFile( MASK_SIZE, MASK_SIZE, PIXEL_FORMAT.R8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
		ImageFile		m_imageMaterial = new ImageFile( MASK_SIZE, MASK_SIZE, PIXEL_FORMAT.BGRA8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
		ImageFile		m_imageLevels = null;

		public TestForm() {
			InitializeComponent();

//DecodeArduinoSerialPacket();

			m_imageLevels = new ImageFile( (uint) panelOutputLevels.Width, (uint) panelOutputLevels.Height, PIXEL_FORMAT.BGRA8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );

			// Read layer materials
			for ( uint layerIndex=0; layerIndex < LAYERS_COUNT; layerIndex++ ) {
				using ( ImageFile I = new ImageFile( new System.IO.FileInfo( "Layer" + layerIndex + ".png" ) ) ) {
					float4[,]	layer = new float4[I.Width,I.Height];
					m_layers[layerIndex] = layer;
					I.ReadPixels( ( uint X, uint Y, ref float4 _color ) => { layer[X,Y] = _color; } );
				}
			}

			// Create default gradient mask
			for ( uint Y=0; Y < MASK_SIZE; Y++ )
				for ( uint X=0; X < MASK_SIZE; X++ )
					m_mask[X,Y] = (float) (0.5f + X) / MASK_SIZE;

			UpdateBrush();
			UpdateLevels();
			UpdateMask();
			UpdateMaterial();
		}

		float[,]	m_brush = new float[1+2*BRUSH_HALF_SIZE,1+2*BRUSH_HALF_SIZE];
		float[,]	m_eraser = new float[1+2*BRUSH_HALF_SIZE,1+2*BRUSH_HALF_SIZE];
		void	UpdateBrush() {
			float	valueBrush = floatTrackbarControlBrushStrength.Value;
//			float	valueEraser = -0.2f;
			float	valueEraser = -valueBrush;
			float	k = -0.0015f;
			for ( int dy=-BRUSH_HALF_SIZE; dy <= BRUSH_HALF_SIZE; dy++ ) {
				float	Vy = (float) Math.Exp( k * dy * dy );
				for ( int dx=-BRUSH_HALF_SIZE; dx <= BRUSH_HALF_SIZE; dx++ ) {
					float	Vx = (float) Math.Exp( k * dx * dx );
					float	V = Vx * Vy;
					m_brush[BRUSH_HALF_SIZE+dx,BRUSH_HALF_SIZE+dy] = valueBrush * V;
					m_eraser[BRUSH_HALF_SIZE+dx,BRUSH_HALF_SIZE+dy] = valueEraser * V;
				}
			}
		}

		void	SetHermite( ref float4 v, float _P0, float _T0, float _P1, float _T1 ) {

		}

		float[]		m_levels = new float[LEVEL_BUCKETS_COUNT];
		void	UpdateLevels() {
// 			const uint	BUCKETS_PER_LAYER = LEVEL_BUCKETS_COUNT / LAYERS_COUNT;
// 
// 			float	normalizer = 1.0f / (LAYERS_COUNT-1);
// 			uint	bucketIndex = 0;
// 			for ( uint layerIndex=0; layerIndex < LAYERS_COUNT; layerIndex++ ) {
// 				for ( uint i=0; i < BUCKETS_PER_LAYER; i++ ) {
// 					float	t = (float) i / BUCKETS_PER_LAYER;
// 					m_levels[bucketIndex++] = normalizer * (layerIndex + t);
// 				}
// 			}

			#if CUBIC_SPLINES
				float	F = 4.0f;
				float	T0 = F * (0*1.0f + floatTrackbarControlTangent0.Value);
				float	T1_in = F * (0*1.0f + floatTrackbarControlTangent1.Value);
				float	T1_out = checkBoxSplit1.Checked ? F * (0*1.0f + floatTrackbarControlTangent1_Out.Value) : T1_in;
				float	T2_in = F * (0*1.0f + floatTrackbarControlTangent2.Value);
				float	T2_out = checkBoxSplit2.Checked ? F * (0*1.0f + floatTrackbarControlTangent2_Out.Value) : T2_in;
				float	T3 = F * (0*1.0f + floatTrackbarControlTangent3.Value);

				float4[]	hermites = new float4[6];
// 				hermites[0].Set( 0.0f, T0, 1.0f, T1 );
// 				hermites[1].Set( 0.0f, T1, 1.0f, T2 );
// 				hermites[2].Set( 0.0f, T2, 1.0f, T3 );

				hermites[0].Set( 0.0f, T0, 0.5f, -0.5f * (T0-T1_in) );
				hermites[1].Set( 0.5f, -0.5f * (T0-T1_in), 1.0f, T1_in );
				hermites[2].Set( 0.0f, T1_out, 0.5f, 0.5f * (T1_out+T2_in) );
				hermites[3].Set( 0.5f, 0.5f * (T1_out+T2_in), 1.0f, T2_in );
				hermites[4].Set( 0.0f, T2_out, 0.5f, 0.5f * (T2_out+T3) );
				hermites[5].Set( 0.5f, 0.5f * (T2_out+T3), 1.0f, T3 );
			#elif POWERS
// 				float[]		powers = new float[LAYERS_COUNT-1];
// 				powers[0] = floatTrackbarControlTangent0.Value;
// 				powers[1] = floatTrackbarControlTangent1.Value;
// 				powers[2] = floatTrackbarControlTangent2.Value;
// 				powers[0] = powers[0] < 0.0f ? 1.0f / (1.0f - 9.0f * powers[0]) : 1.0f + 9.0f * powers[0];
// 				powers[1] = powers[1] < 0.0f ? 1.0f / (1.0f - 9.0f * powers[1]) : 1.0f + 9.0f * powers[1];
// 				powers[2] = powers[2] < 0.0f ? 1.0f / (1.0f - 9.0f * powers[2]) : 1.0f + 9.0f * powers[2];

				float[]		powers = new float[LAYERS_COUNT-1];
				float[]		scalesX = new float[LAYERS_COUNT-1];
				float[]		scalesY = new float[LAYERS_COUNT-1];
				powers[0] = floatTrackbarControlTangent0.Value;
				powers[1] = floatTrackbarControlTangent1.Value;
				powers[2] = floatTrackbarControlTangent2.Value;
// 				powers[0] = powers[0] < 0.0f ? 1.0f / (1.0f - 9.0f * powers[0]) : 1.0f + 9.0f * powers[0];
// 				powers[1] = powers[1] < 0.0f ? 1.0f / (1.0f - 9.0f * powers[1]) : 1.0f + 9.0f * powers[1];
// 				powers[2] = powers[2] < 0.0f ? 1.0f / (1.0f - 9.0f * powers[2]) : 1.0f + 9.0f * powers[2];
				powers[0] = (float) Math.Pow( 10.0, 3.0 * powers[0] );
				powers[1] = (float) Math.Pow( 10.0, 3.0 * powers[1] );
				powers[2] = (float) Math.Pow( 10.0, 3.0 * powers[2] );
				scalesX[0] = 1.0f;// + 0.5f * Math.Max( 0.0f,  floatTrackbarControlTangent0.Value );
				scalesY[0] = 1.0f;// + 0.5f * Math.Max( 0.0f, -floatTrackbarControlTangent0.Value );
				scalesX[1] = 1.0f;// + 0.5f * Math.Max( 0.0f,  floatTrackbarControlTangent1.Value );
				scalesY[1] = 1.0f;// + 0.5f * Math.Max( 0.0f, -floatTrackbarControlTangent1.Value );
				scalesX[2] = 1.0f;// + 0.5f * Math.Max( 0.0f,  floatTrackbarControlTangent2.Value );
				scalesY[2] = 1.0f;// + 0.5f * Math.Max( 0.0f, -floatTrackbarControlTangent2.Value );

			#endif

 			for ( uint bucketIndex=0; bucketIndex < LEVEL_BUCKETS_COUNT; bucketIndex++ ) {
				float	maskIn = (float) bucketIndex / (LEVEL_BUCKETS_COUNT-1);

				float	scaledMask = (LAYERS_COUNT-1) * maskIn;
				uint	layerStart = Math.Min( LAYERS_COUNT-2, (uint) Math.Floor( scaledMask ) );
				float	t = scaledMask - layerStart;

				#if CUBIC_SPLINES
					// Compute Hermite curves
// 					float4	hermite = hermites[layerStart];
// 					float	maskOut = hermite.x * (1+2*t)*(1-t)*(1-t)
// 									+ hermite.y * t*(1-t)*(1-t)
// 									+ hermite.z * t*t*(3-2*t)
// 									+ hermite.w * t*t*(t-1);

					t = 2.0f * t;
					uint	hermiteIndex = 2*layerStart;
					if ( t > 1.0f ) {
						hermiteIndex++;
						t -= 1.0f;
					}
					float4	hermite = hermites[hermiteIndex];
					float	maskOut = hermite.x * (1+2*t)*(1-t)*(1-t)
									+ hermite.y * t*(1-t)*(1-t)
									+ hermite.z * t*t*(3-2*t)
									+ hermite.w * t*t*(t-1);

				#elif POWERS
// 					// Apply curves
// 					float	tIn = 2.0f * t - 1.0f;
// 					float	tOut = Math.Sign( tIn ) * (float) Math.Pow( Math.Abs( tIn ), powers[layerStart] );
// 					float	maskOut = (layerStart + 0.5f * (1.0f + tOut)) / (LAYERS_COUNT-1);

					float	maskOut = scalesY[layerStart] * (float) Math.Pow( t / scalesX[layerStart], powers[layerStart] );
				#endif

						maskOut = Math.Max( 0, Math.Min( 1, maskOut ) );
						maskOut = (layerStart + maskOut) / (LAYERS_COUNT-1);
				m_levels[bucketIndex] = maskOut;
			}

			// Redraw levels
			float2	rangeX = new float2( 0, 1 );
			float2	rangeY = new float2( 0, 1 );
			m_imageLevels.Clear( float4.One );
			m_imageLevels.PlotAxes( float4.UnitW, rangeX, rangeY, 1.0f / (LAYERS_COUNT-1), 1.0f / (LAYERS_COUNT-1) );
			m_imageLevels.PlotGraph( float4.UnitW, rangeX, rangeY, ( float x ) => { return m_levels[Math.Min( LEVEL_BUCKETS_COUNT-1, (uint) ((LEVEL_BUCKETS_COUNT-1) * x) )]; } );
			for ( int layerIndex=1; layerIndex < LAYERS_COUNT; layerIndex++ ) {
				m_imageLevels.DrawLine( new float4( 0.2f, 0.5f, 0.75f, 1 ), m_imageLevels.RangedCoordinates2ImageCoordinates( rangeX, rangeY, new float2( (float) layerIndex / (LAYERS_COUNT-1), 0 ) ), m_imageLevels.RangedCoordinates2ImageCoordinates( rangeX, rangeY, new float2( (float) layerIndex / (LAYERS_COUNT-1), 1 ) ) );
				m_imageLevels.DrawLine( new float4( 0.2f, 0.5f, 0.75f, 1 ), m_imageLevels.RangedCoordinates2ImageCoordinates( rangeX, rangeY, new float2( 0, (float) layerIndex / (LAYERS_COUNT-1) ) ), m_imageLevels.RangedCoordinates2ImageCoordinates( rangeX, rangeY, new float2( 1, (float) layerIndex / (LAYERS_COUNT-1) ) ) );
			}
			panelOutputLevels.m_bitmap = m_imageLevels.AsBitmap;
			panelOutputLevels.Refresh();
		}

		void	UpdateMask() {
			m_imageMask.WritePixels( ( uint X, uint Y, ref float4 _color ) => {
				float	mask = m_mask[X,Y];
				_color.Set( mask, mask, mask, 1 );
			} );
			panelOutputMask.m_bitmap = m_imageMask.AsBitmap;
			panelOutputMask.Refresh();
		}

		void	UpdateMaterial() {
			m_imageMaterial.WritePixels( ( uint X, uint Y, ref float4 _color ) => {
				float	mask = m_mask[X,Y];
				float	leveledMask = m_levels[Math.Min( LEVEL_BUCKETS_COUNT-1, (uint) (mask * LEVEL_BUCKETS_COUNT) )];

				float	scaledMask = (LAYERS_COUNT-1) * leveledMask;
				uint	layerStart = Math.Min( LAYERS_COUNT-2, (uint) Math.Floor( scaledMask ) );
				float	t = scaledMask - layerStart;
				_color = (1.0f - t) * m_layers[layerStart][X,Y] + t * m_layers[layerStart+1][X,Y];

// 				if ( mask < 0.2f )
// 					_color = m_layers[0][X,Y];
// 				else if ( mask < 0.4f )
// 					_color = m_layers[1][X,Y];
// 				else if ( mask < 0.6f )
// 					_color = m_layers[2][X,Y];
// 				else
// 					_color = m_layers[3][X,Y];
			} );
			panelOutputResult.m_bitmap = m_imageMaterial.AsBitmap;
			panelOutputResult.Refresh();
		}

		private void floatTrackbarControlTangent0_ValueChanged_1(FloatTrackbarControl _Sender, float _fFormerValue) {
			UpdateLevels();
			UpdateMaterial();
		}

		MouseButtons	m_butttonDown = System.Windows.Forms.MouseButtons.None;
		private void panelOutputMask_MouseDown(object sender, MouseEventArgs e) {
			panelOutputMask.Capture = true;
			m_butttonDown |= e.Button;
//			Array.Copy( m_mask, m_buttonDownMask, MASK_SIZE*MASK_SIZE );
//			m_mask.CopyTo( m_buttonDownMask, 0 );
		}

		private void panelOutputMask_MouseMove(object sender, MouseEventArgs e) {
			float[,]	brush = null;
			if ( (m_butttonDown & System.Windows.Forms.MouseButtons.Left) != 0 ) {
				// Brush
				brush = m_brush;
			} else if ( (m_butttonDown & System.Windows.Forms.MouseButtons.Right) != 0 ) {
				// Eraser
				brush = m_eraser;
			}
			if ( brush == null )
				return;

			// Give a stroke
			for ( int dy=-BRUSH_HALF_SIZE; dy <= BRUSH_HALF_SIZE; dy++ ) {
				int	Y = e.Y + dy;
				if ( Y < 0 || Y >= MASK_SIZE )
					continue;
				for ( int dx=-BRUSH_HALF_SIZE; dx <= BRUSH_HALF_SIZE; dx++ ) {
					int	X = e.X + dx;
					if ( X < 0 || X >= MASK_SIZE )
						continue;

					float	newV = m_mask[X,Y] + brush[BRUSH_HALF_SIZE+dx,BRUSH_HALF_SIZE+dy];
					m_mask[X,Y] = Math.Max( 0.0f, Math.Min( 1.0f, newV ) );
				}
			}

			UpdateMask();
			UpdateMaterial();
		}

		private void panelOutputMask_MouseUp(object sender, MouseEventArgs e) {
			m_butttonDown &= ~e.Button;
			panelOutputMask.Capture = m_butttonDown != System.Windows.Forms.MouseButtons.None;
		}

		private void floatTrackbarControlBrushStrength_ValueChanged(FloatTrackbarControl _Sender, float _fFormerValue) {
			UpdateBrush();
		}

		private void buttonClear_Click(object sender, EventArgs e) {
			// Create default gradient mask
			for ( uint Y=0; Y < MASK_SIZE; Y++ )
				for ( uint X=0; X < MASK_SIZE; X++ )
					m_mask[X,Y] = 0.0f;
			UpdateMask();
			UpdateMaterial();
		}

		private void buttonClearGradient_Click(object sender, EventArgs e) {
			// Create default gradient mask
			for ( uint Y=0; Y < MASK_SIZE; Y++ )
				for ( uint X=0; X < MASK_SIZE; X++ )
					m_mask[X,Y] = (float) (0.5f + X) / MASK_SIZE;
			UpdateMask();
			UpdateMaterial();
		}

		private void buttonResetLevels_Click(object sender, EventArgs e) {
			floatTrackbarControlTangent0.Value = 0.0f;
			floatTrackbarControlTangent1.Value = 0.0f;
			floatTrackbarControlTangent1_Out.Value = 0.0f;
			floatTrackbarControlTangent2.Value = 0.0f;
			floatTrackbarControlTangent2_Out.Value = 0.0f;
			floatTrackbarControlTangent3.Value = 0.0f;
		}

		private void checkBoxSplit1_CheckedChanged(object sender, EventArgs e) {
			floatTrackbarControlTangent1_Out.Enabled = checkBoxSplit1.Checked;
			UpdateLevels();
			UpdateMaterial();
		}

		private void checkBoxSplit2_CheckedChanged(object sender, EventArgs e) {
			floatTrackbarControlTangent2_Out.Enabled = checkBoxSplit2.Checked;
			UpdateLevels();
			UpdateMaterial();
		}

		float4[,]	LoadImage() {
			if ( openFileDialog.ShowDialog( this ) != System.Windows.Forms.DialogResult.OK )
				return null;

			try {
				using ( ImageFile I = new ImageFile( new System.IO.FileInfo( openFileDialog.FileName ) ) ) {
					float4[,]	result = new float4[I.Width,I.Height];
					I.ReadPixels( ( uint _X, uint _Y, ref float4 _color ) => { result[_X,_Y] = _color; } );
					if ( I.Width != MASK_SIZE || I.Height != MASK_SIZE ) {
						// Resize
						float4[,]	wrongResult = result;
						result = new float4[MASK_SIZE,MASK_SIZE];
						for ( uint Y=0; Y < MASK_SIZE; Y++ )
							for ( uint X=0; X < MASK_SIZE; X++ )
								ImageFile.BilerpClamp( wrongResult, (float) X * I.Width / MASK_SIZE, (float) Y * I.Height / MASK_SIZE, ref result[X,Y] );
					}

					return result;
				}
			} catch ( Exception _e ) {
				MessageBox.Show( this, "An error occurred:\r\n" + _e.Message, "Painter Test", MessageBoxButtons.OK );
			}

			return null;
		}

		private void buttonLoadMat0_Click(object sender, EventArgs e) {
			float4[,]	material = LoadImage();
			if ( material != null ) {
				m_layers[0] = material;
				UpdateMaterial();
			}
		}

		private void buttonLoadMat1_Click(object sender, EventArgs e) {
			float4[,]	material = LoadImage();
			if ( material != null ) {
				m_layers[1] = material;
				UpdateMaterial();
			}
		}

		private void buttonLoadMat2_Click(object sender, EventArgs e) {
			float4[,]	material = LoadImage();
			if ( material != null ) {
				m_layers[2] = material;
				UpdateMaterial();
			}
		}

		private void buttonLoadMat3_Click(object sender, EventArgs e) {
			float4[,]	material = LoadImage();
			if ( material != null ) {
				m_layers[3] = material;
				UpdateMaterial();
			}
		}

		private void buttonLoadMask_Click(object sender, EventArgs e) {
			float4[,]	mask = LoadImage();
			if ( mask != null ) {
				for ( uint Y=0; Y < MASK_SIZE; Y++ )
					for ( uint X=0; X < MASK_SIZE; X++ )
						m_mask[X,Y] = mask[X,Y].y;
				UpdateMask();
				UpdateMaterial();
			}
		}

		private void buttonSaveMask_Click(object sender, EventArgs e) {
			if ( saveFileDialog.ShowDialog( this ) != System.Windows.Forms.DialogResult.OK )
				return;

			try {
				using ( ImageFile I = new ImageFile( MASK_SIZE, MASK_SIZE, PIXEL_FORMAT.R8, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) ) ) {
					I.WritePixels( ( uint _X, uint _Y, ref float4 _color ) => { float V = m_mask[_X,_Y]; _color.Set( V, V, V, 1 ); } );
					I.Save( new System.IO.FileInfo( saveFileDialog.FileName ) );
				}
			} catch ( Exception _e ) {
				MessageBox.Show( this, "An error occurred:\r\n" + _e.Message, "Painter Test", MessageBoxButtons.OK );
			}
		}
	}
}
