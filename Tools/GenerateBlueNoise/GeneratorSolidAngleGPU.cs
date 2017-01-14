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

		const int	MAX_SWAPPED_ELEMENTS_PER_ITERATION = 4;

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

// 		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
// 		struct SB_Score {
// 			public uint		scoreLo;
// 			public uint		scoreHi;
// 		}

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
		ConstantBuffer<CB_Mutation>	m_CB_Mutation;

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
				m_CB_Mutation = new ConstantBuffer<CB_Mutation>( _device, 1 );

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

			m_CB_Mutation.Dispose();
			m_CB_Main.Dispose();

			m_CS_AccumulateScore.Dispose();
			m_CS_ComputeScore1D.Dispose();
			m_CS_Mutate.Dispose();
			m_CS_Copy.Dispose();
		}

		#endregion

		public delegate void	ProgressDelegate( int _iterationIndex, float _energyScore, float[,] _texture );

		/// <summary>
		/// Generates blue noise distribution by randomly swapping pixels in the texture to reach lowest possible score and minimize a specific energy function
		/// </summary>
		/// <param name="_randomSeed"></param>
		/// <param name="_minEnergyThreshold"></param>
		/// <param name="_maxIterations"></param>
		/// <param name="_standardDeviationImage">Standard deviation for image space. If not sure, use 2.1</param>
		/// <param name="_standardDeviationValue">Standard deviation for value space. If not sure, use 1.0</param>
		/// <param name="_progress"></param>
		public void		Generate( uint _randomSeed, float _minEnergyThreshold, int _maxIterations, float _standardDeviationImage, float _standardDeviationValue, int _notifyProgressEveryNIterations, ProgressDelegate _progress ) {

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
				PixelsBuffer	pixels = m_texNoiseCPU.MapWrite( 0, 0 );
				using ( System.IO.BinaryWriter W = pixels.OpenStreamWrite() ) {
					for ( int Y=0; Y < m_textureSize; Y++ )
						for ( int X=0; X < m_textureSize; X++ ) {
							W.Write( (float) SimpleRNG.GetUniform() );
						}
				}
				m_texNoiseCPU.UnMap( pixels );
				m_texNoise0.CopyFrom( m_texNoiseCPU );
			}

			//////////////////////////////////////////////////////////////////////////
			// Perform iterations
			float		bestScore = float.MaxValue;//ComputeScore1D( m_texNoise0 );
			int			iterationIndex = 0;
			uint[,]		sourceTargetIndices = new uint[MAX_SWAPPED_ELEMENTS_PER_ITERATION,2];
			float[,]	textureCPU = new float[m_textureSize,m_textureSize];
			while ( iterationIndex < _maxIterations && bestScore > _minEnergyThreshold ) {

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
					for ( int swapCount=0; swapCount < MAX_SWAPPED_ELEMENTS_PER_ITERATION; swapCount++ ) {
						uint	sourceIndex = GetUniformInt( m_textureTotalSize );
						uint	targetIndex = GetUniformInt( m_textureTotalSize );

						sourceTargetIndices[swapCount,0] = sourceIndex;
						sourceTargetIndices[swapCount,1] = targetIndex;
					}

					ComputeXYFromSingleIndex( sourceTargetIndices[0,0], out m_CB_Mutation.m._pixelSourceX0, out m_CB_Mutation.m._pixelSourceY0 );
					ComputeXYFromSingleIndex( sourceTargetIndices[0,1], out m_CB_Mutation.m._pixelTargetX0, out m_CB_Mutation.m._pixelTargetY0 );
					ComputeXYFromSingleIndex( sourceTargetIndices[1,0], out m_CB_Mutation.m._pixelSourceX1, out m_CB_Mutation.m._pixelSourceY1 );
					ComputeXYFromSingleIndex( sourceTargetIndices[1,1], out m_CB_Mutation.m._pixelTargetX1, out m_CB_Mutation.m._pixelTargetY1 );
					ComputeXYFromSingleIndex( sourceTargetIndices[2,0], out m_CB_Mutation.m._pixelSourceX2, out m_CB_Mutation.m._pixelSourceY2 );
					ComputeXYFromSingleIndex( sourceTargetIndices[2,1], out m_CB_Mutation.m._pixelTargetX2, out m_CB_Mutation.m._pixelTargetY2 );
					ComputeXYFromSingleIndex( sourceTargetIndices[3,0], out m_CB_Mutation.m._pixelSourceX3, out m_CB_Mutation.m._pixelSourceY3 );
					ComputeXYFromSingleIndex( sourceTargetIndices[3,1], out m_CB_Mutation.m._pixelTargetX3, out m_CB_Mutation.m._pixelTargetY3 );

					m_CB_Mutation.UpdateData();

					m_CS_Mutate.Dispatch( MAX_SWAPPED_ELEMENTS_PER_ITERATION, 1, 1 );

					m_texNoise0.RemoveFromLastAssignedSlots();
					m_texNoise1.RemoveFromLastAssignedSlotUAV();
				}

				//////////////////////////////////////////////////////////////////////////
				// Compute new score
				float	score = ComputeScore1D( m_texNoise1 );
				if ( score < bestScore ) {
					// New best score! Swap textures...
					bestScore = score;
					Texture2D	temp = m_texNoise0;
					m_texNoise0 = m_texNoise1;
					m_texNoise1 = temp;
				}

				//////////////////////////////////////////////////////////////////////////
				// Notify
				iterationIndex++;
				if ( _progress == null || (iterationIndex % _notifyProgressEveryNIterations) != 1 )
					continue;
				
