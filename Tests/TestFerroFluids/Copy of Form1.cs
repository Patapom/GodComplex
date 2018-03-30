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

namespace TestFerrofluids
{
	/// <summary>
	/// 
	/// </summary>
	public partial class OldForm1 : Form
	{
// 		#region CONSTANTS
// 
// 		public const int	PARTICLES_COUNT = 1+6+12+18;
// 
// 		public const float	SIMULATION_SPACE_SIZE = 100.0f;
// 
// 		#endregion
// 
// 		#region NESTED TYPES
// 
// 		public struct	Particle
// 		{
// 			public Point2D	P;
// 			public float	Density;
// 			public float	Density;
// 		}
// 
// 		#endregion
// 
// 		#region FIELDS
// 
// 		protected Particle[][]	m_Particles = new Particle[2][]
// 		{
// 			new Particle[PARTICLES_COUNT],
// 			new Particle[PARTICLES_COUNT]
// 		};
// 
// 		protected Point2D		m_FieldCenter = new Point2D( 0.0f, 0.0f );
// 
// 		#endregion
// 
// 		#region METHODS
// 
// 		public OldForm1()
// 		{
// 			InitializeComponent();
// 		}
// 
// 		protected unsafe override void OnLoad( EventArgs e )
// 		{
// 			base.OnLoad( e );
// 
// 			Reset();
// 			Application.Idle += new EventHandler( Application_Idle );
// 		}
// 
// 		void	Reset()
// 		{
// 			for ( int i=0; i < PARTICLES_COUNT; i++ )
// 			{
// 				Point2D	Pos = new Point2D( SIMULATION_SPACE_SIZE * (float) SimpleRNG.GetNormal(), SIMULATION_SPACE_SIZE * (float) SimpleRNG.GetNormal() );
// 
// 				m_Particles[0][i].P = m_Particles[1][i].P = Pos;
// 				m_Particles[0][i].Distance = m_Particles[1][i].Distance = float.MaxValue;
// 			}
// 		}
// 
// 		#endregion
// 
// 		#region EVENT HANDLERS
// 
// 		void Application_Idle( object sender, EventArgs e )
// 		{
// 			if ( !checkBoxSimulate.Checked )
// 				return;
// 
// 			// Apply simulation step
// 			// (from http://www.matthiasmueller.info/publications/sca03.pdf)
// 			//
// 			Point2D	Pi, Pj;
// 			float	Rho_i, Rho_j;
// 			float	Pressure_i, Pressure_j;
// 			Point4D	KernelPoly6 = new Point4D();
// 			Point4D	KernelSpiky = new Point4D();
// 			Point4D	KernelViscosity = new Point4D();
// 
// 			for ( int i=0; i < PARTICLES_COUNT; i++ )
// 			{
// 				Pi = m_Particles[0][i].P;
// 				Rho_i = m_Particles[0][i].Density;
// 				Pressure_i = GAS_CONSTANT * (Rho_i - PARTICLE_REST_DENSITY);
// 
// 				for ( int j=0; j < PARTICLES_COUNT; j++ )
// 					if ( i != j )
// 					{
// 						Pj = m_Particles[0][j].P;
// 						if ( !ComputeKernels( Pi, Pj, KernelPoly6, KernelSpiky, KernelViscosity ) )
// 							continue;
// 
// 						Rho_j = m_Particles[0][j].Density;
// 						Pressure_j = GAS_CONSTANT * (Rho_j - PARTICLE_REST_DENSITY);
// 
// 					}
// 			}
// 
// 			// Swap
// 			Particle[]	Temp = m_Particles[0];
// 			m_Particles[0] = m_Particles[1];
// 			m_Particles[1] = Temp;
// 
// 			panelOutput.UpdateBitmap( m_Particles[0] );
// 		}
// 
// 		/// <summary>
// 		/// Computes the kernel operations
// 		/// </summary>
// 		/// <param name="_P0"></param>
// 		/// <param name="_P1"></param>
// 		/// <param name="_Kernel"></param>
// 		protected bool	ComputeKernels( Point2D _P0, Point2D _P1, Point4D _KernelPoly6, Point4D _KernelPressure, Point4D _KernelViscosity )
// 		{
// 			float	r2 = (_P1 - _P0).SquareMagnitude();
// 			float	DeltaSq = PARTICLE_SIZE_SQ - r2;	// h² - r²
// 			if ( DeltaSq < 0.0f )
// 				return false;
// 
// 			float	r = (float) Math.Sqrt( r2 );
// 
// 			float	DeltaSq_h3 = DeltaSq * PARTICLE_SIZE_INV_POW3;
// 			_KernelPoly6.x = 1.5666814710608447114749495456982f * DeltaSq_h3 * DeltaSq_h3 * DeltaSq_h3;	// 315/(64.PI.h^9) * (h² - r²)^3
// 
// 
// 
// 			float	Delta_h2 = (PARTICLE_SIZE - r) * PARTICLE_SIZE_INV_POW2;
// 			_KernelPressure.x = 4.7746482927568600730665129011754f * Delta_h2 * Delta_h2 * Delta_h2;	// 15/(PI.h^6) * (h - r)^3
// 
// 			_KernelViscosity.x = 2.3873241463784300365332564505877f * PARTICLE_SIZE_INV_POW3 * (-0.5f*PARTICLE_SIZE_INV_POW3 * r2*r + PARTICLE_SIZE_INV_POW2 * r2 + 0.5f * PARTICLE_SIZE / r - 1.0f);	// 15/(2.PI.h^3) * (-r^3/2h^3 + r²/h² + h/2r - 1)
// 
// 			return true;
// 		}
// 
// 		private void buttonReset_Click( object sender, EventArgs e )
// 		{
// 			Reset();
// 		}
// 
// 		private void floatTrackbarControlCloudExtinction_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
// 		{
// 		}
// 
// 		private void floatTrackbarControlOpacityCoefficient_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
// 		{
// 		}
// 
// 		private void floatTrackbarControlStepSize_ValueChanged( Nuaj.Cirrus.Utility.FloatTrackbarControl _Sender, float _fFormerValue )
// 		{
// 		}
// 
// 		private void panelOutput_MouseDown( object sender, MouseEventArgs e )
// 		{
// 			m_FieldCenter.x =  SIMULATION_SPACE_SIZE * (2.0f * e.X / panelOutput.Width - 1.0f);
// 			m_FieldCenter.y =  SIMULATION_SPACE_SIZE * (1.0f - 2.0f * e.Y / panelOutput.Height);
// 		}
// 
// 		#endregion
	}
}
