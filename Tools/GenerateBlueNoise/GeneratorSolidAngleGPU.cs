#define MULTI_MIPS
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
using Renderer;

namespace GenerateBlueNoise
{
	/// <summary>
	/// Implements the Blue Noise genration algorithm described by "Blue-noise Dithered Sampling" Georgiev, Fajardo
	/// https://www.solidangle.com/research/dither_abstract.pdf
	/// 
	/// Simulated Annealing references:
	///		• http://katrinaeg.com/simulated-annealing.html
	///		• http://mathworld.wolfram.com/SimulatedAnnealing.html
	///		• http://www.mit.edu/~dbertsim/papers/Optimization/Simulated%20annealing.pdf
	/// </summary>
	public class GeneratorSolidAngleGPU : IDisposable {

		#region FIELDS

		const uint	MAX_MUTATIONS_RATE = 4096;

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		struct CB_Main {
			public uint		_texturePOT;
			public uint		_textureSize;
			public uint		_textureMask;

			public float	_kernelFactorSpatial;	// = 1/sigma_i²
			public float	_kernelFactorValue;		// = 1/sigma_s²
		}
		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		struct CB_Mips {
			public uint		_textureMipSource;
			public uint		_textureMipTarget;
		}
		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		struct SB_Mutation {
			public uint		_pixelSourceX;
			public uint		_pixelSourceY;
			public uint		_pixelTargetX;
			public uint		_pixelTargetY;
		}

		int				m_texturePOT;
		uint			m_textureSize;
		uint			m_textureSizeMask;
		uint			m_textureTotalSize;
		uint			m_vectorDimension;

		// GPU-structures
		ComputeShader	m_CS_Copy;							// Simply copies a source texture into a target texture
		ComputeShader	m_CS_Mutate;						// Mutates the current distribution into a new, slightly different one
		ComputeShader	m_CS_ComputeScore;					// Computes the simulation's "energy score"
		ComputeShader	m_CS_AccumulateScore16;				// Accumulates the score from a (multiple of) 16x16 texture down to a (multiple of) 1x1 texture
		ComputeShader	m_CS_AccumulateScore8;				// Accumulates the score from a (multiple of) 8x8 texture down to a (multiple of) 1x1 texture
		ComputeShader	m_CS_AccumulateScore4;				// Accumulates the score from a (multiple of) 4x4 texture down to a (multiple of) 1x1 texture
		ComputeShader	m_CS_AccumulateScore2;				// Accumulates the score from a (multiple of) 2x2 texture down to a (multiple of) 1x1 texture

		ConstantBuffer<CB_Main>			m_CB_Main;
		ConstantBuffer<CB_Mips>			m_CB_Mips;
		StructuredBuffer<SB_Mutation>	m_SB_Mutations;

