////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This code is designed to either act as a server (i.e. RECEIVER) or a client (i.e. TRANSMITTER)
//  • The server side is located on the local PC, it sends commands to the clients which respond in turn
//  • The client side responds to the server's command by executing the commands and sending a response
//    ☼ Typically, a client will host various sensors that can be triggered and/or interrogated on demand by the server
//    ☼ The client-side logic should be minimal: wait for server commands to execute, execute them and send the response...
//
// Switch between compiling a server or a client by defining or not the TRANSMITTER macro in Global.h
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
#include "Global.h"

static char* str::ms_globalBuffer = s_globalBuffer;

SoftwareSerial  LoRa( PIN_RX, PIN_TX );

Time_ms startTime;  // Time at which the loop starts

void setup() {
  pinMode( PIN_LED_RED, OUTPUT );
  pinMode( PIN_LED_GREEN, OUTPUT );

  // Setup the HC-SR04
  SetupPins_HCSR04( PIN_HCSR04_TRIGGER, PIN_HCSR04_ECHO );

  // Initiate serial communication
  Serial.begin( 19200 );        // This one is connected to the PC
  while ( !Serial );            // Wait for serial port to connect. Needed for Native USB only

  Flash( PIN_LED_GREEN, 50, 10 );

/*  Test LEDS
while ( true ) {
  Flash( PIN_LED_RED, 100, 10 );
  Flash( PIN_LED_GREEN, 100, 10 );
}
//*/

/* Test SR04
while ( true ) {
//  Log( "Allo?" );
  float distance = MeasureDistance( PIN_HCSR04_TRIGGER, PIN_HCSR04_ECHO );
  Log( str( "distance = %d", distance );
  delay( 100 );
}
//*/

/*  // Hardware reset LoRa module
  pinMode( PIN_RESET, OUTPUT );
  digitalWrite( PIN_RESET, LOW );
  delay( 200 ); // Advised value is at least 100ms
  digitalWrite( PIN_RESET, HIGH );
*/

  LoRa.begin( LORA_BAUD_RATE ); // This software serial is connected to the LoRa module

/* Software reset takes an annoyingly long time...
  SendCommandAndWaitPrint( "AT+RESET" );  // Normally useless due to hard reset above
  delay( 5000 );
//*/

  #ifdef DEBUG_LIGHT
    Log();
    Log( "Initializing..." );
  #endif

  // Initialize the LoRa module
  #ifdef TRANSMITTER
    CONFIG_RESULT configResult = ConfigureLoRaModule( NETWORK_ID, TRANSMITTER );
  #else
    CONFIG_RESULT configResult = ConfigureLoRaModule( NETWORK_ID, RECEIVER_ADDRESS );
  #endif

  #ifdef DEBUG_LIGHT
    if ( configResult == CR_OK ) {
      Log( "Configuration successful!" );
    } else {
      LogError( str( "Configuration failed with code %d", (int) configResult ) );
    }
  #endif

  // Enable "smart mode"
  #if defined(TRANSMITTER) && defined(USE_SMART_MODE)
    configResult = SetWorkingMode( WM_SMART, 1000, 10000 ); // Sleep for 10 seconds, active for 1 second
    if ( configResult == CR_OK ) {
      Log( "Smart mode successful!" );
    } else {
      LogError( str( "Smart mode failed with code %d", (int) configResult ) );
    }
  #endif

  if ( configResult != CR_OK ) {
    // In error...
    while ( true ) {
      Flash( PIN_LED_RED, 250, 1 );
    }
  }

// Optional password encryption
//  SendCommandAndWaitPrint( "AT+CPIN?" );
//  ClearPassword();  Doesn't work! We must reset the chip to properly clear the password...
//  SetPassword( 0x1234ABCDU );
//  SendCommandAndWaitPrint( "AT+CPIN?" );

  // Store start time
  GetTime( startTime );
}

U32 runCounter = 0; // How many cycles did we execute?

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// SERVER SIDE
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
#ifndef TRANSMITTER

// Represents a single water level measurement at a given time
struct Measurement {
  U32 timeStamp_seconds;    // The time of the measurement
  U32 rawTime_microSeconds; // The raw time of flight from the sensor
};

//Measurement bufferMeasurements[256];  // The ring buffer of 256 measurements (too large!)
Measurement bufferMeasurements[MAX_MEASUREMENTS];  // The ring buffer of measurements
U32   measurementsCounter = 0;
U32   measurementErrorsCounter = 0;

U32   cyclesCounter = ~0UL;
U32   timeReference_seconds = 0;  // Set by the user. 0 is the date 2024-06-12 at 00:00. A U32 can then count time in seconds up to 136 years...

