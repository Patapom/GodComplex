#define	LOAD_OCTREE
 
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
using ImageUtility;
using Renderer;

namespace VoxelConeTracing
{
	public class OctreeBuilder : IDisposable {

		struct	Voxel {
			public uint		X, Y, Z;
			public float3	albedo;
			public float3	normal;
		}

		public OctreeBuilder( Device _device, float3 _wsCornerMin, float3 _wsCornerMax, uint _volumeSize ) {

			//////////////////////////////////////////////////////////////////////////
			// 1] Generate a list of non-empty voxels
			//
			#if !LOAD_OCTREE
				// Start collecting all non-empty voxels
				float3	dV = (_wsCornerMax - _wsCornerMin) / _volumeSize;
				float3	wsVoxelMin = _wsCornerMin + 0.5f * dV;	// Start voxel center
				float	voxelDistance = (float) Math.Sqrt( dV.x*dV.x + dV.y*dV.y + dV.z*dV.z );

				List< Voxel >	voxels = new List<Voxel>( 10000000 );	// 10 million voxels

				DateTime	buildStartTime = DateTime.Now;

				float3	voxelCenter = new float3();
						voxelCenter.z = wsVoxelMin.z;
				for ( uint Z=0; Z < _volumeSize; Z++, voxelCenter.z += dV.z ) {
					voxelCenter.y = wsVoxelMin.y;
					for ( uint Y=0; Y < _volumeSize; Y++, voxelCenter.y += dV.y ) {
						voxelCenter.x = wsVoxelMin.x;
						for ( uint X=0; X < _volumeSize; X++, voxelCenter.x += dV.x ) {
							float2	sceneDistance = Map( voxelCenter );
							if ( sceneDistance.x > voxelDistance )
								continue;	// Scene is too far away

							float3	sceneNormal = Normal( voxelCenter );
							float3	sceneAlbedo = Albedo( voxelCenter, sceneDistance.y );

							voxels.Add( new Voxel() {
								X = X, Y = Y, Z = Z,
								albedo = sceneAlbedo,
								normal = sceneNormal
							} );
						}
					}
				}

				DateTime	buildEndTime = DateTime.Now;

				System.Diagnostics.Debug.WriteLine( "Octree build time = " + (buildEndTime - buildStartTime).TotalSeconds + " seconds" );

				// Write to disk as it takes hell of a time to generate!
				using ( System.IO.FileStream S = new System.IO.FileInfo( "CornellBox_" + _volumeSize + ".voxels" ).Create() )
					using ( System.IO.BinaryWriter W = new System.IO.BinaryWriter( S ) ) {
						W.Write( _wsCornerMin.x );
						W.Write( _wsCornerMin.y );
						W.Write( _wsCornerMin.z );
						W.Write( _wsCornerMax.x );
						W.Write( _wsCornerMax.y );
						W.Write( _wsCornerMax.z );
						W.Write( _volumeSize );
						W.Write( voxels.Count );
						foreach ( Voxel V in voxels ) {
							W.Write( V.X );
							W.Write( V.Y );
							W.Write( V.Z );
							W.Write( V.albedo.x );
							W.Write( V.albedo.y );
							W.Write( V.albedo.z );
							W.Write( V.normal.x );
							W.Write( V.normal.y );
							W.Write( V.normal.z );
						}
					}

			#else
				// Read from disk as it takes hell of a time to generate!
				List< Voxel >	voxels = null;

				using ( System.IO.FileStream S = new System.IO.FileInfo( "CornellBox_" + _volumeSize + ".voxels" ).OpenRead() )
					using ( System.IO.BinaryReader R = new System.IO.BinaryReader( S ) ) {

						_wsCornerMin.x = R.ReadSingle();
						_wsCornerMin.y = R.ReadSingle();
						_wsCornerMin.z = R.ReadSingle();
						_wsCornerMax.x = R.ReadSingle();
						_wsCornerMax.y = R.ReadSingle();
						_wsCornerMax.z = R.ReadSingle();
						_volumeSize = R.ReadUInt32();
						int	voxelsCount = (int) R.ReadUInt32();
						voxels = new List<Voxel>( voxelsCount );

						Voxel	V = new Voxel();
						for ( int voxelIndex=0; voxelIndex < voxelsCount; voxelIndex++ ) {
							V.X = R.ReadUInt32();
							V.Y = R.ReadUInt32();
							V.Z = R.ReadUInt32();
							V.albedo.x = R.ReadSingle();
							V.albedo.y = R.ReadSingle();
							V.albedo.z = R.ReadSingle();
							V.normal.x = R.ReadSingle();
							V.normal.y = R.ReadSingle();
							V.normal.z = R.ReadSingle();
							voxels.Add( V );
						}
					}
			#endif

			//////////////////////////////////////////////////////////////////////////
			// 2] Encode these voxels into an octree
// TODO!
		}

		#region Distance-Field Helpers

		static readonly float3	CORNELL_SIZE = new float3( 5.528f, 5.488f, 5.592f );
		static readonly float3	CORNELL_POS = float3.Zero;
		const float				CORNELL_THICKNESS = 0.1f;

		static readonly float3	CORNELL_SMALL_BOX_SIZE = new float3( 1.65f, 1.65f, 1.65f );	// It's a cube
		static readonly float3	CORNELL_SMALL_BOX_POS = new float3( 1.855f, 0.5f * CORNELL_SMALL_BOX_SIZE.y, 1.69f ) - 0.5f * new float3( CORNELL_SIZE.x, 0, CORNELL_SIZE.z );
		const float				CORNELL_SMALL_BOX_ANGLE = 0.29145679447786709199560462143289f;	// ~16°