		Texture2D		m_texNoise0 = null;					// The textures that are flipped on each turn
		Texture2D		m_texNoise1 = null;
		Texture2D		m_texNoiseScore = null;
		Texture2D		m_texNoiseScore2 = null;
		Texture2D		m_texNoiseCPU = null;				// Used to upload/download results
		Texture2D		m_texNoiseScoreCPU = null;

		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="_device">Device CAN be null, in which case the CPU version will be used</param>
		/// <param name="_texturePOT">Power-of-Two texture size</param>
		/// <param name="_dimensions">Dimensions of the noise vectors (1 for monochromatic noise, etc.)</param>
		public	GeneratorSolidAngleGPU( Device _device, uint _texturePOT, uint _dimensions ) {
			m_texturePOT = (int) _texturePOT;
			m_textureSize = 1U << m_texturePOT;
			m_textureSizeMask = m_textureSize - 1;
			m_textureTotalSize = m_textureSize * m_textureSize;
			m_vectorDimension = _dimensions;

			try {

				m_CS_Copy = new ComputeShader( _device, new System.IO.FileInfo( @"Shaders/SimulatedAnnealing.hlsl" ), "CS__Copy", null );
				m_CS_Mutate = new ComputeShader( _device, new System.IO.FileInfo( @"Shaders/SimulatedAnnealing.hlsl" ), "CS__Mutate", null );
				switch ( m_vectorDimension ) {
					case 1:	m_CS_ComputeScore = new ComputeShader( _device, new System.IO.FileInfo( @"Shaders/SimulatedAnnealing.hlsl" ), "CS__ComputeScore1D", null ); break;
					case 2:	m_CS_ComputeScore = new ComputeShader( _device, new System.IO.FileInfo( @"Shaders/SimulatedAnnealing.hlsl" ), "CS__ComputeScore2D", null ); break;
					default:
						throw new Exception( "Unsupported vector dimension!" );
				}
				m_CS_AccumulateScore16 = new ComputeShader( _device, new System.IO.FileInfo( @"Shaders/SimulatedAnnealing.hlsl" ), "CS__AccumulateScore16", null );
				m_CS_AccumulateScore8 = new ComputeShader( _device, new System.IO.FileInfo( @"Shaders/SimulatedAnnealing.hlsl" ), "CS__AccumulateScore8", null );
				m_CS_AccumulateScore4 = new ComputeShader( _device, new System.IO.FileInfo( @"Shaders/SimulatedAnnealing.hlsl" ), "CS__AccumulateScore4", null );
				m_CS_AccumulateScore2 = new ComputeShader( _device, new System.IO.FileInfo( @"Shaders/SimulatedAnnealing.hlsl" ), "CS__AccumulateScore2", null );

				m_CB_Main = new ConstantBuffer<CB_Main>( _device, 0 );
				m_CB_Mips = new ConstantBuffer<CB_Mips>( _device, 1 );
				m_SB_Mutations = new StructuredBuffer<SB_Mutation>( _device, MAX_MUTATIONS_RATE, true );

				PIXEL_FORMAT	noiseFormat = PIXEL_FORMAT.UNKNOWN;
				switch ( m_vectorDimension ) {
					case 1: noiseFormat = PIXEL_FORMAT.R32_FLOAT; break;
					case 2: noiseFormat = PIXEL_FORMAT.RG32_FLOAT; break;
				}

				m_texNoise0 = new Texture2D( _device, m_textureSize, m_textureSize, 1, 1, noiseFormat, false, true, null );
				m_texNoise1 = new Texture2D( _device, m_textureSize, m_textureSize, 1, 1, noiseFormat, false, true, null );
				m_texNoiseCPU = new Texture2D( _device, m_textureSize, m_textureSize, 1, 1, noiseFormat, true, true, null );

				m_texNoiseScore = new Texture2D( _device, m_textureSize, m_textureSize, 1, 1+_texturePOT, PIXEL_FORMAT.R32_FLOAT, false, true, null );
				m_texNoiseScore2 = new Texture2D( _device, m_textureSize, m_textureSize, 1, 1+_texturePOT, PIXEL_FORMAT.R32_FLOAT, false, true, null );
				m_texNoiseScoreCPU = new Texture2D( _device, m_textureSize, m_textureSize, 1, 1+_texturePOT, PIXEL_FORMAT.R32_FLOAT, true, true, null );
			} catch ( Exception _e ) {
				throw new Exception( "An error occurred while creating DirectX structures: " + _e.Message, _e );
			}
		}

		#region IDisposable Members

		public void Dispose() {
			m_texNoiseScoreCPU.Dispose();
			m_texNoiseScore2.Dispose();
			m_texNoiseScore.Dispose();
			m_texNoiseCPU.Dispose();
			m_texNoise1.Dispose();
			m_texNoise0.Dispose();

			m_SB_Mutations.Dispose();
			m_CB_Mips.Dispose();
			m_CB_Main.Dispose();

			m_CS_AccumulateScore2.Dispose();
			m_CS_AccumulateScore4.Dispose();
			m_CS_AccumulateScore8.Dispose();
			m_CS_AccumulateScore16.Dispose();
			m_CS_ComputeScore.Dispose();
			m_CS_Mutate.Dispose();
			m_CS_Copy.Dispose();
		}

