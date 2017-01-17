//#define	DEBUG_BINARY_PATTERN
//#define	BYPASS_GPU_DOWNSAMPLING

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SharpMath;
using Renderer;

namespace GenerateBlueNoise
{
	/// <summary>
	/// Implements the Blue Noise genration algorithm described by "The void-and-cluster method for dither array generation" by Robert Ulichney (1993)
	/// http://cv.ulichney.com/papers/1993-void-cluster.pdf
	/// 
	/// NOTE: The method is implemented as both CPU and GPU depending on whether you pass a valid Device to the constructor.
	/// 
	/// The void-and-cluster algorithm is extremely simple:
	///		► At each step, find the place where there is the largest gap and place a pixel there.
	///					Yup! That's basically it... :/
	///	
	/// In detail:
	///		• We perform a gaussian filtering of a binary texture and find the maximum value.
	///		• We place a 1 where we found the maximum
	///		• We place the normalized index of the iteration in the dithering array
	/// 
	/// We do that for N iterations where N = texture area...
	/// The main issue here is that, in order to be efficient, the gaussian filter kernel needs to be quite large,
	///	 ideally the size of the texture itself so we're basically dealing with a O(N^4) algorithm here!
	/// 
	/// Other references:
	///		• https://pdfs.semanticscholar.org/88d3/c5fc3d631359d4db2d9d44661d13a46bd7e9.pdf
	/// </summary>
	public class GeneratorVoidAndClusterGPU : IDisposable {

		#region FIELDS

