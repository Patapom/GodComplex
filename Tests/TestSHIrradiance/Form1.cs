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
using SphericalHarmonics;
using Renderer;
using Nuaj.Cirrus.Utility;

namespace TestSHIrradiance
{
	public partial class Form1 : Form {

		ImageUtility.ImageFile	m_image = new ImageUtility.ImageFile( 800, 550, ImageUtility.PIXEL_FORMAT.RGBA8, new ImageUtility.ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB ) );
		ImageUtility.ImageFile	m_HDRImage = new ImageUtility.ImageFile();

		float4					m_black = new float4( 0, 0, 0, 1 );
		float4					m_white = new float4( 1, 1, 1, 1 );
		float4					m_red = new float4( 1, 0, 0, 1 );
		float4					m_green = new float4( 0, 1, 0, 1 );
		float4					m_blue = new float4( 0, 0, 1, 1 );

		// D3D Stuff
		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		struct CB_Main {
			public uint		_sizeX;
			public uint		_sizeY;
			public float	_time;
			public uint		_flags;
			public float4	_mouse;
			public float4x4	_world2Proj;
//			public float4x4	_proj2World;
			public float4x4	_camera2World;
			public float	_cosAO;
			public float	_luminanceFactor;
			public float	_filteringWindowSize;
			public float	_influenceAO;
			public float	_influenceBentNormal;
			public uint		_SHOrdersCount;
			public int		_customM;
		}

		Device						m_device = null;
		Shader						m_shader_RenderSphere = null;
		Shader						m_shader_RenderScene = null;
		Shader						m_shader_RenderLDR = null;
		ConstantBuffer< CB_Main >	m_CB_Render;
		Texture2D					m_Tex_HDREnvironment;
		Texture2D					m_Tex_HDRBuffer;
		Texture2D					m_Tex_Noise;

		// Advanced SH to compare with ground truth
		Texture2D					m_Tex_ACoeffs;
		RawConstantBuffer			m_CB_Coeffs;

		Camera						m_camera = new Camera();
		CameraManipulator			m_cameraManipulator = new CameraManipulator();


		public Form1() {
			InitializeComponent();
			UpdateGraph();

//TestSolidAngle();

radioButtonSingleSphere.Checked = true;

		}

void	TestSolidAngle() {
	double	dPhi = 2.0 * Math.PI / 200;
	double	dTheta = Math.PI / 100;
	double	solidAngle;

	double	R = 0.7;
	for ( double D=10.0; D > 0.0; D-=1.0 ) {
		double	Px = -D;
		double	Py = 0.0;
		double	Pz = 0.0;

		double	sum = 0.0;
		double	test = 0.0;
		for ( int X=0; X < 200; X++ ) {
			double	phi = X * dPhi;
			for ( int Y=0; Y < 100; Y++ ) {
				double	theta = (0.5+Y) * dTheta;

				double	dS = Math.Sin( theta ) * dPhi * dTheta;

				double	Nx = Math.Sin( theta ) * Math.Cos( phi );
				double	Ny = Math.Sin( theta ) * Math.Sin( phi );
				double	Nz = Math.Cos( theta );

				double	Rx = R * Nx - Px;
				double	Ry = R * Ny - Py;
				double	Rz = R * Nz - Pz;
// 				double	Rx = Px - R * Nx;
// 				double	Ry = Py - R * Ny;
// 				double	Rz = Pz - R * Nz;
				double	r2 = Rx*Rx + Ry*Ry + Rz*Rz;
				double	r = Math.Sqrt( r2 );
				double	r3 = r * r2;
						Rx /= r3;
						Ry /= r3;
						Rz /= r3;

				sum += (Nx * Rx + Ny * Ry + Nz * Rz) * dS;

				test += Math.Sqrt( 1.0 - Rz * Rz );	// sin( theta du point sur la sphère )
			}
		}
		test /= 200 * 100;

		double	sinConeAngle = R / D;
		solidAngle = 2.0 * Math.PI * (1.0 - Math.Sqrt( 1.0 - sinConeAngle*sinConeAngle ) );
	}
}


		#region HDR Image Encoding

