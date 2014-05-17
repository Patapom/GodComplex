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
 * xml_camera_profile.cpp
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
 * 28 Dec 2010 : V1.2;
 * 02 Dec 2012 : V1.4; updated to DNG V1.4
 *
 */

#include "xml_camera_profile.h"
#include <stdio.h>
#include <libxml/parser.h>
#include <libxml/tree.h>
#include "dng_string.h"
#include "dng_bottlenecks.h"

// Fix the stupid no-one agrees on snprintf issue....
#if defined(WIN32) || defined(_WIN32) || qWinOS
#ifndef snprintf
#define snprintf _snprintf
#endif
#endif

enum
{
	tcCharBufferSize				= 512,
};	

// The lab value of the Gretag CC2 patch is 65.7110, 18.1300, 17.8100, which is
// HSV 26.1355, 0.4450, 0.4257.....so, to get the same skin tone
#define kTwistValueSelect 0.4257


xml_camera_profile::xml_camera_profile(void)
{
	
}

xml_camera_profile::~xml_camera_profile(void)
{
}

void serialize_dng_string(xmlNodePtr parent_node, const dng_string *string, const char *name)
{	
	if ((string != NULL) && (string->NotEmpty())) {		
		/* 
		 * xmlNewChild() creates a new node, which is "attached" as child node
		 * of parent_node node. 
		 */
		xmlNewChild(parent_node, NULL, BAD_CAST name,
					BAD_CAST string->Get());
		}	
}

bool parse_dng_string(xmlDocPtr doc, xmlNodePtr parent_node, dng_string *string, const char *name)
{	
	if (xmlStrEqual(parent_node->name, BAD_CAST name)) {
		xmlChar *key = xmlNodeListGetString(doc, parent_node->xmlChildrenNode, 1);
		string->Set_UTF8((const char*) key);
		xmlFree(key);
		return true;
	}
	else {
		return false; 
	}	
}

bool parse_dng_srational(xmlDocPtr doc, xmlNodePtr parent_node, dng_srational *number, const char *name)
{
	if (xmlStrEqual(parent_node->name, BAD_CAST name)) {
		xmlChar *key = xmlNodeListGetString(doc, parent_node->xmlChildrenNode, 1);
		real32 n;
		if (sscanf ((const char*) key,"%f",&n) != 1) {
			printf("Warning: format error while reading %s\n", name);
		}
		else {
			number->Set_real64(n, 0);
		}
		xmlFree(key);
		return true;
	}
	else {
		return false;
	}
}

void serialize_uint32(xmlNodePtr parent_node, uint32 number, const char *name)
{	
	char str[tcCharBufferSize];
	snprintf(str, tcCharBufferSize, "%d", number);
	// Numbers are lower-128 ASCII, so no need to jump through hoops on UTF8
	xmlNewChild(parent_node, NULL, BAD_CAST name,
				BAD_CAST str);	
}

void serialize_real32(xmlNodePtr parent_node, real32 number, const char *name)
{
	char str[tcCharBufferSize];
	snprintf(str, tcCharBufferSize, "%f", number);
	// Numbers are lower-128 ASCII, so no need to jump through hoops on UTF8
	xmlNewChild(parent_node, NULL, BAD_CAST name,
				BAD_CAST str);
}


bool parse_uint32(xmlDocPtr doc, xmlNodePtr parent_node, uint32 &number, const char *name)
{	
	if (xmlStrEqual(parent_node->name, BAD_CAST name)) {
		xmlChar *key = xmlNodeListGetString(doc, parent_node->xmlChildrenNode, 1);
		uint32 n;
		if (sscanf ((const char*) key,"%d",&n) != 1) {
			printf("Warning: format error while reading %s\n", name);
		}
		else {
			number = n;
		}
		xmlFree(key);
		return true;
	}
	else {
		return false; 
	}	
}

bool parse_real32(xmlDocPtr doc, xmlNodePtr parent_node, real32 &number, const char *name)
{	
	if (xmlStrEqual(parent_node->name, BAD_CAST name)) {
		xmlChar *key = xmlNodeListGetString(doc, parent_node->xmlChildrenNode, 1);
		real32 n;
		if (sscanf ((const char*) key,"%f",&n) != 1) {
			printf("Warning: format error while reading %s\n", name);
		}
		else {
			number = n;
		}
		xmlFree(key);
		return true;
	}
	else {
		return false; 
	}	
}

