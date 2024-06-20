#include "Global.h"

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Measures the distance in meters using the HC-SR04
// According to the documentation, we have to send a 10µs pulse to the trigger pin to send an echo,
//  then depending on the time before we receive an echo, we can measure the distance
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//

void  SetupPins_HCSR04( U8 _pinTrigger, U8 _pinEcho ) {
  pinMode( _pinTrigger, OUTPUT );
  digitalWrite( _pinTrigger, LOW );
  delayMicroseconds( 2 );
  pinMode( _pinEcho, INPUT );
}

// Returns the raw time in µs before we receive an echo (this is the total round trip back and forth Time Of Flight), or 0 if the pulse timed out
U32 MeasureEchoTime( U8 _pinTrigger, U8 _pinEcho ) {
  digitalWrite( _pinTrigger, HIGH );
  delayMicroseconds( 10 );
  digitalWrite( _pinTrigger, LOW );
  U32 duration_microSeconds = pulseIn( _pinEcho, HIGH, 1000000 ); // Timeout after 1s
  return duration_microSeconds;
}

float ConvertTimeOfFlightToDistance( U32 _timeOfFlight_microseconds, float _speedOfSound_metersPerSecond ) {
  return _timeOfFlight_microseconds * _speedOfSound_metersPerSecond * (5e-7f);  // The 5e-7 constant is used to convert the total time of flight in microseconds into seconds for half the time of flight
}
float ConvertTimeOfFlightToDistance( U32 _timeOfFlight_microseconds ) {
  return ConvertTimeOfFlightToDistance( _timeOfFlight_microseconds, 343.4f );  // Default speed of sound at 1 bar and 20°C in dry air is 343.4 m/s
}

float MeasureDistance( U8 _pinTrigger, U8 _pinEcho ) {
  U32 time_microSeconds = MeasureEchoTime( _pinTrigger, _pinEcho );
  if ( time_microSeconds > 38000 )
    return -1;  // Out of range!
  else if ( time_microSeconds == 0 )
    return -2;  // Pulse timed out
  return ConvertTimeOfFlightToDistance( time_microSeconds );
}