		void	LoadHDRImage() {
//			m_HDRImage.Load( new System.IO.FileInfo( @".\Images\grace-new.hdr" ) );
			m_HDRImage.Load( new System.IO.FileInfo( @".\Images\ennis_1024x512.hdr" ) );
			ImageUtility.ImagesMatrix	images = new ImageUtility.ImagesMatrix();
			images.InitTexture2DArray( m_HDRImage.Width, m_HDRImage.Height, 1, 1 );
			images[0][0][0] = m_HDRImage;
			m_Tex_HDREnvironment = new Texture2D( m_device, images, ImageUtility.COMPONENT_FORMAT.AUTO );
			
// 			ImageUtility.ImageFile	tempLDRImage = new ImageUtility.ImageFile();
// 			tempLDRImage.ToneMapFrom( m_HDRImage, ( float3 _HDR, ref float3 _LDR ) => {
// 				_LDR = _HDR;
// 			} );
// 			graphPanel.Bitmap = tempLDRImage.AsBitmap;

			// Integrate SH
//			EncodeSH();
//			EncodeSH_20Orders();

			// Test numerical integration
//			NumericalIntegration();
//			NumericalIntegration_20Orders();
			TestIntegral();
		}

// Estimates A0, A1 and A2 integrals based on the angle of the AO cone from the normal and cos(PI/2 * AO) defining the AO cone's half-angle aperture
// Fitting was done with mathematica:
//
//	A0 = a + b*x + d*x*y + e*x^2 + f*y^2 + g*x^2*y + i*x^2*y^2;
//	With {a -> 0.86342, b -> 0.127258, c -> 4.9738*10^-14, d -> -0.903477, e -> -0.967484, f -> -0.411706, g -> 0.885699, h -> 0., i -> 0.407098}
//
//	A1 = a + b*x + c*y + d*x*y + e*x^2 + f*y^2 + g*x^2*y + h*x*y^2;
//	With {a -> 0.95672, b -> 0.790566, c -> 0.298642, d -> -2.63968, e -> -1.65043, f -> -0.720222, g -> 2.14987, h -> 0.788641 }
//
//	A2 = a + b*x + c*y + d*x*y + e*x^2 + f*y^2 + g*x^2*y + h*x*y^2 + i*x^2*y^2 + j*x^3 + k*y^3 + l*x^3*y + m*x*y^3 + p*x^3*y^3;
//	With {a -> 0.523407, b -> -0.6694, c -> -0.128209, d -> 5.26746, e -> 3.40837, f -> 0.905606, g -> -12.8261, h -> -10.5428, i -> 9.40113, j -> -3.18758, k -> -1.08565, l -> 7.57317, m -> 5.45239, p -> -4.06299}
//
static readonly float3	a = new float3( 0.86342f, 0.95672f, 0.523407f );
static readonly float3	b = new float3( 0.127258f, 0.790566f, -0.6694f );
static readonly float3	c = new float3( 0.0f, 0.298642f, -0.128209f );
static readonly float3	d = new float3( -0.903477f, -2.63968f, 5.26746f );
static readonly float3	e = new float3( -0.967484f, -1.65043f, 3.40837f );
static readonly float3	f = new float3( -0.411706f, -0.720222f, 0.905606f );
static readonly float3	g = new float3( 0.885699f, 2.14987f, -12.8261f );
static readonly float3	h = new float3( 0.0f, 0.788641f, -10.5428f );
static readonly float3	i = new float3( 0.407098f, 0.0f, 9.40113f );
const float		j = -3.18758f, k = -1.08565f, l = 7.57317f, m = 5.45239f, p = -4.06299f;

float3	EstimateLambertReflectanceFactors( float _cosThetaAO, float _coneBendAngle ) {
	float	x = _cosThetaAO;
	float	y = _coneBendAngle * 2.0f / (float) Math.PI;

	float	x2 = x*x;
	float	x3 = x*x2;
	float	y2 = y*y;
	float	y3 = y*y2;

	float	A0 = a.x + x * (b.x + y * d.x + x * (e.x + y * (g.x + y * i.x))) + f.x * y2;
	float	A1 = a.y + x * (b.y + y * (d.y + h.y * y) + x * (e.y + y * g.y)) + y * (c.y + y * f.y);
	float	A2 = a.z + x * (b.z + y * d.z + x * (e.z + y * (g.z + y * i.z) + x * (j + y * (l + y2 * p))))
					 + y * (c.z + y * (f.z + x * h.z + (y * (k + x * m))));

	return new float3( A0, A1, A2 );
}

		void	TestIntegral() {
/*
			// Test double integral
			const double	dTheta = 0.5 * Math.PI / 100;
			const double	dPhi = 2.0 * Math.PI / 400;
			double	sum = 0.0;
			double	sum2 = 0.0;
			double	sum3 = 0.0;
			for ( int Y=0; Y < 100; Y++ ) {
				double	theta = (Y+0.5) * dTheta;
				double	cosTheta = Math.Cos( theta );
				double	sinTheta = Math.Sin( theta );
				for ( int X=0; X < 400; X++ ) {
					double	phi = X * dPhi;

					sum += sinTheta * dTheta * dPhi;
					sum2 += cosTheta * sinTheta * dTheta * dPhi;
					sum3 += cosTheta * cosTheta * sinTheta * dTheta * dPhi;
				}
			}

			// Test random integration with uniform angular distribution
			SimpleRNG.SetSeed( 1 );
			sum = 0.0;
			sum2 = 0.0;
			sum3 = 0.0;
			for ( int Y=0; Y < 100*400; Y++ ) {
				double	theta = SimpleRNG.GetUniform() * 0.5 * Math.PI;
				double	phi = SimpleRNG.GetUniform() * 2.0 * Math.PI;
				double	cosTheta = Math.Cos( theta );
				double	sinTheta = Math.Sin( theta );
				sum += sinTheta * dTheta * dPhi;
				sum2 += cosTheta * sinTheta * dTheta * dPhi;
				sum3 += cosTheta * cosTheta * sinTheta * dTheta * dPhi;
			}

			// Test random integration with uniform vertical distribution
			SimpleRNG.SetSeed( 1 );
			sum = 0.0;
			sum2 = 0.0;
			sum3 = 0.0;
			for ( int Y=0; Y < 100*400; Y++ ) {
				double	phi = SimpleRNG.GetUniform() * 2.0 * Math.PI;
				double	cosTheta = SimpleRNG.GetUniform();
				double	sinTheta = Math.Sqrt( 1.0 - cosTheta*cosTheta );
				sum += dTheta * dPhi;
				sum2 += cosTheta * dTheta * dPhi;
				sum3 += cosTheta * cosTheta * dTheta * dPhi;
			}
			sum *= 2.0 / Math.PI;
			sum2 *= 2.0 / Math.PI;
			sum3 *= 2.0 / Math.PI;

			// Test random integration with uniform vertical cosine-distribution
			SimpleRNG.SetSeed( 1 );
			sum = 0.0;
			sum2 = 0.0;
			sum3 = 0.0;
			for ( int Y=0; Y < 100*400; Y++ ) {
				double	phi = SimpleRNG.GetUniform() * 2.0 * Math.PI;
				double	cosTheta = Math.Sqrt( SimpleRNG.GetUniform() );
				double	sinTheta = Math.Sqrt( 1.0 - cosTheta*cosTheta );
				sum += dTheta * dPhi;
				sum2 += cosTheta * dTheta * dPhi;
				sum3 += cosTheta * cosTheta * dTheta * dPhi;
			}
			sum /= Math.PI;
			sum2 /= Math.PI;
			sum3 /= Math.PI;
*/

			// Test analytical and tabulated A terms
			double		sumDiffA0 = 0.0;
			double		sumDiffA1 = 0.0;
			double		sumDiffA2 = 0.0;
// 			double[]	approxSH = new double[9];
// 			double[]	exactSH = new double[9];
// 			double[]	sumDiffSH = new double[9];
			for ( int i=0; i < 100; i++ ) {
				float	AO = 1.0f - (float) i / 100;
				float	t = (float) Math.Cos( 0.5 * Math.PI * AO );
				float3	tableA = EstimateLambertReflectanceFactors( t, 0.0f );
				double	exactA0 = Math.Sqrt( Math.PI / 4.0 ) * (1 - t*t);
				double	exactA1 = Math.Sqrt( Math.PI / 3.0 ) * (1 - t*t*t);
				double	exactA2 = Math.Sqrt( 5.0 * Math.PI / 4.0 ) * (3.0 / 4.0 * (1 - t*t*t*t) - 1.0 / 2.0 * (1 - t*t));

// 				exactSH[0] = 0.5 * Math.PI * (1 - t*t);
// 				approxSH[0] = Math.PI * tableA.x;

				double	exactC1 = Math.Sqrt( Math.PI / 3 ) * (1 - t*t*t);
				double	approxC1 = tableA.y;

				double	exactc0 = Math.Sqrt( Math.PI / 4.0 ) * (1 - t*t);
				double	exactc1 = Math.Sqrt( Math.PI / 3.0 ) * (1 - t*t*t);
				double	exactc2 = Math.Sqrt( 5.0 * Math.PI / 4.0 ) * (3.0 / 4.0 * (1 - t*t*t*t) - 1.0 / 2.0 * (1 - t*t));

				sumDiffA0 += Math.Abs( exactA0 - tableA.x );
				sumDiffA1 += Math.Abs( exactA1 - tableA.y );
				sumDiffA2 += Math.Abs( exactA2 - tableA.z );
			}
			sumDiffA0 /= 100;
			sumDiffA1 /= 100;
			sumDiffA2 /= 100;
		}

