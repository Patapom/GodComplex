using System;
using System.Collections.Generic;

namespace SharpMath
{
	/// <summary>
	/// Particle Swarm Optimization (https://en.wikipedia.org/wiki/Particle_swarm_optimization)
	/// Original C++ code from Martin Gérard
	/// </summary>
	public class PSO {

		const double	W = 0.715;
		const double	C1 = 1.7;
		const double	C2 = 1.7;

		/// <summary>
		/// Delegate used to evaluate the model
		/// </summary>
		/// <param name="x">The current vector of parameters</param>
		/// <returns></returns>
		public delegate double	Eval( double[] x );

		/// <summary>
		/// Finds the global minimum of a function
		/// </summary>
		/// <param name="x_out">Resulting global minimum</param>
		/// <param name="x_min">Minimum values for each parameter</param>
		/// <param name="x_max">Maximum values for each parameter</param>
		/// <param name="_particlesCount">Amount of particles to simulate</param>
		/// <param name="_maxIterationsCount">Maximum amount of iterations to perform</param>
		/// <param name="_model">The model estimator</param>
		/// <returns></returns>
		public static double	FindGlobalMinimum( double[] x_out, double[] x_min, double[] x_max, int _particlesCount, int _maxIterationsCount, Eval _model ) {
			int			dim = x_out.Length;
			double[][]	positions = new double[_particlesCount][];
			double[][]	velocities = new double[_particlesCount][];
			double[][]	bestPositions = new double[_particlesCount][];
			double[]	bestValue = new double[dim];

			int			globalBestValueIndex = -1;
			double		globalBestValue = double.MaxValue;

			// Initialize positions and velocities
			for ( int i = 0; i < _particlesCount; i++) {
				positions[i] = new double[dim];
				velocities[i] = new double[dim];
				bestPositions[i] = new double[dim];
				for ( int j = 0; j < dim; j++ ) {
					positions[i][j] = (x_max[j] - x_min[j]) * SimpleRNG.GetUniform() + x_min[j];
					velocities[i][j] = 0.0;

					bestPositions[i][j] = positions[i][j];
				}

				bestValue[i] = _model( positions[i] );
			}

			// Main loop
			for ( int i = 0; i < _maxIterationsCount; i++ ) {
				// Update local minimum values
				for ( int j = 0; j < _particlesCount; j++) {
					double value = _model( positions[j] );
					if ( value >= bestValue[j] )
						continue;

					// The new particle yields a better value, validate its position
					bestValue[j] = value;
					positions[j].CopyTo( bestPositions[j], 0 );

					if ( value < globalBestValue ) {
						// The particle also is the new global minimum!
						globalBestValue = value;
						globalBestValueIndex = i;
					}
				}

				// Update global minimum
				positions[globalBestValueIndex].CopyTo( x_out, 0 );

				// Update position and velocities by making the particles tend to the local and global minima using a different mixture of influences
				for ( int j = 0; j < _particlesCount; j++ ) {
					for ( int k = 0; k < dim; k++ ) {
						double	rp = SimpleRNG.GetUniform();
						double	rg = SimpleRNG.GetUniform();

						velocities[j][k] = W * velocities[j][k]									// Dampen current velocity to avoid explosion
										 + C1 * rp * (bestPositions[j][k] - positions[j][k])	// Mix-in an attraction to the local minimum
										 + C2 * rg * (x_out[k] - positions[j][k]);				// ...and an attraction to the global minimum as well

						positions[j][k] += velocities[j][k];

						// Clamp positions to boundaries
						if (positions[j][k] > x_max[k]) {
							positions[j][k] = x_max[k];
						} else if (positions[j][k] < x_min[k]) {
							positions[j][k] = x_min[k];
						}
					}
				}
			}

			return globalBestValue;
		}

	}
}