void attribute_uint32(xmlNodePtr parent_node, uint32 number, const char *name)
{	
	char str[tcCharBufferSize];
	snprintf(str, tcCharBufferSize, "%d", number);
	// Numbers are lower-128 ASCII, so no need to jump through hoops on UTF8
	xmlNewProp(parent_node, BAD_CAST name, BAD_CAST str);
}

void attribute_real32(xmlNodePtr parent_node, real32 number, const char *name)
{	
	char str[tcCharBufferSize];
	snprintf(str, tcCharBufferSize, "%f", number);
	// Numbers are lower-128 ASCII, so no need to jump through hoops on UTF8
	xmlNewProp(parent_node, BAD_CAST name, BAD_CAST str);
}

void serialize_dng_matrix(xmlNodePtr parent_node, const dng_matrix *matrix, const char *name)
{	
	if (matrix != NULL) {
		xmlNodePtr this_node = xmlNewChild(parent_node, NULL, BAD_CAST name, NULL);
		attribute_uint32(this_node, matrix-> Rows(), "Rows");
		attribute_uint32(this_node, matrix-> Cols(), "Cols");
		
		if (matrix->NotEmpty()) {
			int row, column;
			for (row = matrix-> Rows()-1; row >= 0; row--) {
				for (column = matrix-> Cols()-1; column >= 0; column--) {
					char str[tcCharBufferSize];
					snprintf(str, tcCharBufferSize, "%f", (*matrix)[row][column]);
					// Numbers are lower-128 ASCII, so no need to jump through hoops on UTF8
					xmlNodePtr element_node = xmlNewChild(this_node, NULL, BAD_CAST "Element",
														  BAD_CAST str);
					attribute_uint32(element_node, row, "Row");
					attribute_uint32(element_node, column, "Col");
				}
			}
		}	
	}
}

int32 parse_int32_attribute(xmlNodePtr parent_node,  const char *name, bool &parseError)
{	
	uint32 n = 0;
	xmlChar *uri;
	if ((uri = xmlGetProp(parent_node, BAD_CAST name)) == NULL) {
		parseError = true;
	}
	else if (sscanf ((const char*) uri,"%d",&n) != 1) {
		parseError = true;
	}
	xmlFree(uri);
	return n;
}

real32 parse_real32_attribute(xmlNodePtr parent_node,  const char *name, bool &parseError)
{	
	real32 n = 0;
	xmlChar *uri;
	if ((uri = xmlGetProp(parent_node, BAD_CAST name)) == NULL) {
		parseError = true;
	}
	else if (sscanf ((const char*) uri,"%f",&n) != 1) {
		parseError = true;
	}
	xmlFree(uri);
	return n;
}

bool parse_dng_matrix(xmlDocPtr doc, xmlNodePtr parent_node, dng_matrix &matrix, const char *name)
{	
	if (xmlStrEqual(parent_node->name, BAD_CAST name)) {
		bool parseError = false;
		int itemCount = 0;
		uint32 rows = parse_int32_attribute(parent_node,  "Rows", parseError);
		uint32 cols = parse_int32_attribute(parent_node,  "Cols", parseError);
				
		if (rows > 0 && cols > 0) {
			dng_matrix temp (rows, cols);
			xmlNodePtr cur = parent_node->xmlChildrenNode;
			while (cur != NULL) {
				real32 value;
				if (parse_real32(doc, cur, value, "Element")) {		
					temp[parse_int32_attribute(cur,  "Row", parseError)][parse_int32_attribute(cur,  "Col", parseError)] = value;
					itemCount++;
				}
				cur = cur->next;
			}		
			if (parseError) {
				printf("Warning: format error while reading %s\n", name);
			}
			else {
				if (itemCount != (temp.Rows()*temp.Cols())) {
					printf("Warning: number of data items does not match in %s\n", name);
				}
				matrix = temp;		
			}
		}
		else {
			printf("Information: %s has zero dimensions\n", name);
		}
		return true;
	}
	else {
		return false; 
	}	
}


	
void serialize_dng_vector(xmlNodePtr parent_node, const std::vector<dng_point_real64> *dvector, const char *name)
{	
	
	if ((dvector != NULL) && (dvector->size() > 0)) {
		xmlNodePtr this_node = xmlNewChild(parent_node, NULL, BAD_CAST name, NULL);
		attribute_uint32(this_node, (uint32) dvector-> size(), "Size");
	
		for (uint32 j = 0; j < dvector-> size (); j++) {
			xmlNodePtr element_node = xmlNewChild(this_node, NULL, BAD_CAST "Element", NULL);				
			attribute_uint32(element_node, j, "N");
			attribute_real32(element_node, (real32) (*dvector)[j].h, "h");
			attribute_real32(element_node, (real32) (*dvector)[j].v, "v");
		}
	}
}