		#endregion

		public delegate void	ProgressDelegate( uint _iterationIndex, uint _mutationsRate, float _energyScore, Array _texture, List<float> _statistics );

		#region GPU Version

		/// <summary>
		/// Generates blue noise distribution by randomly swapping pixels in the texture to reach lowest possible score and minimize a specific energy function
		/// </summary>
		/// <param name="_randomSeed"></param>
		/// <param name="_maxIterations">The maximum amount of iterations before exiting with the last best solution</param>
		/// <param name="_standardDeviationImage">Standard deviation for image space. If not sure, use 2.1</param>
		/// <param name="_standardDeviationValue">Standard deviation for value space. If not sure, use 1.0</param>
		/// <param name="_neighborsOnlyMutations">True to only authorize mutations of neighbor pixels, false to randomly mutate any pixel</param>
		/// <param name="_notifyProgressEveryNIterations">Will read back the GPU texture to the CPU and notify of progress every N iterations</param>
		/// <param name="_progress"></param>
		public void		Generate( uint _randomSeed, uint _maxIterations, float _standardDeviationImage, float _standardDeviationValue, bool _neighborsOnlyMutations, uint _notifyProgressEveryNIterations, ProgressDelegate _progress ) {
			m_CB_Main.m._texturePOT = (uint) m_texturePOT;
			m_CB_Main.m._textureSize = m_textureSize;
			m_CB_Main.m._textureMask = m_textureSizeMask;

			m_CB_Main.m._kernelFactorSpatial = -1.0f / (_standardDeviationImage * _standardDeviationImage);
			m_CB_Main.m._kernelFactorValue = -1.0f / (_standardDeviationValue * _standardDeviationValue);

			m_CB_Main.UpdateData();

			//////////////////////////////////////////////////////////////////////////
			// Generate initial white noise
			{
				SimpleRNG.SetSeed( _randomSeed, 362436069U );
				switch ( m_vectorDimension ) {
					case 1: {
						// Build ordered initial values
						float[,]	initialValues = new float[m_textureSize,m_textureSize];
						for ( uint Y=0; Y < m_textureSize; Y++ )
							for ( uint X=0; X < m_textureSize; X++ )
								initialValues[X,Y] = (float) (m_textureSize*Y+X) / m_textureTotalSize;

						// Displace them randomly
						for ( uint i=0; i < m_textureTotalSize; i++ ) {
							uint	startX = GetUniformInt( m_textureSize );
							uint	startY = GetUniformInt( m_textureSize );
							uint	endX = GetUniformInt( m_textureSize );
							uint	endY = GetUniformInt( m_textureSize );
							float	temp = initialValues[startX,startY];
							initialValues[startX,startY] = initialValues[endX,endY];
							initialValues[endX,endY] = temp;
						}

						m_texNoiseCPU.WritePixels( 0, 0, ( uint _X, uint _Y, System.IO.BinaryWriter _W ) => {
							_W.Write( initialValues[_X,_Y] );
						} );
						break;
					}

					case 2: {
						// Build ordered initial values
						float2[,]	initialValues = new float2[m_textureSize,m_textureSize];
						for ( uint Y=0; Y < m_textureSize; Y++ )
							for ( uint X=0; X < m_textureSize; X++ )
								initialValues[X,Y].Set( (float) (m_textureSize*Y+X) / m_textureTotalSize, (float) (m_textureSize*Y+X) / m_textureTotalSize );

						// Displace them randomly
						for ( uint i=0; i < m_textureTotalSize; i++ ) {
							uint	startX = GetUniformInt( m_textureSize );
							uint	startY = GetUniformInt( m_textureSize );
							uint	endX = GetUniformInt( m_textureSize );
							uint	endY = GetUniformInt( m_textureSize );
							float	temp = initialValues[startX,startY].x;
							initialValues[startX,startY].x = initialValues[endX,endY].x;
							initialValues[endX,endY].x = temp;

							startX = GetUniformInt( m_textureSize );
							startY = GetUniformInt( m_textureSize );
							endX = GetUniformInt( m_textureSize );
							endY = GetUniformInt( m_textureSize );
							temp = initialValues[startX,startY].y;
							initialValues[startX,startY].y = initialValues[endX,endY].y;
							initialValues[endX,endY].y = temp;
						}

						m_texNoiseCPU.WritePixels( 0, 0, ( uint _X, uint _Y, System.IO.BinaryWriter _W ) => {
							_W.Write( initialValues[_X,_Y].x );
							_W.Write( initialValues[_X,_Y].y );
						} );
						break;
					}
				}
				m_texNoise0.CopyFrom( m_texNoiseCPU );
			}

			//////////////////////////////////////////////////////////////////////////
			// Perform iterations
			float		bestScore = ComputeScore( m_texNoise0 );
			float		score = bestScore;
			uint		iterationIndex = 0;
			uint		mutationsRate = MAX_MUTATIONS_RATE;
			int			iterationsCountWithoutImprovement = 0;
#if !CAILLOU
			float		maxIterationsCountWithoutImprovementBeforeDecreasingMutationsCount = 0.1f * m_textureTotalSize;	// Arbitrary: 10% of the texture size
#else
			float		maxIterationsCountWithoutImprovementBeforeDecreasingMutationsCount = 0.01f * m_textureTotalSize;	// Arbitrary: 1% of the texture size
#endif
//			float		maxIterationsCountWithoutImprovementBeforeDecreasingMutationsCount = 0.002f * m_textureTotalSize;	// Arbitrary: 0.2% of the texture size
			float		averageIterationsCountWithoutImprovement = 0.0f;
			float		alpha = 0.001f;

			uint[]		neighborOffsetX = new uint[8] { 0, 1, 2, 2, 2, 1, 0, 0 };
			uint[]		neighborOffsetY = new uint[8] { 0, 0, 0, 1, 2, 2, 2, 1 };

			List<float>	statistics = new List<float>();
//ReadBackScoreTexture( m_texNoiseScore2, textureCPU );
			while ( iterationIndex < _maxIterations ) {

				//////////////////////////////////////////////////////////////////////////
				// Copy
				if ( m_CS_Copy.Use() ) {
					m_texNoise0.SetCS( 0 );
					m_texNoise1.SetCSUAV( 0 );

					uint	groupsCount = m_textureSize >> 4;
					m_CS_Copy.Dispatch( groupsCount, groupsCount, 1 );
				}

				//////////////////////////////////////////////////////////////////////////
				// Mutate current solution by swapping up to N pixels randomly
				if ( m_CS_Mutate.Use() ) {

					// Fill up mutations buffer
					if ( _neighborsOnlyMutations ) {
						// Swap neighbor pixels only
						for ( int mutationIndex=0; mutationIndex < mutationsRate; mutationIndex++ ) {
							uint	sourceIndex = GetUniformInt( m_textureTotalSize );
							uint	X, Y;
							ComputeXYFromSingleIndex( sourceIndex, out X, out Y );

							// Randomly pick one of the 8 neighbors
							uint	neighborIndex = SimpleRNG.GetUint() & 0x7;
							uint	Xn = (X + m_textureSizeMask + neighborOffsetX[neighborIndex]) & m_textureSizeMask;
							uint	Yn = (Y + m_textureSizeMask + neighborOffsetY[neighborIndex]) & m_textureSizeMask;

							m_SB_Mutations.m[mutationIndex]._pixelSourceX = X;
							m_SB_Mutations.m[mutationIndex]._pixelSourceY = Y;
							m_SB_Mutations.m[mutationIndex]._pixelTargetX = Xn;
							m_SB_Mutations.m[mutationIndex]._pixelTargetY = Yn;

							if ( m_vectorDimension > 1 ) {
								m_SB_Mutations.m[mutationIndex]._pixelTargetY |= SimpleRNG.GetUniform() > 0.5 ? 0x80000000U : 0x40000000U;
							}
						}
					} else {
						// Swap pixels randomly
						for ( int mutationIndex=0; mutationIndex < mutationsRate; mutationIndex++ ) {
							uint	sourceIndex = GetUniformInt( m_textureTotalSize );
							uint	targetIndex = GetUniformInt( m_textureTotalSize );
							ComputeXYFromSingleIndex( sourceIndex, out m_SB_Mutations.m[mutationIndex]._pixelSourceX, out m_SB_Mutations.m[mutationIndex]._pixelSourceY );
							ComputeXYFromSingleIndex( targetIndex, out m_SB_Mutations.m[mutationIndex]._pixelTargetX, out m_SB_Mutations.m[mutationIndex]._pixelTargetY );

							if ( m_vectorDimension > 1 ) {
								m_SB_Mutations.m[mutationIndex]._pixelTargetY |= SimpleRNG.GetUniform() > 0.5 ? 0x80000000U : 0x40000000U;
							}
						}
					}

					m_SB_Mutations.Write( mutationsRate );
					m_SB_Mutations.SetInput( 1 );

					m_CS_Mutate.Dispatch( mutationsRate, 1, 1 );

					m_texNoise0.RemoveFromLastAssignedSlots();
					m_texNoise1.RemoveFromLastAssignedSlotUAV();
				}

				//////////////////////////////////////////////////////////////////////////
				// Compute new score
				float	previousScore = score;
				score = ComputeScore( m_texNoise1 );
				if ( score < bestScore ) {
					// New best score! Swap textures so we accept the new state...
					bestScore = score;
					Texture2D	temp = m_texNoise0;
					m_texNoise0 = m_texNoise1;
					m_texNoise1 = temp;

					iterationsCountWithoutImprovement = 0;
				} else {
					iterationsCountWithoutImprovement++;
				}

				averageIterationsCountWithoutImprovement *= 1.0f - alpha;
				averageIterationsCountWithoutImprovement += alpha * iterationsCountWithoutImprovement;

				if ( averageIterationsCountWithoutImprovement > maxIterationsCountWithoutImprovementBeforeDecreasingMutationsCount ) {
					averageIterationsCountWithoutImprovement = 0.0f;	// Start over...
					mutationsRate >>= 1;	// Halve mutations count
					if ( mutationsRate == 0 )
						break;	// Clearly we've reached a steady state here...
				}

//statistics.Add( averageIterationsCountWithoutImprovement );


				//////////////////////////////////////////////////////////////////////////
				// Notify
				iterationIndex++;
				if ( _progress == null || (iterationIndex % _notifyProgressEveryNIterations) != 1 )
					continue;
				
//				_progress( iterationIndex, mutationsCount, bestScore, ReadBackScoreTexture( m_texNoiseScore ), statistics );	// Notify!
				switch ( m_vectorDimension ) {
					case 1: _progress( iterationIndex, mutationsRate, bestScore, ReadBackTexture1D( m_texNoise0 ), statistics ); break;	// Notify!
					case 2: _progress( iterationIndex, mutationsRate, bestScore, ReadBackTexture2D( m_texNoise0 ), statistics ); break;	// Notify!
				}
			}

			// One final call with our best final result
			switch ( m_vectorDimension ) {
				case 1: ReadBackTexture1D( m_texNoise0 ); break;
				case 2: ReadBackTexture2D( m_texNoise0 ); break;
			}

			if ( _progress != null ) {
//				_progress( iterationIndex, mutationsCount, bestScore, ReadBackScoreTexture( m_texNoiseScore ), statistics );	// Notify!
				switch ( m_vectorDimension ) {
					case 1: _progress( iterationIndex, mutationsRate, bestScore, ReadBackTexture1D( m_texNoise0 ), statistics ); break;	// Notify!
					case 2: _progress( iterationIndex, mutationsRate, bestScore, ReadBackTexture2D( m_texNoise0 ), statistics ); break;	// Notify!
				}
			}
		}

