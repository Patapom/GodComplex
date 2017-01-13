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

namespace GenerateBlueNoise
{
	/// <summary>
	/// Implements the Blue Noise genration algorithm described by "Blue-noise Dithered Sampling" Georgiev, Fajardo
	/// https://www.solidangle.com/research/dither_abstract.pdf
	/// 
	/// This code was adapted from B.Wronski's implementation available at https://github.com/bartwronski/BlueNoiseGenerator
	///  although I could notice this wasn't really a correct implementation of simulated annealing, as advised in the Solid Angle's paper...
	/// 
	/// I rewrote the entire thing with the GPU in the GeneratorSolidAngleGPU class
	/// </summary>
	public class GeneratorSolidAngle {

		// Note: we try to swap between 1 and 3 elements to try to jump over local minimum
		const int	MAX_SWAPPED_ELEMENTS_PER_ITERATION = 3;

		const int	KERNEL_HALF_SIZE = 2;
		const int	KERNEL_SIZE = 1 + 2*KERNEL_HALF_SIZE;

		int				m_texturePOT;
		uint			m_textureSize;
		uint			m_textureSizeMask;
		uint			m_textureTotalSize;
		float[][,]		m_textures = new float[2][,];

		public	GeneratorSolidAngle( uint _texturePOT ) {
			m_texturePOT = (int) _texturePOT;
			m_textureSize = 1U << m_texturePOT;
			m_textureSizeMask = m_textureSize - 1;
			m_textureTotalSize = m_textureSize * m_textureSize;
			m_textures[0] = new float[m_textureSize,m_textureSize];
			m_textures[1] = new float[m_textureSize,m_textureSize];
		}

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
		public void		Generate( uint _randomSeed, float _minEnergyThreshold, int _maxIterations, float _standardDeviationImage, float _standardDeviationValue, ProgressDelegate _progress ) {

			m_kernelFactorImage = -1.0 / (_standardDeviationImage * _standardDeviationImage);
			m_kernelFactorValue = -1.0 / (_standardDeviationValue * _standardDeviationValue);

			// Generate initial white noise
			SimpleRNG.SetSeed( _randomSeed );
			for ( int Y=0; Y < m_textureSize; Y++ )
				for ( int X=0; X < m_textureSize; X++ ) {
					m_textures[0][X,Y] = (float) SimpleRNG.GetUniform();
				}

			// Perform iterations
			float	bestScore = ComputeScore( m_textures[0] );
			int		iterationIndex = 0;
			while ( iterationIndex < _maxIterations && bestScore > _minEnergyThreshold ) {

				// Copy source to target array
				Array.Copy( m_textures[0], m_textures[1], m_textureTotalSize );

				// Swap up to N pixels randomly
				for ( int swapCount=0; swapCount < MAX_SWAPPED_ELEMENTS_PER_ITERATION; swapCount++ ) {
					uint	sourceIndex = GetUniformInt( m_textureTotalSize );
					uint	targetIndex = sourceIndex;
					while ( targetIndex == sourceIndex )
						targetIndex = GetUniformInt( m_textureTotalSize );	// Make sure target index differs!

					float	temp = Get( m_textures[1], sourceIndex );
					Set( m_textures[1], sourceIndex, Get( m_textures[1], targetIndex ) );
					Set( m_textures[1], targetIndex, temp );
				}

				// Compute new score
				float	score = ComputeScore( m_textures[1] );
				if ( score < bestScore ) {
					// New best score! Swap textures...
					bestScore = score;
					float[,]	temp = m_textures[0];
					m_textures[0] = m_textures[1];
					m_textures[1] = temp;
				}

				iterationIndex++;
				if ( _progress != null ) {
					_progress( iterationIndex, bestScore, m_textures[0] );	// Notify!
				}
			}
		}

		double	m_kernelFactorImage;
		double	m_kernelFactorValue;

		/// <summary>
		/// Computes the energy score of the specified texture
		/// </summary>
		/// <param name="_texture"></param>
		/// <returns></returns>
		float	ComputeScore( float[,] _texture ) {
			double	sqDx, sqDy;
			double	centralValue, value;
			double	sqDistanceImage, sqDistanceValue;
			double	dScore;
			uint	finalX, finalY;

			double	score = 0.0;
			for ( int Y=0; Y < m_textureSize; Y++ ) {
				for ( int X=0; X < m_textureSize; X++ ) {

					centralValue = _texture[X,Y];

					// Apply kernel convolution
					for ( int dY=-KERNEL_HALF_SIZE; dY <= KERNEL_HALF_SIZE; dY++ ) {
						finalY = (uint) (Y + dY) & m_textureSizeMask;
						sqDy = dY * dY;
						for ( int dX=-KERNEL_HALF_SIZE; dX <= KERNEL_HALF_SIZE; dX++ ) {
							finalX = (uint) (X + dX) & m_textureSizeMask;
							sqDx = dX * dX;

							// Compute -|Pi - Qi|² / Sigma_i²
							sqDistanceImage = sqDx + sqDy;
							sqDistanceImage *= m_kernelFactorImage;

							// Compute -|Ps - Qs|^(d/2) / Sigma_s²
							value = _texture[finalX,finalY];
//							sqDistanceValue = value - centralValue;								// 2D value => d/2 = 1
							sqDistanceValue = Math.Sqrt( Math.Abs( value - centralValue ) );	// 1D value => d/2 = 0.5
							sqDistanceValue *= m_kernelFactorValue;

							// Compute score as Exp[ -|Pi - Qi|² / Sigma_i² -|Ps - Qs|^(d/2) / Sigma_s² ]
							dScore = Math.Exp( sqDistanceImage + sqDistanceValue );
							score += dScore;
						}
					}
				}
			}
			score /= m_textureTotalSize;

			return (float) score;
		}

		float	Get( float[,] _array, uint _index ) {
			uint	X = _index & m_textureSizeMask;
			uint	Y = _index >> m_texturePOT;
			float	value = _array[X,Y];
			return value;
		}

		void	Set( float[,] _array, uint _index, float _value ) {
			uint	X = _index & m_textureSizeMask;
			uint	Y = _index >> m_texturePOT;
			_array[X,Y] = _value;
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