bool parse_dng_vector(xmlDocPtr doc, xmlNodePtr parent_node, std::vector<dng_point_real64> &dvector, const char *name)
{	
	if (xmlStrEqual(parent_node->name, BAD_CAST name)) {
		bool parseError = false;
		int itemCount = 0;
		
		uint32 Size = parse_int32_attribute(parent_node,  "Size", parseError);
		
		if (Size > 0) {			
			dvector.resize (Size);
			xmlNodePtr cur = parent_node->xmlChildrenNode;
			while (cur != NULL) {
				if (xmlStrEqual(cur->name, BAD_CAST "Element")) {
					uint32 N = parse_int32_attribute(cur,  "N", parseError);
					dvector[N].h = parse_real32_attribute(cur,  "h", parseError);
					dvector[N].v = parse_real32_attribute(cur,  "v", parseError);
					itemCount++;
				}
				cur = cur->next;
			}		
			if (parseError) {
				printf("Warning: format error while reading %s\n", name);
			}
			else {
				if (itemCount != Size) {
					printf("Warning: number of data items does not match in %s\n", name);
				}
			}	
		}
		else {
			printf("Information: %s has zero size\n", name);
		}
		return true;
	}
	else {
		return false; 
	}	
}

void serialize_dng_hue_sat_map(xmlNodePtr parent_node, const dng_hue_sat_map *map, const char *name)
{	
	if ((map != NULL) && (map->IsValid())) {
		xmlNodePtr this_node = xmlNewChild(parent_node, NULL, BAD_CAST name, NULL);
		uint32 hueDivisions;
		uint32 satDivisions;
		uint32 valDivisions;
		map->GetDivisions (hueDivisions,
						   satDivisions,
						   valDivisions);
		attribute_uint32(this_node, hueDivisions, "hueDivisions");
		attribute_uint32(this_node, satDivisions, "satDivisions");
		attribute_uint32(this_node, valDivisions, "valDivisions");
		
		for (uint32 hue = 0; hue < hueDivisions; hue++) {
			for (uint32 sat = 0; sat < satDivisions; sat++) {
				for (uint32 vald = 0; vald < valDivisions; vald++) {
					dng_hue_sat_map::HSBModify item;
					map->GetDelta(hue, sat, vald, item);
					xmlNodePtr element_node = xmlNewChild(this_node, NULL, BAD_CAST "Element", NULL);
					attribute_uint32(element_node, hue, "HueDiv");
					attribute_uint32(element_node, sat, "SatDiv");
					attribute_uint32(element_node, vald, "ValDiv");	
					attribute_real32(element_node, item.fHueShift, "HueShift");	
					attribute_real32(element_node, item.fSatScale, "SatScale");	
					attribute_real32(element_node, item.fValScale, "ValScale");	
				}
			}
		}
	}
}