		void	NumericalIntegration() {
			// Generate a bunch of rays with equal probability on the hemisphere
			const int		THETA_SAMPLES = 100;
			const int		SAMPLES_COUNT = 4*THETA_SAMPLES*THETA_SAMPLES;
			const double	dPhi = 2.0 * Math.PI / (4 * THETA_SAMPLES);
			float3[]		directions = new float3[SAMPLES_COUNT];
			for ( int Y=0; Y < THETA_SAMPLES; Y++ ) {
				for ( int X=0; X < 4*THETA_SAMPLES; X++ ) {
					double	phi = dPhi * (X+SimpleRNG.GetUniform());
					double	theta = 2.0 * Math.Acos( Math.Sqrt( 1.0 - 0.5 * (Y+SimpleRNG.GetUniform()) / THETA_SAMPLES ) );		// Uniform sampling on theta
					directions[4*THETA_SAMPLES*Y+X].Set( (float) (Math.Sin( theta ) * Math.Cos( phi )), (float) (Math.Sin( theta ) * Math.Sin( phi )), (float) Math.Cos( theta ) );
				}
			}

			// Compute numerical integration for various sets of angles
			const int	TABLE_SIZE = 100;

			float3		coneDirection = float3.Zero;
			float3[,]	integratedSHCoeffs = new float3[TABLE_SIZE,TABLE_SIZE];

double	avgDiffA0 = 0.0;
double	avgDiffA1 = 0.0;
double	avgDiffA2 = 0.0;

			for ( int thetaIndex=0; thetaIndex < TABLE_SIZE; thetaIndex++ ) {
//				float	cosTheta = 1.0f - (float) thetaIndex / TABLE_SIZE;
float	cosTheta = (float) Math.Cos( 0.5 * Math.PI * thetaIndex / TABLE_SIZE );
				coneDirection.x = (float) Math.Sqrt( 1.0f - cosTheta*cosTheta );
				coneDirection.z = cosTheta;

				for ( int AOIndex=0; AOIndex < TABLE_SIZE; AOIndex++ ) {
//					float	AO = 1.0f - (float) AOIndex / TABLE_SIZE;
//					float	coneHalfAngle = 0.5f * (float) Math.PI * AO;			// Cone half angle varies in [0,PI/2]
//					float	cosConeHalfAngle = (float) Math.Cos( coneHalfAngle );
float	cosConeHalfAngle = (float) AOIndex / TABLE_SIZE;

					double	A0 = 0.0;
					double	A1 = 0.0;
					double	A2 = 0.0;
					for ( int sampleIndex=0; sampleIndex < SAMPLES_COUNT; sampleIndex++ ) {
						float3	direction = directions[sampleIndex];
						if ( direction.Dot( coneDirection ) < cosConeHalfAngle )
							continue;	// Sample is outside cone

						float	u = direction.z;	// cos(theta_sample)
						float	u2 = u * u;
						float	u3 = u * u2;
						A0 += u;
//A0 += 1.0;
						A1 += u2;
						A2 += 0.5 * (3 * u3 - u);
					}

					A0 *= 2.0 * Math.PI / SAMPLES_COUNT;
					A1 *= 2.0 * Math.PI / SAMPLES_COUNT;
					A2 *= 2.0 * Math.PI / SAMPLES_COUNT;
					A0 *= Math.Sqrt( 1.0 / (4.0 * Math.PI) );
					A1 *= Math.Sqrt( 3.0 / (4.0 * Math.PI) );
					A2 *= Math.Sqrt( 5.0 / (4.0 * Math.PI) );

// float3	verify = EstimateLambertReflectanceFactors( cosConeHalfAngle, 0.5f * (float) Math.PI * thetaIndex / TABLE_SIZE );
// avgDiffA0 += Math.Abs( A0 - verify.x );
// avgDiffA1 += Math.Abs( A1 - verify.y );
// avgDiffA2 += Math.Abs( A2 - verify.z );

					integratedSHCoeffs[thetaIndex,AOIndex].Set( (float) A0, (float) A1, (float) A2 );
				}
			}

avgDiffA0 /= TABLE_SIZE*TABLE_SIZE;
avgDiffA1 /= TABLE_SIZE*TABLE_SIZE;
avgDiffA2 /= TABLE_SIZE*TABLE_SIZE;

			using ( System.IO.FileStream S = new System.IO.FileInfo( @"ConeTable_cosAO.float3" ).Create() )
				using ( System.IO.BinaryWriter W = new System.IO.BinaryWriter( S ) ) {
				for ( int thetaIndex=0; thetaIndex < TABLE_SIZE; thetaIndex++ )
						for ( int AOIndex=0; AOIndex < TABLE_SIZE; AOIndex++ ) {
							W.Write( integratedSHCoeffs[thetaIndex,AOIndex].x );
							W.Write( integratedSHCoeffs[thetaIndex,AOIndex].y );
							W.Write( integratedSHCoeffs[thetaIndex,AOIndex].z );
						}
				}
		}

