using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using WMath;

namespace StandardizedDiffuseAlbedoMaps
{
	/// <summary>
	/// This class hosts the camera calibration database
	/// </summary>
	public class CameraCalibrationDatabase
	{
		#region CONSTANTS

		private const int	REQUIRED_PROBES_COUNT = 6;	// We're expecting 6 probes in the camera calibration files

		#endregion

		#region NESTED TYPES

		[System.Diagnostics.DebuggerDisplay( "ISO={m_EV_ISOSpeed} Shutter={m_EV_ShutterSpeed} Aperture={m_EV_Aperture} EV={EV}" )]
		private class	GridNode
		{
			public CameraCalibration	m_CameraCalibration = null;

			public float				m_EV_ISOSpeed;
			public float				m_EV_ShutterSpeed;
			public float				m_EV_Aperture;

			// The 6 possible neighbors for this node
			public GridNode[][]			m_Neighbors = new GridNode[3][] {	new GridNode[2],	// X left/right
																			new GridNode[2],	// Y left/right
																			new GridNode[2]		// Z left/right
																		};

			/// <summary>
			/// Gets the global EV for this node
			/// </summary>
			public float	EV	{ get { return m_EV_ISOSpeed + m_EV_ShutterSpeed + m_EV_Aperture; } }

			public	GridNode( CameraCalibration _CameraCalibration )
			{
				if ( _CameraCalibration == null )
					throw new Exception( "Invalid camera calibration to build grid node!" );

				m_CameraCalibration = _CameraCalibration;

				// Build normalized EV infos
				Convert2EV( m_CameraCalibration.m_CameraShotInfos.m_ISOSpeed,
							m_CameraCalibration.m_CameraShotInfos.m_ShutterSpeed,
							m_CameraCalibration.m_CameraShotInfos.m_Aperture,
							out m_EV_ISOSpeed, out m_EV_ShutterSpeed, out m_EV_Aperture );
			}

			/// <summary>
			/// Computes the square distance of the current node and provided node
			/// </summary>
			/// <param name="_Node"></param>
			/// <returns></returns>
			public float	SqDistance( GridNode _Node )
			{
				float	Delta_ISOSpeed = _Node.m_EV_ISOSpeed - m_EV_ISOSpeed;
				float	Delta_ShutterSpeed = _Node.m_EV_ShutterSpeed - m_EV_ShutterSpeed;
				float	Delta_Aperture = _Node.m_EV_Aperture - m_EV_Aperture;
				return Delta_ISOSpeed*Delta_ISOSpeed + Delta_ShutterSpeed*Delta_ShutterSpeed + Delta_Aperture*Delta_Aperture;
			}

			/// <summary>
			/// Converts to normalized EV infos
			/// </summary>
			/// <param name="_ISOSpeed"></param>
			/// <param name="_ShutterSpeed"></param>
			/// <param name="_Aperture"></param>
			/// <param name="_EV_ISOSpeed"></param>
			/// <param name="_EV_ShutterSpeed"></param>
			/// <param name="_EV_Aperture"></param>
			public static void	Convert2EV( float _ISOSpeed, float _ShutterSpeed, float _Aperture, out float _EV_ISOSpeed, out float _EV_ShutterSpeed, out float _EV_Aperture )
			{
				_EV_ISOSpeed = (float) (Math.Log( _ISOSpeed / 100.0f ) / Math.Log(2.0));	// 100 ISO = 0 EV, 200 = +1 EV, 400 = +2 EV, etc.
				_EV_ShutterSpeed = (float) (Math.Log( _ShutterSpeed ) / Math.Log(2.0));		// 1s = 0 EV, 2s = +1 EV, 0.5s = -1 EV, etc.
				_EV_Aperture = (float) (-Math.Log( _Aperture ) / Math.Log(2.0));			// f/1.0 = 0 EV, f/1.4 = -0.5 EV, f/2.0 = -1 EV, etc.
			}
		}

		#endregion

		#region FIELDS

