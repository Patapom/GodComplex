#include "Global.h"

const U32 command_delay_ms = 100;  // Delay between commands

//#ifdef USE_GLOBAL_BUFFER
//  char* LoRaBuffer = USE_GLOBAL_BUFFER;
//#else
  char  LoRaBuffer[256];    // Command/Response buffer
//#endif

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Send / Receive
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
// _payload, the payload to send via LoRa. The characters can be in [1,255] but MUST NOT CONTAIN '\0'!
SEND_RESULT Send( U16 _targetAddress, char* _message ) {
  return Send( _targetAddress, strlen( _message ), _message );
}
SEND_RESULT Send( U16 _targetAddress, U8 _payloadLength, char* _payload ) {
  // Check payload
  if ( _payload == NULL  ) return SR_INVALID_PAYLOAD;
  if ( _payloadLength == 0 || _payloadLength > 240 ) return SR_INVALID_PAYLOAD_SIZE;

  // Prepare command
  char* command = LoRaBuffer;
  command += sprintf( command, "AT+SEND=%d,%d,", _targetAddress, _payloadLength );
  memcpy( command, _payload, _payloadLength );           // Copy payload
  command += _payloadLength;
  *command++ = '\r';
  *command++ = '\n';
  *command++ = '\0';

//  if ( SendCommandAndWaitVerify( LoRaBuffer, "+OK" ) != RT_OK ) return SR_ERROR; // <= If using this version then remove the "\r\n" that we add above otherwise the LoRa will believe we send 2 commands and will return an error

  // Use the fast version using char*
#ifdef DEBUG
  LogDebug( str( "Sending command %s", LoRaBuffer ) );
#endif

  SendCommand( LoRaBuffer );
  char* reply = WaitReply();
  if ( reply == NULL )
    return SR_TIMEOUT;  // Timeout!

#ifdef DEBUG
  LogDebug( str( "Reply = %s", reply ) );
#endif

  if ( strstr( reply, "+OK" ) == NULL )
    return SR_ERROR;  // Failed!

  return SR_OK; // Success!
}

// Wait for a +RCV reply and returns payload info
// NOTE: This is a *blocking* function!
RECEIVE_RESULT ReceiveWait( U16& _targetAddress, U8& _payloadLength, char*& _payload ) {
  int RSSI, SNR;  // Ignore those values
  return ReceiveWait( _targetAddress, _payloadLength, _payload, RSSI, SNR );
}
RECEIVE_RESULT ReceiveWait( U16& _targetAddress, U8& _payloadLength, char*& _payload, int& _RSSI, int& _SNR ) {
  char* reply = WaitReply();
  if ( reply == NULL )
    return RR_TIMEOUT;  // Timeout

  return ExtractReply( reply, _targetAddress, _payloadLength, _payload, _RSSI, _SNR );
}

// Check for a +RCV reply and returns payload info if it's available, or RR_EMPTY_BUFFER if no reply is currently available
// NOTE: This is a *non-blocking* function!
RECEIVE_RESULT ReceivePeek( U16& _targetAddress, U8& _payloadLength, char*& _payload ) {
  int RSSI, SNR;  // Ignore those values
  return ReceivePeek( _targetAddress, _payloadLength, _payload, RSSI, SNR );
}
RECEIVE_RESULT ReceivePeek( U16& _targetAddress, U8& _payloadLength, char*& _payload, int& _RSSI, int& _SNR ) {
  if ( LoRa.available() == 0 )
    return RR_EMPTY_BUFFER;

  // Read reply
  char* p = LoRaBuffer;
  char  C = '\0';
  while ( C != '\n' ) {
    while ( !LoRa.available() );
    C = LoRa.read();
    *p++ = C;
  }
//  *p++ = '\0';  // Terminate string properly so it can be printed

//Serial.println( _payload );

  return ExtractReply( LoRaBuffer, _targetAddress, _payloadLength, _payload, _RSSI, _SNR );
}