		/// <summary>
		/// Computes up to 20 orders of A coefficients for various AO and angle values
		/// </summary>
		void	NumericalIntegration_20Orders() {
			// Generate a bunch of rays with equal probability on the hemisphere
			const int		THETA_SAMPLES = 100;
			const int		SAMPLES_COUNT = 4*THETA_SAMPLES*THETA_SAMPLES;
			const double	dPhi = 2.0 * Math.PI / (4 * THETA_SAMPLES);
			float3[]		directions = new float3[SAMPLES_COUNT];
			for ( int Y=0; Y < THETA_SAMPLES; Y++ ) {
				for ( int X=0; X < 4*THETA_SAMPLES; X++ ) {
					double	phi = dPhi * (X+SimpleRNG.GetUniform());
					double	theta = 2.0 * Math.Acos( Math.Sqrt( 1.0 - 0.5 * (Y+SimpleRNG.GetUniform()) / THETA_SAMPLES ) );		// Uniform sampling on theta
					directions[4*THETA_SAMPLES*Y+X].Set( (float) (Math.Sin( theta ) * Math.Cos( phi )), (float) (Math.Sin( theta ) * Math.Sin( phi )), (float) Math.Cos( theta ) );
				}
			}

			// Compute numerical integration for various sets of angles
			const int	TABLE_SIZE = 64;
			const int	ORDERS = 20;

			float3		coneDirection = float3.Zero;
			float[,,]	integratedSHCoeffs = new float[TABLE_SIZE,TABLE_SIZE,ORDERS];

			double[]	A = new double[ORDERS];
			for ( int thetaIndex=0; thetaIndex < TABLE_SIZE; thetaIndex++ ) {
				float	V = (float) thetaIndex / TABLE_SIZE;
//				float	cosTheta = (float) Math.Cos( 0.5 * Math.PI * V );
				float	cosTheta = V;
				coneDirection.x = (float) Math.Sqrt( 1.0f - cosTheta*cosTheta );
				coneDirection.z = cosTheta;

				for ( int AOIndex=0; AOIndex < TABLE_SIZE; AOIndex++ ) {
					float	U = (float) AOIndex / TABLE_SIZE;
//					float	cosConeHalfAngle = U;
					float	cosConeHalfAngle = (float) Math.Cos( 0.5 * Math.PI * U );

					Array.Clear( A, 0, ORDERS );
					for ( int sampleIndex=0; sampleIndex < SAMPLES_COUNT; sampleIndex++ ) {
						float3	direction = directions[sampleIndex];
						if ( direction.Dot( coneDirection ) < cosConeHalfAngle )
							continue;	// Sample is outside cone

						float	u = direction.z;	// cos(theta_sample)
						for ( int order=0; order < ORDERS; order++ ) {
							A[order] += u * SHFunctions.P0( order, u );
						}
					}

					// Finalize integration
					for ( int order=0; order < ORDERS; order++ )
						A[order] *= 2.0 * Math.PI / SAMPLES_COUNT;
					for ( int order=0; order < ORDERS; order++ ) {
						integratedSHCoeffs[thetaIndex,AOIndex,order] = (float) A[order];
					}
				}
			}

			// Save table
			using ( System.IO.FileStream S = new System.IO.FileInfo( @"ConeTable_cosTheta_order20.float" ).Create() )
				using ( System.IO.BinaryWriter W = new System.IO.BinaryWriter( S ) ) {
				for ( int thetaIndex=0; thetaIndex < TABLE_SIZE; thetaIndex++ )
						for ( int AOIndex=0; AOIndex < TABLE_SIZE; AOIndex++ ) {
							for ( int order=0; order < ORDERS; order++ )
								W.Write( integratedSHCoeffs[thetaIndex,AOIndex,order] );
						}
				}
		}

