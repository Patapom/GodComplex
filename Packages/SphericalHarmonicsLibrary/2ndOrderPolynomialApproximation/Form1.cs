using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;

using WMath;

namespace SecondOrderPolynomialApproximation
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();

			// Simulate a click to display something
			sphereView_MouseUp( sphereView, new MouseEventArgs( MouseButtons.Left, 1, sphereView.ClientRectangle.Width / 4, sphereView.ClientRectangle.Height / 2, 0 ) );
		}

		protected double[]	m_Coefficients = new double[3*3];	// Low order means 2

		protected Vector	m_AlignAxis = null;
		private void sphereView_MouseUp( object sender, MouseEventArgs e )
		{
			// Determine the position on the sphere
			if ( e.X < sphereView.Width / 2 )
			{	// Front
				float	fDx = e.X / (.25f * sphereView.Width) - 1.0f;
				float	fDy = 1.0f - e.Y / (.5f * sphereView.Height);
				float	fSDistance = fDx * fDx + fDy * fDy;
				if ( fSDistance > 1.0f )
					return;

				m_AlignAxis = new Vector( fDx, fDy, (float) Math.Sqrt( 1.0f - fSDistance ) );
			}
			else
			{	// Front
				float	fDx = e.X / (.25f * sphereView.Width) - 3.0f;
				float	fDy = 1.0f - e.Y / (.5f * sphereView.Height);
				float	fSDistance = fDx * fDx + fDy * fDy;
				if ( fSDistance > 1.0f )
					return;

				m_AlignAxis = new Vector( fDx, fDy, -(float) Math.Sqrt( 1.0f - fSDistance ) );
			}

			if ( m_AlignAxis == null )
				return;

			GetSHCoefficients( m_AlignAxis, ref m_Coefficients );

			sphereView.RebuildSphere( new SphereView.GetValue( GetValueApproximate2ndOrderPolynomial ) );
			sphereView.Refresh();
		}

		protected float		GetValueApproximate2ndOrderPolynomial( float _x, float _y, float _z )
		{
			Vector	Dir = new Vector( _x, _y, _z );
			if ( (Dir | m_AlignAxis) < 0 )
				return	0;

			return	(float) SphericalHarmonics.SHFunctions.EvaluateSH( m_Coefficients, Dir, 3 );
		}


		protected Vector[]	m_RGBCoefficients = new Vector[3*3];

		private void sphereViewLSH_MouseUp( object sender, MouseEventArgs e )
		{
			if ( openFileDialog.ShowDialog( this ) != DialogResult.OK )
				return;

			FileInfo		File = new FileInfo( openFileDialog.FileName );
			FileStream		Stream = File.OpenRead();
			BinaryReader	Reader = new BinaryReader( Stream );

			uint	Width = Reader.ReadUInt32();
			uint	Height = Reader.ReadUInt32();

			// Read Ambient
			Reader.ReadUInt32();

			// Read Diffuse
			Reader.ReadUInt32();

			// Read main light direction
			Reader.ReadSingle();
			Reader.ReadSingle();
			Reader.ReadSingle();

			// Read the 9 SH coefficients
			float	fBias = Reader.ReadSingle();
			for ( int CoeffIndex=0; CoeffIndex < 9; CoeffIndex++ )
				m_RGBCoefficients[CoeffIndex] = new Vector( fBias, fBias, fBias ) + UnPackRGBE( Reader.ReadUInt32() );

			Reader.Close();
			Stream.Close();

			sphereViewLSH.RebuildSphereWithColor( new SphereView.GetColorValue( GetColorValueFromLSH) );
			sphereViewLSH.Refresh();
			sphereViewLSHOptimized.RebuildSphereWithColor( new SphereView.GetColorValue( GetColorValueFromLSHOptimized) );
			sphereViewLSHOptimized.Refresh();
		}

		protected Color	GetColorValueFromLSH( float _x, float _y, float _z )
		{
			Vector		Dir = new Vector( _x, _y, _z );
			double[]	TempCoeffs = new double[9];
			double[]	Result = new double[3];
			for ( int ComponentIndex=0; ComponentIndex < 3; ComponentIndex++ )
			{
				for ( int CoeffIndex=0; CoeffIndex < 9; CoeffIndex++ )
					TempCoeffs[CoeffIndex] = m_RGBCoefficients[CoeffIndex][ComponentIndex];

				Result[ComponentIndex] = SphericalHarmonics.SHFunctions.EvaluateSH( TempCoeffs, Dir, 3 );
			}

			byte	Red = (byte) Math.Min( 255, Math.Max( 0, 255 * Result[0] ) );
			byte	Green = (byte) Math.Min( 255, Math.Max( 0, 255 * Result[1] ) );
			byte	Blue = (byte) Math.Min( 255, Math.Max( 0, 255 * Result[2] ) );

			return Color.FromArgb( Red, Green, Blue );
		}

		// Same as above, but optimized with SH Coefficients retrieved from 2nd order approximation
		protected Color	GetColorValueFromLSHOptimized( float _x, float _y, float _z )
		{
			Vector		Dir = new Vector( _x, _y, _z );
			double[]	DirectionalCoeffs = new double[9];
			GetSHCoefficients( Dir, ref DirectionalCoeffs );

			double[]	Result = new double[3];
			double[]	TempCoeffs = new double[9];
			for ( int ComponentIndex=0; ComponentIndex < 3; ComponentIndex++ )
			{
				for ( int CoeffIndex=0; CoeffIndex < 9; CoeffIndex++ )
				{
					double	Coeff = m_RGBCoefficients[CoeffIndex][ComponentIndex];
					Result[ComponentIndex] += Coeff * DirectionalCoeffs[CoeffIndex];
				}
			}

			byte	Red = (byte) Math.Min( 255, Math.Max( 0, 255 * Result[0] ) );
			byte	Green = (byte) Math.Min( 255, Math.Max( 0, 255 * Result[1] ) );
			byte	Blue = (byte) Math.Min( 255, Math.Max( 0, 255 * Result[2] ) );

			return Color.FromArgb( Red, Green, Blue );
		}

		protected WMath.Vector	UnPackRGBE( uint _RGBE )
		{
			byte	R = (byte) ((_RGBE >>  8) & 0xFF);
			byte	G = (byte) ((_RGBE >> 16) & 0xFF);
			byte	B = (byte) ((_RGBE >> 24) & 0xFF);
			byte	E = (byte) ((_RGBE >>  0) & 0xFF);

			float	fExponentMultiplier = (float) Math.Pow( 2.0, (float) E - (128 + 8) );

			return	fExponentMultiplier * new WMath.Vector( R + 0.5f, G + 0.5f, B + 0.5f );
		}

		protected void	GetSHCoefficients( WMath.Vector _Direction, ref double[] _Coefficients )
		{
			float	x = -_Direction.z;
			float	y = _Direction.y;
			float	z = _Direction.x;

			// Build the SH coefficients from analytical formulae given in http://www-graphics.stanford.edu/papers/envmap/
			_Coefficients[0] = 0.282095;					// Y00

			_Coefficients[1] = 0.488603 * z;				// Y1-1
			_Coefficients[2] = 0.488603 * y;				// Y10
			_Coefficients[3] = 0.488603 * x;				// Y1+1

			_Coefficients[4] = 1.092548 * x * z;			// Y2-2
			_Coefficients[5] = 1.092548 * y * z;			// Y2-1
			_Coefficients[6] = 0.315392 * (3*y*y - 1);		// Y20
			_Coefficients[7] = 1.092548 * x * y;			// Y2+1
			_Coefficients[8] = 0.546274 * (x*x - z*z);		// Y2+2
		}
	}
}