		private System.IO.DirectoryInfo		m_DatabasePath = null;
		private string						m_ErrorLog = "";

		// The list of camera calibration data contained in the database
		private CameraCalibration[]			m_CameraCalibrations = new CameraCalibration[0];

		// The correction factor to apply to the image's luminances
		// If the camera has been properly calibrated but the lighting changes, the user
		//	has the possibility to drop the white reflectance in an image and use it as
		//	white reference for the new lighting condition
		private float3						m_WhiteReflectanceReference = new float3( 0, 0, -1 );	// Not supplied by the user
		private float						m_WhiteReflectanceCorrectionFactor = 1.0f;				// Default factor is no change at all

		// White reference image to apply minor luminance corrections to pixels (spatial discrepancy in lighting compensation)
		// I assume the provided image has been properly generated and normalized (i.e. it has a white maximum of 1)
		private Bitmap2						m_WhiteReferenceImage = null;
		private float						m_WhiteReflectanceImageMax = 1.0f;			// Maximum luminance in the white reference image

		// Generated calibration grid
		private GridNode					m_RootNode = null;

		// Cached calibration data
		private CameraCalibration			m_InterpolatedCalibration = null;

		#endregion

		#region PROPERTIES

		/// <summary>
		/// Sets the path where to find all the camera calibration files that compose the database
		/// Setting the property will trigger a database rebuild
		/// </summary>
		public System.IO.DirectoryInfo		DatabasePath
		{
			get { return m_DatabasePath; }
			set
			{
				if ( m_DatabasePath != null )
				{	// Clean up existing database
					m_CameraCalibrations = new CameraCalibration[0];
					m_InterpolatedCalibration = null;
					m_RootNode = null;
					m_ErrorLog = "";
				}

				m_DatabasePath = value;

				if ( m_DatabasePath == null )
					return;

				// Setup new database
				if ( !m_DatabasePath.Exists )
					throw new Exception( "Provided database path doesn't exist!" );

				//////////////////////////////////////////////////////////////////////////
				// Collect all calibration files
				System.IO.FileInfo[]	CalibrationFiles = m_DatabasePath.GetFiles( "*.xml" );
				List<CameraCalibration>	CameraCalibrations = new List<CameraCalibration>();
				List<GridNode>			GridNodes = new List<GridNode>();
				foreach ( System.IO.FileInfo CalibrationFile in CalibrationFiles )
				{
					try
					{
						// Attempt to load the camera calibration file
						CameraCalibration	CC = new CameraCalibration();
						CC.Load( CalibrationFile );
						if ( CC.m_Reflectances.Length != REQUIRED_PROBES_COUNT )
							throw new Exception( "Unexpected amount of reflectance probes in calibration file! (" + REQUIRED_PROBES_COUNT + " required)" );

						// Attempt to create a valid grid node from it
						GridNode	Node = new GridNode( CC );
						if ( m_RootNode == null || Node.EV < m_RootNode.EV )
							m_RootNode = Node;	// Found a better root

						// If everything went well, add the new data
						CameraCalibrations.Add( CC );
						GridNodes.Add( Node );
					}
					catch ( Exception _e )
					{
						m_ErrorLog += "Failed to load camera calibration file \"" + CalibrationFile.FullName + "\": " + _e.Message + "\r\n";
					}
				}
				m_CameraCalibrations = CameraCalibrations.ToArray();

				if ( m_CameraCalibrations.Length == 0 )
				{	// Empty!
					m_ErrorLog += "Database is empty: no valid file could be parsed...\r\n";
					return;
				}

				//////////////////////////////////////////////////////////////////////////
				// Build the calibration grid
				// The idea is to build a 3D grid of camera calibration settings, each dimension of the grid is:
				//	_ X = ISO Speed
				//	_ Y = Shutter Speed
				//	_ Z = Aperture
				//
				// Each grid node has neighbors to the previous/next EV value along the specific dimension.
				// For example, following the "next" X neighbor, you will increase the EV on the ISO speed parameter.
				// For example, following the "previous" Y neighbor, you will decrease the EV on the Shutter speed parameter.
				// For example, following the "next" Z neighbor, you will increase the EV on the Aperture parameter.
				//
				// You should imagine the grid as a 3D texture, except the voxels of the texture are not regularly spaced
				//	but can be placed freely in the volume. The grid only maintains the coherence from one voxel to another.
				//
				List<GridNode>	PlacedNodes = new List<GridNode>();
				PlacedNodes.Add( m_RootNode );
				GridNodes.Remove( m_RootNode );

				while ( GridNodes.Count > 0 )
				{
					// The algorithm is simple:
					//	_ While there are still unplaced nodes
					//		_ For each pair of (placed,unplaced) grid nodes
					//			_ If the pair is closer than current pair, then make it the new current pair
					//		_ If the largest EV discrepancy is ISO speed then store unplaced node on X axis (either previous or next depending on sign of discrepancy)
					//		_ If the largest EV discrepancy is shutter speed then store unplaced node on Y axis
					//		_ If the largest EV discrepancy is aperture then store unplaced node on Z axis (either previous or next depending on sign of discrepancy)
					//
					GridNode	PairPlaced = null;
					GridNode	PairUnPlaced = null;
					float		BestPairSqDistance = float.MaxValue;
					foreach ( GridNode NodePlaced in PlacedNodes )
						foreach ( GridNode NodeUnPlaced in GridNodes )
						{
							float	SqDistance = NodePlaced.SqDistance( NodeUnPlaced );
							if ( SqDistance < BestPairSqDistance )
							{	// Found new best pair!
								BestPairSqDistance = SqDistance;
								PairPlaced = NodePlaced;
								PairUnPlaced = NodeUnPlaced;
							}
						}

					// So now we know a new neighbor for the placed node
					// We need to know on which axis and which direction...
					float	DeltaX = PairUnPlaced.m_EV_ISOSpeed - PairPlaced.m_EV_ISOSpeed;
					float	DeltaY = PairUnPlaced.m_EV_ShutterSpeed - PairPlaced.m_EV_ShutterSpeed;
					float	DeltaZ = PairUnPlaced.m_EV_Aperture - PairPlaced.m_EV_Aperture;

					int			AxisIndex = -1;
					int			LeftRight = -1;
					if ( Math.Abs( DeltaX ) > Math.Abs( DeltaY ) )
					{
						if ( Math.Abs( DeltaX ) > Math.Abs( DeltaZ ) )
						{	// Place along X
							AxisIndex = 0;
							LeftRight = DeltaX < 0.0f ? 0 : 1;
						}
						else
						{	// Place along Z
							AxisIndex = 2;
							LeftRight = DeltaZ < 0.0f ? 0 : 1;
						}
					}
					else
					{
						if ( Math.Abs( DeltaY ) > Math.Abs( DeltaZ ) )
						{	// Place along Y
							AxisIndex = 1;
							LeftRight = DeltaY < 0.0f ? 0 : 1;
						}
						else
						{	// Place along Z
							AxisIndex = 2;
							LeftRight = DeltaZ < 0.0f ? 0 : 1;
						}
					}

					// Register each node in the pair as neighbors along the selected axis
					GridNode	FormerNeighbor = PairPlaced.m_Neighbors[AxisIndex][LeftRight];
					PairPlaced.m_Neighbors[AxisIndex][LeftRight] = PairUnPlaced;
					PairUnPlaced.m_Neighbors[AxisIndex][1-LeftRight] = PairPlaced;

					if ( FormerNeighbor != null )
					{	// Re-link with former neighbor
						PairUnPlaced.m_Neighbors[AxisIndex][LeftRight] = FormerNeighbor;
						FormerNeighbor.m_Neighbors[AxisIndex][1-LeftRight] = PairUnPlaced;
					}

					// Remove the node from unplaced nodes & add it to placed ones
					GridNodes.Remove( PairUnPlaced );
					PlacedNodes.Add( PairUnPlaced );
				}
			}
		}

