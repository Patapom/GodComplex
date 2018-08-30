// DemoCode.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"


// fitLTC.cpp : Defines the entry point for the console application.
//
#include "Original Code/glm/glm.hpp"
using namespace glm;

#include <algorithm>
#include <fstream>
#include <iomanip>

#include "Original Code/LTC.h"
#include "Original Code/brdf.h"
#include "Original Code/brdf_ggx.h"
#include "Original Code/brdf_beckmann.h"
#include "Original Code/brdf_disneyDiffuse.h"

#include "Original Code/nelder_mead.h"

//#include "Original Code/export.h"
//#include "Original Code/plot.h"

// size of precomputed table (theta, alpha)
const int N = 64;
// number of samples used to compute the error during fitting
const int Nsample = 32;
// minimal roughness (avoid singularities)
const float MIN_ALPHA = 0.00001f;

const float pi = acosf(-1.0f);

// computes
// * the norm (albedo) of the BRDF
// * the average Schlick Fresnel value
// * the average direction of the BRDF
void computeAvgTerms(const Brdf& brdf, const vec3& V, const float alpha,
    float& norm, float& fresnel, vec3& averageDir)
{
    norm = 0.0f;
    fresnel = 0.0f;
    averageDir = vec3(0, 0, 0);

    for (int j = 0; j < Nsample; ++j)
    for (int i = 0; i < Nsample; ++i)
    {
        const float U1 = (i + 0.5f)/Nsample;
        const float U2 = (j + 0.5f)/Nsample;

        // sample
        const vec3 L = brdf.sample(V, alpha, U1, U2);

        // eval
        float pdf;
        float eval = brdf.eval(V, L, alpha, pdf);

        if (pdf > 0)
        {
            float weight = eval / pdf;

            vec3 H = normalize(V+L);

            // accumulate
            norm       += weight;
            fresnel    += weight * pow( 1.0f - max( dot(V, H), 0.0f ), 5.0f);
            averageDir = averageDir + weight * L;
        }
    }

    norm    /= (float)(Nsample*Nsample);
    fresnel /= (float)(Nsample*Nsample);

    // clear y component, which should be zero with isotropic BRDFs
    averageDir.y = 0.0f;

    averageDir = normalize(averageDir);
}

// compute the error between the BRDF and the LTC
// using Multiple Importance Sampling
float computeError(const LTC& ltc, const Brdf& brdf, const vec3& V, const float alpha)
{
    double error = 0.0;

    for (int j = 0; j < Nsample; ++j)
    for (int i = 0; i < Nsample; ++i)
    {
        const float U1 = (i + 0.5f)/Nsample;
        const float U2 = (j + 0.5f)/Nsample;

        // importance sample LTC
        {
            // sample
            const vec3 L = ltc.sample(U1, U2);

            float pdf_brdf;
            float eval_brdf = brdf.eval(V, L, alpha, pdf_brdf);
            float eval_ltc = ltc.eval(L);
            float pdf_ltc = eval_ltc/ltc.magnitude;

            // error with MIS weight
            double error_ = fabsf(eval_brdf - eval_ltc);
            error_ = error_*error_*error_;
			error_ /= pdf_ltc + pdf_brdf;
            error += error_;
        }

        // importance sample BRDF
        {
            // sample
            const vec3 L = brdf.sample(V, alpha, U1, U2);

            float pdf_brdf;
            float eval_brdf = brdf.eval(V, L, alpha, pdf_brdf);
            float eval_ltc = ltc.eval(L);
            float pdf_ltc = eval_ltc/ltc.magnitude;

            // error with MIS weight
            double error_ = fabsf(eval_brdf - eval_ltc);
            error_ = error_*error_*error_;
			error_ /= pdf_ltc + pdf_brdf;
            error += error_;
        }
    }

    error /= Nsample*Nsample;
	return (float) error;
}

struct FitLTC
{
    FitLTC(LTC& ltc_, const Brdf& brdf, bool isotropic_, const vec3& V_, float alpha_) :
        ltc(ltc_), brdf(brdf), V(V_), alpha(alpha_), isotropic(isotropic_)
    {
    }

    void update(const float* params)
    {
        float m11 = std::max<float>(params[0], 1e-7f);
        float m22 = std::max<float>(params[1], 1e-7f);
        float m13 = params[2];

        if (isotropic)
        {
            ltc.m11 = m11;
            ltc.m22 = m11;
            ltc.m13 = 0.0f;
        }
        else
        {
            ltc.m11 = m11;
            ltc.m22 = m22;
            ltc.m13 = m13;
        }
        ltc.update();
    }

    float operator()(const float* params)
    {
        update(params);
        double	error = computeError(ltc, brdf, V, alpha);
		return (float) error;
    }

    const Brdf& brdf;
    LTC& ltc;
    bool isotropic;

    const vec3& V;
    float alpha;
};

