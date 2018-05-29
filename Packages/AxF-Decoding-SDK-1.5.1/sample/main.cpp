///////////////////////////////////////////////////////////////////////////////
// File:		main.cpp
// Authors:		Gero Mueller, Alexander Gress
//
// Title:	AxFDecode: simple sample to demonstrate the use of AxF Decoding SDK
//
// Version:	1.5
// Created:	2014/05/07
//
// Copyright:  X-Rite 2014-2018
//  		   www.xrite.com
//
// Notes: Required 3rd party libraries for this sample:
//         * boost (tested version: 1.58.0, http://www.boost.org/)
//         * FreeImage (tested version: 3.17.0, http://freeimage.sourceforge.net/)
//           if AXFSAMPLES_FREEIMAGE_DEPENDENCY is set
//
//-----------------------------------------------------------------------------
//
//
///////////////////////////////////////////////////////////////////////////////

// -- AxF Decoding SDK includes: --
#include <AxF/decoding/CPUDecoder.h>
#include <AxF/decoding/Sampler.h>
#include <AxF/decoding/TextureDecoder.h>


// -- C++ standard header includes: --
#ifdef _MSC_VER
    #define _USE_MATH_DEFINES
#endif
#include <cmath>
#include <cstdint>
#include <cassert>
#include <iomanip>

#ifdef _WIN32
    #define NOMINMAX
    #include <Windows.h>   //(only used by unicodeToConsoleOutput() defined below)
#else
    #include <codecvt>
#endif


// -- Boost includes: --
#if defined(AXFSAMPLES_BOOST_DYN_LINK) && !defined(BOOST_ALL_DYN_LINK)   //(AXFSAMPLES_BOOST_DYN_LINK means that Boost is to be linked dynamically)
    // define BOOST_ALL_DYN_LINK for dynamic linking of Boost
    #define BOOST_ALL_DYN_LINK
#endif

#include <boost/program_options/options_description.hpp>
#include <boost/program_options/variables_map.hpp>
#include <boost/program_options/parsers.hpp>
#include <boost/algorithm/string.hpp>

#include <boost/filesystem.hpp>
#include <boost/random.hpp>
#include <boost/format.hpp>
#include <boost/regex.hpp>
#include <boost/foreach.hpp>


// -- FreeImage includes: --
#ifdef AXFSAMPLES_FREEIMAGE_DEPENDENCY
    #if !defined(AXFSAMPLES_FREEIMAGE_DYN_LINK) && !defined(FREEIMAGE_LIB)   //(AXFSAMPLES_FREEIMAGE_DYN_LINK means that FreeImage is to be linked dynamically)
        // define FREEIMAGE_LIB for static linking of FreeImage
        #define FREEIMAGE_LIB
    #endif

    #include <FreeImage.h>
#endif


// -- Namespace shortcuts, defines, and forward declarations: --

namespace axfdec = axf::decoding;        //- throughout this file, we abbreviate namespace axf::decoding by axfdec -
namespace po = boost::program_options;
namespace fs = boost::filesystem;

#ifndef XRITE_THROW
#	define XRITE_THROW(reason) throw std::runtime_error(reason)
#endif


//whether our internal textures buffers should use top-left or bottom-left origin convention  (no matter what we choose, the resulting image files will not differ since we pay attention to this when saving the images)
#define MY_TEXTURE_ORIGIN  axfdec::ORIGIN_TOPLEFT
//#define MY_TEXTURE_ORIGIN  axfdec::ORIGIN_BOTTOMLEFT


//simple shortcuts for message output used in this source file
#define LOG         std::cout
#define LOG_INFO    std::cout << "              [info]    "
#define LOG_WARNING std::cout << "              [warning] "
#define LOG_ERROR   std::cout << "              [error]   "
#define LOG_ENDL    std::endl


//helper routines forward declarations (defined at the end of the file)
static void get_filenames(const fs::path& rclFileNameOrDir, const std::wstring& sRegExFilter, bool bRecursive, std::vector<fs::path>& rvecFilenames);
static std::string unicodeToConsoleOutput(const std::wstring& sInput);
#ifdef AXFSAMPLES_FREEIMAGE_DEPENDENCY
static bool saveFreeImage(const fs::path& rclFileName, const float* pData, size_t w, size_t h, size_t d, size_t c, axfdec::ETextureOrigin eTextureOrigin);
#endif
static void polar2cartesian( float theta, float phi, float res[3] );
static void polar2cartesian( const float v2Polar[2], float res[3] );
static void cartesian2polar( const float v[3], float& theta, float& phi );
static float rnd_norm();


//*************************************************************************************************
// render_test_image()
//
// demonstrates: CPUDecoder::eval()
// desc: render planar image from AxF using CPUDecoder
//*************************************************************************************************
static void render_test_image(axfdec::CPUDecoder* pclDecoder, float scale, float viewz, const fs::path& fn = "test_rendering.exr" )
{
    std::cout << "Render test image...";

    float cp[] = {0.f,0.f,viewz};
    size_t num_channels = 3;
#ifdef _DEBUG
    size_t ui_width = 256;
    size_t ui_height = 256;
    size_t ui_checkerboard_tile_size = 32;
#else
    size_t ui_width = 1024;
    size_t ui_height = 1024;
    size_t ui_checkerboard_tile_size = 128;
#endif
    float f_aspect = pclDecoder->getHeightMM()>0 ? pclDecoder->getWidthMM() / pclDecoder->getHeightMM() : 1;
    std::vector<float> img_test(ui_width*ui_height*num_channels);
    float* pf_pixel = &img_test[0];
    for(size_t y=0; y<ui_height; y++)
    {
        for(size_t x=0; x<ui_width; x++, pf_pixel+=num_channels)
        {
            float p[] = {scale*((float(x)+0.5f)/float(ui_width)-0.5f), (MY_TEXTURE_ORIGIN == axfdec::ORIGIN_BOTTOMLEFT) ? scale*((float(y)+0.5f)/float(ui_height)-0.5f)
                                                                                                                        : scale*(0.5f-(float(y)+0.5f)/float(ui_height)), 0};
            float wi[] = { cp[0]-p[0], cp[1]-p[1], cp[2]-p[2] };
            float f_length = sqrt(wi[0]*wi[0]+wi[1]*wi[1]+wi[2]*wi[2]);
            wi[0] /= f_length;
            wi[1] /= f_length;
            wi[2] /= f_length;
            float wo[] = {0,0,1};
            float uv[] = {scale*(float(x)+0.5f)/float(ui_width),f_aspect*scale*(float(y)+0.5f)/float(ui_height)};
            float f_alpha = 1;
            pclDecoder->eval(pf_pixel,&f_alpha,wi,wo,uv);
            //over checkerboard
            float f_tile_color = ((x / ui_checkerboard_tile_size) % 2 + (y / ui_checkerboard_tile_size) % 2) % 2 ? 1.f : 0.f;
            pf_pixel[0] = f_alpha * pf_pixel[0] + (1 - f_alpha) * f_tile_color;
            pf_pixel[1] = f_alpha * pf_pixel[1] + (1 - f_alpha) * f_tile_color;
            pf_pixel[2] = f_alpha * pf_pixel[2] + (1 - f_alpha) * f_tile_color;
        }
    }

#ifdef AXFSAMPLES_FREEIMAGE_DEPENDENCY
    ::saveFreeImage(fn,&img_test[0], ui_width, ui_height, 1, num_channels, MY_TEXTURE_ORIGIN);
#endif
    std::cout << std::endl;
}


