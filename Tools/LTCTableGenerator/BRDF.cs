//////////////////////////////////////////////////////////////////////////
// Fitter class for Linearly-Transformed Cosines
// From "Real-Time Polygonal-Light Shading with Linearly Transformed Cosines" (https://eheitzresearch.wordpress.com/415-2/)
// This is a C# re-implementation of the code provided by Heitz et al.
//////////////////////////////////////////////////////////////////////////
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using SharpMath;

namespace LTCTableGenerator
{
	public interface	IBRDF {
		// 
		/// <summary>
		/// Evaluation of the ***cosine-weighted*** BRDF
		/// </summary>
		/// <param name="_tsView"></param>
		/// <param name="_tsLight"></param>
		/// <param name="_alpha"></param>
		/// <param name="_pdf">pdf is set to the PDF of sampling L</param>
		/// <returns></returns>
		double	Eval( ref float3 _tsView, ref float3 _tsLight, float _alpha, out double _pdf );

		/// <summary>
		/// Gets an importance-sampled direction
		/// </summary>
		/// <param name="_tsView"></param>
		/// <param name="_alpha"></param>
		/// <param name="_U1"></param>
		/// <param name="_U2"></param>
		/// <param name="_direction"></param>
		void	GetSamplingDirection( ref float3 _tsView, float _alpha, float _U1, float _U2, ref float3 _direction );

// Not used anymore...
// 		/// <summary>
// 		/// Gets the BRDF's maximum amplitude value
// 		/// </summary>
// 		double	MaxValue( ref float3 _tsView, float _alpha );
	};

	/// <summary>
	/// GGX implementation of the BRDF interface
	/// </summary>
	class BRDF_GGX : IBRDF {

		public double	Eval( ref float3 _tsView, ref float3 _tsLight, float _alpha, out double _pdf ) {
			if ( _tsView.z <= 0 ) {
				_pdf = 0;
				return 0;
			}

			_alpha = Mathf.Max( 0.002f, _alpha );

			// masking
			double	lambdaV = Lambda( _tsView.z, _alpha );
			double	G1 = 1.0 / (1.0 + lambdaV);

			// shadowing
			double	G2 = 0;
			if ( _tsLight.z > 0.0f ) {
				double	lambdaL = Lambda( _tsLight.z, _alpha );
				G2 = 1.0 / (1.0 + lambdaV + lambdaL);
			}

			// D
			float3	H = (_tsView + _tsLight).Normalized;
//			H.z = Mathf.Max( 1e-8f, H.z );
			if ( Mathf.Almost( H.z, 0, 1e-8f ) )
				H.z = 1e-8f;

			double	slopex = H.x / H.z;
			double	slopey = H.y / H.z;
			double	D = 1.0 / (1.0 + (slopex*slopex + slopey*slopey) / (_alpha*_alpha));
			D = D*D;
			D = D / (Math.PI * _alpha * _alpha * H.z*H.z*H.z*H.z);

			double	res = D * G2 / (4.0 * _tsView.z);		// Full specular mico-facet model is F * D * G / (4 * NdotL * NdotV) but since we're fitting with the NdotL included, it gets nicely canceled out!

			// pdf = D(H) * (N.H) / (4 * (L.H))
			_pdf = Math.Abs( D * H.z / (4.0 * _tsView.Dot(H)) );

			return res;
		}

		public void	GetSamplingDirection( ref float3 _tsView, float _alpha, float _U1, float _U2, ref float3 _direction ) {
			float	phi = Mathf.TWOPI * _U1;
			float	r = _alpha * Mathf.Sqrt( _U2 / (1.0f - _U2) );
			float3	H = new float3( r*Mathf.Cos(phi), r*Mathf.Sin(phi), 1.0f ).Normalized;
			_direction = -_tsView + 2.0f * H * H.Dot(_tsView);
		}

		public double	MaxValue( ref float3 _tsView, float _alpha ) {
			double	D = 1.0 / (Math.PI * _alpha * _alpha);
			double	G = 1.0 / (1.0 + Lambda( _tsView.z, _alpha ));
			return D * G / (4.0 * _tsView.z);
		}

		double	Lambda( float _cosTheta, float _alpha ) {
			double	a = 1.0f / (_alpha * Math.Tan( Math.Acos( _cosTheta ) ));
			double	lambda = _cosTheta < 1.0 ? 0.5 * (-1.0 + Math.Sqrt(1.0 + 1.0 / (a*a))) : 0.0;
			return lambda;
		}
	}

	/// <summary>
	/// Cook-Torrance implementation of the BRDF interface
	/// </summary>
	class BRDF_CookTorrance : IBRDF {