// fit brute force
// refine first guess by exploring parameter space
void fit(LTC& ltc, const Brdf& brdf, const vec3& V, const float alpha, const float epsilon = 0.05f, const bool isotropic = false)
{
    float startFit[3] = { ltc.m11, ltc.m22, ltc.m13 };
    float resultFit[3];

    FitLTC fitter(ltc, brdf, isotropic, V, alpha);

    // Find best-fit LTC lobe (scale, alphax, alphay)
    float error = NelderMead<3>(resultFit, startFit, epsilon, 1e-5f, 100, fitter);

    // Update LTC with best fitting values
    fitter.update(resultFit);
}

// fit data
void fitTab(mat3* tab, vec3* tabMagFresnel, const int N, const Brdf& brdf)
{
    LTC ltc;

    // loop over theta and alpha
     for (int a = N - 1; a >=     0; --a)
     for (int t =     0; t <= N - 1; ++t)

//	int	a = 19;
//	int	t = 40;

    {
        // parameterised by sqrt(1 - cos(theta))
        float x = t/float(N - 1);
        float ct = 1.0f - x*x;
        float theta = std::min<float>(1.57f, acosf(ct));
        const vec3 V = vec3(sinf(theta), 0, cosf(theta));

        // alpha = roughness^2
        float roughness = a/float(N - 1);
        float alpha = std::max<float>(roughness*roughness, MIN_ALPHA);

        cout << "a = " << a << "\t t = " << t  << endl;
        cout << "alpha = " << alpha << "\t theta = " << theta << endl;
        cout << endl;

        vec3 averageDir;
        computeAvgTerms(brdf, V, alpha, ltc.magnitude, ltc.fresnel, averageDir);

        bool isotropic;

        // 1. first guess for the fit
        // init the hemisphere in which the distribution is fitted
        // if theta == 0 the lobe is rotationally symmetric and aligned with Z = (0 0 1)
        if (t == 0)
        {
            ltc.X = vec3(1, 0, 0);
            ltc.Y = vec3(0, 1, 0);
            ltc.Z = vec3(0, 0, 1);

            if (a == N - 1) // roughness = 1
            {
                ltc.m11 = 1.0f;
                ltc.m22 = 1.0f;
            }
            else // init with roughness of previous fit
            {
                ltc.m11 = tab[a + 1 + t*N][0][0];
                ltc.m22 = tab[a + 1 + t*N][1][1];
            }

            ltc.m13 = 0;
            ltc.update();

            isotropic = true;
        }
        // otherwise use previous configuration as first guess
        else
        {
            vec3 L = averageDir;
            vec3 T1(L.z, 0, -L.x);
            vec3 T2(0, 1, 0);
            ltc.X = T1;
            ltc.Y = T2;
            ltc.Z = L;

            ltc.update();

            isotropic = false;
        }

        // 2. fit (explore parameter space and refine first guess)
        float epsilon = 0.05f;
        fit(ltc, brdf, V, alpha, epsilon, isotropic);

        // copy data
        tab[a + t*N] = ltc.M;
        tabMagFresnel[a + t*N][0] = ltc.magnitude;
        tabMagFresnel[a + t*N][1] = ltc.fresnel;

        // kill useless coefs in matrix
        tab[a+t*N][0][1] = 0;
        tab[a+t*N][1][0] = 0;
        tab[a+t*N][2][1] = 0;
        tab[a+t*N][1][2] = 0;

        cout << tab[a+t*N][0][0] << "\t " << tab[a+t*N][1][0] << "\t " << tab[a+t*N][2][0] << endl;
        cout << tab[a+t*N][0][1] << "\t " << tab[a+t*N][1][1] << "\t " << tab[a+t*N][2][1] << endl;
        cout << tab[a+t*N][0][2] << "\t " << tab[a+t*N][1][2] << "\t " << tab[a+t*N][2][2] << endl;
        cout << endl;
    }
}

float sqr(float x)
{
    return x*x;
}

float G(float w, float s, float g)
{
    return -2.0f*sinf(w)*cosf(s)*cosf(g) + pi/2.0f - g + sinf(g)*cosf(g);
}

float H(float w, float s, float g)
{
    float sinsSq = sqr(sin(s));
    float cosgSq = sqr(cos(g));

    return cosf(w)*(cosf(g)*sqrtf(sinsSq - cosgSq) + sinsSq*asinf(cosf(g)/sinf(s)));
}

float ihemi(float w, float s)
{
    float g = asinf(cosf(s)/sinf(w));
    float sinsSq = sqr(sinf(s));

    if (w >= 0.0f && w <= (pi/2.0f - s))
        return pi*cosf(w)*sinsSq;

    if (w >= (pi/2.0f - s) && w < pi/2.0f)
        return pi*cosf(w)*sinsSq + G(w, s, g) - H(w, s, g);

    if (w >= pi/2.0f && w < (pi/2.0f + s))
        return G(w, s, g) + H(w, s, g);

    return 0.0f;
}

