using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using ShaderInterpreter.ShaderMath;

namespace ShaderInterpreter.Textures
{
	public class	SamplerState
	{	
	}

	public class	Texture2D<T> where T:class,new()
	{
		public T	Sample( SamplerState _sampler, float2 _uv )
		{
			return new T();
		}

		public T	SampleLevel( SamplerState _sampler, float2 _uv, double _Mip )
		{
			return new T();
		}

		public Texture2D()
		{

		}
	}

	public class	Texture2DArray<T> where T:class,new()
	{
		public T	Sample( SamplerState _sampler, float3 _uvw )
		{
			return new T();
		}

		public T	SampleLevel( SamplerState _sampler, float3 _uvw, double _Mip )
		{
			return new T();
		}

		public Texture2DArray()
		{

		}
	}

	public class	TextureCube<T> where T:class,new()
	{
		public T	Sample( SamplerState _sampler, float3 _uvw )
		{
			return new T();
		}

		public T	SampleLevel( SamplerState _sampler, float3 _uvw, double _Mip )
		{
			return new T();
		}

		public TextureCube()
		{

		}
	}

	public class	Texture3D<T> where T:class,new()
	{
		public T	Sample( SamplerState _sampler, float3 _uvw )
		{
			return new T();
		}

		public T	SampleLevel( SamplerState _sampler, float3 _uvw, double _Mip )
		{
			return new T();
		}

		public Texture3D()
		{

		}
	}
}