		public double	Eval( ref float3 _tsView, ref float3 _tsLight, float _alpha, out double _pdf ) {
			if ( _tsView.z <= 0 ) {
				_pdf = 0;
				return 0;
			}

			_alpha = Mathf.Max( 0.002f, _alpha );

			float3	H = (_tsView + _tsLight).Normalized;
			double	NdotL = _tsLight.z;
			double	NdotV = _tsView.z;
			double	NdotH = H.z;
			double	LdotH = _tsLight.Dot( H );

			// D
			double	cosb2 = NdotH * NdotH;
			double	m2 = _alpha * _alpha;
			double	D = Math.Exp( (cosb2 - 1.0) / (cosb2*m2) )	// exp( -tan(a)² / m² ) 
					  / (Math.PI * m2 * cosb2*cosb2);			// / (PI * m² * cos(a)^4)

			// masking/shadowing
			double	G = Math.Max( 0, Math.Min( 1, 2.0 * NdotH * Math.Min( NdotV, NdotL ) / LdotH ) );

			// fr = F(H) * G(V, L) * D(H) / (4 * (N.L) * (N.V))
			double	res = D * G / (4.0 * NdotV);		// Full specular mico-facet model is F * D * G / (4 * NdotL * NdotV) but since we're fitting with the NdotL included, it gets nicely canceled out!

			// pdf = D(H) * (N.H) / (4 * (L.H))
			_pdf = Math.Abs( D * NdotH / (4.0 * LdotH) );

			return res;
		}

		public void	GetSamplingDirection( ref float3 _tsView, float _alpha, float _U1, float _U2, ref float3 _direction ) {
			float	phi = Mathf.TWOPI * _U1;
			float	cosTheta = 1.0f / Mathf.Sqrt( 1 - _alpha*_alpha * Mathf.Log( Mathf.Max( 1e-6f, _U2 ) ) );
			float	sinTheta = Mathf.Sqrt( 1 - cosTheta*cosTheta );
			float3	H = new float3( sinTheta*Mathf.Cos(phi), sinTheta*Mathf.Sin(phi), cosTheta );
			_direction = 2.0f * H.Dot(_tsView) * H - _tsView;	// Mirror view direction
		}

		public double	MaxValue( ref float3 _tsView, float _alpha ) {
			double	a2 = Math.Max( 1e-4, _alpha * _alpha );
			double	D =  1.0 / (Math.PI * a2);
			double	G = 1.0;
			return D * G / (4.0 * _tsView.z);
		}
	}

	/// <summary>
	/// "Charlie" Sheen Implementation
	/// Source from Sony Pictures Imageworks by Estevez and Kulla, "Production Friendly Microfacet Sheen BRDF" (http://blog.selfshadow.com/publications/s2017-shading-course/imageworks/s2017_pbs_imageworks_sheen.pdf)
	/// Details: https://knarkowicz.wordpress.com/2018/01/04/cloth-shading/
	/// </summary>
	class BRDF_Charlie : IBRDF {

		public double	Eval( ref float3 _tsView, ref float3 _tsLight, float _alpha, out double _pdf ) {
			if ( _tsView.z <= 0 ) {
				_pdf = 0;
				return 0;
			}

			_alpha = Mathf.Max( 0.002f, _alpha );

			float3	H = (_tsView + _tsLight).Normalized;
			double	NdotL = _tsLight.z;
			double	NdotV = _tsView.z;
			double	NdotH = H.z;
//			double	LdotH = _tsLight.Dot( H );

			// D
			double	D = CharlieD( _alpha, NdotH );

			// Ashikmin masking/shadowing
//			double	G = V_Ashikhmin( NdotV, NdotL );
			double	G = V_Charlie( NdotV, NdotL, _alpha );

			// fr = F(H) * G(V, L) * D(H)
			// Note that the usual 1 / (4 * (N.L) * (N.V)) part of the Cook-Torrance micro-facet model is actually contained in the G visibility term in our case (as reported by Ashkmin in "Distribution-based BRDFs" eq. 2)
			double	res = D * G * NdotL;	// We also include the (N.L) term here
// 			if ( res < 0 )
// 				throw new Exception( "GAH!" );

			// pdf = D(H) * (N.H) / (4 * (L.H))
//			_pdf = Math.Abs( D * NdotH / (4.0 * LdotH) );	// We're not using something similar to the normals distribution
			_pdf = 0.5 / Math.PI;							// We're using uniform distribution, as advised by the paper...

			return res;
		}

		// Paper recommend plain uniform sampling of upper hemisphere instead of importance sampling for Charlie
		public void	GetSamplingDirection( ref float3 _tsView, float _alpha, float _U1, float _U2, ref float3 _direction ) {
			float	phi = Mathf.TWOPI * _U1;
			float	cosTheta = 1.0f - _U2;
			float	sinTheta = Mathf.Sqrt( 1 - cosTheta*cosTheta );
			_direction = new float3( sinTheta*Mathf.Cos(phi), sinTheta*Mathf.Sin(phi), cosTheta );
		}

		public double	MaxValue( ref float3 _tsView, float _alpha ) {
			double	maxD = (2.0 + 1.0 / _alpha) / (2.0 * Math.PI);
			double	NdotV = Math.Max( 0.0, _tsView.z );
			double	maxG = 1.0 / (4.0 * NdotV);
			return maxD * maxG;
		}