// Extracts the LoRa reply in the form of "+RCV=<Address>,<Length>,<Data>,<RSSI>,<SNR>"
RECEIVE_RESULT  ExtractReply( char* _reply, U16& _targetAddress, U8& _payloadLength, char*& _payload, int& _RSSI, int& _SNR ) {
  if ( strstr( _reply, "+RCV=" ) != _reply )
    return RR_ERROR;  // Unexpected reply!

  // Strip reply for address, payload size and payload
  char* p = _reply + 5; // Skip "+RCV="
  char* addressStart = p;
  while ( *p != '\n' && *p != ',' ) { p++; }
  char* addressEnd = p;
  p++;
  char* payloadLengthStart = p;
  while ( *p != '\n' && *p != ',' ) { p++; }
  char* payloadLengthEnd = p;
  p++;
  _payload = p;

  // Extract address and payload length
  *addressEnd = '\0';       // Replace ',' by '\0' so we can convert to integer
  *payloadLengthEnd = '\0';
  _targetAddress = atoi( addressStart );
  _payloadLength = atoi( payloadLengthStart );

  // Strip reply for RSSI and SNR
  char* payloadEnd = _payload + _payloadLength;
  p = payloadEnd;
  p++;
  char* RSSIStart = p;
  while ( *p != '\n' && *p != ',' ) { p++; }
  char* RSSIEnd = p;
  p++;
  char* SNRStart = p;
  while ( *p != '\n' && *p != ',' ) { p++; }
  char* SNREnd = p;

  // Extract RSSI and SNR
  *payloadEnd = '\0';
  *RSSIEnd = '\0'; // Replace ',' by '\0' so we can convert to integer
  *SNREnd = '\0';
  _RSSI = atoi( RSSIStart );
  _SNR = atoi( SNRStart );

#ifdef DEBUG
//  Serial.print( String( "Address = " ) + String( _targetAddress ) + String( ", payload size = " ) + String( _payloadLength ) );
//  Serial.print( String( ", payload = " ) + _payload );
//  Serial.println( String( ", RSSI = " ) + String( _RSSI ) + String( ", SNR = " ) + String( _SNR ) );
  LogDebug( str( "Address = %d, payload size = %d, payload = %s, RSSI = %d, SNR = %d", _targetAddress, _payloadLength, _payload, _RSSI, _SNR )  );
#endif

  return RR_OK;
}


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Configuration
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
CONFIG_RESULT  ConfigureLoRaModule( U8 _networkID, U16 _address ) {
  return ConfigureLoRaModule( _networkID, _address, 915000000 );
}
CONFIG_RESULT  ConfigureLoRaModule( U8 _networkID, U16 _address, U32 _band ) {
  return ConfigureLoRaModule( _networkID, _address, _band, 9, 7, 1, 12 );
}
CONFIG_RESULT  ConfigureLoRaModule( U8 _networkID, U16 _address, U32 _band, U8 _spreadingFactor, U8 _bandwidth, U8 _codingRate, U8 _programmedPreamble ) {
  // Check parameters
  if ( _networkID < 3 || _networkID > 15 ) return CR_INVALID_PARAMETER;
  if ( _spreadingFactor < 5 || _spreadingFactor > 11 ) return CR_INVALID_PARAMETER;
  if ( _bandwidth < 7 || _bandwidth > 9 ) return CR_INVALID_PARAMETER;

  // Send configuration commands
//  SendCommandAndWaitPrint( "AT+IPR?" );
  if ( SendCommandAndWaitVerify( "AT\r\n", "+OK" ) != RT_OK ) return CR_COMMAND_FAILED_AT;
  delay( command_delay_ms );
  if ( SendCommandAndWaitVerify( str( "AT+NETWORKID=%d\r\n", _networkID ), "+OK" ) != RT_OK ) return CR_COMMAND_FAILED_AT_NETWORKID;
  delay( command_delay_ms );
  if ( SendCommandAndWaitVerify( str( "AT+ADDRESS=%d\r\n", _address ), "+OK" ) != RT_OK ) return CR_COMMAND_FAILED_AT_ADDRESS;
  delay( command_delay_ms );
  if ( SendCommandAndWaitVerify( str( "AT+PARAMETER=%d,%d,%d,%d\r\n", _spreadingFactor, _bandwidth, _codingRate, _programmedPreamble ), "+OK" ) != RT_OK ) return CR_COMMAND_FAILED_AT_PARAMETER;
  delay( command_delay_ms );

  return CR_OK; // Success!
}