		/// <summary>
		/// Tells if the database is valid
		/// </summary>
		public bool		IsValid					{ get { return m_RootNode != null; } }

		/// <summary>
		/// Gets or sets the white reflectance reference to use for the image
		/// Setting a positive value will assume a new white reference for the image and a correction factor
		///  will be computed and applied to all luminances read from the provided images
		/// Set to a negative value to reset the correction factor to normal
		/// </summary>
		public float3	WhiteReflectanceReference
		{
			get { return m_WhiteReflectanceReference; }
			set
			{
				m_WhiteReflectanceReference = value;
				if ( value.z <= 1e-6f || m_InterpolatedCalibration == null )
					m_WhiteReflectanceCorrectionFactor = 1.0f;	// Reset
				else
				{	// Compute the correction factor
					float	NormalReflectance = m_InterpolatedCalibration.m_Reflectance99.m_LuminanceMeasured;	// Our normal 99% reflectance to use as white
					m_WhiteReflectanceCorrectionFactor = NormalReflectance / m_WhiteReflectanceReference.z;
				}
			}
		}
 
		/// <summary>
		/// Gets or sets the white reference image to use for spatial luminance correction
		/// </summary>
		public Bitmap2	WhiteReferenceImage
		{
			get { return m_WhiteReferenceImage; }
			set
			{
				m_WhiteReferenceImage = value;
				m_WhiteReflectanceImageMax = 1.0f;
				if ( m_WhiteReferenceImage == null )
					return;

				// Compute the maximum luminance in the white reference image
				m_WhiteReflectanceImageMax = 0.0f;
				for ( int Y=0; Y < m_WhiteReferenceImage.Height; Y++ )
					for ( int X=0; X < m_WhiteReferenceImage.Width; X++ )
						m_WhiteReflectanceImageMax = Math.Max( m_WhiteReflectanceImageMax, m_WhiteReferenceImage.ContentXYZ[X,Y].y );
			}
		}