void loop() {
//  static U16  commandID = 0;

  runCounter++;
  delay( 1000 );  // Each cycle is 1000ms
//  digitalWrite( PIN_LED_GREEN, runCounter & 1 );

  // Ask client to measure a distance every 15 minutes...
  cyclesCounter++;
  if ( cyclesCounter >= 15 * 60 ) {
    cyclesCounter = 0;  // Reset counter
    if ( ExecuteCommand_MeasureDistance( 0, false ) ) {
      Flash( PIN_LED_GREEN, 250, 1 ); // Notify of a new measurement
    } else {
      Flash( PIN_LED_RED, 150, 10 );  // Notify of a command failure!
    }
  }

  // Notify if client module has been returning too many successive failed measurement commands!
  if ( measurementErrorsCounter > 4 ) {
    digitalWrite( PIN_LED_RED, runCounter & 2 );  // Blink every 2 seconds
  }

  // Wait for a command from the driving application
  if ( !Serial.available() )
    return;

  char  commandBuffer[32];
  char* command = commandBuffer;
  U16   commandID = 0;  // Invalid ID

  char  C = 1;
  while ( C >= 0 && C != '\n' ) {
    C = Serial.read();
    if ( C == ',' && commandID == 0 ) {
      // Read command ID and restart at the beginning of the buffer to store the actual command name
      *command++ = '\0';  // Terminate string
      commandID = atoi( commandBuffer );
      command = commandBuffer;
    } else {
      *command++ = C;
    }
  }
  if ( C == '\n' )
    command[-1] = '\0';  // Replace '\n' with proper string terminator
  command = commandBuffer;

  #ifdef DEBUG_LIGHT
    LogDebug( str( "ID %d, \"%s\"", commandID, command ) );
  #endif

  if ( strstr( command, "PING" ) == command ) {
    // Perform a simple ping
    char* reply = ExecuteAndWaitReply( 1, "PING", commandID, "" );
    if ( reply != NULL ) {
      LogReply( commandID, "Ping" );
    } else {
      LogError( commandID, "Ping failed" );
    }
  } else if ( strstr( command, "MEASURE" ) == command ) {
    // Perform a measurement
    bool  forceOutOfRange = false;
    #ifdef DEBUG_LIGHT
      if ( strstr( command + 7, "PIPO" ) == command+7 ) {
        forceOutOfRange = true;
      }
    #endif

    if ( ExecuteCommand_MeasureDistance( commandID, forceOutOfRange ) ) {
      measurementsCounter--;  // Don't stack it into the buffer
      LogReply( commandID, str( "TIME=%d", bufferMeasurements[measurementsCounter & (MAX_MEASUREMENTS-1)].rawTime_microSeconds ) );
    } else {
      LogError( commandID, "Measure failed." );
    }

  } else if ( strstr( command, "SETTIME=" ) == command ) {
    // Sets the reference time
    timeReference_seconds = atoi( command + 8 );
    LogReply( commandID, str( "New reference time set to 0x%08X", timeReference_seconds ) );

  } else if ( strstr( command, "GETBUFFERSIZE" ) == command ) {
    // Return the amount of measurements in the buffer
    LogReply( commandID, str( "%d", measurementsCounter < MAX_MEASUREMENTS ? measurementsCounter : MAX_MEASUREMENTS ) );

  } else if ( strstr( command, "READBUFFER" ) == command ) {
    // Return the content of the buffer
    U32 count = measurementsCounter < MAX_MEASUREMENTS ? measurementsCounter : MAX_MEASUREMENTS;
    U8  bufferIndex = (measurementsCounter - count) & (MAX_MEASUREMENTS-1); // Index of the first measurement in the ring buffer

    LogReply( commandID, str( "%d", count ) );

    U32 checksum = 0;
    for ( U8 i=0; i < count; i++, bufferIndex++ ) {
      Measurement&  m = bufferMeasurements[bufferIndex];
      LogReply( commandID, str( "%d;%d", m.timeStamp_seconds, m.rawTime_microSeconds ) );

      checksum += m.timeStamp_seconds;
      checksum += m.rawTime_microSeconds;
    }

    // Send final checksum
    LogReply( commandID, str( "CHECKSUM=%08X", checksum ) );

  } else if ( strstr( command, "FLUSH" ) == command ) {
    // Flush the buffer
    measurementsCounter = 0;
    measurementErrorsCounter = 0;
    LogReply( commandID );
  }
}

