using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

//using WMath;
using SharpDX;
using FBX.Scene.Nodes;
using FBX.Scene.Materials;

namespace FBX.Scene
{
	/// <summary>
	/// <summary>
	/// A scene is a collection of meshes, lights, cameras, textures and material parameters
	/// A mesh is a collection of primitives that are rendered by the renderer's render techniques
	/// 
	/// You can load and save a scene in the Nuaj.Cirrus proprietary format.
	/// Scenes can also be imported through 3rd party formats (cf. FBXSceneLoader) then saved into the proprietary format.
	/// </summary>
	public class Scene
	{
		#region NESTED TYPES

		public delegate bool	FindNodeDelegate( Node _Node );

		#endregion

		#region FIELDS

		// Nodes hierarchy
		protected int									m_NodeIDCounter = 0;
		protected Node									m_Root = null;
		protected Dictionary<int,Node>					m_ID2Node = new Dictionary<int,Node>();

		// Object classes
		protected List<Mesh>							m_Meshes = new List<Mesh>();
		protected List<Light>							m_Lights = new List<Light>();
		protected List<Camera>							m_Cameras = new List<Camera>();

		// Textures
		protected int									m_TextureIDCounter = 0;
		protected List<Texture2D>						m_Textures = new List<Texture2D>();
		protected Dictionary<string,Texture2D>			m_URL2Texture = new Dictionary<string,Texture2D>();
		protected Dictionary<int,Texture2D>				m_ID2Texture = new Dictionary<int,Texture2D>();

		// Material Parameters
		protected int									m_MaterialParametersIDCounter = 0;
		protected List<MaterialParameters>				m_MaterialParameters = new List<MaterialParameters>();
		protected Dictionary<int,MaterialParameters>	m_ID2MaterialParameters = new Dictionary<int,MaterialParameters>();

		#endregion

		#region PROPERTIES

		/// <summary>
		/// Gets the scene's root node
		/// </summary>
		public Node				RootNode
		{
			get { return m_Root; }
		}

		/// <summary>
		/// Gets the scene's meshes collapsed as an array
		/// </summary>
		public Mesh[]			Meshes
		{
			get { return m_Meshes.ToArray(); }
		}

		/// <summary>
		/// Gets the scene's lights collapsed as an array
		/// </summary>
		public Light[]			Lights
		{
			get { return m_Lights.ToArray(); }
		}

		/// <summary>
		/// Gets the scene's cameras collapsed as an array
		/// </summary>
		public Camera[]			Cameras
		{
			get { return m_Cameras.ToArray(); }
		}

		/// <summary>
		/// Gets the scene's material parameters as an array
		/// </summary>
		public MaterialParameters[]	MaterialParameters
		{
			get { return m_MaterialParameters.ToArray(); }
		}

		#endregion

		#region METHODS

		/// <summary>
		/// Creates a default scene
		/// </summary>
		/// <param name="_Device"></param>
		/// <param name="_Name"></param>
		/// <param name="_Renderer">The renderer that can render the scene</param>
		public	Scene()
		{
		}

		/// <summary>
		/// Creates a new node for the scene
		/// </summary>
		/// <param name="_Name"></param>
		/// <param name="_Parent"></param>
		/// <param name="_Local2Parent"></param>
		/// <returns></returns>
		public Node			CreateNode( string _Name, Node _Parent, Matrix _Local2Parent )
		{
			Node	Result = new Node( this, m_NodeIDCounter++, _Name, _Parent, _Local2Parent );
			m_ID2Node[Result.ID] = Result;

			if ( _Parent == null )
			{	// New root ?
				if ( m_Root != null )
					throw new Exception( "You're providing a root (i.e. no parent node) whereas there is already one ! Did you forget to clear the nodes?" );
			
				m_Root = Result;	// Got ourselves a new root !
			}

			return Result;
		}

