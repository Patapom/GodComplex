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

			m_imageSparseInput.WritePixels( ( uint X, uint Y, ref float4 _color ) => { _color = m_sparsePixels[X,Y]; } );
			panelSparseInputImage.m_bitmap = m_imageSparseInput.AsBitmap;

			//////////////////////////////////////////////////////////////////////////
			// Build reconstructed image by pull-push
			m_imageReconstructedOutput = new ImageFile( W, H, PIXEL_FORMAT.BGRA8, m_imageInput.ColorProfile );
			m_imageDensity = new ImageFile( W, H, PIXEL_FORMAT.BGRA8, m_imageInput.ColorProfile );
			for ( int mipLevel=0; mipLevel < 9; mipLevel++ ) {
				m_inputPixels[mipLevel] = new float4[W >> mipLevel, H >> mipLevel];
				m_outputPixels[mipLevel] = new float4[W >> mipLevel, H >> mipLevel];
			}
			ApplyPullPush( floatTrackbarControlGamma.Value );
		}

		protected override void OnFormClosed(FormClosedEventArgs e) {
			base.OnFormClosed(e);

			m_imageInput.Dispose();
		}

		void	ApplyPullPush( float _gamma ) {
//			ApplyPullPush_Original( _gamma );
//			ApplyPullPush_Expand( _gamma );		// Good result!
			ApplyPullPush_DistanceField( _gamma );
		}

		#region Working but Ugly

		void	ApplyPullPush_Original( float _gamma ) {
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

		#region Expand Input Range with a Gaussian

		// I'm trying to expand the inut filtering range to avoid sharp patterns here
		void	ApplyPullPush_Expand( float _gamma ) {
			uint	W = m_imageInput.Width;
			uint	H = m_imageInput.Height;

			float	sigma = floatTrackbarControlSigma.Value;
			int		kernelHalfSize = (int) Math.Ceiling( 2.0f * sigma );
			int		kernelSize = 1 + 2 * kernelHalfSize;
			float[,]	kernelValues = new float[kernelSize,kernelSize];
			for ( int Y=-kernelHalfSize; Y <= kernelHalfSize; Y++ )
				for ( int X=-kernelHalfSize; X <= kernelHalfSize; X++ )
					kernelValues[kernelHalfSize+X,kernelHalfSize+Y] = (float) (Math.Exp( -(X*X + Y*Y) / (2 * sigma * sigma) ) / (2.0 * Math.PI * sigma * sigma) );

// 			int			kernelHalfSize = 2;
// 			float[,]	kernelValues = new float[5,5] {
// 				{ 1 / 273.0f, 4 / 273.0f, 7 / 273.0f, 4 / 273.0f, 1 / 273.0f },
// 				{ 4 / 273.0f, 16 / 273.0f, 26 / 273.0f, 16 / 273.0f, 4 / 273.0f },
// 				{ 7 / 273.0f, 26 / 273.0f, 41 / 273.0f, 26 / 273.0f, 7 / 273.0f },
// 				{ 4 / 273.0f, 16 / 273.0f, 26 / 273.0f, 16 / 273.0f, 4 / 273.0f },
// 				{ 1 / 273.0f, 4 / 273.0f, 7 / 273.0f, 4 / 273.0f, 1 / 273.0f },
// 			};


			// Initialize source pixels at mip 0
			for ( uint Y=0; Y < H; Y++ )
				for ( uint X=0; X < W; X++ )
					m_inputPixels[0][X,Y] = m_sparsePixels[X,Y];

			// ========= PULL ========= 
			// Build all mips up to 1x1
const int	MAX_MIP = 8;

			uint	Ws = W;
			uint	Hs = H;
			for ( int i=1; i <= MAX_MIP; i++ ) {
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

Array.Copy( m_inputPixels[MAX_MIP], m_outputPixels[MAX_MIP], W*H );

			// ========= PUSH ========= 
			// Build all mips back to finest level by re-using lower mip values
			for ( int i=MAX_MIP; i > 0; i-- ) {
				Ws <<= 1;
				Hs <<= 1;
				float4[,]	previousLevel = m_outputPixels[i];
				float4[,]	currentLevelIn = m_inputPixels[i-1];
				float4[,]	currentLevelOut = m_outputPixels[i-1];

				for ( uint Y=0; Y < Hs; Y++ ) {
					for ( uint X=0; X < Ws; X++ ) {

						// Apply filtering kernel
// 						float4	sum = float4.Zero;
// 						for ( int dY=-kernelHalfSize; dY <= kernelHalfSize; dY++ ) {
// 							int	kY = (int) (Y>>1) + dY;
// 							if ( kY < 0 || kY >= H )
// 								continue;
// 							for ( int dX=-kernelHalfSize; dX <= kernelHalfSize; dX++ ) {
// 								int	kX = (int) (X>>1) + dX;
// 								if ( kX < 0 || kX >= W )
// 									continue;
// 
// 								float4	V = previousLevel[kX,kY];
// 								float	kernel = kernelValues[kernelHalfSize+dX,kernelHalfSize+dY];
// 								sum += kernel * V;
// 							}
// 						}

						float4	sum = Bilerp( previousLevel, (X+0.5f) / Ws, (Y+0.5f) / Hs );

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

		#region Alternative Test

		// Pourri! Laissé dans un état moisi qui marche plus du tout de toute manière... :(
		void	ApplyPullPush_TEST_GAUSSIAN( float _gamma ) {
//		void	ApplyPullPush( float _gamma ) {
			uint	W = m_imageInput.Width;
			uint	H = m_imageInput.Height;

			float[,]	kernelValues = new float[5,5] {
				{ 1, 4, 7, 4, 1 },
				{ 4, 16, 26, 16, 4 },
				{ 7, 26, 41, 26, 7 },
				{ 4, 16, 26, 16, 4 },
				{ 1, 4, 7, 4, 1 },
			};

			// Initialize source pixels at mip 0
			for ( uint Y=0; Y < H; Y++ )
				for ( uint X=0; X < W; X++ )
					m_inputPixels[0][X,Y] = m_sparsePixels[X,Y];

			// ========= PULL ========= 
			// Build all mips up to 1x1
			uint	Ws = W >> 1;
			uint	Hs = H >> 1;

			// ---- Start by a unique gaussian filtering ----

			// Build gaussian kernel
			float	sigma = floatTrackbarControlSigma.Value;
			int		kernelSize = (int) Math.Ceiling( 3.0f * sigma );
			float[]	kernelValuesD = new float[1+kernelSize];
			for ( int j=0; j <= kernelSize; j++ )
				kernelValuesD[j] = (float) Math.Exp( -j*j / (2.0 * sigma * sigma) ) / (float) Math.Sqrt( 2.0 * Math.PI * sigma * sigma );

			// Apply horizontal filtering
			float4[,]	temp = new float4[Ws,H];
			for ( uint Y=0; Y < H; Y++ ) {
				for ( uint X=0; X < Ws; X++ ) {

					// Apply filtering kernel
					float4	sum = float4.Zero;
					for ( int dX=-kernelSize; dX <= kernelSize; dX++ ) {
						int	kX = (int) (2*X) + dX;
						if ( kX < 0 || kX >= W )
							continue;

						float4	V = m_inputPixels[0][kX,Y];
						sum += kernelValuesD[Math.Abs( dX )] * V;
					}

// 					float	ratio = sum.w > 0.0f ? 1.0f / sum.w : 0.0f;
// 					sum.x *= ratio;
// 					sum.y *= ratio;
// 					sum.z *= ratio;
					temp[X,Y] = sum;
				}
			}

			// Apply vertical filtering
			for ( uint Y=0; Y < Hs; Y++ ) {
				for ( uint X=0; X < Ws; X++ ) {

					// Apply filtering kernel
					float4	sum = float4.Zero;
					for ( int dY=-kernelSize; dY <= kernelSize; dY++ ) {
						int	kY = (int) (2*Y) + dY;
						if ( kY < 0 || kY >= H )
							continue;

						float4	V = temp[X,kY];
						sum += kernelValuesD[Math.Abs( dY )] * V;
					}

					float	ratio = sum.w > 0.0f ? 1.0f / sum.w : 0.0f;
					float	clampedWeight = 1.0f - (float) Math.Pow( Math.Max( 0.0f, 1.0f - sum.w ), _gamma );
					float	normalizer = sum.w > 0.0f ? clampedWeight / sum.w : 0.0f;
					sum.x *= normalizer;
					sum.y *= normalizer;
					sum.z *= normalizer;
					m_inputPixels[1][X,Y] = sum;
				}
			}

			W >>= 1;
			H >>= 1;


			// ---- Generate simple mips by averaging 4 pixels ----
			for ( int i=2; i < 9; i++ ) {
				Ws >>= 1;
				Hs >>= 1;
				float4[,]	previousLevel = m_inputPixels[i-1];
				float4[,]	currentLevel = m_inputPixels[i];
				for ( uint Y=0; Y < Hs; Y++ ) {
					uint	Yp = Y << 1;
					for ( uint X=0; X < Ws; X++ ) {
						uint	Xp = X << 1;
// 						float4	sum = 0.25f * (previousLevel[Xp,Yp] + previousLevel[Xp+1,Yp] + previousLevel[Xp,Yp+1] + previousLevel[Xp+1,Yp+1]);
						float4	V00 = Bilerp( previousLevel, (X-0.5f) / Ws, (Y-0.5f) / Hs );
						float4	V01 = Bilerp( previousLevel, (X+0.5f) / Ws, (Y-0.5f) / Hs );
						float4	V10 = Bilerp( previousLevel, (X-0.5f) / Ws, (Y+0.5f) / Hs );
						float4	V11 = Bilerp( previousLevel, (X+0.5f) / Ws, (Y+0.5f) / Hs );
						float4	sum = 0.25f * (V00 + V01 + V10 + V11);
// 						float	ratio = sum.w > 0.0f ? 1.0f / sum.w : 0.0f;
// 								sum.x *= ratio;
// 								sum.y *= ratio;
// 								sum.z *= ratio;
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
						for ( int dY=-2; dY <= 2; dY++ ) {
							int	kY = (int) (Y>>1) + dY;
							if ( kY < 0 || kY >= H )
								continue;
							for ( int dX=-2; dX <= 2; dX++ ) {
								int	kX = (int) (X>>1) + dX;
								if ( kX < 0 || kX >= W )
									continue;

								float4	V = previousLevel[kX,kY];
								float	kernel = 1.0f / 273.0f * kernelValues[2+dX,2+dY];
								sum += kernel * V;
							}
						}

						// Normalize filtered result
						float	clampedWeight = Math.Min( 1.0f, sum.w );
						float	normalizer = sum.w > 0.0f ? 1.0f / clampedWeight : 0.0f;
						sum *= normalizer;

						// Blend with existing value
						float4	currentValue = currentLevelIn[X,Y];
						float4	newValue = currentValue.w * currentValue + (1.0f - currentValue.w) * sum;
// 								newValue.x *= newValue.w;
// 								newValue.y *= newValue.w;
// 								newValue.z *= newValue.w;
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

		#region Expand Input Influence with a Distance Field + Renormalize

		// I'm trying to expand the inut filtering range to avoid sharp patterns here
		void	ApplyPullPush_DistanceField( float _gamma ) {
			uint	W = m_imageInput.Width;
			uint	H = m_imageInput.Height;

			// Initialize distance field and pixel indices
			float[][,]	distances = new float[2][,] {
				new float[W,H], new float[W,H]
			};
			uint[][,]	pixelIndex = new uint[2][,] {
				new uint[W,H], new uint[W,H]
			};
			for ( uint Y=0; Y < H; Y++ )
				for ( uint X=0; X < W; X++ ) {
					if ( m_sparsePixels[X,Y].w > 0.5f ) {
						distances[0][X,Y] = 0.0f;
						pixelIndex[0][X,Y] = W*Y+X;
					} else {
						distances[0][X,Y] = float.PositiveInfinity;
						pixelIndex[0][X,Y] = ~0U;
					}
				}

			//////////////////////////////////////////////////////////////////////////
			// Compute distance field
			const int	S = 256;

			// Horizontal spread
			for ( uint Y=0; Y < H; Y++ ) {
				for ( uint X=0; X < W; X++ ) {
					float	minDistance = distances[0][X,Y];
					uint	minPixelIndex = pixelIndex[0][X,Y];
					for ( int dX=-S; dX <= S; dX++ ) {
						int	kX = (int) X + dX;
						if ( kX < 0 || kX >= W )
							continue;

						float	currentDistance = distances[0][kX,Y] + Math.Abs( dX );
						if ( currentDistance < minDistance ) {
							minDistance = currentDistance;
							minPixelIndex = pixelIndex[0][kX,Y];
						}
					}
					distances[1][X,Y] = minDistance;
					pixelIndex[1][X,Y] = minPixelIndex;
				}
			}

			// Vertical spread
			for ( uint Y=0; Y < H; Y++ ) {
				for ( uint X=0; X < W; X++ ) {
					float	minSqDistance = distances[1][X,Y];
							minSqDistance *= minSqDistance;
					uint	minPixelIndex = pixelIndex[1][X,Y];
					for ( int dY=-S; dY <= S; dY++ ) {
						int	kY = (int) Y + dY;
						if ( kY < 0 || kY >= H )
							continue;

						float	currentSqDistance = distances[1][X,kY];
								currentSqDistance *= currentSqDistance;
								currentSqDistance += dY*dY;
						if ( currentSqDistance < minSqDistance ) {
							minSqDistance = currentSqDistance;
							minPixelIndex = pixelIndex[1][X,kY];
						}
					}
					distances[0][X,Y] = (float) Math.Sqrt( minSqDistance );
					pixelIndex[0][X,Y] = minPixelIndex;
				}
			}

			for ( uint Y=0; Y < H; Y++ ) {
				for ( uint X=0; X < W; X++ ) {
					uint	i = pixelIndex[0][X,Y];
					float4	value = i < W*H ? m_imageSparseInput[i%W,i/W] : float4.Zero;
					float	distance = 0.05f * distances[0][X,Y];
					m_outputPixels[0][X,Y].Set( value.x, value.y, value.z, 1.0f + distance );
				}
			}

			// Display
			DisplayResult();
		}

		#endregion

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

		void	DisplayResult() {
			uint	W = m_imageInput.Width;
			uint	H = m_imageInput.Height;

//			m_imageRecosntructedOutput.WritePixels( ( uint X, uint Y, ref float4 _color ) => { _color = m_inputPixels[0][X,Y]; } );
			float4[,]	source = checkBoxInput.Checked ? m_inputPixels[integerTrackbarControlMipLevel.Value] : m_outputPixels[integerTrackbarControlMipLevel.Value];
			m_imageReconstructedOutput.WritePixels( ( uint X, uint Y, ref float4 _color ) => {
				float	U = (float) X / W;
				float	V = (float) Y / H;
				_color = Bilerp( source, U, V );
//				_color = Bilerp( m_outputPixels[1], U, V );
			} );
			panelOutputReconstruction.m_bitmap = m_imageReconstructedOutput.AsBitmap;
			panelOutputReconstruction.Refresh();

			// Show weight
			m_imageDensity.WritePixels( ( uint X, uint Y, ref float4 _color ) => {
				float	U = (float) X / W;
				float	V = (float) Y / H;
				float4	temp = 1.0f *  Bilerp( source, U, V );
						temp.w -= 1.0f;
				_color.Set( temp.w, temp.w, temp.w, 1.0f );
			} );
			panelPixelDensity.m_bitmap = m_imageDensity.AsBitmap;
			panelPixelDensity.Refresh();
		}

		private void floatTrackbarControlGamma_SliderDragStop(Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fStartValue) {
			ApplyPullPush( floatTrackbarControlGamma.Value );
		}

		private void integerTrackbarControlMipLevel_ValueChanged(Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue) {
			DisplayResult();
		}

		private void checkBoxInput_CheckedChanged(object sender, EventArgs e) {
			DisplayResult();
		}
	}
}
