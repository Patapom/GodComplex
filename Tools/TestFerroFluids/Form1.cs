using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using WMath;

namespace TestSPH
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Form1 : Form
	{
		#region CONSTANTS

		public const int	PARTICLES_COUNT = 100;//1+6+12+18;
		public const float	PARTICLES_MASS = 0.01f;			// Assume a mass although we're simulating "centers of spikes" which are theorically mass-less

		public const float	SIMULATION_SPACE_SIZE = 100.0f;
		public const float	SIMULATION_DELTA_TIME = 0.05f;

// 		public const float	DEFAULT_ATTRACTION_FORCE = 500.0f;
// 		public const float	DEFAULT_REPULSION_FORCE = 11.0f;
// 		public const float	DEFAULT_REPULSION_POWER = 5.0f;

		public const float	DEFAULT_ATTRACTION_FORCE = 200.0f;
		public const float	DEFAULT_REPULSION_FORCE = 11.0f;
		public const float	DEFAULT_REPULSION_POWER = 7.0f;
		public const float	DEFAULT_SIZE_FACTOR = 91.0f;

		#endregion

		#region NESTED TYPES

		public struct	Particle
		{
			public Point2D	P;
			public float	Size;
		}

		#endregion

		#region FIELDS

		protected Particle[][]	m_Particles = new Particle[2][]
		{
			new Particle[PARTICLES_COUNT],
			new Particle[PARTICLES_COUNT]
		};

		protected Point2D		m_FieldCenter = new Point2D( 0.0f, 0.0f );
		protected float			m_FieldAmplitude = 0.0f;

		#endregion

		#region METHODS

		public Form1()
		{
			InitializeComponent();

			floatTrackbarControlAttractionFactor.Value = DEFAULT_ATTRACTION_FORCE;
			floatTrackbarControlRepulsionForce.Value = DEFAULT_REPULSION_FORCE;
			floatTrackbarControlRepulsionCoefficient.Value = DEFAULT_REPULSION_POWER;
			floatTrackbarControlSizeFactor.Value = DEFAULT_SIZE_FACTOR;
		}

		protected unsafe override void OnLoad( EventArgs e )
		{
			base.OnLoad( e );

			Reset();
			Application.Idle += new EventHandler( Application_Idle );
		}

		void	Reset()
		{
			m_StepCount = 0;
			for ( int i=0; i < PARTICLES_COUNT; i++ )
			{
				Point2D	Pos = new Point2D( SIMULATION_SPACE_SIZE * (float) SimpleRNG.GetNormal(), SIMULATION_SPACE_SIZE * (float) SimpleRNG.GetNormal() );

				m_Particles[0][i].P = m_Particles[1][i].P = Pos;
				m_Particles[0][i].Size = m_Particles[1][i].Size = 0;
			}
		}

		#endregion

		#region EVENT HANDLERS

		int		m_StepCount = 0;
		float	m_ParticlesSizeFactor;
		void Application_Idle( object sender, EventArgs e )
		{
			if ( !checkBoxSimulate.Checked )
				return;

			float	ForceFactor = SIMULATION_DELTA_TIME*SIMULATION_DELTA_TIME / PARTICLES_MASS;

			float	FieldAttraction = 1.0f / floatTrackbarControlAttractionFactor.Value;
			float	SelfRepulsion = 1.0f / floatTrackbarControlRepulsionForce.Value;
			float	RepulsionPower = -floatTrackbarControlRepulsionCoefficient.Value;
			m_ParticlesSizeFactor = 1.0f / floatTrackbarControlSizeFactor.Value;

			// Apply simulation step
			//
			Point2D		NewPi, Pi, Pj;
			Vector2D	ToCenter_i, ToCenter_j, Pi2Pj;
			Vector2D	Force = new Vector2D( 0.0f, 0.0f );
			float		Distance2Center_i, Distance2CenterSq_i, InvDistance3_i, ParticleSize_i;
			float		Distance2Center_j, Distance2CenterSq_j, ParticleSize_j;
			float		DistanceMin, RepulsionAmplitude;
			float		DistancePi2Pj, DistancePi2PjSq;
			for ( int i=0; i < PARTICLES_COUNT; i++ )
			{
				Pi = m_Particles[0][i].P;
				ToCenter_i = m_FieldCenter - Pi;
				Distance2CenterSq_i = ToCenter_i.SquareMagnitude();
				ParticleSize_i = ComputeParticleSize( (float) Math.Sqrt( Distance2CenterSq_i ) );

				Distance2CenterSq_i *= FieldAttraction;								// Artificially decrease the distance to augment attraction
				Distance2CenterSq_i = Math.Max( 1.0f, Distance2CenterSq_i );		// Limit to avoid infinity

				Distance2Center_i = (float) Math.Sqrt( Distance2CenterSq_i );
				InvDistance3_i = 1.0f / (Distance2CenterSq_i * Distance2Center_i);	// Magnetic force is in r^-3 (actually, r^-2 once r is normalized)
				m_Particles[0][i].Size = ParticleSize_i;

				ToCenter_i.x *= InvDistance3_i;
				ToCenter_i.y *= InvDistance3_i;

				// Initialize force with a will to reach the center of magnetism
				Force.x = ToCenter_i.x;
				Force.y = ToCenter_i.y;

				// Next, account for neighbor particles so they can't get too close from one another !
				for ( int j=0; j < PARTICLES_COUNT; j++ )
					if ( i != j )
					{
						Pj = m_Particles[0][j].P;
						ToCenter_j = m_FieldCenter - Pj;
						Distance2CenterSq_j = ToCenter_j.SquareMagnitude();
						Distance2Center_j = (float) Math.Sqrt( Distance2CenterSq_j );
						ParticleSize_j = ComputeParticleSize( Distance2Center_j );

						Pi2Pj = Pj - Pi;
						DistancePi2PjSq = Pi2Pj.SquareMagnitude();

						// We now have the sizes of each 2 particles and the distance between them
						// We need to devise a repulsion force that will counteract the attraction toward the magnetic center
						DistanceMin = ParticleSize_i + ParticleSize_j;	// This is the minimum distance the particles can come close together


						// Compute repulsion
						DistancePi2Pj = (float) Math.Sqrt( DistancePi2PjSq );
						RepulsionAmplitude = (float) Math.Pow( Math.Max( 1.0f, SelfRepulsion * (DistancePi2Pj - DistanceMin)), RepulsionPower );


						Force.x -= RepulsionAmplitude * Pi2Pj.x;
						Force.y -= RepulsionAmplitude * Pi2Pj.y;
					}

				// Compute resulting acceleration
				Force.x *= ForceFactor;
				Force.y *= ForceFactor;

				// Apply verlet integration (http://www.fisica.uniud.it/~ercolessi/md/md/node21.html)
				NewPi = m_Particles[1][i].P;	// This means we'll need an extra RT since we cannot read from the writing RT !
												// NOTE: NewPi also is OldPi in this case (same read/write buffer containing P(t-Dt) (READ) and that will contain P(t+Dt) AFTER WRITING)

				NewPi.x = 2.0f * Pi.x - NewPi.x + Force.x;
				NewPi.y = 2.0f * Pi.y - NewPi.y + Force.y;
			}

			// Swap
			Particle[]	Temp = m_Particles[0];
			m_Particles[0] = m_Particles[1];
			m_Particles[1] = Temp;

			m_StepCount++;
			panelOutput.m_Time = m_StepCount * SIMULATION_DELTA_TIME;
			panelOutput.m_Center = m_FieldCenter;

			panelOutput.UpdateBitmap( m_Particles[0] );
		}

		/// <summary>
		/// Computes the influence size of the particle based on its distance to the magnetism center
		/// </summary>
		/// <param name="_Distance2Center"></param>
		/// <returns></returns>
		private float	ComputeParticleSize( float _Distance2Center )
		{
			_Distance2Center *= m_ParticlesSizeFactor;
			return 10.0f * (float) Math.Exp( -_Distance2Center * _Distance2Center );
		}

		private void buttonReset_Click( object sender, EventArgs e )
		{
			Reset();
		}

		protected bool	m_bButtonDown = false;
		private void panelOutput_MouseDown( object sender, MouseEventArgs e )
		{
			m_bButtonDown = (e.Button | MouseButtons.Left) != 0;
		}

		private void panelOutput_MouseMove( object sender, MouseEventArgs e )
		{
			if ( !m_bButtonDown )
				return;

			m_FieldCenter.x =  SIMULATION_SPACE_SIZE * (2.0f * e.X / panelOutput.Width - 1.0f);
			m_FieldCenter.y =  SIMULATION_SPACE_SIZE * (1.0f - 2.0f * e.Y / panelOutput.Height);
		}

		private void panelOutput_MouseUp( object sender, MouseEventArgs e )
		{
			if ( (e.Button | MouseButtons.Left) != 0 )
				m_bButtonDown = false;
		}

		private void checkBoxSimulate_CheckedChanged( object sender, EventArgs e )
		{
			timerSimulation.Enabled = checkBoxSimulate.Checked;
		}

		#endregion
	}
}
