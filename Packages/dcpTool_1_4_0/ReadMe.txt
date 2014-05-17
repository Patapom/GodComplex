This contains the binaries, documentation and source code for dcpTool

For more information on dcpTool, see the documentation directory (open the index.html 
file) or the website: http://dcpTool.sourceforge.com.

======================================================================================
Licensing

(C) Copyright 2009-2012, by Sandy McGuffog and Contributors.
The DNG SDK and XMP SDK are (C) Copyright 2005-2012 Adobe Inc.

The supplied libxml2, iconv and zlib dlls for the Windows version of dcpTool
are sourced from Igor Zlatković's web site: http://www.zlatkovic.com/projects/libxml/

All Rights reserved

This program is free software; you can redistribute it and/or modify it under the terms
of the GNU General Public License as published by the Free Software Foundation;
either version 2 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with this
library; if not, write to the Free Software Foundation, Inc., 59 Temple Place, Suite 330,
Boston, MA 02111-1307, USA.


-----------------------------------------------------------------------------------------
The binary executables:

The binary executables are the only things that most people will ever need to know about:

The binaries can be found in the "Binaries" subdirectory, in either the "Windows" or 
"OS X" directories.

The OS X version of dcpTool is entirely self contained, but in order to run the Windows 
version, you must have the supplied dlls (iconv.dll, libxl2.dll and zlib1.dll) in the same 
directory as the dcpTool.exe file

Usage is documented in the documentation directory (open the index.html 
file) or the website: http://dcpTool.sourceforge.com.


------------------------------------------------------------------------------------------

Notes for compliling the source:

1. This was built under VC Express 2008 for Windows, and XCode 3 for the Mac. The respective 
project files are included

2. In order to compile dcpTool, you need to download the Adobe DNG SDK 1.3, and compile it

3. In order to recompile the DNG SDK, you will need to:

a) Download the Adobe XMP SDK, as well as the approporiate version of Expat, zlib, MD5 and QTDevWin. 
How to include these into the XMP SDK is documented by Adobe on www.adobe.com, and in the XMP SDK

b) You will need to modify the DNG SDK if you want the ability to extract profiles from DNG files.
The diff files to do this is provided in the dng_sdk/source directory

More information on compiling is in the documentation directory (open the index.html 
file) or the website: http://dcpTool.sourceforge.com.


Enjoy..................