		/// <summary>
		/// Gets the white reflectance correction factor applied to all luminances read from any provided image
		/// This factor is updated by setting the WhiteReflectanceReference property
		/// </summary>
		public float	WhiteReflectanceCorrectionFactor
		{
			get { return m_WhiteReflectanceCorrectionFactor; }
		}

		/// <summary>
		/// Tells if there were some errors during construction
		/// </summary>
		public bool		HasErrors				{ get { return m_ErrorLog != ""; } }

		/// <summary>
		/// Shows error during database construction if not ""
		/// </summary>
		public string	ErrorLog				{ get { return m_ErrorLog; } }

		public float	PreparedForISOSpeed		{ get { return m_InterpolatedCalibration != null ? m_InterpolatedCalibration.m_CameraShotInfos.m_ISOSpeed : -1.0f; } }
		public float	PreparedForShutterSpeed	{ get { return m_InterpolatedCalibration != null ? m_InterpolatedCalibration.m_CameraShotInfos.m_ShutterSpeed : -1.0f; } }
		public float	PreparedForAperture		{ get { return m_InterpolatedCalibration != null ? m_InterpolatedCalibration.m_CameraShotInfos.m_Aperture : -1.0f; } }

		public CameraCalibration	InterpolatedCalibration	{ get { return m_InterpolatedCalibration; } }

		#endregion

		#region METHODS

		/// <summary>
		/// Prepares the interpolated calibration table to process the pixels in an image shot with the specified shot infos
		/// </summary>
		/// <param name="_Image"></param>
		public void	PrepareCalibrationFor( Bitmap2 _Image )
		{
			if ( !_Image.HasValidShotInfo )
				throw new Exception( "Can't prepare calibration for specified image since it doesn't have valid shot infos!" );

			PrepareCalibrationFor( _Image.ISOSpeed, _Image.ShutterSpeed, _Image.Aperture );
		}

