using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using SharpMath;
using Renderer;
using ImageUtility;

namespace ObjSceneUtility
{
    public class SceneObj : IDisposable {

		#region NESTED TYPES

		/// <summary>
		/// Called for each object that is about to be rendered
		/// </summary>
		/// <param name="_mesh"></param>
		/// <param name="_material"></param>
		public delegate	bool	ObjectAndMaterialSetupDelegate( Mesh.Surface _surface );

		[System.Diagnostics.DebuggerDisplay( "{m_name} D={m_textureNameDiffuse} N={m_textureNameNormal}" )]
		public class	Material : IDisposable {

			#region NESTED TYPES

			enum	TEXTURE_TYPE {
				DIFFUSE,
				NORMAL,
				ROUGHNESS,
				METAL,
				HEIGHT,
				EMISSIVE,
			}

			#endregion

			#region FIELDS

			private SceneObj	m_owner;

			public string		m_name;
			public float3		m_colorAmbient;
			public float3		m_colorDiffuse;
			public float3		m_colorSpecular;
			public float3		m_colorEmissive;
			public float		m_exponentSpecular;
			public float		m_alpha = 1.0f;

			public string		m_textureNameDiffuse;
			public string		m_textureNameAlpha;
			public string		m_textureNameBump;
			public string		m_textureNameNormal;
			public string		m_textureNameSpecular;
			public string		m_textureNameEmissive;

			// Custom fields
			public float		m_bumpHeight = 100.0f;

			public Texture2D	m_textureDiffuse = null;
			public Texture2D	m_textureNormal = null;
			public Texture2D	m_textureSpecular = null;
			public Texture2D	m_textureEmissive = null;

			public static bool	ms_bumpIsNormalMap = false;			// If true then assume bump maps are normal maps, otherwise we need to convert them on the fly unless a texture with the same name but with an "_n" suffix exists.
			public static bool	ms_saveConvertedNormalMaps = true;	// If true then converted bump maps are saved as normal maps in the same directory as the bump map, with an "_n" suffix.
			public static bool	ms_loadTexturesAsDDS = true;		// If true then textures are loaded from DDS files if they exist
			public static bool	ms_saveTexturesAsDDS = true;		// If true then textures are saved as DDS files after conversion
			public static bool	ms_diffuseIssRGB = true;			// If true then diffuse/specular textures are treaded as sRGB
			public static bool	ms_fastLoadDefaultColor = false;	// If true then created default color textures instead of loading actual files

			#endregion

			#region METHODS

			public	Material( SceneObj _owner ) {
				m_owner = _owner;
			}

			public	Material( SceneObj _owner, BinaryReader _R ) {
				m_owner = _owner;
				Read( _R );
			}

			public void		CreateTextures( Device _device ) {
				m_textureDiffuse = CreateOrUseTexture( _device, m_textureNameDiffuse, m_colorDiffuse, TEXTURE_TYPE.DIFFUSE );
				m_textureSpecular = CreateOrUseTexture( _device, m_textureNameSpecular, m_colorSpecular, TEXTURE_TYPE.DIFFUSE );
				m_textureNormal = CreateOrUseTexture( _device, m_textureNameNormal == null && ms_bumpIsNormalMap ? m_textureNameBump : m_textureNameNormal, new float3( 0.5f, 0.5f, 1.0f ), TEXTURE_TYPE.NORMAL );
				m_textureEmissive = CreateOrUseTexture( _device, m_textureNameEmissive, m_colorEmissive, TEXTURE_TYPE.EMISSIVE );
			}

			Texture2D	CreateOrUseTexture( Device _device, string  _textureName, float3 _defaultColor, TEXTURE_TYPE _textureType ) {
				bool	isDefaultColor = false;
				if ( _textureName == null || ms_fastLoadDefaultColor ) {
					_textureName = _defaultColor.ToString();
					isDefaultColor = true;
				}
				_textureName = _textureName.ToLower();
				if ( m_owner.m_textureName2Texture.ContainsKey( _textureName ) )
					return m_owner.m_textureName2Texture[_textureName];	// Return existing texture

				Texture2D	texture = null;
				if ( !isDefaultColor ) {
					// Load the texture and register it
					FileInfo	imageFileName = new FileInfo( Path.Combine( m_owner.m_baseDirectory.FullName, _textureName ) );
					if ( !imageFileName.Exists )
						throw new Exception( "Image file \"" + imageFileName.FullName + "\" not found!" );

					// Check if we have a DDS file for this
					FileInfo	imageDDSFileName = new FileInfo( Path.Combine( imageFileName.DirectoryName, Path.GetFileNameWithoutExtension( imageFileName.FullName ) + ".dds" ) );
					if ( ms_loadTexturesAsDDS && imageDDSFileName.Exists ) {
						using ( ImagesMatrix images = new ImagesMatrix() ) {
							COMPONENT_FORMAT	loadedFormat = images.DDSLoadFile( imageDDSFileName );
							texture = new Renderer.Texture2D( _device, images, loadedFormat );
						}
						return texture;
					}

					// Read image's content
					ImageFile	image = new ImageFile( imageFileName );

 					// Build mips & create texture
					COMPONENT_FORMAT	resultFormat = COMPONENT_FORMAT.AUTO;
					ImagesMatrix mips = BuildMips( image, _textureType, out resultFormat );
					texture = new Renderer.Texture2D( _device, mips, resultFormat );

					// Save as DDS for faster loading next time
					if ( ms_saveTexturesAsDDS ) {
						mips.DDSSaveFile( imageDDSFileName, texture.ComponentFormat );
					}

					mips.Dispose();

				} else {
					// Create a dummy 16x16 texture with the default color
					if ( _textureType == TEXTURE_TYPE.NORMAL ) {
						Renderer.PixelsBuffer	sourceNormalMap = new  Renderer.PixelsBuffer( 16 * 16 *4 );
						using ( System.IO.BinaryWriter Wr = sourceNormalMap.OpenStreamWrite() )
							for ( int Y=0; Y < 16; Y++ ) {
								for ( int X=0; X < 16; X++ ) {
									Wr.Write( (sbyte) Mathf.Clamp( 256.0f * _defaultColor.x - 128.0f, -128, 127 ) );
									Wr.Write( (sbyte) Mathf.Clamp( 256.0f * _defaultColor.y - 128.0f, -128, 127 ) );
									Wr.Write( (sbyte) Mathf.Clamp( 256.0f * _defaultColor.z - 128.0f, -128, 127 ) );
									Wr.Write( (byte) 255 );
								}
							}
						texture = new Renderer.Texture2D( _device, 16, 16, 1, 1, ImageUtility.PIXEL_FORMAT.RGBA8, ImageUtility.COMPONENT_FORMAT.SNORM, false, false, new Renderer.PixelsBuffer[] { sourceNormalMap } );
						sourceNormalMap.Dispose();
					} else {
						ImageFile	image = new ImageFile( 16, 16, PIXEL_FORMAT.RGBA8, new ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.sRGB ) );
						image.WritePixels( ( uint _X, uint _Y, ref float4 _color ) => { _color.Set( _defaultColor, 1.0f ); } );
						texture = new Texture2D( _device, new ImagesMatrix( new ImageFile[,] {{image}} ), COMPONENT_FORMAT.UNORM );
						image.Dispose();
					}
				}

