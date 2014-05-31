using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// using SharpDX;
// using SharpDX.Direct3D10;
// using SharpDX.DXGI;
using WMath;

namespace StandardizedDiffuseAlbedoMaps
{
	/// <summary>
	/// This is the interface to pixel format structures needed for images and textures
	/// </summary>
	public interface IPixelFormat
	{
		/// <summary>
		/// Gets the equivalent DirectX format this interface is covering
		/// </summary>
//TODO!		Format	DirectXFormat	{ get; }

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
		void	Write( Vector4D _Color );
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
		/// <summary>
		/// Gets the equivalent DirectX format that should be used to create the depth stencil resource so it can later be bound to a shader
		/// </summary>
//		Format	ReadableDirectXFormat		{ get; }

		/// <summary>
		/// Gets the equivalent DirectX format that should be used to make the depth stencil buffer readable by a shader
		/// </summary>
//		Format	ShaderResourceDirectXFormat	{ get; }
	}
}