		/// <summary>
		/// Encodes HDR radiance image into SH
		/// We're assuming the input image is encoded as panoramic format described here: http://gl.ict.usc.edu/Data/HighResProbes/
		/// </summary>
		/// <returns></returns>
		string	EncodeSH() {
			// Thus, if we consider the images to have a rectangular image domain of u=[0,2], v=[0,1], we have theta= pi*(u-1), phi=pi*v.
			// The unit vector pointing in the corresponding direction is obtained by (Dx,Dy,Dz) = (sin(phi)*sin(theta), cos(phi), -sin(phi)*cos(theta)).
			// For the reverse mapping from the direction vector in the world (Dx, Dy, Dz), the corresponding (u,v) coordinate in the light probe image is ( 1 + atan2(Dx,-Dz) / pi, arccos(Dy) / pi).
			//
			// This mapping is convenient but does not have equal area.
			// Thus to find the average pixel value one must first multiply by the vertical cosine falloff function cos(phi).
			//
			// NOTE: Apparently, they consider Y-up axis and we're using Z-up
			//
			uint	W = m_HDRImage.Width;
			uint	H = m_HDRImage.Height;
			double	dPhi = 2.0 * Math.PI / W;
			double	dTheta = Math.PI / H;

			double[]	SH0 = new double[9];
			double[]	SH1 = new double[9];

			double[,]	coeffs = new double[9,3];
			for ( uint Y=0; Y < H; Y++ ) {
				double	theta = (0.5+Y) * dTheta;
//				double	cosTheta = Math.Cos( theta );
				double	sinTheta = Math.Sin( theta );

				for ( uint X=0; X < W; X++ ) {
					double	phi = X * dPhi - Math.PI;
// 					double	cosPhi = Math.Cos( phi );
// 					double	sinPhi = Math.Sin( phi );

					float4	HDRColor = m_HDRImage[X,Y];
//HDRColor.Set( 1, 1, 1, 1 );

					// Accumulate weighted SH
					double	omega = sinTheta * dPhi * dTheta;


// Compare our 2 ways of generating Ylm
// SHFunctions.Ylm( new float3( (float) (sinTheta * cosPhi), (float) (sinTheta * sinPhi), (float) cosTheta ), SH0 );
// for ( int l=0; l < 3; l++ ) {
// 	for ( int m=-l; m <= l; m++ ) {
// 		int		i = l*(l+1)+m;
// 		SH1[i] = SHFunctions.Ylm( l, m, theta, phi );
// 	}
// }
// for ( int i=0; i < 9; i++ )
// 	if ( Math.Abs( SH0[i] - SH1[i] ) > 1e-4 )
// 		throw new Exception( "ARGH!" );
			


					for ( int l=0; l < 3; l++ ) {
						for ( int m=-l; m <= l; m++ ) {
							double	Ylm = SHFunctions.Ylm( l, m, theta, phi );
							int		i = l*(l+1)+m;
							coeffs[i,0] += HDRColor.x * omega * Ylm;
							coeffs[i,1] += HDRColor.y * omega * Ylm;
							coeffs[i,2] += HDRColor.z * omega * Ylm;
						}
					}
				}
			}

			string	SHText = "float3	SH[9] = {\r\n";
			for ( int l=0; l < 3; l++ ) {
				for ( int m=-l; m <= l; m++ ) {
					int		i = l*(l+1)+m;
					SHText += "					float3( " + coeffs[i,0] + ", " + coeffs[i,1] + ", " + coeffs[i,2] + " ), \r\n";
				}
			}
			SHText += "};\r\n";
			return SHText;
		}

		void	EncodeSH_20Orders() {
			const int	ORDERS = 20;

			uint	W = m_HDRImage.Width;
			uint	H = m_HDRImage.Height;
			double	dPhi = 2.0 * Math.PI / W;
			double	dTheta = Math.PI / H;

			double[,]	coeffs = new double[ORDERS*ORDERS,3];
			for ( uint Y=0; Y < H; Y++ ) {
				double	theta = (0.5+Y) * dTheta;
				double	sinTheta = Math.Sin( theta );
				double	omega = sinTheta * dPhi * dTheta;

				for ( uint X=0; X < W; X++ ) {
					double	phi = X * dPhi - Math.PI;

					float4	HDRColor = m_HDRImage[X,Y];

					// Accumulate weighted SH
					for ( int l=0; l < ORDERS; l++ ) {
						for ( int m=-l; m <= l; m++ ) {
							double	Ylm = SHFunctions.Ylm( l, m, theta, phi );
							int		i = l*(l+1)+m;
							coeffs[i,0] += HDRColor.x * omega * Ylm;
							coeffs[i,1] += HDRColor.y * omega * Ylm;
							coeffs[i,2] += HDRColor.z * omega * Ylm;
						}
					}
				}
			}

			// Save table
			using ( System.IO.FileStream S = new System.IO.FileInfo( @"Ennis_order20.float3" ).Create() )
				using ( System.IO.BinaryWriter Wr = new System.IO.BinaryWriter( S ) ) {
					for ( int i=0; i < ORDERS*ORDERS; i++ ) {
						Wr.Write( (float) coeffs[i,0] );
						Wr.Write( (float) coeffs[i,1] );
						Wr.Write( (float) coeffs[i,2] );
					}
				}
		}

		#endregion

		#region Graph Plotting

		double	EstimateSHCoeff( int l, double _thetaMax ) {
			const int		STEPS_COUNT = 100;

			double	normalizationFactor = SphericalHarmonics.SHFunctions.K( l, 0 );

			double	dTheta = _thetaMax / STEPS_COUNT;
			double	sum = 0.0;
			for ( int i=0; i < STEPS_COUNT; i++ ) {
				double	theta = (0.5+i) * dTheta;
				double	cosTheta = Math.Cos( theta );
				double	sinTheta = Math.Sin( theta );
				double	Pl0 = SphericalHarmonics.SHFunctions.P( l, 0, cosTheta );
				sum += Pl0 * cosTheta * sinTheta * dTheta;
			}
			sum *= 2.0 * Math.PI * normalizationFactor;

			return sum;
		}

		const int	MAX_ORDER = 20;
		float2	rangeX = new float2( 0, MAX_ORDER );
		float2	rangeY = new float2( -0.2f, 1.1f );
		float[]	coeffs = new float[1+MAX_ORDER];

