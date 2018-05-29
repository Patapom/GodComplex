///////////////////////////////////////////////////////////////////////////////
// File:		AxF_basic_io.h
// Authors:		Gero Mueller, Alexander Gress
//
// Title:	DLL interface for basic AxF access
// Library: AxF SDK
//
// Version:	1.5
// Created:	2014/06/24
//
// Copyright:  X-Rite 2014-2018
//  		   www.xrite.com
//
//-----------------------------------------------------------------------------
//
//
///////////////////////////////////////////////////////////////////////////////
#pragma once


#include "api_def.h"


namespace axf
{
    /*!
    This namespace includes functionality required for reading and decoding AxF representations.
    It is exported as a C-only interface for maximum portability.
    Nevertheless, for convenience this version of the  decoding interface uses C++ language elements.
    The C-only version is available on request.
    */
    namespace decoding {}
}


        //compatibility profiles (for axfCheckRepresentationCompatibilityProfile())
#define AXF_COMPAT_PROF_SVBRDF "AxFSvbrdf"
#define AXF_COMPAT_PROF_SVBRDF_REFRACT "AxFSvbrdfRefract"
#define AXF_COMPAT_PROF_CARPAINT "AxFCarPaint"
#define AXF_COMPAT_PROF_CARPAINT_REFRACT "AxFCarPaintRefract"
#define AXF_COMPAT_PROF_BTF "AxFBtf"
        //deprecated compatibility profiles - don't use in new code
#define AXF_COMPAT_PROF_BASELINE_SVBRDF "AxFBaselineFixedSvbrdf"
#define AXF_COMPAT_PROF_BASELINE_BTF "AxFBaselineFixedBtf"


        //representation class strings (as returned by axfGetRepresentationClass())
#define AXF_REPRESENTATION_CLASS_SVBRDF "SVBRDF"
#define AXF_REPRESENTATION_CLASS_CARPAINT "CarPaint"
#define AXF_REPRESENTATION_CLASS_CARPAINT2 "CarPaint2"
#define AXF_REPRESENTATION_CLASS_FACTORIZED_BTF "FactorizedBTF" 
#define AXF_REPRESENTATION_CLASS_LAYERED "Layered"

        //svbrdf child representation type key strings (as returned by axfGetRepresentationTypeKey())
#define AXF_TYPEKEY_SVBRDF_DIFFUSE_LAMBERT "com.xrite.LambertDiffuseModel"
#define AXF_TYPEKEY_SVBRDF_DIFFUSE_ORENNAYAR "com.xrite.OrenNayarDiffuseModel"
#define AXF_TYPEKEY_SVBRDF_SPECULAR_WARD "com.xrite.WardSpecularModel"
#define AXF_TYPEKEY_SVBRDF_SPECULAR_BLINNPHONG "com.xrite.BlinnPhongSpecularModel"
#define AXF_TYPEKEY_SVBRDF_SPECULAR_COOKTORRANCE "com.xrite.CookTorranceSpecularModel"
#define AXF_TYPEKEY_SVBRDF_SPECULAR_PHONG "com.xrite.PhongSpecularModel"
#define AXF_TYPEKEY_SVBRDF_SPECULAR_GGX "com.xrite.GGXSpecularModel"

        //svbrdf specular model variant strings (as returned by axfGetSvbrdfSpecularModelVariant())
#define AXF_SVBRDF_SPECULAR_WARD_VARIANT_GEISLERMORODER "GeislerMoroder2010"
#define AXF_SVBRDF_SPECULAR_WARD_VARIANT_DUER "Duer2006"
#define AXF_SVBRDF_SPECULAR_WARD_VARIANT_WARD "Ward1992"
#define AXF_SVBRDF_SPECULAR_BLINN_VARIANT_ASHIKHMIN_SHIRLEY "Ashikhmin2000"
#define AXF_SVBRDF_SPECULAR_BLINN_VARIANT_BLINN "Blinn1977"
#define AXF_SVBRDF_SPECULAR_BLINN_VARIANT_VRAY "VRay"
#define AXF_SVBRDF_SPECULAR_BLINN_VARIANT_LEWIS "Lewis1993"

        //svbrdf fresnel model variant strings (as returned by axfGetSvbrdfSpecularFresnelVariant())
#define AXF_SVBRDF_FRESNEL_VARIANT_SCHLICK "Schlick1994"
#define AXF_SVBRDF_FRESNEL_VARIANT_FRESNEL "Fresnel1818"

        //factorized btf representation type key strings (as returned by axfGetRepresentationTypeKey())
#define AXF_TYPEKEY_FACTORIZED_BTF_DFMF "com.xrite.Dfmf"
#define AXF_TYPEKEY_FACTORIZED_BTF_DPVF "com.xrite.Dpvf"

        //factorized btf representation variant strings (as returned by axfGetRepresentationVariant())
#define AXF_FACTORIZED_BTF_REPRESENTATION_VARIANT_DEFAULT ""
#define AXF_FACTORIZED_BTF_REPRESENTATION_VARIANT_SQRTY "SqrtY"

        //preview image name strings (as returned by axfGetPreviewImageName())
#define AXF_PREVIEW_IMAGE_NAME_DEFAULT "default"

        //svbrdf texture name strings (as returned by TextureDecoder::getTextureName() for Representation Class *SVBRDF*)
#define AXF_SVBRDF_TEXTURE_NAME_DIFFUSE_COLOR "DiffuseColor"
#define AXF_SVBRDF_TEXTURE_NAME_NORMAL "Normal"
#define AXF_SVBRDF_TEXTURE_NAME_SPECULAR_COLOR "SpecularColor"
#define AXF_SVBRDF_TEXTURE_NAME_SPECULAR_LOBE "SpecularLobe"
#define AXF_SVBRDF_TEXTURE_NAME_ANISO_ROTATION "AnisoRotation"
#define AXF_SVBRDF_TEXTURE_NAME_ALPHA "Alpha"
#define AXF_SVBRDF_TEXTURE_NAME_HEIGHT "Height"
#define AXF_SVBRDF_TEXTURE_NAME_FRESNEL "Fresnel"
#define AXF_SVBRDF_TEXTURE_NAME_CLEARCOAT_NORMAL "ClearcoatNormal"
#define AXF_SVBRDF_TEXTURE_NAME_CLEARCOAT_IOR "ClearcoatIOR"
#define AXF_SVBRDF_TEXTURE_NAME_CLEARCOAT_COLOR "ClearcoatColor"
#define AXF_SVBRDF_TEXTURE_NAME_SUBSURFACESCATTERING_TRANSMISSIONCOLOR "SubSurfaceScatteringTransmissionColor"
#define AXF_SVBRDF_TEXTURE_NAME_SUBSURFACESCATTERING_EXTINCTIONLENGTH "SubSurfaceScatteringExtinctionLength"

        //CarPaint2 texture name string
#define AXF_CARPAINT2_TEXTURE_NAME_BRDF_COLORS "BRDFcolors"
#define AXF_CARPAINT2_TEXTURE_NAME_BTF_FLAKES "BTFflakes"
#define AXF_CARPAINT2_TEXTURE_NAME_CLEARCOAT_NORMAL "ClearcoatNormal"

        //CarPaint2 property name strings (as returned by TextureDecoder::getPropertyName() for Representation Class *CarPaint2*)
#define AXF_CARPAINT2_PROPERTY_BRDF_CT_DIFFUSE "CT_diffuse"
#define AXF_CARPAINT2_PROPERTY_BRDF_CT_COEFFS "CT_coeffs"
#define AXF_CARPAINT2_PROPERTY_BRDF_CT_F0S "CT_F0s"
#define AXF_CARPAINT2_PROPERTY_BRDF_CT_SPREADS "CT_spreads"
#define AXF_CARPAINT2_PROPERTY_FLAKES_NUM_THETAF "num_thetaF"
#define AXF_CARPAINT2_PROPERTY_FLAKES_NUM_THETAI "num_thetaI"
#define AXF_CARPAINT2_PROPERTY_FLAKES_MAX_THETAI "max_thetaI"
#define AXF_CARPAINT2_PROPERTY_FLAKES_THETAFI_SLICE_LUT "thetaFI_sliceLUT"
#define AXF_CARPAINT2_PROPERTY_CC_IOR "IOR"

        //Clearcoat property name strings (as returned by TextureDecoder::getPropertyName() for Representation Classes *SVBRDF* and *CarPaint2*)
#define AXF_CLEARCOAT_PROPERTY_NAME_NO_REFRACTION "cc_no_refraction"

        //target color space strings
#define AXF_COLORSPACE_CIE_1931_XYZ "XYZ"
#define AXF_COLORSPACE_LINEAR_SRGB_E "sRGB,E"
#define AXF_COLORSPACE_LINEAR_ADOBE_RGB_E "AdobeRGB,E"
#define AXF_COLORSPACE_LINEAR_ADOBE_WIDEGAMUT_RGB_E "WideGamutRGB,E"
#define AXF_COLORSPACE_LINEAR_PROPHOTO_RGB_E "ProPhotoRGB,E"