		/// <summary>
		/// Prepares the interpolated calibration table to process the pixels in an image shot with the specified shot infos
		/// </summary>
		/// <param name="_ISOSpeed"></param>
		/// <param name="_ShutterSpeed"></param>
		/// <param name="_Aperture"></param>
		public void	PrepareCalibrationFor( float _ISOSpeed, float _ShutterSpeed, float _Aperture )
		{
			if ( m_RootNode == null )
				throw new Exception( "Calibration grid hasn't been built: did you provide a valid database path? Does the path contain camera calibration data?" );

			if ( IsPreparedFor( _ISOSpeed, _ShutterSpeed, _Aperture ) )
				return;	// Already prepared!

			//////////////////////////////////////////////////////////////////////////
			// Find the 8 nodes encompassing our values
			// I'm making the delicate assumption that, although the starting node is chosen on the
			//	condition its EV values are strictly inferior to the target we're looking for, all
			//	neighbor nodes should satisfy the condition they're properly placed.
			//
			// This is true for the direct neighbors +X, +Y, +Z that are immediately above target values
			//	but for example, neighbor (+X +Y) may have a very bad aperture value (Z) that may be
			//	above the target aperture...
			//
			// Let's hope the user won't provide too fancy calibrations...
			// (anyway, interpolants are clamped in [0,1] so there's no risk of overshooting)
			//
			float3	EV;
			GridNode.Convert2EV( _ISOSpeed, _ShutterSpeed, _Aperture, out EV.x, out EV.y, out EV.z );

			// Find the start node
			GridNode		StartNode = FindStartNode( EV.x, EV.y, EV.z );

			// Build the 8 grid nodes from it
			GridNode[,,]	Grid = new GridNode[2,2,2];
			Grid[0,0,0] = StartNode;
			Grid[1,0,0] = StartNode.m_Neighbors[0][1] != null ? StartNode.m_Neighbors[0][1] : StartNode;		// +X
			Grid[0,1,0] = StartNode.m_Neighbors[1][1] != null ? StartNode.m_Neighbors[1][1] : StartNode;		// +Y
			Grid[0,0,1] = StartNode.m_Neighbors[2][1] != null ? StartNode.m_Neighbors[2][1] : StartNode;		// +Z
			Grid[1,1,0] = Grid[1,0,0].m_Neighbors[1][1] != null ? Grid[1,0,0].m_Neighbors[1][1] : Grid[1,0,0];	// +X +Y
			Grid[0,1,1] = Grid[0,1,0].m_Neighbors[2][1] != null ? Grid[0,1,0].m_Neighbors[2][1] : Grid[0,1,0];	// +Y +Z
			Grid[1,0,1] = Grid[0,0,1].m_Neighbors[0][1] != null ? Grid[0,0,1].m_Neighbors[0][1] : Grid[0,0,1];	// +X +Z
			Grid[1,1,1] = Grid[1,1,0].m_Neighbors[2][1] != null ? Grid[1,1,0].m_Neighbors[2][1] : Grid[1,1,0];	// +X +Y +Z

			//////////////////////////////////////////////////////////////////////////
			// Create the successive interpolants for trilinear interpolation
			//
			// Assume we interpolate on X first (ISO speed), so we need 4 distinct values
			float4	tX = new float4(
					Math.Max( 0.0f, Math.Min( 1.0f, (EV.x - Grid[0,0,0].m_EV_ISOSpeed) / Math.Max( 1e-6f, Grid[1,0,0].m_EV_ISOSpeed - Grid[0,0,0].m_EV_ISOSpeed) ) ),	// Y=0 Z=0
					Math.Max( 0.0f, Math.Min( 1.0f, (EV.x - Grid[0,1,0].m_EV_ISOSpeed) / Math.Max( 1e-6f, Grid[1,1,0].m_EV_ISOSpeed - Grid[0,1,0].m_EV_ISOSpeed) ) ),	// Y=1 Z=0
					Math.Max( 0.0f, Math.Min( 1.0f, (EV.x - Grid[0,0,1].m_EV_ISOSpeed) / Math.Max( 1e-6f, Grid[1,0,1].m_EV_ISOSpeed - Grid[0,0,1].m_EV_ISOSpeed) ) ),	// Y=0 Z=1
					Math.Max( 0.0f, Math.Min( 1.0f, (EV.x - Grid[0,1,1].m_EV_ISOSpeed) / Math.Max( 1e-6f, Grid[1,1,1].m_EV_ISOSpeed - Grid[0,1,1].m_EV_ISOSpeed) ) )	// Y=1 Z=1
				);
			float4	rX = new float4( 1.0f - tX.x, 1.0f - tX.y, 1.0f - tX.z, 1.0f - tX.w );

				// Compute the 4 interpolated shutter speeds & apertures
			float4	ShutterSpeedsX = new float4(
					rX.x * Grid[0,0,0].m_EV_ShutterSpeed + tX.x * Grid[1,0,0].m_EV_ShutterSpeed,	// Y=0 Z=0
					rX.y * Grid[0,1,0].m_EV_ShutterSpeed + tX.y * Grid[1,1,0].m_EV_ShutterSpeed,	// Y=1 Z=0
					rX.z * Grid[0,0,1].m_EV_ShutterSpeed + tX.z * Grid[1,0,1].m_EV_ShutterSpeed,	// Y=0 Z=1
					rX.w * Grid[0,1,1].m_EV_ShutterSpeed + tX.w * Grid[1,1,1].m_EV_ShutterSpeed		// Y=1 Z=1
				);
			float4	AperturesX = new float4(
					rX.x * Grid[0,0,0].m_EV_Aperture + tX.x * Grid[1,0,0].m_EV_Aperture,			// Y=0 Z=0
					rX.y * Grid[0,1,0].m_EV_Aperture + tX.y * Grid[1,1,0].m_EV_Aperture,			// Y=1 Z=0
					rX.z * Grid[0,0,1].m_EV_Aperture + tX.z * Grid[1,0,1].m_EV_Aperture,			// Y=0 Z=1
					rX.w * Grid[0,1,1].m_EV_Aperture + tX.w * Grid[1,1,1].m_EV_Aperture				// Y=1 Z=1
				);

			// Next we interpolate on Y (Shutter speed), so we need 2 distinct values
			float2	tY = new float2(
					Math.Max( 0.0f, Math.Min( 1.0f, (EV.y - ShutterSpeedsX.x) / Math.Max( 1e-6f, ShutterSpeedsX.y - ShutterSpeedsX.x) ) ),	// Z=0
					Math.Max( 0.0f, Math.Min( 1.0f, (EV.y - ShutterSpeedsX.z) / Math.Max( 1e-6f, ShutterSpeedsX.w - ShutterSpeedsX.z) ) )	// Z=1
				);
			float2	rY = new float2( 1.0f - tY.x, 1.0f - tY.y );

				// Compute the 2 apertures
			float2	AperturesY = new float2(
					rY.x * AperturesX.x + tY.x * AperturesX.y,
					rY.y * AperturesX.z + tY.y * AperturesX.w
				);

			// Finally, we interpolate on Z (Aperture), we need only 1 single value
			float	tZ = Math.Max( 0.0f, Math.Min( 1.0f, (EV.z - AperturesY.x) / Math.Max( 1e-6f, AperturesY.y - AperturesY.x) ) );
			float	rZ = 1.0f - tZ;


			//////////////////////////////////////////////////////////////////////////
			// Create the special camera calibration that is the result of the interpolation of the 8 nearest ones in the grid
			m_InterpolatedCalibration = new CameraCalibration();
			m_InterpolatedCalibration.m_CameraShotInfos.m_ISOSpeed = _ISOSpeed;
			m_InterpolatedCalibration.m_CameraShotInfos.m_ShutterSpeed = _ShutterSpeed;
			m_InterpolatedCalibration.m_CameraShotInfos.m_Aperture = _Aperture;

			for ( int ProbeIndex=0; ProbeIndex < REQUIRED_PROBES_COUNT; ProbeIndex++ )
			{
				CameraCalibration.Probe TargetProbe = m_InterpolatedCalibration.m_Reflectances[ProbeIndex];

				float	L000 = Grid[0,0,0].m_CameraCalibration.m_Reflectances[ProbeIndex].m_LuminanceMeasured;
				float	L100 = Grid[1,0,0].m_CameraCalibration.m_Reflectances[ProbeIndex].m_LuminanceMeasured;
				float	L010 = Grid[0,1,0].m_CameraCalibration.m_Reflectances[ProbeIndex].m_LuminanceMeasured;
				float	L110 = Grid[1,1,0].m_CameraCalibration.m_Reflectances[ProbeIndex].m_LuminanceMeasured;
				float	L001 = Grid[0,0,1].m_CameraCalibration.m_Reflectances[ProbeIndex].m_LuminanceMeasured;
				float	L101 = Grid[1,0,1].m_CameraCalibration.m_Reflectances[ProbeIndex].m_LuminanceMeasured;
				float	L011 = Grid[0,1,1].m_CameraCalibration.m_Reflectances[ProbeIndex].m_LuminanceMeasured;
				float	L111 = Grid[1,1,1].m_CameraCalibration.m_Reflectances[ProbeIndex].m_LuminanceMeasured;

				// Interpolate on X (ISO speed)
				float	L00 = rX.x * L000 + tX.x * L100;
				float	L10 = rX.y * L010 + tX.y * L110;
				float	L01 = rX.z * L001 + tX.z * L101;
				float	L11 = rX.w * L011 + tX.w * L111;

				// Interpolate on Y (shutter speed)
				float	L0 = rY.x * L00 + tY.x * L10;
				float	L1 = rY.y * L01 + tY.y * L11;

				// Interpolate on Z (aperture)
				float	L = rZ * L0 + tZ * L1;

				TargetProbe.m_IsAvailable = true;
				TargetProbe.m_LuminanceMeasured = L;
			}

			// Fill missing values
			m_InterpolatedCalibration.UpdateAllLuminances();

			// Reset white reflectance reference because it was set for another setup
			WhiteReflectanceReference = new float3( 0, 0, -1 );
		}

