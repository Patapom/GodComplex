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

		const uint	MAX_MUTATIONS = 1024;

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		struct CB_Main {
			public uint		_texturePOT;
			public uint		_textureSize;
			public uint		_textureMask;

			public float	_kernelFactorSpatial;	// = 1/sigma_i²
			public float	_kernelFactorValue;		// = 1/sigma_s²
		}
		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		struct CB_Mutation {
			public uint		_pixelSourceX0;
			public uint		_pixelSourceX1;
			public uint		_pixelSourceX2;
			public uint		_pixelSourceX3;
			public uint		_pixelSourceY0;
			public uint		_pixelSourceY1;
			public uint		_pixelSourceY2;
			public uint		_pixelSourceY3;

			public uint		_pixelTargetX0;
			public uint		_pixelTargetX1;
			public uint		_pixelTargetX2;
			public uint		_pixelTargetX3;
			public uint		_pixelTargetY0;
			public uint		_pixelTargetY1;
			public uint		_pixelTargetY2;
			public uint		_pixelTargetY3;
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

		// GPU-structures
		ComputeShader	m_CS_Copy;							// Simply copies a source texture into a target texture
		ComputeShader	m_CS_Mutate;						// Mutates the current distribution into a new, slightly different one
		ComputeShader	m_CS_ComputeScore1D;				// Computes the simulation's "energy score"
		ComputeShader	m_CS_AccumulateScore;				// Accumulates the score from a (multiple of) 16x16 texture down to a (multiple of) 1x1 texture

		ConstantBuffer<CB_Main>		m_CB_Main;
//		ConstantBuffer<CB_Mutation>	m_CB_Mutation;
		StructuredBuffer<SB_Mutation>	m_SB_Mutations;

		Texture2D		m_texNoise0 = null;					// The textures that are flipped on each turn
		Texture2D		m_texNoise1 = null;
		Texture2D		m_texNoiseScore = null;
		Texture2D		m_texNoiseCPU = null;				// Used to upload/download results
		Texture2D		m_texNoiseScoreCPU = null;

		public	GeneratorSolidAngleGPU( Device _device, uint _texturePOT ) {
			m_texturePOT = (int) _texturePOT;
			m_textureSize = 1U << m_texturePOT;
			m_textureSizeMask = m_textureSize - 1;
			m_textureTotalSize = m_textureSize * m_textureSize;

			try {
				m_CS_Copy = new ComputeShader( _device, new System.IO.FileInfo( @"Shaders/SimulatedAnnealing.hlsl" ), "CS__Copy", null );
				m_CS_Mutate = new ComputeShader( _device, new System.IO.FileInfo( @"Shaders/SimulatedAnnealing.hlsl" ), "CS__Mutate", null );
				m_CS_ComputeScore1D = new ComputeShader( _device, new System.IO.FileInfo( @"Shaders/SimulatedAnnealing.hlsl" ), "CS__ComputeScore1D", null );
				m_CS_AccumulateScore = new ComputeShader( _device, new System.IO.FileInfo( @"Shaders/SimulatedAnnealing.hlsl" ), "CS__AccumulateScore16", null );

				m_CB_Main = new ConstantBuffer<CB_Main>( _device, 0 );
//				m_CB_Mutation = new ConstantBuffer<CB_Mutation>( _device, 1 );
				m_SB_Mutations = new StructuredBuffer<SB_Mutation>( _device, MAX_MUTATIONS, true );

				m_texNoise0 = new Texture2D( _device, m_textureSize, m_textureSize, 1, 1, PIXEL_FORMAT.R32_FLOAT, false, true, null );
				m_texNoise1 = new Texture2D( _device, m_textureSize, m_textureSize, 1, 1, PIXEL_FORMAT.R32_FLOAT, false, true, null );
				m_texNoiseCPU = new Texture2D( _device, m_textureSize, m_textureSize, 1, 1, PIXEL_FORMAT.R32_FLOAT, true, true, null );
				m_texNoiseScore = new Texture2D( _device, m_textureSize, m_textureSize, 1, 1+_texturePOT, PIXEL_FORMAT.R32_FLOAT, false, true, null );
				m_texNoiseScoreCPU = new Texture2D( _device, m_textureSize, m_textureSize, 1, 1+_texturePOT, PIXEL_FORMAT.R32_FLOAT, true, true, null );
			} catch ( Exception _e ) {
				throw new Exception( "An error occurred while creating DirectX structures: " + _e.Message, _e );
			}
		}

		#region IDisposable Members

		public void Dispose() {
			m_texNoiseScoreCPU.Dispose();
			m_texNoiseScore.Dispose();
			m_texNoiseCPU.Dispose();
			m_texNoise1.Dispose();
			m_texNoise0.Dispose();

			m_SB_Mutations.Dispose();
//			m_CB_Mutation.Dispose();
			m_CB_Main.Dispose();

			m_CS_AccumulateScore.Dispose();
			m_CS_ComputeScore1D.Dispose();
			m_CS_Mutate.Dispose();
			m_CS_Copy.Dispose();
		}

		#endregion

		public delegate void	ProgressDelegate( uint _iterationIndex, uint _mutationsCount, float _energyScore, float[,] _texture, List<float> _statistics );

		/// <summary>
		/// Generates blue noise distribution by randomly swapping pixels in the texture to reach lowest possible score and minimize a specific energy function
		/// </summary>
		/// <param name="_randomSeed"></param>
		/// <param name="_energyThreshold">The threshold below which the amount of mutations is decreased, the loop exits if the amount of mutation is 0. A default value can be 1e-3f</param>
		/// <param name="_maxIterations">The maximum amount of iterations before exiting with the last best solution</param>
		/// <param name="_standardDeviationImage">Standard deviation for image space. If not sure, use 2.1</param>
		/// <param name="_standardDeviationValue">Standard deviation for value space. If not sure, use 1.0</param>
		/// <param name="_notifyProgressEveryNIterations">Will read back the GPU texture to the CPU and notify of progress every N iterations</param>
		/// <param name="_progress"></param>
		public void		Generate( uint _randomSeed, float _energyThreshold, int _maxIterations, float _standardDeviationImage, float _standardDeviationValue, int _notifyProgressEveryNIterations, ProgressDelegate _progress ) {

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
				m_texNoiseCPU.WritePixels( 0, 0, ( uint _X, uint _Y, System.IO.BinaryWriter _W ) => {
					_W.Write( (float) SimpleRNG.GetUniform() );
				} );
				m_texNoise0.CopyFrom( m_texNoiseCPU );
			}

			//////////////////////////////////////////////////////////////////////////
			// Perform iterations
			float		bestScore = ComputeScore1D( m_texNoise0 );
			float		score = bestScore;
			uint		iterationIndex = 0;
			uint		mutationsCount = MAX_MUTATIONS;
			int			iterationsCountWithoutImprovement = 0;
//			float		maxIterationsCountWithoutImprovementBeforeDecreasingMutationsCount = 0.01f * m_textureTotalSize;	// Arbitrary: 1% of the texture size
			float		maxIterationsCountWithoutImprovementBeforeDecreasingMutationsCount = 0.002f * m_textureTotalSize;	// Arbitrary: 0.2% of the texture size
			float		averageIterationsCountWithoutImprovement = 0.0f;
			float		alpha = 0.001f;

			uint[]		neighborOffsetX = new uint[8] { 0, 1, 2, 2, 2, 1, 0, 0 };
			uint[]		neighborOffsetY = new uint[8] { 0, 0, 0, 1, 2, 2, 2, 1 };

			List<float>	statistics = new List<float>();

			float[,]	textureCPU = new float[m_textureSize,m_textureSize];
			while ( iterationIndex < _maxIterations && bestScore > _energyThreshold ) {

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
					for ( int mutationIndex=0; mutationIndex < mutationsCount; mutationIndex++ ) {
						#if CAILLOU
							// Swap pixels randomly
							uint	sourceIndex = GetUniformInt( m_textureTotalSize );
							uint	targetIndex = GetUniformInt( m_textureTotalSize );

							ComputeXYFromSingleIndex( sourceIndex, out m_SB_Mutations.m[mutationIndex]._pixelSourceX, out m_SB_Mutations.m[mutationIndex]._pixelSourceY );
							ComputeXYFromSingleIndex( targetIndex, out m_SB_Mutations.m[mutationIndex]._pixelTargetX, out m_SB_Mutations.m[mutationIndex]._pixelTargetY );
						#else
							// Swap neighbor pixels only
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
						#endif
					}

					m_SB_Mutations.Write( mutationsCount );
					m_SB_Mutations.SetInput( 1 );

// 					for ( int swapCount=0; swapCount < MAX_SWAPPED_ELEMENTS_PER_ITERATION; swapCount++ ) {
// 						uint	sourceIndex = GetUniformInt( m_textureTotalSize );
// 						uint	targetIndex = GetUniformInt( m_textureTotalSize );
// 
// 						sourceTargetIndices[swapCount,0] = sourceIndex;
// 						sourceTargetIndices[swapCount,1] = targetIndex;
// 					}
// 
// 					ComputeXYFromSingleIndex( sourceTargetIndices[0,0], out m_CB_Mutation.m._pixelSourceX0, out m_CB_Mutation.m._pixelSourceY0 );
// 					ComputeXYFromSingleIndex( sourceTargetIndices[0,1], out m_CB_Mutation.m._pixelTargetX0, out m_CB_Mutation.m._pixelTargetY0 );
// 					ComputeXYFromSingleIndex( sourceTargetIndices[1,0], out m_CB_Mutation.m._pixelSourceX1, out m_CB_Mutation.m._pixelSourceY1 );
// 					ComputeXYFromSingleIndex( sourceTargetIndices[1,1], out m_CB_Mutation.m._pixelTargetX1, out m_CB_Mutation.m._pixelTargetY1 );
// 					ComputeXYFromSingleIndex( sourceTargetIndices[2,0], out m_CB_Mutation.m._pixelSourceX2, out m_CB_Mutation.m._pixelSourceY2 );
// 					ComputeXYFromSingleIndex( sourceTargetIndices[2,1], out m_CB_Mutation.m._pixelTargetX2, out m_CB_Mutation.m._pixelTargetY2 );
// 					ComputeXYFromSingleIndex( sourceTargetIndices[3,0], out m_CB_Mutation.m._pixelSourceX3, out m_CB_Mutation.m._pixelSourceY3 );
// 					ComputeXYFromSingleIndex( sourceTargetIndices[3,1], out m_CB_Mutation.m._pixelTargetX3, out m_CB_Mutation.m._pixelTargetY3 );
// 
// 					m_CB_Mutation.UpdateData();

					m_CS_Mutate.Dispatch( mutationsCount, 1, 1 );

					m_texNoise0.RemoveFromLastAssignedSlots();
					m_texNoise1.RemoveFromLastAssignedSlotUAV();
				}

				//////////////////////////////////////////////////////////////////////////
				// Compute new score
				float	previousScore = score;
				score = ComputeScore1D( m_texNoise1 );
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
					mutationsCount >>= 1;	// Halve mutations count
					if ( mutationsCount == 0 )
						break;	// Clearly we've reached a steady state here...
				}

