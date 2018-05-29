///////////////////////////////////////////////////////////////////////////////
// File:		Sampler.h
// Authors:		Gero Mueller, Alexander Gress
//
// Title:	External interface for sampling AxF representations in a Monte-Carlo rendering context.
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
        class CPUDecoder;

        namespace detail { class SamplerInterface; }  // private implementation

        //Sampler
        ///////////////////////////////////////////////
        /*!
        \brief Samples AxF representations on the CPU

        Importance sampling is a well-known strategy for Monte-Carlo based rendering algorithms to reduce variance i.e noise. Briefly,
        secondary rays are sampled at the surface to be rendered based on a pdf that behaves closely to the surfaces' BRDF.
        Basic usage:
        \code{.cpp}
             AXF_REPRESENTATION_HANDLE h_axf_rep = axfGetPreferredRepresentation( h_axf_file );
             CPUDecoder* pcl_decoder = CPUDecoder::create( h_axf_rep );
             ...
             Sampler* pcl_sampler = Sampler::create( h_axf_rep, pcl_decoder );
             ...
             Vec3 v3_sampled;
             Vec3 v3_xi = rndVec3();
             float f_pdf = pcl_sampler->sample( &v3_xi[0], &v3_dir[0], uv, &v3_sampled[0] );
              ...
              pcl_sampler->destroy();
        \endcode
        */
        class AXF_API Sampler
        {
        public:
            //! For using shared_ptr<Sampler>, pass this as second parameter to the shared_ptr constructor
            struct Deleter
            {
                inline void operator()( Sampler* pclSampler )
                {
                    if ( pclSampler ) pclSampler->destroy();
                };
            };

            /*!
            \param v3Xi Vector of three random values \f$ \xi_{0,1,2} \in [0,1]\f$ drawn from a uniform distribution
            \param v3Dir The normalized direction of the (viewing) ray in local tangent space for which corresponding ray will be sampled
            \param v2UV Normalized UV coordinate \f$u,v \in [0,1)\f$ specifying the location on the material
            \param v3DirSampled Returns the sampled normalized direction in local tangent space
            \return Value of the sampled pdf for the drawn direction

            The actual pdf used for sampling depends on the representation.
            */
            float sample( const float* v3Xi, const float* v3Dir, const float* v2UV, float* v3DirSampled ) const;
            /*!
            \param v3DirIn The normalized direction of the incoming ray in local tangent space
            \param v3DirOut The normalized direction of the outgoing ray in local tangent space
            \param v2UV Normalized UV coordinate \f$u,v \in [0,1)\f$ specifying the location on the material
            \return Value of the pdf.

            Evaluate the pdf used for sampling the representation.
            */
            float pdf( const float* v3DirIn, const float* v3DirOut, const float* v2UV ) const;

            //proposal: get parameters for using external sampling, not implemented yet
            //static bool get
            //void getNormal(const float* v2UV, float* v3Normal );
            //float getDiffuseCoeff(const float* v2UV );
            //float getSpecularCoeff(const float* v2UV );
            //float getWardAlphaEstimate( const float* v2UV );
            //void getAnisoWardAlphaEstimate( const float* v2UV, float* v2AlphaXY );

            /*! \brief Static factory method to create a CPUDecoder for given AxF representation.

            \param hAxFRepresentation Valid handle to AxF representation (cf. axfGetRepresentation() etc.)
            \param pclDecoder Pointer to CPUDecoder instance created for hAxFRepresentation
            \return Pointer to Sampler instance. Needs to be deallocated using destroy().
            */
            static Sampler* create( AXF_REPRESENTATION_HANDLE hAxFRepresentation, CPUDecoder* pclDecoder );

            /*! \brief Destroys Sampler instance.
            */
            void destroy();
            //! Static version of destroy(): destroys **ppSampler and additionally sets *ppSampler to NULL
            static void destroy( Sampler** ppSampler );
        private:
            Sampler();
            ~Sampler();  //(private to make sure that destroy() is called instead of delete

            detail::SamplerInterface* m_pclSmplItf;
        };

AXF_DECODING_CLOSE_NAMESPACE