				// Register texture
				texture.Tag = _textureName;
				m_owner.m_textureName2Texture[_textureName] = texture;

				return texture;
			}

			ImagesMatrix	BuildMips( ImageFile _mip0, TEXTURE_TYPE _type, out COMPONENT_FORMAT _componentFormat ) {
				PIXEL_FORMAT			targetFormat = PIXEL_FORMAT.RGBA8;
				ImagesMatrix.IMAGE_TYPE	mipType = ImagesMatrix.IMAGE_TYPE.LINEAR;
				_componentFormat = COMPONENT_FORMAT.UNORM;

				switch ( _type ) {
					case TEXTURE_TYPE.DIFFUSE:
					case TEXTURE_TYPE.EMISSIVE:
						_componentFormat = ms_diffuseIssRGB ? COMPONENT_FORMAT.UNORM_sRGB : COMPONENT_FORMAT.UNORM;
						mipType = ms_diffuseIssRGB ? ImagesMatrix.IMAGE_TYPE.sRGB : ImagesMatrix.IMAGE_TYPE.LINEAR;
						break;

					case TEXTURE_TYPE.NORMAL:
						_componentFormat = COMPONENT_FORMAT.SNORM;
						mipType = ImagesMatrix.IMAGE_TYPE.NORMAL_MAP;
						break;

					case TEXTURE_TYPE.ROUGHNESS:
					case TEXTURE_TYPE.METAL:
					case TEXTURE_TYPE.HEIGHT:
						targetFormat = PIXEL_FORMAT.R8;
						break;

					default:
						throw new Exception( "Unsupported type!" );
				}

				if ( targetFormat != _mip0.PixelFormat ) {
					// Convert to target format
					ImageFile	correctMip = new ImageFile();
					correctMip.ConvertFrom( _mip0, targetFormat );
					_mip0.Dispose();
					_mip0 = correctMip;
				}

				// Build actual mips
				ImagesMatrix	mips = new ImagesMatrix();
				mips.InitTexture2DArray( _mip0.Width, _mip0.Height, 1, 0 );
				mips[0][0][0] = _mip0;
				mips.AllocateImageFiles( _mip0.PixelFormat, new ColorProfile( ImageUtility.ColorProfile.STANDARD_PROFILE.LINEAR ) );
				mips.BuildMips( mipType );
				if ( _componentFormat == COMPONENT_FORMAT.SNORM ) {
					mips.MakeSigned();
				}

				return mips;
			}

/*			Renderer.PixelsBuffer[]	BuildMips( float4[,] _mip0, bool _isNormalMap, bool _sRGB ) {
				uint	W = (uint) _mip0.GetLength( 0 );
				uint	H = (uint) _mip0.GetLength( 1 );
				uint	mipsCount = 1 + (uint) Mathf.Ceiling( Mathf.Log( Math.Max( W, H ) ) / Mathf.Log( 2 ) );

				float4[,]	sourceMip = _mip0;
				List< Renderer.PixelsBuffer >	mips = new List<Renderer.PixelsBuffer>( (int) mipsCount );
				for ( uint mipLevel=0; mipLevel < mipsCount; mipLevel++ ) {

					// Write mip
					Renderer.PixelsBuffer	mip = new  Renderer.PixelsBuffer( W*H*4 );
					mips.Add( mip );
					using ( System.IO.BinaryWriter Wr = mip.OpenStreamWrite() ) {
						if ( _isNormalMap ) {
							for ( int Y=0; Y < H; Y++ ) {
								for ( int X=0; X < W; X++ ) {
									Wr.Write( (sbyte) Mathf.Clamp( 256.0f * sourceMip[X,Y].x - 128.0f, -128, 127 ) );
									Wr.Write( (sbyte) Mathf.Clamp( 256.0f * sourceMip[X,Y].y - 128.0f, -128, 127 ) );
									Wr.Write( (sbyte) Mathf.Clamp( 256.0f * sourceMip[X,Y].z - 128.0f, -128, 127 ) );
									Wr.Write( (byte) 255 );
								}
							}
						} else {
							for ( int Y=0; Y < H; Y++ ) {
								for ( int X=0; X < W; X++ ) {
									Wr.Write( (byte) Mathf.Clamp( 256.0f * sourceMip[X,Y].x, 0, 255 ) );
									Wr.Write( (byte) Mathf.Clamp( 256.0f * sourceMip[X,Y].y, 0, 255 ) );
									Wr.Write( (byte) Mathf.Clamp( 256.0f * sourceMip[X,Y].z, 0, 255 ) );
									Wr.Write( (byte) 255 );
								}
							}
						}
					}

					// Build a new mip
					if ( mipLevel < mipsCount-1 ) {
						uint	oldW = W;
						uint	oldH = H;
						W = Math.Max( 1, W >> 1 );
						H = Math.Max( 1, H >> 1 );
						float4[,]	targetMip = new float4[W,H];

						float4	V00, V01, V10, V11, V;
						float4	two = new float4( 2, 2, 2, 1 );
						float4	one = new float4( 1, 1, 1, 0 );
						float4	eighth = new float4( 0.125f, 0.125f, 0.125f, 0 );
						float4	quarter = new float4( 0.5f, 0.5f, 0.5f, 0 );
						if ( _isNormalMap ) {
							// Sum as vectors
							for ( int Y=0; Y < H; Y++ ) {
								int	Y0 = Math.Min( (int) oldH-1, 2*Y+0 );
								int	Y1 = Math.Min( (int) oldH-1, 2*Y+1 );
								for ( int X=0; X < W; X++ ) {
									int	X0 = Math.Min( (int) oldW-1, 2*X+0 );
									int	X1 = Math.Min( (int) oldW-1, 2*X+1 );

									V00 = two * sourceMip[X0,Y0] - one;
									V01 = two * sourceMip[X0,Y1] - one;
									V10 = two * sourceMip[X1,Y0] - one;
									V11 = two * sourceMip[X1,Y1] - one;
									V = eighth * (V00 + V01 + V10 + V11) + quarter;
									targetMip[X,Y] = V;
								}
							}
						} else if ( _sRGB ) {
							// Sum as sRGB-packed colors
							for ( int Y=0; Y < H; Y++ ) {
								int	Y0 = Math.Min( (int) oldH-1, 2*Y+0 );
								int	Y1 = Math.Min( (int) oldH-1, 2*Y+1 );
								for ( int X=0; X < W; X++ ) {
									int	X0 = Math.Min( (int) oldW-1, 2*X+0 );
									int	X1 = Math.Min( (int) oldW-1, 2*X+1 );

									V00 = sRGB2Linear( sourceMip[X0,Y0] );
									V01 = sRGB2Linear( sourceMip[X0,Y1] );
									V10 = sRGB2Linear( sourceMip[X1,Y0] );
									V11 = sRGB2Linear( sourceMip[X1,Y1] );
									V = 0.25f * (V00 + V01 + V10 + V11);
									targetMip[X,Y] = Linear2sRGB( V );
								}
							}
						} else {
							// Sum as regular linear values
							for ( int Y=0; Y < H; Y++ ) {
								int	Y0 = Math.Min( (int) oldH-1, 2*Y+0 );
								int	Y1 = Math.Min( (int) oldH-1, 2*Y+1 );
								for ( int X=0; X < W; X++ ) {
									int	X0 = Math.Min( (int) oldW-1, 2*X+0 );
									int	X1 = Math.Min( (int) oldW-1, 2*X+1 );

									V00 = sourceMip[X0,Y0];
									V01 = sourceMip[X0,Y1];
									V10 = sourceMip[X1,Y0];
									V11 = sourceMip[X1,Y1];
									V = 0.25f * (V00 + V01 + V10 + V11);
									targetMip[X,Y] = V;
								}
							}
						}

						sourceMip = targetMip;
					}
				}

				return mips.ToArray();
			}

			float4	sRGB2Linear( float4 _V ) {
				return new float4( 
					ColorProfile.sRGB2Linear( _V.x ),
					ColorProfile.sRGB2Linear( _V.y ),
					ColorProfile.sRGB2Linear( _V.z ),
					_V.w
				);
			}
			float4	Linear2sRGB( float4 _V ) {
				return new float4( 
					ColorProfile.Linear2sRGB( _V.x ),
					ColorProfile.Linear2sRGB( _V.y ),
					ColorProfile.Linear2sRGB( _V.z ),
					_V.w
				);
			}
*/


