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
 * xml_camera_profile.h
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
 * 02 Dec 2012 : V1.4; updated to DNG V1.4
 *
 */

#include "dng_camera_profile.h"

class xml_camera_profile :
	public dng_camera_profile
	{
	public:
		xml_camera_profile(void);
		~xml_camera_profile(void);
		void serializeToXML(const char *filename);		
		void loadFromXML(const char *filename);		
		void makeInvariate(void);
		void unTwist(void);
		
	private:
		void combineTables(dng_hue_sat_map &hueSatTable);	
	};