#ifndef NELDER_MEAD_H
#define NELDER_MEAD_H

void mov(float* r, const float* v, int dim)
{
    for (int i = 0; i < dim; ++i)
        r[i] = v[i];
}

void set(float* r, const float v, int dim)
{
    for (int i = 0; i < dim; ++i)
        r[i] = v;
}

void add(float* r, const float* v, int dim)
{
    for (int i = 0; i < dim; ++i)
        r[i] += v[i];
}

// Downhill simplex solver:
// http://en.wikipedia.org/wiki/Nelder%E2%80%93Mead_method#One_possible_variation_of_the_NM_algorithm
// using the termination criterion from Numerical Recipes in C++ (3rd Ed.)
ofstream logFile( "log.txt" );

template<int DIM, typename FUNC>
float NelderMead(
    float* pmin, const float* start, float delta, float tolerance, int maxIters, FUNC objectiveFn)
{
    // standard coefficients from Nelder-Mead
    const float reflect  = 1.0f;
    const float expand   = 2.0f;
    const float contract = 0.5f;
    const float shrink   = 0.5f;

    typedef float point[DIM];
    const int NB_POINTS = DIM + 1;

    point s[NB_POINTS];
    float f[NB_POINTS];

    // initialise simplex
    mov(s[0], start, DIM);
    for (int i = 1; i < NB_POINTS; i++)
    {
        mov(s[i], start, DIM);
        s[i][i - 1] += delta;
    }

    // evaluate function at each point on simplex
logFile << "Init" << endl;
    for (int i = 0; i < NB_POINTS; i++) {
        f[i] = objectiveFn(s[i]);
logFile << "f[" << i << "] = " << f[i] << endl;
	}

    int lo = 0, hi, nh;

	int	iterationsCount = 0;
    for (; iterationsCount < maxIters; iterationsCount++)
    {
logFile << endl;
logFile << "===================================" << endl;
logFile << "Iteration #" << iterationsCount << endl;

        // find lowest, highest and next highest
        lo = hi = nh = 0;
        for (int i = 1; i < NB_POINTS; i++)
        {
            if (f[i] < f[lo])
                lo = i;
            if (f[i] > f[hi])
            {
                nh = hi;
                hi = i;
            }
            else if (f[i] > f[nh])
                nh = i;

logFile << "f[" << i << "] = " << f[i] << endl;
        }

        // stop if we've reached the required tolerance level
        float a = fabsf(f[lo]);
        float b = fabsf(f[hi]);
        if (2.0f*fabsf(a - b) < (a + b)*tolerance)
            break;

        // compute centroid (excluding the worst point)
        point o;
        set(o, 0.0f, DIM);
        for (int i = 0; i < NB_POINTS; i++)
        {
            if (i == hi) continue;
            add(o, s[i], DIM);
        }

        for (int i = 0; i < DIM; i++)
            o[i] /= DIM;

logFile << "centroid = {" << o[0] << ", " << o[1] << ", " << o[2] << "}" << endl;

        // reflection
        point r;
        for (int i = 0; i < DIM; i++)
            r[i] = o[i] + reflect*(o[i] - s[hi][i]);

        float fr = objectiveFn(r);

logFile << "reflection = {" << r[0] << ", " << r[1] << ", " << r[2] << "}" << endl;
logFile << "reflection error = " << fr << endl;

        if (fr < f[nh])
        {
            if (fr < f[lo])
            {
                // expansion
                point e;
                for (int i = 0; i < DIM; i++)
                    e[i] = o[i] + expand*(o[i] - s[hi][i]);
                float fe = objectiveFn(e);

logFile << "expansion = {" << e[0] << ", " << e[1] << ", " << e[2] << "}" << endl;
logFile << "expansion error = " << fe << endl;

                if (fe < fr)
                {
                    mov(s[hi], e, DIM);
                    f[hi] = fe;
logFile << "CHOSE EXPANSION" << endl;
                    continue;
                }
            }

            mov(s[hi], r, DIM);
            f[hi] = fr;

logFile << "CHOSE REFLECTION" << endl;
            continue;
        }

        // contraction
        point c;
        for (int i = 0; i < DIM; i++)
            c[i] = o[i] - contract*(o[i] - s[hi][i]);

        float fc = objectiveFn(c);

logFile << "contraction = {" << c[0] << ", " << c[1] << ", " << c[2] << "}" << endl;
logFile << "contraction error = " << fc << endl;

        if (fc < f[hi])
        {
            mov(s[hi], c, DIM);
            f[hi] = fc;
logFile << "CHOSE CONTRACTION" << endl;
            continue;
        }

        // reduction
        for (int k = 0; k < NB_POINTS; k++)
        {
            if (k == lo) continue;
            for (int i = 0; i < DIM; i++)
                s[k][i] = s[lo][i] + shrink*(s[k][i] - s[lo][i]);
            f[k] = objectiveFn(s[k]);
        }
logFile << "CHOSE REDUCTION" << endl;
    }

    // return best point and its value
    mov(pmin, s[lo], DIM);

logFile << endl;
logFile << endl;
logFile << "===================================" << endl;
logFile << "Exiting after " << iterationsCount << " iterations" << endl;
logFile << "Result = {" << pmin[0] << ", " << pmin[1] << ", " << pmin[2] << "}" << endl;
logFile << "Error = " << f[lo] << endl;

	return f[lo];
}

#endif // NELDER_MEAD_H
