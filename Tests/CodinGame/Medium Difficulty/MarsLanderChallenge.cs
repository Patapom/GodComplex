// This is my solution to the coding challenge at https://www.codingame.com/ide/puzzle/mars-lander-episode-2
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Solution
{
	// Landing phase occurs in 3 states:
	enum STATE {
		ASCENDING,	// Ascending part of the parabola
		DESCENDING,	// Descending part of the parabola
		LANDING,	// Vertical landing
	}
	STATE				m_state = STATE.ASCENDING;

	public static float	m_targetX;
	public static float	m_targetY;

	public static void		Plan( float PX, float PY, float VX, float VY, float _angle, float _thrust, out int _newAngle, out int _newThrust ) {
		_newAngle = (int) (180 * _angle / Math.PI);
		_newThrust = (int) _thrust;
	}

#if MEUH
	public static void Meuh(string[] args) {
#else
	public static void Main(string[] args) {
#endif
		string[] inputs;
		string	line = Console.ReadLine();
Console.Error.WriteLine( line );
		int N = int.Parse( line ); // the number of points used to draw the surface of Mars.

		int[]	LandX = new int[N];
		int[]	LandY = new int[N];
		int		landMinY = int.MaxValue;
		int		landMaxY = -int.MaxValue;
		float	landAvgY = 0.0f;
		for (int i = 0; i < N; i++) {
			line = Console.ReadLine();
Console.Error.WriteLine( line );
			inputs = line.Split(' ');
			LandX[i] = int.Parse(inputs[0]); // X coordinate of a surface point. (0 to 6999)
			LandY[i] = int.Parse(inputs[1]); // Y coordinate of a surface point. By linking all the points together in a sequential fashion, you form the surface of Mars.

			landMinY = Math.Min( landMinY, LandY[i] );
			landMaxY = Math.Max( landMaxY, LandY[i] );
			landAvgY += LandY[i];
		}
		landAvgY /= N;

		// Find flat landing zone and slopes
		float FlatX0 = 0.0f, FlatX1 = 0.0f;
		float FlatY = 0.0f;
		float Slope0, Slope1;
		for ( int i = 1; i < N; i++ ) {
			if ( Math.Abs( LandY[i] - LandY[i-1] ) < 1 ) {
				FlatX0 = LandX[i-1];
				FlatX1 = LandX[i];
				FlatY = LandY[i];

				Slope0 = i > 1 ? (float) (LandY[i-2] - LandY[i-1]) / (LandX[i-2] - LandX[i-1]) : 0.0f;
				Slope1 = i < N-2 ? (float) (LandY[i+1] - LandY[i]) / (LandX[i+1] - LandX[i]) : 0.0f;
				break;
			}
		}

//		m_targetX = Math.Max( FlatX0+400, Math.Min( FlatX1-400, X ));	// Closest
		m_targetX = 0.5f * (FlatX0+FlatX1);	// Center of the flat pad
//		m_targetY = FlatY;
//		m_targetY = 0.75f * landAvgY;			// Center high above ground
		m_targetY = 0.75f * landMaxY;			// Center high above ground

		// game loop
		while (true) {
		line = Console.ReadLine();
Console.Error.WriteLine( line );
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

			int	newRotation, newThrust;
			Plan( X, Y, HS, VS, (float) Math.PI * R / 180.0f, P, out newRotation, out newThrust );

//			Console.WriteLine( AngleDeg + " " + PowerSt ); // R P. R is the desired rotation angle. P is the desired thrust power.
//Console.WriteLine( R + " " + P );
Console.WriteLine( newRotation + " " + newThrust );
		}
	}
}