//*************************************************************************************************
// test_sampling()
//
// demonstrates: Sampler::sample(), Sampler::pdf()
// desc: sample the AxF representation for a given number of directions, plots generated samples into an image
//*************************************************************************************************
static void test_sampling(axfdec::Sampler* pclSampler, int ires = 24, const fs::path& fn ="M_brdf_sampling.exr")
{
    std::cout << "Compute test sampling...";

    int i_num_theta_o = ires;
    int i_num_phi_o = ires;
    int i_num_theta_h = ires;
    int i_num_phi_h = ires;

    std::vector<float> tex_test_sampling_rgb( i_num_theta_o*i_num_phi_o * i_num_theta_h*i_num_phi_h * 3 );
    static const int NUMBER_OF_INCOMING_DIRECTIONS = 100;

    int i_x = 0;
    int i_counter = 0;
    for ( int i_theta_o = 0; i_theta_o < i_num_theta_o; ++i_theta_o )
    {
        float f_theta_o = ((float)i_theta_o+0.5f)/(float)i_num_theta_o * float(M_PI_2);
        for ( int i_phi_o = 0; i_phi_o < i_num_phi_o; ++i_phi_o )
        {
            i_x = i_theta_o * i_num_phi_o + i_phi_o;
            float f_phi_o = ((float)i_phi_o+0.5f)/(float)i_num_phi_o * (2.f * float(M_PI));
            float wo_polar[] = { f_theta_o, f_phi_o };
            float wo[3];
            polar2cartesian( wo_polar, wo );
            for ( int i = 0; i < NUMBER_OF_INCOMING_DIRECTIONS; ++i, ++i_counter )
            {
                float xi0 = rnd_norm();
                float xi1 = rnd_norm();
                float xi2 = rnd_norm();
                float wi[3];
                float xi3[] = {xi0,xi1,xi2};
                float uv[] = {0.5f,0.5f};
                float pdf = pclSampler->sample( xi3, wo, uv, wi );
                //float pdf_check = pclSampler->pdf( wi, wo, uv );
                if ( pdf > 0 && wi[2] > 0 )
                {
                    //if ( fabs(pdf - pdf_check) > 0.1)
                    //    LOG_WARNING << "Inconsistent pdf: " << fabs(pdf - pdf_check) << "   (" << pdf << ", " <<  pdf_check << ")\n";
                    float f_theta_i, f_phi_i;
                    cartesian2polar( wi, f_theta_i, f_phi_i );
                    int i_theta_i = (int)floor((f_theta_i/float(M_PI_2)) * i_num_theta_h);
                    int i_phi_i = (int)floor((f_phi_i/(2.f*float(M_PI))) * i_num_phi_h);
                    int i_y = i_theta_i * i_num_phi_h + i_phi_i;
                    float* p_pixel = &tex_test_sampling_rgb[3*(i_x + (i_y * i_num_theta_o*i_num_phi_o))];
                    if ( pdf > 0 )
                    {
                        p_pixel[0] = 1;
                        p_pixel[1] = 0;
                        p_pixel[2] = 0;
                    }
                    else
                    {
                        p_pixel[0] = 0;
                        p_pixel[1] = 1;
                        p_pixel[2] = 0;
                    }
                }
            }
        }
    }

#ifdef AXFSAMPLES_FREEIMAGE_DEPENDENCY
    ::saveFreeImage( fn, &tex_test_sampling_rgb[0], i_num_theta_o*i_num_phi_o, i_num_theta_h*i_num_phi_h, 1, 3, axfdec::ORIGIN_TOPLEFT );
#endif
    std::cout << std::endl;
}

//*************************************************************************************************
// demonstrates: metadata interface
// desc:
// print out metadata documents recursively
//*************************************************************************************************
template <typename T>
void print_metadata_property(axfdec::AXF_METADATA_DOCUMENT_HANDLE hAxFMetadataDoc, int iProperty, int iType, const char* sTypeName, const char* sPropertyName)
{
    int i_len = axfdec::axfGetMetadataPropertyValueLen(hAxFMetadataDoc, iProperty);
    std::cout << "Property <" << sPropertyName << ": ";
    std::cout << sTypeName << "[" << i_len << "]> = ";
    T* arr_value = new T[i_len];
    if (axfdec::axfGetMetadataPropertyValue(hAxFMetadataDoc, iProperty, iType, arr_value, static_cast<int>(i_len*sizeof(T))))
    {
        for (int i = 0; i < i_len - 1; ++i) std::cout << arr_value[i] << ",";
        std::cout << arr_value[i_len - 1];
    }
    delete [] arr_value;
    std::cout << std::endl;
}

template <>
void print_metadata_property<std::wstring>(axfdec::AXF_METADATA_DOCUMENT_HANDLE hAxFMetadataDoc, int iProperty, int iType, const char* sTypeName, const char* sPropertyName)
{
    assert(iType == axfdec::TYPE_UTF_STRING);
    int i_len = axfdec::axfGetMetadataPropertyValueLen(hAxFMetadataDoc, iProperty);
    std::cout << "Property <" << sPropertyName << ": ";
    std::cout << sTypeName << "[" << i_len << "]> = \"";
    wchar_t* psz_value = new wchar_t[i_len];
    if (axfdec::axfGetMetadataPropertyValue(hAxFMetadataDoc, iProperty, iType, psz_value, static_cast<int>(i_len*sizeof(wchar_t))))
    {
        std::cout << unicodeToConsoleOutput(std::wstring(psz_value));
    }
    delete [] psz_value;
    std::cout << "\"" << std::endl;
}