		/// <summary>
		/// Tells if the database is prepared and can be used for processing colors of an image with the specified shot infos
		/// </summary>
		/// <param name="_ISOSpeed"></param>
		/// <param name="_ShutterSpeed"></param>
		/// <param name="_Aperture"></param>
		/// <returns></returns>
		public bool	IsPreparedFor( float _ISOSpeed, float _ShutterSpeed, float _Aperture )
		{
			return m_InterpolatedCalibration != null
				&& Math.Abs( _ISOSpeed - m_InterpolatedCalibration.m_CameraShotInfos.m_ISOSpeed ) < 1e-6f
				&& Math.Abs( _ShutterSpeed - m_InterpolatedCalibration.m_CameraShotInfos.m_ShutterSpeed ) < 1e-6f
				&& Math.Abs( _Aperture - m_InterpolatedCalibration.m_CameraShotInfos.m_Aperture ) < 1e-6f;
		}

		/// <summary>
		/// Calibrates a raw luminance value
		/// </summary>
		/// <param name="_Luminance">The uncalibrated luminance value</param>
		/// <returns>The calibrated reflectance value</returns>
		/// <remarks>Typically, you start from a RAW XYZ value that you convert to xyY, pass the Y to this method
		/// and replace it into your orignal xyY, convert back to XYZ and voilà!</remarks>
		public float	Calibrate( float _Luminance )
		{
			if ( m_RootNode == null )
				throw new Exception( "Calibration grid hasn't been built: did you provide a valid database path? Does the path contain camera calibration data?" );
			if ( m_InterpolatedCalibration == null )
				throw new Exception( "Calibration grid hasn't been prepared for calibration: did you call the PrepareCalibrationFor() method?" );

			float	Reflectance = m_InterpolatedCalibration.Calibrate( m_WhiteReflectanceCorrectionFactor * _Luminance );
			return Reflectance;
		}