bool parse_dng_hue_sat_map(xmlDocPtr doc, xmlNodePtr parent_node, dng_hue_sat_map &map, const char *name)
{	
	if (xmlStrEqual(parent_node->name, BAD_CAST name)) {
		bool parseError = false;
		int itemCount = 0;

		uint32 hueDivisions = parse_int32_attribute(parent_node,  "hueDivisions", parseError);
		uint32 satDivisions = parse_int32_attribute(parent_node,  "satDivisions", parseError);
		uint32 valDivisions = parse_int32_attribute(parent_node,  "valDivisions", parseError);
		
		if (hueDivisions > 0 && satDivisions > 1) {
		
			map.SetDivisions(hueDivisions, 
							satDivisions,
							valDivisions
							);
			
			xmlNodePtr cur = parent_node->xmlChildrenNode;
			while (cur != NULL) {
				if (xmlStrEqual(cur->name, BAD_CAST "Element")) {		
					dng_hue_sat_map::HSBModify item;
					item.fHueShift = parse_real32_attribute(cur,  "HueShift", parseError);
					item.fSatScale = parse_real32_attribute(cur,  "SatScale", parseError);
					item.fValScale = parse_real32_attribute(cur,  "ValScale", parseError);
					map.SetDelta(parse_int32_attribute(cur,  "HueDiv", parseError), 
								 parse_int32_attribute(cur,  "SatDiv", parseError), 
								 parse_int32_attribute(cur,  "ValDiv", parseError), 
								 item);
					itemCount++;
				}
				cur = cur->next;
			}		
			if (parseError) {
				printf("Warning: format error while reading %s\n", name);
			}
			else {
				if (itemCount != map.DeltasCount()) {
					printf("Warning: number of data items does not match in %s\n", name);
				}	
			}	
		}
		else {
			printf("Information: %s is of zero size\n", name);
		}
		return true;
	}
	else {
		return false; 
	}	
}

void xml_camera_profile::serializeToXML(const char *filename)
{	
	LIBXML_TEST_VERSION;
	
	/* 
	 * Creates a new document, a node and set it as a root node
	 */
	xmlDocPtr doc = xmlNewDoc(BAD_CAST "1.0");
	xmlNodePtr root_node = xmlNewNode(NULL, BAD_CAST "dcpData");
	xmlDocSetRootElement(doc, root_node);
	
	/*
	 * Creates a DTD declaration. Isn't mandatory. 
	 */
//	xmlCreateIntSubset(doc, BAD_CAST "root", NULL, BAD_CAST "AdobeDCP.dtd");

	serialize_dng_string(root_node, &fName, "ProfileName");
	serialize_uint32(root_node, fCalibrationIlluminant1, "CalibrationIlluminant1");
	serialize_uint32(root_node, fCalibrationIlluminant2, "CalibrationIlluminant2");
	serialize_dng_matrix(root_node, &fColorMatrix1, "ColorMatrix1");
	serialize_dng_matrix(root_node, &fColorMatrix2, "ColorMatrix2");
	serialize_dng_matrix(root_node, &fForwardMatrix1, "ForwardMatrix1");
	serialize_dng_matrix(root_node, &fForwardMatrix2, "ForwardMatrix2");	
	serialize_dng_matrix(root_node, &fReductionMatrix1, "ReductionMatrix1");
	serialize_dng_matrix(root_node, &fReductionMatrix2, "ReductionMatrix2");
	serialize_dng_string(root_node, &fCopyright, "Copyright");
	serialize_uint32(root_node, fEmbedPolicy, "EmbedPolicy");
	serialize_dng_hue_sat_map(root_node, &fHueSatDeltas1, "HueSatDeltas1");	
	serialize_dng_hue_sat_map(root_node, &fHueSatDeltas2, "HueSatDeltas2");	
	serialize_dng_hue_sat_map(root_node, &fLookTable, "LookTable");		
	serialize_dng_vector(root_node, &fToneCurve.fCoord, "ToneCurve");	
	serialize_dng_string(root_node, &fProfileCalibrationSignature, "ProfileCalibrationSignature");
	serialize_dng_string(root_node, &fUniqueCameraModelRestriction, "UniqueCameraModelRestriction");
	serialize_uint32(root_node, fLookTableEncoding, "ProfileLookTableEncoding");
	serialize_real32(root_node, (real32) fBaselineExposureOffset.As_real64(), "BaselineExposureOffset");
	serialize_uint32(root_node, fDefaultBlackRender, "DefaultBlackRender");
	
	/* 
	 * Dumping document to stdio or file
	 */
	xmlSaveFormatFileEnc(filename, doc, "UTF-8", 1);
	
	/*free the document */
	xmlFreeDoc(doc);
	
	/*
	 *Free the global variables that may
	 *have been allocated by the parser.
	 */
	xmlCleanupParser();
	
	/*
	 * this is to debug memory for regression tests
	 */
	xmlMemoryDump();	
}