void print_metadata_doc(axfdec::AXF_METADATA_DOCUMENT_HANDLE hAxFMetadataDoc, int iLevel = 0)
{
    int i_buf_size = axfdec::axfGetMetadataDocumentName(hAxFMetadataDoc, 0, 0);
    if (i_buf_size > 0)
    {
        char* s_buf = new char[i_buf_size];
        axfdec::axfGetMetadataDocumentName(hAxFMetadataDoc, s_buf, i_buf_size);
        std::cout << std::setw(iLevel*3) << " ";
        std::cout << "Document <" << s_buf << ">" << std::endl;
        delete [] s_buf;
    }
    int i_num_props = axfdec::axfGetNumberOfMetadataProperties(hAxFMetadataDoc);
    for (int i_prop = 0; i_prop < i_num_props; ++i_prop)
    {
        i_buf_size = axfdec::axfGetMetadataPropertyName(hAxFMetadataDoc, i_prop, 0, 0);
        if (i_buf_size > 0)
        {
            char* s_buf = new char[i_buf_size];
            axfdec::axfGetMetadataPropertyName(hAxFMetadataDoc, i_prop, s_buf, i_buf_size);
            std::cout << std::setw((iLevel+1)*3) << " ";
            int i_storage_type = axfdec::axfGetMetadataPropertyType(hAxFMetadataDoc, i_prop);
            switch (i_storage_type)
            {
                case axfdec::TYPE_BOOLEAN:
                    std::cout << std::boolalpha;
                    print_metadata_property<bool>(hAxFMetadataDoc, i_prop, axfdec::TYPE_BOOLEAN, "bool", s_buf);
                    break;
                case axfdec::TYPE_INT:
                    print_metadata_property<int32_t>(hAxFMetadataDoc, i_prop, axfdec::TYPE_INT, "int32_t", s_buf);
                    break;
                case axfdec::TYPE_INT_ARRAY:
                    print_metadata_property<int32_t>(hAxFMetadataDoc, i_prop, axfdec::TYPE_INT_ARRAY, "int32_t", s_buf);
                    break;
                case axfdec::TYPE_HALF:
                case axfdec::TYPE_FLOAT:
                    print_metadata_property<float>(hAxFMetadataDoc, i_prop, axfdec::TYPE_FLOAT, "float", s_buf);
                    break;
                case axfdec::TYPE_HALF_ARRAY:
                case axfdec::TYPE_FLOAT_ARRAY:
                    print_metadata_property<float>(hAxFMetadataDoc, i_prop, axfdec::TYPE_FLOAT_ARRAY, "float", s_buf);
                    break;
                case axfdec::TYPE_STRING:
                case axfdec::TYPE_UTF_STRING:
                    print_metadata_property<std::wstring>(hAxFMetadataDoc, i_prop, axfdec::TYPE_UTF_STRING, "wchar_t", s_buf);
                    break;
                case axfdec::TYPE_ERROR:
                default:
                    std::cout << "ERROR: unsupported property type" << std::endl;
            }
            delete [] s_buf;
        }
    }

    int i_num_sub_docs = axfdec::axfGetNumberOfMetadataSubDocuments(hAxFMetadataDoc);
    for (int i_doc = 0; i_doc < i_num_sub_docs; ++i_doc)
    {        
        print_metadata_doc(axfdec::axfGetMetadataDataSubDocument(hAxFMetadataDoc, i_doc), iLevel + 1);
    }
}


//*************************************************************************************************
// logging_callback()
//
// desc: sample for a logging callback function for axfEnableLogging()
//*************************************************************************************************
void logging_callback(int iLogLevel, int iLogContext, const wchar_t* sLogMessage)
{
    try
    {
        // iLogLevel should be one of the values from enum ELogLevel, and iLogContext one of the values from enum ELogContext; the clamping is for safety only and shouldn't be needed
        static const char* asLogContexts[4] = { "AxF IO:       ", "AxF Decoders: ", "AxF SDK:      ", "" };
        static const char* asLogLevels[4]   = { "[info]    ",     "[warning] ",     "[error]   ",     "" };
        std::cout << asLogContexts[std::min<size_t>(iLogContext, 3)] << asLogLevels[std::min<size_t>(iLogLevel, 3)] << unicodeToConsoleOutput(sLogMessage) << std::endl;
    }
    catch (...)
    {
    }
}


#ifdef _WIN32
// native representation of fs::path is std::wstring
#define PO_PATH_STRING_VALUE po::wvalue<std::wstring>
#else
// native representation of fs::path is std::string
#define PO_PATH_STRING_VALUE po::value<std::string>
#endif