			#region Serialization

			public void		Write( BinaryWriter _W ) {
				_W.Write( m_name );

				_W.Write( m_colorAmbient.x );
				_W.Write( m_colorAmbient.y );
				_W.Write( m_colorAmbient.z );
				_W.Write( m_colorDiffuse.x );
				_W.Write( m_colorDiffuse.y );
				_W.Write( m_colorDiffuse.z );
				_W.Write( m_colorSpecular.x );
				_W.Write( m_colorSpecular.y );
				_W.Write( m_colorSpecular.z );
				_W.Write( m_colorEmissive.x );
				_W.Write( m_colorEmissive.y );
				_W.Write( m_colorEmissive.z );
				_W.Write( m_exponentSpecular );
				_W.Write( m_alpha );
				_W.Write( m_bumpHeight );

				_W.Write( m_textureNameDiffuse != null ? m_textureNameDiffuse : "" );
				_W.Write( m_textureNameAlpha != null ? m_textureNameAlpha : "" );
				_W.Write( m_textureNameBump != null ? m_textureNameBump : "" );
				_W.Write( m_textureNameNormal != null ? m_textureNameNormal : "" );
				_W.Write( m_textureNameSpecular != null ? m_textureNameSpecular : "" );
				_W.Write( m_textureNameEmissive != null ? m_textureNameEmissive : "" );
			}

			public void		Read( BinaryReader _R ) {
				m_name = _R.ReadString();

				m_colorAmbient.Set( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle() );
				m_colorDiffuse.Set( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle() );
				m_colorSpecular.Set( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle() );
				m_colorEmissive.Set( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle() );
				m_exponentSpecular = _R.ReadSingle();
				m_alpha = _R.ReadSingle();
				m_bumpHeight = _R.ReadSingle();

				m_textureNameDiffuse = _R.ReadString();
				if ( m_textureNameDiffuse == "" ) m_textureNameDiffuse = null;
				m_textureNameAlpha = _R.ReadString();
				if ( m_textureNameAlpha == "" ) m_textureNameAlpha = null;
				m_textureNameBump = _R.ReadString();
				if ( m_textureNameBump == "" ) m_textureNameBump = null;
				m_textureNameNormal = _R.ReadString();
				if ( m_textureNameNormal == "" ) m_textureNameNormal = null;
				m_textureNameSpecular = _R.ReadString();
				if ( m_textureNameSpecular == "" ) m_textureNameSpecular = null;
				m_textureNameEmissive = _R.ReadString();
				if ( m_textureNameEmissive == "" ) m_textureNameEmissive = null;
			}

			#endregion

			#region IDisposable Members

			public void Dispose() {
				m_textureDiffuse = null;
				m_textureSpecular = null;
				m_textureNormal = null;
				m_textureEmissive = null;
			}

			#endregion

			#endregion
		}

		[System.Diagnostics.DebuggerDisplay( "{m_name} G={m_groupName} V#={m_vertices.Length,d} F#={m_faces.Length,d}" )]
		public class	Mesh : IDisposable {

			#region NESTED TYPES

			[System.Diagnostics.DebuggerDisplay( "[V#={m_vertices.Length} F#={m_faces.Length} Mat={m_materialName}" )]
			public class	Surface : IDisposable {

				#region NESTED TYPES

				[System.Diagnostics.DebuggerDisplay( "[{V0}, {V1}, {V2}] SG = {SG}" )]
				public struct	Face {
					public uint		V0, V1, V2;
					public uint		SG;			// Smoothing group

					public void		Write( BinaryWriter _W ) {
						_W.Write( V0 );
						_W.Write( V1 );
						_W.Write( V2 );
						_W.Write( SG );
					}

					public void		Read( BinaryReader _R ) {
						V0 = _R.ReadUInt32();
						V1 = _R.ReadUInt32();
						V2 = _R.ReadUInt32();
						SG = _R.ReadUInt32();
					}
				}

				[System.Diagnostics.DebuggerDisplay( "P={P}, N={N}, UV={UVW}" )]
				public struct	Vertex {
					public float3	P;
					public float3	N;
					public float3	T;
					public float3	B;
					public float3	UVW;

					public void		Write( BinaryWriter _W ) {
						_W.Write( P.x );
						_W.Write( P.y );
						_W.Write( P.z );
						_W.Write( N.x );
						_W.Write( N.y );
						_W.Write( N.z );
						_W.Write( T.x );
						_W.Write( T.y );
						_W.Write( T.z );
						_W.Write( B.x );
						_W.Write( B.y );
						_W.Write( B.z );
						_W.Write( UVW.x );
						_W.Write( UVW.y );
						_W.Write( UVW.z );
					}

					public void		Read( BinaryReader _R ) {
						P.Set( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle() );
						N.Set( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle() );
						T.Set( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle() );
						B.Set( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle() );
						UVW.Set( _R.ReadSingle(), _R.ReadSingle(), _R.ReadSingle() );
					}
				}

				#endregion

				public Mesh			m_owner;

				public Face[]		m_faces = null;
				public Vertex[]		m_vertices = null;
				public string		m_materialName;

				public Primitive	m_primitive = null;
				public Material		m_material = null;

				public Surface( Mesh _owner ) {
					m_owner = _owner;
				}

				public void		Render( Shader _shader, ObjectAndMaterialSetupDelegate _setup ) {
					if ( !_setup( this ) )
						return;

					m_primitive.Render( _shader );
				}