		float	ComputeScore( Texture2D _texture ) {
			// 1] Compute score for each texel and store it into mip 0
			if ( m_CS_ComputeScore.Use() ) {
				uint	groupsCount = m_textureSize >> 4;	// Each group is using 16x16 threads

				_texture.SetCS( 0 );
				m_texNoiseScore.SetCSUAV( 0 );

				m_CS_ComputeScore.Dispatch( groupsCount, groupsCount, 1 );

				_texture.RemoveFromLastAssignedSlots();
				m_texNoiseScore.RemoveFromLastAssignedSlotUAV();
			}

			// 2] Accumulate scores of each individual pixels into a single final score
#if MULTI_MIPS
			if ( m_CS_AccumulateScore16.Use() ) {
				// 2.1) Downsample from mip 0 to down 4 mips each time
// 				View2D	sourceView = null;
// 				View2D	targetView = m_texNoiseScore.GetView( 0, 1, 0, 1 );
				uint	mipLevelIndex = 0;
				uint	mipLevelsCount = (uint) m_texturePOT;
				uint	groupsCount = m_textureSize;

				m_CB_Mips.m._textureMipTarget = 0;

				while ( mipLevelsCount >= 4 ) {
					mipLevelIndex += 4;
					mipLevelsCount -= 4;
					groupsCount >>= 4;	// Each group is using 8x8 threads that each read 2x2 values, thus each group is effectively covering 16x16 pixels


// !!!IMPORTANT NOTE!!!
// It's confirmed: Compute Shaders don't always support individual mip SRVs when using the Load() or [] operator!
// One must always pass the entire range of mips for a SRV!!!
//
// 					sourceView = targetView;
// 					targetView = m_texNoiseScore.GetView( mipLevelIndex, 1, 0, 1 );
// 					m_texNoiseScore.SetCS( 0, sourceView );
// 					m_texNoiseScore.SetCSUAV( 0, targetView );

					m_texNoiseScore.SetCS( 0 );
					m_texNoiseScore2.SetCSUAV( 0, m_texNoiseScore2.GetView( mipLevelIndex, 1, 0, 1 ) );

					m_CB_Mips.m._textureMipSource = m_CB_Mips.m._textureMipTarget;
					m_CB_Mips.m._textureMipTarget = mipLevelIndex;
					m_CB_Mips.UpdateData();

					m_CS_AccumulateScore16.Dispatch( groupsCount, groupsCount, 1 );

					m_texNoiseScore.RemoveFromLastAssignedSlots();
					m_texNoiseScore2.RemoveFromLastAssignedSlotUAV();

					// Swap
					Texture2D	temp = m_texNoiseScore;
					m_texNoiseScore = m_texNoiseScore2;
					m_texNoiseScore2 = temp;
				}

				// 2.2) Downsample to last mip
				ComputeShader	CSLastMip = null;
				switch ( mipLevelsCount ) {
					case 3: CSLastMip = m_CS_AccumulateScore8; break;
					case 2: CSLastMip = m_CS_AccumulateScore4; break;
					case 1: CSLastMip = m_CS_AccumulateScore2; break;
				}
				if ( CSLastMip != null && CSLastMip.Use() ) {
// !!!IMPORTANT NOTE!!!
// It's confirmed: Compute Shaders don't always support individual mip SRVs when using the Load() or [] operator!
// One must always pass the entire range of mips for a SRV!!!
// 					sourceView = targetView;
// 					targetView = m_texNoiseScore.GetView( (uint) m_texturePOT, 1, 0, 1 );
// 					m_texNoiseScore.SetCS( 0, sourceView );
// 					m_texNoiseScore.SetCSUAV( 0, targetView );

					m_texNoiseScore.SetCS( 0 );
					m_texNoiseScore2.SetCSUAV( 0, m_texNoiseScore2.GetView( (uint) m_texturePOT, 1, 0, 1 ) );

					m_CB_Mips.m._textureMipSource = m_CB_Mips.m._textureMipTarget;
					m_CB_Mips.m._textureMipTarget = (uint) m_texturePOT;
					m_CB_Mips.UpdateData();

					CSLastMip.Dispatch( 1, 1, 1 );

					m_texNoiseScore.RemoveFromLastAssignedSlots();
					m_texNoiseScore2.RemoveFromLastAssignedSlotUAV();

					// Swap
					Texture2D	temp = m_texNoiseScore;
					m_texNoiseScore = m_texNoiseScore2;
					m_texNoiseScore2 = temp;
				}
			}
#elif GLOU
			if ( m_CS_AccumulateScore2.Use() ) {
				View2D	sourceView = null;
				View2D	targetView = m_texNoiseScore.GetView( 0, 1, 0, 1 );
				uint	mipLevelIndex = 0;
				uint	mipLevelsCount = (uint) m_texturePOT;
				uint	groupsCount = m_textureSize;

				while ( mipLevelsCount > 0 ) {
					mipLevelIndex++;
					mipLevelsCount--;
					groupsCount >>= 1;

					sourceView = targetView;
					targetView = m_texNoiseScore.GetView( mipLevelIndex, 1, 0, 1 );
					m_texNoiseScore.SetCS( 0, sourceView );
					m_texNoiseScore.SetCSUAV( 0, targetView );

					m_CS_AccumulateScore2.Dispatch( groupsCount, groupsCount, 1 );

					m_texNoiseScore.RemoveFromLastAssignedSlots();
					m_texNoiseScore.RemoveFromLastAssignedSlotUAV();
				}
			}
#else
			if ( m_CS_AccumulateScore2.Use() ) {
				uint	mipLevelIndex = 0;
				uint	mipLevelsCount = (uint) m_texturePOT;
				uint	groupsCount = m_textureSize;

				while ( mipLevelsCount > 0 ) {
					mipLevelIndex++;
					mipLevelsCount--;
					groupsCount >>= 1;

					m_texNoiseScore.SetCS(0);
					m_texNoiseScore2.SetCSUAV( 0, m_texNoiseScore2.GetView( mipLevelIndex, 1, 0, 1 ) );

					m_CB_Mips.m._textureMipSource = mipLevelIndex-1;
					m_CB_Mips.m._textureMipTarget = mipLevelIndex;
					m_CB_Mips.UpdateData();

					m_CS_AccumulateScore2.Dispatch( groupsCount, groupsCount, 1 );

					m_texNoiseScore.RemoveFromLastAssignedSlots();
					m_texNoiseScore2.RemoveFromLastAssignedSlotUAV();
					Texture2D	temp = m_texNoiseScore;
					m_texNoiseScore = m_texNoiseScore2;
					m_texNoiseScore2 = temp;
				}
			}
#endif

			// Copy to CPU
			m_texNoiseScoreCPU.CopyFrom( m_texNoiseScore );
			float	score = 0.0f;
			m_texNoiseScoreCPU.ReadPixels( (uint) m_texturePOT, 0, ( uint _X, uint _Y, System.IO.BinaryReader _R ) => {
				score = _R.ReadSingle();
			} );

			score /= m_textureSize*m_textureSize;

			return score;
		}

