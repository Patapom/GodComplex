#define BISOU

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

namespace TestWaveletATrousFiltering
{
	public partial class Form1 : Form
	{
		ImageFile		m_imageInput;
		ImageFile		m_imageSparseInput;
		ImageFile		m_imageReconstructedOutput;
		ImageFile		m_imageDensity;

		float4[,]		m_sparsePixels;
		float4[][,]		m_inputPixels = new float4[9][,];
		float4[][,]		m_outputPixels = new float4[9][,];

		public Form1() {
			InitializeComponent();

			m_imageInput = new ImageFile( new System.IO.FileInfo( "Parrot.png" ) );
			panelInputImage.m_bitmap = m_imageInput.AsBitmap;

			uint	W = m_imageInput.Width;
			uint	H = m_imageInput.Height;

			//////////////////////////////////////////////////////////////////////////
			// Build the sparse input image which is the original image but with many holes in pixels
			m_imageSparseInput = new ImageFile( W, H, PIXEL_FORMAT.BGRA8, m_imageInput.ColorProfile );
			m_imageSparseInput.Clear( float4.UnitW );

			m_sparsePixels = new float4[W,H];
			m_imageInput.ReadPixels( ( uint X, uint Y, ref float4 _color ) => { m_sparsePixels[X,Y] = _color; } );
			for ( uint Y=0; Y < H; Y++ ) {
				float	fY = 2.0f * Y / H - 1.0f;
				for ( uint X=0; X < W; X++ ) {
					float	fX = 2.0f * X / W - 1.0f;
					float	gauss = (float) Math.Exp( -5.0f * (fX*fX + fY*fY) );
					float	weight = SimpleRNG.GetUniform() < gauss ? 1.0f : 0.0f;
					m_sparsePixels[X,Y].x *= weight;
					m_sparsePixels[X,Y].y *= weight;
					m_sparsePixels[X,Y].z *= weight;
					m_sparsePixels[X,Y].w *= weight;
				}
			}

// Triangle test
// m_sparsePixels[100, 50] = new float4( 1, 0, 0, 1.0f );
// m_sparsePixels[50, 160] = new float4( 1, 1, 0, 1.0f );
// m_sparsePixels[200, 128] = new float4( 0, 1, 1, 1.0f );

// for ( int i=0; i < 20; i++ ) {
// 	int	X = (int) (128 + 80 * Math.Cos( 2.0 * Math.PI * i / 20 ));
// 	int	Y = (int) (128 + 80 * Math.Sin( 2.0 * Math.PI * i / 20 ));
// 	m_sparsePixels[X,Y] = new float4( 0.25f + 0.75f * (float) SimpleRNG.GetUniform(), 0.25f + 0.75f * (float) SimpleRNG.GetUniform(), 0.25f + 0.75f * (float) SimpleRNG.GetUniform(), 1.0f );
// }


			m_imageSparseInput.WritePixels( ( uint X, uint Y, ref float4 _color ) => { _color = m_sparsePixels[X,Y]; } );


			panelSparseInputImage.m_paintBackground = true;
			panelSparseInputImage.m_bitmap = m_imageSparseInput.AsBitmap;

			//////////////////////////////////////////////////////////////////////////
			// Build reconstructed image by pull-push
			m_imageReconstructedOutput = new ImageFile( W, H, PIXEL_FORMAT.BGRA8, m_imageInput.ColorProfile );
			m_imageDensity = new ImageFile( W, H, PIXEL_FORMAT.BGRA8, m_imageInput.ColorProfile );
			for ( int mipLevel=0; mipLevel < 9; mipLevel++ ) {
				m_inputPixels[mipLevel] = new float4[W >> mipLevel, H >> mipLevel];
				m_outputPixels[mipLevel] = new float4[W >> mipLevel, H >> mipLevel];
			}
			PreComputeDistanceField();
			ApplyFiltering( floatTrackbarControlGamma.Value );
		}

		protected override void OnFormClosed(FormClosedEventArgs e) {
			base.OnFormClosed(e);

			m_imageInput.Dispose();
		}

		void	ApplyFiltering( float _gamma ) {
			ApplyFiltering_Original( _gamma );
		}

		#region Working but Ugly