				internal void	CreatePrimitive( Device _device ) {
					List< uint >	faces = new List<uint>();
					foreach ( Face F in m_faces ) {
						faces.Add( F.V0 );
						faces.Add( F.V1 );
						faces.Add( F.V2 );
					}

					List<VertexP3N3G3B3T2>	vertices = new List<VertexP3N3G3B3T2>();
					foreach ( Vertex V in m_vertices ) {
						vertices.Add( new VertexP3N3G3B3T2() {
							P = V.P,
							N = V.N,
							T = V.T,
							B = V.B,
							UV = V.UVW.xy
						} );
// if ( V.B.Length < 0.99f )
// 	throw new Exception( "WTF?!" );
					}

					using ( ByteBuffer vertexBuffer = VertexP3N3G3B3T2.FromArray( vertices.ToArray() ) ) {
						m_primitive = new Primitive( _device, (uint) vertices.Count, vertexBuffer, faces.ToArray(), Primitive.TOPOLOGY.TRIANGLE_LIST, VERTEX_FORMAT.P3N3G3B3T2 );
					}

					// Resolve material
					m_material = m_owner.m_owner.m_materialName2Material.ContainsKey( m_materialName ) ? m_owner.m_owner.m_materialName2Material[m_materialName] : m_owner.m_owner.m_materialDefault;
				}

				#region Tangent Space Generation

				/// <summary>
				/// Builds the tangent space (UV set must exist!!!)
				/// </summary>
				public void	BuildTangents( bool _hasNormals ) {

					float[]		faceAreas = new float[m_faces.Length];
					float3[]	faceNormals = new float3[m_faces.Length];
					float3[]	faceTangents = new float3[m_faces.Length];
					float3[]	faceBiTangents = new float3[m_faces.Length];
					float3		T = float3.Zero;
					float3		B = float3.Zero;

					for ( int faceIndex=0; faceIndex < m_faces.Length; faceIndex++ ) {
						Surface.Face	F = m_faces[faceIndex];

						Surface.Vertex	V0 = m_vertices[(int)F.V0];
						Surface.Vertex	V1 = m_vertices[(int)F.V1];
						Surface.Vertex	V2 = m_vertices[(int)F.V2];

						float3	P0 = V0.P;
						float3	P1 = V1.P;
						float3	P2 = V2.P;
						float2	UV0 = V0.UVW.xy;
						float2	UV1 = V1.UVW.xy;
						float2	UV2 = V2.UVW.xy;

						float3	dP0 = P2 - P1;
						float3	dP1 = P0 - P1;
						float2	dUV0 = UV2 - UV1;
						float2	dUV1 = UV0 - UV1;

						float3	N = dP0.Cross( dP1 );
						float	faceArea = N.LengthSquared;
						if ( faceArea > 0.0f ) {
							faceArea = Mathf.Sqrt( faceArea );
							N /= faceArea;
							faceArea *= 0.5f;
						} else {
							N = float3.UnitY;
						}

						// Texture space area
						float	area = dUV0.x * dUV1.y - dUV1.x * dUV0.y;
						if ( Math.Abs( area ) < 1e-20f ) {
							// bmayaux (2014-03-03) This case can occur when one of the UV coordinates is mapped to an identical value (i.e. bad case of planar mapping)
							// In that case, we must check if we can save the day by using any one of the non-degenerate coordinates...
							bool	degenerateU = Mathf.Almost( P0.x, P1.x ) && Mathf.Almost( P0.x, P2.x );
							bool	degenerateV = Mathf.Almost( P0.y, P1.y ) && Mathf.Almost( P0.y, P2.y );

							if ( !degenerateU ) {
								// Cannot use V...
								// We apply grad(U) = dUV0.x * grad(X) + dUV1.x * grad(Y)  (except I re-used id's code because I don't understand their orientation)
								// Where X = B-A and Y = C-A
								//
								B.x = dUV0.x * dP1.x - dP0.x * dUV1.x;
								B.y = dUV0.x * dP1.y - dP0.y * dUV1.x;
								B.z = dUV0.x * dP1.z - dP0.z * dUV1.x;
								float	length = B.Length;
								if ( length > 1e-8f ) {
									B /= length;	// Valid!
									T = B.Cross( N );
								} else {
									degenerateU = degenerateV = true;
								}
							} else if ( !degenerateV ) {
								// Cannot use U...
								// We apply grad(V) = dUV0.y * grad(X) + dUV1.y * grad(Y)  (except I re-used id's code because I don't understand their orientation)
								// Where X = B-A and Y = C-A
								//
								T.x = dP0.x * dUV1.y - dUV0.y * dP1.x;
								T.y = dP0.y * dUV1.y - dUV0.y * dP1.y;
								T.z = dP0.z * dUV1.y - dUV0.y * dP1.z;
								float	length = T.Length;
								if ( length > 1e-8f ) {
									T /= length;	// Valid!
									B = N.Cross( T );
								} else {
									degenerateU = degenerateV = true;
								}
							}
						
							if ( degenerateU && degenerateV ) {
								// Degenerate texture space // ARKANE: bmayaux (2014-03-03) HARRRR!! NOT A REASON TO SET THE TBN TO 0!
								N.OrthogonalBasis( out T, out B );
							}

						} else {

							// Normal case
							float polarity = area < 0.0f ? -1.0f : 1.0f;

							T.x = ( dP0.x * dUV1.y - dUV0.y * dP1.x ) * polarity;
							T.y = ( dP0.y * dUV1.y - dUV0.y * dP1.y ) * polarity;
							T.z = ( dP0.z * dUV1.y - dUV0.y * dP1.z ) * polarity;
							T.Normalize();

							polarity *= -1.0f;	// ARKANE: bmayaux (2014-03-03) In the shader, we need bitangents pointing the other way, like in Maya!

							B.x = ( dUV0.x * dP1.x - dP0.x * dUV1.x ) * polarity;
							B.y = ( dUV0.x * dP1.y - dP0.y * dUV1.x ) * polarity;
							B.z = ( dUV0.x * dP1.z - dP0.z * dUV1.x ) * polarity;
							B.Normalize();
						}

						// Store tangent space
						faceAreas[faceIndex] = faceArea;
						faceNormals[faceIndex] = N;
						faceTangents[faceIndex] = T;
						faceBiTangents[faceIndex] = B;
					}

					// Dispatch tangent space to vertices
					for ( int vertexIndex=0; vertexIndex < m_vertices.Length; vertexIndex++ ) {
						Vertex	V = m_vertices[vertexIndex];
						V.T = float3.Zero;
						V.B = float3.Zero;
					}
					for ( int faceIndex=0; faceIndex < m_faces.Length; faceIndex++ ) {
						Surface.Face	F = m_faces[faceIndex];
						float			A = faceAreas[faceIndex];

						m_vertices[F.V0].T += A * faceTangents[faceIndex];
						m_vertices[F.V0].B += A * faceBiTangents[faceIndex];

						m_vertices[F.V1].T += A * faceTangents[faceIndex];
						m_vertices[F.V1].B += A * faceBiTangents[faceIndex];

						m_vertices[F.V2].T += A * faceTangents[faceIndex];
						m_vertices[F.V2].B += A * faceBiTangents[faceIndex];

						if ( _hasNormals )
							continue;

						m_vertices[F.V0].N += A * faceNormals[faceIndex];
						m_vertices[F.V1].N += A * faceNormals[faceIndex];
						m_vertices[F.V2].N += A * faceNormals[faceIndex];
					}

					// Normalize results
					for ( int vertexIndex=0; vertexIndex < m_vertices.Length; vertexIndex++ ) {
						Vertex	V = m_vertices[vertexIndex];
						if ( !_hasNormals ) {
							if ( V.N.LengthSquared > 0 )
								V.N.Normalize();
							else
								V.N = V.P.Normalized;	// Why not ?
						}
						if ( V.T.LengthSquared < 1e-6f || V.B.LengthSquared < 1e-6f ) {
							V.N.OrthogonalBasis( out V.T, out V.B );	// Build replacement basis
						}
						V.T.Normalize();
						V.B.Normalize();

if ( !Mathf.Almost( V.N.Length, 1.0f, 1e-3f ) || !Mathf.Almost( V.T.Length, 1.0f, 1e-3f ) || !Mathf.Almost( V.B.Length, 1.0f, 1e-3f ) )	throw new Exception( "MERDE!" );
if ( float.IsNaN( V.N.x ) || float.IsNaN( V.N.y ) || float.IsNaN( V.N.z ) )	throw new Exception( "MERDE!" );
if ( float.IsNaN( V.T.x ) || float.IsNaN( V.T.y ) || float.IsNaN( V.T.z ) )	throw new Exception( "MERDE!" );
if ( float.IsNaN( V.B.x ) || float.IsNaN( V.B.y ) || float.IsNaN( V.B.z ) )	throw new Exception( "MERDE!" );
if ( float.IsInfinity( V.N.x ) || float.IsInfinity( V.N.y ) || float.IsInfinity( V.N.z ) )	throw new Exception( "MERDE!" );
if ( float.IsInfinity( V.T.x ) || float.IsInfinity( V.T.y ) || float.IsInfinity( V.T.z ) )	throw new Exception( "MERDE!" );
if ( float.IsInfinity( V.B.x ) || float.IsInfinity( V.B.y ) || float.IsInfinity( V.B.z ) )	throw new Exception( "MERDE!" );

						m_vertices[vertexIndex] = V;
					}
				}

