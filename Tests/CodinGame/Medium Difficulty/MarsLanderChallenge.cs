// This is my solution to the coding challenge at https://www.codingame.com/ide/puzzle/mars-lander-episode-2
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        int N = int.Parse(Console.ReadLine()); // the number of points used to draw the surface of Mars.

        int[] LandX = new int[N];
        int[] LandY = new int[N];
        for (int i = 0; i < N; i++) {
            inputs = Console.ReadLine().Split(' ');
            LandX[i] = int.Parse(inputs[0]); // X coordinate of a surface point. (0 to 6999)
            LandY[i] = int.Parse(inputs[1]); // Y coordinate of a surface point. By linking all the points together in a sequential fashion, you form the surface of Mars.
        }

        // Find flat landing zone and slopes
        float FlatX0 = 0.0f, FlatX1 = 0.0f;
        float TargetY = 0.0f;
        float Slope0, Slope1;
        for ( int i = 1; i < N; i++ ) {
            if ( Math.Abs( LandY[i] - LandY[i-1] ) < 1 ) {
                FlatX0 = LandX[i-1];
                FlatX1 = LandX[i];
                TargetY = LandY[i];
                
                Slope0 = i > 1 ? (float) (LandY[i-2] - LandY[i-1]) / (LandX[i-2] - LandX[i-1]) : 0.0f;
                Slope1 = i < N-2 ? (float) (LandY[i+1] - LandY[i]) / (LandX[i+1] - LandX[i]) : 0.0f;
                break;
            }
        }

        inputs = Console.ReadLine().Split(' ');
        int X0 = int.Parse(inputs[0]);
        int Y0 = int.Parse(inputs[1]);
        int HS0 = int.Parse(inputs[2]); // the horizontal speed (in m/s), can be negative.
        int VS0 = int.Parse(inputs[3]); // the vertical speed (in m/s), can be negative.
        int F0 = int.Parse(inputs[4]); // the quantity of remaining fuel in liters.
        int R0 = int.Parse(inputs[5]); // the rotation angle in degrees (-90 to 90).
        int P0 = int.Parse(inputs[6]); // the thrust power (0 to 4).

//TargetX = FlatX0 + 1000;

Console.WriteLine( "0 0" );

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int X = int.Parse(inputs[0]);
            int Y = int.Parse(inputs[1]);
            int HS = int.Parse(inputs[2]); // the horizontal speed (in m/s), can be negative.
            int VS = int.Parse(inputs[3]); // the vertical speed (in m/s), can be negative.
            int F = int.Parse(inputs[4]); // the quantity of remaining fuel in liters.
            int R = int.Parse(inputs[5]); // the rotation angle in degrees (-90 to 90).
            int P = int.Parse(inputs[6]); // the thrust power (0 to 4).

//        float   TargetX = Math.Max( FlatX0+400, Math.Min( FlatX1-400, X ));
        float   TargetX = 0.5f * (FlatX0+FlatX1);

            float   CurrentAngle = (float) Math.PI * R / 180.0f;
            float   CurrentAx = (float) Math.Sin( -CurrentAngle ); // Horizontal acceleration
            float   CurrentAy = (float) Math.Cos( CurrentAngle ); // Vertical acceleration

            float   CurrentVx = HS;
            float   CurrentVy = VS;

            float   TimeAhead = 20.0f;   // Plan 10s ahead
            float   ProjectedX = X + TimeAhead * (CurrentVx + 0.5f * TimeAhead * CurrentAx);
            float   ProjectedY = Y + TimeAhead * (CurrentVy + 0.5f * TimeAhead * CurrentAy);
            
Console.Error.WriteLine( "Ax = " + CurrentAx + " - Ay = " + CurrentAy );
Console.Error.WriteLine( "Vx = " + CurrentVx + " - Vy = " + CurrentVy );
Console.Error.WriteLine( "ProjX = " + ProjectedX + " - ProjY = " + ProjectedY );
            
            // Course correct
            float   Vx = ProjectedX - TargetX;
            float   Vy = Math.Max( 0.0f, ProjectedY - TargetY );
            
            // Attenuate angle when close to target
            const float DampingStartDistance = 500.0f;  // Start damping angle at this distance
            const float DampingEndDistance = 100.0f;  // Start damping angle at this distance
            
            float   Dx = X - TargetX;
            float   Dy = Y - TargetY;
            float   DistanceFromTarget = (float) Math.Sqrt( Dx*Dx + Dy*Dy );
            float   AngularDamping = Math.Min( 1.0f, Math.Max( 0.0f, (DistanceFromTarget - DampingEndDistance) / (DampingStartDistance - DampingEndDistance) ) );
                    AngularDamping = (float) Math.Pow( AngularDamping, 4.0 );
    
Console.Error.WriteLine( "DistanceFromTarget = " + DistanceFromTarget + " - AngularDamping = " + AngularDamping );
            
            
            // No matter the choice, engage full thrust below this altitude!
            const float   FullThrustY = 1500;//1500;
    
Console.Error.WriteLine( "TargetX = " + TargetX );
            
Console.Error.WriteLine( "Vx = " + Vx );
Console.Error.WriteLine( "Vy = " + Vy );
            
            float   Angle = AngularDamping * (float) Math.Atan2( Vx, Vy );
            float   PowerX = Math.Min( 4, 2.0f * Vx );
            float   PowerY = 4 - Math.Min( 4, 0.001f * Math.Max( 0, Vy-FullThrustY ) );
            
Console.Error.WriteLine( "PowerX = " + PowerX );
Console.Error.WriteLine( "PowerY = " + PowerY );

            string  AngleDeg = ((int) (180.0f * Angle / Math.PI)).ToString();
            string  PowerSt = ((int) Math.Max( PowerX, PowerY )).ToString();

            Console.WriteLine( AngleDeg + " " + PowerSt ); // R P. R is the desired rotation angle. P is the desired thrust power.
        }
    }
}