		/// <summary>
		/// Creates a new mesh for the scene
		/// </summary>
		/// <param name="_Name"></param>
		/// <param name="_Parent"></param>
		/// <param name="_Local2Parent"></param>
		/// <returns></returns>
		public Mesh			CreateMesh( string _Name, Node _Parent, Matrix _Local2Parent )
		{
			Mesh	Result = new Mesh( this, m_NodeIDCounter++, _Name, _Parent, _Local2Parent );
			m_ID2Node[Result.ID] = Result;
			m_Meshes.Add( Result );

			return Result;
		}

		/// <summary>
		/// Creates a new camera for the scene
		/// </summary>
		/// <param name="_Name"></param>
		/// <param name="_Parent"></param>
		/// <param name="_Local2Parent"></param>
		/// <returns></returns>
		public Camera		CreateCamera( string _Name, Node _Parent, Matrix _Local2Parent )
		{
			Camera	Result = new Camera( this, m_NodeIDCounter++, _Name, _Parent, _Local2Parent );
			m_ID2Node[Result.ID] = Result;
			m_Cameras.Add( Result );

			return Result;
		}

		/// <summary>
		/// Creates a new light for the scene
		/// </summary>
		/// <param name="_Name"></param>
		/// <param name="_Parent"></param>
		/// <param name="_Local2Parent"></param>
		/// <returns></returns>
		public Light		CreateLight( string _Name, Node _Parent, Matrix _Local2Parent )
		{
			Light	Result = new Light( this, m_NodeIDCounter++, _Name, _Parent, _Local2Parent );
			m_ID2Node[Result.ID] = Result;
			m_Lights.Add( Result );

			return Result;
		}

		/// <summary>
		/// Clear the hierarchy of nodes
		/// </summary>
		public void			ClearNodes()
		{
			if ( m_Root != null )
				m_Root.Dispose();
			m_Root = null;
			m_ID2Node.Clear();
			m_Meshes.Clear();
			m_Lights.Clear();
			m_Cameras.Clear();
			m_NodeIDCounter = 0;
		}

		/// <summary>
		/// Registers the node's ID and associate it to the node so we can find nodes by ID later
		/// </summary>
		/// <param name="_Node"></param>
		internal void		RegisterNodeID( Node _Node )
		{
			m_ID2Node[_Node.ID] = _Node;
		}

		/// <summary>
		/// Finds a node by name
		/// </summary>
		/// <param name="_Name"></param>
		/// <param name="_bCaseSensitive"></param>
		/// <returns></returns>
		public Node			FindNode( string _Name, bool _bCaseSensitive )
		{
			return m_Root != null ? FindNode( m_Root, _bCaseSensitive ? _Name : _Name.ToLower(), _bCaseSensitive ) : null;
		}

		protected Node		FindNode( Node _Node, string _Name, bool _bCaseSensitive )
		{
			if ( _bCaseSensitive ? _Node.Name == _Name : _Node.Name.ToLower() == _Name )
				return _Node;	// Found it !

			foreach ( Node Child in _Node.Children )
			{
				Node	Result = FindNode( Child, _Name, _bCaseSensitive );
				if ( Result != null )
					return Result;
			}

			return null;
		}

		/// <summary>
		/// Finds a node by ID
		/// </summary>
		/// <param name="_NodeID"></param>
		/// <returns></returns>
		public Node			FindNode( int _NodeID )
		{
			return m_Root != null ? FindNode( m_Root, _NodeID ) : null;
		}

		protected Node		FindNode( Node _Node, int _NodeID )
		{
			if ( _Node.ID == _NodeID )
				return _Node;

			foreach ( Node Child in _Node.Children )
			{
				Node	Result = FindNode( Child, _NodeID );
				if ( Result != null )
					return Result;
			}

			return null;
		}

		/// <summary>
		/// Custom node finder
		/// </summary>
		/// <param name="_Node"></param>
		/// <param name="_D"></param>
		/// <returns></returns>
		public Node		FindNode( Node _Node,  FindNodeDelegate _D )
		{
			if ( _D( _Node ) )
				return _Node;	// Found it !

			foreach ( Node Child in _Node.Children )
			{
				Node	Result = FindNode( Child, _D );
				if ( Result != null )
					return Result;
			}

			return null;
		}

