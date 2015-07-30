using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using RendererManaged;

namespace TestBoxFitting
{
	public partial class Form1 : Form
	{
		public const int		PIXELS_COUNT = 4*128;

		public struct Pixel {
			public float	Distance;
			public float2	Normal;
		}

		public Pixel[]		m_Pixels = new Pixel[PIXELS_COUNT];

		public Form1()
		{
			InitializeComponent();
			panelOutput.m_Owner = this;

			// Build a random polygonal room
			Random	RNG = new Random( 1 );

			const float		MAX_DISTANCE = 10.0f;

			int			planesCount = (int) (4 + 2 * RNG.NextDouble());
			float2[]	planePositions = new float2[planesCount];
			float2[]	planeNormals = new float2[planesCount];

			float2		center = new float2( (float) (-10.0 + 20.0 * RNG.NextDouble()), (float) (-10.0 + 20.0 * RNG.NextDouble()) );
			float		baseAngle = (float) (RNG.NextDouble() * 2.0 * Math.PI);
			float		averageAngle = (float) (2.0 * Math.PI / planesCount);
			for ( int planeIndex=0; planeIndex < planesCount; planeIndex++ ) {
				float2	normal = new float2( (float) Math.Cos( baseAngle ), (float) Math.Sin( baseAngle ) );
				planeNormals[planeIndex] = normal;

				float2	toCenter = center;
				float	distance2Center = toCenter.Dot( normal );
				float	planeDistance = distance2Center > 0.0f ? distance2Center + 0.1f + (float) (Math.Max( 0.0f, MAX_DISTANCE-distance2Center ) * RNG.NextDouble()) : (float) (MAX_DISTANCE * RNG.NextDouble());

				float2	position = center - planeDistance * normal;
				planePositions[planeIndex] = position;

				baseAngle += (float) (averageAngle * (0.9 + 0.2 * RNG.NextDouble()));
			}

			// Build the original room pixels
			for ( int i=0; i < PIXELS_COUNT; i++ ) {
				float2	C = float2.Zero;
				float	angle = (float) (2.0 * Math.PI * i / PIXELS_COUNT);
				float2	V = new float2( (float) Math.Cos( angle ), (float) Math.Sin( angle ) );

				// Compute intersection with the planes
				float	minHitDistance = float.MaxValue;
				int		hitPlaneIndex = 0;
				for ( int planeIndex=0; planeIndex < planesCount; planeIndex++ ) {
					float2	D = C - planePositions[planeIndex];
					float	hitDistance = -D.Dot( planeNormals[planeIndex] ) / V.Dot( planeNormals[planeIndex] );
					if ( hitDistance > 0.0f && hitDistance < minHitDistance ) {
						minHitDistance = hitDistance;
						hitPlaneIndex = planeIndex;
					}
				}

				m_Pixels[i].Distance = minHitDistance;
				m_Pixels[i].Normal = planeNormals[hitPlaneIndex];
			}

			// Add some obstacles (TODO)


			panelOutput.UpdateBitmap();
		}
	}
}
