using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WMath;

namespace GenerateEyeCaustics
{
	public partial class Form1 : Form
	{
		const int		TEXTURE_SIZE = 256;
		const float		EYE_RADIUS = 24.0f;			// 24mm
		const float		IRIS_START_ANGLE = (float) (31.788330617051619834071659608316 * Math.PI / 180.0f);	// The iris plateau is roughly at 6.8/8.0 of the eye radius so the cone angle defining the spherical section of the iris is about acos(6.8/8) = 31.78°
//		const float		IRIS_START_ANGLE = (float) (53.130102354155978703144387440907 * Math.PI / 180.0f);	// The iris plateau is roughly at 6.8/8.0 of the eye radius so the cone angle defining the spherical section of the iris is about acos(6.8/8) = 31.78°
		
		public float[,]		m_PhotonsAccumulation = new float[TEXTURE_SIZE,TEXTURE_SIZE];

		public Form1()
		{
			InitializeComponent();
		}

		private void buttonShoot_Click( object sender, EventArgs e )
		{
			// Clear accumulation
			for ( int Y=0; Y < TEXTURE_SIZE; Y++ )
			{
				for ( int X=0; X < TEXTURE_SIZE; X++ )
				{
					m_PhotonsAccumulation[X,Y] = 0.0f;
				}
			}

			float	LightTheta = (float) Math.PI * floatTrackbarControlTheta.Value / 180.0f;
			Vector	Light = new Vector( (float) -Math.Sin( LightTheta ), 0.0f, (float) Math.Cos( LightTheta ) );
//			float	Flux = TEXTURE_SIZE*TEXTURE_SIZE / integerTrackbarControlPhotonsCount.Value;
			float	Flux = 20000.0f / integerTrackbarControlPhotonsCount.Value;

			float	PlaneD = (float) Math.Cos( IRIS_START_ANGLE );
			float	IrisRadius = (float) Math.Sin( IRIS_START_ANGLE );

			// Compute bounds for photon generation
			float	BoundX0 = -IrisRadius;
			float	BoundX1 = +IrisRadius;

			Vector	Ortho = new Vector( Light.z, 0.0f, -Light.x );	// Vector tangent to the plane where we can measure projected bounds

			float	ProjectedBound0 = BoundX0 * Ortho.x + PlaneD * Ortho.z;
			float	ProjectedBound1 = BoundX1 * Ortho.x + PlaneD * Ortho.z;
					ProjectedBound1 = Math.Max( ProjectedBound1, Ortho.z );	// Max with the projected sphere's tangent

			ProjectedBound1 *= 1.1f;

			// We now have a tight rectangle in projected space [-IrisRadius,ProjectedBound0] [IrisRadius,ProjectedBound1]
			//	where we can place random photons that will shoot toward the eye's iris.
			// We still can miss the sphere though...


			// Start shooting
			double	MaxThetaRandom = Math.Pow( Math.Sin( IRIS_START_ANGLE ), 2.0 );
			Vector	P = new Vector();
			Vector	N = new Vector();
			Vector	Ray = new Vector();
			Vector	Intersection = new Vector();

			float	Eta = 1.00029f / 1.34f;	// n1 / n2

			float	fX, fY;
			int		Px, Py;

			for ( int PhotonIndex=0; PhotonIndex < integerTrackbarControlPhotonsCount.Value; PhotonIndex++ )
			{
// Wrong as photons are not distributed on the spherical cap
// 				double	Phi = 2.0 * Math.PI * SimpleRNG.GetUniform();
// 				double	Theta = Math.Asin( Math.Sqrt( MaxThetaRandom * SimpleRNG.GetUniform() ) );
// 
// 				N.x = (float) (Math.Sin( Theta ) * Math.Cos( Phi ));
// 				N.y = (float) (Math.Sin( Theta ) * Math.Sin( Phi ));
// 				N.z = (float) Math.Cos( Theta );
// 				if ( N.Dot( Light ) < 0.0f )
// 					continue;	// Opposite side of the spherical cap

				// Draw a random position on the light plane and shoot toward the iris
				float	x = (float) (ProjectedBound0 + SimpleRNG.GetUniform() * (ProjectedBound1 - ProjectedBound0));
				float	y = (float) (IrisRadius * (2.0 * SimpleRNG.GetUniform() - 1.0));

				float	SqRadius = x*x + y*y;
 				if ( SqRadius > 1.0f )
 					continue;	// Photon will hit outside the sphere (should never happen unless iris is as large as the eye itself)

				float	z = (float) Math.Sqrt( 1.0 - SqRadius );

				// Recompute normal at intersection
				N.x = x * Ortho.x + z * Light.x;
				N.y = y;
				N.z = x * Ortho.z + z * Light.z;

				if ( N.z < PlaneD )
					continue;	// We drew a position beneath the iris plane (outside of zone of interest)

				// Refract ray through the surface
				float	c1 = -N.Dot( Light );
				float	cs2 = 1.0f - Eta * Eta * (1.0f - c1 * c1);
				if ( cs2 < 0.0f )
					continue;	// Total internal reflection

				cs2 = Eta * c1 - (float) Math.Sqrt( cs2 );
				Ray.x = Eta * Light.x + cs2 * N.x;
				Ray.y = Eta * Light.y + cs2 * N.y;
				Ray.z = Eta * Light.z + cs2 * N.z;

				// Compute intersection with plane
				float	d = (PlaneD - N.z) / Ray.z;
				if ( d < 0.0f )
					continue;	// ?

				Intersection.x = N.x + d * Ray.x;
				Intersection.y = N.y + d * Ray.y;
				Intersection.z = N.z + d * Ray.z;

				fX = 0.5f * (1.0f + Intersection.x / IrisRadius);
				fY = 0.5f * (1.0f + Intersection.y / IrisRadius);

				Px = (int) Math.Floor( fX * TEXTURE_SIZE );
				if ( Px < 0 || Px >= TEXTURE_SIZE )
					continue;	// Out of range??
				Py = (int) Math.Floor( fY * TEXTURE_SIZE );
				if ( Py < 0 || Py >= TEXTURE_SIZE )
					continue;	// Out of range??

				m_PhotonsAccumulation[Px,Py] += Flux;
			}

			outputPanel1.PhotonsAccumulation = m_PhotonsAccumulation;
		}
	}
}
