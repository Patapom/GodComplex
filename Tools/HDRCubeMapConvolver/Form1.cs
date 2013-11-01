using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;
using WMath;

namespace TestGradientPNG
{
	public partial class Form1 : Form
	{
		public unsafe Form1()
		{
			InitializeComponent();

			// Read HDR cube map as a cross image
			FileInfo	SourceFile = new FileInfo( "kitchen_cross.float" );

			int	Width, Height;
			Vector4D[,]	HDRValues = null;
			using ( Stream S = SourceFile.OpenRead() )
				using ( BinaryReader R = new BinaryReader( S ) )
				{
					Width = R.ReadInt32();
					Height = R.ReadInt32();
					HDRValues = new Vector4D[Width,Height];

					for ( int Y=0; Y < Height; Y++ )
						for ( int X=0; X < Width; X++ )
						{
							float	Red = R.ReadSingle();
							float	Green = R.ReadSingle();
							float	Blue = R.ReadSingle();
							HDRValues[X,Y] = new Vector4D( Red, Green, Blue, 1 );
						}
				}

			// Extract cube faces
			int	CubeSize = 0;
			Vector4D[][,]	CubeFaces = new Vector4D[6][,] { null, null, null, null, null, null };
			if ( Height > Width )
			{	// Vertical cross
				CubeSize = Height / 4;
				if ( Width != CubeSize * 3 )
					throw new Exception( "BROUTE!" );

				CubeFaces[0] = ReadCubeFace( HDRValues, CubeSize, 2, 1 );	// +X
				CubeFaces[1] = ReadCubeFace( HDRValues, CubeSize, 0, 1 );	// -X
				CubeFaces[2] = ReadCubeFace( HDRValues, CubeSize, 1, 0 );	// +Y
				CubeFaces[3] = ReadCubeFace( HDRValues, CubeSize, 1, 2 );	// -Y
				CubeFaces[4] = ReadCubeFace( HDRValues, CubeSize, 1, 1 );	// +Z
				CubeFaces[5] = ReadCubeFace( HDRValues, CubeSize, 1, 3, true );	// -Z
			}
			else
			{	// Horizontal cross
				CubeSize = Width / 4;
				if ( Height != CubeSize * 3 )
					throw new Exception( "BROUTE!" );

				CubeFaces[0] = ReadCubeFace( HDRValues, CubeSize, 2, 1 );	// +X
				CubeFaces[1] = ReadCubeFace( HDRValues, CubeSize, 0, 1 );	// -X
				CubeFaces[2] = ReadCubeFace( HDRValues, CubeSize, 1, 0 );	// +Y
				CubeFaces[3] = ReadCubeFace( HDRValues, CubeSize, 1, 2 );	// -Y
				CubeFaces[4] = ReadCubeFace( HDRValues, CubeSize, 1, 1 );	// +Z
				CubeFaces[5] = ReadCubeFace( HDRValues, CubeSize, 3, 1 );	// -Z
			}

			Vector4D[][][,]	CubeFacesMips = ConvolveCubeMap( CubeFaces );

			DirectXTexManaged.CubeMapCreator.CreateCubeMapFile( "Test.dds", CubeSize, CubeFacesMips );
		}

