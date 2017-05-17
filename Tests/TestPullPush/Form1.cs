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

namespace TestPullPush
{
	public partial class Form1 : Form
	{
		ImageFile		m_imageInput;
		ImageFile		m_imageSparseInput;

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
			float4[][,]	inputPixels = new float4[9][,];
			inputPixels[0] = new float4[W,H];
			m_imageInput.ReadPixels( ( uint X, uint Y, ref float4 _color ) => { inputPixels[0][X,Y] = _color; } );
			for ( uint Y=0; Y < H; Y++ ) {
				float	fY = 2.0f * Y / H - 1.0f;
				for ( uint X=0; X < W; X++ ) {
					float	fX = 2.0f * X / W - 1.0f;
					float	gauss = (float) Math.Exp( -5.0f * (fX*fX + fY*fY) );
					float	weight = SimpleRNG.GetUniform() < gauss ? 1.0f : 0.0f;
					inputPixels[0][X,Y].x *= weight;
					inputPixels[0][X,Y].y *= weight;
					inputPixels[0][X,Y].z *= weight;
					inputPixels[0][X,Y].w *= weight;
				}
			}

			m_imageSparseInput.WritePixels( ( uint X, uint Y, ref float4 _color ) => { _color = inputPixels[0][X,Y]; } );
			panelSparseInputImage.m_bitmap = m_imageSparseInput.AsBitmap;

			//////////////////////////////////////////////////////////////////////////
			// Build reconstructed image by pull-push
			float[,]	kernelValues = new float[5,5] {
				{ 1, 4, 7, 4, 1 },
				{ 4, 16, 26, 16, 4 },
				{ 7, 26, 41, 26, 7 },
				{ 4, 16, 26, 16, 4 },
				{ 1, 4, 7, 4, 1 },
			};

			// ========= PULL ========= 
			// Build all mips up to 1x1
			const float	gamma = 4.0f;

			uint	Ws = W;
			uint	Hs = H;
			for ( int i=1; i < 9; i++ ) {
				Ws >>= 1;
				Hs >>= 1;
				float4[,]	previousLevel = inputPixels[i-1];
				float4[,]	currentLevel = new float4[Ws,Hs];
				inputPixels[i] = currentLevel;

				for ( uint Y=0; Y < Hs; Y++ ) {
					for ( uint X=0; X < Ws; X++ ) {

						// Apply filtering kernel
						float4	sum = float4.Zero;
						for ( int dY=-2; dY <= 2; dY++ ) {
							int	kY = (int) (2*Y) + dY;
							if ( kY < 0 || kY >= H )
								continue;
							for ( int dX=-2; dX <= 2; dX++ ) {
								int	kX = (int) (2*X) + dX;
								if ( kX < 0 || kX >= H )
									continue;

								float4	V = previousLevel[kX,kY];
								float	kernel = 1.0f / 273.0f * kernelValues[2+dX,2+dY];
								sum += kernel * V;
							}
						}

						// Normalize filtered result
						float	clampedWeight = Math.Min( 1.0f, sum.w );
//								clampedWeight = 1.0f - (float) Math.Pow( 1.0f - sum.x, gamma );
						float	ratio = sum.x > 0.0f ? clampedWeight / sum.w : 0.0f;
						sum *= ratio;
						currentLevel[X,Y] = sum;
					}
				}

				W >>= 1;
				H >>= 1;
			}

			// ========= PUSH ========= 
			// Build all mips back to finest level by re-using lower mip values
			for ( int i=8; i > 0; i++ ) {
				Ws <<= 1;
				Hs <<= 1;
				float4[,]	previousLevel = inputPixels[i];
				float4[,]	currentLevel = inputPixels[i-1];

				for ( uint Y=0; Y < Hs; Y++ ) {
					for ( uint X=0; X < Ws; X++ ) {

						// Apply filtering kernel
						float4	sum = float4.Zero;
						for ( int dY=-2; dY <= 2; dY++ ) {
							int	kY = (int) (2*Y) + dY;
							if ( kY < 0 || kY >= H )
								continue;
							for ( int dX=-2; dX <= 2; dX++ ) {
								int	kX = (int) (2*X) + dX;
								if ( kX < 0 || kX >= H )
									continue;

								float4	V = previousLevel[kX,kY];
								float	kernel = 1.0f / 273.0f * kernelValues[2+dX,2+dY];
								sum += kernel * V;
							}
						}

						// Normalize filtered result
						float	clampedWeight = Math.Min( 1.0f, sum.w );
						float	normalizer = sum.x > 0.0f ? 1.0f / clampedWeight : 0.0f;
						sum *= normalizer;

						// Blend with existing value
						float4	currentValue = currentLevel[X,Y];

						currentLevel[X,Y] = sum;
					}
				}

				W <<= 1;
				H <<= 1;
			}
		}

		protected override void OnFormClosed(FormClosedEventArgs e) {
			base.OnFormClosed(e);

			m_imageInput.Dispose();
		}
	}
}