		/// <summary>
		/// Calibrates a raw luminance value with spatial luminance correction
		/// </summary>
		/// <param name="_U">The U coordinate in the image (U=X/Width)</param>
		/// <param name="_V">The V coordinate in the image (V=Y/Height)</param>
		/// <param name="_Luminance">The uncalibrated luminance value</param>
		/// <returns>The calibrated reflectance value</returns>
		/// <remarks>Typically, you start from a RAW XYZ value that you convert to xyY, pass the Y to this method
		/// and replace it into your orignal xyY, convert back to XYZ and voilà!</remarks>
		public float	CalibrateWithSpatialCorrection( float _U, float _V, float _Luminance )
		{
			if ( m_RootNode == null )
				throw new Exception( "Calibration grid hasn't been built: did you provide a valid database path? Does the path contain camera calibration data?" );
			if ( m_InterpolatedCalibration == null )
				throw new Exception( "Calibration grid hasn't been prepared for calibration: did you call the PrepareCalibrationFor() method?" );
			
			float	CorrectionFactor = m_WhiteReflectanceCorrectionFactor;
					CorrectionFactor *= GetSpatialLuminanceCorrectionFactor( _U, _V );	// Apply spatial correction

			float	Reflectance = m_InterpolatedCalibration.Calibrate( CorrectionFactor * _Luminance );
			return Reflectance;
		}