//*************************************************************************************************
// main()
// demonstrates: axfOpenFile(), axfCloseFile(), axfGetPreviewImage(), axfGetMetadataDocument()
// axf::decoding::CPUDecoder, axf::decoding::TextureDecoder, axf::decoding::Sampler
//
// desc: opens the given AxF file, retrieves representation handle, retrieves preview image,
// creates and demonstrates CPUDecoder, TextureDecoder and Sampler classes
//*************************************************************************************************
#ifdef _WIN32
int wmain(int argc, wchar_t* argv[])
#else
int main(int argc, char* argv[])
#endif
{
    std::cout << " ** AxF Decoding SDK demo tool **" << std::endl;
    std::cout << "(c) X-Rite 2014-2018, (Last built: " << __DATE__ << " " <<  __TIME__ << ")" << std::endl;

    axfdec::axfEnableLogging(&logging_callback, axfdec::LOGLEVEL_INFO);

#ifdef AXFSAMPLES_FREEIMAGE_DEPENDENCY
    ::FreeImage_Initialise();
#endif

    //command line interface
    fs::path::string_type s_input_file_or_dir, s_output_dir;
    std::wstring s_input_regex;
    std::string s_material_ID_string, s_target_color_space;
    bool b_recursive, b_recompute_preview, b_update_axf, b_skip_sampling_test;
    po::options_description cl_desc( "Program options" );
    cl_desc.add_options()
        ("help,?", "produce help message")
        ("in,i", PO_PATH_STRING_VALUE(&s_input_file_or_dir), "input AxF file or directory")
        ("filter,f", po::wvalue<std::wstring>(&s_input_regex)->default_value(L".*\\.axf$", ".*\\.axf$"), "input file filter (regular expression) [only relevant if --in specifies a directory]")
        ("recursive,R", po::bool_switch(&b_recursive), "traverse input directory recursively [only relevant if --in specifies a directory]")
        ("material,m", po::value<std::string>(&s_material_ID_string), "ID string of the material in the AxF file to decode (optional)")
        ("out,o", PO_PATH_STRING_VALUE(&s_output_dir), "output directory [if not specified the output is written into the same directory as the respective input file]")
        ("colspace,c", po::value<std::string>(&s_target_color_space)->default_value(AXF_COLORSPACE_LINEAR_SRGB_E), "render (target) color space")
        ("recompute-preview", po::bool_switch(&b_recompute_preview), "re-render preview image rather than reading an existing preview image from the AxF file")
        ("update-axf", po::bool_switch(&b_update_axf), "store computed preview image back to the input AxF file [this only has an effect if the input AxF file doesn't yet contain a preview image or if --recompute-preview is given]")
        ("skip-sampling", po::bool_switch(&b_skip_sampling_test), "skip test of BRDF sampler");


    po::variables_map cl_vm;
    try
    {
        po::store(po::parse_command_line(argc, argv, cl_desc), cl_vm);
        po::notify(cl_vm);

    }
    catch( std::exception& e )
    {
        std::cout << "error during command line parsing: " << e.what() << std::endl;
        return -1;
    }
    if (cl_vm.count("help"))
    {
        std::cout << std::endl << cl_desc << std::endl;
        return 0;
    }

    if (s_input_file_or_dir.empty())
    {
        std::cout << "\nPlease specify --in\n";
        std::cout << std::endl << "Type 'AxFDecode --help' or 'AxFDecode -?' for help" << std::endl;
        return -1;
    }

    bool b_readonly = !b_update_axf;

    std::cout << "Parameters:\n";
    std::cout << "  General:\n";
    std::cout << "    Input file or directory:  " << unicodeToConsoleOutput(fs::path(s_input_file_or_dir).wstring()) << "\n";
    std::cout << "    Input filter:             " << unicodeToConsoleOutput(s_input_regex) << "\n";
    std::cout << "    Recursive:                " << (b_recursive ? "yes" : "no") << "\n";
    std::cout << "    Input material ID:        " << (!s_material_ID_string.empty() ? s_material_ID_string : "(default)") << "\n";
    std::cout << "    Output directory:         " << (!s_output_dir.empty() ? unicodeToConsoleOutput(fs::path(s_output_dir).wstring()) : "(same as input)") << "\n";
    std::cout << "    Target color space:       " << s_target_color_space << "\n";
    std::cout << "    Read-only:                " << (b_readonly ? "yes" : "no") << "\n";


    try
    {
        std::vector<fs::path> vec_filenames;
        get_filenames( s_input_file_or_dir, s_input_regex, b_recursive, vec_filenames );

        BOOST_FOREACH( const fs::path& cl_path, vec_filenames )
        {
            if ( boost::algorithm::iequals( cl_path.extension().wstring(), L".axf" ) )
            {
                std::wstring s_stem = cl_path.stem().wstring();
                fs::path cl_output_dir_path = (!s_output_dir.empty()) ? fs::system_complete( s_output_dir ) : cl_path.parent_path();

                if ( !s_output_dir.empty() && ( !fs::exists(cl_output_dir_path) || !fs::is_directory(cl_output_dir_path) ) )
                {
                    LOG_ERROR << "Invalid output path: " << unicodeToConsoleOutput(cl_output_dir_path.wstring()) << LOG_ENDL;
                    XRITE_THROW( "Path does not exist or is no directory" );
                }

                std::cout << "\nDecoding file: " << unicodeToConsoleOutput(cl_path.wstring()) << "\n";

              #ifdef _WIN32
                axfdec::AXF_FILE_HANDLE h_axf_file = axfdec::axfOpenFileW( cl_path.c_str(), b_readonly );
              #else
                axfdec::AXF_FILE_HANDLE h_axf_file = axfdec::axfOpenFile( cl_path.c_str(), b_readonly );
              #endif
                if ( !h_axf_file ) XRITE_THROW( "Could not open AxF file" );

                char s_buf[axfdec::AXF_MAX_KEY_SIZE];

                //retrieve general information about materials in AxF file
                int i_num_materials = axfdec::axfGetNumberOfMaterials( h_axf_file );
                LOG_INFO << "Number of materials in file: " << i_num_materials << LOG_ENDL;

                axfdec::AXF_MATERIAL_HANDLE h_axf_material = 0;
                //do we have a material ID string provided?
                if (!s_material_ID_string.empty())
                {
                    h_axf_material = axfdec::axfFindMaterialByIDString(h_axf_file, s_material_ID_string.c_str());
                    if (!h_axf_material)
                    {
                        LOG_ERROR << "File does not contain a material with the given ID " << s_material_ID_string << LOG_ENDL;
                        axfdec::axfCloseFile(&h_axf_file);
                        continue;
                    }
                }
                else
                {
                    //try to retrieve default material
                    h_axf_material = axfdec::axfGetDefaultMaterial(h_axf_file);
                    if (h_axf_material)
                        LOG_INFO << "Retrieved default material" << LOG_ENDL;
                }

                axfdec::AXF_REPRESENTATION_HANDLE h_axf_rep = 0;
                if (h_axf_material)
                {
                    //show the material display name
                    int i_buf_size = axfdec::axfGetMaterialDisplayName(h_axf_material, 0, 0);
                    if (i_buf_size > 0)
                    {
                        wchar_t* ws_buf = new wchar_t[i_buf_size / sizeof(wchar_t)];
                        axfdec::axfGetMaterialDisplayName(h_axf_material, ws_buf, i_buf_size);
                        LOG_INFO << "Selected material display name: " << unicodeToConsoleOutput(std::wstring(ws_buf)) << LOG_ENDL;
                        delete [] ws_buf;
                    }

                    //print out metadata 
                    int i_num_metadata_docs = axfdec::axfGetNumberOfMetadataDocuments(h_axf_material);
                    if (i_num_metadata_docs != 0)
                    {
                        LOG_INFO << "Print metadata documents to stdout" << LOG_ENDL;
                        for (int i_doc = 0; i_doc < i_num_metadata_docs; ++i_doc)
                        {
                            axfdec::AXF_METADATA_DOCUMENT_HANDLE h_axf_doc = axfdec::axfGetMetadataDocument(h_axf_material, i_doc);
                            print_metadata_doc(h_axf_doc);
                        }
                    }

                    //choose the preferred representation
                    h_axf_rep = axfdec::axfGetPreferredRepresentation(h_axf_material);
                }
                else
                {
                    LOG_WARNING << "No material was specified via command line option -m, and the file has no default material either; choosing an arbitrary material for convenience." << LOG_ENDL;
                    h_axf_rep = axfdec::axfGetPreferredRepresentation(h_axf_file);
                }

                //test the representation: read some data, render image, test sampler etc. 
                if ( h_axf_rep )
                {                    
                    axfdec::axfGetRepresentationClass( h_axf_rep, s_buf, axfdec::AXF_MAX_KEY_SIZE );     //(useful to classify the representation as SVBRDF, CarPaint, ...)
                    std::string s_representation_class( s_buf );
                    LOG_INFO << "Found representation of class: " << s_representation_class << LOG_ENDL;
                  #if 0
                    axfdec::axfGetRepresentationTypeKey( h_axf_rep, s_buf, axfdec::AXF_MAX_KEY_SIZE );   //(note: for diagnostic output only, NOT useful otherwise)
                    std::string s_representation_type_key( s_buf );
                    LOG_INFO << "    Type of root node: " << s_representation_type_key << LOG_ENDL;
                  #endif

                    //get representation version
                    int i_major, i_minor, i_revision;
                    if (axfdec::axfGetRepresentationVersion(h_axf_rep, i_major, i_minor, i_revision))
                        LOG_INFO << "Representation version: " << i_major << "." << i_minor << "." << i_revision << LOG_ENDL;

                    //get child representations to be able to retrieve some additional information about them
                    axfdec::AXF_REPRESENTATION_HANDLE h_axf_rep_carpaint_flakes = 0;
                    axfdec::AXF_REPRESENTATION_HANDLE h_axf_rep_carpaint_tab_brdf = 0;
                    axfdec::AXF_REPRESENTATION_HANDLE h_axf_rep_diffuse = 0;
                    axfdec::AXF_REPRESENTATION_HANDLE h_axf_rep_specular = 0;
                    if ( s_representation_class == AXF_REPRESENTATION_CLASS_CARPAINT || s_representation_class == AXF_REPRESENTATION_CLASS_CARPAINT2 )
                    {
                        h_axf_rep_carpaint_flakes = axfdec::axfGetCarPaintFlakesBtfRepresentation( h_axf_rep );
                        if ( h_axf_rep_carpaint_flakes )
                        {
                            axfdec::axfGetRepresentationTypeKey( h_axf_rep_carpaint_flakes, s_buf, axfdec::AXF_MAX_KEY_SIZE );
                            LOG_INFO << "    Child representation: CarPaint flakes BTF of type:     " << s_buf << LOG_ENDL;
                        }
                        h_axf_rep_carpaint_tab_brdf = axfdec::axfGetCarPaintTabulatedBrdfRepresentation( h_axf_rep );
                        if ( h_axf_rep_carpaint_tab_brdf )
                        {
                            axfdec::axfGetRepresentationTypeKey( h_axf_rep_carpaint_tab_brdf, s_buf, axfdec::AXF_MAX_KEY_SIZE );
                            LOG_INFO << "    Child representation: CarPaint tabulated BRDF of type: " << s_buf << LOG_ENDL;
                        }
                    }
                    if ( s_representation_class == AXF_REPRESENTATION_CLASS_SVBRDF || s_representation_class == AXF_REPRESENTATION_CLASS_CARPAINT || s_representation_class == AXF_REPRESENTATION_CLASS_CARPAINT2 )
                    {
                        //retrieve information on SVBRDF type
                        h_axf_rep_diffuse = axfdec::axfGetSvbrdfDiffuseModelRepresentation( h_axf_rep );
                        if ( h_axf_rep_diffuse )
                        {
                            axfdec::axfGetRepresentationTypeKey( h_axf_rep_diffuse, s_buf, axfdec::AXF_MAX_KEY_SIZE );
                            LOG_INFO << "    Child representation: SVBRDF diffuse model of type:    " << s_buf << LOG_ENDL;
                        }
                        h_axf_rep_specular = axfdec::axfGetSvbrdfSpecularModelRepresentation( h_axf_rep );
                        if ( h_axf_rep_specular )
                        {
                            axfdec::axfGetRepresentationTypeKey( h_axf_rep_specular, s_buf, axfdec::AXF_MAX_KEY_SIZE );
                            LOG_INFO << "    Child representation: SVBRDF specular model of type:   " << s_buf << LOG_ENDL;

                            //retrieve information on specular model variants
                            bool b_has_fresnel = false;
                            bool b_is_anisotropic = false;
                            axfdec::axfGetSvbrdfSpecularModelVariant( h_axf_rep_specular, s_buf, axfdec::AXF_MAX_KEY_SIZE, b_is_anisotropic, b_has_fresnel );
                            if ( !std::string(s_buf).empty() )
                                LOG_INFO << "        Variant: " << s_buf << LOG_ENDL;

                            if ( b_has_fresnel )
                            {
                                axfdec::axfGetSvbrdfSpecularFresnelVariant( h_axf_rep_specular, s_buf, axfdec::AXF_MAX_KEY_SIZE );
                                LOG_INFO << "        Fresnel Variant: " << s_buf << LOG_ENDL;
                            }
                        }
                    }

                    //some resource testing (print out node paths and semantics for all resources)
                    int i_num_resources = axfdec::axfGetNumberOfRepresentationResources(h_axf_rep);
                    for (int i_resource = 0; i_resource < i_num_resources; ++i_resource)
                    {
                        axfdec::AXF_RESOURCE_HANDLE h_axf_resource = axfdec::axfGetRepresentationResourceFromIndex(h_axf_rep, i_resource);
                        if (h_axf_resource)
                        {
                            if (axfdec::axfGetResourceNodePath(h_axf_resource, s_buf, axfdec::AXF_MAX_KEY_SIZE) != 0)
                            {
                                LOG_INFO << "Resource " << i_resource << ": \"" << s_buf << "\"";
                                int i_num_dims = axfdec::axfGetResourceDataNumDims(h_axf_resource);
                                for (int i_dim = 0; i_dim < i_num_dims; ++i_dim)
                                    LOG << (i_dim == 0 ? " (" : "x") << axfdec::axfGetResourceDataDimExtent(h_axf_resource, i_dim);
                                LOG << (i_num_dims != 0 ? ")" : "") << LOG_ENDL;

                                bool b_referenced_by_child_representation = false;
                                if (h_axf_rep_carpaint_flakes && axfdec::axfGetResourceLookupPath(h_axf_rep_carpaint_flakes, h_axf_resource, s_buf, axfdec::AXF_MAX_KEY_SIZE) != 0)
                                {
                                    LOG_INFO << "    Referenced by CarPaint flakes BTF with semantic:     " << s_buf << LOG_ENDL;
                                    b_referenced_by_child_representation = true;
                                }
                                if (h_axf_rep_carpaint_tab_brdf && axfdec::axfGetResourceLookupPath(h_axf_rep_carpaint_tab_brdf, h_axf_resource, s_buf, axfdec::AXF_MAX_KEY_SIZE) != 0)
                                {
                                    LOG_INFO << "    Referenced by CarPaint tabulated BRDF with semantic: " << s_buf << LOG_ENDL;
                                    b_referenced_by_child_representation = true;
                                }
                                if (h_axf_rep_diffuse && axfdec::axfGetResourceLookupPath(h_axf_rep_diffuse, h_axf_resource, s_buf, axfdec::AXF_MAX_KEY_SIZE) != 0)
                                {
                                    LOG_INFO << "    Referenced by SVBRDF diffuse model with semantic:    " << s_buf << LOG_ENDL;
                                    b_referenced_by_child_representation = true;
                                }
                                if (h_axf_rep_specular && axfdec::axfGetResourceLookupPath(h_axf_rep_specular, h_axf_resource, s_buf, axfdec::AXF_MAX_KEY_SIZE) != 0)
                                {
                                    LOG_INFO << "    Referenced by SVBRDF specular model with semantic:   " << s_buf << LOG_ENDL;
                                    b_referenced_by_child_representation = true;
                                }
                                if (!b_referenced_by_child_representation && axfdec::axfGetResourceLookupPath(h_axf_rep, h_axf_resource, s_buf, axfdec::AXF_MAX_KEY_SIZE) != 0)
                                {
                                    LOG_INFO << "    Referenced by representation root with semantic:     " << s_buf << LOG_ENDL;
                                }
                            }
                        }
                    }

                    //example: check for a "DiffuseRoughnessThreshold" resource (if existing)
                    axfdec::AXF_RESOURCE_HANDLE h_axf_resource = axfdec::axfGetRepresentationResourceFromLookupName(h_axf_rep, "DiffuseRoughnessThreshold", true);
                    if (h_axf_resource)
                    {
                        if (1 == axfdec::axfGetResourceDataNumElems(h_axf_resource))
                        {
                            float f_threshold = 0;
                            if (axfdec::axfGetResourceData(h_axf_resource, &f_threshold, 1))
                            {
                                LOG_INFO << "Retrieved DiffuseRoughnessThreshold: " << f_threshold << LOG_ENDL;
                            }
                        }
                    }


                    //check compatibility profiles
                    struct { const char* sProfile; int iMaxProfileVersion; } arr_profiles[] = { { AXF_COMPAT_PROF_SVBRDF, 3 }, { AXF_COMPAT_PROF_SVBRDF_REFRACT, 3 }, { AXF_COMPAT_PROF_CARPAINT, 1 }, { AXF_COMPAT_PROF_CARPAINT_REFRACT, 1 }, { AXF_COMPAT_PROF_BTF, 1 } };
                    BOOST_FOREACH( const auto& prf, arr_profiles )
                    {
                        for ( int i_version = 1; i_version <= prf.iMaxProfileVersion; ++i_version )
                        {
                            if (axfdec::axfCheckRepresentationCompatibilityProfile(h_axf_rep, prf.sProfile, i_version))
                            {
                                LOG_INFO << "Passed compatibility check for profile " << prf.sProfile << " " << i_version << LOG_ENDL;
                            }
                            else
                            {
                                LOG_WARNING << "Compatibility check failed for profile " << prf.sProfile << " " << i_version << LOG_ENDL;
                            }
                        }
                    }

                    //render test image
                    LOG_INFO << (boost::format("-------render test image (in %s)-------") % s_target_color_space).str() << LOG_ENDL;
                    axfdec::CPUDecoder* pcl_axf_decode = axfdec::CPUDecoder::create( h_axf_rep, s_target_color_space.c_str(), MY_TEXTURE_ORIGIN );
                    if (pcl_axf_decode)
                    {
                        render_test_image( pcl_axf_decode, 4.0, 1.0, cl_output_dir_path/(s_stem + L"_TestRendering.exr") );
                    }
                    else
                    {
                        LOG_ERROR << "Creation of axfdec::CPUDecoder failed" << LOG_ENDL;   //(occurs when representation version is not supported by given SDK version)
                    }

                    //unless b_recompute_preview is set, try to load the default preview image from the AxF file first, if existing,
                    //i.e. determine the index of the preview image with name AXF_PREVIEW_IMAGE_NAME_DEFAULT, if existing, and read this from the file
                    int i_preview_image_index = -1;
                    if ( !b_recompute_preview )
                    {
                        int i_num_preview_images = axfdec::axfGetNumPreviewImages( h_axf_rep );
                        for ( int i = 0; i < i_num_preview_images; ++i )
                        {
                            if ( axfdec::axfGetPreviewImageName( h_axf_rep, i, s_buf, axfdec::AXF_MAX_KEY_SIZE ) && std::string( s_buf ) == AXF_PREVIEW_IMAGE_NAME_DEFAULT )
                                i_preview_image_index = i;
                        }
                    }

                    if ( i_preview_image_index >= 0 )   //(i.e. a default preview image exists in the AxF file and b_recompute_preview was not set)
                    {
                        LOG_INFO << "-------Get preview image-------" << LOG_ENDL;
                        int i_width, i_height, i_channels;
                        float f_size;
                        if ( axfdec::axfGetPreviewImageInfo( h_axf_rep, i_preview_image_index, i_width, i_height, i_channels, f_size, f_size ) )
                        {
                            std::vector<float> vec_preview_image( i_width*i_height*i_channels );
                            axfdec::axfGetPreviewImage( h_axf_rep, i_preview_image_index, s_target_color_space.c_str(), &vec_preview_image[0], i_width, i_height, i_channels, MY_TEXTURE_ORIGIN );
                          #ifdef AXFSAMPLES_FREEIMAGE_DEPENDENCY
                            fs::path cl_output_preview_image_path( cl_output_dir_path/(s_stem + L"_PreviewImage.exr") );
                            ::saveFreeImage( cl_output_preview_image_path, &vec_preview_image[0], i_width, i_height, 1, i_channels, MY_TEXTURE_ORIGIN );
                            LOG_INFO << "Preview image written to file '" << unicodeToConsoleOutput(cl_output_preview_image_path.filename().wstring()) << "'" << LOG_ENDL;
                          #endif
                        }
                    }
                    else if ( pcl_axf_decode )   //(otherwise we render a new preview image using CPUDecoder (when its creation did not fail))
                    {
                        LOG_INFO << "-------Compute preview image-------" << LOG_ENDL;

                        int i_width = 200, i_height = 200, i_dpi = 96;
                        int i_channels = (pcl_axf_decode->hasTransparency()) ? 4 : 3;
                        std::vector<float> vec_preview_image( i_width * i_height * i_channels );
                        float f_preview_width_mm  = i_width  * 25.4f / i_dpi;
                        float f_preview_height_mm = i_height * 25.4f / i_dpi;
                        pcl_axf_decode->computePreviewImage( vec_preview_image.data(), i_width, i_height, i_channels, f_preview_width_mm, f_preview_height_mm );
                      #ifdef AXFSAMPLES_FREEIMAGE_DEPENDENCY
                        fs::path cl_output_preview_image_path( cl_output_dir_path/(s_stem + L"_PreviewImage.exr") );
                        ::saveFreeImage( cl_output_preview_image_path, &vec_preview_image[0], i_width, i_height, 1, i_channels, MY_TEXTURE_ORIGIN );
                        LOG_INFO << "Preview image written to file '" << unicodeToConsoleOutput(cl_output_preview_image_path.filename().wstring()) << "'" << LOG_ENDL;
                      #endif
                        if ( b_update_axf )
                        {
                            LOG_INFO << "-------Store preview image in AxF file-------" << LOG_ENDL;
                            // note: MY_TEXTURE_ORIGIN == pcl_axf_decode->getTargetTextureOrigin()
                            std::string s_preview_image_name = AXF_PREVIEW_IMAGE_NAME_DEFAULT;
                            int i_result = axfdec::axfStorePreviewImage( h_axf_rep, vec_preview_image.data(), i_width, i_height, i_channels, f_preview_width_mm, f_preview_height_mm, s_target_color_space.c_str(), MY_TEXTURE_ORIGIN, s_preview_image_name.c_str() );
                            if ( i_result > 0 )
                            {
                                LOG_INFO << "Stored preview image '" << s_preview_image_name << "' in AxF" << LOG_ENDL;
                            }
                            else
                            {
                                LOG_WARNING << "Storing preview image in AxF failed" << LOG_ENDL;
                            }
                        }
                    }


                    LOG_INFO << "-------Get some textures and properties -------" << LOG_ENDL;
                    axfdec::TextureDecoder* pcl_axf_texdecode = axfdec::TextureDecoder::create(h_axf_rep,s_target_color_space.c_str(), MY_TEXTURE_ORIGIN);
                    if (pcl_axf_texdecode)
                    {
                        int i_num_properties = pcl_axf_texdecode->getNumProperties();
                        for ( int i = 0; i < i_num_properties; ++i )
                        {
                            char s_name[axfdec::AXF_MAX_KEY_SIZE];
                            pcl_axf_texdecode->getPropertyName( i, s_name, axfdec::AXF_MAX_KEY_SIZE );
                            int i_type = pcl_axf_texdecode->getPropertyType( i );
                            switch (i_type)
                            {
                                case axfdec::TYPE_INT:
                                case axfdec::TYPE_INT_ARRAY:
                                {
                                    std::vector<int> vec_int( pcl_axf_texdecode->getPropertySize( i )/sizeof(int) );
                                    pcl_axf_texdecode->getProperty( i, vec_int.data(), i_type, pcl_axf_texdecode->getPropertySize( i ) );
                                    std::stringstream ss;
                                    for (int j = 0; j < static_cast<int>(vec_int.size()) - 1; ++j)
                                    {
                                        ss << vec_int[j] << ",";
                                    }
                                    ss << vec_int.back();
                                    LOG_INFO << s_name << ": " << ss.str() << LOG_ENDL;
                                    break;
                                }
                                case axfdec::TYPE_FLOAT:
                                case axfdec::TYPE_FLOAT_ARRAY:
                                {
                                    std::vector<float> vec_float( pcl_axf_texdecode->getPropertySize( i )/sizeof(float) );
                                    pcl_axf_texdecode->getProperty( i, vec_float.data(), i_type, pcl_axf_texdecode->getPropertySize( i ) );
                                    std::stringstream ss;
                                    for ( int j = 0; j < static_cast<int>(vec_float.size())-1; ++j )
                                    {
                                        ss << vec_float[j] << ",";
                                    }
                                    ss << vec_float.back();
                                    LOG_INFO << s_name << ": " << ss.str() << LOG_ENDL;
                                    break;
                                }
                            }
                        }

                        int i_num_textures = pcl_axf_texdecode->getNumTextures();
                        LOG_INFO << "#Textures: " << i_num_textures << ", Extent (MM): " << pcl_axf_texdecode->getWidthMM() << "x" << pcl_axf_texdecode->getHeightMM() << LOG_ENDL;
                        for ( int i = 0; i < i_num_textures; ++i )
                        {
                            char s_name[axfdec::AXF_MAX_KEY_SIZE];
                            pcl_axf_texdecode->getTextureName(i, s_name, axfdec::AXF_MAX_KEY_SIZE);
                            int i_width, i_height, i_depth, i_channels, i_datatype;
                            pcl_axf_texdecode->getTextureSize(i, 0, i_width, i_height, i_depth, i_channels, i_datatype );
                            LOG_INFO << "Read texture " << s_name << " (" << i_width << "x" << i_height << "x" << i_depth << "x" << i_channels << ")";
                            float f_width_mm, f_height_mm;
                            if (pcl_axf_texdecode->getTextureSizeMM(i, f_width_mm, f_height_mm))
                                LOG << " Extent (MM): " << f_width_mm << "x" << f_height_mm;
                            LOG << LOG_ENDL;
                            std::vector<float> cl_texture(i_width*i_height*i_depth*i_channels);
                            pcl_axf_texdecode->getTextureData(i, 0, axfdec::TEXTURE_TYPE_FLOAT, &cl_texture[0]);

                          #ifdef AXFSAMPLES_FREEIMAGE_DEPENDENCY
                            //compose image output filename from input filename and texture name
                            std::wstring s_out_image_filename = s_stem + L"_";
                            BOOST_FOREACH( char c, std::string(s_name) )
                            {
                                if ( std::isalnum(c, std::locale::classic()) || c == '.' || c == '_' || c == '-' )   //(cf. boost::filesystem::portable_posix_name())
                                    s_out_image_filename += c;
                                else
                                    s_out_image_filename += L'_';
                            }
                            s_out_image_filename += L".exr";
                            if ( 2 == i_channels )
                            {
                                std::vector<float> cl_texture_3chan(i_width*i_height*i_depth*3, 0);
                                for ( int i = 0; i < i_width*i_height*i_depth; ++i )
                                {
                                    memcpy(&cl_texture_3chan[i*3],&cl_texture[i*2], 2*sizeof(float) );
                                }
                                ::saveFreeImage(cl_output_dir_path/s_out_image_filename, &cl_texture_3chan[0], i_width, i_height, i_depth, 3, MY_TEXTURE_ORIGIN);
                            }
                            else if ( i_channels > 4 )
                            {
                                LOG_WARNING << "Did not save texture to disk (channel count > 4)" << LOG_ENDL;
                            }
                            else
                            {
                                ::saveFreeImage(cl_output_dir_path/s_out_image_filename, &cl_texture[0], i_width, i_height, i_depth, i_channels, MY_TEXTURE_ORIGIN);
                            }
                          #endif
                        }
                    }
                    else
                    {
                        LOG_ERROR << "Creation of axfdec::TextureDecoder failed" << LOG_ENDL;   //(occurs when representation version is not supported by given SDK version)
                    }
                    axfdec::TextureDecoder::destroy( &pcl_axf_texdecode );

                    if (!b_skip_sampling_test)
                    {
                        LOG_INFO << "-------test sampling-------" << LOG_ENDL;
                        axfdec::Sampler* pcl_axf_sampler = axfdec::Sampler::create(h_axf_rep, pcl_axf_decode);
                        if (pcl_axf_sampler)
                        {
                            test_sampling(pcl_axf_sampler, 24, cl_output_dir_path / (s_stem + L"_M_sampling.exr"));
                        }
                        else
                        {
                            LOG_ERROR << "Creation of axfdec::Sampler failed" << LOG_ENDL;
                        }

                        axfdec::Sampler::destroy(&pcl_axf_sampler);
                    }
                    axfdec::CPUDecoder::destroy( &pcl_axf_decode );
                }
                else
                {
                    LOG_WARNING << "AxF material has no supported representation" << LOG_ENDL;
                }
                axfdec::axfCloseFile(&h_axf_file);
            }
            else
            {
                LOG_WARNING << "Skipping file " << unicodeToConsoleOutput(cl_path.wstring())  << " (unknown extension)" << LOG_ENDL;
            }
        }
    }
    catch( std::exception& e )
    {
        LOG_ERROR << e.what() << "\n" << LOG_ENDL;
        return -1;
    }

    return 0;
}



