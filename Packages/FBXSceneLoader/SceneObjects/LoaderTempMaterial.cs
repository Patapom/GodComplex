using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using WMath;

namespace FBX.SceneLoader.Objects
{
	public class	Material : LoaderTempSceneObject
	{
		#region FIELDS

		protected Dictionary<string,LoaderTempTexture>	m_TexturesDiffuse = new Dictionary<string,LoaderTempTexture>();
		protected Dictionary<string,LoaderTempTexture>	m_TexturesNormal = new Dictionary<string,LoaderTempTexture>();
		protected Dictionary<string,LoaderTempTexture>	m_TexturesRegular = new Dictionary<string,LoaderTempTexture>();

		#endregion

		#region PROPERTIES

		public LoaderTempTexture[]	DiffuseTextures
		{
			get
			{
				LoaderTempTexture[]	Result = new LoaderTempTexture[m_TexturesDiffuse.Count];
				m_TexturesDiffuse.Values.CopyTo( Result, 0 );

				return	Result;
			}
		}

		public LoaderTempTexture[]	NormalTextures
		{
			get
			{
				LoaderTempTexture[]	Result = new LoaderTempTexture[m_TexturesNormal.Count];
				m_TexturesNormal.Values.CopyTo( Result, 0 );

				return	Result;
			}
		}

		public LoaderTempTexture[]	RegularTextures
		{
			get
			{
				LoaderTempTexture[]	Result = new LoaderTempTexture[m_TexturesRegular.Count];
				m_TexturesRegular.Values.CopyTo( Result, 0 );

				return	Result;
			}
		}

		#endregion

		#region METHODS

		public Material( SceneLoader _Owner, string _Name ) : base( _Owner, _Name )
		{
		}

		/// <summary>
		/// Adds a diffuse texture to the material
		/// </summary>
		/// <param name="_TextureName">The name of the texture to add</param>
		/// <param name="_Texture">The texture to add</param>
		public void		AddTextureDiffuse( LoaderTempTexture _Texture )
		{
			if ( m_TexturesDiffuse.ContainsKey( _Texture.Name ) )
				throw new Exception( "Material \"" + m_Name + "\" already contains a DIFFUSE texture named \"" + _Texture.Name + "\"!" );

			_Texture.GenerateMipMaps = m_Owner.GenerateMipMapsDiffuse;
			m_TexturesDiffuse[_Texture.Name] = _Texture;
		}

		/// <summary>
		/// Adds a normal texture to the material
		/// </summary>
		/// <param name="_TextureName">The name of the texture to add</param>
		/// <param name="_Texture">The texture to add</param>
		public void		AddTextureNormal( LoaderTempTexture _Texture )
		{
			if ( m_TexturesNormal.ContainsKey( _Texture.Name ) )
				throw new Exception( "Material \"" + m_Name + "\" already contains a NORMAL texture named \"" + _Texture.Name + "\"!" );

			_Texture.GenerateMipMaps = m_Owner.GenerateMipMapsNormal;
			m_TexturesNormal[_Texture.Name] = _Texture;
		}

		/// <summary>
		/// Adds a generic texture to the material
		/// </summary>
		/// <param name="_TextureName">The name of the texture to add</param>
		/// <param name="_Texture">The texture to add</param>
		public void		AddTexture( LoaderTempTexture _Texture )
		{
			if ( m_TexturesRegular.ContainsKey( _Texture.Name ) )
				throw new Exception( "Material \"" + m_Name + "\" already contains a GENERIC texture named \"" + _Texture.Name + "\"!" );

			_Texture.GenerateMipMaps = m_Owner.GenerateMipMapsRegular;
			m_TexturesRegular[_Texture.Name] = _Texture;
		}

		#endregion
	};
}