void xml_camera_profile::loadFromXML(const char *filename)
{
	
    xmlDoc *doc = NULL;
    xmlNode *root_element = NULL;
	
    /*
     * this initialize the library and check potential ABI mismatches
     * between the version it was compiled for and the actual shared
     * library used.
     */
    LIBXML_TEST_VERSION
	
    /*parse the file and get the DOM */
    doc = xmlReadFile(filename, NULL, 0);
	
    if (doc == NULL) {
        printf("error: not a valid XML file %s\n", filename);
    }
	
    /*Get the root element node */
    root_element = xmlDocGetRootElement(doc);
	
    for (xmlNode *cur_node =  root_element; cur_node; cur_node = cur_node->next) {
        if (cur_node->type == XML_ELEMENT_NODE) {
			if (xmlStrEqual(cur_node->name, BAD_CAST "dcpData")) {
				for (xmlNode *dcp_node =  cur_node->children; dcp_node; dcp_node = dcp_node->next) {
					if (dcp_node->type == XML_ELEMENT_NODE) {
						if (parse_dng_string(doc, dcp_node, &fName, "ProfileName")) {}
						else if (parse_uint32(doc, dcp_node, fCalibrationIlluminant1, "CalibrationIlluminant1")) {}
						else if (parse_uint32(doc, dcp_node, fCalibrationIlluminant2, "CalibrationIlluminant2")) {}
						else if (parse_dng_matrix(doc, dcp_node, fColorMatrix1, "ColorMatrix1")) {}
						else if (parse_dng_matrix(doc, dcp_node, fColorMatrix2, "ColorMatrix2")) {}
						else if (parse_dng_matrix(doc, dcp_node, fForwardMatrix1, "ForwardMatrix1")) {}
						else if (parse_dng_matrix(doc, dcp_node, fForwardMatrix2, "ForwardMatrix2")) {}	
						else if (parse_dng_matrix(doc, dcp_node, fReductionMatrix1, "ReductionMatrix1")) {}
						else if (parse_dng_matrix(doc, dcp_node, fReductionMatrix2, "ReductionMatrix2")) {}
						else if (parse_dng_string(doc, dcp_node, &fCopyright, "Copyright")) {}
						else if (parse_uint32(doc, dcp_node, fEmbedPolicy, "EmbedPolicy")) {}
						else if (parse_dng_hue_sat_map(doc, dcp_node, fHueSatDeltas1, "HueSatDeltas1")) {}
						else if (parse_dng_hue_sat_map(doc, dcp_node, fHueSatDeltas2, "HueSatDeltas2")) {}	
						else if (parse_dng_hue_sat_map(doc, dcp_node, fLookTable, "LookTable")) {}		
						else if (parse_dng_vector(doc, dcp_node, fToneCurve.fCoord, "ToneCurve")) {}	
						else if (parse_dng_string(doc, dcp_node, &fProfileCalibrationSignature, "ProfileCalibrationSignature")) {}
						else if (parse_dng_string(doc, dcp_node, &fUniqueCameraModelRestriction, "UniqueCameraModelRestriction")) {}
                        else if (parse_uint32(doc, dcp_node, fLookTableEncoding, "ProfileLookTableEncoding")) {}
                        else if (parse_dng_srational(doc, dcp_node, &fBaselineExposureOffset, "BaselineExposureOffset")) {}
                        else if (parse_uint32(doc, dcp_node, fDefaultBlackRender, "DefaultBlackRender")) {}
						;
					}
				}
			}
        }
    }
	// Get an MD5 hash
	CalculateFingerprint ();
	
    /*free the document */
    xmlFreeDoc(doc);
	
    /*
     *Free the global variables that may
     *have been allocated by the parser.
     */
    xmlCleanupParser();		
	
}


