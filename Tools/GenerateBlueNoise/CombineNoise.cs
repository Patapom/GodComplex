//#define	DEBUG_BINARY_PATTERN
//#define	BYPASS_GPU_DOWNSAMPLING

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SharpMath;
using ImageUtility;
//using Renderer;

namespace GenerateBlueNoise
{
	/// <summary>
	/// Combines several channels of blue-noise into a multi-channel texture in a manner that makes
	///  the channels the least correlated together
	/// </summary>
	public class CombineNoise {
		public	CombineNoise() {
		}

		public ImageFile	Combine( ImageFile[] _layers ) {
			if ( _layers == null || _layers.Length == 1 )
				throw new Exception( "Invalid list of layers or only 1 layer!" );
			uint	size = _layers[0].Width;
			if ( _layers[0].Height != size )
				throw new Exception( "Layers must be square-sized!" );
			int		sizePOT = (int) (Math.Log( size ) / Math.Log(2));
			uint	mask = size - 1;
			if ( (1 << sizePOT) != size )
				throw new Exception( "Layer size must be a power of two!" );	// For no other reason we want the mask to be full of 1's, otherwise you can replace the & mask by a modulo... :/
			uint	layersCount = (uint) _layers.Length;
			for ( uint layerIndex=1; layerIndex < layersCount; layerIndex++ ) {
				if ( _layers[layerIndex].Width != size || _layers[layerIndex].Height != size )
					throw new Exception( "Layer sizes mismatch!" );
			}

			// Read layers into a simple format
			float[][,]	layers = new float[layersCount][,];
			for ( uint layerIndex=0; layerIndex < layersCount; layerIndex++ ) {
				layers[layerIndex] = new float[size,size];
				_layers[layerIndex].ReadPixels( ( uint X, uint Y, ref float4 _color ) => { layers[layerIndex][X,Y] = _color.x; } );
			}
			float4[,]	alignedLayers = new float4[size,size];
			for ( uint Y=0; Y < size; Y++ )
				for ( uint X=0; X < size; X++ ) {
					alignedLayers[X,Y].x = layers[0][X,Y];
				}

			const double	sigma_s = 1.0;
			double	exponentialFactor = -1.0 / sigma_s;

			// Attempt to align each layer
			for ( uint layerIndex=1; layerIndex < layersCount; layerIndex++ ) {
				float[,]	layer = layers[layerIndex];

				uint		bestOffsetX = 0, bestOffsetY = 0;
				double		minScore = double.MaxValue;
				for ( uint offsetY=0; offsetY < size; offsetY++ ) {
					for ( uint offsetX=0; offsetX < size; offsetX++ ) {

						// Accumulate scores for each pixel
						double	score = 0.0;
						switch ( layerIndex ) {
							case 1: {
								for ( uint Y=0; Y < size; Y++ ) {
									for ( uint X=0; X < size; X++ ) {
										float	V0 = alignedLayers[X,Y].x;	// Already aligned
										float	V1 = layer[(X+offsetX)&mask,(Y+offsetY)&mask];
										float	delta = Math.Abs( V0 - V1 );
										score += Math.Exp( exponentialFactor * delta );
									}
								}
								break;
							}

							case 2: {
								for ( uint Y=0; Y < size; Y++ ) {
									for ( uint X=0; X < size; X++ ) {
										float	V0 = alignedLayers[X,Y].x;	// Already aligned
										float	V1 = alignedLayers[X,Y].y;	// Already aligned
										float	V2 = layer[(X+offsetX)&mask,(Y+offsetY)&mask];
										float	delta0 = Math.Abs( V2 - V0 );
										float	delta1 = Math.Abs( V2 - V1 );
										score += Math.Exp( exponentialFactor * (Math.Pow( delta0, 1.5 ) + Math.Pow( delta1, 1.5 )) );
									}
								}
								break;
							}

							case 3: {
								for ( uint Y=0; Y < size; Y++ ) {
									for ( uint X=0; X < size; X++ ) {
										float	V0 = alignedLayers[X,Y].x;	// Already aligned
										float	V1 = alignedLayers[X,Y].y;	// Already aligned
										float	V2 = alignedLayers[X,Y].z;	// Already aligned
										float	V3 = layer[(X+offsetX)&mask,(Y+offsetY)&mask];
										float	delta0 = V3 - V0;
										float	delta1 = V3 - V1;
										float	delta2 = V3 - V2;
										score += Math.Exp( exponentialFactor * (delta0*delta0 + delta1*delta1 + delta2*delta2) );
									}
								}
								break;
							}
						}
						score /= size*size;	// Not necessary but easier to debug...

						// Keep best score and position where it was found
						if ( score < minScore ) {
							// Better!
							minScore = score;
							bestOffsetX = offsetX;
							bestOffsetY = offsetY;
						}
					}
				}

				// Write the layer with the best selected offset
				switch ( layerIndex ) {
					case 1:
						for ( uint Y=0; Y < size; Y++ ) {
							for ( uint X=0; X < size; X++ ) {
								alignedLayers[X,Y].y = layer[(X+bestOffsetX)&mask,(Y+bestOffsetY)&mask];
							}
						}
						break;
					case 2:
						for ( uint Y=0; Y < size; Y++ ) {
							for ( uint X=0; X < size; X++ ) {
								alignedLayers[X,Y].z = layer[(X+bestOffsetX)&mask,(Y+bestOffsetY)&mask];
							}
						}
						break;
					case 3:
						for ( uint Y=0; Y < size; Y++ ) {
							for ( uint X=0; X < size; X++ ) {
								alignedLayers[X,Y].w = layer[(X+bestOffsetX)&mask,(Y+bestOffsetY)&mask];
							}
						}
						break;
				}
			}

			// Create resulting image
			PIXEL_FORMAT	format = PIXEL_FORMAT.UNKNOWN;
			switch ( layersCount ) {
				case 2: format = PIXEL_FORMAT.RGB8; break;		// FreeImage doesn't support R8G8! It believes we're specifying R5G6B5! :(
				case 3: format = PIXEL_FORMAT.RGB8; break;
				case 4: format = PIXEL_FORMAT.RGBA8; break;
			}
			ImageFile	result = new ImageFile( size, size, format, new ColorProfile( ColorProfile.STANDARD_PROFILE.sRGB ) );
			result.WritePixels( ( uint X, uint Y, ref float4 _color ) => { _color = alignedLayers[X,Y]; } );

			return result;
		}
	}
}