		void	PlotSquare( float4 _color, float2 _rangeX, float2 _rangeY, float2 _rangedPosition ) {
			float2	imagePosition = m_image.RangedCoordinates2ImageCoordinates( _rangeX, _rangeY, _rangedPosition );
			int		size = 4;
			int		X0 = Math.Max( 0, (int) Math.Floor( imagePosition.x - size ) );
			int		Y0 = Math.Max( 0, (int) Math.Floor( imagePosition.y - size ) );
			int		X1 = Math.Min( (int) m_image.Width-1, (int) Math.Floor( imagePosition.x + size ) );
			int		Y1 = Math.Min( (int) m_image.Height-1, (int) Math.Floor( imagePosition.y + size ) );
			for ( int Y=Y0; Y <= Y1; Y++ )
				for ( int X=X0; X <= X1; X++ )
					m_image[(uint)X,(uint)Y] = _color;
		}

		void	UpdateGraph() {
			string	text = "";

			// Estimate coeffs
			double	thetaMax = floatTrackbarControlThetaMax.Value * 0.5 * Math.PI / 90.0;
			for ( int l=0; l <= MAX_ORDER; l++ ) {
				coeffs[l] = (float) EstimateSHCoeff( l, thetaMax );
				text += "SH #" + l + " = " + coeffs[l] + "\r\n";
			}

			m_image.Clear( m_white );
//			m_image.PlotGraphAutoRangeY( m_black, rangeX, ref rangeY, ( float x ) => {
			m_image.PlotGraph( m_black, rangeX, rangeY, ( float x ) => {
				int		l0 = Math.Min( MAX_ORDER, (int) Math.Floor( x ) );
				float	A0 = coeffs[l0];
				int		l1 = Math.Min( MAX_ORDER, l0+1 );
				float	A1 = coeffs[l1];
				x = x - l0;
				return A0 + (A1 - A0) * x;
			} );

			// Plot A0, A1 and A2 terms
			double	C = Math.Cos( thetaMax );
// 			m_image.PlotGraph( m_red, rangeX, rangeY, ( float x ) => { return (float) (Math.Sqrt( Math.PI ) * (1.0 - C*C) / 2.0); } );
// 			m_image.PlotGraph( m_green, rangeX, rangeY, ( float x ) => { return (float) (Math.Sqrt( 3.0 * Math.PI ) * (1.0 - C*C*C) / 3.0); } );
// 			m_image.PlotGraph( m_blue, rangeX, rangeY, ( float x ) => { return (float) (Math.Sqrt( 5.0 * Math.PI / 4.0 ) * (3.0/4.0 * (1.0 - C*C*C*C) - 1.0 / 2.0 * (1 - C*C))); } );
			PlotSquare( m_red, rangeX, rangeY, new float2( 0, (float) (Math.Sqrt( Math.PI ) * (1.0 - C*C) / 2.0) ) );
			PlotSquare( m_green, rangeX, rangeY, new float2( 1, (float) (Math.Sqrt( Math.PI / 3.0 ) * (1.0 - C*C*C)) ) );
			PlotSquare( m_blue, rangeX, rangeY, new float2( 2, (float) (Math.Sqrt( 5.0 * Math.PI / 4.0 ) * (3.0/4.0 * (1.0 - C*C*C*C) - 1.0 / 2.0 * (1 - C*C))) ) );

			m_image.PlotAxes( m_black, rangeX, rangeY, 1, 0.1f );

			text += "\r\nRange Y = [" + rangeY.x + ", " + rangeY.y + "]\r\n";
			textBoxResults.Text = text;

			graphPanel.Bitmap = m_image.AsBitmap;
			graphPanel.EnablePaint = true;
			graphPanel.Refresh();
			floatTrackbarControlThetaMax.Refresh();
			textBoxResults.Refresh();
		}

		#endregion