				#endregion

				#region Serialization

				public void		Write( BinaryWriter _W ) {
					_W.Write( m_materialName );

					uint	verticesCount = (uint) m_vertices.Length;
					_W.Write( verticesCount );
					for ( uint vertexIndex=0; vertexIndex < verticesCount; vertexIndex++ ) {
						m_vertices[vertexIndex].Write( _W );
					}

					uint	facesCount = (uint) m_faces.Length;
					_W.Write( facesCount );
					for ( uint faceIndex=0; faceIndex < facesCount; faceIndex++ ) {
						m_faces[faceIndex].Write( _W );
					}
				}

				public void		Read( BinaryReader _R ) {
					m_materialName = _R.ReadString();

					uint	verticesCount = _R.ReadUInt32();
					m_vertices = new Vertex[verticesCount];
					for ( uint vertexIndex=0; vertexIndex < verticesCount; vertexIndex++ ) {
						m_vertices[vertexIndex].Read( _R );
					}

					uint	facesCount = _R.ReadUInt32();
					m_faces = new Face[facesCount];
					for ( uint faceIndex=0; faceIndex < facesCount; faceIndex++ ) {
						m_faces[faceIndex].Read( _R );
					}
				}

				#endregion

				#region IDisposable Members

				public void Dispose() {
					if ( m_primitive != null )
						m_primitive.Dispose();
					m_primitive = null;
				}

				#endregion
			}

			#endregion

			#region FIELDS

			private SceneObj		m_owner;

			public string			m_name;
			public string			m_groupName;
			public Surface[]		m_surfaces = null;

			public float4x4			m_local2World = float4x4.Identity;
			public float4x4			m_previousFrameLocal2World = float4x4.Identity;

			public static float		ms_weldingThreshold_Normal = 0.95f;	// Difference below which the dot product between normals is discarded
			public static float		ms_weldingThreshold_UV = 0.01f;		// Difference above which the distance between UVs is discarded

			#endregion

			#region METHODS

			public	Mesh( SceneObj _owner ) {
				m_owner = _owner;
			}

			public	Mesh( SceneObj _owner, BinaryReader _R ) {
				m_owner = _owner;
				Read( _R );
			}

			public void		Render( Shader _shader, ObjectAndMaterialSetupDelegate _setup ) {
				foreach ( Surface S in m_surfaces ) {
					S.Render( _shader, _setup );
				}
			}

			internal void		CreatePrimitive( Device _device, float4x4 _local2World ) {
				m_local2World = _local2World;
				m_previousFrameLocal2World = _local2World;

				foreach ( Surface S in m_surfaces ) {
					S.CreatePrimitive( _device );
				}
			}

			#region Serialization

			public void		Write( BinaryWriter _W ) {
				_W.Write( m_name );
				_W.Write( m_groupName );

				uint	surfacesCount = (uint) m_surfaces.Length;
				_W.Write( surfacesCount );
				foreach ( Surface S in m_surfaces )
					S.Write( _W );
			}

			public void		Read( BinaryReader _R ) {
				m_name = _R.ReadString();
				m_groupName = _R.ReadString();

				uint	surfacesCount = _R.ReadUInt32();
				m_surfaces = new Surface[surfacesCount];
				for ( int surfaceIndex=0; surfaceIndex < surfacesCount; surfaceIndex++ ) {
					Surface	S = new Surface( this );
					m_surfaces[surfaceIndex] = S;
					S.Read( _R );
				}
			}

			#endregion

			#region IDisposable Members

			public void Dispose() {
				foreach ( Surface S in m_surfaces )
					S.Dispose();
			}

			#endregion

			#endregion
		}

		#endregion

		#region FIELDS

		protected DirectoryInfo		m_baseDirectory = null;

		protected List< Mesh >		m_meshes = new List<Mesh>();

		protected List< Material >	m_materials = new List<Material>();
		protected Material			m_materialDefault = null;
		protected Dictionary< string, Material >	m_materialName2Material = new Dictionary<string, Material>();

		protected Dictionary< string, Texture2D >	m_textureName2Texture = new Dictionary<string, Texture2D>();

		#endregion

		#region PROPERTIES

		public Mesh[]		Meshes	{ get { return m_meshes.ToArray(); } }

		#endregion

		#region METHODS

		public SceneObj() {
		}

		public void		CreatePrimitivesAndTextures( Device _device, float4x4 _local2World ) {
			if ( m_materialDefault == null ) {
				// Create default material
				m_materialDefault = new Material( this ) { m_name = "<DEFAULT>" };
				m_materialDefault.m_colorDiffuse = new float3( 1, 0, 1 );
				m_materialDefault.m_colorSpecular = 0.1f * float3.One;
				m_materialDefault.m_colorEmissive = m_materialDefault.m_colorDiffuse;
				m_materials.Add( m_materialDefault );
			}

			foreach ( Material M in m_materials ) {
				M.CreateTextures( _device );
			}

			foreach ( Mesh M in m_meshes ) {
				M.CreatePrimitive( _device, _local2World );
			}
		}

		public void		Render( Shader _shader, ObjectAndMaterialSetupDelegate _setup ) {
			foreach ( Mesh M in m_meshes ) {
				M.Render( _shader, _setup );
			}
		}

		#region Binary Scene File

		public void		Write( BinaryWriter _W ) {
			_W.Write( (uint) m_materials.Count );
			foreach ( Material M in m_materials )
				M.Write( _W );

			_W.Write( (uint) m_meshes.Count );
			foreach ( Mesh M in m_meshes )
				M.Write( _W );
		}