		/// <summary>
		/// Finds a mesh by name
		/// </summary>
		/// <param name="_Name"></param>
		/// <param name="_bCaseSensitive"></param>
		/// <returns></returns>
		public Mesh			FindMesh( string _Name, bool _bCaseSensitive )
		{
			return m_Root != null ? FindMesh( m_Root, _bCaseSensitive ? _Name : _Name.ToLower(), _bCaseSensitive ) : null;
		}

		protected Mesh		FindMesh( Node _Node, string _Name, bool _bCaseSensitive )
		{
			if ( _Node is Mesh && (_bCaseSensitive ? _Node.Name == _Name : _Node.Name.ToLower() == _Name) )
				return _Node as Mesh;	// Found it !

			foreach ( Node Child in _Node.Children )
			{
				Mesh	Result = FindMesh( Child, _Name, _bCaseSensitive );
				if ( Result != null )
					return Result;
			}

			return null;
		}

		/// <summary>
		/// Creates a new texture for the scene
		/// </summary>
		/// <param name="_URL">The relative texture URL to identify the texture</param>
		/// <param name="_OpacityURL">The optional relative URL to identify the opacity texture</param>
		/// <param name="_bCreateMipMaps">True if the texture should be created using mip-maps</param>
		/// <param name="_TextureProvider">The texture provider capable of creating the actual texture</param>
		/// <returns></returns>
		public Texture2D	CreateTexture( string _URL, string _OpacityURL, bool _bCreateMipMaps )
		{
			string	FullURL = _URL + "|" + _OpacityURL;	// The full URL is a concatenation of both URLs
			if ( m_URL2Texture.ContainsKey( FullURL ) )
				return m_URL2Texture[FullURL];	// Already registered...

			Texture2D	Result = new Texture2D( this, m_TextureIDCounter++, _URL, _OpacityURL, _bCreateMipMaps );
			m_Textures.Add( Result );
			m_URL2Texture[FullURL] = Result;
			m_ID2Texture[Result.ID] = Result;

			return Result;
		}

		/// <summary>
		/// Finds a texture by ID
		/// </summary>
		/// <param name="_ID"></param>
		/// <returns></returns>
		public Texture2D	FindTexture( int _ID )
		{
			return m_ID2Texture.ContainsKey( _ID ) ? m_ID2Texture[_ID] : null;
		}

		/// <summary>
		/// Clear the list of textures
		/// </summary>
		public void			ClearTextures()
		{
			m_Textures.Clear();
			m_URL2Texture.Clear();
			m_ID2Texture.Clear();
			m_TextureIDCounter = 0;
		}

		/// <summary>
		/// Creates a new material parameter block
		/// </summary>
		/// <param name="_Name">The name of the parameter block</param>
		/// <param name="_ShaderURL">The URL of the shader that uses theses parameters (this can be a path to an actual shader, or an identifier like Phong, Lambert, Blinn, whatever really as anyway these parameters are later read and identified by you so you can use whatever makes you comfortable)</param>
		/// <returns></returns>
		public MaterialParameters	CreateMaterialParameters( string _Name, string _ShaderURL )
		{
			MaterialParameters	Result = new MaterialParameters( this, m_MaterialParametersIDCounter++, _Name, _ShaderURL );
			m_MaterialParameters.Add( Result );
			m_ID2MaterialParameters.Add( Result.ID, Result );

			return Result;
		}
		
		/// <summary>
		/// Finds a material parameter block by ID
		/// </summary>
		/// <param name="_ID"></param>
		/// <returns></returns>
		public MaterialParameters	FindMaterialParameters( int _ID )
		{
			return m_ID2MaterialParameters.ContainsKey( _ID ) ? m_ID2MaterialParameters[_ID] : null;
		}
		
		/// <summary>
		/// Finds a material parameter block by name
		/// </summary>
		/// <param name="_Name"></param>
		/// <returns></returns>
		public MaterialParameters	FindMaterialParameters( string _Name )
		{
			foreach ( MaterialParameters Params in m_MaterialParameters )
				if ( Params.Name == _Name )
					return Params;

			return null;	// Not found
		}

