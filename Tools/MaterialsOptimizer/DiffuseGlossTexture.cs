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
		/// Gives the name of the (diffuse+gloss) texture that will be generated
		/// </summary>
		/// <returns></returns>
		public FileInfo	GetDiffuseGlossTextureFileName() {
			if ( m_diffuse == null )
				throw new Exception( "Invalid source diffuse image file!" );
			if ( !m_diffuse.m_fileName.Exists )
				throw new Exception( "Source diffuse image file \"" + m_diffuse.m_fileName.FullName + "\" does not exist on disk!" );

			string		targetFileNameString = TextureFileInfo.GetOptimizedDiffuseGlossNameFromDiffuseName( m_diffuse.m_fileName.FullName );
			FileInfo	targetFileName = new FileInfo( targetFileNameString );

			return targetFileName;
		}

		/// <summary>
		/// Generates a (diffuse+gloss) texture from 2 distinct textures
		/// </summary>
		public void GenerateDiffuseGlossTexture() {
			if ( m_diffuse == null )
				throw new Exception( "Invalid source diffuse image file!" );
			if ( !m_diffuse.m_fileName.Exists )
				throw new Exception( "Source diffuse image file \"" + m_diffuse.m_fileName.FullName + "\" does not exist on disk!" );

			FileInfo	targetFileName = GetDiffuseGlossTextureFileName();

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
						int			W2 = gloss.Width;
						int			H2 = gloss.Height;
						float[,]	source = new float[W2,H2];
						for ( int Y=0; Y < H2; Y++ )
							for ( int X=0; X < W2; X++ )
								source[X,Y] = 0.3f * gloss.ContentXYZ[X,Y].x
											+ 0.5f * gloss.ContentXYZ[X,Y].y
											+ 0.2f * gloss.ContentXYZ[X,Y].z;

						// Downscale first
						while ( W2 > W ) {
							int			halfW2 = W2 >> 1;
							float[,]	temp = new float[halfW2,H2];
							for ( int Y=0; Y < H2; Y++ )
								for ( int X=0; X < halfW2; X++ )
									temp[X,Y] = 0.5f * (source[2*X+0,Y] + source[2*X+1,Y]);

							source = temp;
							W2 = halfW2;
						}
						while ( H2 > H ) {
							int			halfH2 = H2 >> 1;
							float[,]	temp = new float[W2,halfH2];
							for ( int Y=0; Y < halfH2; Y++ )
								for ( int X=0; X < W2; X++ )
									temp[X,Y] = 0.5f * (source[X,2*Y+0] + source[X,2*Y+1]);

							source = temp;
							H2 = halfH2;
						}

						// Upscale then
						while ( W2 < W ) {
							int			doubleW2 = W2 << 1;
							float[,]	temp = new float[doubleW2,H2];
							for ( int Y=0; Y < H2; Y++ )
								for ( int X=0; X < W2; X++ ) {
									temp[2*X+0,Y] = source[X,Y];
									temp[2*X+1,Y] = 0.5f * (source[X,Y] + source[Math.Min( W2-1, X+1 ),Y]);
								}

							source = temp;
							W2 = doubleW2;
						}
						while ( H2 < H ) {
							int			doubleH2 = H2 << 1;
							float[,]	temp = new float[W2,doubleH2];
							for ( int Y=0; Y < H2; Y++ )
								for ( int X=0; X < W2; X++ ) {
									temp[X,2*Y+0] = source[X,Y];
									temp[X,2*Y+1] = 0.5f * (source[X,Y] + source[X,Math.Min( H2-1, Y+1 )]);
								}

							source = temp;
							H2 = doubleH2;
						}

						for ( int Y=0; Y < H; Y++ )
							for ( int X=0; X < W; X++ )
								diffuse.ContentXYZ[X,Y].w = source[X,Y];

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