		double	CharlieD( float _roughness, double _NdotH ) {
			double	invR = 1.0 / _roughness;
			double	cos2h = _NdotH * _NdotH;
			double	sin2h = 1.0f - cos2h;
			double	res = (2.0 + invR) * Math.Pow( sin2h, invR * 0.5 ) / (2.0 * Math.PI);
			return res;
		}
 
		double V_Ashikhmin( double _NdotV, double _NdotL ) {
			return 1.0 / (4.0 * (_NdotL + _NdotV - _NdotL * _NdotV));
		}
 
		// Note: This version don't include the softening of the paper: Production Friendly Microfacet Sheen BRDF
		double	V_Charlie( double _NdotV, double _NdotL, double _roughness ) {
			double	lambdaV = _NdotV < 0.5 ? Math.Exp( CharlieL(_NdotV, _roughness) ) : Math.Exp( 2.0 * CharlieL(0.5, _roughness) - CharlieL(1.0 - _NdotV, _roughness) );
			double	lambdaL = _NdotL < 0.5 ? Math.Exp( CharlieL(_NdotL, _roughness) ) : Math.Exp( 2.0 * CharlieL(0.5, _roughness) - CharlieL(1.0 - _NdotL, _roughness) );

			return 1.0 / ((1.0 + lambdaV + lambdaL) * (4.0 * _NdotV * _NdotL));
		}
		double	CharlieL( double x, double _roughness ) {
			float	r = Mathf.Saturate( (float) _roughness );
					r = 1.0f - r * r;

			float	a = Mathf.Lerp( 25.3245f, 21.5473f, r );
			float	b = Mathf.Lerp( 3.32435f, 3.82987f, r );
			float	c = Mathf.Lerp( 0.16801f, 0.19823f, r );
			float	d = Mathf.Lerp( -1.27393f, -1.97760f, r );
			float	e = Mathf.Lerp( -4.85967f, -4.32054f, r );

			double	res = a / (1.0 + b * Math.Pow( Math.Max( 0, x ), c )) + d * x + e;
			return res;
		}
	}

/*	/// <summary>
	/// GGX implementation of the BRDF interface
	/// This implementation is pulling the view-dependent terms out of the BRDF
	/// It was an idea I had while writing the MSBRDF dossier since I saw all these view-dependent terms nicely come out of the irradiance integrals
	///  but it turns out it's not that interesting for regular BRDFs dealing with radiance since the view masking term is somehow involved into the
	///  global G2 term and is thus integrated along with the shadowing term...
	/// </summary>
	class BRDF_GGX_NoView : IBRDF {

		public float	Eval( ref float3 _tsView, ref float3 _tsLight, float _alpha, out float _pdf ) {
			if ( _tsView.z <= 0 ) {
				_pdf = 0;
				return 0;
			}

			_alpha = Mathf.Max( 0.01f, _alpha );

			// masking
			float	lambdaV = Lambda( _tsView.z, _alpha );
			float	G1 = 1.0f / (1.0f + lambdaV);

			// shadowing
			float	G2 = 0;
			if ( _tsLight.z > 0.0f ) {
				float	lambdaL = Lambda( _tsLight.z, _alpha );
//				G2 = 1.0f / (1.0f + lambdaV + lambdaL);
G2 = 1.0f / (1.0f + lambdaL);	// WRONG!
			}

			// D
			float3	H = (_tsView + _tsLight).Normalized;
			float	slopex = H.x / H.z;
			float	slopey = H.y / H.z;
			float	D = 1.0f / (1.0f + (slopex*slopex + slopey*slopey) / (_alpha*_alpha));
			D = D*D;
			D = D / (Mathf.PI * _alpha * _alpha * H.z*H.z*H.z*H.z);

			_pdf = Mathf.Abs( D * H.z / (4.0f * _tsView.Dot(H)) );
//			float	res = D * G2 / (4.0f * _tsView.z);		// Full specular mico-facet model is F * D * G / (4 * NdotL * NdotV) but since we're fitting with the NdotL included, it gets nicely canceled out!

			float	res = D * G2;	// Don't divide by NdotV!

			return res;
		}

		public void	GetSamplingDirection( ref float3 _tsView, float _alpha, float _U1, float _U2, ref float3 _direction ) {
			float	phi = Mathf.TWOPI * _U1;
			float	r = _alpha * Mathf.Sqrt( _U2 / (1.0f - _U2) );
			float3	H = new float3( r*Mathf.Cos(phi), r*Mathf.Sin(phi), 1.0f ).Normalized;
			_direction = -_tsView + 2.0f * H * H.Dot(_tsView);
		}

		public float	Lambda( float _cosTheta, float _alpha ) {
			float	a = 1.0f / (_alpha * Mathf.Tan( Mathf.Acos( _cosTheta ) ));
			float	lambda = _cosTheta < 1.0f ? 0.5f * (-1.0f + Mathf.Sqrt(1.0f + 1.0f / (a*a))) : 0.0f;
			return lambda;
		}
	}
*/
}