//				ReadBackTexture1D( m_texNoise0, textureCPU );
ReadBackScoreTexture1D( m_texNoiseScore, textureCPU );
				_progress( iterationIndex, bestScore, textureCPU );	// Notify!
			}

			// One final call with our best final result
			_progress( iterationIndex, bestScore, textureCPU );
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

			if ( m_CS_AccumulateScore.Use() ) {
				// 2] Downsample from mip 0 to mip 4
				View2D	sourceView = m_texNoiseScore.GetView( 0, 1, 0, 1 );
				View2D	targetView = m_texNoiseScore.GetView( 4, 1, 0, 1 );
				m_texNoiseScore.SetCS( 0, sourceView );
				m_texNoiseScore.SetCSUAV( 0, targetView );

				uint	groupsCount = m_textureSize >> 4;	// Each group is using 8x8 threads that each read 2x2 values, thus each group is effectively covering 16x16 pixels
				m_CS_AccumulateScore.Dispatch( groupsCount, groupsCount, 1 );

				m_texNoiseScore.RemoveFromLastAssignedSlots();
				m_texNoiseScore.RemoveFromLastAssignedSlotUAV();

				// 3] Downsample from mip 4 to mip 8
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
			PixelsBuffer	lastPixel = m_texNoiseScoreCPU.MapRead( 8, 0 );
			float			score = 0.0f;
			using ( System.IO.BinaryReader R = lastPixel.OpenStreamRead() )
				score = R.ReadSingle();
			m_texNoiseScoreCPU.UnMap( lastPixel );

			score /= m_textureSize*m_textureSize;

			return score;
		}

		/// <summary>
		/// Reads the specified GPU 1D-valued texture back to the CPU
		/// </summary>
		/// <param name="_textureGPU"></param>
		/// <param name="_textureCPU"></param>
		void	ReadBackTexture1D( Texture2D _textureGPU, float[,] _textureCPU ) {
			m_texNoiseCPU.CopyFrom( _textureGPU );

			PixelsBuffer	pixels = m_texNoiseCPU.MapRead( 0, 0 );
			using ( System.IO.BinaryReader R = pixels.OpenStreamRead() ) {
				for ( int Y=0; Y < m_textureSize; Y++ )
					for ( int X=0; X < m_textureSize; X++ ) {
						_textureCPU[X,Y] = R.ReadSingle();
					}
			}
			m_texNoiseCPU.UnMap( pixels );
		}

		/// <summary>
		/// Reads the specified GPU 1D-valued texture back to the CPU
		/// </summary>
		/// <param name="_textureGPU"></param>
		/// <param name="_textureCPU"></param>
		void	ReadBackScoreTexture1D( Texture2D _textureGPU, float[,] _textureCPU ) {
			m_texNoiseScoreCPU.CopyFrom( _textureGPU );

			PixelsBuffer	pixels = m_texNoiseScoreCPU.MapRead( 0, 0 );
			using ( System.IO.BinaryReader R = pixels.OpenStreamRead() ) {
				for ( int Y=0; Y < m_textureSize; Y++ )
					for ( int X=0; X < m_textureSize; X++ ) {
						_textureCPU[X,Y] = R.ReadSingle();
					}
			}
			m_texNoiseScoreCPU.UnMap( pixels );

			float[,]	bisou = new float[16,16];
			pixels = m_texNoiseScoreCPU.MapRead( 4, 0 );
			using ( System.IO.BinaryReader R = pixels.OpenStreamRead() ) {
				for ( int Y=0; Y < m_textureSize >> 4; Y++ )
					for ( int X=0; X < m_textureSize >> 4; X++ ) {
						bisou[X,Y] = R.ReadSingle();
					}
			}
			m_texNoiseScoreCPU.UnMap( pixels );
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