		void	ApplyFiltering_Original( float _gamma ) {
			uint	W = m_imageInput.Width;
			uint	H = m_imageInput.Height;

// 			float	sigma = floatTrackbarControlSigma.Value;
// 			int		kernelHalfSize = (int) Math.Ceiling( 3.0f * sigma );
// 			int		kernelSize = 1 + 2 * kernelHalfSize;
// 			float[,]	kernelValues = new float[kernelSize,kernelSize];
// 			for ( int Y=-kernelHalfSize; Y <= kernelHalfSize; Y++ )
// 				for ( int X=-kernelHalfSize; X <= kernelHalfSize; X++ )
// 					kernelValues[kernelHalfSize+X,kernelHalfSize+Y] = (float) (Math.Exp( -(X*X + Y*Y) / (2 * sigma * sigma) ) / (2.0 * Math.PI * sigma * sigma) );

			int			kernelHalfSize = 2;
			float[,]	kernelValues = new float[5,5] {
				{ 1 / 273.0f, 4 / 273.0f, 7 / 273.0f, 4 / 273.0f, 1 / 273.0f },
				{ 4 / 273.0f, 16 / 273.0f, 26 / 273.0f, 16 / 273.0f, 4 / 273.0f },
				{ 7 / 273.0f, 26 / 273.0f, 41 / 273.0f, 26 / 273.0f, 7 / 273.0f },
				{ 4 / 273.0f, 16 / 273.0f, 26 / 273.0f, 16 / 273.0f, 4 / 273.0f },
				{ 1 / 273.0f, 4 / 273.0f, 7 / 273.0f, 4 / 273.0f, 1 / 273.0f },
			};


			// Initialize source pixels at mip 0
			for ( uint Y=0; Y < H; Y++ )
				for ( uint X=0; X < W; X++ )
					m_inputPixels[0][X,Y] = m_sparsePixels[X,Y];

			// ========= PULL ========= 
			// Build all mips up to 1x1
			uint	Ws = W;
			uint	Hs = H;
			for ( int i=1; i < 9; i++ ) {
				Ws >>= 1;
				Hs >>= 1;
				float4[,]	previousLevel = m_inputPixels[i-1];
				float4[,]	currentLevel = new float4[Ws,Hs];
				m_inputPixels[i] = currentLevel;

				for ( uint Y=0; Y < Hs; Y++ ) {
					for ( uint X=0; X < Ws; X++ ) {

						// Apply filtering kernel
						float4	sum = float4.Zero;
						for ( int dY=-kernelHalfSize; dY <= kernelHalfSize; dY++ ) {
							int	kY = (int) (2*Y) + dY;
							if ( kY < 0 || kY >= H )
								continue;
							for ( int dX=-kernelHalfSize; dX <= kernelHalfSize; dX++ ) {
								int	kX = (int) (2*X) + dX;
								if ( kX < 0 || kX >= W )
									continue;

								float4	V = previousLevel[kX,kY];
								float	kernel = kernelValues[kernelHalfSize+dX,kernelHalfSize+dY];
								sum += kernel * V;
							}
						}

						// Normalize filtered result
//						float	clampedWeight = Math.Min( 1.0f, sum.w );
						float	clampedWeight = 1.0f - (float) Math.Pow( Math.Max( 0.0f, 1.0f - sum.w ), _gamma );

//clampedWeight = 1.0f;

						float	ratio = sum.w > 0.0f ? clampedWeight / sum.w : 0.0f;
						sum *= ratio;
						currentLevel[X,Y] = sum;
					}
				}

				W >>= 1;
				H >>= 1;
			}

			// ========= PUSH ========= 
			// Build all mips back to finest level by re-using lower mip values
			for ( int i=8; i > 0; i-- ) {
				Ws <<= 1;
				Hs <<= 1;
				float4[,]	previousLevel = m_outputPixels[i];
				float4[,]	currentLevelIn = m_inputPixels[i-1];
				float4[,]	currentLevelOut = m_outputPixels[i-1];

				for ( uint Y=0; Y < Hs; Y++ ) {
					for ( uint X=0; X < Ws; X++ ) {

						// Apply filtering kernel
						float4	sum = float4.Zero;
						for ( int dY=-kernelHalfSize; dY <= kernelHalfSize; dY++ ) {
							int	kY = (int) (Y>>1) + dY;
							if ( kY < 0 || kY >= H )
								continue;
							for ( int dX=-kernelHalfSize; dX <= kernelHalfSize; dX++ ) {
								int	kX = (int) (X>>1) + dX;
								if ( kX < 0 || kX >= W )
									continue;

								float4	V = previousLevel[kX,kY];
								float	kernel = kernelValues[kernelHalfSize+dX,kernelHalfSize+dY];
								sum += kernel * V;
							}
						}

						// Normalize filtered result
						float	clampedWeight = Math.Min( 1.0f, sum.w );
						float	normalizer = sum.w > 0.0f ? 1.0f / clampedWeight : 0.0f;
						sum *= normalizer;

						// Blend with existing value
						float4	currentValue = currentLevelIn[X,Y];
						float4	newValue = currentValue + (1.0f - currentValue.w) * sum;
								newValue.x *= newValue.w;
								newValue.y *= newValue.w;
								newValue.z *= newValue.w;
						currentLevelOut[X,Y] = newValue;
					}
				}

				W <<= 1;
				H <<= 1;
			}

			// Display
			DisplayResult();
		}