void genSphereTab(float* tabSphere, int N)
{
    for (int j = 0; j < N; ++j)
    for (int i = 0; i < N; ++i)
    {
        const float U1 = float(i)/(N - 1);
        const float U2 = float(j)/(N - 1);

        // z = cos(elevation angle)
        float z = 2.0f*U1 - 1.0f;

        // length of average dir., proportional to sin(sigma)^2
        float len = U2;

        float sigma = asinf(sqrtf(len));
        float omega = acosf(z);

        // compute projected (cosine-weighted) solid angle of spherical cap
        float value = 0.0f;

        if (sigma > 0.0f)
            value = ihemi(omega, sigma)/(pi*len);
        else
            value = std::max<float>(z, 0.0f);

        if (value != value)
            printf("nan!\n");

        tabSphere[i + j*N] = value;
    }
}

// void packTab(
//     vec4* tex1, vec4* tex2,
//     const mat3*  tab,
//     const vec2*  tabMagFresnel,
//     const float* tabSphere,
//     int N)
// {
//     for (int i = 0; i < N*N; ++i)
//     {
//         const mat3& m = tab[i];
// 
//         mat3 invM = inverse(m);
// 
//         // normalize by the middle element
//         invM /= invM[1][1];
// 
//         // store the variable terms
//         tex1[i].x = invM[0][0];
//         tex1[i].y = invM[0][2];
//         tex1[i].z = invM[2][0];
//         tex1[i].w = invM[2][2];
//         tex2[i].x = tabMagFresnel[i][0];
//         tex2[i].y = tabMagFresnel[i][1];
//         tex2[i].z = 0.0f; // unused
//         tex2[i].w = tabSphere[i];
//     }
// }

// export data to C
void writeTabC( mat3* tab, vec3* tabMagFresnel, int N ) {
    ofstream file( "ltc.inc" );

    file << std::fixed;
    file << std::setprecision(6);

    file << "static const int size = " << N  << ";" << endl << endl;

    file << "static const mat33 tabM[size*size] = {" << endl;
    for (int t = 0; t < N; ++t)
    for (int a = 0; a < N; ++a)
    {
        file << "{";
        file << tab[a + t*N][0][0] << ", " << tab[a + t*N][0][1] << ", " << tab[a + t*N][0][2] << ", ";
        file << tab[a + t*N][1][0] << ", " << tab[a + t*N][1][1] << ", " << tab[a + t*N][1][2] << ", ";
        file << tab[a + t*N][2][0] << ", " << tab[a + t*N][2][1] << ", " << tab[a + t*N][2][2] << "}";
        if (a != N - 1 || t != N - 1)
            file << ", ";
        file << endl;
    }
    file << "};" << endl << endl;

    file << "static const mat33 tabMinv[size*size] = {" << endl;
    for (int t = 0; t < N; ++t)	// Theta
    for (int a = 0; a < N; ++a)	// Alpha
    {
        mat3 Minv = glm::inverse(tab[a + t*N]);

        file << "{";
        file << Minv[0][0] << ", " << Minv[0][1] << ", " << Minv[0][2] << ", ";	// Export column 0
        file << Minv[1][0] << ", " << Minv[1][1] << ", " << Minv[1][2] << ", ";	// Export column 1
        file << Minv[2][0] << ", " << Minv[2][1] << ", " << Minv[2][2] << "}";	// Export column 2
        if (a != N - 1 || t != N - 1)
            file << ", ";
        file << endl;
    }
    file << "};" << endl << endl;

//     file << "static const float tabMagnitude[size*size] = {" << endl;
//     for (int t = 0; t < N; ++t)
//     for (int a = 0; a < N; ++a)
//     {
//         file << tabMagFresnel[a + t*N][0] << "f";
//         if (a != N - 1 || t != N - 1)
//             file << ", ";
//         file << endl;
//     }
//     file << "};" << endl;

    file.close();
}


int _tmain(int argc, _TCHAR* argv[]) {
	// BRDF to fit
	BrdfGGX brdf;
	//BrdfBeckmann brdf;
	//BrdfDisneyDiffuse brdf;

	// allocate data
	mat3*  tab = new mat3[N*N];
	vec3*  tabMagFresnel = new vec3[N*N];
	float* tabSphere = new float[N*N];

	// fit
	fitTab(tab, tabMagFresnel, N, brdf);

// 	// projected solid angle of a spherical cap, clipped to the horizon
// 	genSphereTab(tabSphere, N);
// 
// 	// pack tables (texture representation)
// 	vec4* tex1 = new vec4[N*N];
// 	vec4* tex2 = new vec4[N*N];
// 	packTab(tex1, tex2, tab, tabMagFresnel, tabSphere, N);
// 
// 	// export to C, MATLAB and DDS
// 	writeTabMatlab(tab, tabMagFresnel, N);
 	writeTabC( tab, tabMagFresnel, N );
// 	writeDDS(tex1, tex2, N);
// 	writeJS(tex1, tex2, N);

	// spherical plots
	// make_spherical_plots(brdf, tab, N);

	// delete data
	delete[] tab;
	delete[] tabMagFresnel;
	delete[] tabSphere;
// 	delete[] tex1;
// 	delete[] tex2;

	return 0;
}
