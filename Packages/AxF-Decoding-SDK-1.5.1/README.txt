AxF Decoding SDK
================

* A detailed documentation can be found here:
    - SDK overview and documentation:                doc/AxF-SDK-Documentation.html
    - SDK acknowledgements:                          doc/AxF-SDK-Acknowledgements.html
    - Detailed white paper on the AxF format:        doc/AxF.pdf
    - Frequently asked questions on the AxF format:  doc/AxF-FAQ.pdf
    - SDK changelog:                                 doc/VERSION.txt

* Available builds of the AxF Decoding SDK library:
    - Windows (x64):
        - Location of binary:    Windows.x64/bin/AxFDecoding.dll  (import library: Windows.x64/lib/AxFDecoding.lib)
        - External dependencies: none except for Windows system libraries  (kernel32.dll)
        - Supported systems:     all non-EOL Windows versions  (see http://windows.microsoft.com/en-us/windows/lifecycle)
        - Supported compilers:   MS Visual C++ [>= 2010]  (no particular MSVC version required, since the built DLL has no external dependency to the MSVC runtime library)
    - Linux (x64):
        - Location of binary:    Linux.x64/lib/AxFDecoding.so
        - External dependencies: none except for GLIBC libraries
        - Supported systems:     all Linux distributions based on GLIBC >= 2.4
        - Supported compilers:   GCC
    - Mac OS X (x64):
        - Location of binary:    MacOSX.x64/lib/AxFDecoding.dylib
        - External dependencies: none except for OS X SDK libraries
        - Supported systems:     Mac OS X >= 10.9
        - Supported compilers:   Clang (Apple LLVM), for instance via Xcode

* Instructions how to build sample "AxFDecode":
    - To build the sample for any of the above systems, we recommend to use the supplied CMake build.
      For this, a recent CMake version must be installed (http://www.cmake.org).
    - Use CMake (cmake-gui) with the 'sample' directory as source directory (and an arbitrary empty directory as build directory).
    - The following third party libraries are required:
        * Boost (tested version: 1.58.0, http://www.boost.org/)
        * FreeImage (tested versions: 3.15.4 and 3.17.0, http://freeimage.sourceforge.net/)
      In case of Linux, the libraries should be found automatically by CMake when installed via package manager.
      In case of Windows, you might need to set the custom CMake variables CUSTOM_INCLUDE_SEARCH_PATH and CUSTOM_LIBRARY_SEARCH_PATH (see cmake-gui)
      in order to make sure both libraries can be found by CMake. (For Boost to be found, alternatively, setting the two environment variables BOOST_ROOT
      and BOOST_LIBRARYDIR appropriately should work as well.)
    - Use the switches BOOST_DYN_LINK and FREEIMAGE_DYN_LINK in cmake-gui to indicate whether to look for the dynamic or static versions of these libraries.
    - If no FreeImage build is at hand, you may alternatively disable the FreeImage dependency of this sample by turning off the switch FREEIMAGE_DEPENDENCY
      in cmake-gui. In that case however, the sample will not write any image files to disk.