		#endregion

		float2	m_clickedMousePosition = float2.Zero;
		void	DisplayResult() {
			uint	W = m_imageInput.Width;
			uint	H = m_imageInput.Height;


//			m_imageRecosntructedOutput.WritePixels( ( uint X, uint Y, ref float4 _color ) => { _color = m_inputPixels[0][X,Y]; } );
			float4[,]	source = checkBoxInput.Checked ? m_inputPixels[integerTrackbarControlMipLevel.Value] : m_outputPixels[integerTrackbarControlMipLevel.Value];
			m_imageReconstructedOutput.WritePixels( ( uint X, uint Y, ref float4 _color ) => {
				float	U = (float) X / W;
				float	V = (float) Y / H;
				_color = Bilerp( source, U, V );
				_color.w = 1.0f;
//				_color = Bilerp( m_outputPixels[1], U, V );
			} );


			panelOutputReconstruction.m_bitmap = m_imageReconstructedOutput.AsBitmap;
			panelOutputReconstruction.Refresh();

			// Show weight
			m_imageDensity.WritePixels( ( uint X, uint Y, ref float4 _color ) => {
				float	U = (float) X / W;
				float	V = (float) Y / H;
				float4	temp = 1.0f *  Bilerp( source, U, V );
				_color.Set( temp.w, temp.w, temp.w, 1.0f );
			} );
			panelPixelDensity.m_bitmap = m_imageDensity.AsBitmap;
			panelPixelDensity.Refresh();
		}

		float4	Bilerp( float4[,] _values, float _U, float _V ) {
			int	W = _values.GetLength(0);
			int	H = _values.GetLength(1);
			float	x = Math.Max( 0, _U * W );
			int		X = (int) Math.Floor( x );
					x -= X;
			int		Xn = Math.Min( W-1, X+1 );
			float	y = Math.Max( 0, _V * H );
			int		Y = (int) Math.Floor( y );
					y -= Y;
			int		Yn = Math.Min( H-1, Y+1 );

			float4	V00 = _values[X,Y];
			float4	V01 = _values[Xn,Y];
			float4	V10 = _values[X,Yn];
			float4	V11 = _values[Xn,Yn];
			float4	V0 = V00 + (V01-V00) * x;
			float4	V1 = V10 + (V11-V10) * x;
			float4	V = V0 + (V1 - V0) * y;
			return V;
		}

		private void floatTrackbarControlGamma_SliderDragStop(Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fStartValue) {
			ApplyFiltering( floatTrackbarControlGamma.Value );
		}

		private void integerTrackbarControlMipLevel_ValueChanged(Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue) {
			DisplayResult();
		}

		private void checkBoxInput_CheckedChanged(object sender, EventArgs e) {
			DisplayResult();
		}

		private void panelOutputReconstruction_MouseDown(object sender, MouseEventArgs e) {
			m_clickedMousePosition = new float2( (float) m_imageInput.Width * e.X / panelOutputReconstruction.Width, (float) m_imageInput.Height * e.Y / panelOutputReconstruction.Height );
			DisplayResult();
		}
	}
}