		/// <summary>
		/// Clear the list of material parameters
		/// </summary>
		public void			ClearMaterialParameters()
		{
			m_MaterialParameters.Clear();
			m_ID2MaterialParameters.Clear();
			m_MaterialParametersIDCounter = 0;
		}

		/// <summary>
		/// Creates a scene from a stream
		/// </summary>
		/// <param name="_Reader"></param>
		/// <param name="_TextureProvider"></param>
		/// <returns></returns>
		public void		Load( System.IO.BinaryReader _Reader )
		{
			// Read back textures
			ClearTextures();
			int	TexturesCount = _Reader.ReadInt32();
			for ( int TextureIndex=0; TextureIndex < TexturesCount; TextureIndex++ )
			{
				Texture2D	T = new Texture2D( this, _Reader );
				m_Textures.Add( T );
				m_URL2Texture.Add( T.URL, T );
				m_ID2Texture.Add( T.ID, T );
			}
			m_TextureIDCounter = m_Textures.Count;

			// Read back material parameters
			ClearMaterialParameters();
			int	MaterialParametersCount = _Reader.ReadInt32();
			for ( int MaterialParameterIndex=0; MaterialParameterIndex < MaterialParametersCount; MaterialParameterIndex++ )
			{
				MaterialParameters	MP = new MaterialParameters( this, _Reader );
				m_MaterialParameters.Add( MP );
				m_ID2MaterialParameters.Add( MP.ID, MP );
			}
			m_MaterialParametersIDCounter = m_MaterialParameters.Count;

			// Read back the nodes hierarchy
			ClearNodes();
			bool	bHasRoot = _Reader.ReadBoolean();
			if ( !bHasRoot )
				return;

			// Read back root type
			Node.NODE_TYPE	Type = (Node.NODE_TYPE) _Reader.ReadByte();
			switch ( Type )
			{
				case Node.NODE_TYPE.NODE:
					m_Root = new Node( this, null, _Reader );
					break;

				case Node.NODE_TYPE.MESH:
					m_Root = new Mesh( this, null, _Reader );
					m_Meshes.Add( m_Root as Mesh );
					break;

				case Node.NODE_TYPE.LIGHT:
					m_Root = new Light( this, null, _Reader );
					m_Lights.Add( m_Root as Light );
					break;

				case Node.NODE_TYPE.CAMERA:
					m_Root = new Camera( this, null, _Reader );
					m_Cameras.Add( m_Root as Camera );
					break;
			}
			m_ID2Node[m_Root.ID] = m_Root;


			// Propagate state once so matrices are up to date
			m_Root.PropagateState();
		}

		/// <summary>
		/// Writes a scene to a stream
		/// </summary>
		/// <param name="_Stream"></param>
		public void		Save( System.IO.BinaryWriter _Writer )
		{
			// Write textures
			_Writer.Write( m_Textures.Count );
			foreach ( Texture2D T in m_Textures )
				T.Save( _Writer );

			// Write material parameters
			_Writer.Write( m_MaterialParameters.Count );
			foreach ( MaterialParameters MatParams in m_MaterialParameters )
				MatParams.Save( _Writer );

			// Recursively save nodes
			if ( m_Root == null )
				_Writer.Write( false );	// No root...
			else
			{
				_Writer.Write( true );
				m_Root.Save( _Writer );
			}
		}

		#region Motion Blur Helpers

