using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using RendererManaged;

namespace MaterialsOptimizer
{
	public class DiffuseGlossTexture {

		public FileInfo			m_diffuseFileName = null;
		public FileInfo			m_glossFileName = null;

		// Generated texture data
		public DateTime			m_diffuseTimeAtGeneration;
		public DateTime			m_glossTimeAtGeneration;
		public FileInfo			m_optimizedDiffuseGlossFileName = null;

		// Resolved on load or provided on construction
		public TextureFileInfo	m_diffuse = null;
		public TextureFileInfo	m_gloss = null;
		public TextureFileInfo	m_optimizedDiffuseGloss = null;

		/// <summary>
		/// Checks file time stamps against last generated texture time stamps to know if the (diffuse+gloss) file should be re-generated
		/// </summary>
		public bool		NeedsToBeGenerated {
			get {
				if ( m_optimizedDiffuseGlossFileName != null ) {
					m_optimizedDiffuseGlossFileName.Refresh();
					if ( !m_optimizedDiffuseGlossFileName.Exists )
						return true;
				}

				TimeSpan	deltaTimeDiffuse = m_diffuseFileName.LastWriteTime - m_diffuseTimeAtGeneration;
				if ( deltaTimeDiffuse.TotalSeconds > 30 )
					return true;	// Diffuse file was modified since last generation...

				if ( m_glossFileName == null )
					return false;	// No gloss file to check

				TimeSpan	deltaTimeGloss = m_glossFileName.LastWriteTime - m_glossTimeAtGeneration;
				return deltaTimeGloss.TotalSeconds > 30;
			}
		}

		public DiffuseGlossTexture( TextureFileInfo _diffuse, TextureFileInfo _gloss ) {
			m_diffuseFileName = _diffuse.m_fileName;
			m_glossFileName = _gloss != null ? _gloss.m_fileName : null;
			m_diffuse = _diffuse;
			m_gloss = _gloss;
		}

		public DiffuseGlossTexture( BinaryReader R ) {
			Read( R );
		}

		public void		Write( BinaryWriter W ) {
			W.Write( m_diffuseFileName.FullName);
			W.Write( m_glossFileName != null ? m_glossFileName.FullName : "" );

			W.Write( m_diffuseTimeAtGeneration.ToBinary() );
			W.Write( m_glossTimeAtGeneration.ToBinary() );
			W.Write( m_optimizedDiffuseGlossFileName != null ? m_optimizedDiffuseGlossFileName.FullName : "" );
		}

		public void		Read( BinaryReader R ) {
			m_diffuseFileName = new FileInfo( R.ReadString() );

			string	glossTextureFileName = R.ReadString();
			m_glossFileName = glossTextureFileName != "" ? new FileInfo( glossTextureFileName ) : null;

			m_diffuseTimeAtGeneration = DateTime.FromBinary( R.ReadInt64() );
			m_glossTimeAtGeneration = DateTime.FromBinary( R.ReadInt64() );

			string	diffuseGlossTextureName = R.ReadString();
			m_optimizedDiffuseGlossFileName = diffuseGlossTextureName != "" ? new FileInfo( diffuseGlossTextureName ) : null;
		}

		/// <summary>
		/// Generates a (diffuse+gloss) texture from 2 distinct textures
		/// </summary>
		public void GenerateDiffuseGlossTexture() {
			if ( m_diffuse == null )
				throw new Exception( "Invalid source diffuse image file!" );
			if ( !m_diffuse.m_fileName.Exists )
				throw new Exception( "Source diffuse image file \"" + m_diffuse.m_fileName.FullName + "\" does not exist on disk!" );

			string		targetFileNameString = TextureFileInfo.GetOptimizedDiffuseGlossNameFromDiffuseName( m_diffuse.m_fileName.FullName );
			FileInfo	targetFileName = new FileInfo( targetFileNameString );

			using ( ImageUtility.Bitmap diffuse = new ImageUtility.Bitmap( m_diffuse.m_fileName ) ) {

				int	W = diffuse.Width;
				int	H = diffuse.Height;

				ImageUtility.Bitmap	gloss = null;
				if ( m_gloss != null ) {
					gloss = new ImageUtility.Bitmap( m_gloss.m_fileName );

// 					int	gW = gloss.Width;
// 					int	gH = gloss.Height;
// 					for ( int Y=0; Y < gH; Y++ )
// 						for ( int X=0; X < gW; X++ ) {
// 							gloss.ContentXYZ[X,Y].x = ImageUtility.ColorProfile.sRGB2Linear( gloss.ContentXYZ[X,Y].x );
// 						}
				}
				bool	needsScale = false;
				if ( gloss != null && (gloss.Width != W || gloss.Height != H) ) {
					needsScale = true;
				}

				if ( gloss != null ) {
					if ( needsScale ) {
						// Set gloss as alpha with re-scaling
						float	scaleX = (float) gloss.Width / W;
						float	scaleY = (float) gloss.Height / H;
						for ( int Y=0; Y < H; Y++ ) {
							float	Y2 = scaleY * Y;
							for ( int X=0; X < W; X++ )
								diffuse.ContentXYZ[X,Y].w = gloss.BilinearSample( scaleX * X, Y2 ).x;
						}
					} else {
						// Set gloss as alpha without re-scaling
						for ( int Y=0; Y < H; Y++ )
							for ( int X=0; X < W; X++ ) {
//								diffuse.ContentXYZ[X,Y].w = gloss.ContentXYZ[X,Y].x;

								diffuse.ContentXYZ[X,Y].w = 0.3f * gloss.ContentXYZ[X,Y].x
														  + 0.5f * gloss.ContentXYZ[X,Y].y
														  + 0.2f * gloss.ContentXYZ[X,Y].z;
							}
					}
				} else {
					for ( int Y=0; Y < H; Y++ )
						for ( int X=0; X < W; X++ )
							diffuse.ContentXYZ[X,Y].w = 1.0f;
				}

				// Save diffuse as target
				diffuse.HasAlpha = gloss != null;
				diffuse.Save( targetFileName );
			}

			// Save optimized filename + timestamps of source files
			m_diffuseFileName.Refresh();
			if ( m_glossFileName != null )
				m_glossFileName.Refresh();

			m_optimizedDiffuseGlossFileName = targetFileName;
			m_diffuseTimeAtGeneration = m_diffuseFileName.LastWriteTime;
			m_glossTimeAtGeneration = m_glossFileName != null ? m_glossFileName.LastWriteTime : DateTime.MinValue;
		}
	}
}
