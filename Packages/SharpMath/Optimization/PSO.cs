using System;
using System.Collections.Generic;

namespace SharpMath
{
	/// <summary>
	/// Particle Swarm Optimization (https://en.wikipedia.org/wiki/Particle_swarm_optimization)
	/// Original C++ code from Martin Gérard
	/// </summary>
	public class PSO {

		static const double	W = 0.715;
		static const double	C1 = 1.7;
		static const double	C2 = 1.7;

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
/*
#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include <time.h>



#define W	0.715f
#define C1	1.7f
#define C2	1.7f



double Rastrigin(double* x, int d)
{
	double sum = 0.f;

	for (int i = 0; i < d; i++)
	{
		sum += x[i] * x[i] - 10.f * cosf(2.f * 3.1415926f * x[i]);
	}

	return 10.f * d + sum;
}



double Rosenbrock(double* x, int d)
{
	double sum = 0.f;

	for (int i = 0; i < d - 1; i++)
	{
		double p = x[i + 1] - x[i] * x[i];
		double q = x[i] - 1.f;

		sum += 100.f * p * p + q * q;
	}

	return sum;
}



void Copy(double* out, double* in, int d)
{
	for (int i = 0; i < d; i++)
		out[i] = in[i];
}



double PSO(double* x_out, double (*Eval)(double* x, int d), double* x_min, double* x_max, int dim, int n_part, int max_iter)
{
	double** pos			= new double*[n_part];
	double** vel			= new double*[n_part];
	double**	best_pos	= new double*[n_part];	
	double*	best_value	= new double[n_part];
	double	global_best = 1e8f;

	// Initialize positions and velocities
	for (int i = 0; i < n_part; i++)
	{
		pos[i]		= new double[dim];
		vel[i]		= new double[dim];
		best_pos[i] = new double[dim];
		
		for (int j = 0; j < dim; j++)
		{
			pos[i][j] = (x_max[j] - x_min[j]) * rand() / RAND_MAX + x_min[j];
			vel[i][j] = 0.f;

			best_pos[i][j] = pos[i][j];
		}

		best_value[i] = Eval(pos[i], dim);

		if (best_value[i] < global_best)
		{
			global_best = best_value[i];
			Copy(x_out, pos[i], dim);
		}
	}


	// Main loop
	for (int i = 0; i < max_iter; i++)
	{
		// Update values
		for (int j = 0; j < n_part; j++)
		{
			double value = Eval(pos[j], dim);

			if (value < best_value[j])
			{
				best_value[j] = value;
				Copy(best_pos[j], pos[j], dim);

				if (value < global_best)
				{
					global_best = value;
					Copy(x_out, pos[j], dim);
				}
			}
		}

		// Update position and velocities
		for (int j = 0; j < n_part; j++)
		{
			for (int k = 0; k < dim; k++)
			{
				double rp = 1.f * rand() / RAND_MAX;
				double rg = 1.f * rand() / RAND_MAX;

				vel[j][k] = W * vel[j][k] + C1 * rp * (best_pos[j][k] - pos[j][k]) + C2 * rg * (x_out[k] - pos[j][k]);
				pos[j][k] += vel[j][k];

				if (pos[j][k] > x_max[k])
					pos[j][k] = x_max[k];

				else if (pos[j][k] < x_min[k])
					pos[j][k] = x_min[k];
			}
		}
	}

	for (int i = 0; i < n_part; i++)
	{
		delete[] pos[i];
		delete[] vel[i];
		delete[] best_pos[i];
	}

	delete[] pos;
	delete[] vel;
	delete[] best_pos;
	delete[] best_value;

	return global_best;
}



int main()
{
	srand((unsigned int)time(NULL));

	int dim = 10;

	double* x = new double[dim]();

	double* x_min = new double[dim];
	double* x_max = new double[dim];

	for (int i = 0; i < dim; i++)
	{
		// Rosenbrock
		//x_min[i] = -5.f;
		//x_max[i] = 10.f;

		// Rastrigin
		x_min[i] = -5.12f;
		x_max[i] = 5.12f;
	}

	double min_val = PSO(x, Rastrigin, x_min, x_max, dim, 5000, 1000);

	printf("Minimum value : %g\n", min_val);
	printf("Found at : (");

	for (int i = 0; i < dim - 1; i++)
		printf("%g, ", x[i]);

	printf("%g)\n\n", x[dim - 1]);

	getchar();

	delete[] x;
	delete[] x_min;
	delete[] x_max;

    return 0;
}
*/