//statistics.Add( averageIterationsCountWithoutImprovement );


				//////////////////////////////////////////////////////////////////////////
				// Notify
				iterationIndex++;
				if ( _progress == null || (iterationIndex % _notifyProgressEveryNIterations) != 1 )
					continue;
				
				ReadBackTexture1D( m_texNoise0, textureCPU );
//ReadBackScoreTexture1D( m_texNoiseScore, textureCPU );
				_progress( iterationIndex, mutationsCount, bestScore, textureCPU, statistics );	// Notify!
			}

			// One final call with our best final result
			_progress( iterationIndex, mutationsCount, bestScore, textureCPU, statistics );
		}

		float	ComputeScore1D( Texture2D _texture ) {
			// 1] Compute score for each texel and store it into mip 0
			if ( m_CS_ComputeScore1D.Use() ) {
				uint	groupsCount = m_textureSize >> 4;	// Each group is using 16x16 threads

				_texture.SetCS( 0 );
				m_texNoiseScore.SetCSUAV( 0 );

				m_CS_ComputeScore1D.Dispatch( groupsCount, groupsCount, 1 );

				_texture.RemoveFromLastAssignedSlots();
				m_texNoiseScore.RemoveFromLastAssignedSlotUAV();
			}

			// 2] Accumulate scores of each individual pixels into a single final score
			if ( m_CS_AccumulateScore.Use() ) {
				// 2.1) Downsample from mip 0 to mip 4
				View2D	sourceView = m_texNoiseScore.GetView( 0, 1, 0, 1 );
				View2D	targetView = m_texNoiseScore.GetView( 4, 1, 0, 1 );
				m_texNoiseScore.SetCS( 0, sourceView );
				m_texNoiseScore.SetCSUAV( 0, targetView );

				uint	groupsCount = m_textureSize >> 4;	// Each group is using 8x8 threads that each read 2x2 values, thus each group is effectively covering 16x16 pixels
				m_CS_AccumulateScore.Dispatch( groupsCount, groupsCount, 1 );

				m_texNoiseScore.RemoveFromLastAssignedSlots();
				m_texNoiseScore.RemoveFromLastAssignedSlotUAV();

				// 2.2) Downsample from mip 4 to mip 8
				sourceView = targetView;
				targetView = m_texNoiseScore.GetView( 8, 1, 0, 1 );
				m_texNoiseScore.SetCS( 0, sourceView );
				m_texNoiseScore.SetCSUAV( 0, targetView );

				groupsCount >>= 4;	// Should be 1!
				m_CS_AccumulateScore.Dispatch( groupsCount, groupsCount, 1 );

				m_texNoiseScore.RemoveFromLastAssignedSlots();
				m_texNoiseScore.RemoveFromLastAssignedSlotUAV();
			}

			// Copy to CPU
			m_texNoiseScoreCPU.CopyFrom( m_texNoiseScore );
			float	score = 0.0f;
			m_texNoiseScoreCPU.ReadPixels( 8, 0, ( uint _X, uint _Y, System.IO.BinaryReader _R ) => {
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
		void	ReadBackTexture1D( Texture2D _textureGPU, float[,] _textureCPU ) {
			m_texNoiseCPU.CopyFrom( _textureGPU );
			m_texNoiseCPU.ReadPixels( 0, 0, ( uint _X, uint _Y, System.IO.BinaryReader _R ) => {
				_textureCPU[_X,_Y] = _R.ReadSingle();
			} );
		}

		/// <summary>
		/// Reads the specified GPU 1D-valued texture back to the CPU
		/// </summary>
		/// <param name="_textureGPU"></param>
		/// <param name="_textureCPU"></param>
		void	ReadBackScoreTexture1D( Texture2D _textureGPU, float[,] _textureCPU ) {
			m_texNoiseScoreCPU.CopyFrom( _textureGPU );
			m_texNoiseScoreCPU.ReadPixels( 0, 0, ( uint _X, uint _Y, System.IO.BinaryReader _R ) => {
				_textureCPU[_X,_Y] = _R.ReadSingle();
			} );

// 			float[,]	bisou = new float[16,16];
// 			m_texNoiseScoreCPU.ReadPixels( 4, 0, ( uint _X, uint _Y, System.IO.BinaryReader _R ) => {
// 				bisou[_X,_Y] = _R.ReadSingle();
// 			} );
// 
// 			float	finalMip = 0.0f;
// 			m_texNoiseScoreCPU.ReadPixels( 8, 0, ( uint _X, uint _Y, System.IO.BinaryReader _R ) => {
// 				finalMip = _R.ReadSingle();
// 			} );
		}

		void	ComputeXYFromSingleIndex( uint _index, out uint _X, out uint _Y ) {
			_X = _index & m_textureSizeMask;
			_Y = _index >> m_texturePOT;
		}

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