		public void		Read( DirectoryInfo _baseDirectory, BinaryReader _R ) {
			m_baseDirectory= _baseDirectory;

			m_meshes.Clear();
			m_materials.Clear();
			m_materialName2Material.Clear();

			uint	materialsCount = _R.ReadUInt32();
			for ( uint materialIndex=0; materialIndex < materialsCount; materialIndex++ ) {
				Material	M = new Material( this, _R );
				m_materials.Add( M );
				if ( !m_materialName2Material.ContainsKey( M.m_name ) )
					m_materialName2Material.Add( M.m_name, M );
			}

			uint	meshesCount = _R.ReadUInt32();
			for ( uint meshIndex=0; meshIndex < meshesCount; meshIndex++ ) {
				Mesh	M = new Mesh( this, _R );
				m_meshes.Add( M );
			}
		}

		#endregion

		#region OBJ Text File Support

		public void		LoadOBJFile( FileInfo _fileNameOBJ, bool _loadFromBinaryOrSaveIfNotAvailable ) {
			m_baseDirectory = _fileNameOBJ.Directory;

			// Check if the binary scene file is available
			FileInfo	binarySceneFileName = new FileInfo( Path.Combine( _fileNameOBJ.DirectoryName, Path.GetFileNameWithoutExtension( _fileNameOBJ.FullName ) + ".objbin" ) );
			if ( _loadFromBinaryOrSaveIfNotAvailable && binarySceneFileName.Exists ) {
				using ( FileStream S = binarySceneFileName.OpenRead() )
					using ( BinaryReader R = new BinaryReader( S ) ) {
						Read( m_baseDirectory, R );
					}

				return;
			}

			// Load from OBJ text file
			char[]	separators = new char[] { ' ', '\t' };

			bool	readFaces = false;
			using ( StreamReader R = _fileNameOBJ.OpenText() ) {
				while ( !R.EndOfStream ) {
					string	L = R.ReadLine();
					if ( L.StartsWith( "#" ) ) {
						continue;	// Skip comments
					}

					string[]	elements = L.Split( separators, StringSplitOptions.RemoveEmptyEntries );
					if ( elements.Length == 0 ) {
						continue;	// Empty line...
					}

					switch ( elements[0].ToLower() ) {
						// Geometry
						case "v":
							if ( elements.Length < 4 )
								throw new Exception( "Unexpected amount of vertex position components! (expecting 3 or 4)" );

							if ( readFaces ) {
								// If we're reading vertices again after having read faces then it means we're starting a new mesh...
								readFaces = false;
								FinalizeMesh();		// Finalize any former mesh
							}
							AppendPosition( elements[1], elements[2], elements[3] );
							break;

						case "vn":
							if ( elements.Length != 4 )
								throw new Exception( "Unexpected amount of vertex normal components! (expecting 3)" );
							AppendNormal( elements[1], elements[2], elements[3] );
							break;

						case "vt":
							if ( elements.Length < 3 )
								throw new Exception( "Unexpected amount of vertex UV components! (expecting 2 or 3)" );
							AppendUV( elements[1], elements[2], elements.Length == 4 ? elements[3] : "0.0" );
							break;

						// Faces
						case "s":
							if ( elements.Length < 2 )
								throw new Exception( "Missing smoothing group value!" );
							if ( !uint.TryParse( elements[1], out m_smoothingGroups ) )
								m_smoothingGroups = 0;
							break;
						case "f":
							if ( elements.Length < 4 )
								throw new Exception( "Missing face elements (expecting at least 3 for triangles)!" );
							AddPolygon( elements );
							readFaces = true;
							break;
						case "invertfaces":	// CUSTOM!
							uint	invertState = 1;
							if ( elements.Length == 2 ) {
								uint.TryParse( elements[1], out invertState );
							}
							m_invertFaces = invertState != 0;
							break;

						// Object naming
						case "o":
							if ( elements.Length < 2 )
								throw new Exception( "Missing object name!" );
							m_objectName = elements[1];
							break;
						case "g":
							if ( elements.Length < 2 )
								throw new Exception( "Missing object group name!" );
							m_objectGroupName = elements[1];
							break;

						// Object materials
						case "mtllib":
							if ( elements.Length < 2 )
								throw new Exception( "Material library name is missing!" );
							LoadMaterialsLibrary( new FileInfo( Path.Combine( _fileNameOBJ.DirectoryName, elements[1] ) ) );
							break;
						case "usemtl":
							if ( elements.Length < 2 )
								throw new Exception( "Missing material name!" );

							// Start a new surface
							FinalizeSurface();

							m_surfaceMaterialName = elements[1].ToLower();
							break;

						case "usemap":
							// Ignore... The map name should be specified in the material instead...
							break;

						default:
							throw new Exception( "Unsupported element type!" );
					}
				}

				FinalizeMesh();
			}

			// Save if needed as binary later
			if ( _loadFromBinaryOrSaveIfNotAvailable ) {
				using ( System.IO.FileStream S = binarySceneFileName.Create() )
					using ( System.IO.BinaryWriter W = new System.IO.BinaryWriter( S ) )
						Write( W );
			}
		}

