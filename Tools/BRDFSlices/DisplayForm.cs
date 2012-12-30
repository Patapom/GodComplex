using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BRDFSlices
{
	public partial class DisplayForm : Form
	{
		protected Vector3[,,]	m_BRDF = null;
		protected Bitmap		m_Slice = null;
		protected Pen			m_Pen = null;

		public DisplayForm( Vector3[,,] _BRDF )
		{
			InitializeComponent();

			m_BRDF = _BRDF;
			m_Slice = new Bitmap( 90, 90, PixelFormat.Format32bppArgb );

			m_Pen = new Pen( Color.Black, 1.0f );
			m_Pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

			integerTrackbarControlPhiD_ValueChanged( integerTrackbarControlPhiD, 0 );
		}

		#region BRDF Handling

		//////////////////////////////////////////////////////////////////////////
		// This code is a translation from http://people.csail.mit.edu/wojciech/BRDFDatabase/code/BRDFRead.cpp
		//////////////////////////////////////////////////////////////////////////
		//
		const int		BRDF_SAMPLING_RES_THETA_H = 90;
		const int		BRDF_SAMPLING_RES_THETA_D = 90;
		const int		BRDF_SAMPLING_RES_PHI_D = 360;

		const double	BRDF_SCALE_RED = 1.0 / 1500.0;
		const double	BRDF_SCALE_GREEN = 1.15 / 1500.0;
		const double	BRDF_SCALE_BLUE = 1.66 / 1500.0;

		/// <summary>
		/// Given a pair of incoming/outgoing angles, look up the BRDF.
		/// </summary>
		/// <param name="_BRDF"></param>
		/// <param name="_ThetaIn"></param>
		/// <param name="_PhiIn"></param>
		/// <param name="_ThetaOut"></param>
		/// <param name="_PhiOut"></param>
		/// <param name="_ComponentScale">Should be BRDF_SCALE_RED, BRDF_SCALE_GREEN, BRDF_SCALE_BLUE depending on the component</param>
		/// <returns></returns>
		public static void	LookupBRDF( Vector3[,,] _BRDF, double _ThetaIn, double _PhiIn, double _ThetaOut, double _PhiOut, ref Vector3 _Result )
		{
			// Convert to half angle / difference angle coordinates
			double ThetaHalf, PhiHalf, ThetaDiff, PhiDiff;
			std_coords_to_half_diff_coords(	_ThetaIn, _PhiIn, _ThetaOut, _PhiOut,
											out ThetaHalf, out PhiHalf, out ThetaDiff, out PhiDiff );

			// (note that PhiHalf is ignored, since isotropic BRDFs are assumed)
			Vector3	r;
			int PhiDiffIndex = PhiDiff_index( PhiDiff, out r.z );
			int	ThetaDiffIndex = ThetaDiff_index( ThetaDiff, out r.y );
			int	ThetaHalfIndex = ThetaHalf_index( ThetaHalf, out r.x );

			_Result = _BRDF[ThetaHalfIndex,ThetaDiffIndex,PhiDiffIndex];
		}

		/// <summary>
		/// Given a pair of incoming/outgoing angles, look up the BRDF.
		/// </summary>
		/// <param name="_BRDF"></param>
		/// <param name="_ThetaIn"></param>
		/// <param name="_PhiIn"></param>
		/// <param name="_ThetaOut"></param>
		/// <param name="_PhiOut"></param>
		/// <param name="_ComponentScale">Should be BRDF_SCALE_RED, BRDF_SCALE_GREEN, BRDF_SCALE_BLUE depending on the component</param>
		/// <returns></returns>
		static Vector3[,,]	TempTrilerp = new Vector3[2,2,2];
		static Vector3[,]	TempBilerp = new Vector3[2,2];
		static Vector3[]	TempLerp = new Vector3[2];
		public static void	LookupBRDFTrilinear( Vector3[,,] _BRDF, double _ThetaHalf, double _ThetaDiff, double _PhiDiff, ref Vector3 _Result )
		{
			// (note that PhiHalf is ignored, since isotropic BRDFs are assumed)
			Vector3	r;
			int PhiDiffIndex0 = PhiDiff_index( _PhiDiff, out r.z );
			int	ThetaDiffIndex0 = ThetaDiff_index( _ThetaDiff, out r.y );
			int	ThetaHalfIndex0 = ThetaHalf_index( _ThetaHalf, out r.x );

			int	PhiDiffIndex1 = Math.Min( BRDF_SAMPLING_RES_PHI_D/2-1, PhiDiffIndex0+1 );
			int	ThetaDiffIndex1 = Math.Min( BRDF_SAMPLING_RES_THETA_D-1, ThetaDiffIndex0+1 );
			int	ThetaHalfIndex1 = Math.Min( BRDF_SAMPLING_RES_THETA_H-1, ThetaHalfIndex0+1 );

			TempTrilerp[0,0,0] = _BRDF[ThetaHalfIndex0,ThetaDiffIndex0,PhiDiffIndex0];
			TempTrilerp[0,0,1] = _BRDF[ThetaHalfIndex0,ThetaDiffIndex0,PhiDiffIndex1];
			TempTrilerp[0,1,1] = _BRDF[ThetaHalfIndex0,ThetaDiffIndex1,PhiDiffIndex1];
			TempTrilerp[0,1,0] = _BRDF[ThetaHalfIndex0,ThetaDiffIndex1,PhiDiffIndex0];
			TempTrilerp[1,0,0] = _BRDF[ThetaHalfIndex1,ThetaDiffIndex0,PhiDiffIndex0];
			TempTrilerp[1,0,1] = _BRDF[ThetaHalfIndex1,ThetaDiffIndex0,PhiDiffIndex1];
			TempTrilerp[1,1,1] = _BRDF[ThetaHalfIndex1,ThetaDiffIndex1,PhiDiffIndex1];
			TempTrilerp[1,1,0] = _BRDF[ThetaHalfIndex1,ThetaDiffIndex1,PhiDiffIndex0];

			TempBilerp[0,0] = (1.0 - r.z) * TempTrilerp[0,0,0] + r.z * TempTrilerp[0,0,1];
			TempBilerp[0,1] = (1.0 - r.z) * TempTrilerp[0,1,0] + r.z * TempTrilerp[0,1,1];
			TempBilerp[1,1] = (1.0 - r.z) * TempTrilerp[1,1,0] + r.z * TempTrilerp[1,1,1];
			TempBilerp[1,0] = (1.0 - r.z) * TempTrilerp[1,0,0] + r.z * TempTrilerp[1,0,1];

			TempLerp[0] = (1.0 - r.y) * TempBilerp[0,0] + r.y * TempBilerp[0,1];
			TempLerp[1] = (1.0 - r.y) * TempBilerp[1,0] + r.y * TempBilerp[1,1];

			_Result = (1.0 - r.x) * TempLerp[0] + r.x * TempLerp[1];
		}

		/// <summary>
		/// Convert standard (Theta,Phi) coordinates to half vector & difference vector coordinates
		/// (from http://graphics.stanford.edu/papers/brdf_change_of_variables/brdf_change_of_variables.pdf)
		/// </summary>
		/// <param name="_ThetaIn"></param>
		/// <param name="_PhiIn"></param>
		/// <param name="_ThetaOut"></param>
		/// <param name="_PhiOut"></param>
		/// <param name="_ThetaHalf"></param>
		/// <param name="_PhiHalf"></param>
		/// <param name="_ThetaDiff"></param>
		/// <param name="_PhiDiff"></param>
		// 
		static private Vector3	In = new Vector3();
		static private Vector3	Out = new Vector3();
		static private Vector3	Half = new Vector3();
		static private Vector3	Diff = new Vector3();
		static private Vector3	Tangent = new Vector3() { x=1.0, y=0.0, z=0.0 };
		static private Vector3	BiTangent = new Vector3() { x=0.0, y=1.0, z=0.0 };
		static private Vector3	Normal = new Vector3() { x=0.0, y=0.0, z=1.0 };
		static private Vector3	Temp = new Vector3();
		static void std_coords_to_half_diff_coords( double _ThetaIn, double _PhiIn, double _ThetaOut, double _PhiOut,
													out double _ThetaHalf, out double _PhiHalf, out double _ThetaDiff, out double _PhiDiff )
		{
			// compute in vector
			double in_vec_z = Math.Cos(_ThetaIn);
			double proj_in_vec = Math.Sin(_ThetaIn);
			double in_vec_x = proj_in_vec*Math.Cos(_PhiIn);
			double in_vec_y = proj_in_vec*Math.Sin(_PhiIn);
			In.Set( in_vec_x, in_vec_y, in_vec_z );

			// compute out vector
			double out_vec_z = Math.Cos(_ThetaOut);
			double proj_out_vec = Math.Sin(_ThetaOut);
			double out_vec_x = proj_out_vec*Math.Cos(_PhiOut);
			double out_vec_y = proj_out_vec*Math.Sin(_PhiOut);
			Out.Set( out_vec_x, out_vec_y, out_vec_z );

			// compute halfway vector
			Half.Set( in_vec_x + out_vec_x, in_vec_y + out_vec_y, in_vec_z + out_vec_z );
			Half.Normalize();

			// compute  _ThetaHalf, _PhiHalf
			_ThetaHalf = Math.Acos( Half.z );
			_PhiHalf = Math.Atan2( Half.y, Half.x );

			// Compute diff vector
			In.Rotate( ref Normal, -_PhiHalf, out Temp );
			Temp.Rotate( ref BiTangent, -_ThetaHalf, out Diff );
	
			// Compute _ThetaDiff, _PhiDiff	
			_ThetaDiff = Math.Acos( Diff.z );
			_PhiDiff = Math.Atan2( Diff.y, Diff.x );
		}

		static void	half_diff_coords_to_std_coords( double _ThetaHalf, double _PhiHalf, double _ThetaDiff, double _PhiDiff,
													out double _ThetaIn, out double _PhiIn, out double _ThetaOut, out double _PhiOut )
		{
			double	SinTheta_half = Math.Sin( _ThetaHalf );
			Half.Set( Math.Cos( _PhiHalf ) * SinTheta_half, Math.Sin( _PhiHalf ) * SinTheta_half, Math.Cos( _ThetaHalf ) );

			// Build the 2 vectors representing the frame in which we can use the diff angles
			Vector3	OrthoX;
			Half.Cross( ref Normal, out OrthoX );
			if ( OrthoX.LengthSq() < 1e-6 )
				OrthoX.Set( 1, 0, 0 );
			else
				OrthoX.Normalize();

			Vector3	OrthoY;
			Half.Cross( ref OrthoX, out OrthoY );

			// Rotate using diff angles to retrieve incoming direction
			Half.Rotate( ref OrthoX, -_ThetaDiff, out Temp );
			Temp.Rotate( ref Half, _PhiDiff, out In );

			// We can get the outgoing vector either by rotating the incoming vector half a circle
// 			Temp.Rotate( ref Half, _PhiDiff + Math.PI, out Out );

			// ...or by mirroring in "Half tangent space"
			double	MirrorX = -In.Dot( ref OrthoX );
			double	MirrorY = -In.Dot( ref OrthoY );
			double	z = In.Dot( ref Half );
			Out.Set(
				MirrorX*OrthoX.x + MirrorY*OrthoY.x + z*Half.x,
				MirrorX*OrthoX.y + MirrorY*OrthoY.y + z*Half.y,
				MirrorX*OrthoX.z + MirrorY*OrthoY.z + z*Half.z
			);

			// CHECK
// 			Vector3	CheckHalf = new Vector3() { x = In.x+Out.x, y = In.y+Out.y, z = In.z+Out.z };
// 					CheckHalf.Normalize();	// Is this Half ???
			// CHECK

			// Finally, we can retrieve the angles we came here to look for...
			_ThetaIn = Math.Acos( In.z );
			_PhiIn = Math.Atan2( In.y, In.x );
			_ThetaOut = Math.Acos( Out.z );
			_PhiOut = Math.Atan2( Out.y, Out.x );
		}

		static void	half_diff_coords_to_std_coords( double _ThetaHalf, double _PhiHalf, double _ThetaDiff, double _PhiDiff,
													ref Vector3 _In, ref Vector3 _Out )
		{
			double	SinTheta_half = Math.Sin( _ThetaHalf );
			Half.Set( Math.Cos( _PhiHalf ) * SinTheta_half, Math.Sin( _PhiHalf ) * SinTheta_half, Math.Cos( _ThetaHalf ) );

			// Build the 2 vectors representing the frame in which we can use the diff angles
			Vector3	OrthoX;
			Half.Cross( ref Normal, out OrthoX );
			if ( OrthoX.LengthSq() < 1e-6 )
				OrthoX.Set( 1, 0, 0 );
			else
				OrthoX.Normalize();

			Vector3	OrthoY;
			Half.Cross( ref OrthoX, out OrthoY );

			// Rotate using diff angles to retrieve incoming direction
			Half.Rotate( ref OrthoX, -_ThetaDiff, out Temp );
			Temp.Rotate( ref Half, _PhiDiff, out _In );

			// ...or by mirroring in "Half tangent space"
			double	MirrorX = -_In.Dot( ref OrthoX );
			double	MirrorY = -_In.Dot( ref OrthoY );
			double	z = _In.Dot( ref Half );
			_Out.Set(
				MirrorX*OrthoX.x + MirrorY*OrthoY.x + z*Half.x,
				MirrorX*OrthoX.y + MirrorY*OrthoY.y + z*Half.y,
				MirrorX*OrthoX.z + MirrorY*OrthoY.z + z*Half.z
			);

// 			if ( _In.z < -0.5 || _Out.z < -0.5 )
// 				throw new Exception( "RHA MAIS MERDE!" );
		}

		// Lookup _ThetaHalf index
		// This is a non-linear mapping!
		// In:  [0 .. pi/2]
		// Out: [0 .. 89]
		static int ThetaHalf_index( double _ThetaHalf, out double _Interpolant )
		{
			if ( _ThetaHalf <= 0.0 )
			{
				_Interpolant = 0.0;
				return 0;
			}

			double	ThetaHalf_deg = ((_ThetaHalf / (0.5*Math.PI)) * BRDF_SAMPLING_RES_THETA_H);
			double	fIndex = ThetaHalf_deg*BRDF_SAMPLING_RES_THETA_H;
					fIndex = Math.Sqrt( fIndex );

			int Index = (int) Math.Floor( fIndex );
			_Interpolant = fIndex - Index;	// This is wrong but I suppose it won't be noticeable! (the interpolant should increase with sqrt as well)
				Index = Math.Max( 0, Math.Min( Index, BRDF_SAMPLING_RES_THETA_H-1 ) );
			return Index;
		}

		// Lookup _ThetaDiff index
		// In:  [0 .. pi/2]
		// Out: [0 .. 89]
		static int ThetaDiff_index( double _ThetaDiff, out double _Interpolant )
		{
			double	fIndex = _ThetaDiff / (Math.PI * 0.5) * BRDF_SAMPLING_RES_THETA_D;
			int		Index = (int) Math.Floor( fIndex );
			_Interpolant = fIndex - Index;
					Index = Math.Max( 0, Math.Min( Index, BRDF_SAMPLING_RES_THETA_D-1 ) );
			return Index;
		}

		// Lookup _PhiDiff index
		static int PhiDiff_index( double _PhiDiff, out double _Interpolant )
		{
			// Because of reciprocity, the BRDF is unchanged under
			// _PhiDiff -> _PhiDiff + PI
			if ( _PhiDiff < 0.0 )
				_PhiDiff += Math.PI;

			// In: _PhiDiff in [0 .. PI]
			// Out: tmp in [0 .. 179]
			double	fIndex = 2*_PhiDiff / Math.PI * BRDF_SAMPLING_RES_PHI_D;
			int		Index = (int) Math.Floor( fIndex );
			_Interpolant = fIndex - Index;
					Index = Math.Max( 0, Math.Min( Index, BRDF_SAMPLING_RES_PHI_D/2-1 ) );
			return Index;
		}

		/// <summary>
		/// Loads a MERL BRDF file
		/// </summary>
		/// <param name="_BRDFFile"></param>
		/// <returns>The 3D array of RGB values (dimension 0 is ThetaH in [0,89], dimension 1 is ThetaD in [0,89], dimension 2 is PhiD in 0,179])</returns>
		public static Vector3[,,]	LoadBRDF( FileInfo _BRDFFile )
		{
			Vector3[,,]	Result = null;
			try
			{
				using ( FileStream S = _BRDFFile.OpenRead() )
					using ( BinaryReader Reader = new BinaryReader( S ) )
					{
						// Check coefficients count is the expected value
						int	DimX = Reader.ReadInt32();
						int	DimY = Reader.ReadInt32();
						int	DimZ = Reader.ReadInt32();
						int	CoeffsCount = DimX*DimY*DimZ;
						if ( CoeffsCount != BRDF_SAMPLING_RES_THETA_H*BRDF_SAMPLING_RES_THETA_D*BRDF_SAMPLING_RES_PHI_D/2 )
							throw new Exception( "The amount of coefficients stored in the file is not the expected value (i.e. " + CoeffsCount + "! (is it a BRDF file?)" );

						// Allocate the R,G,B arrays
						Result = new Vector3[DimX,DimY,DimZ];

						int	PitchThetaD = BRDF_SAMPLING_RES_PHI_D/2;
						int	PitchThetaH = PitchThetaD*BRDF_SAMPLING_RES_THETA_D;

						// Read content
						int[]		NegativeValuesCount = new int[3] { 0, 0, 0 };
						Vector3		MinValues = new Vector3() { x=double.MaxValue, y=double.MaxValue, z=double.MaxValue };

						for ( int ComponentIndex=0; ComponentIndex < 3; ComponentIndex++ )
						{
							double	Factor = 1.0;
							if ( ComponentIndex == 0 )
								Factor = BRDF_SCALE_RED;
							else if ( ComponentIndex == 0 )
								Factor = BRDF_SCALE_GREEN;
							else 
								Factor = BRDF_SCALE_BLUE;

							for ( int CoeffIndex=0; CoeffIndex < CoeffsCount; CoeffIndex++ )
							{
								int	Temp = CoeffIndex;
								int	ThetaH = Temp / PitchThetaH;
								Temp -= ThetaH * PitchThetaH;
								int	ThetaD = Temp / PitchThetaD;
								Temp -= ThetaD * PitchThetaD;
								int	PhiD = Temp;

								double	Value = Factor * Reader.ReadDouble();

//								Result[ThetaH,ThetaD,PhiD] = V = new Vector3();
								Result[ThetaH,ThetaD,PhiD][ComponentIndex] = Value;

								if ( Value < 0.0 )
									NegativeValuesCount[ComponentIndex]++;
								MinValues[ComponentIndex] = Math.Min( MinValues[ComponentIndex], Value );
							}

						}
					}
			}
			catch ( Exception _e )
			{	// Forward...
				throw new Exception( "Failed to load source BRDF file: " + _e.Message );
			}

			return Result;
		}

		#endregion

		protected void	WarpSlice( int _ThetaHIndex, int _ThetaDIndex, double _Warp, ref Vector3 _Result )
		{
			double	ThetaD = Math.PI * _ThetaDIndex / 180.0;
			double	PhiD = Math.PI * _Warp / 180.0;
			double	ThetaH_Max = Math.Atan( Math.Tan( 0.5*Math.PI - ThetaD ) / Math.Cos( PhiD ) );

// 			double	ThetaH = Math.PI * _ThetaHIndex / 180.0;
// 			LookupBRDFTrilinear( m_BRDF, ThetaH, ThetaD, 0.5*Math.PI, ref _Result );

			int		ThetaH_MaxIndex = Math.Max( 1, (int) (ThetaH_Max * 180 / Math.PI) );

			// Add a security slice
			ThetaH_MaxIndex = Math.Min( 89, ThetaH_MaxIndex+4 );

			int		WarpedThetaHIndex = (int) (89 * _ThetaHIndex / ThetaH_MaxIndex);	// Scale so we reach 90° at max ThetaH
			if ( WarpedThetaHIndex > 89 )
			{
				_Result = new Vector3();	// Out of range
				return;
			}

			int	ThetaH = (int) (89 * Math.Sqrt( WarpedThetaHIndex / 89.0 ));	// Square ThetaH

			_Result = m_BRDF[ThetaH,_ThetaDIndex,90];
		}

		protected unsafe void	Redraw()
		{
			int		PhiD = integerTrackbarControlPhiD.Value;
			double	Gamma = 1.0 / floatTrackbarControlGamma.Value;
			double	Exposure = Math.Pow( 2.0, floatTrackbarControlExposure.Value );
			bool	bShowDifferences = checkBoxDifferences.Checked;

			bool	bUseWarping = checkBoxUseWarping.Checked;
			double	Warp = floatTrackbarControlWarpFactor.Value;

			byte	R, G, B;
			Vector3	Temp = new Vector3(), Temp2;
			BitmapData	LockedBitmap = m_Slice.LockBits( new Rectangle( 0, 0, m_Slice.Width, m_Slice.Height ), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb );
			for ( int Y=0; Y < 90; Y++ )
			{
				int		ThetaD = 89 - Y;
				byte*	pScanline = (byte*) LockedBitmap.Scan0.ToPointer() + Y * LockedBitmap.Stride;
				for ( int X=0; X < 90; X++ )
				{
					if ( !bUseWarping )
					{
						int	ThetaH = (int) (89 * Math.Sqrt( X/89.0 ));	// Square ThetaH

						Temp = m_BRDF[ThetaH,ThetaD,PhiD];

						if ( !bShowDifferences )
						{
							Temp.x = Math.Pow( Exposure * Temp.x, Gamma );
							Temp.y = Math.Pow( Exposure * Temp.y, Gamma );
							Temp.z = Math.Pow( Exposure * Temp.z, Gamma );
						}
						else
						{
							Temp2 = m_BRDF[ThetaH,ThetaD,Math.Min( 179, 179-PhiD )];	// Read from symetrical slice
							Temp.x = 64.0 * Exposure * Math.Abs( Temp.x - Temp2.x );
							Temp.y = 64.0 * Exposure * Math.Abs( Temp.y - Temp2.y );
							Temp.z = 64.0 * Exposure * Math.Abs( Temp.z - Temp2.z );
						}
					}
					else
					{	// Use slice warping !
						int	ThetaH = X;	// Don't square ThetaH yet!

						WarpSlice( ThetaH, ThetaD, Warp, ref Temp );
						Temp.x = Math.Pow( Exposure * Temp.x, Gamma );
						Temp.y = Math.Pow( Exposure * Temp.y, Gamma );
						Temp.z = Math.Pow( Exposure * Temp.z, Gamma );
					}

					R = (byte) Math.Max( 0, Math.Min( 255, 255.0 * Temp.x ) );
					G = (byte) Math.Max( 0, Math.Min( 255, 255.0 * Temp.y ) );
					B = (byte) Math.Max( 0, Math.Min( 255, 255.0 * Temp.z ) );

					*pScanline++ = B;
					*pScanline++ = G;
					*pScanline++ = R;
					*pScanline++ = 0xFF;
				}
			}
			m_Slice.UnlockBits( LockedBitmap );

			if ( checkBoxShowIsolines.Checked )
			{
				PointF	P0 = new PointF(), P1 = new PointF();

				using ( Graphics Graph = Graphics.FromImage( m_Slice ) )
				{
					double	ThetaHalf, PhiHalf, ThetaDiff, PhiDiff;
					double	ThetaHalfN, PhiHalfN, ThetaDiffN, PhiDiffN;
					for ( int IsolineIndex=0; IsolineIndex < 4; IsolineIndex++ )
					{
//						double	Angle = (1+IsolineIndex) * 0.5 * Math.PI / 4;
						double	Angle = 0.25 * Math.PI;

						for ( int i=0; i < 40; i++ )
						{
							double	Phi = i * Math.PI / 40;
//							std_coords_to_half_diff_coords( Angle, Phi, Angle, Math.PI + Phi, out ThetaHalf, out PhiHalf, out ThetaDiff, out PhiDiff );
							std_coords_to_half_diff_coords( Angle, 0, i * Math.PI / 80.0, Math.PI, out ThetaHalf, out PhiHalf, out ThetaDiff, out PhiDiff );

							Phi = (i+1) * Math.PI / 40;
//							std_coords_to_half_diff_coords( Angle, Phi, Angle, Math.PI + Phi, out ThetaHalfN, out PhiHalfN, out ThetaDiffN, out PhiDiffN );
							std_coords_to_half_diff_coords( Angle, 0, (i+1) * Math.PI / 80.0, Math.PI, out ThetaHalfN, out PhiHalfN, out ThetaDiffN, out PhiDiffN );

							P0.X = (float) (m_Slice.Width * ThetaHalf * 0.63661977236758134307553505349006);			// divided by PI/2
							P0.Y = (float) (m_Slice.Height * (1.0f - ThetaDiff * 0.63661977236758134307553505349006));	// divided by PI/2
							P1.X = (float) (m_Slice.Width * ThetaHalfN * 0.63661977236758134307553505349006);			// divided by PI/2
							P1.Y = (float) (m_Slice.Height * (1.0f - ThetaDiffN * 0.63661977236758134307553505349006));	// divided by PI/2
							Graph.DrawLine( m_Pen, P0, P1 );
						}
					}
				}
			}

			panelDisplay.Slice = m_Slice;	// Will trigger update
		}

		private void integerTrackbarControlPhiD_ValueChanged( Nuaj.Cirrus.Utility.IntegerTrackbarControl _Sender, int _FormerValue )
		{
			Redraw();
		}

		private void floatTrackbarControlGamma_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			Redraw();
		}

		private void floatTrackbarControlExposure_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			Redraw();
		}

		private void checkBoxDifferences_CheckedChanged( object sender, EventArgs e )
		{
			Redraw();
		}

		private void checkBoxUseWarping_CheckedChanged( object sender, EventArgs e )
		{
			Redraw();
			floatTrackbarControlWarpFactor.Enabled = checkBoxUseWarping.Checked;
		}

		private void floatTrackbarControlWarpFactor_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			Redraw();
		}

		private void checkBoxShowIsolines_CheckedChanged( object sender, EventArgs e )
		{
			Redraw();
		}
	}
}
