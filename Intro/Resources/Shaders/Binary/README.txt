FXBIN files are compiled shaders that are created and stored while compiling GodComplex in DEBUG mode.

You must then run the "ConcatenateShaders" project in the Tools.sln solution that will concatenated the FXBIN together into HLSL files.

WARNING: The HLSL files are BINARY versions of the actual HLSL files one directory up.
These HLSL files are required to run in RELEASE mode.