		const int	KERNEL_HALF_SIZE = 16;

		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		struct CB_Main {
			public uint		_texturePOT;
			public uint		_textureSize;
			public uint		_textureMask;
			public float	_kernelFactor;		// = 1/sigma_s²

			public uint		_randomOffsetX;
			public uint		_randomOffsetY;
			public uint		_iterationIndex;
		}
		[System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
		struct CB_Mips {
			public uint		_textureMipSource;
			public uint		_textureMipTarget;
		}

		int				m_texturePOT;
		uint			m_textureSize;
		uint			m_textureSizeMask;
		uint			m_textureTotalSize;

		// Resulting array
		float[,]		m_ditheringArray = null;

		// GPU-structures
		Device			m_device;

		ComputeShader	m_CS_Filter;						// Filters the binary pattern and computes a "clustering score"
		ComputeShader	m_CS_DownsampleScore16;				// Downsamples and keeps the best score from a (multiple of) 16x16 texture down to a (multiple of) 1x1 texture
		ComputeShader	m_CS_DownsampleScore8;				// Downsamples and keeps the best score from a (multiple of) 8x8 texture down to a (multiple of) 1x1 texture
		ComputeShader	m_CS_DownsampleScore4;				// Downsamples and keeps the best score from a (multiple of) 4x4 texture down to a (multiple of) 1x1 texture
		ComputeShader	m_CS_DownsampleScore2;				// Downsamples and keeps the best score from a (multiple of) 2x2 texture down to a (multiple of) 1x1 texture
		ComputeShader	m_CS_Splat;							// Splats a single value where we spotted the best score

		ConstantBuffer<CB_Main>			m_CB_Main;
 		ConstantBuffer<CB_Mips>			m_CB_Mips;

 		Texture2D		m_texBinaryPattern = null;
 		Texture2D		m_texBinaryPatternCPU = null;
 		Texture2D		m_texDitheringArray = null;
 		Texture2D		m_texDitheringArrayCPU = null;
 		Texture2D		m_texScore0 = null;
 		Texture2D		m_texScore1 = null;
 		Texture2D		m_texScoreCPU = null;

		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="_device">Device CAN be null, in which case the CPU version will be used</param>
		/// <param name="_texturePOT"></param>
		public	GeneratorVoidAndClusterGPU( Device _device, uint _texturePOT ) {
			m_texturePOT = (int) _texturePOT;
			m_textureSize = 1U << m_texturePOT;
			m_textureSizeMask = m_textureSize - 1;
			m_textureTotalSize = m_textureSize * m_textureSize;

			m_ditheringArray = new float[m_textureSize,m_textureSize];

			m_device = _device;
			if ( _device == null )
				return;	// Use CPU version

			try {
				m_CS_Filter = new ComputeShader( _device, new System.IO.FileInfo( @"Shaders/VoidAndCluster.hlsl" ), "CS__Filter", null );
				m_CS_DownsampleScore16 = new ComputeShader( _device, new System.IO.FileInfo( @"Shaders/VoidAndCluster.hlsl" ), "CS__DownsampleScore16", null );
				m_CS_DownsampleScore8 = new ComputeShader( _device, new System.IO.FileInfo( @"Shaders/VoidAndCluster.hlsl" ), "CS__DownsampleScore8", null );
				m_CS_DownsampleScore4 = new ComputeShader( _device, new System.IO.FileInfo( @"Shaders/VoidAndCluster.hlsl" ), "CS__DownsampleScore4", null );
				m_CS_DownsampleScore2 = new ComputeShader( _device, new System.IO.FileInfo( @"Shaders/VoidAndCluster.hlsl" ), "CS__DownsampleScore2", null );
				m_CS_Splat = new ComputeShader( _device, new System.IO.FileInfo( @"Shaders/VoidAndCluster.hlsl" ), "CS__Splat", null );

				m_CB_Main = new ConstantBuffer<CB_Main>( _device, 0 );
				m_CB_Mips = new ConstantBuffer<CB_Mips>( _device, 1 );

				m_texBinaryPattern = new Texture2D( _device, m_textureSize, m_textureSize, 1, 1, PIXEL_FORMAT.R32_UINT, false, true, null );
				#if DEBUG_BINARY_PATTERN || BYPASS_GPU_DOWNSAMPLING
					m_texBinaryPatternCPU = new Texture2D( _device, m_textureSize, m_textureSize, 1, 1, PIXEL_FORMAT.R32_UINT, true, true, null );
				#endif
				m_texDitheringArray = new Texture2D( _device, m_textureSize, m_textureSize, 1, 1, PIXEL_FORMAT.R32_FLOAT, false, true, null );
				m_texDitheringArrayCPU = new Texture2D( _device, m_textureSize, m_textureSize, 1, 1, PIXEL_FORMAT.R32_FLOAT, true, true, null );
				m_texScore0 = new Texture2D( _device, m_textureSize, m_textureSize, 1, 0, PIXEL_FORMAT.RG32_FLOAT, false, true, null );
				m_texScore1 = new Texture2D( _device, m_textureSize, m_textureSize, 1, 0, PIXEL_FORMAT.RG32_FLOAT, false, true, null );
				m_texScoreCPU = new Texture2D( _device, m_textureSize, m_textureSize, 1, 0, PIXEL_FORMAT.RG32_FLOAT, true, true, null );
			} catch ( Exception _e ) {
				throw new Exception( "An error occurred while creating DirectX structures: " + _e.Message, _e );
			}
		}

		#region IDisposable Members

		public void Dispose() {
			if ( m_device == null )
				return;	// CPU version didn't allocate anything

			m_texScoreCPU.Dispose();
			m_texScore1.Dispose();
			m_texScore0.Dispose();
			m_texDitheringArrayCPU.Dispose();
			m_texDitheringArray.Dispose();
			m_texBinaryPattern.Dispose();

			m_CB_Mips.Dispose();
			m_CB_Main.Dispose();

			m_CS_Splat.Dispose();
			m_CS_DownsampleScore2.Dispose();
			m_CS_DownsampleScore4.Dispose();
			m_CS_DownsampleScore8.Dispose();
			m_CS_DownsampleScore16.Dispose();
			m_CS_Filter.Dispose();
		}

		#endregion

		public delegate void	ProgressDelegate( float _progress, float[,] _texture, List<float> _statistics );

		#region GPU Version

		/// <summary>
		/// Generates blue noise distribution using the void-and-cluster method
		/// </summary>
		/// <param name="_randomSeed"></param>
		/// <param name="_standardDeviation">Standard deviation for gaussian kernel. If not sure, use 2.1</param>
		/// <param name="_notifyProgressInterval">Will read back the GPU texture to the CPU and notify of progress each time we run for that much progress interval</param>
		/// <param name="_progress"></param>
		public void		Generate( uint _randomSeed, float _standardDeviation, float _notifyProgressInterval, ProgressDelegate _progress ) {
			if ( m_device == null ) {
				// Use the CPU version!
				GenerateCPU( _randomSeed, _standardDeviation, _notifyProgressInterval, _progress );
				return;
			}

			uint	progressNotificationInterval = Math.Max( 1, (uint) Math.Ceiling( _notifyProgressInterval * m_textureTotalSize ) );

			m_CB_Main.m._texturePOT = (uint) m_texturePOT;
			m_CB_Main.m._textureSize = m_textureSize;
			m_CB_Main.m._textureMask = m_textureSizeMask;
			m_CB_Main.m._kernelFactor = -1.0f / (2.0f * _standardDeviation * _standardDeviation);

			m_texBinaryPattern.SetCSUAV( 0 );
			m_texDitheringArray.SetCSUAV( 1 );

			SimpleRNG.SetSeed( _randomSeed, 362436069U );

			for ( uint iterationIndex=0; iterationIndex < m_textureTotalSize; iterationIndex++ ) {

				m_CB_Main.m._randomOffsetX = GetUniformInt( m_textureSize );
				m_CB_Main.m._randomOffsetY = GetUniformInt( m_textureSize );
				m_CB_Main.m._iterationIndex = iterationIndex;
				m_CB_Main.UpdateData();

				// 1] Filter binary texture and compute "clustering score"
				if ( m_CS_Filter.Use() ) {
					m_texScore0.SetCSUAV( 2 );

					uint	groupsCount = m_textureSize >> 4;
					m_CS_Filter.Dispatch( groupsCount, groupsCount, 1 );

					m_texScore0.RemoveFromLastAssignedSlotUAV();
				}

				// 2] Downsample score and keep lowest value and the position where to find it
				#if !BYPASS_GPU_DOWNSAMPLING
					if ( m_CS_DownsampleScore16.Use() ) {
						uint	groupsCount = m_textureSize;
						uint	mipLevelIndex = 0;
						uint	mipLevelsCount = (uint) m_texturePOT;

						m_CB_Mips.m._textureMipTarget = 0;

						// 2.1) Downsample by groups of 4 mips
						while ( mipLevelsCount >= 4 ) {
							mipLevelIndex += 4;
							mipLevelsCount -= 4;
							groupsCount >>= 4;

							m_texScore0.SetCS( 0 );
							m_texScore1.SetCSUAV( 2, m_texScore1.GetView( mipLevelIndex, 1, 0, 1 ) );

							m_CB_Mips.m._textureMipSource = m_CB_Mips.m._textureMipTarget;
							m_CB_Mips.m._textureMipTarget = mipLevelIndex;
							m_CB_Mips.UpdateData();

							m_CS_DownsampleScore16.Dispatch( groupsCount, groupsCount, 1 );

							m_texScore0.RemoveFromLastAssignedSlots();
							m_texScore1.RemoveFromLastAssignedSlotUAV();

							// Swap
							Texture2D	temp = m_texScore0;
							m_texScore0 = m_texScore1;
							m_texScore1 = temp;
						}

						// 2.2) Downsample to last mip
						ComputeShader	CSLastMip = null;
						switch ( mipLevelsCount ) {
							case 3: CSLastMip = m_CS_DownsampleScore8; break;
							case 2: CSLastMip = m_CS_DownsampleScore4; break;
							case 1: CSLastMip = m_CS_DownsampleScore2; break;
						}
						if ( CSLastMip != null && CSLastMip.Use() ) {

							m_texScore0.SetCS( 0 );
							m_texScore1.SetCSUAV( 2, m_texScore1.GetView( (uint) m_texturePOT, 1, 0, 1 ) );

							m_CB_Mips.m._textureMipSource = m_CB_Mips.m._textureMipTarget;
							m_CB_Mips.m._textureMipTarget = (uint) m_texturePOT;
							m_CB_Mips.UpdateData();

							CSLastMip.Dispatch( 1, 1, 1 );

							m_texScore0.RemoveFromLastAssignedSlots();
							m_texScore1.RemoveFromLastAssignedSlotUAV();

							// Swap
							Texture2D	temp = m_texScore0;
							m_texScore0 = m_texScore1;
							m_texScore1 = temp;
						}

#if DEBUG
//DebugDownsampling();
//DebugSplatPosition( iterationIndex );
#endif
					}
				#else
					DownsampleCPU( iterationIndex );
					m_CB_Mips.m._textureMipTarget = (uint) m_texturePOT;
				#endif

				// 3] Splat a new pixel where we located the best score
				if ( m_CS_Splat.Use() ) {
					m_texScore0.SetCS( 0 );

					m_CB_Mips.m._textureMipSource = m_CB_Mips.m._textureMipTarget;
					m_CB_Mips.UpdateData();

					m_CS_Splat.Dispatch( 1, 1, 1 );

					m_texScore0.RemoveFromLastAssignedSlots();
				}

				if ( _progress == null || iterationIndex % progressNotificationInterval != 0 )
					continue;

				ReadBackTexture();
				_progress( (float) iterationIndex / m_textureTotalSize, m_ditheringArray, null );
			}

			m_texBinaryPattern.RemoveFromLastAssignedSlotUAV();
			m_texDitheringArray.RemoveFromLastAssignedSlotUAV();

			// Read back final result
			ReadBackTexture();

			// Notify one last time
			if ( _progress != null )
				_progress( 1.0f, m_ditheringArray, null );
		}

		/// <summary>
		/// Reads the specified 1D-valued GPU texture back to the CPU
		/// </summary>
		/// <param name="_textureGPU"></param>
		/// <param name="_textureCPU"></param>
		void	ReadBackTexture() {
			#if !DEBUG_BINARY_PATTERN
				m_texDitheringArrayCPU.CopyFrom( m_texDitheringArray );
				m_texDitheringArrayCPU.ReadPixels( 0, 0, ( uint _X, uint _Y, System.IO.BinaryReader _R ) => {
					m_ditheringArray[_X,_Y] = _R.ReadSingle();
				} );
			#else
				m_texBinaryPatternCPU.CopyFrom( m_texBinaryPattern );
				m_texBinaryPatternCPU.ReadPixels( 0, 0, ( uint _X, uint _Y, System.IO.BinaryReader _R ) => {
					m_ditheringArray[_X,_Y] = _R.ReadUInt32();
				} );
			#endif
		}

		uint[,]		m_binaryPattern;
#if BYPASS_GPU_DOWNSAMPLING
		float[,]	m_score;
		void	DownsampleCPU( uint _iterationIndex ) {
			if ( m_score == null ) {
				m_score = new float[m_textureSize,m_textureSize];
				m_binaryPattern = new uint[m_textureSize,m_textureSize];
			}

			// Manually read back scores and look for minimum
			m_texScoreCPU.CopyFrom( m_texScore0 );
			m_texScoreCPU.ReadPixels( 0, 0, ( uint _X, uint _Y, System.IO.BinaryReader _R ) => { m_score[_X,_Y] = _R.ReadSingle(); _R.ReadUInt32(); } );

			float	bestScore = float.MaxValue;
			uint	bestX = 0, bestY = 0;
			for ( uint Y=0; Y < m_textureSize; Y++ )
				for ( uint X=0; X < m_textureSize; X++ ) {
					if ( m_score[X,Y] >= bestScore )
						continue;
					bestScore = m_score[X,Y];
					bestX = X;
					bestY = Y;
				}

//bestX = _iterationIndex & m_textureSizeMask;
//bestY = _iterationIndex >> m_texturePOT;

			#if DEBUG
				m_texBinaryPatternCPU.CopyFrom( m_texBinaryPattern );
				m_texBinaryPatternCPU.ReadPixels( 0, 0, ( uint _X, uint _Y, System.IO.BinaryReader _R ) => { m_binaryPattern[_X,_Y] = _R.ReadUInt32(); } );
				if ( m_binaryPattern[bestX,bestY] != 0 )
					throw new Exception( "Already selected!" );
			#endif

			// Write best minimum to last mip
			m_texScoreCPU.WritePixels( (uint) m_texturePOT, 0, ( uint _X, uint _Y, System.IO.BinaryWriter _W ) => {
				_W.Write( bestScore );
				uint	V = (bestY << 16) | bestX;
				_W.Write( V );
			} );
			m_texScore0.CopyFrom( m_texScoreCPU );
		}
#endif

		[System.Diagnostics.DebuggerDisplay( "{coordX},{coordY} = {score}" )]
		struct bisou {
			public float	score;
			public uint		coordX;
			public uint		coordY;
		}
		void	DebugDownsampling() {
			m_texScoreCPU.CopyFrom( m_texScore0 );

			bisou[][,]	mips = new bisou[1+m_texturePOT][,];
			uint	S = m_textureSize;
			for ( int mipIndex=0; mipIndex <= m_texturePOT; mipIndex++ ) {
				bisou[,]	mip = new bisou[S,S];
				mips[mipIndex] = mip;

				m_texScoreCPU.ReadPixels( (uint) mipIndex, 0, ( uint _X, uint _Y, System.IO.BinaryReader _R ) => {
					mip[_X,_Y].score = _R.ReadSingle();
					uint	V = _R.ReadUInt32();
					mip[_X,_Y].coordX = V & 0xFFFFU;
					mip[_X,_Y].coordY = (V >> 16) & 0xFFFFU;
				} );

				S >>= 1;
			}
		}

		void	DebugSplatPosition( uint _iterationIndex ) {
			if ( m_binaryPattern == null ) {
				m_binaryPattern = new uint[m_textureSize,m_textureSize];
			}

			float	bestScore;
			uint	bestPos = 0;
			m_texScoreCPU.CopyFrom( m_texScore0 );
			m_texScoreCPU.ReadPixels( (uint) m_texturePOT, 0, ( uint _X, uint _Y, System.IO.BinaryReader _R ) => { bestScore = _R.ReadSingle(); bestPos = _R.ReadUInt32(); } );

			uint	bestX = bestPos & 0xFFFFU;
			uint	bestY = (bestPos >> 16) & 0xFFFFU;

System.Diagnostics.Debug.WriteLine( "Iteration #" + _iterationIndex + " => X=" + bestX + ", Y=" + bestY );

			m_texBinaryPatternCPU.CopyFrom( m_texBinaryPattern );
			m_texBinaryPatternCPU.ReadPixels( 0, 0, ( uint _X, uint _Y, System.IO.BinaryReader _R ) => { m_binaryPattern[_X,_Y] = _R.ReadUInt32(); } );
			if ( m_binaryPattern[bestX,bestY] != 0 )
				System.Diagnostics.Debug.WriteLine( "CONFLICT!" );
//				throw new Exception( "Already selected!" );
		}

		#endregion

		#region CPU Version

		/// <summary>
		/// Generates blue noise distribution using the void-and-cluster method
		/// </summary>
		/// <param name="_randomSeed"></param>
		/// <param name="_standardDeviationImage">Standard deviation for image space. If not sure, use 2.1</param>
		/// <param name="_standardDeviationValue">Standard deviation for value space. If not sure, use 1.0</param>
		/// <param name="_notifyProgressInterval">Will read back the GPU texture to the CPU and notify of progress each time we run for that much progress interval</param>
		/// <param name="_progress"></param>
		public void		GenerateCPU( uint _randomSeed, float _standardDeviation, float _notifyProgressInterval, ProgressDelegate _progress ) {

			int[,]		binaryPattern = new int[m_textureSize,m_textureSize];

// 			// 1] Place a first pixel randomly in the image
// 			uint	randomPixelIndex = GetUniformInt( m_textureTotalSize );
// 			uint	randomPixelX = randomPixelIndex & m_textureSizeMask;
// 			uint	randomPixelY = randomPixelIndex >> m_texturePOT;
// 			binaryPattern[randomPixelX,randomPixelY] = 1;
// 			m_ditheringArray[randomPixelX,randomPixelY] = 1.0f / m_textureTotalSize;

			// 2] Perform N iterations to cover the entire texture
			_standardDeviation = 1.0f / (2.0f * _standardDeviation * _standardDeviation);
			float	gaussianNormalizer = _standardDeviation / (float) Math.PI;

			int		progressNotificationInterval = Math.Max( 1, (int) Math.Ceiling( _notifyProgressInterval * m_textureTotalSize ) );

			float	kernelValue, filterValue;
			int		bestPositionX = 0, bestPositionY = 0;
			uint	finalX, finalY;
			for ( uint iterationIndex=1; iterationIndex < m_textureTotalSize; iterationIndex++ ) {

				// Find a random offset
				uint	randomPixelIndex = GetUniformInt( m_textureTotalSize );
				uint	randomPixelX = randomPixelIndex & m_textureSizeMask;
				uint	randomPixelY = randomPixelIndex >> m_texturePOT;

				// 2.1) Filter out the binary pattern and keep track of the maximum
				float	minFilterValue = float.MaxValue;
				for ( uint Y=0; Y < (int) m_textureSize; Y++ ) {
					int	centralY = (int) ((Y + randomPixelY) & m_textureSizeMask);
					for ( uint X=0; X < (int) m_textureSize; X++ ) {
						int	centralX = (int) ((X + randomPixelX) & m_textureSizeMask);
						if ( binaryPattern[centralX,centralY] != 0 )
							continue;	// Already filled up!

						filterValue = 0.0f;
						for ( int dY=-KERNEL_HALF_SIZE; dY <= KERNEL_HALF_SIZE; dY++ ) {
							for ( int dX=-KERNEL_HALF_SIZE; dX <= KERNEL_HALF_SIZE; dX++ ) {
								finalX = (uint) (m_textureSize + centralX + dX) & m_textureSizeMask;
								finalY = (uint) (m_textureSize + centralY + dY) & m_textureSizeMask;

								kernelValue = (float) Math.Exp( -(dX*dX + dY*dY) * _standardDeviation );
								filterValue += binaryPattern[finalX,finalY] * kernelValue;
							}
						}

//						filterValue *= gaussianNormalizer;
						if ( filterValue < minFilterValue ) {
							minFilterValue = filterValue;
							bestPositionX = centralX;
							bestPositionY = centralY;
						}
					}
				}

				// 2.2) Place a 1 at the place where we found the maximum value and update the dithering array
				binaryPattern[bestPositionX,bestPositionY] = 1;
				m_ditheringArray[bestPositionX,bestPositionY] = (float) iterationIndex / m_textureTotalSize;
//				m_ditheringArray[bestPositionX,bestPositionY] = 1;

				if ( _progress != null && (iterationIndex % progressNotificationInterval) == 0 )
					_progress( (float) iterationIndex / m_textureTotalSize, m_ditheringArray, null );
			}

			// One last notification
			if ( _progress != null )
				_progress( 1.0f, m_ditheringArray, null );
		}

		#endregion

		/// <summary>
		/// Returns a value in [0,_maxValue[
		/// </summary>
		/// <param name="_maxValue"></param>
		/// <returns></returns>
		uint	GetUniformInt( uint _maxValue ) {
			ulong	value = (ulong) _maxValue * (ulong) SimpleRNG.GetUint();
					value >>= 32;
			return (uint) value;
		}
	}
}
