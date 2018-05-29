///////////////////////////////////////////////////////////////////////////////
// File:		CPUDecoder.h
// Authors:		Gero Mueller, Alexander Gress
//
// Title:	External interface for CPU decoding of AxF representations
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


AXF_DECODING_OPEN_NAMESPACE

        //forward declarations
        class Sampler;
        class TextureDecoder;

        namespace detail { class CPUDecoderInterface; }  // private implementation

        ////////////////////////////////////////////////
        //
        //   AxF representation decoding functionality
        //
        ///////////////////////////////////////////////

        //CPUDecoder
        ///////////////////////////////////////////////
        /*!
        \brief Decodes AxF representations on the CPU

        Decodes AxF representations on the CPU using a basic *BTF* or *SVBRDF*-like interface respectively. A spatially varying BRDF
        is a seven-dimensional function \f$ f(\lambda,u,v,\theta_i,\phi_i,\theta_o,\phi_o) \f$ whereas \f$\lambda\f$ is wavelength, \f$u,v\f$ are the 2D surface coordinates the
        material is attached to and \f$\theta_i,\phi_i,\theta_o,\phi_o\f$ are the in- and outgoing angles respectively. 
        Note that the returned results include the cosine term, i.e. the BRDF is multiplied with <N,L>.

        The interface should be mainly used for quickly integrating AxF representation decoding into CPU based renderers like ray- or pathtracers.
        For Monte-Carlo raytracers the Sampler interface is provided.
        Basic usage:
        \code{.cpp}
             AXF_REPRESENTATION_HANDLE h_axf_rep = axfGetPreferredRepresentation( h_axf_file );
             CPUDecoder* pcl_decoder = CPUDecoder::create( h_axf_rep, "sRGB,E" );
             ...
             \\collect incoming, outgoing, UV; assume trichromatic color space + alpha channel
             float f4_result_rgba[4];
             pcl_decoder->eval( f4_result_rgba, f4_result_rgba+3, wi, wo, uv );
             ...
             pcl_decoder->destroy();
        \endcode
        */
        class AXF_API CPUDecoder
        {
        public:
            //! For using shared_ptr<CPUDecoder>, pass this as second parameter to the shared_ptr constructor
            struct Deleter
            {
                inline void operator()( CPUDecoder* pclCPUDecoder )
                {
                    if ( pclCPUDecoder ) pclCPUDecoder->destroy();
                };
            };

            /*!
            \brief Evaluates the material's reflectance function in the target color space

            \param pfResult pointer to a buffer (of 3 floats) to receive the per-color-channel values of the materials reflectance function (BRDF multiplied by cosine term) for the given parameters
            \param pfAlpha optional pointer to a buffer (of 1 float); unless this is a null pointer, *pfAlpha receives an alpha/opacity value for representations with transparency (see hasTransparency()) or 1.0f otherwise
            \param v3DirIn Normalized direction specifying the incoming (light direction) in local tangent space
            \param v3DirOut Normalized direction specifying the outgoing (view direction) in local tangent space
            \param v2UV Normalized UV coordinate \f$u,v \in [0,1)\f$ specifying the location on the material

            Note that the values stored in pfResult are in the target color space that was specified in the call to create().
            In any case, the returned values are trichromatic (3 float values) since currently only trichromatic target color spaces can be specified.
            */
            void eval( float* pfResult, float* pfAlpha, const float* v3DirIn, const float* v3DirOut, const float* v2UV ) const;

            ///@{
            /**@name Convenience functions (for computing preview images)
            */
            /*!
            \brief Compute simple planar preview image
            
            \param pImage pointer to a buffer of iWidthPixel*iHeightPixel*iChannels floats to receive the preview image
            \param iWidthPixel width of the preview image in pixels
            \param iHeightPixel height of the preview image in pixels
            \param iChannels number of channels of the preview image (3 or 4)
            \param fWidthMM spatial width of the preview image in millimeters
            \param fHeightMM spatial height of the preview image in millimeters

            A simple preview image is computed with a central viewpoint and point light source placed above the planar sample geometry.
            The preview image is computed in the target color space that was specified in the call to create(), which is trichromatic, and can optionally include an alpha channel.

            Thus iChannels must be either 3 or 4:
               - For iChannels = 3, a 3-channel color image (without alpha) is returned. If the decoded representation has transparency (see hasTransparency()), the material is rendered in front of a checkerboard
                 background, which becomes "baked" into the resulting 3-channel image.
               - For iChannels = 4, a 4-channel color/alpha image is returned (e.g. RGBA). No background is integrated into the resulting image.

            If it is intented to store the computed preview image in the AxF file via axfStorePreviewImage(), it is recommended to choose iChannel = 4 if hasTransparency() is true, and iChannel = 3 otherwise.
            */
            void computePreviewImage(float* pImage, int iWidthPixel, int iHeightPixel, int iChannels, float fWidthMM, float fHeightMM);

            /*!
            \brief Compute simple planar preview image (variant with non-interleaved color and alpha images)
            
            \param pColorImage pointer to a buffer of iWidthPixel*iHeightPixel*3 floats to receive the preview image's color channels
            \param pAlphaImage optional pointer to a buffer of iWidthPixel*iHeightPixel floats to receive the preview image's alpha/opacity channel
            \param iWidthPixel width of the preview image in pixels
            \param iHeightPixel height of the preview image in pixels
            \param fWidthMM spatial width of the preview image in millimeters
            \param fHeightMM spatial height of the preview image in millimeters

            A simple preview image is computed with a central viewpoint and point light source placed above the planar sample geometry.
            The preview image is computed in the target color space that was specified in the call to create(), which is trichromatic.

            This variant of the function returns the computed color channels and alpha channel separately as 3-channel color image and 1-channel alpha image, respectively.

            If pAlphaImage is null, no alpha image is returned. The returned color image is not affected by this.
            (So using this variant of computePreviewImage() with pAlphaImage = null is NOT equivalent to using the above variant with iChannels = 3 if the representation has transparency.)
            */
            void computePreviewImage(float* pColorImage, float* pAlphaImage, int iWidthPixel, int iHeightPixel, float fWidthMM, float fHeightMM);

            //! DEPRECATED variant of the function: equivalent to computePreviewImage(pColorImage, pAlphaImage, iSizePixel, iSizePixel, fSizeMM, fSizeMM)
            void computePreviewImage(float* pColorImage, float* pAlphaImage, int iSizePixel, float fSizeMM);
            ///@}

            ///@{
            /**@name Information about the decoded representation
            */
            //! Get width of the representation in pixels
            int getWidthPixel() const;
            //! Get height of the representation in pixels
            int getHeightPixel() const;
            //! Get width of the representation in millimeters
            float getWidthMM() const;
            //! Get height of the representation in millimeters
            float getHeightMM() const;
            //! Indicates whether the representation contains an alpha map that specifies transparency
            bool hasTransparency() const;
            ///@}

#ifdef PANTORA_EXTERNAL_DEPENDENCIES_ALLOWED
            static CPUDecoder* createFromInternalItf(detail::CPUDecoderInterface* pclCPUDecItf);
            //provide internal interface for convenience
            detail::CPUDecoderInterface* getInternalItf() { return m_pclDecItf; };
#endif

            ///@{
            /**@name Factory interface
            */
            //! Create a texture decoder for a given AxF representation, target color space and target system ID

            /*! \brief Static factory method to create a CPUDecoder for given AxF representation.

            \param hAxFRepresentation Valid handle to AxF representation (cf. axfGetRepresentation() etc.)
            \param sTargetColorSpace Trichromatic target color space (see below)
            \param iTextureOrigin cf. enum ::ETextureOrigin
            \return Pointer to CPUDecoder instance. Needs to be deallocated using destroy().

            sTargetColorSpace must be one of:
            - AXF_COLORSPACE_CIE_1931_XYZ ("XYZ"):                            the CIE 1931 XYZ color space
            - AXF_COLORSPACE_LINEAR_SRGB_E ("sRGB,E"):                        a *linear* color space with primary chromaticities matching those of the sRGB color space (IEC 61966-2-1), but - similarly to CIE XYZ - with equal-energy white point [i.e. the unity reflectance spectrum is mapped to RGB values (1 1 1)]
            - AXF_COLORSPACE_LINEAR_ADOBE_RGB_E ("AdobeRGB,E"):               a *linear* color space with primary chromaticities matching those of the Adobe RGB (1988) color space,     but - similarly to CIE XYZ - with equal-energy white point [i.e. the unity reflectance spectrum is mapped to RGB values (1 1 1)]
            - AXF_COLORSPACE_LINEAR_ADOBE_WIDEGAMUT_RGB_E ("WideGamutRGB,E"): a *linear* color space with primary chromaticities matching those of the Adobe Wide-Gamut RGB color space, but - similarly to CIE XYZ - with equal-energy white point [i.e. the unity reflectance spectrum is mapped to RGB values (1 1 1)]
            - AXF_COLORSPACE_LINEAR_PROPHOTO_RGB_E ("ProPhotoRGB,E"):         a *linear* color space with primary chromaticities matching those of the ProPhoto RGB color space,         but - similarly to CIE XYZ - with equal-energy white point [i.e. the unity reflectance spectrum is mapped to RGB values (1 1 1)]

            Note that all of the above options represent linear, trichromatic color spaces for the 2 degree standard observer.
            */
            static CPUDecoder* create( AXF_REPRESENTATION_HANDLE hAxFRepresentation, const char* sTargetColorSpace, int iTextureOrigin = ORIGIN_BOTTOMLEFT );

            // Parameters as passed to create()
            const char* getTargetColorSpaceString() const;
            int getTargetTextureOrigin() const;

            /*! \brief Destroys CPUDecoder instance.
            */
            void destroy();
            //! Static version of destroy(): destroys **ppCPUDecoder and additionally sets *ppCPUDecoder to NULL
            static void destroy( CPUDecoder** ppCPUDecoder );
            ///@}

        private:
            CPUDecoder();
            ~CPUDecoder();  //(private to make sure that destroy() is called instead of delete

            detail::CPUDecoderInterface* m_pclDecItf;

            friend class Sampler;
            friend class TextureDecoder;
        };

AXF_DECODING_CLOSE_NAMESPACE