void getInterpolatedHSVDeltas(uint32 hDiv,
							  uint32 sDiv,
							  uint32 vDiv,
							  const dng_hue_sat_map &srcLut, 
							  real32 &deltaH,
							  real32 &deltaS,
							  real32 &deltaV,
							  const dng_hue_sat_map &dstLut)
{	
	real32 h, s, v;
	uint32 hueDivisions;
	uint32 satDivisions;
	uint32 valDivisions;
	
	srcLut.GetDivisions (hueDivisions,
						 satDivisions,
						 valDivisions);
	
	h = (real32) hDiv / (real32) hueDivisions * 360.0f;
	s = (real32) sDiv / (real32) (satDivisions - 1);
	v = (real32) vDiv / (real32) (valDivisions - 1);

	dstLut.GetDivisions (hueDivisions,
						 satDivisions,
						 valDivisions);
	
	hDiv = (int32) (((real32) hueDivisions) * h);
	sDiv = (int32) (((real32) satDivisions - 1) * s);
	vDiv = (int32) (((real32) valDivisions - 1) * v);
	
	real32 hDistance = (hueDivisions * h) - hDiv;
	real32 sDistance = ((satDivisions - 1) * s) - sDiv;
	real32 vDistance = ((valDivisions - 1) * v) - vDiv;
	real32 hueSigma = 0.0f, satSigma = 0.0f, valSigma = 0.0f;
	for (uint32 i = 0; i < 2; i++) {
		// Wrap......
		uint32 hIndex = ((hDiv+i) < hueDivisions) ? (hDiv+i) : 0;
		for (uint32 j = 0; j < 2; j++) {
			uint32 sIndex = sDiv + ((sDistance < 0.0000001f) ? 0 : j);
			for (uint32 k = 0; k < 2; k++) {
				uint32 vIndex = vDiv + ((vDistance < 0.0000001f) ? 0 : k);
				dng_hue_sat_map::HSBModify item;
				dstLut.GetDelta(hIndex, sIndex, vIndex, item);
	
				hueSigma += item.fHueShift * (i == 0 ? (1.0f - hDistance) : hDistance);
				satSigma += item.fSatScale * (i == 0 ? (1.0f - sDistance) : sDistance);
				valSigma += item.fValScale * (i == 0 ? (1.0f - vDistance) : vDistance);
			}
		}
	}
	
	deltaH += hueSigma/4.0f;
	if (deltaH > 180.0f) deltaH -= 360.0f;
	if (deltaH < -180.0f) deltaH += 360.0f;
	deltaS *= satSigma/4.0f;
	deltaV *= valSigma/4.0f;
}

void xml_camera_profile::combineTables(dng_hue_sat_map &hueSatTable)
{	
	dng_hue_sat_map tempMap = hueSatTable;
	uint32 hHD, hSD, hVD, lHD, lSD, lVD, mHD, mSD, mVD;
	tempMap.GetDivisions(hHD, hSD, hVD);
	fLookTable.GetDivisions(lHD, lSD, lVD);
	mHD = Max_uint32 (hHD, lHD);
	mSD = Max_uint32 (hSD, lSD);
	mVD = Max_uint32 (hVD, lVD);
	
	hueSatTable.SetDivisions(mHD, mSD, mVD);
	
	for (mHD = 0; mHD < Max_uint32 (hHD, lHD); mHD++) 
	{
		for (mSD = 0; mSD < Max_uint32 (hSD, lSD); mSD++) 
		{
			for (mVD = 0; mVD < Max_uint32 (hVD, lVD); mVD++)
			{
				real32 dh = 0.0f, ds = 1.0f, dv = 1.0f;
				getInterpolatedHSVDeltas(mHD,
										 mSD,
										 mVD,
										 fHueSatDeltas1, 
										 dh,
										 ds,
										 dv,
										 tempMap);
				getInterpolatedHSVDeltas(mHD,
										 mSD,
										 mVD,
										 fHueSatDeltas1, 
										 dh,
										 ds,
										 dv,
										 fLookTable);
				dng_hue_sat_map::HSBModify item;
				item.fHueShift = dh;
				item.fSatScale = ds;
				// The zero saturation entry is required to have a value scale
				// of 1.0f.
				if (mSD == 0) {
					item.fValScale = 1.0f;
				}
				else {
					item.fValScale = dv;
				}
				hueSatTable.SetDelta(mHD, 
									mSD, 
									mVD, 
									item);
			}			
		}		
	}
}