//*************************************************************************************************
// helper routines
//*************************************************************************************************

static void get_filenames_in_dir(const fs::path& rclDirPath, const boost::wregex& rclRegExFilter, bool bRecursive, std::vector<fs::path>& rvecFileNames)
{
    fs::directory_iterator it_end;
    fs::directory_iterator it(rclDirPath);
    for ( ; it != it_end; ++it )
    {
        if ( !fs::is_directory( it->status() ) )    //(note: using it->status() here rather than it->path() makes a _huge_ performance difference on smb shares)
        {
            if ( rclRegExFilter.empty() || boost::regex_match(it->path().leaf().wstring(),rclRegExFilter) )
            {
                rvecFileNames.push_back(it->path());
            }
        }
        else if (bRecursive)
            get_filenames_in_dir( *it, rclRegExFilter, bRecursive, rvecFileNames );
    }
}

static void get_filenames(const fs::path& rclFileNameOrDir, const std::wstring& sRegExFilter, bool bRecursive, std::vector<fs::path>& rvecFilenames)
{
    fs::path cl_path = fs::system_complete( rclFileNameOrDir );

    if ( !fs::exists(cl_path) )
    {
        LOG_ERROR << "Invalid input path: " << unicodeToConsoleOutput(cl_path.wstring()) << LOG_ENDL;
        XRITE_THROW( "Path does not exist" );
    }

    rvecFilenames.clear();
    if ( fs::is_directory(cl_path) )
    {
        boost::wregex cl_regex;
        if ( !sRegExFilter.empty() )
            cl_regex = boost::wregex(sRegExFilter, boost::wregex::perl|boost::wregex::icase);

        get_filenames_in_dir(cl_path, cl_regex, bRecursive, rvecFilenames);

        std::sort(rvecFilenames.begin(), rvecFilenames.end());
    }
    else
        rvecFilenames.push_back(cl_path);
}