		Material	m_currentMaterial = null;
		public void		LoadMaterialsLibrary( FileInfo _fileNameMTL ) {
			char[]	separators = new char[] { ' ', '\t' };

			using ( StreamReader R = _fileNameMTL.OpenText() ) {
				while ( !R.EndOfStream ) {
					string	L = R.ReadLine();
					if ( L.StartsWith( "#" ) ) {
						continue;	// Skip comments
					}

					string[]	elements = L.Split( separators, StringSplitOptions.RemoveEmptyEntries );
					if ( elements.Length == 0 ) {
						continue;	// Empty line...
					}

					switch ( elements[0].ToLower() ) {
						case "newmtl":
							if ( elements.Length < 2 )
								throw new Exception( "Missing material name!" );

							FinalizeMaterial();
							m_currentMaterial = new Material( this );
							m_materials.Add( m_currentMaterial );
							m_currentMaterial.m_name = elements[1].ToLower();
							break;

						// Basic colors
						case "ka":
							if ( elements.Length != 4 )
								throw new Exception( "Unexpected amount of components for ambient color!" );
							m_currentMaterial.m_colorAmbient.Set( float.Parse( elements[1] ), float.Parse( elements[2] ), float.Parse( elements[3] ) );
							break;

						case "kd":
							if ( elements.Length != 4 )
								throw new Exception( "Unexpected amount of components for diffuse color!" );
							m_currentMaterial.m_colorDiffuse.Set( float.Parse( elements[1] ), float.Parse( elements[2] ), float.Parse( elements[3] ) );
							break;

						case "ks":
							if ( elements.Length != 4 )
								throw new Exception( "Unexpected amount of components for specular color!" );
							m_currentMaterial.m_colorSpecular.Set( float.Parse( elements[1] ), float.Parse( elements[2] ), float.Parse( elements[3] ) );
							break;

						case "ke":
							if ( elements.Length != 4 )
								throw new Exception( "Unexpected amount of components for emissive color!" );
							m_currentMaterial.m_colorEmissive.Set( float.Parse( elements[1] ), float.Parse( elements[2] ), float.Parse( elements[3] ) );
							break;

						// Alpha
						case "d":
							if ( elements.Length != 2 )
								throw new Exception( "Unexpected amount of components for alpha!" );
							m_currentMaterial.m_alpha = float.Parse( elements[1] );
							break;
						case "tr":
							if ( elements.Length != 2 )
								throw new Exception( "Unexpected amount of components for alpha!" );
							m_currentMaterial.m_alpha = 1.0f - float.Parse( elements[1] );
							break;

						// Specular exponent
						case "ns":
							if ( elements.Length != 2 )
								throw new Exception( "Unexpected amount of components for specular exponent!" );
							m_currentMaterial.m_exponentSpecular = float.Parse( elements[1] );
							break;

						// Textures
						case "map_kd":
							if ( elements.Length != 2 )
								throw new Exception( "Unexpected amount of components for diffuse map name!" );
							m_currentMaterial.m_textureNameDiffuse = elements[1];
							break;
						case "map_d":
							if ( elements.Length != 2 )
								throw new Exception( "Unexpected amount of components for alpha map name!" );
							m_currentMaterial.m_textureNameAlpha = elements[1];
							break;
						case "map_ks":
							if ( elements.Length != 2 )
								throw new Exception( "Unexpected amount of components for specular map name!" );
							m_currentMaterial.m_textureNameSpecular = elements[1];
							break;
						case "map_bump":
							if ( elements.Length != 2 )
								throw new Exception( "Unexpected amount of components for bump map name!" );
							m_currentMaterial.m_textureNameBump = elements[1];
							break;
						case "norm":
							if ( elements.Length != 2 )
								throw new Exception( "Unexpected amount of components for normal map name!" );
							m_currentMaterial.m_textureNameNormal = elements[1];
							break;
						case "map_ke":
							if ( elements.Length != 2 )
								throw new Exception( "Unexpected amount of components for emissive map name!" );
							m_currentMaterial.m_textureNameEmissive = elements[1];
							break;

						// Custom Fields
						case "bump_height":
							if ( elements.Length != 2 )
								throw new Exception( "Unexpected amount of components for bump height!" );
							m_currentMaterial.m_bumpHeight = float.Parse( elements[1] );
							break;
					}
				}

				FinalizeMaterial();
			}
		}

		#region Temporary Mesh Building

		List< float3 >		m_tempPositions = new List<float3>();
		List< float3 >		m_tempNormals = new List<float3>();
		List< float3 >		m_tempUVs = new List<float3>();
		List< Mesh.Surface.Face >	m_tempFaces = new List<Mesh.Surface.Face>();
		List< Mesh.Surface.Vertex >	m_tempVertices = new List<Mesh.Surface.Vertex>();
		List< Mesh.Surface >		m_tempSurfaces = new List<Mesh.Surface>();
		string				m_objectName = "<unnamed>";
		string				m_objectGroupName = "<default>";
		string				m_surfaceMaterialName = "<default>";
		uint				m_smoothingGroups = 1;
		uint				m_offsetPosition = 1;
		uint				m_offsetNormal = 1;
		uint				m_offsetUV = 1;
		bool				m_invertFaces = false;

		void		AppendPosition( string _X, string _Y, string _Z ) {
			m_tempPositions.Add( new float3( float.Parse( _X ), float.Parse( _Y ), float.Parse( _Z ) ) );
		}
		void		AppendNormal( string _X, string _Y, string _Z ) {
			float	sign = m_invertFaces ? -1.0f : 1.0f;
			m_tempNormals.Add( sign * new float3( float.Parse( _X ), float.Parse( _Y ), float.Parse( _Z ) ) );
		}
		void		AppendUV( string _U, string _V, string _W ) {
//			m_tempUVs.Add( new float3( float.Parse( _U ), float.Parse( _V ), float.Parse( _W ) ) );
			m_tempUVs.Add( new float3( float.Parse( _U ), 1.0f - float.Parse( _V ), float.Parse( _W ) ) );
		}

		void		AddPolygon( string[] _faceElements ) {
			int		facesCount = _faceElements.Length - 3;
			uint	faceVertex0 = AddVertex( _faceElements[1].Split( '/' ) );
			uint	faceVertex2 = AddVertex( _faceElements[2].Split( '/' ) );
			for ( int faceIndex=0; faceIndex < facesCount; faceIndex++ ) {
				uint	faceVertex1 = faceVertex2;
				faceVertex2 = AddVertex( _faceElements[3+faceIndex].Split( '/' ) );
				if ( !m_invertFaces )
					m_tempFaces.Add( new Mesh.Surface.Face() { V0 = faceVertex0, V1 = faceVertex1, V2 = faceVertex2, SG = m_smoothingGroups } );
				else
					m_tempFaces.Add( new Mesh.Surface.Face() { V0 = faceVertex0, V1 = faceVertex2, V2 = faceVertex1, SG = m_smoothingGroups } );
			}
		}

		Dictionary< uint, List< uint > >	m_vertexPosition2SharedVertices = new Dictionary<uint, List<uint>>();
		uint		AddVertex( string[] _faceElements ) {
			uint	indexPosition = uint.Parse( _faceElements[0] ) - m_offsetPosition;
			float3	N;
			float3	UV;
			if ( m_tempNormals.Count > 0 && m_tempUVs.Count > 0 ) {
				// Both normals and UVs
				if ( _faceElements.Length != 3 )
					throw new Exception( "Unexpected amount of face indices!" );

				uint	indexUV = uint.Parse( _faceElements[1] ) - m_offsetUV;
				UV = m_tempUVs[(int) indexUV];

				uint	indexNormal = uint.Parse( _faceElements[2] ) - m_offsetNormal;
				N = m_tempNormals[(int) indexNormal];

			} else if ( m_tempNormals.Count > 0 ) {
				// Only normals, no UV
				if ( _faceElements.Length != 2 )
					throw new Exception( "Unexpected amount of face indices!" );

				uint	indexNormal = uint.Parse( _faceElements[1] ) - m_offsetNormal;
				N = m_tempNormals[(int) indexNormal];

				UV = float3.Zero;
			} else if ( m_tempUVs.Count > 0 ) {
				// Only UVs, no normal
				if ( _faceElements.Length != 2 )
					throw new Exception( "Unexpected amount of face indices!" );

				uint	indexUV = uint.Parse( _faceElements[1] ) - m_offsetUV;
				UV = m_tempUVs[(int) indexUV];

				N = float3.Zero;
			} else {
				// Only positions
				N = float3.Zero;
				UV = float3.Zero;
			}

			// Check if existing vertices already contain the same data
			uint			selectedVertexIndex = ~0U;
			List< uint >	existingIndices = null;
			if ( m_vertexPosition2SharedVertices.ContainsKey( indexPosition ) ) {
				existingIndices = m_vertexPosition2SharedVertices[indexPosition];
				foreach ( uint index in existingIndices ) {
					Mesh.Surface.Vertex	existingVertex = m_tempVertices[(int) index];
					float		dotN = existingVertex.N.Dot( N );
					if ( dotN < Mesh.ms_weldingThreshold_Normal )
						continue;	// Too much discrepancy
					float3	dUVW = existingVertex.UVW - UV;
					float	dotUVW = dUVW.Dot( dUVW );
					if ( dotUVW > Mesh.ms_weldingThreshold_UV*Mesh.ms_weldingThreshold_UV )
						continue;	// Too much distance

					// Found it!
					selectedVertexIndex = index;
					break;
				}
			}

			if ( selectedVertexIndex != ~0U )
				return selectedVertexIndex;	// Found an existing vertex that fits!

			// Create a new vertex
			uint	newVertexIndex = (uint) m_tempVertices.Count;
			if ( existingIndices == null ) {
				existingIndices = new List<uint>();
				m_vertexPosition2SharedVertices.Add( indexPosition, existingIndices );
			}
			existingIndices.Add( newVertexIndex );

			Mesh.Surface.Vertex	V = new Mesh.Surface.Vertex();
			V.P = m_tempPositions[(int) indexPosition];
			V.N = N;
			V.UVW = UV;

			m_tempVertices.Add( V );

			return newVertexIndex;
		}