		/// <summary>
		/// Uses the white reference image to retrieve the luminance factor to apply based on the position in the image
		/// </summary>
		/// <param name="_U">The U coordinate in the image (U=X/Width)</param>
		/// <param name="_V">The V coordinate in the image (V=Y/Height)</param>
		/// <returns>The luminance factor to apply to correct the spatial luminance discrepancies</returns>
		public float	GetSpatialLuminanceCorrectionFactor( float _U, float _V )
		{
			if ( m_WhiteReferenceImage == null )
				return 1.0f;

			float4	XYZ = m_WhiteReferenceImage.BilinearSample(_U * m_WhiteReferenceImage.Width, _V * m_WhiteReferenceImage.Height );
			float	SpatialWhiteRefCorrection = m_WhiteReflectanceImageMax / Math.Max( 1e-6f, XYZ.y );
			return SpatialWhiteRefCorrection;
		}

		/// <summary>
		/// Finds the first grid node whose ISO, shutter speed and aperture values are immediately inferior to the ones provided
		/// </summary>
		/// <param name="_EV_ISOSpeed"></param>
		/// <param name="_EV_ShutterSpeed"></param>
		/// <param name="_EV_Aperture"></param>
		/// <returns></returns>
		private GridNode	FindStartNode( float _EV_ISOSpeed, float _EV_ShutterSpeed, float _EV_Aperture )
		{
			GridNode	Current = m_RootNode;

			// Move along X
			while ( Current.m_EV_ISOSpeed <= _EV_ISOSpeed && Current.m_Neighbors[0][1] != null )
			{
				GridNode	Next = Current.m_Neighbors[0][1];
				if ( Next.m_EV_ISOSpeed > _EV_ISOSpeed )
					break;	// Next node is larger than provided value! We have our start node along X!
				Current = Next;
			}

			// Move along Y
			while ( Current.m_EV_ShutterSpeed <= _EV_ShutterSpeed && Current.m_Neighbors[1][1] != null )
			{
				GridNode	Next = Current.m_Neighbors[1][1];
				if ( Next.m_EV_ShutterSpeed > _EV_ShutterSpeed )
					break;	// Next node is larger than provided value! We have our start node along Y!
				Current = Next;
			}

			// Move along Z
			while ( Current.m_EV_Aperture <= _EV_Aperture && Current.m_Neighbors[2][1] != null )
			{
				GridNode	Next = Current.m_Neighbors[2][1];
				if ( Next.m_EV_Aperture > _EV_Aperture )
					break;	// Next node is larger than provided value! We have our start node along Z!
				Current = Next;
			}

			return Current;
		}

		#endregion
	}
}
