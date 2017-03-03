// This is my solution to the coding challenge at https://www.codingame.com/ide/puzzle/mars-lander-episode-2
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution
{
	public class	LanderSimulator {

		const float		TANGENT_STRENGTH_START = 10.0f;
		const float		TANGENT_STRENGTH_END = 100.0f;
		const float		DELTA_TIME = 1.0f;	// Assume a 1s tick

		// Landing phase occurs in 3 states:
		public enum STATE {
			ASCENDING,	// Ascending part of the parabola
			DESCENDING,	// Descending part of the parabola
			LANDING,	// Vertical landing
		}

		/// <summary>
		/// Attempts to reach the target angle and thrust by following small increments
		/// </summary>
		/// <param name="_targetAngle">In radians</param>
		/// <param name="_targetThrust"></param>
		/// <param name="_newAngle"></param>
		/// <param name="_newThrust"></param>
		static void		NewAngleAndThrust( float _targetAngle, float _targetThrust, ref int _newAngle, ref int _newThrust ) {
			_targetAngle = (int) (180 * _targetAngle / Math.PI);
			_targetAngle = Math.Max( -90, Math.Min( 90, _targetAngle ) );
			_targetThrust = Math.Max( 0, Math.Min( 4, _targetThrust ) );

			// Ensure steps of 15° close to existing angle
			int	targetAngle = 15 * (int) Math.Round( _targetAngle / 15.0f );
				targetAngle = Math.Max( _newAngle-15, targetAngle );
				targetAngle = Math.Min( _newAngle+15, targetAngle );

			// Ensure unit increments close to existing thrust
			int	targetThrust = (int) Math.Round( _targetThrust );
				targetThrust = Math.Max( _newThrust-1, targetThrust );
				targetThrust = Math.Min( _newThrust+1, targetThrust );

			_newAngle = targetAngle;
			_newThrust = targetThrust;
		}

		// Prefered trajectory
		public float	m_startX;
		public float	m_startY;
		public float	m_tangentStartX;
		public float	m_tangentStartY;

		public float	m_targetX;
		public float	m_targetY;
		public float	m_targetLandY;
		public float	m_tangentTargetX;
		public float	m_tangentTargetY;

		public float	m_peakTime = 0.0f;
		public float	m_curveLength = 0.0f;

		// PID
		public float	m_sumErrorX = 0.0f;
		public float	m_sumErrorY = 0.0f;
		public float	m_previousErrorX = 0.0f;
		public float	m_previousErrorY = 0.0f;

		// Current state
 		public STATE	m_state = STATE.ASCENDING;
		public float	m_time = 0.0f;

		public LanderSimulator( string _initialLine ) {
			int N = int.Parse( _initialLine ); // the number of points used to draw the surface of Mars.

			int[]	landX = new int[N];
			int[]	landY = new int[N];
			int		landMinY = int.MaxValue;
			int		landMaxY = -int.MaxValue;
			float	landAvgY = 0.0f;
			for (int i = 0; i < N; i++) {
				string	line = Console.ReadLine();
Console.Error.WriteLine( line );
				string[]	inputs = line.Split(' ');
				landX[i] = int.Parse(inputs[0]); // X coordinate of a surface point. (0 to 6999)
				landY[i] = int.Parse(inputs[1]); // Y coordinate of a surface point. By linking all the points together in a sequential fashion, you form the surface of Mars.

				landMinY = Math.Min( landMinY, landY[i] );
				landMaxY = Math.Max( landMaxY, landY[i] );
				landAvgY += landY[i];
			}
			landAvgY /= N;

			// Find flat landing zone and slopes
			float FlatX0 = 0.0f, FlatX1 = 0.0f;
			float FlatY = 0.0f;
			float Slope0, Slope1;
			for ( int i = 1; i < N; i++ ) {
				if ( Math.Abs( landY[i] - landY[i-1] ) < 1 ) {
					FlatX0 = landX[i-1];
					FlatX1 = landX[i];
					FlatY = landY[i];

					Slope0 = i > 1 ? (float) (landY[i-2] - landY[i-1]) / (landX[i-2] - landX[i-1]) : 0.0f;
					Slope1 = i < N-2 ? (float) (landY[i+1] - landY[i]) / (landX[i+1] - landX[i]) : 0.0f;
					break;
				}
			}

//			m_targetX = Math.Max( FlatX0+400, Math.Min( FlatX1-400, X ));	// Closest
			m_targetX = 0.5f * (FlatX0+FlatX1);		// Center of the flat pad
//			m_targetY = FlatY;
//			m_targetY = 0.75f * landAvgY;			// Center high above ground
			m_targetY = 0.75f * landMaxY;			// Center high above ground
			m_targetLandY = FlatY;
		}

		public LanderSimulator( LanderSimulator _other ) {
			m_startX = _other.m_startX;
			m_startY = _other.m_startY;
			m_tangentStartX = _other.m_tangentStartX;
			m_tangentStartY = _other.m_tangentStartY;
			m_targetX = _other.m_targetX;
			m_targetY = _other.m_targetY;
			m_tangentTargetX = _other.m_tangentTargetX;
			m_tangentTargetY = _other.m_tangentTargetY;

			m_peakTime = _other.m_peakTime;
			m_curveLength = _other.m_curveLength;
		}

		bool	m_firstTime = true;
		void		Init( float PX, float PY, float VX, float VY, int _angle, int _thrust ) {
			float	angle = (float) (Math.PI * _angle / 180.0f);

			m_startX = PX;
			m_startY = PY;
			m_tangentStartX = TANGENT_STRENGTH_START * VX;
			m_tangentStartY = TANGENT_STRENGTH_START * VY;

			m_tangentTargetX = 0 * TANGENT_STRENGTH_END;
			m_tangentTargetY = -100.0f * TANGENT_STRENGTH_END;

			// Pre-compute curve length and peak height/time for phase change
			{
				float	posX, posY, temp;
				float	peakY = -float.MaxValue;
				SplineTrajectory( 0, out posX, out posY, out temp, out temp );
				for ( int i=1; i < 1000; i++ ) {
					float	oldPosX = posX, oldPosY = posY;
					SplineTrajectory( i / 1000.0f, out posX, out posY, out temp, out temp );

					float	Dx = posX - oldPosX;
					float	Dy = posY - oldPosY;
					float	dL = (float) Math.Sqrt( Dx*Dx + Dy*Dy );
					m_curveLength += dL;

					if ( posY > peakY ) {
						// Store peak Y
						peakY = posY;
						m_peakTime = 0.001f * i;
					}
				}
			}

			// Assuming a constant velocity, precompute time required to complete curve
//			const float	CAILLOU = 2.0f;
//			m_curveTime = CAILLOU * m_curveLength / 2.0f;
		}

		public void		Plan( float PX, float PY, float VX, float VY, ref int _angle, ref int _thrust ) {
			if ( m_firstTime ) {
				Init( PX, PY, VX, VY, _angle, _thrust );
				m_firstTime = false;
			}

			m_time += DELTA_TIME;	

			float	angle = (float) (Math.PI * _angle / 180.0f);

			// Estimate curve time, expected position and velocity
//			float	t = m_time / m_curveTime;
			float	t = MapXToTime( PX, VX );
			float	expectedPX, expectedPY, expectedVX, expectedVY;
			SplineTrajectory( t, out expectedPX, out expectedPY, out expectedVX, out expectedVY );

			// Update state
			if ( m_state != STATE.LANDING ) {
				if ( t < m_peakTime )
					m_state = STATE.ASCENDING;
				else {
					if ( t < 0.90f && PY > m_targetY )
						m_state = STATE.DESCENDING;
					else
						m_state = STATE.LANDING;
				}
			}

			switch ( m_state ) {
				case STATE.LANDING:
					expectedVX = 0;
					expectedVY = 0;
					expectedPX = m_targetX;
					expectedPY = m_targetLandY;
					break;
			}

			// Compute error in position and velocity compared to expected values
			float	deltaPX = expectedPX - PX;
			float	deltaPY = expectedPY - PY;
			float	deltaVX = expectedVX - VX;
			float	deltaVY = expectedVY - VY;
// 			float	sqDeltaPosition = deltaPX*deltaPX + deltaPY*deltaPY;
// 			float	sqDeltaVelocity = deltaVX*deltaVX + deltaVY*deltaVY;
// 			float	error = 1.0f * sqDeltaPosition + 0.0f * sqDeltaVelocity;

float	SCALE_X = ms_ownerForm.floatTrackbarControl1.Value;
float	SCALE_Y = ms_ownerForm.floatTrackbarControl2.Value;
float	SCALE_POS = ms_ownerForm.floatTrackbarControl3.Value;
float	SCALE_VEL = ms_ownerForm.floatTrackbarControl4.Value;

			float	errorX = SCALE_X * SCALE_POS * deltaPX + SCALE_X * SCALE_VEL * deltaVX;
			float	errorY = SCALE_Y * SCALE_POS * deltaPY + SCALE_Y * SCALE_VEL * deltaVY;

// 			float	errorX = deltaPX;
// 			float	errorY = deltaPY;

//Console.Error.WriteLine( "Error = " + errorX + ", " + errorY );

// 			// PID
// 			const float	K_P = 1.0f;
// 			const float	K_I = 1.0f;
// 			const float	K_D = 0.0f;
// 
// 			float	errorDerivativeX = (errorX - m_previousErrorX) / DELTA_TIME;
// 			float	errorDerivativeY = (errorY - m_previousErrorY) / DELTA_TIME;
// 			float	errorIntegralX = m_sumErrorX / m_time;
// 			float	errorIntegralY = m_sumErrorY / m_time;
// 			float	errorPID_X = K_P * errorX + K_I * errorIntegralX + K_D * errorDerivativeX;
// 			float	errorPID_Y = K_P * errorY + K_I * errorIntegralY + K_D * errorDerivativeY;
// 
// 			m_previousErrorX = errorX;
// 			m_previousErrorY = errorY;
// 			m_sumErrorX += errorX;
// 			m_sumErrorY += errorY;

			// Convert errors into target velocity
float	BISOU =  ms_ownerForm.floatTrackbarControl5.Value;

			float	targetVelX = BISOU * errorX;
//			float	targetVelY = BISOU * errorY + ms_ownerForm.floatTrackbarControl6.Value * 3.711f;
			float	targetVelY = Math.Max( BISOU * errorY, ms_ownerForm.floatTrackbarControl6.Value * 3.711f );

			float	targetAngle = (float) Math.Atan2( targetVelX, targetVelY );
			float	targetThrust = (float) Math.Ceiling( Math.Sqrt( targetVelX*targetVelX + targetVelY*targetVelY ) );

			NewAngleAndThrust( targetAngle, targetThrust, ref _angle, ref _thrust );
		}

		#region Spline

		// I didn't try to find the roots, just used brute force here
		public float	MapXToTime( float PX, float VX ) {
			float	closestPos = float.MaxValue;
			float	closestTime = 0.0f;
			float	posX, posY, velX, velY;
			for ( int i=0; i < 1000; i++ ) {
				SplineTrajectory( i / 1000.0f, out posX, out posY, out velX, out velY );
				float	deltaPos = Math.Abs( posX - PX ) + 0*Math.Abs( velX - VX );
				if ( deltaPos < closestPos ) {
					closestPos = deltaPos;
					closestTime = i;
				}
			}
			closestTime *= 0.001f;
			return closestTime;
		}

		// From https://en.wikipedia.org/wiki/Cubic_Hermite_spline
		public void	SplineTrajectory( float t, out float PX, out float PY, out float VX, out float VY ) {
			t = Math.Max( 1e-3f, Math.Min( 1.0f, t ) );

			float	t2 = t * t;
			float	a = 1 + t2 * (-3.0f + t * 2.0f);	// 1 - 3t² + 2t^3
			float	b = t * (1.0f + t * (-2.0f + t));	// t - 2t² + t^3
			float	c = t2 * (3.0f + t * -2.0f);		// 3t² - 2t^3
			float	d = t2 * (-1.0f + t);				// -t² + t^3

			PX = a * m_startX + b * m_tangentStartX + c * m_targetX + d * m_tangentTargetX;
			PY = a * m_startY + b * m_tangentStartY + c * m_targetY + d * m_tangentTargetY;

			float	da = 6.0f * t * (-1.0f + t);		// -6t + 6t²
			float	db = 1.0f + t * (-4.0f + 3.0f * t);	// 1 - 4t + 3t²
			float	dc = 6.0f * (t - t2);				// 6t - 6t²
			float	dd = -2.0f * t + 3.0f * t2;			// -2t + 3t²
			VX = da * m_startX + db * m_tangentStartX + dc * m_targetX + dd * m_tangentTargetX;
			VY = da * m_startY + db * m_tangentStartY + dc * m_targetY + dd * m_tangentTargetY;

			VX /= TANGENT_STRENGTH_END;
			VY /= TANGENT_STRENGTH_END;
		}

		#endregion
	}

	public static LanderSimulator	ms_simulation = null;

#if MEUH
	static TestForm.MarsLanderForm ms_ownerForm;
	public static void Meuh(string[] args, TestForm.MarsLanderForm _ownerForm) {
		ms_ownerForm = _ownerForm;
#else
	public static void Main(string[] args) {
#endif
		string[] inputs;
		string	line = Console.ReadLine();
Console.Error.WriteLine( line );

		ms_simulation = new LanderSimulator( line );

		// game loop
		while (true) {
			line = Console.ReadLine();
//Console.Error.WriteLine( line );
			inputs = line.Split(' ');
			int		X = int.Parse(inputs[0]);
			int		Y = int.Parse(inputs[1]);
			int		HS = int.Parse(inputs[2]); // the horizontal speed (in m/s), can be negative.
			int		VS = int.Parse(inputs[3]); // the vertical speed (in m/s), can be negative.
			int		F = int.Parse(inputs[4]); // the quantity of remaining fuel in liters.
			int		R = int.Parse(inputs[5]); // the rotation angle in degrees (-90 to 90).
			int		P = int.Parse(inputs[6]); // the thrust power (0 to 4).

// 			float   CurrentAngle = (float) Math.PI * R / 180.0f;
// 			float   CurrentAx = (float) Math.Sin( -CurrentAngle ); // Horizontal acceleration
// 			float   CurrentAy = (float) Math.Cos( CurrentAngle ); // Vertical acceleration
// 
// 			float   CurrentVx = HS;
// 			float   CurrentVy = VS;
// 
// 			float   TimeAhead = 20.0f;   // Plan 10s ahead
// 			float   ProjectedX = X + TimeAhead * (CurrentVx + 0.5f * TimeAhead * CurrentAx);
// 			float   ProjectedY = Y + TimeAhead * (CurrentVy + 0.5f * TimeAhead * CurrentAy);
// 
// Console.Error.WriteLine( "Ax = " + CurrentAx + " - Ay = " + CurrentAy );
// Console.Error.WriteLine( "Vx = " + CurrentVx + " - Vy = " + CurrentVy );
// Console.Error.WriteLine( "ProjX = " + ProjectedX + " - ProjY = " + ProjectedY );
// 
// 			// Course correct
// 			float   ErrorX = ProjectedX - TargetX;
// 			float   ErrorY = Math.Max( 0.0f, ProjectedY - TargetY );
// 			
// 			// Attenuate angle when close to target
// 			const float DampingStartDistance = 500.0f;  // Start damping angle at this distance
// 			const float DampingEndDistance = 100.0f;  // Start damping angle at this distance
// 			
// 			float	Dx = X - TargetX;
// 			float	Dy = Y - TargetY;
// 			float	DistanceFromTarget = (float) Math.Sqrt( Dx*Dx + Dy*Dy );
// 			float	AngularDamping = Math.Min( 1.0f, Math.Max( 0.0f, (DistanceFromTarget - DampingEndDistance) / (DampingStartDistance - DampingEndDistance) ) );
// 					AngularDamping = (float) Math.Pow( AngularDamping, 4.0 );
// 
// Console.Error.WriteLine( "DistanceFromTarget = " + DistanceFromTarget + " - AngularDamping = " + AngularDamping );
// 
// 
// 			// No matter the choice, engage full thrust below this altitude!
// 			const float   FullThrustY = 1500;//1500;
// 
// Console.Error.WriteLine( "TargetX = " + TargetX );
// 
// Console.Error.WriteLine( "Error = " + ErrorX + ", " + ErrorY );
// 
// 			float   Angle = AngularDamping * (float) Math.Atan2( ErrorX, ErrorY );
// 			float   PowerX = Math.Min( 4, 2.0f * ErrorX );
// 			float   PowerY = 4 - Math.Min( 4, 0.001f * Math.Max( 0, ErrorY-FullThrustY ) );
// 
// Console.Error.WriteLine( "PowerX = " + PowerX );
// Console.Error.WriteLine( "PowerY = " + PowerY );
// 
// 			string  AngleDeg = ((int) (180.0f * Angle / Math.PI)).ToString();
// 			string  PowerSt = ((int) Math.Max( PowerX, PowerY )).ToString();

			int	newRotation = R;
			int	newThrust = P;
			ms_simulation.Plan( X, Y, HS, VS, ref newRotation, ref newThrust );
			Console.WriteLine( newRotation + " " + newThrust );
		}
	}
}