		void	FinalizeSurface() {
			if ( m_tempFaces.Count == 0 )
				return;	// No current surface to finalize...

			Mesh.Surface	newSurface = new Mesh.Surface( null );
			m_tempSurfaces.Add( newSurface );

			newSurface.m_materialName = m_surfaceMaterialName;
			newSurface.m_vertices = m_tempVertices.ToArray();
			newSurface.m_faces = m_tempFaces.ToArray();
			m_tempVertices.Clear();
			m_tempFaces.Clear();
			m_surfaceMaterialName = "<default>";
			m_vertexPosition2SharedVertices.Clear();
		}

		void	FinalizeMesh() {
			if ( m_tempPositions.Count == 0 )
				return;	// Empty mesh...

			FinalizeSurface();	// Finalize any current surface

			Mesh	newMesh = new Mesh( this );
			m_meshes.Add( newMesh );

			newMesh.m_name = m_objectName;
			newMesh.m_groupName = m_objectGroupName;
			newMesh.m_surfaces = m_tempSurfaces.ToArray();
			foreach ( Mesh.Surface S in newMesh.m_surfaces ) {
				S.m_owner = newMesh;	// Finally assign owner mesh
				S.BuildTangents( m_tempNormals.Count > 0 );
			}

			// Cleanup data
			m_offsetPosition += (uint) m_tempPositions.Count;	// For some reason, indices keep on accumulating from the vertex we left off...
			m_offsetNormal += (uint) m_tempNormals.Count;
			m_offsetUV += (uint) m_tempUVs.Count;

			m_tempPositions.Clear();
			m_tempNormals.Clear();
			m_tempUVs.Clear();
			m_tempSurfaces.Clear();
			m_objectName = "<unnamed>";
			m_objectGroupName = "<default>";
			m_smoothingGroups = 1;
		}

		#endregion

		#region Temporary Material Building

		void	FinalizeMaterial() {
			if ( m_currentMaterial == null )
				return;

			// Convert bump map into normal map and save the result
			if ( !Material.ms_bumpIsNormalMap && Material.ms_saveConvertedNormalMaps && m_currentMaterial.m_textureNameBump != null && m_currentMaterial.m_textureNameNormal == null ) {
				try {
//					FileInfo	normalMapFileName = new FileInfo( Path.Combine( Path.GetDirectoryName( m_currentMaterial.m_textureNameBump ), Path.GetFileNameWithoutExtension( m_currentMaterial.m_textureNameBump ) + "_n" + Path.GetExtension( m_currentMaterial.m_textureNameBump ) ) );
					string		sourceBumpMapFileName = m_currentMaterial.m_textureNameBump;
					string		targetNormalMapFileName = sourceBumpMapFileName.Substring( 0, sourceBumpMapFileName.LastIndexOf( '.') ) + "_n" + Path.GetExtension( sourceBumpMapFileName );
					FileInfo	targetFileName = new FileInfo( Path.Combine( m_baseDirectory.FullName, targetNormalMapFileName ) );
					if ( !targetFileName.Exists ) {
						// Read back source height map
						FileInfo	sourceFileName = new FileInfo( Path.Combine( m_baseDirectory.FullName, sourceBumpMapFileName ) );
						if ( !sourceFileName.Exists )
							throw new Exception( "Source bump map file not found!" );

						float3		LUMINANCE = new float3( 0.2126f, 0.7152f, 0.0722f );
						ImageFile	imageSource = new ImageFile( sourceFileName );
						uint		W = imageSource.Width;
						uint		H = imageSource.Height;
						float		heightFactor = 0.1f * m_currentMaterial.m_bumpHeight;
						float[,]	heightMap = new float[W,H];
						imageSource.ReadPixels( ( uint _X, uint _Y, ref float4 _color ) => { heightMap[_X,_Y] = heightFactor * _color.xyz.Dot( LUMINANCE ); } );
						imageSource.Dispose();

						// Convert into normal map
						float3[,]	normalMap = new float3[W,H];
						uint		Xn , Xp, Yn, Yp;
						float3		Dx = new float3( 2, 0, 0 ), Dy = new float3( 0, 2, 0 ), N;
						for ( uint Y=0; Y < H; Y++ ) {
							Yn = (Y + H - 1) % H;
							Yp = (Y + 1) % H;
							for ( uint X=0; X < W; X++ ) {
								Xn = (X + W - 1) % W;
								Xp = (X + 1) % W;

								Dx.z = heightMap[Xp,Y] - heightMap[Xn,Y];
								Dy.z = heightMap[X,Yp] - heightMap[X,Yn];
								N = Dx.Cross( Dy );
								N.Normalize();
								normalMap[X,Y] = N;
							}
						}

						// Save result
						ImageFile	imageTarget = new ImageFile( (uint) normalMap.GetLength(0), (uint) normalMap.GetLength(1), PIXEL_FORMAT.BGR8, new ColorProfile( ColorProfile.STANDARD_PROFILE.LINEAR ) );
						imageTarget.WritePixels( (uint _X, uint _Y, ref float4 _color ) => {
							_color.Set( 0.5f * (float3.One + normalMap[_X,_Y]), 1.0f );
						} );

						imageTarget.Save( targetFileName, ImageFile.FILE_FORMAT.PNG );
					}

					// Replace normal map texture name
					m_currentMaterial.m_textureNameNormal = targetNormalMapFileName;

				} catch ( Exception _e ) {
					throw new Exception( "Failed to convert source bump map \"" + m_currentMaterial.m_textureNameBump + "\" into normal map:\r\n" + _e.Message, _e );
				}
			}

			// Register into dictionary
			m_materialName2Material.Add( m_currentMaterial.m_name, m_currentMaterial );
			m_currentMaterial = null;
		}

		#endregion

		#endregion

		#region IDisposable Members

		public void Dispose() {
			foreach ( Mesh M in m_meshes )
				M.Dispose();
			foreach ( Material M in m_materials )
				M.Dispose();
			foreach ( Texture2D T in m_textureName2Texture.Values )
				T.Dispose();

			m_meshes.Clear();
			m_materials.Clear();
			m_materialName2Material.Clear();
			m_textureName2Texture.Clear();
		}

		#endregion

		#endregion
	}
}
