using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ImageUtility
{
	/// <summary>
	/// This is the interface to pixel format structures needed for images and textures
	/// NOTE: This interface is almost identical to the one found in the C++ GodComplex renderer but is abstract from DirectX internal DXGI formats
	/// </summary>
	public interface IPixelFormat
	{
		/// <summary>
		/// Tells if the format uses sRGB input (cf. Image<PF> Gamma Correction)
		/// </summary>
		bool	sRGB			{ get; }

		/// <summary>
		/// LDR pixel writer
		/// </summary>
		/// <param name="_R"></param>
		/// <param name="_G"></param>
		/// <param name="_B"></param>
		/// <param name="_A"></param>
		void	Write( uint _R, uint _G, uint _B, uint _A );
		void	Write( uint _A );

		/// <summary>
		/// HDR pixel writer
		/// </summary>
		/// <param name="_Color"></param>
		void	Write( float4 _Color );
		void	Write( float _R, float _G, float _B, float _A );
		void	Write( float _A );

		float	Red			{ get; }
		float	Green		{ get; }
		float	Blue		{ get; }
		float	Alpha		{ get; }
	}

	/// <summary>
	/// This is the interface to depth format structures needed for depth stencil buffers
	/// They inherit pixel format data and define additional data
	/// </summary>
	public interface IDepthFormat : IPixelFormat
	{
		// Actually, they don't! :)
	}

	[StructLayout( LayoutKind.Sequential )]
	public struct	PF_Empty : IPixelFormat {
		#region CODE

		#region IPixelFormat Members

		public bool	sRGB	{ get { return false; } }

		public void Write( uint _R, uint _G, uint _B, uint _A )		{}
		public void Write( float4 _Color )	{}
		public void Write( float _R, float _G, float _B, float _A )	{}
		public void Write( uint _A )		{}
		public void Write( float _A )		{}
		public int  MipAverage( int _C0, int _C1, int _C2, int _C3 )	{ return 0; }

		public float	Red		{ get { return 0.0f; } }
		public float	Green	{ get { return 0.0f; } }
		public float	Blue	{ get { return 0.0f; } }
		public float	Alpha	{ get { return 1.0f; } }

		#endregion

		/// <summary>
		/// Converts a byte component to a float component
		/// </summary>
		/// <param name="_Component"></param>
		/// <returns></returns>
		public static float		ToFloat( uint _Component )
		{
			return _Component / 255.0f;
		}

		/// <summary>
		/// Converts a float component to a byte component
		/// </summary>
		/// <param name="_Component"></param>
		/// <returns></returns>
		public static byte		ToByte( float _Component )
		{
			return (byte) Math.Min( 255.0f, Math.Max( 0.0f, _Component * 255.0f ) );
		}

		/// <summary>
		/// Converts a float component to a byte component
		/// </summary>
		/// <param name="_Component"></param>
		/// <returns></returns>
		public static ushort	ToUShort( float _Component )
		{
			return (ushort) Math.Min( 65535.0f, Math.Max( 0.0f, _Component * 65535.0f) );
		}

		#endregion
	}

	#region 8-Bits Formats

	/// <summary>
	/// R8 format
	/// </summary>
	[StructLayout( LayoutKind.Sequential )]
	public struct	PF_R8 : IPixelFormat
	{
		public byte	R;

		#region IPixelFormat Members

		public bool	sRGB	{ get { return false; } }

		public void Write( uint _R, uint _G, uint _B, uint _A )
		{
			R = (byte) _R;
		}

		public void Write( float4 _Color )
		{
			R = PF_Empty.ToByte(_Color.x);
		}

		public void Write( float _R, float _G, float _B, float _A )
		{
			R = PF_Empty.ToByte( _R );
		}

		public void Write( uint _A )
		{
		}

		public void Write( float _A )
		{
		}

		public float	Red		{ get { return R / 255.0f; } }
		public float	Green	{ get { return 0.0f; } }
		public float	Blue	{ get { return 0.0f; } }
		public float	Alpha	{ get { return 1.0f; } }

		#endregion
	}

	/// <summary>
	/// RG8 format
	/// </summary>
	[StructLayout( LayoutKind.Sequential )]
	public struct	PF_RG8 : IPixelFormat
	{
		public byte	G, R;

		#region IPixelFormat Members

		public bool	sRGB	{ get { return false; } }

		public void Write( uint _R, uint _G, uint _B, uint _A )
		{
			R = (byte) _R;
			G = (byte) _G;
		}

		public void Write( float4 _Color )
		{
			R = PF_Empty.ToByte(_Color.x);
			G = PF_Empty.ToByte(_Color.y);
		}

		public void Write( float _R, float _G, float _B, float _A )
		{
			R = PF_Empty.ToByte( _R );
			G = PF_Empty.ToByte( _G );
		}

		public void Write( uint _A )
		{
		}

		public void Write( float _A )
		{
		}

		public float	Red		{ get { return R / 255.0f; } }
		public float	Green	{ get { return G / 255.0f; } }
		public float	Blue	{ get { return 0.0f; } }
		public float	Alpha	{ get { return 1.0f; } }

		#endregion
	}

// UNSUPPORTED !
// 	/// <summary>
// 	/// RGB8 format
// 	/// </summary>
// 	[StructLayout( LayoutKind.Sequential )]
// 	public struct	PF_RGB8 : IPixelFormat
// 	{
// 		public byte	B, G, R;
// 
// 		#region IPixelFormat Members
// 
// 		public Format DirectXFormat
// 		{
// 			get { return Format.R8G8B8A8_UNorm; }
// 		}
// 
// 		public void Write( uint _R, uint _G, uint _B, uint _A )
// 		{
// 			R = (byte) _R;
// 			G = (byte) _G;
// 			B = (byte) _B;
// 		}
// 
// 		public void Write( Vector4D _Color )
// 		{
// 			R = PF_Empty.ToByte(_Color.X);
// 			G = PF_Empty.ToByte(_Color.Y);
// 			B = PF_Empty.ToByte(_Color.Z);
// 		}
// 
// 		public void Write( uint _A )
// 		{
// 		}
// 
// 		public void Write( float _A )
// 		{
// 		}
// 
// 		#endregion
// 	}

	/// <summary>
	/// RGBA8 format
	/// </summary>
	[StructLayout( LayoutKind.Sequential )]
	public struct	PF_RGBA8 : IPixelFormat
	{
		public byte	R, G, B, A;

		#region IPixelFormat Members

		public bool	sRGB	{ get { return false; } }

		public void Write( uint _R, uint _G, uint _B, uint _A )
		{
			R = (byte) _R;
			G = (byte) _G;
			B = (byte) _B;
			A = (byte) _A;
		}

		public void Write( float4 _Color )
		{
			R = PF_Empty.ToByte(_Color.x);
			G = PF_Empty.ToByte(_Color.y);
			B = PF_Empty.ToByte(_Color.z);
			A = PF_Empty.ToByte(_Color.w);
		}

		public void Write( float _R, float _G, float _B, float _A )
		{
			R = PF_Empty.ToByte( _R);
			G = PF_Empty.ToByte( _G );
			B = PF_Empty.ToByte( _B );
			A = PF_Empty.ToByte( _A );
		}

		public void Write( uint _A )
		{
			A = (byte) _A;
		}

		public void Write( float _A )
		{
			A = PF_Empty.ToByte( _A );
		}

		public float	Red		{ get { return R / 255.0f; } }
		public float	Green	{ get { return G / 255.0f; } }
		public float	Blue	{ get { return B / 255.0f; } }
		public float	Alpha	{ get { return A / 255.0f; } }

		#endregion
	}

	/// <summary>
	/// RGBA8 sRGB format
	/// </summary>
	[StructLayout( LayoutKind.Sequential )]
	public struct	PF_RGBA8_sRGB : IPixelFormat
	{
		public byte	R, G, B, A;

		#region IPixelFormat Members

		public bool	sRGB	{ get { return true; } }

		public void Write( uint _R, uint _G, uint _B, uint _A )
		{
			R = (byte) _R;
			G = (byte) _G;
			B = (byte) _B;
			A = (byte) _A;
		}

		public void Write( float4 _Color )
		{
			R = PF_Empty.ToByte(_Color.x);
			G = PF_Empty.ToByte(_Color.y);
			B = PF_Empty.ToByte(_Color.z);
			A = PF_Empty.ToByte(_Color.w);
		}

		public void Write( float _R, float _G, float _B, float _A )
		{
			R = PF_Empty.ToByte( _R );
			G = PF_Empty.ToByte( _G );
			B = PF_Empty.ToByte( _B );
			A = PF_Empty.ToByte( _A );
		}

		public void Write( uint _A )
		{
			A = (byte) _A;
		}

		public void Write( float _A )
		{
			A = PF_Empty.ToByte( _A );
		}

		public float	Red		{ get { return R / 255.0f; } }
		public float	Green	{ get { return G / 255.0f; } }
		public float	Blue	{ get { return B / 255.0f; } }
		public float	Alpha	{ get { return A / 255.0f; } }

		#endregion
	}


	/// <summary>
	/// This format is a special encoding of 3 floating point values into 4 byte values, aka "Real Pixels"
	/// The RGB encode the mantissa of each RGB float component while A encodes the exponent by which multiply these 3 mantissae
	/// In fact, we only use a single common exponent that we factor out to 3 different mantissae.
	/// This format was first created by Gregory Ward for his Radiance software (http://www.graphics.cornell.edu/~bjw/rgbe.html)
	///  and allows to store HDR values using standard 8-bits formats.
	/// It's also quite useful to pack some data as we divide the size by 3, from 3 floats (12 bytes) down to only 4 bytes.
	/// </summary>
	/// <remarks>This format only allows storage of POSITIVE floats !</remarks>
	[StructLayout( LayoutKind.Sequential )]
	public struct	PF_RGBE : IPixelFormat
	{
		public byte	B, G, R, E;

		#region IPixelFormat Members

		public bool	sRGB	{ get { return false; } }

		public void Write( uint _R, uint _G, uint _B, uint _E )
		{
			R = (byte) _R;
			G = (byte) _G;
			B = (byte) _B;
			E = (byte) _E;
		}

		// NOTE: Alpha is ignored, RGB is encoded in RGBE
		public void Write( float4 _Color )
		{
			float	fMaxComponent = Math.Max( _Color.x, Math.Max( _Color.y, _Color.z ) );
			if ( fMaxComponent < 1e-16f )
			{	// Too low to encode...
				R = G = B = E = 0;
				return;
			}

			double	CompleteExponent = Math.Log( fMaxComponent ) / Math.Log( 2.0 );
			int		Exponent = (int) Math.Ceiling( CompleteExponent );
			double	Mantissa = fMaxComponent / Math.Pow( 2.0f, Exponent );
			if ( Mantissa == 1.0 )
			{	// Step to next order
				Mantissa = 0.5;
				Exponent++;
			}

			double	Debug0 = Mantissa * Math.Pow( 2.0, Exponent );

			fMaxComponent = (float) Mantissa * 255.99999999f / fMaxComponent;

			R = (byte) (_Color.x * fMaxComponent);
			G = (byte) (_Color.y * fMaxComponent);
			B = (byte) (_Color.z * fMaxComponent);
			E = (byte) (Exponent + 128 );
		}

		public void Write( float _R, float _G, float _B, float _E )
		{
			Write( new float4( _R, _G, _B, _E ) );
		}

		public void Write( uint _E )
		{
			E = (byte) _E;
		}

		public void Write( float _E )
		{
			E = PF_Empty.ToByte( _E );
		}

		public float	Red		{ get { return DecodedColor.x; } }
		public float	Green	{ get { return DecodedColor.y; } }
		public float	Blue	{ get { return DecodedColor.z; } }
		public float	Alpha	{ get { return 1.0f; } }

		#endregion

		public float4	DecodedColor
		{
			get
			{
				double Exponent = Math.Pow( 2.0, E - (128 + 8) );
				return new float4(
									(float) ((R + .5) * Exponent),
									(float) ((G + .5) * Exponent),
									(float) ((B + .5) * Exponent),
									1.0f
									);
			}
		}
	}

	#endregion

	#region 16-Bits Formats

	/// <summary>
	/// R16 format
	/// </summary>
	[StructLayout( LayoutKind.Sequential )]
	public struct	PF_R16 : IPixelFormat
	{
		public ushort	R;

		#region IPixelFormat Members

		public bool	sRGB	{ get { return false; } }

		public void Write( uint _R, uint _G, uint _B, uint _A )
		{
			R = (ushort) _R;
		}

		public void Write( float4 _Color )
		{
			R = PF_Empty.ToUShort(_Color.x);
		}

		public void Write( float _R, float _G, float _B, float _A )
		{
			R = PF_Empty.ToUShort( _R );
		}

		public void Write( uint _A )
		{
		}

		public void Write( float _A )
		{
		}

		public float	Red		{ get { return R / 65535.0f; } }
		public float	Green	{ get { return 0.0f; } }
		public float	Blue	{ get { return 0.0f; } }
		public float	Alpha	{ get { return 1.0f; } }

		#endregion
	}

	/// <summary>
	/// RG16 format
	/// </summary>
	[StructLayout( LayoutKind.Sequential )]
	public struct	PF_RG16 : IPixelFormat
	{
		public ushort	R, G;

		#region IPixelFormat Members

		public bool	sRGB	{ get { return false; } }

		public void Write( uint _R, uint _G, uint _B, uint _A )
		{
			R = (ushort) _R;
			G = (ushort) _G;
		}

		public void Write( float4 _Color )
		{
			R = PF_Empty.ToUShort(_Color.x);
			G = PF_Empty.ToUShort(_Color.y);
		}

		public void Write( float _R, float _G, float _B, float _A )
		{
			R = PF_Empty.ToUShort( _R );
			G = PF_Empty.ToUShort( _G );
		}

		public void Write( uint _A )
		{
		}

		public void Write( float _A )
		{
		}

		public float	Red		{ get { return R / 65535.0f; } }
		public float	Green	{ get { return G / 65535.0f; } }
		public float	Blue	{ get { return 0.0f; } }
		public float	Alpha	{ get { return 1.0f; } }

		#endregion
	}

	/// <summary>
	/// RGBA16 format
	/// </summary>
	[StructLayout( LayoutKind.Sequential )]
	public struct	PF_RGBA16 : IPixelFormat
	{
		public ushort	R, G, B, A;

		#region IPixelFormat Members

		public bool	sRGB	{ get { return false; } }

		public void Write( uint _R, uint _G, uint _B, uint _A )
		{
			R = (ushort) _R;
			G = (ushort) _G;
			B = (ushort) _B;
			A = (ushort) _A;
		}

		public void Write( float4 _Color )
		{
			R = PF_Empty.ToUShort(_Color.x);
			G = PF_Empty.ToUShort(_Color.y);
			B = PF_Empty.ToUShort(_Color.z);
			A = PF_Empty.ToUShort(_Color.w);
		}

		public void Write( float _R, float _G, float _B, float _A )
		{
			R = PF_Empty.ToUShort( _R );
			G = PF_Empty.ToUShort( _G );
			B = PF_Empty.ToUShort( _B );
			A = PF_Empty.ToUShort( _A );
		}

		public void Write( uint _A )
		{
			A = (ushort) _A;
		}

		public void Write( float _A )
		{
			A = PF_Empty.ToUShort( _A );
		}

		public float	Red		{ get { return R / 65535.0f; } }
		public float	Green	{ get { return G / 65535.0f; } }
		public float	Blue	{ get { return B / 65535.0f; } }
		public float	Alpha	{ get { return A / 65535.0f; } }

		#endregion
	}

	#endregion

// 	#region 16F Formats
// 
// 	/// <summary>
// 	/// R16F format
// 	/// </summary>
// 	[StructLayout( LayoutKind.Sequential )]
// 	public struct	PF_R16F : IPixelFormat
// 	{
// 		public Half	R;
// 
// 		#region IPixelFormat Members
// 
// // 		public Format DirectXFormat
// // 		{
// // 			get { return Format.R16_Float; }
// // 		}
// 
// 		public bool	sRGB	{ get { return false; } }
// 
// 		public void Write( uint _R, uint _G, uint _B, uint _A )
// 		{
// 			R = (Half) PF_Empty.ToFloat(_R);
// 		}
// 
// 		public void Write( Vector4D _Color )
// 		{
// 			R = (Half) _Color.X;
// 		}
// 
// 		public void Write( float _R, float _G, float _B, float _A )
// 		{
// 			R = (Half) _R;
// 		}
// 
// 		public void Write( uint _A )
// 		{
// 		}
// 
// 		public void Write( float _A )
// 		{
// 		}
// 
// 		public float	Red		{ get { return R; } }
// 		public float	Green	{ get { return 0.0f; } }
// 		public float	Blue	{ get { return 0.0f; } }
// 		public float	Alpha	{ get { return 1.0f; } }
// 
// 		#endregion
// 	}
// 
// 	/// <summary>
// 	/// RG16F format
// 	/// </summary>
// 	[StructLayout( LayoutKind.Sequential )]
// 	public struct	PF_RG16F : IPixelFormat
// 	{
// 		public Half	R, G;
// 
// 		#region IPixelFormat Members
// 
// 		public Format DirectXFormat
// 		{
// 			get { return Format.R16G16_Float; }
// 		}
// 
// 		public bool	sRGB	{ get { return false; } }
// 
// 		public void Write( uint _R, uint _G, uint _B, uint _A )
// 		{
// 			R = (Half) PF_Empty.ToFloat(_R);
// 			G = (Half) PF_Empty.ToFloat(_G);
// 		}
// 
// 		public void Write( Vector4D _Color )
// 		{
// 			R = (Half) _Color.X;
// 			G = (Half) _Color.Y;
// 		}
// 
// 		public void Write( float _R, float _G, float _B, float _A )
// 		{
// 			R = (Half) _R;
// 			G = (Half) _G;
// 		}
// 
// 		public void Write( uint _A )
// 		{
// 		}
// 
// 		public void Write( float _A )
// 		{
// 		}
// 
// 		public float	Red		{ get { return R; } }
// 		public float	Green	{ get { return G; } }
// 		public float	Blue	{ get { return 0.0f; } }
// 		public float	Alpha	{ get { return 1.0f; } }
// 
// 		#endregion
// 	}
// 
// 	/// <summary>
// 	/// RGBA16F format
// 	/// </summary>
// 	[StructLayout( LayoutKind.Sequential )]
// 	public struct	PF_RGBA16F : IPixelFormat
// 	{
// 		public Half	R, G, B, A;
// 
// 		#region IPixelFormat Members
// 
// 		public Format DirectXFormat
// 		{
// 			get { return Format.R16G16B16A16_Float; }
// 		}
// 
// 		public bool	sRGB	{ get { return false; } }
// 
// 		public void Write( uint _R, uint _G, uint _B, uint _A )
// 		{
// 			R = (Half) PF_Empty.ToFloat(_R);
// 			G = (Half) PF_Empty.ToFloat(_G);
// 			B = (Half) PF_Empty.ToFloat(_B);
// 			A = (Half) PF_Empty.ToFloat(_A);
// 		}
// 
// 		public void Write( Vector4D _Color )
// 		{
// 			R = (Half) _Color.X;
// 			G = (Half) _Color.Y;
// 			B = (Half) _Color.Z;
// 			A = (Half) _Color.W;
// 		}
// 
// 		public void Write( float _R, float _G, float _B, float _A )
// 		{
// 			R = (Half) _R;
// 			G = (Half) _G;
// 			B = (Half) _B;
// 			A = (Half) _A;
// 		}
// 
// 		public void Write( uint _A )
// 		{
// 			A = (Half) PF_Empty.ToFloat( _A );
// 		}
// 
// 		public void Write( float _A )
// 		{
// 			A = (Half) _A;
// 		}
// 
// 		public float	Red		{ get { return R; } }
// 		public float	Green	{ get { return G; } }
// 		public float	Blue	{ get { return B; } }
// 		public float	Alpha	{ get { return A; } }
// 
// 		#endregion
// 	}
// 
// 	#endregion

	#region 32F Formats

	/// <summary>
	/// R32F format
	/// </summary>
	[StructLayout( LayoutKind.Sequential )]
	public struct	PF_R32F : IPixelFormat
	{
		public float	R;

		#region IPixelFormat Members

		public bool	sRGB	{ get { return false; } }

		public void Write( uint _R, uint _G, uint _B, uint _A )
		{
			R = PF_Empty.ToFloat(_R);
		}

		public void Write( float4 _Color )
		{
			R = _Color.x;
		}

		public void Write( float _R, float _G, float _B, float _A )
		{
			R = _R;
		}

		public void Write( uint _A )
		{
		}

		public void Write( float _A )
		{
		}

		public float	Red		{ get { return R; } }
		public float	Green	{ get { return 0.0f; } }
		public float	Blue	{ get { return 0.0f; } }
		public float	Alpha	{ get { return 1.0f; } }

		#endregion
	}

	/// <summary>
	/// RG32F format
	/// </summary>
	[StructLayout( LayoutKind.Sequential )]
	public struct	PF_RG32F : IPixelFormat
	{
		public float	R, G;

		#region IPixelFormat Members

		public bool	sRGB	{ get { return false; } }

		public void Write( uint _R, uint _G, uint _B, uint _A )
		{
			R = PF_Empty.ToFloat(_R);
			G = PF_Empty.ToFloat(_G);
		}

		public void Write( float4 _Color )
		{
			R = _Color.x;
			G = _Color.y;
		}

		public void Write( float _R, float _G, float _B, float _A )
		{
			R = _R;
			G = _G;
		}

		public void Write( uint _A )
		{
		}

		public void Write( float _A )
		{
		}

		public float	Red		{ get { return R; } }
		public float	Green	{ get { return G; } }
		public float	Blue	{ get { return 0.0f; } }
		public float	Alpha	{ get { return 1.0f; } }

		#endregion
	}

	/// <summary>
	/// RGB32F format
	/// </summary>
	[StructLayout( LayoutKind.Sequential )]
	public struct	PF_RGB32F : IPixelFormat
	{
		public float	R, G, B;

		#region IPixelFormat Members

		public bool	sRGB	{ get { return false; } }

		public void Write( uint _R, uint _G, uint _B, uint _A )
		{
			R = PF_Empty.ToFloat(_R);
			G = PF_Empty.ToFloat(_G);
			B = PF_Empty.ToFloat(_B);
		}

		public void Write( float4 _Color )
		{
			R = _Color.x;
			G = _Color.y;
			B = _Color.z;
		}

		public void Write( float _R, float _G, float _B, float _A )
		{
			R = _R;
			G = _G;
			B = _B;
		}

		public void Write( uint _A )
		{
		}

		public void Write( float _A )
		{
		}

		public float	Red		{ get { return R; } }
		public float	Green	{ get { return G; } }
		public float	Blue	{ get { return B; } }
		public float	Alpha	{ get { return 1.0f; } }

		#endregion
	}

	/// <summary>
	/// RGBA32F format
	/// </summary>
	[StructLayout( LayoutKind.Sequential )]
	public struct	PF_RGBA32F : IPixelFormat
	{
		public float	R, G, B, A;

		#region IPixelFormat Members

		public bool	sRGB	{ get { return false; } }

		public void Write( uint _R, uint _G, uint _B, uint _A )
		{
			R = PF_Empty.ToFloat(_R);
			G = PF_Empty.ToFloat(_G);
			B = PF_Empty.ToFloat(_B);
			A = PF_Empty.ToFloat(_A);
		}

		public void Write( float4 _Color )
		{
			R = _Color.x;
			G = _Color.y;
			B = _Color.z;
			A = _Color.w;
		}

		public void Write( float _R, float _G, float _B, float _A )
		{
			R = _R;
			G = _G;
			B = _B;
			A = _A;
		}

		public void Write( uint _A )
		{
			A = PF_Empty.ToFloat(_A);
		}

		public void Write( float _A )
		{
			A = _A;
		}

		public float	Red		{ get { return R; } }
		public float	Green	{ get { return G; } }
		public float	Blue	{ get { return B; } }
		public float	Alpha	{ get { return A; } }

		#endregion
	}

	#endregion

	#region Depth Formats

// 	/// <summary>
// 	/// D16 format
// 	/// </summary>
// 	[StructLayout( LayoutKind.Sequential )]
// 	public struct	PF_D16 : IDepthFormat
// 	{
// 		public Half	D;
// 
// 		#region IPixelFormat Members
// 
// 		public Format DirectXFormat
// 		{
// 			get { return Format.D16_UNorm; }
// 		}
// 
// 		public bool	sRGB	{ get { return false; } }
// 
// 		public void Write( uint _R, uint _G, uint _B, uint _A )
// 		{
// 			D = (Half) PF_Empty.ToFloat(_R);
// 		}
// 
// 		public void Write( Vector4D _Color )
// 		{
// 			D = (Half) _Color.X;
// 		}
// 
// 		public void Write( float _R, float _G, float _B, float _A )
// 		{
// 			D = (Half) _R;
// 		}
// 
// 		public void Write( uint _A )
// 		{
// 		}
// 
// 		public void Write( float _A )
// 		{
// 		}
// 
// 		public float	Red		{ get { return D; } }
// 		public float	Green	{ get { return 0.0f; } }
// 		public float	Blue	{ get { return 0.0f; } }
// 		public float	Alpha	{ get { return 1.0f; } }
// 
// 		#endregion
// 	}

	/// <summary>
	/// D32 format
	/// </summary>
	[StructLayout( LayoutKind.Sequential )]
	public struct	PF_D32 : IDepthFormat
	{
		public float	D;

		#region IPixelFormat Members

		public bool	sRGB	{ get { return false; } }

		public void Write( uint _R, uint _G, uint _B, uint _A )
		{
			D = PF_Empty.ToFloat(_R);
		}

		public void Write( float4 _Color )
		{
			D = _Color.x;
		}

		public void Write( float _R, float _G, float _B, float _A )
		{
			D = _R;
		}

		public void Write( uint _A )
		{
		}

		public void Write( float _A )
		{
		}

		public float	Red		{ get { return D; } }
		public float	Green	{ get { return 0.0f; } }
		public float	Blue	{ get { return 0.0f; } }
		public float	Alpha	{ get { return 1.0f; } }

		#endregion
	}
	
	/// <summary>
	/// D24S8 format (24 bits depth + 8 bits stencil)
	/// NOTE: This format is NOT readable and will throw an exception if used for a readable depth stencil !
	/// </summary>
	[StructLayout( LayoutKind.Sequential )]
	public struct	PF_D24S8 : IDepthFormat
	{
		public byte	R, G, B, A;

		#region IPixelFormat Members

		public bool	sRGB	{ get { return false; } }

		public void Write( uint _R, uint _G, uint _B, uint _A )
		{
			R = (byte) _R;
			G = (byte) _G;
			B = (byte) _B;
			A = (byte) _A;
		}

		public void Write( float4 _Color )
		{
			R = PF_Empty.ToByte(_Color.x);
			G = PF_Empty.ToByte(_Color.y);
			B = PF_Empty.ToByte(_Color.z);
			A = PF_Empty.ToByte(_Color.w);
		}

		public void Write( uint _A )
		{
			A = (byte) _A;
		}

		public void Write( float _A )
		{
			A = PF_Empty.ToByte( _A );
		}

		public void Write( float _R, float _G, float _B, float _A )
		{
			A = PF_Empty.ToByte( _A );
		}

		public float	Red		{ get { return R / 255.0f; } }
		public float	Green	{ get { return G / 255.0f; } }
		public float	Blue	{ get { return B / 255.0f; } }
		public float	Alpha	{ get { return A / 255.0f; } }

		#endregion
	}

	#endregion
}
