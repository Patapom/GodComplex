using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using WMath;

namespace FBX.SceneLoader.Objects
{
	public class	LoaderTempTexture : LoaderTempSceneObject
	{
		#region NESTED TYPES

		protected enum	WRAP_MODE
		{
			WRAP,
			MIRROR,
			CLAMP,
			BORDER,
		}

		protected enum	FILTER_TYPE
		{
			NONE,
			POINT,
			LINEAR,
			ANISOTROPIC,
		}

		#endregion

		#region FIELDS

		protected string						m_SamplerName = null;
		protected FBXImporter.Texture			m_SourceTexture = null;
		protected bool							m_bEmbed = false;
		protected bool							m_bGenerateMipMaps = false;

		protected Dictionary<string,string>		m_SamplerParams = new Dictionary<string,string>();

		#endregion

		#region PROPERTIES

		/// <summary>
		/// Gets or sets the "Embed" flag telling if the texture should be embedded in the scene archive
		/// </summary>
		public bool		Embed
		{
			get { return m_bEmbed; }
			set { m_bEmbed = value; }
		}

		/// <summary>
		/// Gets or sets the "GenerateMipMaps" flag telling if the texture should generate multiple mip levels
		/// </summary>
		public bool		GenerateMipMaps
		{
			get { return m_bGenerateMipMaps; }
			set { m_bGenerateMipMaps = value; }
		}

		/// <summary>
		/// Gets or sets the name of the sampler that will reference that texture
		/// </summary>
		/// <remarks>
		/// If unspecified, the name of the sampler will be the name of the texture with "Sampler" appended
		/// </remarks>
		public string	SamplerName
		{
			get { return m_SamplerName != null ? m_SamplerName : (m_SourceTexture.Name + "Sampler"); }
			set { m_SamplerName = value; }
		}

		/// <summary>
		/// Gets the absolute name of the texture file
		/// </summary>
		public string	TextureFileName	{ get { return m_SourceTexture.AbsoluteFileName; } }

		/// <summary>
		/// Gets the dictionary of sampler params
		/// This will be serialized as "params" for a TextureSampler object in the JSON file
		/// </summary>
		public Dictionary<string,string>	SamplerParams
		{
			get { return m_SamplerParams; }
		}

		#endregion

		#region METHODS

		public LoaderTempTexture( SceneLoader _Owner, string _Name ) : base( _Owner, _Name )
		{
		}

		/// <summary>
		/// Sets a sampler param
		/// </summary>
		/// <param name="_Name">The name of the param to set</param>
		/// <param name="_Value">The value of the param (null clears the param)</param>
		public void		SetSamplerParam( string _Name, string _Value )
		{
			if ( _Value == null && m_SamplerParams.ContainsKey( _Name ) )
				m_SamplerParams.Remove( _Name );
			else
				m_SamplerParams[_Name] = _Value;
		}

		/// <summary>
		/// Sets the source FBX texture to get the parameters from
		/// </summary>
		/// <remarks>You can override individual parameters using the "SetSamplerParam()" function,
		///  if a parameter exists in the parameters table, it will be used instead
		///  of the one that would be generated from the texture
		/// </remarks>
		/// <param name="_Texture"></param>
		public void		SetSourceFBXTexture( FBXImporter.Texture _Texture )
		{
			m_SourceTexture = _Texture;

// 			// Build default sampler params
// 			SetSamplerParam( "o3d.addressModeU", Helpers.FormatParamObject( ConvertWrapMode( _Texture.WrapModeU ) ) );
// 			SetSamplerParam( "o3d.addressModeV", Helpers.FormatParamObject( ConvertWrapMode( _Texture.WrapModeV ) ) );
// 			SetSamplerParam( "o3d.borderColor", Helpers.FormatParamObject( new Vector4D( 0, 0, 0, 0 ) ) );
// 			SetSamplerParam( "o3d.magFilter", Helpers.FormatParamObject( (int) FILTER_TYPE.LINEAR ) );
// 			SetSamplerParam( "o3d.minFilter", Helpers.FormatParamObject( (int) FILTER_TYPE.LINEAR ) );
// 			SetSamplerParam( "o3d.mipFilter", Helpers.FormatParamObject( (int) FILTER_TYPE.LINEAR ) );
// 			SetSamplerParam( "o3d.maxAnisotropy", Helpers.FormatParamObject( 1 ) );
		}


		/// <summary>
		/// Converts an FBX wrap mode into an O3D wrap mode
		/// </summary>
		/// <param name="_WrapMode"></param>
		/// <returns></returns>
		protected int	ConvertWrapMode( FBXImporter.Texture.WRAP_MODE _WrapMode )
		{
			switch ( _WrapMode )
			{
				case FBXImporter.Texture.WRAP_MODE.CLAMP:
					return	(int) WRAP_MODE.CLAMP;

				case FBXImporter.Texture.WRAP_MODE.REPEAT:
					return	(int) WRAP_MODE.WRAP;
			}

			return	(int) WRAP_MODE.WRAP;
		}

		#endregion
	};
}