		/// <summary>
		/// This is the heart of that tool
		/// The goal is to compute mip levels for the cube map where each new mip will match the corresponding glossiness of an exponential lobe
		///		like those used in standard normal distribution models (Ward, Beckmann, etc.) so that we can use a specific mip according to the
		///		roughness parameter of the model.
		/// 
		/// The typical reflection lobe is given by the following equation:
		///		f(theta) = exp( -tan(theta)² / roughness² )
		///	
		/// Fooplot link for different plots with different roughnesses:
		///		W3sidHlwZSI6MSwiZXEiOiJleHAoLXRhbihhYnModGhldGEtcGkvMikpXjIvMC4wMSkiLCJjb2xvciI6IiMwMDgwY2MiLCJ0aGV0YW1pbiI6IjAiLCJ0aGV0YW1heCI6InBpIiwidGhldGFzdGVwIjoiLjAxIn0seyJ0eXBlIjoxLCJlcSI6ImV4cCgtdGFuKGFicyh0aGV0YS1waS8yKSleMi8wLjEpIiwiY29sb3IiOiIjMDA4MGNjIiwidGhldGFtaW4iOiIwIiwidGhldGFtYXgiOiJwaSIsInRoZXRhc3RlcCI6Ii4wMSJ9LHsidHlwZSI6MSwiZXEiOiJleHAoLXRhbihhYnModGhldGEtcGkvMikpXjIvMC40KSIsImNvbG9yIjoiIzAwODBjYyIsInRoZXRhbWluIjoiMCIsInRoZXRhbWF4IjoiMnBpIiwidGhldGFzdGVwIjoiLjAxIn0seyJ0eXBlIjoxLCJlcSI6ImV4cCgtdGFuKGFicyh0aGV0YS1waS8yKSleMi8xLjApIiwiY29sb3IiOiIjMDA4MGNjIiwidGhldGFtaW4iOiIwIiwidGhldGFtYXgiOiIycGkiLCJ0aGV0YXN0ZXAiOiIuMDEifSx7InR5cGUiOjEsImVxIjoiZXhwKC10YW4oYWJzKHRoZXRhLXBpLzIpKV4yLzIuNykiLCJjb2xvciI6IiMwMDgwY2MiLCJ0aGV0YW1pbiI6IjAiLCJ0aGV0YW1heCI6IjJwaSIsInRoZXRhc3RlcCI6Ii4wMSJ9LHsidHlwZSI6MSwiZXEiOiJleHAoLXRhbihhYnModGhldGEtcGkvMikpXjIvMS44KSIsImNvbG9yIjoiIzAwODBjYyIsInRoZXRhbWluIjoiMCIsInRoZXRhbWF4IjoiMnBpIiwidGhldGFzdGVwIjoiLjAxIn0seyJ0eXBlIjoxLCJlcSI6ImNvcygodGhldGEtcGkvMikpIiwiY29sb3IiOiIjRkYwMDY2IiwidGhldGFtaW4iOiIwIiwidGhldGFtYXgiOiJwaSIsInRoZXRhc3RlcCI6Ii4wMSJ9LHsidHlwZSI6MTAwMCwid2luZG93IjpbIi0wLjc1IiwiMC43NSIsIjAiLCIxLjA1Il19XQ
		///	
		/// 
		/// 
		/// 
		/// A roughness of 1 is trying to simulate a cosine lobe as for a standard diffuse lambert reflection
		/// Every new mip encompasses twice more pixels than the previous mip but we consider it like growing the radius of the lobe instead.
		/// 
		/// The idea is to retrieve the roughness depending on the width of the exponential which will be given by the mip level.
		/// For example, with a cube map of size 64:
		///		At mip level 0 the lobe is a straight line and roughness is then 0 (a perfect reflector).
		///		At mip level 1, a pixel has a size of 2 which translates into an angle of PI/2 * 2/64 = PI/64
		///			=> We 
		/// </summary>
		/// <param name="_CubeFaces"></param>
		/// <returns></returns>
		/// 

		private Vector4D[][,]	m_CubeFaces;
		private Vector4D[][][,]	ConvolveCubeMap( Vector4D[][,] _CubeFaces )
		{
			m_CubeFaces = _CubeFaces;

			Vector4D[][][,]	Result = new Vector4D[1][][,] { _CubeFaces };



			return Result;
		}

		private Vector4D	SampleCubeMap(  )

		private Vector4D[,]	ReadCubeFace( Vector4D[,] _Source, int _CubeSize, int _X, int _Y )
		{
			return ReadCubeFace( _Source, _CubeSize, _X, _Y, false );
		}
		private Vector4D[,]	ReadCubeFace( Vector4D[,] _Source, int _CubeSize, int _X, int _Y, bool _Flip )
		{
			int	X = _CubeSize * _X;
			int	Y = _CubeSize * _Y;

			Vector4D[,]	Result = new Vector4D[_CubeSize,_CubeSize];
			if ( _Flip )
			{
				for ( int y = 0; y < _CubeSize; y++ )
					for ( int x = 0; x < _CubeSize; x++ )
						Result[x, y] = _Source[X + _CubeSize-1-x, Y + _CubeSize-1-y];
			}
			else
			{
				for ( int y = 0; y < _CubeSize; y++ )
					for ( int x = 0; x < _CubeSize; x++ )
						Result[x, y] = _Source[X + x, Y + y];
			}

			return Result;
		}

		private void floatTrackbarControlScaleX_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.ScaleX = _Sender.Value;
		}

		private void floatTrackbarControlScaleY_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.ScaleY = _Sender.Value;
		}

		private void floatTrackbarControlWhitePoint_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.WhitePoint = _Sender.Value;
		}

		private void floatTrackbarControlA_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.A = _Sender.Value;
		}

		private void floatTrackbarControlB_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.B = _Sender.Value;
		}

		private void floatTrackbarControlC_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.C = _Sender.Value;
		}

		private void floatTrackbarControlD_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.D = _Sender.Value;
		}

		private void floatTrackbarControlE_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.E = _Sender.Value;
		}

		private void floatTrackbarControlF_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
		{
			panelGraph.F = _Sender.Value;
		}
	}
}