////////////////////////////////////////////////
//
//   basic AxF access functionality
//
///////////////////////////////////////////////
AXF_DECODING_OPEN_NAMESPACE

        enum {
            AXF_MAX_KEY_SIZE = 256  //< maximum size of fixed ASCII strings (type keys etc.) returned by this interface (including 0-terminator)
        };


        //handle types
        struct AxFFileHandle;
        typedef AxFFileHandle* AXF_FILE_HANDLE;
        struct AxFMaterialHandle;
        typedef AxFMaterialHandle* AXF_MATERIAL_HANDLE;
        struct AxFRepresentationHandle;
        typedef AxFRepresentationHandle* AXF_REPRESENTATION_HANDLE;
        struct AxFMetadataDocumentHandle;
        typedef AxFMetadataDocumentHandle* AXF_METADATA_DOCUMENT_HANDLE;
        struct AxFResourceHandle;
        typedef AxFResourceHandle* AXF_RESOURCE_HANDLE;

        //enums
        //! Enum of property data types used in the metadata retrieval interface and TextureDecoder's property interface.
        enum PropertyType
        {
            TYPE_HALF        = 0,       //!< 16-bit IEEE half-precision floating point (not yet used for properties)
            TYPE_HALF_ARRAY  = 1,       //!< array of TYPE_HALF
            TYPE_INT         = 2,       //!< 32-bit signed integer
            TYPE_INT_ARRAY   = 3,       //!< array of TYPE_INT
            TYPE_FLOAT       = 4,       //!< 32-bit IEEE single-precision floating point
            TYPE_FLOAT_ARRAY = 5,       //!< array of TYPE_FLOAT
            TYPE_STRING      = 7,       //!< null-terminated array of 8-bit characters, representing a string in ISO-8859-1 (Latin-1) encoding
            TYPE_UTF_STRING  = 8,       //!< null-terminated array of wchar_t characters, representing a string in UTF-16 encoding (Windows) or UTF-32 encoding (Linux/Mac)
            TYPE_BOOLEAN     = 9,       //!< 8-bit boolean
            TYPE_ERROR       = 0xFFFF
        };

        //! Enum of texture types used in TextureDecoder's texture retrieval interface.
        enum TextureType
        {
            TEXTURE_TYPE_HALF  = 0,     //!< texture of 16-bit IEEE half-precision floating point values
            TEXTURE_TYPE_FLOAT = 4,     //!< texture of 32-bit IEEE single-precision floating point values
            TEXTURE_TYPE_BYTE  = 6      //!< texture of 8-bit unsigned integer values
        };

        //! Spatially varying textures need to have a defined embedding into 3D
        /*!
        Appearance data like BTF images or SVBRDFs is implicitly embedded into a local 3D coordinate system which is usually placed in one of the corners of the images.
        The xy directions of this system are aligned with plane defined by the 2D image coordinate system and the z direction points up into 3D space. This system defines how directions like a
        light source direction are interpreted.

        Most rendering applications define a local coordinate system at each point where shading calculations are performed (usually called *local tangent space*). This space needs to match the
        aforementioned material's local coordinate system. AxF allows to query spatial varying appearance data with two different coordinate systems:
            1. top left image corner
            2. bottom left image corner
            .
        */
        //These two different coordinate systems correspond to the mainly used image coordinate systems: e.g. OpenGL uses an image coordinate system with its origin in the bottom left corner
        //while DirectX places the origin in the top left corner.
        enum ETextureOrigin
        {
            ORIGIN_TOPLEFT    = 0,      //!< Local coordinate system placed in top left corner
            ORIGIN_BOTTOMLEFT = 1       //!< Local coordinate system placed in bottom left corner
        };


        //! Log levels for axfEnableLogging() and logging callback
        enum ELogLevel
        {
            LOGLEVEL_INFO    = 0,       //!< Informational messages
            LOGLEVEL_WARNING = 1,       //!< Warning messages
            LOGLEVEL_ERROR   = 2        //!< Error messages
        };

        //! Log context indicator for logging callback
        enum ELogContext
        {
            LOGCONTEXT_AXF_IO   = 0,    //!< Log message is related to basic AxF file IO
            LOGCONTEXT_DECODERS = 1,    //!< Log message is related to the use of CPUDecoder or TextureDecoder
            LOGCONTEXT_GENERIC  = 2     //!< Log message is related to the SDK in general
        };

        //! Function pointer type for logging callback function
        typedef void (*AxFLoggingCallbackPtr)( int iLogLevel, int iLogContext, const wchar_t* sLogMessage );


        /** @name Logging
        */
        ///@{
        /*! \brief Enable logging.
            \return true if successful

            \param pCallback Pointer to callback a function of signature: void callback_func(int iLogLevel, int iLogContext, const char* sLogMessage)
            \param iLogLevel The minimum level for log messages (i.e. LOGLEVEL_INFO shows all messages, LOGLEVEL_WARNING warning and error messages, and LOGLEVEL_ERROR only error messages)

            This function can be used to enable logging, for instance for debugging purposes. (By default, logging is disabled.)
            It calls the specified callback function for each log message (cf. sample code).
        */
        bool AXF_API axfEnableLogging( AxFLoggingCallbackPtr pCallback, int iLogLevel = LOGLEVEL_INFO );
        /*! \brief Disable logging.

            This function disables the logging again.
        */
        void AXF_API axfDisableLogging();
        ///@}


        /** @name Open/Close
        */
        ///@{
        /*! \brief Open an AxF File and return handle.

            \param sFilename Path to AxF file (in the 8-bit encoding that is used by the standard C/C++ file APIs)
            \param bReadOnly Open as read only
            \param bReadLazy Only read header on first access. Read data on demand.
            \return Handle to AxF file to be used in subsequent calls.

            This function expects the path in the 8-bit encoding that is used by the standard C/C++ file APIs.
            Note: In case of Mac OS X, this is specified as UTF-8.
                  However, in case of Windows this is a system-locale-dependent, non-Unicode encoding (see MSDN, for instance AreFileAPIsANSI(), for details), which in general can only represent
                  a subset of the characters supported by the character encoding of the Windows filesystem (UTF-16). Therefore, on Windows it is more recommendable to use axfOpenFileW() instead.

            After usage the AxF file handle has to be closed using axfCloseFile().
        */
        AXF_FILE_HANDLE AXF_API axfOpenFile( const char* sFilename, bool bReadOnly = false, bool bReadLazy = true );
        ///@{
        /*! \brief Open an AxF File and return handle.

            \param sFilename Path to AxF file (in 16- or 32-bit Unicode encoding, according to the compiler's wchar_t definition)
            \param bReadOnly Open as read only
            \param bReadLazy Only read header on first access. Read data on demand.
            \return Handle to AxF file to be used in subsequent calls.

            Unicode version of axfOpenFile(), which expects the path in UTF-16 encoding on Windows or in UCS-4/UTF-32 encoding on Linux/Mac.
            Note: In case of Windows, this corresponds to native character encoding of the filesystem and thus maps directly to the Windows wide-char file APIs.
                  For Linux and Mac, which do not have wide-char file APIs, this function tries to translate the Unicode representation of the path to the native 8-bit character
                  encoding of the filesystem and calls axfOpenFile() on that. However, for Linux this conversion is not fully well-defined, as the filesystem's encoding is in general not
                  well-specified there (the current implementation uses the system locale on Linux like e.g. boost::filesystem does; so this may or may not work as expected on that system).
                  Therefore, on Linux it is in general more recommendable to use axfOpenFile() directly instead.

            After usage the AxF file handle has to be closed using axfCloseFile().
        */
        AXF_FILE_HANDLE AXF_API axfOpenFileW( const wchar_t* sFilename, bool bReadOnly = false, bool bReadLazy = true );
        /*! \brief Closes a valid AxF file handle.

            \param phAxfFile Pointer to valid AxF file handle

            After closing the file handle all other open handles retrieved for the file become invalid.
        */
        void AXF_API axfCloseFile(AXF_FILE_HANDLE* phAxfFile);
        ///@}


        /** @name AxF Materials
        */
        ///@{
        /*! \brief Get the number of materials stored in the AxF file

            \param hAxFFile Handle to open AxF File
            \return Number of materials stored in the AxF file

            Each AxF file stores a certain number of *materials*.
            An AxF material can represent for instance a measured real-world material, and edited version of it, or some virtual material.
        */
        int AXF_API axfGetNumberOfMaterials(AXF_FILE_HANDLE hAxFFile);
        /*! \brief Get a handle to a specific material stored in the AxF file

            \param hAxFFile Handle to open AxF file
            \param iMaterial Index of the material in the file (in range 0 .. axfGetNumberOfMaterials()-1)
            \return Handle to material (or zero)
        */
        AXF_MATERIAL_HANDLE AXF_API axfGetMaterial( AXF_FILE_HANDLE hAxFFile, int iMaterial );
        /*! \brief Get a handle to the *default* material stored in the AxF file

            \param hAxFFile Handle to open AxF file
            \return Handle to found material (or zero)

            AxF files may have a designated default material.
            This is implicitly the case for AxF files consisting of a single material only.
            AxF files with multiple materials might also have a designated default material, for instance if there is a layered material which references all other materials in the file as layers.

            If the AxF files contains multiple materials without a designated default material, this function fails with return value 0.
            In that case the desired material handle needs to be queried via axfGetMaterial().

            For AxF files with multiple materials in general some kind of user interaction is recommended to allow the user choosing a material (at least when there is no designated default material).
            The default material, if existing, is useful for instance to preselect a material in case of user interaction, or to autonomously choose a material from the file without user interaction
            (such as to choose a preview image that is representative for the full AxF file).
        */
        AXF_MATERIAL_HANDLE AXF_API axfGetDefaultMaterial( AXF_FILE_HANDLE hAxFFile );
        /*! \brief Get the display name of a given material

            \param hAxFMaterial Handle to an AxF material
            \param sBuf Character buffer big enough to hold the name of the material (or 0 to query the required buffer size only)
            \param iBufSize size of sBuf in *bytes*
            \return The buffer size in bytes that is needed to return the full string (number of characters - including terminating 0 - times sizeof(wchar_t)). If iBufSize was at least this large, the full string was returned successfully, otherwise the returned string was truncated. 0 indicates an error (in which case no string is returned).

            Returns the display name of a material as Unicode wide character string, i.e. in UTF-16 encoding on Windows or in UCS-4/UTF-32 encoding on Linux/Mac.
            It is recommended to query the required buffer size first (see parameter description), since the length of the display name is not limited.
        */
        int AXF_API axfGetMaterialDisplayName( AXF_MATERIAL_HANDLE hAxFMaterial, wchar_t* sBuf, int iBufSize );
        /*! \brief Get the ID string of a given material

            \param hAxFMaterial Handle to an AxF material
            \param sBuf Character buffer big enough to hold the ID string of the material (or 0 to query the required buffer size only)
            \param iBufSize size of sBuf in bytes
            \return The buffer size in bytes that is needed to return the full string (number of characters including terminating 0). If iBufSize was at least this large, the full string was returned successfully, otherwise the returned string was truncated. 0 indicates an error (in which case no string is returned).

            In the scope of a given AxF file, the ID strings for the contained materials are unique and remain the same even if the file is edited (but not if the material is renamed).
            Therefore the ID string can be used as to re-indentify a certain material after closing and reopening the file (in contrast to the material handle which is invalidated when the file is closed), see axfFindMaterialByIDString().
            In contrast to the material display name (see axfGetMaterialDisplayName()), the ID string returned by this function should not be displayed to the user.
            Though if you want to display this string e.g. in debug output, you can assume ISO-8859-1 (Latin-1) encoding. Note that the length of the ID string is not limited by AXF_MAX_KEY_SIZE.
        */
        int AXF_API axfGetMaterialIDString( AXF_MATERIAL_HANDLE hAxFMaterial, char* sBuf, int iBufSize );
        /*! \brief Get the *ID string* of a given material (deprecated)

            \param hAxFFile Handle to open AxF file
            \param iMaterial Index of the material in the file (in range 0 .. axfGetNumberOfMaterials()-1)
            \param sBuf Character buffer big enough to hold the ID string of the material (or 0 to query the required buffer size only)
            \param iBufSize size of sBuf
            \return The buffer size that is needed to return the full string (number of characters including terminating 0). If iBufSize was at least this large, the full string was returned successfully, otherwise the returned string was truncated. 0 indicates an error (in which case no string is returned).

            DEPRECATED: equivalent to axfGetMaterialIDString(axfGetMaterial(hAxFFile, iMaterial), sBuf, iBufSize)

            Note: To query the display name of a material in order to show this in a UI, use axfGetMaterialDisplayName() instead.
        */
        int AXF_API axfGetMaterialName( AXF_FILE_HANDLE hAxFFile, int iMaterial, char* sBuf, int iBufSize );
        /*! \brief Get the *ID string* of the default material in the AxF file (deprecated)

            \param hAxFFile Handle to open AxF file
            \param sBuf Character buffer big enough to hold the ID string of the material (or 0 to query the required buffer size only)
            \param iBufSize size of sBuf
            \return The buffer size that is needed to return the full string (number of characters including terminating 0). If iBufSize was at least this large, the full string was returned successfully, otherwise the returned string was truncated. 0 indicates an error (in which case no string is returned).

            DEPRECATED: equivalent to axfGetMaterialIDString(axfGetDefaultMaterial(hAxFFile), sBuf, iBufSize)
        */
        int AXF_API axfGetDefaultMaterialName( AXF_FILE_HANDLE hAxFFile, char* sBuf, int iBufSize );
        /*! \brief Get the handle to a specific material stored in the AxF file based on its ID string

            \param hAxFFile Handle to open AxF file
            \param sMaterialID ID string of the material (cf. axfGetMaterialIDString())
            \return Handle to found material (or zero)
        */
        AXF_MATERIAL_HANDLE AXF_API axfFindMaterialByIDString( AXF_FILE_HANDLE hAxFFile, const char* sMaterialID );

        /** @name AxF Material Metadata

        Each material can have a metadata section which allows to store an arbitrary number of metadata documents.
        */
        ///@{
        /*! \brief Get the number of metadata documents for an AxF material

        \param hAxFMaterial Handle to an AxF material
        \return Number of metadata documents stored in the metadata section of the material

        An AxF material can have more than one metadata section, i.e. *documents*,  in order to support different workflows.
        */
        int AXF_API axfGetNumberOfMetadataDocuments(AXF_MATERIAL_HANDLE hAxFMaterial);
        ///@{
        /*! \brief Get the number of metadata documents for an AxF material (deprecated)

        \param hAxFFile Handle to open AxF File
        \param sMaterialID ID string of the material (cf. axfGetMaterialIDString())
        \return Number of metadata documents stored in the metadata section of the material

        DEPRECATED variant of the function: equivalent to axfGetNumberOfMetadataDocuments(axfFindMaterialByIDString(hAxFFile, sMaterialID))
        */
        int AXF_API axfGetNumberOfMetadataDocuments(AXF_FILE_HANDLE hAxFFile, const char* sMaterialID);

        /*! \brief Return handle to a material's metadata document

        \param hAxFMaterial Handle to an AxF material
        \param iMetadataDocument Index of the metadata document in the material's metadata document list.
        \return Handle to found metadata document (or zero)
        */
        AXF_METADATA_DOCUMENT_HANDLE AXF_API axfGetMetadataDocument(AXF_MATERIAL_HANDLE hAxFMaterial, int iMetadataDocument);
        /*! \brief Return handle to a material's metadata document (deprecated)

        \param hAxFFile Handle to open AxF File
        \param sMaterialID ID string of the material (cf. axfGetMaterialIDString())
        \param iMetadataDocument Index of the metadata document in the material's metadata document list.
        \return Handle to found metadata document (or zero)

        DEPRECATED variant of the function: equivalent to axfGetMetadataDocument(axfFindMaterialByIDString(hAxFFile, sMaterialID), iMetadataDocument)
        */
        AXF_METADATA_DOCUMENT_HANDLE AXF_API axfGetMetadataDocument(AXF_FILE_HANDLE hAxFFile, const char* sMaterialID, int iMetadataDocument);

        /*! \brief Get the name of a metadata document stored in the AxF file

        \param hAxFMetadataDocument Handle to metadata document
        \param sBuf Character buffer big enough to hold the name (or 0 to query the required buffer size only)
        \param iBufSize size of sBuf
        \return The buffer size that is needed to return the full string (number of characters including terminating 0). If iBufSize was at least this large, the full string was returned successfully, otherwise the returned string was truncated. 0 indicates an error (in which case no string is returned).
        */
        int AXF_API axfGetMetadataDocumentName(AXF_METADATA_DOCUMENT_HANDLE hAxFMetadataDocument, char* sBuf, int iBufSize);

        /*! \brief Return the number of sub documents for an existing metadata document

        \param hAxFMetadataDocument Handle to metadata document
        \return Number of sub- or child-metadata documents for the given metadata document

        AxF metadata documents are recursive, i.e. they can contain sub documents.
        */
        int AXF_API axfGetNumberOfMetadataSubDocuments(AXF_METADATA_DOCUMENT_HANDLE hAxFMetadataDocument);

        /*! \brief Return handle to a metadata subdocument

        \param hAxFMetadataDocument Handle to axf metadata document
        \param iMetadataSubdocument Index of the metadata subdocument in the parent's metadata document list.
        \return Handle to found metadata document (or zero)
        */
        AXF_METADATA_DOCUMENT_HANDLE AXF_API axfGetMetadataDataSubDocument(AXF_METADATA_DOCUMENT_HANDLE hAxFMetadataDocument, int iMetadataSubdocument);

        /*! \brief Return the number of properties for an existing metadata document

        \param hAxFMetadataDocument Handle to metadata document
        \return Number of properties for the given metadata document
        */
        int AXF_API axfGetNumberOfMetadataProperties(AXF_METADATA_DOCUMENT_HANDLE hAxFMetadataDocument);

        /*! \brief Get the name of the metadata property

        \param hAxFMetadataDocument Handle to metadata document
        \param iProperty Index of the property
        \param sBuf Character buffer big enough to hold the name of the property (or 0 to query the required buffer size only)
        \param iBufSize size of sBuf
        \return The buffer size that is needed to return the full string (number of characters including terminating 0). If iBufSize was at least this large, the full string was returned successfully, otherwise the returned string was truncated. 0 indicates an error (in which case no string is returned).

        Metadata properties are classical name-value pairs inside a metadata document. 
        */
        int AXF_API axfGetMetadataPropertyName(AXF_METADATA_DOCUMENT_HANDLE hAxFMetadataDocument, int iProperty, char* sBuf, int iBufSize);

        /*! \brief Get the datatype in which a given metadata property is stored in the AxF file

        \param hAxFMetadataDocument Handle to metadata document
        \param iProperty Index of the property
        \return Datatype of the property, which may be any value of enum axf::decoding::PropertyType. If the property in the AxF file has a type not supported by the current SDK version, TYPE_ERROR is returned.

        Note: The destinction between types TYPE_STRING and TYPE_UTF_STRING for string properties stored in the AxF file exists for legacy reasons. Nonetheless, both string types can be retrieved in the same way (in UTF-16/UTF-32 encoding), see axfGetMetadataPropertyValue().
        */
        int AXF_API axfGetMetadataPropertyType(AXF_METADATA_DOCUMENT_HANDLE hAxFMetadataDocument, int iProperty);

        /*! \brief Returns the length (in number of elements) of the given metadata property value

        \param hAxFMetadataDocument Handle to metadata document
        \param iProperty Index of the property
        \return Number of elements if the property has an array type, 1 otherwise. For TYPE_UTF_STRING the number of UTF characters *including* the terminating null character is returned.
        */
        int AXF_API axfGetMetadataPropertyValueLen(AXF_METADATA_DOCUMENT_HANDLE hAxFMetadataDocument, int iProperty);


        /*! \brief Returns the property value into a user provided buffer

        \param hAxFMetadataDocument Handle to metadata document
        \param iProperty Index of the property
        \param iType The data type (from enum axf::decoding::PropertyType) in which the property value should be retrieved.
        \param pBuf Pointer to memory buffer big enough to hold the property value data
        \param iBufSize size of the provided memory buffer, must be (at least) axfGetMetadataPropertyValueLen() * sizeof(SCALAR_TYPE), with sizeof(SCALAR_TYPE)=1..4 dependent on iType (cf. documentation for enum axf::decoding::PropertyType)
        \return *true*, if data has been successfully copied

        In general, property values should be read in the type in which they are stored in the AxF file, i.e. iType should be the type value returned by axfGetMetadataPropertyType().
        Otherwise, this function tries to convert the data to the requested data type. This is supported for certain combinations of storage type and retrieval type, including:
        - Bool values (TYPE_BOOLEAN) may optionally be retrieved as int values (TYPE_INT).
        - Half values (TYPE_HALF) or half arrays (TYPE_HALF_ARRAY) may optionally be retrieved as float values (TYPE_FLOAT) or float arrays (TYPE_FLOAT_ARRAY) respectively.
        - Strings stored in Latin-1 encoding (TYPE_STRING) may optionally be retrieved as strings in UTF-16 or UTF-32 encoding (TYPE_UTF_STRING), but not vice versa.
          Note that in this case the number of UTF-16 or UTF-32 charachters equals the number of Latin-1 characters (as returned by axfGetMetadataPropertyValueLen()),
          i.e. axfGetMetadataPropertyValueLen() * sizeof(wchar_t) is in fact the correct buffer size when retrieving TYPE_STRING properties as TYPE_UTF_STRING.
        */
        bool AXF_API axfGetMetadataPropertyValue(AXF_METADATA_DOCUMENT_HANDLE hAxFMetadataDocument, int iProperty, int iType, void* pBuf, int iBufSize);

        ///@}

        /** @name AxF Representations

        The interface provided here offers basic access functionality required to enumerate and select the available representations.
        Higher level access is provided using the decoder functionality from classes CPUDecoder and TextureDecoder.
        */
        ///@{
        /*! \brief Get the number of representations stored for an AxF material

            \param hAxFMaterial Handle to an AxF material
            \return Number of representations associated with the material

            An AxF material can be represented by one or multiple representations.
            If a material has multiple representations, these can be considered different-quality encodings of the same material, which may differ in terms of memory consumption, rendering efficiency, etc.
            [Think of a document stored as pure ASCII text or as HTML containing the same text (with the same meaning) but with different annotations and detail.]

            Each representation is associated with a version number (see axfGetRepresentationVersion()), which corresponds to the lowest AxF SDK version that can decode this representation.
            To ensure upwards compatibility of this SDK version with AxF files generated by newer encoders, this SDK supports reading of AxF files that contain not yet supported representation versions,
            as long as there is at least one supported representation for some material in the file.
            Since all existing representations in the file can be queried via this interface, you need to make sure that you pick a representation that is actually supported by the given SDK version
            (see axfIsRepresentationSupported()).

            Additionally, the capabilities of the third-party application might need to be considered (if the application implements an own renderer rather than just using the CPUDecoder class from this SDK for rendering).
            For that purpose the function axfCheckRepresentationCompatibilityProfile() might be used for finding a suitable representation.

            In any case, an automatic selection of the most suitable representation should in general be preferred over a manual user selection.
        */
        int AXF_API axfGetNumberOfRepresentations( AXF_MATERIAL_HANDLE hAxFMaterial );
        /*! \brief Get the number of representations stored for an AxF material (deprecated)

            \param hAxFFile Handle to open AxF File
            \param sMaterialID ID string of the material (cf. axfGetMaterialIDString()).
            \return Number of representations associated with the material

            DEPRECATED variant of the function: equivalent to axfGetNumberOfRepresentations(axfFindMaterialByIDString(hAxFFile, sMaterialID))
        */
        int AXF_API axfGetNumberOfRepresentations( AXF_FILE_HANDLE hAxFFile, const char* sMaterialID );

        /*! \brief Return a handle to a specific representation of an AxF material

            \param hAxFMaterial Handle to an AxF material
            \param iRepresentation Index of the representation in the material's representation list (in range 0 .. axfGetNumberOfMaterials()-1)
            \return Handle to found representation (or zero)
        */
        AXF_REPRESENTATION_HANDLE AXF_API axfGetRepresentation( AXF_MATERIAL_HANDLE hAxFMaterial, int iRepresentation );
        /*! \brief Return a handle to a specific representation of an AxF material (deprecated)

            \param hAxFFile Handle to open AxF file
            \param sMaterialID ID string of the material (cf. axfGetMaterialIDString()).
            \param iRepresentation Index of the representation in the material's representation list (in range 0 .. axfGetNumberOfMaterials()-1)
            \return Handle to found representation (or zero)

            DEPRECATED variant of the function: equivalent to axfGetRepresentation(axfFindMaterialByIDString(hAxFFile, sMaterialID), iRepresentation)
        */
        AXF_REPRESENTATION_HANDLE AXF_API axfGetRepresentation( AXF_FILE_HANDLE hAxFFile, const char* sMaterialID, int iRepresentation );

        /*! \brief Return a handle to the preferred supported representation of an AxF material

            \param hAxFMaterial Handle to an AxF material
            \return Handle to found representation (or zero)

            For a given material, this function tries to choose the representation that provides the best quality from all available representations that are supported by the given SDK version.
            This implies that the returned representation, if any, will satisfy axfIsRepresentationSupported().
            (However, it might be the case that the given material has no supported representation, in which case 0 is returned.)

            This function is primarily intended for the case that the given third-party application can actually handle *all* representations that are supported by this SDK (for instance via the use of CPUDecoder).
            If this is not the case, the application might need to explicitly enumerate the representations instead and choose the best one from those which it supports (for instance via the use of
            axfCheckRepresentationCompatibilityProfile()).
        */
        AXF_REPRESENTATION_HANDLE AXF_API axfGetPreferredRepresentation( AXF_MATERIAL_HANDLE hAxFMaterial );
        /*! \brief Return a handle to the preferred supported representation of an AxF material

            \param hAxFFile Handle to open AxF file
            \param sMaterialID ID string of the material (cf. axfGetMaterialIDString()). Optional for this function (see full text).
            \return Handle to found representation (or zero)

            This variant of the function is DEPRECATED, except for the following convenience use case:

            If no material ID string is given (i.e. if sMaterialID==0), the designated default material is chosen (see axfGetDefaultMaterial()) if existing,
            otherwise the first material that has at least one supported representation (if existing). (This again assumes that the application can handle all representations that are supported by this SDK.)
        */
        AXF_REPRESENTATION_HANDLE AXF_API axfGetPreferredRepresentation( AXF_FILE_HANDLE hAxFFile, const char* sMaterialID = 0 );

        /*! \brief Retrieve the *RepresentationClass*, which classifies the representation as SVBRDF, CarPaint, etc.
        \param hAxFRepresentation Valid handle to a representation root (as returned by axfGetRepresentation() or axfGetPreferredRepresentation())
        \param sBuf Character buffer big enough to hold the RepresentationClass string
        \param iBufSize size of sBuf (AXF_MAX_KEY_SIZE is sufficient)
        \return true if successful

        Classifies the given representation as one of the following categories (by considering the full hierarchical representation rooted at the given representation handle):
        - AXF_REPRESENTATION_CLASS_SVBRDF ("SVBRDF"): Spatially Varying Bidirectional Reflectance Distribution Function
        - AXF_REPRESENTATION_CLASS_CARPAINT ("CarPaint"): Hybrid carpaint model (deprecated)
        - AXF_REPRESENTATION_CLASS_CARPAINT2 ("CarPaint2"): New improved carpaint model (AxF version >= 1.2)
        - AXF_REPRESENTATION_CLASS_FACTORIZED_BTF ("FactorizedBTF"): Factorized Bidirectional Texture Function
        - AXF_REPRESENTATION_CLASS_LAYERED ("Layered"): A layering of multiple sub-materials
        */
        bool AXF_API axfGetRepresentationClass(AXF_REPRESENTATION_HANDLE hAxFRepresentation, char* sBuf, int iBufSize);

        /*! \brief Retrieve the *TypeKey* from the given representation node
        \param hAxFRepresentation Valid handle to a representation (as returned by axfGetRepresentation() or axfGetSvbrdfDiffuseModelRepresentation() etc.)
        \param sBuf Character buffer big enough to hold the TypeKey
        \param iBufSize size of sBuf (AXF_MAX_KEY_SIZE is sufficient)
        \return true if successful

        An AxF representation consists of a hierarchy of AxF nodes (describing the representation), each of which are uniquely identified by their AxF *TypeKey*.
        This method returns the TypeKey of the AxF node corresponding to the given representation handle.
        Note that a representation handle returned by axfGetRepresentation() or axfGetPreferredRepresentation() corresponds to the root node of such an hierarchical representation description,
        while a representation handle returned by axfGetSvbrdfDiffuseModelRepresentation() and axfGetSvbrdfSpecularModelRepresentation() corresponds to a sub-node in the hierarchical representation description.
        
        Note that (at least since AxF version 1.1) the TypeKey of the representation's root node can in general not be used to classify the whole hierarchical representation.
        For that purpose, axfGetRepresentationClass() should be used instead.

        However, dependent of the RepresentationClass returned by axfGetRepresentationClass(), the following TypeKeys can be useful to discriminate the kind of representation in more detail:
        - For RepresentationClass *SVBRDF*, the TypeKey for the representation handle returned by axfGetSvbrdfDiffuseModelRepresentation():
            - AXF_TYPEKEY_SVBRDF_DIFFUSE_LAMBERT ("com.xrite.LambertDiffuseModel")
            - AXF_TYPEKEY_SVBRDF_DIFFUSE_ORENNAYAR ("com.xrite.OrenNayarDiffuseModel")
        - For RepresentationClass *SVBRDF*, the TypeKey for the representation handle returned by axfGetSvbrdfSpecularModelRepresentation():
            - AXF_TYPEKEY_SVBRDF_SPECULAR_WARD ("com.xrite.WardSpecularModel")
            - AXF_TYPEKEY_SVBRDF_SPECULAR_BLINNPHONG ("com.xrite.BlinnPhongSpecularModel")
            - AXF_TYPEKEY_SVBRDF_SPECULAR_COOKTORRANCE ("com.xrite.CookTorranceSpecularModel")
            - AXF_TYPEKEY_SVBRDF_SPECULAR_GGX ("com.xrite.GGXSpecularModel")
            - AXF_TYPEKEY_SVBRDF_SPECULAR_PHONG ("com.xrite.PhongSpecularModel")  \warning inofficial
        - For RepresentationClass *FactorizedBTF*, the TypeKey of the representation's root node:
            - AXF_TYPEKEY_FACTORIZED_BTF_DFMF ("com.xrite.Dfmf"): Full Matrix Factorization
            - AXF_TYPEKEY_FACTORIZED_BTF_DPVF ("com.xrite.Dpvf"): Per-View Factorization

        For additional information on the SVBRDF-related TypeKeys, see axfGetSvbrdfDiffuseModelRepresentation() and axfGetSvbrdfSpecularModelRepresentation().
        */
        bool AXF_API axfGetRepresentationTypeKey( AXF_REPRESENTATION_HANDLE hAxFRepresentation, char* sBuf, int iBufSize );


        /*! \brief Retrieve a string specifying a variant of a given representation
        \param hAxFRepresentation Valid handle to a representation
        \param sVariantStringBuf Character buffer big enough to hold the variant's identifying string
        \param iBufSize size of sVariantStringBuf (AXF_MAX_KEY_SIZE is sufficient)
        \return true if the representation consists of the returned variant, false if no variant is defined

        Only the following variants of the given representations are currently part of the AxF definition:
        - For RepresentationClass *FactorizedBTF* (TypeKey *com.xrite.Dfmf* or *com.xrite.Dpvf*):
            - AXF_FACTORIZED_BTF_REPRESENTATION_VARIANT_DEFAULT (""): YUV decorrelation
            - AXF_FACTORIZED_BTF_REPRESENTATION_VARIANT_SQRTY ("SqrtY")

        For additional information on AxF representations cf. \ref index.
        */
        bool AXF_API axfGetRepresentationVariant( AXF_REPRESENTATION_HANDLE hAxFRepresentation, char* sVariantStringBuf, int iBufSize );


        /*! \brief Retrieve the representation's version number
        \param hAxFRepresentation Valid handle to a representation
        \param iMajor Major version number
        \param iMinor Minor version number
        \param iRevision Revision number
        \return true if successfull

        Returns the representation version for the given representation handle, i.e. the lowest AxF version that supports all features utilized by the given representation.
        */
        bool AXF_API axfGetRepresentationVersion( AXF_REPRESENTATION_HANDLE hAxFRepresentation, int& iMajor, int& iMinor, int& iRevision );

        /*! \brief Retrieve the SDK's highest supported representation version
        \param iMajor Major version number
        \param iMinor Minor version number

        Returns the highest representation version supported by the given SDK (constant that corresponds to the major.minor version of the dynamic library linked against).
        */
        void AXF_API axfGetHighestSupportedRepresentationVersion( int& iMajor, int& iMinor );

        /*! \brief Check if representation is supported by the given SDK
        \param hAxFRepresentation Valid handle to a representation
        \return true if the representation class is supported and the representation version does not exceed the SDK's highest supported representation version

        The purpose of this function is to check whether the given representation is supported by given SDK version (more precisely whether a CPUDecoder or TextureDecoder
        can be created from it using the given SDK). This is not necessarily sufficient to determine whether the given representation is to be supported by a particular
        third-party application. For that, see axfCheckRepresentationCompatibilityProfile().

        This is a convenience function which is equivalent to comparing the result of axfGetRepresentationClass() against the defined representation class strings
        and the result of axfGetRepresentationVersion() against axfGetHighestSupportedRepresentationVersion().
        */
        bool AXF_API axfIsRepresentationSupported( AXF_REPRESENTATION_HANDLE hAxFRepresentation );


        /*! \brief Check if representation fits into the given compatibility profile
        \param hAxFRepresentation Valid handle to a representation
        \param sCompatibilityProfile The compatibility profile string
        \param iVersion Version of the given compatibility profile
        \return true if representation matches the given compatibility profile

        A representation matches a certain compatibility profile if it uses a certain subset of features only (besides having a supported representation version).
        This is intended for ensuring compatibility to third-party renderers (or other third-party applications). The basic idea is that at file creation, the user may enforce
        that representations for a certain compatibility profile will be included in the AxF file. If the third-party application is known to support all features of that
        compatibility profile, it is assured that it will be able to pick a representation that it can handle. For this use case, an application can enumerate all representations,
        while making corresponding calls to this function to see which representations match the application's supported profile(s).

        The following compatibility profiles (specified by profile string and profile version) are defined:

        Profile strings:
        - AXF_COMPAT_PROF_SVBRDF ("AxFSvbrdf")
        - AXF_COMPAT_PROF_SVBRDF_REFRACT ("AxFSvbrdfRefract")
        - AXF_COMPAT_PROF_CARPAINT ("AxFCarPaint")
        - AXF_COMPAT_PROF_CARPAINT_REFRACT ("AxFCarPaintRefract")
        - AXF_COMPAT_PROF_BTF ("AxFBtf")
        - AXF_COMPAT_PROF_BASELINE_SVBRDF ("AxFBaselineFixedSvbrdf") - *deprecated*
        - AXF_COMPAT_PROF_BASELINE_BTF ("AxFBaselineFixedBtf") - *deprecated*

        The following table lists the representation classes, variants, and features for each defined profile.
        (The deprecated profiles shouldn't be used in new code and are thus omitted from the table.)

        <table>
        <tr>
        <th rowspan="4"><br><br>Profile<br>string<br></th>
        <th rowspan="4"><br><br>Profile<br>version</th>
        <th colspan="14">SVBRDF</th>
        <th colspan="4">FactorizedBTF</th>
        <th colspan="4">CarPaint2</th>
        </tr>
        <tr>
        <td colspan="2" align="center">Diffuse Model</td>
        <td colspan="8" align="center">Specular Model</td>
        <td rowspan="1" colspan="2" align="center">Clearcoat Layer</td>
        <td rowspan="1" colspan="2" align="center">Auxiliary Maps</td>
        <td rowspan="2" colspan="2" align="center">com.xrite.Dfmf</td>
        <td rowspan="2" colspan="2" align="center">com.xrite.Dpvf</td>
        <td rowspan="2" colspan="2"></td>
        <td rowspan="1" colspan="2" align="center">Clearcoat Layer</td>
        </tr>
        <tr>
        <td rowspan="2">Lambert</td>
        <td rowspan="2">Oren-<br/>Nayar</td>
        <td colspan="3" align="center">Ward (Iso-/Anisotropic)</td>
        <td colspan="2" align="center">Blinn-Phong (Iso-/Anisotropic)</td>
        <td rowspan="2">Cook-<br/>Torrance</td>
        <td rowspan="2">GGX</td>
        <td rowspan="2" align="center">Fresnel Term<br/>(Schlick1994)</td>
        <td rowspan="1" colspan="2" align="center">Refraction</td>
        <td rowspan="2" align="center">Alpha<br/>(Opacity)</td>
        <td rowspan="2" align="center">Height<br/>(Displacement)</td>
        <td rowspan="1" colspan="2" align="center">Refraction</td>
        </tr>
        <tr>
        <td>GM2010</td>
        <td>Duer2006</td>
        <td>Ward1992</td>
        <td>Ashikhmin2000</td>
        <td>Blinn1977</td>
        <td>Yes</td>
        <td>No</td>
        <td>Yuv (Default)</td>
        <td>SqrtY</td>
        <td>Yuv (Default)</td>
        <td>SqrtY</td>
        <td>Brdf</td>
        <td>Flakes</td>
        <td>Yes</td>
        <td>No</td>
        </tr>
        <tr>
        <td>AxFSvbrdf</td>
        <td>1</td>
        <td>x</td>
        <td></td>
        <td>x</td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td>x</td>
        <td></td>
        <td>x</td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        </tr>
        <tr>
        <td>AxFSvbrdf</td>
        <td>2</td>
        <td>x</td>
        <td></td>
        <td>x</td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td>x</td>
        <td></td>
        <td>x</td>
        <td>x</td>
        <td>x</td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        </tr>
        <tr>
        <td>AxFSvbrdf</td>
        <td>3</td>
        <td>x</td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td>x</td>
        <td>x</td>
        <td></td>
        <td>x</td>
        <td>x</td>
        <td>x</td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        </tr>
        <tr>
        <td>AxFSvbrdfRefract</td>
        <td>1</td>
        <td>x</td>
        <td></td>
        <td>x</td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td>x</td>
        <td>x</td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        </tr>
        <tr>
        <td>AxFSvbrdfRefract</td>
        <td>2</td>
        <td>x</td>
        <td></td>
        <td>x</td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td>x</td>
        <td>x</td>
        <td></td>
        <td>x</td>
        <td>x</td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        </tr>
        <tr>
        <td>AxFSvbrdfRefract</td>
        <td>3</td>
        <td>x</td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td>x</td>
        <td>x</td>
        <td>x</td>
        <td></td>
        <td>x</td>
        <td>x</td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        </tr>
        <tr>
        <td>AxFCarPaint</td>
        <td>1</td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td>x</td>
        <td>x*</td>
        <td></td>
        <td>x</td>
        </tr>
        <tr>
        <td>AxFCarPaintRefract</td>
        <td>1</td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td>x</td>
        <td>x*</td>
        <td>x</td>
        <td></td>
        </tr>
        <tr>
        <td>AxFBtf</td>
        <td>1</td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td>x</td>
        <td>x</td>
        <td>x</td>
        <td>x</td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        </tr>
        </table>

        (*) Flakes are an optional feature for third-party renderers: an application does not need to render them to be compliant with the profile.

        Compatibility profiles are always inclusive which means all features of a matching representation must be part of the profile but not all features of the
        profile need to be present in the representation. 

        Note that for SVBRDF and car paint, there are two profiles each, which match only representations with refractive or non-refractive clear coat layer respectively,
        for the case that an application supports only one of these two clear coat variants. (Otherwise the application should check for both of these profile variants.)
        Representations that do not include a clear coat layer implicitly match both of these profile variants.

        Further note that only *CarPaint2* representations match the profiles AXF_COMPAT_PROF_CARPAINT and AXF_COMPAT_PROF_CARPAINT_REFRACT,
        not representations for the old, deprecated *CarPaint* representation class (see \ref carpaint_sec02).

        If this function returns true, this also implies axfIsRepresentationSupported().

        If the selected representation is not compatible to the application's supported compatibility profile an appropriate message like

            No AxF representation compatible with <YourApplication> has been found in the AxF file <FileName>

        should be presented to the user.

        <!--
        Deprecated profiles:
        <table>
        <tr>
        <th rowspan="4"><br><br>Profile<br>string<br></th>
        <th rowspan="4"><br><br>Profile<br>version</th>
        <th rowspan="4"><br><br>SDK<br>version</th>
        <th colspan="11">SVBRDF</th>
        <th colspan="4">FactorizedBTF</th>
        <th colspan="2">CarPaint</th>
        <th colspan="4">CarPaint2</th>
        </tr>
        <tr>
        <td colspan="2" align="center">Diffuse Model</td>
        <td colspan="7" align="center">Specular Model</td>
        <td rowspan="1" colspan="2" align="center">+Clearcoat<br>Layer</td>
        <td rowspan="2" colspan="2" align="center">com.xrite.Dfmf</td>
        <td rowspan="2" colspan="2" align="center">com.xrite.Dpvf</td>
        <td rowspan="2" colspan="2"></td>
        <td rowspan="2" colspan="2"></td>
        <td rowspan="1" colspan="2" align="center">+Clearcoat<br/>Layer</td>
        </tr>
        <tr>
        <td rowspan="2">Lambert</td>
        <td rowspan="2">Oren-<br/>Nayar</td>
        <td colspan="3">Ward (+Anisotropic)</td>
        <td colspan="2">Blinn-Phong (+Anisotropic)</td>
        <td rowspan="2">Cook-<br/>Torrance</td>
        <td rowspan="2">+Fresnel<br/>Term</td>
        <td rowspan="1" colspan="2">Refraction</td>
        <td rowspan="1" colspan="2">Refraction</td>
        </tr>
        <tr>
        <td>GM2010</td>
        <td>Duer2006</td>
        <td>Ward1992</td>
        <td>Ashikhmin2000</td>
        <td>Blinn1977</td>
        <td>Yes</td>
        <td>No</td>
        <td>Yuv (Default)</td>
        <td>SqrtY</td>
        <td>Yuv (Default)</td>
        <td>SqrtY</td>
        <td>Brdf</td>
        <td>Flakes</td>
        <td>Brdf</td>
        <td>Flakes</td>
        <td>Yes</td>
        <td>No</td>
        </tr>
        <tr>
        <td>AxFBaselineFixedSvbrdf</td>
        <td>1</td>
        <td>1.0</td>
        <td>x</td>
        <td></td>
        <td>x</td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        </tr>
        <tr>
        <td>AxFBaselineFixedBtf</td>
        <td>1</td>
        <td>1.0</td>
        <td>x</td>
        <td></td>
        <td>x</td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td>x</td>
        <td>x</td>
        <td>x</td>
        <td>x</td>
        <td>x</td>
        <td>x</td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        </tr>
        <tr>
        <td>AxFBaselineFixedSvbrdf</td>
        <td>2</td>
        <td>1.1</td>
        <td>x</td>
        <td></td>
        <td>x</td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td>x</td>
        <td>x</td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        </tr>
        <tr>
        <td>AxFBaselineFixedBtf</td>
        <td>2</td>
        <td>1.2</td>
        <td>x</td>
        <td></td>
        <td>x</td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td></td>
        <td>x</td>
        <td>x</td>
        <td></td>
        <td>x</td>
        <td>x</td>
        <td>x</td>
        <td>x</td>
        <td></td>
        <td></td>
        <td>x</td>
        <td>x</td>
        <td>x</td>
        <td></td>
        </tr>
        </table> -->

        */
        bool AXF_API axfCheckRepresentationCompatibilityProfile( AXF_REPRESENTATION_HANDLE hAxFRepresentation, const char* sCompatibilityProfile, int iVersion );

        /*! \brief Given an AxF representation (of representation class *CarPaint* or *CarPaint2*), retrieve the child representation for the flakes

        \param hAxFCarPaintRepresentation Valid handle to an AxF representation
        \return Handle to the flakes BTF child representation if existing, 0 otherwise (Note that flakes are an optional component for CarPaint / CarPaint2 representations.)
        */
        AXF_REPRESENTATION_HANDLE AXF_API axfGetCarPaintFlakesBtfRepresentation( AXF_REPRESENTATION_HANDLE hAxFCarPaintRepresentation );

        /*! \brief Given an AxF representation (of representation class *CarPaint* or *CarPaint2*), retrieve the child representation for the tabulated BRDF component

        \param hAxFCarPaintRepresentation Valid handle to an AxF representation
        \return Handle to the tabulated BRDF child representation if existing, 0 otherwise
        */
        AXF_REPRESENTATION_HANDLE AXF_API axfGetCarPaintTabulatedBrdfRepresentation( AXF_REPRESENTATION_HANDLE hAxFCarPaintRepresentation );

        /*! \brief Given an AxF representation (of representation class *SVBRDF*, *CarPaint*, or *CarPaint2*), retrieve the child representation for the diffuse component of the BRDF model

        \param hAxFSvbrdfRepresentation Valid handle to an AxF representation
        \return Handle to the diffuse model child representation if existing, 0 otherwise

        The SVBRDF representation as implemented within AxF supports different models for the diffuse component. The following representation TypeKeys are part of the current AxF definition
        and supported by the AxF SDK (cf. axfGetRepresentationTypeKey()):
        - *com.xrite.LambertDiffuseModel*: classic Lambertian diffuse model
        - *com.xrite.OrenNayarDiffuseModel*: not implemented yet

        More details can be found at \ref page1.
        */
        AXF_REPRESENTATION_HANDLE AXF_API axfGetSvbrdfDiffuseModelRepresentation( AXF_REPRESENTATION_HANDLE hAxFSvbrdfRepresentation );

        /*! \brief Given an AxF representation (of representation class *SVBRDF*, *CarPaint*, or *CarPaint2*), retrieve the child representation for the specular component of the BRDF model

        \param hAxFSvbrdfRepresentation Valid handle to an AxF representation
        \return Handle to the specular model child representation if existing, 0 otherwise

        The SVBRDF representation as implemented within AxF supports different models for the specular component. The following representation TypeKeys are part of the current AxF definition
        and supported by the AxF SDK (cf. axfGetRepresentationTypeKey()):
        - *com.xrite.WardSpecularModel*: Ward model
        - *com.xrite.BlinnPhongSpecularModel*: Blinn-Phong model (classic isotropic version as well as anisotropic version as proposed by Ashikhmin-Shirley)
        - *com.xrite.CookTorranceSpecularModel*: Cook-Torrance model
        - *com.xrite.PhongSpecularModel*: classic Phong model  \warning inofficial

        More details can be found at \ref page1.
        */
        AXF_REPRESENTATION_HANDLE AXF_API axfGetSvbrdfSpecularModelRepresentation( AXF_REPRESENTATION_HANDLE hAxFSvbrdfRepresentation );

        /*! \brief Retrieve a string and a few flags identifying the variant of the specular model represented in the AxF file.
        \param hAxFSpecularModelRepresentation Valid handle to an AxF specular model representation (cf. axfGetSvbrdfSpecularModelRepresentation())
        \param sVariantStringBuf Character buffer big enough to hold the variant's identifying string
        \param iBufSize size of sVariantStringBuf (AXF_MAX_KEY_SIZE is sufficient)
        \param bIsAnisotropic Is it the anisotropic variant of the model?
        \param bHasFresnel Does the model include a Fresnel term?
        \return True, if successful.

        The following variants are currently part of the AxF definition:
        - *com.xrite.WardSpecularModel*
            - AXF_SVBRDF_SPECULAR_WARD_VARIANT_GEISLERMORODER ("GeislerMoroder2010")
            - AXF_SVBRDF_SPECULAR_WARD_VARIANT_DUER ("Duer2006")
            - AXF_SVBRDF_SPECULAR_WARD_VARIANT_WARD ("Ward1992")
        - *com.xrite.BlinnPhongSpecularModel*
            - AXF_SVBRDF_SPECULAR_BLINN_VARIANT_ASHIKHMIN_SHIRLEY ("Ashikhmin2000")
            - AXF_SVBRDF_SPECULAR_BLINN_VARIANT_BLINN ("Blinn1977")
            - AXF_SVBRDF_SPECULAR_BLINN_VARIANT_VRAY ("VRay")  \warning inoffical
            - AXF_SVBRDF_SPECULAR_BLINN_VARIANT_LEWIS ("Lewis1993")  \warning inoffical
        - *com.xrite.CookTorranceSpecularModel*
            - no variants defined yet
        - *com.xrite.GGXSpecularModel*
            - no variants defined yet
        */
        bool AXF_API axfGetSvbrdfSpecularModelVariant( AXF_REPRESENTATION_HANDLE hAxFSpecularModelRepresentation, char* sVariantStringBuf, int iBufSize, bool& bIsAnisotropic, bool& bHasFresnel );


        /*! \brief Retrieve a string identifying the variant of the used Fresnel approximation.
        \param hAxFSpecularModelRepresentation Valid handle to an AxF specular model representation (cf. axfGetSvbrdfSpecularModelRepresentation()) that has a Fresnel term (cf. axfGetSvbrdfSpecularModelVariant())
        \param sVariantStringBuf Character buffer big enough to hold the variant's identifying string
        \param iBufSize size of sVariantStringBuf (AXF_MAX_KEY_SIZE is sufficient)
        \return True, if successful.

        The following variants are currently part of the AxF definition:
        - AXF_SVBRDF_FRESNEL_VARIANT_SCHLICK ("Schlick1994")
        - AXF_SVBRDF_FRESNEL_VARIANT_FRESNEL ("Fresnel1818")  \warning inoffical
            */
        bool AXF_API axfGetSvbrdfSpecularFresnelVariant(AXF_REPRESENTATION_HANDLE hAxFSpecularModelRepresentation, char* sVariantStringBuf, int iBufSize );

        ///@}

        ///@{

        /** @name AxF representation resources

        All data that is used by an AxF representation during evaluation i.e. rendering is called a *resource*.
        Typically resources are model parameters like gloss exponents or measurements like intensities, colors, spectra etc. and are thus stored as x-D arrays 
        of floating point values.

        AxF stores all resources for a material within a material resource collection that is shared by all representations of the material.
        Each representation *references* some (or all) resources from that material resource collection via a so-called *lookup operator*.
        The name of this lookup operator defines the *semantic* of the respective resource for the given representation, i.e. which model parameter of the given representation it represents.
        */
        
        /*!  \brief Retrieve the number of (unique) resources that are referenced by a representation (or its sub-representations)

        \param hAxFRepresentation Valid handle to representation
        \return number of unique resources referenced by the representation
        */
        int AXF_API axfGetNumberOfRepresentationResources(AXF_REPRESENTATION_HANDLE hAxFRepresentation);

        /*!  \brief Return handle to a representation's resource by index

        \param hAxFRepresentation Valid handle to representation
        \param iResource index of resource within representation
        \return handle to AxF resource

        This can be used to enumerate all resources that are referenced by a representation (or its sub-representations).
        */
        AXF_RESOURCE_HANDLE AXF_API axfGetRepresentationResourceFromIndex(AXF_REPRESENTATION_HANDLE hAxFRepresentation, int iResource);

        /*!  \brief Return handle to a representation's resource from a "lookup path"

        \param hAxFRepresentation Valid handle to representation
        \param sLookupPath Lookup path (cf. axfGetResourceLookupPath())
        \return handle to AxF resource

        Given a representation and a lookup operator - specified by a "lookup path" -, return the resource that this lookup operator references.
        (The "lookup path" corresponds to the AxF node path of the respective lookup operator node relative to the representation node,
        optionally followed by "[index]" for array-like lookup operators that reference multiple resources.)
        */
        AXF_RESOURCE_HANDLE AXF_API axfGetRepresentationResourceFromLookupPath(AXF_REPRESENTATION_HANDLE hAxFRepresentation, const char* sLookupPath);

        /*!  \brief Return handle to a representation's resource from a "lookup name"

        \param hAxFRepresentation Valid handle to representation
        \param sLookupName Lookup name (last component of the lookup path)
        \param bSearchRecursive If true all representation's child nodes are searched for a lookup of the given name. Please note that the first matching node is returned in this case. 
        \return handle to AxF resource

        Given a representation and a lookup operator - specified by a "lookup name" -, return the resource that this lookup operator references.
        The "lookup name" corresponds to the last component of the "lookup path" (cf. axfGetRepresentationResourceFromLookupPath()).

        With bSearchRecursive = false, this is equivalent to calling axfGetRepresentationResourceFromLookupPath() with lookup path "com.xrite.Resources/<lookup name>".
        With bSearchRecursive = true, this is equivalent to enumerating all resources referenced by the representation or its sub-representations (cf. axfGetRepresentationResourceFromIndex())
        and returning the first one whose lookup path ends in ".../<lookup name>" (cf. axfGetResourceLookupPath()).
        */
        AXF_RESOURCE_HANDLE AXF_API axfGetRepresentationResourceFromLookupName(AXF_REPRESENTATION_HANDLE hAxFRepresentation, const char* sLookupName, bool bSearchRecursive = true);

        /*!  \brief Retrieve a resource's "lookup path"

        \param hAxFRepresentation Valid handle to representation
        \param hAxFResource Valid handle to resource
        \param sBuf Character buffer big enough to hold the path (or 0 to query the required buffer size only)
        \param iBufSize size of sBuf
        \return The buffer size that is needed to return the full string (number of characters including terminating 0). If iBufSize was at least this large, the full string was returned successfully, otherwise the returned string was truncated. 0 indicates an error (in which case no string is returned).

        The "lookup path" of a resource with respect to a given representation specifies the *semantic* of the respective resource for that representation,
        i.e. which model parameter of the given representation it represents.

        See axfGetRepresentationResourceFromLookupPath() for more details on the "lookup path". In particular, note that the path is relative to the given representation.

        If neither the given representation nor its sub-representations reference the given resource, 0 is returned.
        */        
        int AXF_API axfGetResourceLookupPath(AXF_REPRESENTATION_HANDLE hAxFRepresentation, AXF_RESOURCE_HANDLE hAxFResource, char* sBuf, int iBufSize);

        /*!  \brief Retrieve a resource's node path

        \param hAxFResource Valid handle to resource
        \param sBuf Character buffer big enough to hold the path (or 0 to query the required buffer size only)
        \param iBufSize size of sBuf
        \return The buffer size that is needed to return the full string (number of characters including terminating 0). If iBufSize was at least this large, the full string was returned successfully, otherwise the returned string was truncated. 0 indicates an error (in which case no string is returned).

        The resource's node path is defined by recursively concatenating the node names of the resource parent nodes in the AxF file until the material resource collection node is reached.

        Note that the node path is not fixed as per AxF specification, thus in general it is not suited for intentifying the semantic of a resource (with respect to a representation).
        For that purpose, axfGetResourceLookupPath() should be used instead.

        This function is mainly useful for diagnostic purposes.
        */        
        int AXF_API axfGetResourceNodePath(AXF_RESOURCE_HANDLE hAxFResource, char* sBuf, int iBufSize);

        /*!  \brief Retrieve the number of dimensions of the resource data 

        \param hAxFResource Valid handle to resource
        \return Number of dimensions of resource data

        Typical instances of resource data are either uniforms (1 dimension) or textures (> 1 dimensions).
        Note that this refers to the actual dimension of the multi-dimensional data array and is different from what is commonly considered the dimension of a texture.
        For instance, a "2D" texture parameterized by UV typically has 3 dimensions: V, U, and channel (where V is the outermost dimension when enrolled into in a C array as per axfGetResourceData()).
        */
        int AXF_API axfGetResourceDataNumDims(AXF_RESOURCE_HANDLE hAxFResource);

        /*!  \brief Retrieve the extent of a given dimension of resource data

        \param hAxFResource Valid handle to resource
        \param iDim the dimension's index
        \return Extent of dimension
        */
        int AXF_API axfGetResourceDataDimExtent(AXF_RESOURCE_HANDLE hAxFResource, int iDim);

        /*!  \brief Retrieve the total number of elements in the resource i.e. the enrolled extensions along each dimensions

        \param hAxFResource Valid handle to resource
        \return Total number of elements in the resource
        */
        int AXF_API axfGetResourceDataNumElems(AXF_RESOURCE_HANDLE hAxFResource);

        /*!  \brief Copy the resource data into the provided buffer

        \param hAxFResource Valid handle to resource
        \param pfBuffer float buffer big enough to hold the resource data
        \param iBufSize size of the given float buffer, must be (at least) axfGetResourceDataNumElems() * sizeof(float)

        \return Total number of elements in the resource on success (same as axfGetResourceDataNumElems()), 0 on failure
        */
        int AXF_API axfGetResourceData(AXF_RESOURCE_HANDLE hAxFResource, float* pfBuffer, int iBufSize);

        ///@}

        ///@{

        /** @name AxF representation preview images
            @anchor PreviewImages

            Access to preview images that are stored in the AxF file for a certain representation.
            Note that AxF preview images are evaluated SVBRDFs (for a certain geometry and lighting condition), i.e. a texture of linear reflectance values,
            premultiplied with <N,L>, though not yet multiplied with the light color (and not yet mapped to a non-linear display color space).
        */
        int AXF_API axfGetNumPreviewImages( AXF_REPRESENTATION_HANDLE hAxFRepresentation );
        int AXF_API axfGetPreviewImageName( AXF_REPRESENTATION_HANDLE hAxFRepresentation, int iImageIdx, char* sBuf, int iBufSize );   //< \return The buffer size that is needed to return the full string (number of characters including terminating 0). If iBufSize was at least this large, the full string was returned successfully, otherwise the returned string was truncated. 0 indicates an error (in which case no string is returned).

        /*! \brief Retrieve information about a representation preview image

            \param hAxFRepresentation Valid handle to a representation
            \param iImageIdx Index of the preview image to query (in range 0 .. axfGetNumPreviewImages()-1)
            \param iWidth Width of the stored preview image in pixels (output parameter)
            \param iHeight Height of the stored preview image in pixels (output parameter)
            \param iChannels Will be set to 4 if the stored preview image contains an alpha/opacity channel, 3 otherwise (output parameter)
            \param fWidthMM Spatial width of the stored preview image in millimeters (output parameter)
            \param fHeightMM Spatial height of the stored preview image in millimeters (output parameter)
            \return true if successful

            Note that the output value iChannel is provided for convenient usage of axfGetPreviewImage() only, see its description. iChannel does not necessarily coincide with the original number
            of channels of the stored preview image (in case it is stored in a non-trichromatic source color space), since the original number of channels is not relevant for usage of axfGetPreviewImage().
            Instead, iChannels can be used safely to distinguish whether the stored preview image contains an alpha/opacity channel (in which case it will be set to 4) or not (in which case it will be set to 3).
        */
        bool AXF_API axfGetPreviewImageInfo( AXF_REPRESENTATION_HANDLE hAxFRepresentation, int iImageIdx, int& iWidth, int& iHeight, int& iChannels, float& fWidthMM, float& fHeightMM );

        /*! \brief Returns a representation preview image into a user provided buffer
            
            \param hAxFRepresentation Valid handle to a representation
            \param iImageIdx Index of the preview image to query (in range 0 .. axfGetNumPreviewImages()-1)
            \param sTargetColorSpace Trichromatic target color space (see below)
            \param pImage pointer to a buffer of iWidth*iHeight*iChannels floats, in which to retrieve the preview image
            \param iWidth width of the preview image in pixels
            \param iHeight height of the preview image in pixels
            \param iChannels number of channels of the image buffer (3 or 4)
            \param iTextureOrigin cf. enum ETextureOrigin

            iWidth and iHeight must match the corresponding values returned by axfGetPreviewImageInfo().
            If you want to retrieve an alpha/opacity channel if and only if the stored preview image contains one, also choose iChannel to match the value returned by axfGetPreviewImageInfo(). Otherwise you
            may choose iChannel differently (see below).

            sTargetColorSpace must be one of:
            - AXF_COLORSPACE_CIE_1931_XYZ ("XYZ"):                            the CIE 1931 XYZ color space
            - AXF_COLORSPACE_LINEAR_SRGB_E ("sRGB,E"):                        a *linear* color space with primary chromaticities matching those of the sRGB color space (IEC 61966-2-1), but - similarly to CIE XYZ - with equal-energy white point [i.e. the unity reflectance spectrum is mapped to RGB values (1 1 1)]
            - AXF_COLORSPACE_LINEAR_ADOBE_RGB_E ("AdobeRGB,E"):               a *linear* color space with primary chromaticities matching those of the Adobe RGB (1988) color space,     but - similarly to CIE XYZ - with equal-energy white point [i.e. the unity reflectance spectrum is mapped to RGB values (1 1 1)]
            - AXF_COLORSPACE_LINEAR_ADOBE_WIDEGAMUT_RGB_E ("WideGamutRGB,E"): a *linear* color space with primary chromaticities matching those of the Adobe Wide-Gamut RGB color space, but - similarly to CIE XYZ - with equal-energy white point [i.e. the unity reflectance spectrum is mapped to RGB values (1 1 1)]
            - AXF_COLORSPACE_LINEAR_PROPHOTO_RGB_E ("ProPhotoRGB,E"):         a *linear* color space with primary chromaticities matching those of the ProPhoto RGB color space,         but - similarly to CIE XYZ - with equal-energy white point [i.e. the unity reflectance spectrum is mapped to RGB values (1 1 1)]

            The preview image from the AxF file is transformed to the specified target color space. Currently only trichromatic target color spaces are supported (irrespective of whether the image is stored
            in a trichromatic color space in the AxF file or not).

            Thus iChannels must be either 3 or 4:
               - For iChannels = 3, a 3-channel color image (without alpha) is returned. If the source preview image from the AxF file actually has an alpha/opacity channel, the SDK tries to convert it to a
                 non-transparent preview image by rendering it in front of a checkerboard background, which becomes "baked" into the resulting 3-channel image. Note that the latter is only guarantueed to work
                 correctly for the default planar preview image (named AXF_PREVIEW_IMAGE_NAME_DEFAULT).
               - For iChannels = 4, a 4-channel color/alpha image is returned (e.g. RGBA). No background is integrated into the resulting image. If the source preview image from the AxF file does not have an
                 alpha/opacity channel, a trivial alpha channel (1.0f) is added to the result image.
        */
        bool AXF_API axfGetPreviewImage( AXF_REPRESENTATION_HANDLE hAxFRepresentation, int iImageIdx, const char* sTargetColorSpace, float* pImage, int iWidth, int iHeight, int iChannels, int iTextureOrigin = ORIGIN_BOTTOMLEFT );

        //! DEPRECATED variant of the function: equivalent to axfGetPreviewImage(hAxFRepresentation, iImageIdx, sTargetColorSpace, pImage, iWidth, iHeight, iChannels, iTextureOrigin) with iWidth, iHeight, and iChannels as returned by axfGetPreviewImageInfo()
        bool AXF_API axfGetPreviewImage( AXF_REPRESENTATION_HANDLE hAxFRepresentation, int iImageIdx, const char* sTargetColorSpace, float* pImage, int iTextureOrigin = ORIGIN_BOTTOMLEFT );

        int AXF_API axfStorePreviewImage( AXF_REPRESENTATION_HANDLE hAxFRepresentation, const float* pImage, int iWidth, int iHeight, int iChannels, float fWidthMM, float fHeightMM,
            const char* sSourceColorSpace, int iTextureOrigin = ORIGIN_BOTTOMLEFT, const char* sName = AXF_PREVIEW_IMAGE_NAME_DEFAULT );   //< \return -1 on failure, axfGetNumPreviewImages(hAxFRepresentation) > 0 on success.
        ///@}

        /** @name AxF spectral information

            Retrieve spectral information about a representation.
        */
        /////@{

        /*!  \brief Get a material-dependent linear transformation from a (linear) source color space to the spectral space (for a given spectral sampling).

        \param hAxFRepresentation Valid handle to a representation
        \param vSpectralSampling Array of float values containing the spectral sample points in nm
        \param iNumSpectralSamples Number of spectral samples (number of elements in vSpectralSampling)
        \param pMatrix iNumSpectralSamplesx3 sized float buffer for holding the linear color transformation in row (column-) major order.
        \param sSourceColorSpace The color space to be used as source of the transformation in "Colspace,WP" notation (see CPUDecoder::create() or TextureDecoder::create()).
                                 Typically, the target color space of the respective decoder (CPUDecoder or TextureDecoder) is to be used here.
        \return True, if successful.

        This function returns a "spectralization" transformation which transform from the representation's source color space or a user defined color space to a given spectral sampling.
        The second option can be used e.g. if the application wants to transform the color value from the representations's source color space into another trichromatic working color space
        first
        This spectralization transformation has been optimized for the materials spectral properties and significantly improves upon a standard RGB-to-spectral
        conversion (e.g. \cite Smits99)).
        */
        bool AXF_API axfGetSpectralizationTrafo( AXF_REPRESENTATION_HANDLE hAxFRepresentation, const float* vSpectralSampling, int iNumSpectralSamples, float* pMatrix, const char* sSourceColorSpace );
        ///@}

AXF_DECODING_CLOSE_NAMESPACE