		/// <summary>
		/// Helper method that helps to compute the difference in translation and rotation of an object moving from one frame to the other
		/// This is essentially used for motion blur
		/// </summary>
		/// <param name="_Previous">The object's matrix at previous frame</param>
		/// <param name="_Current">The object's matrix at current frame</param>
		/// <param name="_DeltaPosition">Returns the difference in position from last frame</param>
		/// <param name="_DeltaRotation">Returns the difference in rotation from last frame</param>
		/// <param name="_Pivot">Returns the pivot position the object rotated about</param>
		public static void	ComputeObjectDeltaPositionRotation( ref Matrix _Previous, ref Matrix _Current, out Vector3 _DeltaPosition, out Quaternion _DeltaRotation, out Vector3 _Pivot )
		{
			// Compute the rotation the matrix sustained
			Quaternion	PreviousRotation = QuatFromMatrix( _Previous );
			Quaternion	CurrentRotation = QuatFromMatrix( _Current );
			_DeltaRotation = QuatMultiply( QuatInvert( PreviousRotation ), CurrentRotation );

			Vector3	PreviousPosition = (Vector3) _Previous.Row4;
			Vector3	CurrentPosition = (Vector3) _Current.Row4;

			// Retrieve the pivot point about which that rotation occurred
			_Pivot = CurrentPosition;

			float	RotationAngle = _DeltaRotation.Angle;
			if ( Math.Abs( RotationAngle ) > 1e-4f )
			{
				Vector3	RotationAxis = _DeltaRotation.Axis;
				Vector3	Previous2Current = CurrentPosition - PreviousPosition;
				float	L = Previous2Current.Length();
				if ( L > 1e-4f )
				{
					Previous2Current /= L;
					Vector3	N = Vector3.Cross( Previous2Current, RotationAxis );
					N.Normalize();

					Vector3	MiddlePoint = 0.5f * (PreviousPosition + CurrentPosition);
					float	Distance2Pivot = 0.5f * L / (float) Math.Tan( 0.5f * RotationAngle );
					_Pivot = MiddlePoint + N * Distance2Pivot;
				}

				// Rotate previous position about pivot, this should yield us current position
				Vector3	RotatedPreviousPosition = RotateAbout( PreviousPosition, _Pivot, _DeltaRotation );

//				// Update previous position so the remaining position gap is filled by delta translation
//				PreviousPosition = RotatedPreviousPosition;
				PreviousPosition = CurrentPosition;	// Close the gap so we have no delta translation
			}

			_DeltaPosition = CurrentPosition - PreviousPosition;	// Easy !
		}

		static Quaternion	QuatFromMatrix( Matrix M )
		{
			Quaternion	q = new Quaternion();

			float	s = (float) System.Math.Sqrt( M.M11 + M.M22 + M.M33 + 1.0f );
			q.W = s * 0.5f;
			s = 0.5f / s;
			q.X = (M.M32 - M.M23) * s;
			q.Y = (M.M13 - M.M31) * s;
			q.Z = (M.M21 - M.M12) * s;

			return	q;
		}

		static Quaternion	QuatInvert( Quaternion q )
		{
			float	fNorm = q.LengthSquared();
			if ( fNorm < float.Epsilon )
				return q;

			float	fINorm = -1.0f / fNorm;
			q.X *=  fINorm;
			q.Y *=  fINorm;
			q.Z *=  fINorm;
			q.W *= -fINorm;

			return q;
		}

		static Vector3	RotateAbout( Vector3 _Point, Vector3 _Pivot, Quaternion _Rotation )
		{
			Quaternion	Q = new Quaternion( _Point - _Pivot, 0.0f );
			Quaternion	RotConjugate = _Rotation;
			RotConjugate.Conjugate();
			Quaternion	Pr = QuatMultiply( QuatMultiply( _Rotation, Q ), RotConjugate );
			Vector3		Protated = new Vector3( Pr.X, Pr.Y, Pr.Z );
			return _Pivot + Protated;
		}

		static Quaternion	QuatMultiply( Quaternion q0, Quaternion q1 )
		{
			Quaternion	q;
			Vector3	V0 = new Vector3( q0.X, q0.Y, q0.Z );
			Vector3	V1 = new Vector3( q1.X, q1.Y, q1.Z );
			q.W = q0.W * q1.W - Vector3.Dot( V0, V1 );
			Vector3	V = q0.W * V1 + V0 * q1.W + Vector3.Cross( V0, V1 );
			q.X = V.X;
			q.Y = V.Y;
			q.Z = V.Z;

			return q;
		}

		#endregion

		#endregion
	}
}
