using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using WMath;

namespace FBX.SceneLoader.Objects
{
	public class	LoaderTempTransform : LoaderTempSceneObject
	{
		#region FIELDS

		// Pivot setup. The actual transform's matrix is the composition of the pivot with either the static matrix or the dynamic animation source matrix
		protected Matrix4x4			m_Pivot = null;

		// Static transform setup
		protected Matrix4x4			m_Matrix = null;
		protected Point				m_Position = null;
		protected Matrix3x3			m_Rotation = null;
		protected Quat				m_QuatRotation = null;
		protected Vector			m_Scale = null;

		// Dynamic transform setup
		protected bool							m_bAnimated = false;
		protected Matrix4x4						m_AnimationSourceMatrix = null;
		protected FBXImporter.AnimationTrack[]	m_AnimP = null;
		protected FBXImporter.AnimationTrack[]	m_AnimR = null;
		protected FBXImporter.AnimationTrack[]	m_AnimS = null;

		protected List<LoaderTempMesh>		m_Meshes = new List<LoaderTempMesh>();

		#endregion

		#region PROPERTIES

		public Matrix4x4	Pivot
		{
			get { return m_Pivot != null ? m_Pivot : new Matrix4x4().MakeIdentity(); }
			set { m_Pivot = value; }
		}

		/// <summary>
		/// The staic local transform
		/// </summary>
		public Matrix4x4	Matrix
		{
			get
			{
				if ( m_Matrix != null )
					return	Pivot * m_Matrix;	// Simple...

				// Otherwise, recompose matrix
				Matrix4x4	Result = new Matrix4x4();
							Result.MakeIdentity();

				// Setup the rotation part
				if ( m_Rotation != null )
					Result.SetRotation( m_Rotation );
				else if ( (m_QuatRotation as object) != null )
					Result = (Matrix4x4) m_QuatRotation;

				// Setup the scale part
				if ( m_Scale != null )
					Result.Scale( m_Scale );

				// Setup the translation part
				if ( m_Position != null )
					Result.SetTrans( m_Position );

				return	Pivot * Result;
			}
		}

		/// <summary>
		/// Tells if the transform is animated
		/// </summary>
		public bool			IsAnimated
		{
			get { return m_bAnimated; }
		}

		public FBXImporter.AnimationTrack[]	AnimationTrackPositions		{ get { return m_AnimP; } }
		public FBXImporter.AnimationTrack[]	AnimationTrackRotations		{ get { return m_AnimR; } }
		public FBXImporter.AnimationTrack[]	AnimationTrackScales		{ get { return m_AnimS; } }
		public Matrix4x4					AnimationSourceMatrix		{ get { return Pivot * m_AnimationSourceMatrix; } }

		/// <summary>
		/// Gets the list of meshes attached to this transform
		/// </summary>
		public LoaderTempMesh[]		Meshes
		{
			get { return m_Meshes.ToArray(); }
		}

		#endregion

		#region METHODS

		public LoaderTempTransform( SceneLoader _Owner, string _Name ) : base( _Owner, _Name )
		{
		}

		#region Static Transform Setup

		public void		SetMatrix( Matrix4x4 _Matrix )
		{
			m_Matrix = _Matrix;
		}

		public void		SetPosition( float _x, float _y, float _z )
		{
			m_Position = new Point( _x, _y, _z );
		}

		public void		SetRotationFromMatrix( float[] _Row0, float[] _Row1, float[] _Row2 )
		{
			if ( _Row0 == null )
				throw new Exception( "Invalid row #0!" );
			if ( _Row1 == null )
				throw new Exception( "Invalid row #1!" );
			if ( _Row2 == null )
				throw new Exception( "Invalid row #2!" );
			if ( _Row0.Length != 3 || _Row1.Length != 3 || _Row2.Length != 3 )
				throw new Exception( "Rows must be of length 3!" );

			float[,]	Mat = new float[3,3];
						Mat[0,0] = _Row0[0];	Mat[0,1] = _Row0[1];	Mat[0,2] = _Row0[2];
						Mat[1,0] = _Row1[0];	Mat[1,1] = _Row1[1];	Mat[1,2] = _Row1[2];
						Mat[2,0] = _Row2[0];	Mat[2,1] = _Row2[1];	Mat[2,2] = _Row2[2];

			m_Rotation = new Matrix3x3( Mat );
		}

		public void		SetRotationFromQuat( float _x, float _y, float _z, float _s )
		{
			m_QuatRotation = new Quat( _s, _x, _y, _z );
		}

		public void		SetScale( float _x, float _y, float _z )
		{
			m_Scale = new Vector( _x, _y, _z );
		}

		#endregion

		#region Dynamic Transform Setup

		public void		SetAnimationTrackPositions( FBXImporter.AnimationTrack[] _Tracks )
		{
			m_AnimP = _Tracks;
			m_bAnimated = true;
		}

		public void		SetAnimationTrackRotations( FBXImporter.AnimationTrack[] _Tracks )
		{
			m_AnimR = _Tracks;
			m_bAnimated = true;
		}

		public void		SetAnimationTrackScales( FBXImporter.AnimationTrack[] _Tracks )
		{
			m_AnimS = _Tracks;
			m_bAnimated = true;
		}

		public void		SetAnimationSourceMatrix( Matrix4x4 _SourceMatrix )
		{
			m_AnimationSourceMatrix = _SourceMatrix;
		}

		#endregion

		/// <summary>
		/// Adds a mesh to this transform
		/// </summary>
		/// <param name="_Mesh">The mesh to add</param>
		/// <remarks>This will transform into referenced shapes in the JSON file</remarks>
		public void		AddMesh( LoaderTempMesh _Mesh )
		{
			m_Meshes.Add( _Mesh );
		}

		#endregion
	};
}