		/// <summary>
		/// Reads the specified 1D-valued GPU texture back to the CPU
		/// </summary>
		/// <param name="_textureGPU"></param>
		/// <param name="_textureCPU"></param>
		float[,]	m_textureCPU_1D = null;
		float[,]	ReadBackTexture1D( Texture2D _textureGPU ) {
			if ( m_textureCPU_1D == null )
				m_textureCPU_1D = new float[m_textureSize,m_textureSize];

			m_texNoiseCPU.CopyFrom( _textureGPU );
			m_texNoiseCPU.ReadPixels( 0, 0, ( uint _X, uint _Y, System.IO.BinaryReader _R ) => {
				m_textureCPU_1D[_X,_Y] = _R.ReadSingle();
			} );

			return m_textureCPU_1D;
		}

		/// <summary>
		/// Reads the specified 2D-valued GPU texture back to the CPU
		/// </summary>
		/// <param name="_textureGPU"></param>
		/// <param name="_textureCPU"></param>
		float2[,]	m_textureCPU_2D = null;
		float2[,]	ReadBackTexture2D( Texture2D _textureGPU ) {
			if ( m_textureCPU_2D == null )
				m_textureCPU_2D = new float2[m_textureSize,m_textureSize];

			m_texNoiseCPU.CopyFrom( _textureGPU );
			m_texNoiseCPU.ReadPixels( 0, 0, ( uint _X, uint _Y, System.IO.BinaryReader _R ) => {
				m_textureCPU_2D[_X,_Y].x = _R.ReadSingle();
				m_textureCPU_2D[_X,_Y].y = _R.ReadSingle();
			} );

			return m_textureCPU_2D;
		}

