/* =======================================================
 * dcpTool - a decompiler/compiler for DCP camera profiles
 * =======================================================
 *
 * Project Info:  http://sourceforge.net/projects/dcpTool/
 * Project Lead:  Sandy McGuffog (sandy.cornerfix@gmail.com);
 *
 * (C) Copyright 2009, by Sandy McGuffog and Contributors.
 *
 * This program is free software; you can redistribute it and/or modify it under the terms
 * of the GNU General Public License as published by the Free Software Foundation;
 * either version 2 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along with this
 * library; if not, write to the Free Software Foundation, Inc., 59 Temple Place, Suite 330,
 * Boston, MA 02111-1307, USA.
 *
 * ---------------
 * main.cpp
 * ---------------
 * (C) Copyright 2009-2012, by Sandy McGuffog and Contributors.
 *
 * Original Author:  Sandy McGuffog;
 * Contributor(s):   -;
 *
 *
 * Changes
 * -------
 * 04 Feb 2009 : Original version;
 * 12 Feb 2009 : V1.1;
 * 12 Aug 2009 : V1.11; Upgrade to DNG V1.3 libraries
 * 28 Dec 2010 : V1.2; values extrapolated for untwist
 * 02 Dec 2012 : V1.4; updated to DNG V1.4
 *
 */

#include "dng_file_stream.h"
#include "dng_image_writer.h"
#include "xml_camera_profile.h"


#define kdcpToolVersion "1.40"

enum
{
	gNone				= 0,
	gCompile,
	gInvariate,
	gUnTwist,	
};	


static int gOperation = gNone;

void printUsage(char *argv [])
{
	fprintf (stderr,
			 "\n"
			 "dcpTool, version " kdcpToolVersion " "
#if qDNG64Bit
			 "(64-bit)"
#else
			 "(32-bit)"
#endif
			 "\n"
			 "Copyright (c) 2009-2012 Sandy McGuffog\n"
			 "Portions Copyright 2005-2012 Adobe Systems, Inc.\n"
			 "\n"
			 "Usage:  %s [options] file1 [file2]\n"
			 "\n"
			 "Valid options:\n"
			 "-h            Print this message\n"
			 "-c            Compile file1(xml) to file2(dcp)\n"
			 "-d            Decompile file1(dcp) to file2(xml) (default)\n"
			 "-i            Convert file1(dcp) to invariate file2(dcp)\n"
			 "-u            Convert file1(dcp) to untwisted file2(dcp)\n"
			 "\n",
			 argv [0]);		
}

int main (int argc, char *argv [])
{
	
	try
	{
		if (argc == 1)
		{
			printUsage(argv);
		}
		
		int index;
		
		for (index = 1; index < argc && argv [index] [0] == '-'; index++)
		{
			
			dng_string option;
			
			option.Set (&argv [index] [1]);
			
			if (option.Matches ("h", true))
			{
				printUsage(argv);
			}
			else if (option.Matches ("c", true))
			{
				gOperation = gCompile;
			}
			else if (option.Matches ("d", true))
			{
				gOperation = gNone;
			}			
			else if (option.Matches ("i", true))
			{
				gOperation = gInvariate;
			}	
			else if (option.Matches ("u", true))
			{
				gOperation = gUnTwist;
			}	
			else
			{
				fprintf (stderr, "*** Unknown option \"-%s\"\n", option.Get ());
				return 1;
			}
			
		}
		
		int numFiles = argc - index;
		
		if (numFiles == 0)
		{
			fprintf (stderr, "*** No file specified\n");
			return 1;
		}

		if ((numFiles != 2) && (gOperation == gCompile) )
		{
			fprintf (stderr, "*** Two files must be specified to compile\n");
			return 1;
		}

		if ((numFiles != 2) && (gOperation == gInvariate) )
		{
			fprintf (stderr, "*** Two files must be specified to convert to invariate\n");
			return 1;
		}
		
		if ((numFiles != 2) && (gOperation == gUnTwist) )
		{
			fprintf (stderr, "*** Two files must be specified to convert to untwist\n");
			return 1;
		}

		if (gOperation == gCompile) {
			
			xml_camera_profile profile;
			profile.loadFromXML(argv [index++]);
			
			dng_file_stream outStream (argv [index++], true);
			
			tiff_dng_extended_color_profile writer (profile);
			
			writer.Put (outStream);
			
			outStream.Flush ();
		}
		else if (gOperation == gInvariate) {
			dng_file_stream inStream (argv [index++]);
			
			xml_camera_profile profile;
			
			profile.ParseExtended (inStream);
			
			profile.makeInvariate ();
			
			dng_file_stream outStream (argv [index++], true);
			
			tiff_dng_extended_color_profile writer (profile);
			
			writer.Put (outStream);
			
			outStream.Flush ();
		}
		else if (gOperation == gUnTwist) {
			dng_file_stream inStream (argv [index++]);
			
			xml_camera_profile profile;
			
			profile.ParseExtended (inStream);
			
			profile.unTwist ();
			
			dng_file_stream outStream (argv [index++], true);
			
			tiff_dng_extended_color_profile writer (profile);
			
			writer.Put (outStream);
			
			outStream.Flush ();
		}
		else {
			
			dng_file_stream inStream (argv [index++]);
			
			xml_camera_profile profile;
			
			profile.ParseExtended (inStream);
			
			if (numFiles != 2) {
				profile.serializeToXML("-");
			}
			else {
				profile.serializeToXML(argv [index++]);
			}
			
		}
		
		int result = 0;
		
		return result;
		
	}
	
	catch (...)
	{

	}
	
	fprintf (stderr, "*** Exception thrown in main routine - probably could not access a file\n");
	
	return 1;
	
}