void xml_camera_profile::makeInvariate(void)
{	
	if (fLookTable.IsValid()) {
		if (fHueSatDeltas1.IsValid()) {
			// Here we have both tables, so we need to combine them.........
			
			// First the main table......
			combineTables(fHueSatDeltas1);
			if (fHueSatDeltas2.IsValid()) {
				combineTables(fHueSatDeltas2);
			}
		}
		else {
			// Here there's a LookTable, but no HueSatDelta tables - easy!!!!
			fHueSatDeltas1 = fLookTable;
			fHueSatDeltas2 = fLookTable;
		}
		fLookTable.SetInvalid();
		fName.Append(" dcpTool Invariant");
	}
	else {
		// No LookTable, nothing to do.....
		fprintf (stderr, "*** File is already invariant, nothing to do.\n");		
	}
}


void untwistTables(dng_hue_sat_map &hueSatTable)
{	
	uint32 lHD, lSD, lVD;
	hueSatTable.GetDivisions(lHD, lSD, lVD);
	real32 floorWeight = 0.0;
	
	dng_hue_sat_map tempMap = hueSatTable;
	hueSatTable.SetDivisions(lHD, lSD, 1);
	
	uint32 vDivSelectFloor = Pin_uint32(0, (uint32) (((real32) lVD - 1) * kTwistValueSelect), lVD - 1);
	uint32 vDivSelectCeil = Pin_uint32(0, vDivSelectFloor + 1, lVD - 1);

	
	if (vDivSelectFloor != vDivSelectCeil) {
		real32 floorVal = ((real32) vDivSelectFloor) / ((real32) (lVD-1));
		real32 ceilVal = ((real32) vDivSelectCeil) / ((real32) (lVD-1));
		floorWeight = Pin_real32(0.0f, (ceilVal - (real32) kTwistValueSelect) / (ceilVal - floorVal), 1.0f);
	}
	
	for (uint32 mHD = 0; mHD < lHD; mHD++) 
	{
		for (uint32 mSD = 0; mSD < lSD; mSD++) 
		{
			dng_hue_sat_map::HSBModify floorItem;
			dng_hue_sat_map::HSBModify ceilItem;
			tempMap.GetDelta(mHD, mSD, vDivSelectFloor, floorItem);
			tempMap.GetDelta(mHD, mSD, vDivSelectCeil, ceilItem);			
			
			floorItem.fHueShift = floorItem.fHueShift * floorWeight + ceilItem.fHueShift * (1-floorWeight);
			floorItem.fSatScale = floorItem.fSatScale * floorWeight + ceilItem.fSatScale * (1-floorWeight);
			floorItem.fValScale = floorItem.fValScale * floorWeight + ceilItem.fValScale * (1-floorWeight);
			
			// The zero saturation entry is required to have a value scale
			// of 1.0f.
			if (mSD == 0) {
				floorItem.fValScale = 1.0f;
			}
			hueSatTable.SetDelta(mHD, 
								mSD, 
								0, 
								floorItem);
		}
	}		
}

void xml_camera_profile::unTwist(void)
{
	uint32 lHD, lSD, lVD;
	bool changed = false;
	
	fLookTable.GetDivisions(lHD, lSD, lVD);
	if (fLookTable.IsValid() && lVD > 1) {
		untwistTables(fLookTable);
		changed = true;
	}
	
	fHueSatDeltas1.GetDivisions(lHD, lSD, lVD);
	if (fHueSatDeltas1.IsValid() && lVD > 1) {
		untwistTables(fHueSatDeltas1);
		changed = true;
	}
	
	fHueSatDeltas2.GetDivisions(lHD, lSD, lVD);
	if (fHueSatDeltas2.IsValid() && lVD > 1) {
		untwistTables(fHueSatDeltas2);
		changed = true;
	}
	
	if (changed) {
		fName.Append(" dcpTool Untwist");
	}
	else {
		// No LookTable/no value dependencies, nothing to do.....
		fprintf (stderr, "*** File does not have hue twists, nothing to do.\n");		
	}
}


