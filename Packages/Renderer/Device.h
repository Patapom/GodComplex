// RendererManaged.h
#pragma once

#include "RenderStates.h"

using namespace System;

namespace Renderer {

	ref class Shader;
	ref class Primitive;
	ref class Texture2D;
	ref class Texture3D;

	// Texture view interface
	public interface class	IView {
	public:
		virtual property UInt32							Width				{ UInt32 get() = 0; }
		virtual property UInt32							Height				{ UInt32 get() = 0; }
		virtual property UInt32							ArraySizeOrDepth	{ UInt32 get() = 0; }
		virtual property ::ID3D11ShaderResourceView*	SRV					{ ::ID3D11ShaderResourceView*	get() = 0; }
		virtual property ::ID3D11RenderTargetView*		RTV					{ ::ID3D11RenderTargetView*		get() = 0; }
		virtual property ::ID3D11UnorderedAccessView*	UAV					{ ::ID3D11UnorderedAccessView*	get() = 0; }
		virtual property ::ID3D11DepthStencilView*		DSV					{ ::ID3D11DepthStencilView*		get() = 0; }
	};

	// Main device class from which everything else is derived
	public ref class Device {
	public:

		ref class AdapterOutput {
		internal:
			const ::Device::AdapterOutput*	m_pAdapterOutput;
		public:
			enum class ROTATION {
				UNSPECIFIED = 0,
				IDENTITY = 1,
				ROTATE_90 = 2,
				ROTATE_180 = 3,
				ROTATE_270 = 4,
			};

			property UInt32						AdapterIndex;
			property UInt32						OutputIndex;
			property System::IntPtr				MonitorHandle;		// The HMONITOR associated to this output
			property System::Drawing::Rectangle	Rectangle;			// The desktop rectangle covered by this output
			property ROTATION					Rotation;			// Optional rotation specification

		};

	internal:

		::Device*		m_pDevice;
 		Texture2D^		m_defaultTarget;
 		Texture2D^		m_defaultDepthStencil;

		// We offer the ever useful fullscreen quad
//		::Primitive*	m_pScreenQuad;
		Primitive^		m_screenQuad;

		// We keep the list of the last 8 assigned RTVs
		::ID3D11RenderTargetView**	m_ppRenderTargetViews;

		cli::array< cli::array< AdapterOutput^ >^ >^	m_cachedAdapterOutputs;

	public:

		property void*			NativeDevice		{ void* get() { return reinterpret_cast<void*>( m_pDevice ); } }
		property Texture2D^		DefaultTarget		{ Texture2D^ get() { return m_defaultTarget; } }
		property Texture2D^		DefaultDepthStencil { Texture2D^ get() { return m_defaultDepthStencil; } }
		property Primitive^		ScreenQuad			{ Primitive^ get() { return m_screenQuad; } }
		property cli::array< cli::array< AdapterOutput^ >^ >^	AdapterOutputs	{ cli::array< cli::array< AdapterOutput^ >^ >^ get(); }

		Device() {
			m_pDevice = new ::Device();
 			m_defaultTarget = nullptr;
 			m_defaultDepthStencil = nullptr;
			m_ppRenderTargetViews = new ::ID3D11RenderTargetView*[8];
			memset( m_ppRenderTargetViews, 0, 8*sizeof(::ID3D11RenderTargetView*) );
			m_cachedAdapterOutputs = nullptr;
		}
		~Device() {
//			delete m_pQuad;
			SAFE_DELETE_ARRAY( m_ppRenderTargetViews );
			m_pDevice->Exit();
			delete m_pDevice;
			m_pDevice = NULL;
		}

 		void	Init( System::IntPtr _WindowHandle, bool _FullScreen, bool _sRGBRenderTarget );
		void	Exit() {
			m_pDevice->Exit();
		}

		// Resizes the internal swap chain and default render targets
		void	ResizeSwapChain( UInt32 _width, UInt32 _height, bool _sRGB );
		void	ResizeSwapChain( UInt32 _width, UInt32 _height );

		// Switches to fullscreen
		//	_targetOutput, an optional output to switch to (only used if _fullscreen = true)
		void	SwitchFullScreenState( bool _fullscreen, AdapterOutput^ _targetOutput );
		void	SwitchFullScreenState( bool _fullscreen );

		void	Clear( SharpMath::float4 _clearColor );
		void	Clear( Texture2D^ _renderTarget, SharpMath::float4 _clearColor );
		void	Clear( Texture3D^ _renderTarget, SharpMath::float4 _clearColor );
		void	ClearDepthStencil( Texture2D^ _renderTarget, float _Z, Byte _Stencil, bool _ClearDepth, bool _ClearStencil );

		void	SetRenderStates( RASTERIZER_STATE _RS, DEPTHSTENCIL_STATE _DS, BLEND_STATE _BS );
		void	SetRenderTarget( Texture2D^ _renderTarget, Texture2D^ _depthStencilTarget );
		void	SetRenderTargets( cli::array<IView^>^ _RenderTargetViews, IView^ _depthStencilTargetView );

		void	RemoveRenderTargets()	{ m_pDevice->RemoveRenderTargets(); }
		void	RemoveUAVs()			{ m_pDevice->RemoveUAVs(); }

		void	RenderFullscreenQuad( Shader^ _Shader );

		void	Present( bool _flushCommands ) {
			if ( _flushCommands )
				m_pDevice->DXContext().Flush();

			m_pDevice->DXSwapChain().Present( 0, 0 );
		}

		void	ReloadModifiedShaders() {
			// Reload modified shaders
			::Shader::WatchShadersModifications();
			::ComputeShader::WatchShadersModifications();
		}

		// Patapom [18/01/30] Performance queries
		void		PerfBeginFrame()											{ m_pDevice->PerfBeginFrame(); }
		void		PerfSetMarker( U32 _markerID )								{ m_pDevice->PerfSetMarker( _markerID ); }
		double		PerfEndFrame()												{ return m_pDevice->PerfEndFrame(); }
		double		PerfGetMilliSeconds( U32 _markerID )						{ return m_pDevice->PerfGetMilliSeconds( _markerID ); }
		double		PerfGetMilliSeconds( U32 _markerIDStart, U32 _markerIDEnd ) { return m_pDevice->PerfGetMilliSeconds( _markerIDStart, _markerIDEnd ); }

		// Compresses the source image to the appropriate format using GPU acceleration
		ImageUtility::ImagesMatrix^	DDSCompress( ImageUtility::ImagesMatrix^ _sourceImage, ImageUtility::ImagesMatrix::COMPRESSION_TYPE _compressionType, ImageUtility::COMPONENT_FORMAT _componentFormat );
	};
}
