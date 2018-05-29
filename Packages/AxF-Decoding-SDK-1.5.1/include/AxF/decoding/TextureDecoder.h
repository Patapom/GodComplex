///////////////////////////////////////////////////////////////////////////////
// File:		TextureDecoder.h
// Authors:		Gero Mueller, Alexander Gress
//
// Title:	External interface for decoding textures from a AxF representation.
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
#include "AxF_basic_io.h"

#ifdef PANTORA_EXTERNAL_DEPENDENCIES_ALLOWED
    namespace PANTORA { class VariantType; namespace utils { struct TextureStruct; } }
#endif


AXF_DECODING_OPEN_NAMESPACE

        //forward declarations
        class CPUDecoder;

        namespace detail { class TextureDecoderInterface; class PropertyMap; }  // private implementation

        //! Enum of different target systems supported by TextureDecoder
        /*!
        \warning Deprecated - use always ID_DEFAULT
        */
        enum ETargetSystem {
            ID_DEFAULT      = 0,            //!< The default system ID to be used all client applications unless indicated otherwise
            ID_VRED         = ID_DEFAULT    //!< Autodesk VRED
        };


        //TextureDecoder
        ///////////////////////////////////////////////
        /*!
        \brief Decodes texture resources from AxF representations.

        Interface for simple extraction of texture resources (and associated rendering semantic) from AxF representations, for instance in order to use them in a custom renderer.
        This is useful in particular when the usage of CPUDecoder for rendering is not feasible for a certain target application. This is especially true for GPU-based renderers
        but also for some CPU-based engines which rely on special techniques or data-structures optimized for parallelization.

        Basic usage:
        \code{.cpp}
             AXF_REPRESENTATION_HANDLE h_axf_rep = axfGetPreferredRepresentation( h_axf_file );
             CPUDecoder* pcl_decoder = CPUDecoder::create( h_axf_rep );
             ...
             TextureDecoder* pcl_tex_decoder = TextureDecoder::create( h_axf_rep, pcl_decoder, ORIGIN_TOPLEFT, ID_DEFAULT );
             ...
             if ( pcl_tex_decoder->getNumTextures() > 0) {
                  char s_tex_name[255];
                  pcl_tex_decoder->getTextureName( 0, s_tex_name, 255 );
                  if ( "diffuse" == s_tex_name ) {
                      int i_width, i_height, i_depth, i_channels, i_datatype_src;
                      pcl_tex_decoder->getTextureSize( 0, 0, i_width, i_height, i_depth, i_channels, i_datatype_src );
                      float* pcl_texture_buffer = createTextureBuffer<float>( i_width, i_height, i_depth, i_channels );
                      pcl_tex_decoder->getTextureData( 0, 0, TEXTURE_TYPE_FLOAT, pcl_texture_buffer )
                      ...
                  }
                  ...
              }
              ...
              pcl_tex_decoder->destroy();
        \endcode
        */
        class AXF_API TextureDecoder
        {
        public:
            //! For using shared_ptr<TextureDecoder>, pass this as second parameter to the shared_ptr constructor
            struct Deleter
            {
                inline void operator()( TextureDecoder* pclTextureDecoder )
                {
                    if ( pclTextureDecoder ) pclTextureDecoder->destroy();
                };
            };

            ///@{
            /**@name Dimensions
            */
            //! Returns the maximum width in pixels of all texture resources in this representation that are parameterized over the spatial U,V axes
            /*!
                \return Maximum width in pixels of all texture resources in this representation that are parameterized over the spatial U,V axes

                Note that the width of the individual textures (which can be queried using getTextureSize()) may vary from this maximum width.
                This enables SVBRDF representations to store different BRDF parameters in different resolutions, for instance in order to reduce the overall
                memory consumption of the representation (cf. \ref page4).

                A potential use case of this function (along with getWidthMM()) is for estimating the overall spatial resolution of the representation along the U axis.
            */
            int getWidthPixel() const;

            //! Returns the maximum height in pixels of all texture resources in this representation that are parameterized over the spatial U,V axes
            /*!
                \return Maximum height in pixels of all texture resources in this representation that are parameterized over the spatial U,V axes

                Note that the height of the individual textures (which can be queried using getTextureSize()) may vary from this maximum height.
                This enables SVBRDF representations to store different BRDF parameters in different resolutions, for instance in order to reduce the overall
                memory consumption of the representation (cf. \ref page4). 

                A potential use case of this function (along with getHeightMM()) is for estimating the overall spatial resolution of the representation along the V axis.
            */
            int getHeightPixel() const;

            //! Returns the (maximum) physical width in millimeters of all texture resources in this representation that are parameterized over the spatial U,V axes
            /*!
                \return Maximum physical width in millimeters of all texture resources in this representation that are parameterized over the spatial U,V axes

                Note that while (unedited) measured materials always have a unique physical size, which is shared by all (spatially parameterized) textures in
                the material representation, AxF nonetheless allows that the individual textures might have varying physical sizes (which can be queried using
                getTextureSizeMM()). In case of SVBRDF representations, the latter makes sense in context of tiled materials only (as it describes the physical
                size of a single tiled pattern for the respective resource, and thus indirectly its tiling frequency). While we don't encourage the creation of
                SVBRDF representations with varying physical sizes per resource in general, this feature was introduced to allow for more artistic freedom when
                hand-editing a material.
            */
            float getWidthMM() const;

            //! Returns the (maximum) physical height in millimeters of all texture resources in this representation that are parameterized over the spatial U,V axes
            /*!
                \return Maximum physical height in millimeters of all texture resources in this representation that are parameterized over the spatial U,V axes
                
                Note that while (unedited) measured materials always have a unique physical size, which is shared by all (spatially parameterized) textures in
                the material representation, AxF nonetheless allows that the individual textures might have varying physical sizes (which can be queried using
                getTextureSizeMM()). In case of SVBRDF representations, the latter makes sense in context of tiled materials only (as it describes the physical
                size of a single tiled pattern for the respective resource, and thus indirectly its tiling frequency). While we don't encourage the creation of
                SVBRDF representations with varying physical sizes per resource in general, this feature was introduced to allow for more artistic freedom when
                hand-editing a material.
            */
            float getHeightMM() const;
            ///@}

            ///@{
            /**@name Property interface
            These methods query and set parameters required to evaluate the representation correctly.
            These properties can be regarded as analogue to uniform variables in shader scripts. Type and value of these
            properties depend on the representation.
            */
            //! Returns number of properties.
            int getNumProperties() const;
            //! Retrieves the property value. The buffer size iSize must be (at least) getPropertySize(index), and iDataType must match getPropertyType(index).
            bool getProperty(int index, void* pBuf, int iDataType, int iSize) const;
            //! Returns the name, i.e. semantic of the given property. AXF_MAX_KEY_SIZE is sufficient for iBufSize.
            bool getPropertyName(int index, char* sBuf, int iBufSize) const;
            int getPropertyIndexFromName( const char* sBuf ) const;
            //! Returns the type of the given property (from enum axf::decoding::PropertyType). For the current SDK version, this is always one of: TYPE_INT, TYPE_INT_ARRAY, TYPE_FLOAT, TYPE_FLOAT_ARRAY.
            int getPropertyType(int index) const;
            //! Returns the size of the property value in *bytes*.
            int getPropertySize(int index) const;

            int getPropertyLen(int index) const { return getPropertySize(index); }  //< *deprecated* (equivalent to getPropertySize())
            ///@}

#ifdef PANTORA_EXTERNAL_DEPENDENCIES_ALLOWED
            const char* getPropertyName( int iIndex ) const;
            const PANTORA::VariantType& getProperty( const char* sName ) const;
            const PANTORA::utils::TextureStruct& getTexture( int iIndex ) const;
            PANTORA::utils::TextureStruct& getTexture( int iIndex );

            //provide internal interface for convenience
            detail::TextureDecoderInterface* getInternalItf() { return m_pclTexDecItf; };
            static TextureDecoder* createFromInternalItf( detail::TextureDecoderInterface* pclTexDecItf );
            void updateFromInternalItf();
#endif

            ///@{
            /**@name Texture interface
            These methods are used to query the representations textures.
            The number, type, size and parameters of these textures depend on the representation.
            Please find detailed documentation on this in the representation documentation:
                - \ref page1
                - \ref page2
                - \ref page3
            */
            //! Returns number of textures
            int getNumTextures() const;
            //! Returns the number of available mip-map levels for the given texture (1 if non-mipmap texture)
            int getTextureNumMipLevels( int index ) const;
            //! Returns the name, i.e. semantic of the given texture (cf.  \ref svbrdf_sec03)
            bool getTextureName( int index, char* sBuf, int iBufSize ) const;  //< (AXF_MAX_KEY_SIZE is sufficient for iBufSize)
            bool getTextureParams( int index, int & iMinFilter, int & iMagFilter, int & iWrapS, int & iWrapT, int & iWrapR, bool & bTextureArray ) const;
            //! Returns the size of the given texture in pixels, the number of channels, and the texture type in which the data is stored in the AxF file
            bool getTextureSize(int index, int iMipLevel, int & iWidth, int & iHeight, int & iDepth, int & iChannels, int & iStorageTextureType) const;
            //! Returns the physical width and height (corresponding to the spatial U,V axes) of the given texture in millimeters, provided that this texture is parameterized over the spatial U,V axes (0 otherwise)
            bool getTextureSizeMM(int index, float & fWidthMM, float& fHeightMM) const;
            //! Returns the content of the given texture / mip-map level as float array (iTargetTextureType=TEXTURE_TYPE_FLOAT) or half-float array (iTargetTextureType=TEXTURE_TYPE_HALF) into a user-allocated buffer,
            //! whose size must be iWidth*iHeight*iDepth*iChannels*sizeof(iTargetTextureType) (where iWidth,iHeight,iDepth,iChannels are the values returned by getTextureSize(),
            //! and sizeof(iTargetTextureType) is 4 or 2 bytes for TEXTURE_TYPE_FLOAT or TEXTURE_TYPE_HALF, respectively); iTargetTextureType may differ from iStorageTextureType returned by getTextureSize()
            bool getTextureData( int index, int iMipLevel, int iTargetTextureType, void* pData ) const;
            ///@}

#ifdef PANTORA_SHADER_RETRIEVAL_ENABLED
            //shader
            int getShaderSourceLen() const;
            bool getShaderSource( char* buf, int len );
#endif

            ///@{
            /**@name Factory interface
                Create and destroy decoder instances
            */
            /*! 
            \brief Create a texture decoder for a given AxF representation, target color space and target system ID

            \param hAxFRepresentation Valid handle to AxF representation (cf. axfGetRepresentation() etc.)
            \param sTargetColorSpace Trichromatic target color space (see below)
            \param iTextureOrigin cf. enum ::ETextureOrigin
            \param iTargetSystemID Deprecated - use ID_DEFAULT
            \return Pointer to TextureDecoder instance. Needs to be deallocated using destroy().

            sTargetColorSpace must be one of:
            - AXF_COLORSPACE_CIE_1931_XYZ ("XYZ"):                            the CIE 1931 XYZ color space
            - AXF_COLORSPACE_LINEAR_SRGB_E ("sRGB,E"):                        a *linear* color space with primary chromaticities matching those of the sRGB color space (IEC 61966-2-1), but - similarly to CIE XYZ - with equal-energy white point [i.e. the unity reflectance spectrum is mapped to RGB values (1 1 1)]
            - AXF_COLORSPACE_LINEAR_ADOBE_RGB_E ("AdobeRGB,E"):               a *linear* color space with primary chromaticities matching those of the Adobe RGB (1988) color space,     but - similarly to CIE XYZ - with equal-energy white point [i.e. the unity reflectance spectrum is mapped to RGB values (1 1 1)]
            - AXF_COLORSPACE_LINEAR_ADOBE_WIDEGAMUT_RGB_E ("WideGamutRGB,E"): a *linear* color space with primary chromaticities matching those of the Adobe Wide-Gamut RGB color space, but - similarly to CIE XYZ - with equal-energy white point [i.e. the unity reflectance spectrum is mapped to RGB values (1 1 1)]
            - AXF_COLORSPACE_LINEAR_PROPHOTO_RGB_E ("ProPhotoRGB,E"):         a *linear* color space with primary chromaticities matching those of the ProPhoto RGB color space,         but - similarly to CIE XYZ - with equal-energy white point [i.e. the unity reflectance spectrum is mapped to RGB values (1 1 1)]

            Note that all of the above options represent linear, trichromatic color spaces for the 2 degree standard observer.
            */
            static TextureDecoder* create( AXF_REPRESENTATION_HANDLE hAxFRepresentation, const char* sTargetColorSpace, int iTextureOrigin = ORIGIN_BOTTOMLEFT, int iTargetSystemID = ID_DEFAULT);
            //! Create a texture decoder for a given AxF representation, based on a CPUDecoder instance
            static TextureDecoder* create( AXF_REPRESENTATION_HANDLE hAxFRepresentation, CPUDecoder* pclCPUDecoder, int iTargetSystemID = ID_DEFAULT);

            //! Destroy TextureDecoder instance (ensuring correct delete across DLL boundaries)
            void destroy();
            //! Static version of destroy(): destroys **ppTextureDecoder and additionally sets *ppTextureDecoder to NULL
            static void destroy( TextureDecoder** ppTextureDecoder );
            ///@}

            // Parameters as passed to create()
            const char* getTargetColorSpaceString() const;
            int getTargetTextureOrigin() const;
            int getTargetSystemID() const;
        private:
            TextureDecoder();
            ~TextureDecoder();  //(private to make sure that destroy() is called instead of delete

            detail::TextureDecoderInterface* m_pclTexDecItf;
            detail::PropertyMap* m_pclPropertyMap;
        };

AXF_DECODING_CLOSE_NAMESPACE
