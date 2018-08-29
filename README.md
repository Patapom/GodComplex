﻿
 God Complex: A 64K Experiment
-------------------------------

  Code ► Patapom

-------------------------------
After some time, it became a bit more than a 64K intro framework actually.
It's now much much more. It's become the core framework for all my experiments...

NOTE: *ALL* the parts described below have been tested in real use condition!


What you will find in this framework:
=====================================

  • Various libraries, including:
    ► An image library that supports all formats the FreeImage library is supporting, plus the DDS format
      It also offers a simple and efficient plotting service as well as a HDR=>LDR conversion service
      It supports various color profiles and can be used as a very simple Profile Connection Space (CIE XYZ) to ensure a full control over your import/export pipeline as it attempts to apply the principles explained in my wiki about colorimetry that you can find at http://wiki.nuaj.net/index.php?title=Colorimetry and http://wiki.nuaj.net/index.php?title=Color_Profile
    
    ► A very simple yet efficient DirectX 11 rendering library
      Don't expect a super optimized renderer here: the main goal is to quickly and easily prototype some tools and experiments!
      But don't take me for a fool either: all the necessary heavy-duty structures for creating powerful GPGPU softwares are there.
	  Most of the tools I wrote are heavily using compute shaders by the way.
    
    ► A multi-tier math library that is very simple to use as it mimics the vectors found in HLSL (i.e. float2, float3, float4, float4x4, etc.)
      The second tier of the library contains more involved tools like quaternions, pseudo- & quasi-random-number generators, spherical harmonics support, complex numbers suppport, noise generation algorithms, etc.
      The last tier of the library contains high-level "solver" tools like Levenberg-Marquardt, BFGS, simulated annealing or other linear regression tools.
       It also offers a GPU-accelerated 1D and 2D FFT library, but also the CPU version of the FFT and DFT.
    
  • Various generators, namely:
    ► AO Map generator, that is capable of generating high quality AO maps from height maps and normal maps
    ► Self-Shadowed Bump Map Generator (SSBump), that is capable of generating a RGB SSBump map from a height map (cf. http://n00body.squarespace.com/journal/2010/2/7/self-shadowed-bump-maps.html)
    ► Translucency Map Generator, that is capable of generating a RGB map encoding the translucency of a thin material lit by 3 different light positions (cf. https://www.cg.tuwien.ac.at/research/publications/2007/Habel_2007_RTT/)
    ► A blue-noise generator that implements 3 different algorithms to generate a blue noise texture
    
  • Various tests:
    ► Area Light implementation, as used in the Dishonored 2 title
    ► Filmic Curve + Histogram Auto-Exposure, as used in the Dishonored 2 title
    ► Fresnel Tests, showing the various fresnel equations (Schlick, exact, and more recently the 2-terms approximation for metals described by http://jcgt.org/published/0003/04/03/paper.pdf)
    ► SH Irradiance encoding/decoding experiments, as described in my article about SH available at http://wiki.nuaj.net/index.php?title=SphericalHarmonicsPortal
    ► Many others coming up as they still need to be converted from 32 to 64 bits...


What you will *NOT* find though:
================================
  • Exactly the total amount of information in the Universe that is not represented by this bunch of code and data

More precisely:
  • Everything animation-related, except some basic support for quaternions
  • Everything mesh-related except the most basic mesh generators, as I loathe geometry problems :D
  • Everything sound-related, as I never wrote anything else than graphical stuff
  • Any meta-templated-convoluted shitty code that is abominable to read and debug (i.e. anything like the STL or boost, if you like those and desperately need to explain why, please just go away)
   =► My main concerns are ► useability, ► readability, ► maintanability and most of all, ► simplicity (and God knows it's awfully hard to make something simple!)


This framework is written for the Microsoft Windows platform using Microsoft Visual Studio 64-bits C++ for the low-level part, it is (usually) wrapped in CLR Managed C++ to make the native low-level part easily accessible by the high-level tools and applications that I write in C# (if you're one of those who like to advocate why open-source software and linux is so much better than windows and desperately need to explain why, please just go away also).
You end up with tools that you can write very easily and very quickly thanks to C#, but that are also fast as lightning thanks to optimized native C++ and/or hardware acceleration offered by DirectX.
Best of both worlds, right?


Minimum requirements are:
=========================
  • Windows 7
  • Visual Studio 2012
  • DotNet framework version 4.5, available from https://www.microsoft.com/en-us/download/details.aspx?id=30653
  • DirectX 11 SDK, available from https://developer.microsoft.com/en-us/windows/downloads/windows-8-1-sdk
    Especially, the D3DCompiler_46.dll needs to be deployed to the ./build/Debug and ./build/Release directories!