		/// <summary>
		/// Reads the specified GPU 1D-valued texture back to the CPU
		/// </summary>
		/// <param name="_textureGPU"></param>
		/// <param name="_textureCPU"></param>
		float[,]	ReadBackScoreTexture( Texture2D _textureGPU ) {
			if ( m_textureCPU_1D == null )
				m_textureCPU_1D = new float[m_textureSize,m_textureSize];

			m_texNoiseScoreCPU.CopyFrom( _textureGPU );
			m_texNoiseScoreCPU.ReadPixels( 0, 0, ( uint _X, uint _Y, System.IO.BinaryReader _R ) => {
				m_textureCPU_1D[_X,_Y] = _R.ReadSingle();
			} );

// 			float[,]	trump = new float[2,2];
// 			m_texNoiseScoreCPU.ReadPixels( 7, 0, ( uint _X, uint _Y, System.IO.BinaryReader _R ) => {
// 				trump[_X,_Y] = _R.ReadSingle();
// 			} );

			float[,]	bisou = new float[16,16];
			m_texNoiseScoreCPU.ReadPixels( (uint) m_texturePOT - 4, 0, ( uint _X, uint _Y, System.IO.BinaryReader _R ) => {
				bisou[_X,_Y] = _R.ReadSingle();
			} );

			float	finalMip = 0.0f;
			m_texNoiseScoreCPU.ReadPixels( (uint) m_texturePOT, 0, ( uint _X, uint _Y, System.IO.BinaryReader _R ) => {
				finalMip = _R.ReadSingle();
			} );

			return m_textureCPU_1D;
		}

		void	ComputeXYFromSingleIndex( uint _index, out uint _X, out uint _Y ) {
			_X = _index & m_textureSizeMask;
			_Y = _index >> m_texturePOT;
		}

		#endregion

		#region CPU Version

		#endregion

		/// <summary>
		/// Returns a value in [0,_maxValue[
		/// </summary>
		/// <param name="_maxValue"></param>
		/// <returns></returns>
		uint	GetUniformInt( uint _maxValue ) {
			ulong	value = (ulong) _maxValue * (ulong) SimpleRNG.GetUint();
					value >>= 32;
			return (uint) value;
		}
	}
}