// Sets the working mode for the device (default is WM_TRANSCEIVER)
CONFIG_RESULT  SetWorkingMode( WORKING_MODE _workingMode, U16 _RXTime, U16 _sleepTime ) {
  if ( _workingMode == WM_SMART ) {
    if ( SendCommandAndWaitVerify( str( "AT+MODE=2,%d,%d\r\n", _RXTime, _sleepTime ), "+OK" ) != RT_OK ) return CR_COMMAND_FAILED_AT_MODE;
  } else {
    if ( SendCommandAndWaitVerify( str( "AT+MODE=%d\r\n", int(_workingMode) ), "+OK" ) != RT_OK ) return CR_COMMAND_FAILED_AT_MODE;
  }
  return CR_OK;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Password
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
CONFIG_RESULT  SetPassword( U32 _password ) {
  if ( !_password ) return CR_INVALID_PASSWORD;
  if ( SendCommandAndWaitVerify( str( "AT+CPIN=%08X\r\n", _password ), "+OK" ) != RT_OK ) return CR_COMMAND_FAILED_AT_CPIN;
  delay( command_delay_ms );

  return CR_OK; // Success!
}

// Apparently, the only way to reset the password is to send an "AT+RESET" command...
//CONFIG_RESULT ClearPassword() {
//  if ( SendCommandAndWaitVerify( "AT+CPIN=00000000\r\n", "+OK" ) != RT_OK ) return CR_COMMAND_FAILED_AT_CPIN;
//  delay( command_delay_ms );
//  return CR_OK; // Success!
//}


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Commands
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//

// Commands (from LoRa AT Command.pdf):
//  AT+RESET
//  AT+IPR=<rate>             // Set baud rate (default 57600)
//  AT+ADDRESS=<ID 16 bits>   // Specific to the module (default 0)
//  AT+NETWORKID=[3,15]       // Common to all modules (default 18)
//  AT+BAND=915000000         // Set the center frequency of wireless band. Common to all modules (default 915000000)
//  AT+PARAMETER=9,7,1,12   
//                              [1] <Spreading Factor>: The larger the SF is, the better the sensitivity is. But the transmission time will take longer. 5~11 (default 9) *SF7to SF9 at 125kHz, SF7 to SF10 at 250kHz, and SF7 to SF11 at 500kHz
//                              [2] <Bandwidth>: The smaller the bandwidth is, the better the sensitivity is. But the transmission time will take longer. 7: 125 KHz (default), 8: 250 KHz, 9: 500 KHz
//                              [3] <Coding Rate>: The coding rate will be the fastest if setting it as 1.
//                              [4] <Programmed Preamble>: Preamble code. If the preamble code is bigger, it will result in the less opportunity of losing data.
//                                    Generally preamble code can be set above 10 if under the permission of the transmission time.
//                                    When the Payload length is greater than 100 bytes, recommend to set “AT + PARAMETER = 8,7,1,12”
//  AT+CPIN=<Password>        // Domain password (4 bytes hexa)
//  AT+CRFOP=<power [0,22]>   // RF Output power in dBm (default=22)
//  AT+SEND=<address 16 bits>, <payload size [0,240]>, <payload>  // Due to the program used by the module, the payload part will increase more 8 bytes than the actual data length.


char* WaitReply() { return WaitReply( ~0U ); } // No timeout
char* WaitReply( U32 _maxIterationsCount ) {
  char* p = LoRaBuffer;
  char  receivedChar = '\0';
  int   iterationsCount = 0;
  while ( receivedChar != '\n' && iterationsCount < _maxIterationsCount ) {
    while ( LoRa.available() == 0 ); // Wait until a character is available...
    receivedChar = LoRa.read();
    *p++ = receivedChar;  // Append characters to the received message
    iterationsCount++;
  }
  if ( iterationsCount >= _maxIterationsCount )  
    return  NULL;  // Timeout!
 
  *p++ = '\0';  // Terminate string properly so it can be displayed...

//Serial.println( "Received reply!" );
//  Serial.println( LoRaBuffer );  // Print the reply to the Serial monitor

  return LoRaBuffer;
}

// NOTE: _command must end with "\r\n"!
void  SendCommand( char* _command ) {
  LoRa.print( _command );
}
//void  SendCommand( String _command ) {
//  LoRa.print( _command + "\r\n" );
//}

// Sends a command and awaits reply
char* SendCommandAndWait( char* _command ) {
  SendCommand( _command );
  return WaitReply();
}

// Sends a command, waits for the reply and compares to the expected reply
// Return an enum depending on the result
RESPONSE_TYPE  SendCommandAndWaitVerify( char* _command, char* _expectedReply ) {
#ifdef DEBUG
  LogDebug( str( "Sending command %s", _command ) );
#endif

  char* reply = SendCommandAndWait( _command );
  if  ( reply == NULL )
    return RT_TIMEOUT;

#ifdef DEBUG
LogDebug( str( "Received reply %s", reply ) ); // No need to println since the reply contains the \r\n...
#endif

  char* ptrExpectedReply = strstr( reply, _expectedReply );
  return ptrExpectedReply != reply == 0 ? RT_OK : RT_ERROR;
}

// For debugging purpose
void  SendCommandAndWaitPrint( char* _command ) {
  Log( str( "Sending command %s", _command ) );
  char* reply = SendCommandAndWait( _command );
  if ( reply != NULL ) {
    Log( str( "Received reply: %s", reply ) );  // Print the reply to the Serial monitor
  } else {
    LogError( "TIMEOUT!" );
  }
}