////////////////////////////////////////////////////////////////
// Command Execution
//
bool  ExecuteCommand_MeasureDistance( U16 _commandID, bool _forceOutOfRange ) {

  U16 rawTime_microSeconds = ~0U;
  U16 retriesCount = 0;
  while ( rawTime_microSeconds == ~0U && retriesCount < 5 ) { // Retry while we're getting an out of range response...
    if ( retriesCount > 0 ) {
      delay( 100 );
    }

    #if 0 // At the moment the sensor is busted so let's just simulate a fake measure command
      char* reply = ExecuteAndWaitReply( 1, "PING", _commandID, "" );

      // Fake reply with a sawtooth signal ...
      if ( reply != NULL ) {
        const int MIN =  1772;  // 1772µs at top tank level
        const int MAX = 11848;  // 11848µs at bottom tank level
//      rawTime_microSeconds = MAX + ((cyclesCounter & 0x3FF) * (MIN - MAX) >> 10);
//        rawTime_microSeconds = MAX + ((cyclesCounter & 0x3F) * (MIN - MAX) >> 6);
        rawTime_microSeconds = U16( MAX + (1.0 + sin( cyclesCounter * (3.1415 / 64.0) )) * float(MIN - MAX) / 2.0);
        reply = str( "DST0,%04X,%d", _commandID, rawTime_microSeconds );
      }
    #else
      // Execute the command and wait for the reply
      char* reply = ExecuteAndWaitReply( 1, "DST0", _commandID, "" );
    #endif

    if ( reply == NULL ) {
      // Command failed after several attempts!
LogDebug( "reply == NULL!" );
      measurementErrorsCounter++; // Count the amount of errors, after too many errors like this we'll consider an issue with the client module!
      return false;
    }

//LogDebug( "Mais putain?!" );

    // Read back time measurement
    rawTime_microSeconds = atoi( reply + 10 );
    retriesCount++;
  }

  if ( _forceOutOfRange ) {
    rawTime_microSeconds = ~0U; // Simulate an out of range measurement
  }

LogDebug( str( "rawTime_microSeconds = %d (%04X) (retries count %d)", rawTime_microSeconds, rawTime_microSeconds, retriesCount ) );

  // Register a new measurement
  U8  bufferIndex = U8( measurementsCounter ) & (MAX_MEASUREMENTS-1);

  Time_ms now;
  GetTime( now );

  float deltaTime_seconds = now.GetTime_seconds() - startTime.GetTime_seconds();  // Total time since the device is up

  bufferMeasurements[bufferIndex].timeStamp_seconds = timeReference_seconds + U32( deltaTime_seconds );
  bufferMeasurements[bufferIndex].rawTime_microSeconds = rawTime_microSeconds;

  measurementsCounter++;
  measurementErrorsCounter = 0; // Clear errors counter!

  return true;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// CLIENT SIDE
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
#else

void loop() {
  runCounter++; // 1 more cycle...
  #ifdef DEBUG
    digitalWrite( PIN_LED_GREEN, runCounter & 1 );
  #endif

  // Check for a command
  U16   senderAddress;
  U8    payloadLength;
  char* payload;
  U8    result;
  //*
  if ( (result = ReceivePeek( senderAddress, payloadLength, payload )) != RR_OK ) {
//  if ( (result = ReceiveWait( senderAddress, payloadLength, payload )) != RR_OK ) {
    // Nothing received...
    delay( CLIENT_POLL_INTERVAL_MS );
    return;
  }
//*/
// Test continuous measurements...
//payload = "CMD=DST0,1234,";
//payloadLength = strlen( payload );

  #ifdef DEBUG_LIGHT
    Serial.println( str( "Client 1 => Received %s", payload ) );
  #endif

  if ( senderAddress != RECEIVER_ADDRESS ) {
    return; // Not from the server...
  }

  if ( payloadLength < 4 ) {
    // Invalid payload!
    Flash( 50, 10 );  // Flash to signal error! => We received something that is badly formatted...
    return;
  }

  // Analyze command
  if ( strstr( payload, "CMD=" ) != payload ) {
    // Not a command?
    Flash( 50, 10 );  // Flash to signal error! => We received something that is badly formatted...
    return;
  }

  // Skip "CMD="
  payload += 4;
  payloadLength -= 4;

  if ( payloadLength < 4 ) {
    // Invalid payload!
    Flash( 50, 10 );  // Flash to signal error! => We received something that is badly formatted...
    return;
  }

  // Check for common commands
  if ( QuickCheckCommand( payload, "TIME" ) ) {
    ExecuteCommand_Runtime( payloadLength, payload );
    return;
  } else if ( QuickCheckCommand( payload, "PING" ) ) {
    ExecuteCommand_Ping( payloadLength, payload );
    return;
  }

  // Let handler check for supported commands
  if ( !HandleCommand( payloadLength, payload ) ) {
    // Unrecognized command?
    Flash( 150, 10 );  // Flash to signal error!
    return;
  }
}

void  ExecuteCommand_Runtime( U8 _payloadLength, char* _payload ) {
  Reply( _payload, _payload+5, str( "%d", runCounter ) );  // Send back the runtime counter
}

void  ExecuteCommand_Ping(  U8 _payloadLength, char* _payload ) {
  Reply( _payload, _payload+5, "" );  // Just send the ping back...
}

#endif