static std::string unicodeToConsoleOutput(const std::wstring& sInput)
{
#ifdef _WIN32
    size_t ui_len = sInput.length();
    if ( ui_len == 0 ) return std::string();
    UINT ui_cp = GetConsoleOutputCP();
    int i_size_needed = WideCharToMultiByte(ui_cp, 0, sInput.data(), (int)ui_len, NULL, 0, NULL, NULL);
    std::string s_result(i_size_needed, 0);
    WideCharToMultiByte(ui_cp, 0, sInput.data(), (int)ui_len, &s_result[0], i_size_needed, NULL, NULL);
    return s_result;
#else
    // For simplicity, assume a UTF-8 console (which should be the default setting for all recent Linux and Mac versions):
    return std::wstring_convert<std::codecvt_utf8<wchar_t>>().to_bytes(sInput);
#endif
}


#ifdef AXFSAMPLES_FREEIMAGE_DEPENDENCY
static bool saveFreeImage(const fs::path& rclFileName, const float* pData, size_t w, size_t h, size_t d, size_t c, axfdec::ETextureOrigin eTextureOrigin)
{
  #ifdef _WIN32
    FREE_IMAGE_FORMAT fif = FreeImage_GetFIFFromFilenameU(rclFileName.c_str());
  #else
    FREE_IMAGE_FORMAT fif = FreeImage_GetFIFFromFilename(rclFileName.c_str());
  #endif
    if ( (fif != FIF_UNKNOWN) && (FreeImage_FIFSupportsWriting(fif)) )
    {
        size_t ui_num_chans = c;
        size_t ui_width = w;
        size_t ui_height = h * d;

        FREE_IMAGE_TYPE fit;
        if ( ui_num_chans == 1 )
            fit = FIT_FLOAT;
        else if ( ui_num_chans == 3 )
            fit = FIT_RGBF;
        else if ( ui_num_chans == 4 )
            fit = FIT_RGBAF;
        else
            throw std::runtime_error((boost::format("%d is unsupported channel count for FreeImage")%ui_num_chans).str());

        FIBITMAP* pcl_pic = FreeImage_AllocateT(fit,(int)ui_width,(int)ui_height);
        if ( !pcl_pic )
        {
            return false;
        }

        const float* pf_src = pData;
        size_t ui_num_values_per_row = ui_width*ui_num_chans;
        for(size_t y = 0; y < ui_height; y++)
        {
            float *pixel = (float*)FreeImage_GetScanLine(pcl_pic, (eTextureOrigin == axfdec::ORIGIN_TOPLEFT) ? (int)(ui_height-y-1) : (int)y);   //(FreeImage uses bottom-left texture origin convention)
            for(size_t i=0; i<ui_num_values_per_row; i++)
                pixel[i] = pf_src[i];
            pf_src += ui_num_values_per_row;
        }

      #ifdef _WIN32
        bool b_result = (FreeImage_SaveU(fif,pcl_pic,rclFileName.c_str())!=0);
      #else
        bool b_result = (FreeImage_Save(fif,pcl_pic,rclFileName.c_str())!=0);
      #endif

        FreeImage_Unload(pcl_pic);
        return b_result;

    }

    return false;
}
#endif


static void polar2cartesian( float theta, float phi, float res[3] )
{
    float ct = (float) cos(theta);
    float cp = (float) cos(phi);
    float st = (float) sin(theta);
    float sp = (float) sin(phi);
    res[0] = st*cp;
    res[1] = st*sp;
    res[2] = ct;
}

static void polar2cartesian( const float v2Polar[2], float res[3] )
{
    polar2cartesian(v2Polar[0],v2Polar[1],res);
}

static void cartesian2polar( const float v[3], float& theta, float& phi )
{
    float z = (v[2]>1) ? 1 : (v[2]<-1) ? -1 : v[2];
    theta = (float) acos(z);
    phi =   (float) atan2(v[1], v[0]);
    if (phi<0) phi = phi+2*(float)M_PI;
}

static float rnd_norm()
{
    return rand()/(float)((unsigned int)RAND_MAX+1);
}