		static readonly float3	CORNELL_LARGE_BOX_SIZE = new float3( 1.65f, 3.3f, 1.65f );
		static readonly float3	CORNELL_LARGE_BOX_POS = new float3( 3.685f, 0.5f * CORNELL_LARGE_BOX_SIZE.y, 3.5125f ) - 0.5f * new float3( CORNELL_SIZE.x, 0, CORNELL_SIZE.z );
		const float				CORNELL_LARGE_BOX_ANGLE = -0.30072115015043337195437489062082f;	// ~17°

		float	DistBox( float3 _wsPosition, float3 _wsBoxCenter, float3 _wsBoxSize ) {
			_wsPosition -= _wsBoxCenter;
			_wsBoxSize *= 0.5f;

			float x = Math.Max( _wsPosition.x - _wsBoxSize.x, -_wsPosition.x - _wsBoxSize.x );
			float y = Math.Max( _wsPosition.y - _wsBoxSize.y, -_wsPosition.y - _wsBoxSize.y );
			float z = Math.Max( _wsPosition.z - _wsBoxSize.z, -_wsPosition.z - _wsBoxSize.z );
			return Math.Max( Math.Max( x, y ), z );
		}
		float	DistBox( float3 _wsPosition, float3 _wsBoxCenter, float3 _wsBoxSize, float _rotationAngle ) {
			float	s = (float) Math.Sin( _rotationAngle );
			float	c = (float) Math.Cos( _rotationAngle );
			float3	rotatedX = new float3( c, 0.0f, s );
			float3	rotatedZ = new float3( -s, 0.0f, c );

			_wsPosition -= _wsBoxCenter;
			_wsPosition = new float3( _wsPosition.Dot( rotatedX ), _wsPosition.y, _wsPosition.Dot( rotatedZ ) );
			return DistBox( _wsPosition, float3.Zero, _wsBoxSize );
		}
		float2	DistMin( float2 a, float2 b ) {
			return a.x < b.x ? a : b;
		}

		float3	Normal( float3 _wsPosition ) {
			const float	eps = 0.001f;

			_wsPosition.x += eps;
			float	Dx = Map( _wsPosition ).x;
			_wsPosition.x -= 2.0f * eps;
			Dx -= Map( _wsPosition ).x;
			_wsPosition.x += eps;

			_wsPosition.y += eps;
			float	Dy = Map( _wsPosition ).x;
			_wsPosition.y -= 2.0f * eps;
			Dy -= Map( _wsPosition ).x;
			_wsPosition.y += eps;

			_wsPosition.z += eps;
			float	Dz = Map( _wsPosition ).x;
			_wsPosition.z -= 2.0f * eps;
			Dz -= Map( _wsPosition ).x;
//			_wsPosition.x += eps;

// 			Dx *= 2.0f / eps;
// 			Dy *= 2.0f / eps;
// 			Dz *= 2.0f / eps;

			return new float3( Dx, Dy, Dz ).Normalized;
		}

		/// <summary>
		/// Maps a world-space position into a distance to the nearest object in the scene
		/// </summary>
		/// <param name="_wsPosition"></param>
		/// <returns></returns>
		float2	Map( float3 _wsPosition ) {
			// Walls
			float2	distance = new float2( DistBox( _wsPosition, float3.Zero, new float3( CORNELL_SIZE.x, CORNELL_THICKNESS, CORNELL_SIZE.z ) ), 1.0f );	// Floor
					distance = DistMin( distance, new float2( DistBox( _wsPosition, new float3( 0, CORNELL_SIZE.y, 0 ), new float3( CORNELL_SIZE.x, CORNELL_THICKNESS, CORNELL_SIZE.z ) ), 2.0f ) );	// Ceiling
					distance = DistMin( distance, new float2( DistBox( _wsPosition, new float3( -0.5f * CORNELL_SIZE.x, 0.5f * CORNELL_SIZE.y, 0 ), new float3( CORNELL_THICKNESS, CORNELL_SIZE.y, CORNELL_SIZE.z ) ), 3.0f ) );	// Left wall
					distance = DistMin( distance, new float2( DistBox( _wsPosition, new float3( 0.5f * CORNELL_SIZE.x, 0.5f * CORNELL_SIZE.y, 0 ), new float3( CORNELL_THICKNESS, CORNELL_SIZE.y, CORNELL_SIZE.z ) ), 4.0f ) );	// Right wall
					distance = DistMin( distance, new float2( DistBox( _wsPosition, new float3( 0, 0.5f * CORNELL_SIZE.y, 0.5f * CORNELL_SIZE.z ), new float3( CORNELL_SIZE.x, CORNELL_SIZE.y, CORNELL_THICKNESS ) ), 5.0f ) );	// Back wall

			// Small box
			distance = DistMin( distance, new float2( DistBox( _wsPosition, CORNELL_SMALL_BOX_POS, CORNELL_SMALL_BOX_SIZE, CORNELL_SMALL_BOX_ANGLE ), 6.0f ) );

			// Large box
			distance = DistMin( distance, new float2( DistBox( _wsPosition, CORNELL_LARGE_BOX_POS, CORNELL_LARGE_BOX_SIZE, CORNELL_LARGE_BOX_ANGLE ), 7.0f ) );

			return distance;
		}

		float3	Albedo( float3 _wsPosition, float _materialID ) {
			switch ( (int) _materialID ) {
				case 3: return 0.6f * new float3( 0.2f, 0.5f, 1.0f );
				case 4: return 0.6f * new float3( 1.0f, 0.1f, 0.01f );
			}

			return 0.6f * float3.One;
		}

		#endregion

		public void Dispose() {
		}
	}
}