		#region 3D Rendering

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );

			try {
				// Initialize the device
				m_device = new Device();
				m_device.Init( graphPanel.Handle, false, true );

				// Create the render shaders
				try {
					Shader.WarningAsError = false;

					m_shader_RenderSphere = new Shader( m_device, new System.IO.FileInfo( @"./Shaders/RenderSphere.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
					m_shader_RenderScene = new Shader( m_device, new System.IO.FileInfo( @"./Shaders/RenderScene.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
					m_shader_RenderLDR = new Shader( m_device, new System.IO.FileInfo( @"./Shaders/RenderLDR.hlsl" ), VERTEX_FORMAT.Pt4, "VS", null, "PS", null );
				} catch ( Exception _e ) {
					throw new Exception( "Failed to compile shader! " + _e.Message );
				}

				// Create CB
				m_CB_Render = new ConstantBuffer< CB_Main >( m_device, 0 );

				// Create textures
				LoadHDRImage();

				m_Tex_HDRBuffer = new Texture2D( m_device, (uint) graphPanel.Width, (uint) graphPanel.Height, 2, 1, ImageUtility.PIXEL_FORMAT.RGBA32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, null );

				{	// Build noise texture
					SimpleRNG.SetSeed( 1U );
					PixelsBuffer	content = new PixelsBuffer( 256*256*16 );
					using ( System.IO.BinaryWriter W = content.OpenStreamWrite() )
						for ( int i=0; i < 256*256; i++ ) {
							W.Write( (float) SimpleRNG.GetUniform() );
							W.Write( (float) SimpleRNG.GetUniform() );
							W.Write( (float) SimpleRNG.GetUniform() );
							W.Write( (float) SimpleRNG.GetUniform() );
						}
					m_Tex_Noise = new Texture2D( m_device, 256, 256, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, new PixelsBuffer[] { content } );
				}

				// Build SH coeffs
				const int	ORDERS = 20;
				{
					const int	TABLE_SIZE = 64;

					// Load A coeffs into a texture array
					float[,,]	A = new float[TABLE_SIZE,TABLE_SIZE,ORDERS];
//					using ( System.IO.FileStream S = new System.IO.FileInfo( @"ConeTable_cosAO_order20.float" ).OpenRead() )
					using ( System.IO.FileStream S = new System.IO.FileInfo( @"ConeTable_cosTheta_order20.float" ).OpenRead() )
						using ( System.IO.BinaryReader R = new System.IO.BinaryReader( S ) ) {
							for ( int thetaIndex=0; thetaIndex < TABLE_SIZE; thetaIndex++ )
								for ( int AOIndex=0; AOIndex < TABLE_SIZE; AOIndex++ ) {
									for ( int order=0; order < ORDERS; order++ )
										A[thetaIndex,AOIndex,order] = R.ReadSingle();
								}
						}

					PixelsBuffer[]	coeffSlices = new PixelsBuffer[5];	// 5 slices of 4 coeffs each to get our 20 orders
					for ( int sliceIndex=0; sliceIndex < coeffSlices.Length; sliceIndex++ ) {
						PixelsBuffer	coeffSlice = new PixelsBuffer( TABLE_SIZE*TABLE_SIZE*16 );
						coeffSlices[sliceIndex] = coeffSlice;

						using ( System.IO.BinaryWriter W = coeffSlice.OpenStreamWrite() ) {
							for ( int thetaIndex=0; thetaIndex < TABLE_SIZE; thetaIndex++ )
								for ( int AOIndex=0; AOIndex < TABLE_SIZE; AOIndex++ ) {
									W.Write( A[thetaIndex,AOIndex,4*sliceIndex+0] );
									W.Write( A[thetaIndex,AOIndex,4*sliceIndex+1] );
									W.Write( A[thetaIndex,AOIndex,4*sliceIndex+2] );
									W.Write( A[thetaIndex,AOIndex,4*sliceIndex+3] );
								}
						}
					}

					m_Tex_ACoeffs = new Texture2D( m_device, 64, 64, coeffSlices.Length, 1, ImageUtility.PIXEL_FORMAT.RGBA32F, ImageUtility.COMPONENT_FORMAT.AUTO, false, false, coeffSlices );
				}

				{
					// Load environment coeffs into a constant buffer
					float3[]	coeffs = new float3[ORDERS*ORDERS];
					using ( System.IO.FileStream S = new System.IO.FileInfo( @"Ennis_order20.float3" ).OpenRead() )
						using ( System.IO.BinaryReader R = new System.IO.BinaryReader( S ) )
							for ( int coeffIndex=0; coeffIndex < ORDERS*ORDERS; coeffIndex++ )
								coeffs[coeffIndex].Set( R.ReadSingle(), R.ReadSingle(), R.ReadSingle() );

					// Write into a raw byte[]
					byte[]	rawContent = new byte[400 * 4 * 4];
					using ( System.IO.MemoryStream	MS = new System.IO.MemoryStream( rawContent ) )
						using ( System.IO.BinaryWriter W = new System.IO.BinaryWriter( MS ) ) {
							for ( int coeffIndex=0; coeffIndex < ORDERS*ORDERS; coeffIndex++ ) {
								W.Write( coeffs[coeffIndex].x );
								W.Write( coeffs[coeffIndex].y );
								W.Write( coeffs[coeffIndex].z );
								W.Write( 0.0f );
							}
						}
 					m_CB_Coeffs = new RawConstantBuffer( m_device, 1, rawContent.Length );
					m_CB_Coeffs.UpdateData( rawContent );
				}

				// Create camera + manipulator
				m_camera.CreatePerspectiveCamera( 0.5f * (float) Math.PI, (float) graphPanel.Width / graphPanel.Height, 0.01f, 100.0f );
				m_camera.CameraTransformChanged += m_camera_CameraTransformChanged;
				m_cameraManipulator.Attach( graphPanel, m_camera );
				m_cameraManipulator.InitializeCamera( -2.0f * float3.UnitZ, float3.Zero, float3.UnitY );
				m_camera_CameraTransformChanged( null, EventArgs.Empty );

				// Start rendering
				Application.Idle += Application_Idle;

			} catch ( Exception _e ) {
				MessageBox.Show( "Failed to initialize D3D renderer!\r\nReason: " + _e.Message );
			}
		}

		protected override void OnFormClosing( FormClosingEventArgs e ) {
			base.OnFormClosing( e );
			if ( e.Cancel )
				return;

			radioButtonCoeffs.Checked = true;

			m_CB_Coeffs.Dispose();
			m_Tex_ACoeffs.Dispose();
			m_Tex_Noise.Dispose();
			m_Tex_HDRBuffer.Dispose();
			m_Tex_HDREnvironment.Dispose();
			m_CB_Render.Dispose();
			m_shader_RenderLDR.Dispose();
			m_shader_RenderScene.Dispose();
			m_shader_RenderSphere.Dispose();
			m_device.Dispose();
			m_device = null;
		}

		void m_camera_CameraTransformChanged( object sender, EventArgs e ) {
			m_CB_Render.m._world2Proj = m_camera.World2Proj;
//			m_CB_Render.m._proj2World =  m_camera.Proj2World;
			m_CB_Render.m._camera2World =  m_camera.Camera2World;
		}

		DateTime	m_startTime = DateTime.Now;
		uint		m_frameIndex = 0;
		void Application_Idle( object sender, EventArgs e ) {
			if ( m_device == null || radioButtonCoeffs.Checked )
				return;	// No 3D render

			m_frameIndex++;

			m_device.SetRenderStates( RASTERIZER_STATE.CULL_NONE, DEPTHSTENCIL_STATE.DISABLED, BLEND_STATE.DISABLED );

			Point	clientMousePos = graphPanel.PointToClient( Control.MousePosition );

			// Update CB
			m_CB_Render.m._sizeX = (uint) graphPanel.Width;
			m_CB_Render.m._sizeY = (uint) graphPanel.Height;
			m_CB_Render.m._time = (float) (DateTime.Now - m_startTime).TotalSeconds;
			m_CB_Render.m._mouse.Set( clientMousePos.X, clientMousePos.Y, (Control.MouseButtons & MouseButtons.Left) != 0 ? 1 : 0, (Control.MouseButtons & MouseButtons.Right) != 0 ? 1 : 0 );
			m_CB_Render.m._cosAO = (float) Math.Cos( floatTrackbarControlThetaMax.Value * Math.PI / 180.0 );
			m_CB_Render.m._luminanceFactor = floatTrackbarControlLuminanceFactor.Value;
			m_CB_Render.m._filteringWindowSize = floatTrackbarControlFilterWindowSize.Value;
			m_CB_Render.m._influenceAO = 0.01f * floatTrackbarControlAOInfluence.Value;
			m_CB_Render.m._influenceBentNormal = 0.01f * floatTrackbarControlBentNormalInfluence.Value;
			m_CB_Render.m._SHOrdersCount = (uint) integerTrackbarControlSHCoeffsCount.Value;
			m_CB_Render.m._customM = integerTrackbarControlm.Value;
			
			m_CB_Render.m._flags = 0;
			if ( radioButtonSideBySide.Checked )
				m_CB_Render.m._flags = 1;
			m_CB_Render.m._flags |= checkBoxAO.Checked ? 0x8U : 0U;
			m_CB_Render.m._flags |= checkBoxShowAO.Checked ? 0x10U : 0U;
			m_CB_Render.m._flags |= checkBoxShowBentNormal.Checked ? 0x20U : 0U;
			m_CB_Render.m._flags |= checkBoxUseAOAsAFactor.Checked ? 0x40U : 0U;
			m_CB_Render.m._flags |= checkBoxUseIQAO.Checked ? 0x80U : 0U;
			m_CB_Render.m._flags |= checkBoxEnvironmentSH.Checked ? 0x100U : 0U;
			m_CB_Render.m._flags |= checkBoxGroundTruth.Checked ? 0x1000U : 0U;

			m_CB_Render.UpdateData();

			m_Tex_HDREnvironment.SetPS( 0 );

			// Render to HDR buffer
			View2D	targetView = m_Tex_HDRBuffer.GetView( 0, 0, m_frameIndex & 1, 1 );
			View2D	sourceView = m_Tex_HDRBuffer.GetView( 0, 0, 1 - (m_frameIndex & 1), 1 );

			Shader	S = radioButtonSimpleScene.Checked ? m_shader_RenderScene : m_shader_RenderSphere;
			if ( S.Use() ) {
				m_device.SetRenderTargets( targetView.Width, targetView.Height, new IView[] { targetView }, null );
				m_Tex_HDRBuffer.SetPS( 1, sourceView );
				m_Tex_Noise.SetPS( 2 );
				m_Tex_ACoeffs.SetPS( 3 );
				m_device.RenderFullscreenQuad( S );
			}

			// Render to LDR back buffer
			if ( m_shader_RenderLDR.Use() ) {
				m_device.SetRenderTarget( m_device.DefaultTarget, null );
				m_Tex_HDRBuffer.SetPS( 1, targetView );
				m_device.RenderFullscreenQuad( m_shader_RenderLDR );
			}

			m_device.Present( false );
		}

		#endregion

		private void floatTrackbarControlThetaMax_ValueChanged(Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue) {
			if ( radioButtonCoeffs.Checked )
				UpdateGraph();
		}

		private void buttonReload_Click( object sender, EventArgs e ) {
			m_device.ReloadModifiedShaders();
		}

		private void radioButtonCoeffs_CheckedChanged( object sender, EventArgs e ) {
			graphPanel.Refresh();
			graphPanel.EnablePaint = radioButtonCoeffs.Checked;

			checkBoxAO.Visible = radioButtonSingleSphere.Checked | radioButtonSimpleScene.Checked;

			panelScene.Visible = radioButtonSimpleScene.Checked;
			panelGraph.Visible = !radioButtonSimpleScene.Checked;
			checkBoxShowAO.Visible = radioButtonSimpleScene.Checked;
			checkBoxShowBentNormal.Visible = radioButtonSimpleScene.Checked;
		}

		private void buttonCameraCopy_Click( object sender, EventArgs e ) {
			string	text = m_cameraManipulator.CameraPosition + "\r\n" + m_cameraManipulator.TargetPosition;
			Clipboard.SetText( text );
		}

		private void buttonCameraPaste_Click( object sender, EventArgs e ) {
			string[]	lines = Clipboard.ContainsText() ? Clipboard.GetText().Split( '\n' ) : null;
			if ( lines == null || lines.Length <= 1 ) {
				MessageBox.Show( "Clipboard doesn't contain a valid camera text!" );
				return;
			}

			float3	cameraPosition = float3.UnitY + 4.0f * float3.UnitZ;
			float3	cameraTarget = float3.UnitY;
			if (	!float3.TryParse( lines[0], ref cameraPosition )
				|| 	!float3.TryParse( lines[1], ref cameraTarget ) ) {
				// Okay, let's use that as a reset
			}

			m_cameraManipulator.InitializeCamera( cameraPosition, cameraTarget, float3.UnitY );
		}

		private void integerTrackbarControlSHCoeffsCount_ValueChanged( IntegerTrackbarControl _Sender, int _FormerValue ) {
			integerTrackbarControlm.RangeMin = -integerTrackbarControlSHCoeffsCount.Value;
			integerTrackbarControlm.RangeMax = integerTrackbarControlSHCoeffsCount.Value;
			integerTrackbarControlm.VisibleRangeMin = -integerTrackbarControlSHCoeffsCount.Value;
			integerTrackbarControlm.VisibleRangeMax = integerTrackbarControlSHCoeffsCount.Value;
		}
	}
}
