using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using WMath;
using FBX.SceneLoader.Objects;

namespace FBX.SceneLoader
{
	/// <summary>
	/// This class is able to convert a FBX scene into a custom scene
	/// 
	/// Load a scene like this:
	/// 1) Create an empty scene
	/// 2) Create the scene loader
	/// 3) Ask the loader to load a FBX scene providing:
	///  . The filename of the scene to load
	///  . A material map that is able to convert FBX materials into Cirrus render techniques that render these materials
	///  . A texture provider that loads textures for the scene materials
	/// 
	/// Once the FBX scene is loaded, you can save the scene into a proprietary scene format and shouldn't need the FBX format anymore
	/// (i.e. FBX is only a DEBUG mode format that should NOT be used at runtime as proprietary scenes are much more lightweight).
	/// </summary>
	/// <remarks>
	///	_ ZUp fix from Max orientation is done in the RecurseProcessNode() method at root node level
	///	_ V coordinate complement to 1 is done in PrimitiveFeeder.GetField() method for the TEX_COORD2D case
	/// </remarks>
	public class	SceneLoader
	{
		#region NESTED TYPES

		// The list of suported texture formats
		//
		public enum		TEXTURE_CONVERSION_TYPES
		{
			NONE,	// No conversion, leave texture as is
			JPG,
			PNG,
			DDS,	// NOT SUPPORTED YET
			DXT1,	// NOT SUPPORTED YET
			DXT2,	// NOT SUPPORTED YET
			DXT3,	// NOT SUPPORTED YET
			DXT5,	// NOT SUPPORTED YET
		}

		// The list of actions to perform if no tangent space data is available
		//
		public enum		NO_TANGENT_SPACE_ACTION
		{
			THROW,
			SKIP,
			VALIDATE
		}

		#endregion

		#region FIELDS

		protected FBX.Scene.Scene				m_Scene = null;
//		protected Cirrus.MaterialMap			m_MaterialMap = null;
		protected float							m_ScaleFactor = 1.0f;

		// The list of material mapping from FBX materials to Cirrus render technique able to handle the materials
//		protected Dictionary<FBXImporter.Material,Cirrus.ITechniqueSupportsObjects>	m_Material2Technique = new Dictionary<FBXImporter.Material,Cirrus.ITechniqueSupportsObjects>();

		// The list of material mapping from FBX materials to Cirrus material parameters
		protected Dictionary<FBXImporter.Material,Scene.Materials.MaterialParameters>	m_Material2Parameters = new Dictionary<FBXImporter.Material,Scene.Materials.MaterialParameters>();

		// The table of meshes
		protected Dictionary<LoaderTempMesh, Scene.Nodes.Mesh>	m_TempMesh2FinalMesh = new Dictionary<LoaderTempMesh, Scene.Nodes.Mesh>();

		// The list of texture files
		protected List<FileInfo>			m_Textures = new List<FileInfo>();


		// =================== Internal options ===================

		// Mesh options
		protected bool						m_bGenerateTangentSpace = true;
		protected NO_TANGENT_SPACE_ACTION	m_NoTangentSpaceAction = NO_TANGENT_SPACE_ACTION.THROW;

		protected bool						m_bStoreHDRVertexColors = true;
		protected bool						m_bGenerateBoundingBoxes = true;
		protected bool						m_bResetXForm = false;
		protected bool						m_bGenerateTriangleStrips = false;
		protected bool						m_bCompactUVs = true;
		protected bool						m_bExportAnimations = false;

			// Consolidation options
		protected bool						m_bConsolidateMeshes = true;
		protected bool						m_bConsolidateSplitBySMG = true;
		protected bool						m_bConsolidateSplitByUV = true;
		protected bool						m_bConsolidateSplitByColor = true;

			// UV Compacting
		protected bool						m_bCompactIdenticalMeshes = false;	//TODO! Finish instancing and shared vertices!
		protected int						m_MinUVsCount = 1;

		// Textures
		protected DirectoryInfo				m_TargetTexturesBaseDirectory = null;

		protected TEXTURE_CONVERSION_TYPES	m_ConvertDiffuse = TEXTURE_CONVERSION_TYPES.NONE;
		protected bool						m_bGenerateMipMapsDiffuse = true;

		protected TEXTURE_CONVERSION_TYPES	m_ConvertNormal = TEXTURE_CONVERSION_TYPES.NONE;
		protected bool						m_bGenerateMipMapsNormal = true;

		protected TEXTURE_CONVERSION_TYPES	m_ConvertRegular = TEXTURE_CONVERSION_TYPES.NONE;
		protected bool						m_bGenerateMipMapsRegular = true;

		protected int						m_JPEGQuality = 80;

		#endregion

		#region PROPERTIES

		/// <summary>
		/// True to generate missing tangent space informations from meshes
		/// (they must have at least either UV coordinates or a normal and 
		/// tangent/binormal set of data to be eligible to tangent space
		/// construction, otherwise the NoTangentSpaceAction is applied)
		/// </summary>
		public bool				GenerateTangentSpace
		{
			get { return m_bGenerateTangentSpace; }
			set { m_bGenerateTangentSpace = value; }
		}

		/// <summary>
		/// Defines the action to perform when no tangent space can be built on a mesh
		/// </summary>
		public NO_TANGENT_SPACE_ACTION	NoTangentSpaceAction
		{
			get { return m_NoTangentSpaceAction; }
			set { m_NoTangentSpaceAction = value; }
		}

		/// <summary>
		/// True to store HDR (i.e. Vector4) colors instead of LDR (i.e. UInt32) colors
		/// </summary>
		public bool				StoreHDRVertexColors
		{
			get { return m_bStoreHDRVertexColors; }
			set { m_bStoreHDRVertexColors = value; }
		}

		/// <summary>
		/// True to compute local mesh bounding-boxes
		/// </summary>
		public bool				GenerateBoundingBoxes
		{
			get { return m_bGenerateBoundingBoxes; }
			set { m_bGenerateBoundingBoxes = value; }
		}

		/// <summary>
		/// True to reset the transform of meshes to an identity matrix (discards animations)
		/// </summary>
		public bool				ResetXForm
		{
			get { return m_bResetXForm; }
			set { m_bResetXForm = value; }
		}

// Not supported yet !
// 		/// <summary>
// 		/// True to generate triangle strips instead of default triangls lists
// 		/// </summary>
// 		public bool				GenerateTriangleStrips
// 		{
// 			get { return m_bGenerateTriangleStrips; }
// 			set { m_bGenerateTriangleStrips = value; }
// 		}

		/// <summary>
		/// Compacts identical UV layers into a single layer
		/// </summary>
		public bool				CompactUVs
		{
			get { return m_bCompactUVs; }
			set { m_bCompactUVs = value; }
		}

		/// <summary>
		/// Gets or sets the minimum amount of UV sets to keep when compacting UVs
		/// </summary>
		public int				MinUVsCount
		{
			get { return m_MinUVsCount; }
			set { m_MinUVsCount = value; }
		}

		/// <summary>
		/// True to consolidate meshes and split primitives by difference in vertex infos
		/// (cf. consolidation options below)
		/// </summary>
		public bool				ConsolidateMeshes
		{
			get { return m_bConsolidateMeshes; }
			set { m_bConsolidateMeshes = value; }
		}

		/// <summary>
		/// True to split a vertex if it's shared by faces whose SMG differ
		/// </summary>
		public bool				ConsolidateSplitBySMG
		{
			get { return m_bConsolidateSplitBySMG; }
			set { m_bConsolidateSplitBySMG = value; }
		}

		/// <summary>
		/// True to split a vertex if it's shared by faces whose UVs differ
		/// </summary>
		public bool				ConsolidateSplitByUV
		{
			get { return m_bConsolidateSplitByUV; }
			set { m_bConsolidateSplitByUV = value; }
		}

		/// <summary>
		/// True to split a vertex if it's shared by faces whose colors differ
		/// </summary>
		public bool				ConsolidateSplitByColor
		{
			get { return m_bConsolidateSplitByColor; }
			set { m_bConsolidateSplitByColor = value; }
		}

		/// <summary>
		/// Compatcs all identical meshes into mesh instances, keeping only the difference between meshes
		/// </summary>
		public bool				CompactIdenticalMeshes
		{
			get { return m_bCompactIdenticalMeshes; }
			set { m_bCompactIdenticalMeshes = value; }
		}

		public DirectoryInfo	TargetTexturesBaseDirectory
		{
			get { return m_TargetTexturesBaseDirectory; }
			set { m_TargetTexturesBaseDirectory = value; }
		}

		public TEXTURE_CONVERSION_TYPES	ConvertDiffuse
		{
			get { return m_ConvertDiffuse; }
			set { m_ConvertDiffuse = value; }
		}
		public bool						GenerateMipMapsDiffuse
		{
			get { return m_bGenerateMipMapsDiffuse; }
			set { m_bGenerateMipMapsDiffuse = value; }
		}

		public TEXTURE_CONVERSION_TYPES	ConvertNormal
		{
			get { return m_ConvertNormal; }
			set { m_ConvertNormal = value; }
		}
		public bool						GenerateMipMapsNormal
		{
			get { return m_bGenerateMipMapsNormal; }
			set { m_bGenerateMipMapsNormal = value; }
		}

		public TEXTURE_CONVERSION_TYPES	ConvertRegular
		{
			get { return m_ConvertRegular; }
			set { m_ConvertRegular = value; }
		}
		public bool						GenerateMipMapsRegular
		{
			get { return m_bGenerateMipMapsRegular; }
			set { m_bGenerateMipMapsRegular = value; }
		}

		public int						JPEGQuality
		{
			get { return m_JPEGQuality; }
			set { m_JPEGQuality = value; }
		}

		#endregion

		#region METHODS

		public	SceneLoader()
		{
		}

		/// <summary>
		/// Loads a FBX file into a Cirrus scene (which may already contain meshes and materials, don't care)
		/// </summary>
		/// <param name="_FileName">The name of the FBX file to load</param>
		/// <param name="_Scene">The cirrus scene into which we should store the data</param>
		public void	Load( FileInfo _FileName, FBX.Scene.Scene _Scene )
		{
			Load( _FileName, _Scene, 1.0f );
		}

		/// <summary>
		/// Loads a FBX file into a Cirrus scene (which may already contain meshes and materials, don't care)
		/// </summary>
		/// <param name="_FileName">The name of the FBX file to load</param>
		/// <param name="_Scene">The cirrus scene into which we should store the data</param>
		/// <param name="_ScaleFactor">The scale factor to apply to the entire scene
		/// By default, internal MAX units can be considered as centimeters so if you create a scene whose dimensions of a one meter box are 100x100x100, you
		/// will want to use a scale factor of 0.01.
		/// FBX offers the possibility of scaling but does a shitty job at it as it doesn't even rescale other dimensions like near/far clips or ranges for lights
		///  and camera, which plain sucks.</param>
		public void	Load( FileInfo _FileName, FBX.Scene.Scene _Scene, float _ScaleFactor )
		{
			if ( _FileName == null )
				throw new Exception( "Invalid file name!" );
			if ( !_FileName.Exists )
				throw new Exception( "Scene file \"" + _FileName + "\" does not exist!" );
			if ( _Scene == null )
				throw new Exception( "Invalid Scene to load into!" );

			m_Scene = _Scene;
			m_TempMesh2FinalMesh.Clear();
			m_ScaleFactor = _ScaleFactor;

			FBXImporter.Scene	FBXScene = null;
			try
			{
				FBXScene = new FBXImporter.Scene();
				FBXScene.Load( _FileName.FullName );

				// Process materials
				ProcessMaterials( FBXScene.Materials );

				// Process the scene nodes
				RecurseProcessNode( FBXScene.RootNode, null );

				// Attach camera & light targets
				PostProcessNodes( m_Scene.RootNode );

				// Build actual optimized and consolidated meshes
				BuildConsolidatedMeshes();

				// Propagate state once so Local2World matrices are up to date
				m_Scene.RootNode.PropagateState();
			}
			catch ( Exception _e )
			{
				throw new Exception( "An error occurred while importing the FBX file \"" + _FileName + "\"!", _e );
			}
			finally
			{
				FBXScene.Dispose();
			}
		}

		/// <summary>
		/// Attempts to map FBX materials to render techniques
		/// </summary>
		/// <param name="_Materials">The list of materials to process</param>
		protected void	ProcessMaterials( FBXImporter.Material[] _Materials )
		{
			m_Material2Parameters.Clear();

			foreach ( FBXImporter.Material Material in _Materials )
			{
				Scene.Materials.MaterialParameters	MatParams = null;

// DEBUG
// if ( Material.Name == "sp_00_svod" )
// 	MatParams = null;
// DEBUG

				if ( Material is FBXImporter.MaterialHardwareShader )
				{
					FBXImporter.MaterialHardwareShader	SpecificMaterial = Material as FBXImporter.MaterialHardwareShader;

					MatParams = m_Scene.CreateMaterialParameters( Material.Name, SpecificMaterial.RelativeURL );

					foreach ( FBXImporter.MaterialHardwareShader.TableEntry Entry in SpecificMaterial.ShaderEntries )
					{
						switch ( Entry.TypeName )
						{
							case "Boolean":
								MatParams.CreateParameter( Entry.Name, Scene.Materials.MaterialParameters.PARAMETER_TYPE.BOOL ).AsBool.Value = (bool) Entry.Value;
								break;
							case "Integer":
								MatParams.CreateParameter( Entry.Name, Scene.Materials.MaterialParameters.PARAMETER_TYPE.INT ).AsInt.Value = (int) Entry.Value;
								break;
							case "Float":
								MatParams.CreateParameter( Entry.Name, Scene.Materials.MaterialParameters.PARAMETER_TYPE.FLOAT ).AsFloat.Value = (float) Entry.Value;
								break;
							case "Float2":
								MatParams.CreateParameter( Entry.Name, Scene.Materials.MaterialParameters.PARAMETER_TYPE.FLOAT2 ).AsFloat2.Value = Entry.Value as Vector2D;
								break;
							case "Float3":
								MatParams.CreateParameter( Entry.Name, Scene.Materials.MaterialParameters.PARAMETER_TYPE.FLOAT3 ).AsFloat3.Value = Entry.Value as Vector;
								break;
							case "Float4":
								MatParams.CreateParameter( Entry.Name, Scene.Materials.MaterialParameters.PARAMETER_TYPE.FLOAT4 ).AsFloat4.Value = Entry.Value as Vector4D;
								break;
							case "Matrix":
								MatParams.CreateParameter( Entry.Name, Scene.Materials.MaterialParameters.PARAMETER_TYPE.MATRIX4 ).AsMatrix4.Value = Entry.Value as Matrix4x4;
								break;
							case "Texture":
								CreateTextureParameter( Material, Entry.Name, MatParams, Entry.Name );
								break;
						}
					}
				}
				else if ( Material is FBXImporter.MaterialPhong )
				{
					FBXImporter.MaterialPhong	SpecificMaterial = Material as FBXImporter.MaterialPhong;

					MatParams = m_Scene.CreateMaterialParameters( Material.Name, "Phong" );

					// Lambert parameters
					MatParams.CreateParameter( "AmbientColor", Scene.Materials.MaterialParameters.PARAMETER_TYPE.FLOAT3 ).AsFloat3.Value = SpecificMaterial.AmbientColor;
					CreateTextureParameter( Material, "AmbientColor", MatParams, "AmbientTexture" );
					MatParams.CreateParameter( "AmbientFactor", Scene.Materials.MaterialParameters.PARAMETER_TYPE.FLOAT ).AsFloat.Value = SpecificMaterial.AmbientFactor;

					MatParams.CreateParameter( "DiffuseColor", Scene.Materials.MaterialParameters.PARAMETER_TYPE.FLOAT3 ).AsFloat3.Value = SpecificMaterial.DiffuseColor;
					MatParams.CreateParameter( "DiffuseFactor", Scene.Materials.MaterialParameters.PARAMETER_TYPE.FLOAT ).AsFloat.Value = SpecificMaterial.DiffuseFactor;
					bool	bHasDiffuseTexture = CreateTextureParameter( Material, "DiffuseColor", MatParams, "DiffuseTexture", "TransparentColor" );
					MatParams.CreateParameter( "HasDiffuseTexture", Scene.Materials.MaterialParameters.PARAMETER_TYPE.BOOL ).AsBool.Value = bHasDiffuseTexture;

					MatParams.CreateParameter( "EmissiveColor", Scene.Materials.MaterialParameters.PARAMETER_TYPE.FLOAT3 ).AsFloat3.Value = SpecificMaterial.EmissiveColor;
					MatParams.CreateParameter( "EmissiveFactor", Scene.Materials.MaterialParameters.PARAMETER_TYPE.FLOAT ).AsFloat.Value = SpecificMaterial.EmissiveFactor;
					CreateTextureParameter( Material, "EmissiveColor", MatParams, "EmissiveTexture" );

					float	fOpacity = (float) SpecificMaterial.FindProperty( "Opacity" ).Value;
					MatParams.CreateParameter( "Opacity", Scene.Materials.MaterialParameters.PARAMETER_TYPE.FLOAT ).AsFloat.Value = fOpacity;
//					MatParams.IsOpaque = fOpacity >= 1.0f;

					bool	bHasNormalTexture = CreateTextureParameter( Material, "Bump", MatParams, "NormalTexture" );
					MatParams.CreateParameter( "HasNormalTexture", Scene.Materials.MaterialParameters.PARAMETER_TYPE.BOOL ).AsBool.Value = bHasNormalTexture;

					// Phong parameters
					MatParams.CreateParameter( "ReflectionColor", Scene.Materials.MaterialParameters.PARAMETER_TYPE.FLOAT3 ).AsFloat3.Value = SpecificMaterial.ReflectionColor;
					CreateTextureParameter( Material, "ReflectionColor", MatParams, "ReflectionTexture" );
					MatParams.CreateParameter( "ReflectionFactor", Scene.Materials.MaterialParameters.PARAMETER_TYPE.FLOAT ).AsFloat.Value = SpecificMaterial.ReflectionFactor;

					MatParams.CreateParameter( "Shininess", Scene.Materials.MaterialParameters.PARAMETER_TYPE.FLOAT ).AsFloat.Value = SpecificMaterial.Shininess;
					MatParams.CreateParameter( "SpecularColor", Scene.Materials.MaterialParameters.PARAMETER_TYPE.FLOAT3 ).AsFloat3.Value = SpecificMaterial.SpecularColor;
					MatParams.CreateParameter( "SpecularFactor", Scene.Materials.MaterialParameters.PARAMETER_TYPE.FLOAT ).AsFloat.Value = SpecificMaterial.SpecularFactor;
					bool	bHasSpecularTexture = CreateTextureParameter( Material, "SpecularFactor", MatParams, "SpecularTexture" );
					MatParams.CreateParameter( "HasSpecularTexture", Scene.Materials.MaterialParameters.PARAMETER_TYPE.BOOL ).AsBool.Value = bHasSpecularTexture;
				}
				else if ( Material is FBXImporter.MaterialLambert )
				{
					FBXImporter.MaterialLambert	SpecificMaterial = Material as FBXImporter.MaterialLambert;

					MatParams = m_Scene.CreateMaterialParameters( Material.Name, "Lambert" );

					// Lambert parameters
					MatParams.CreateParameter( "AmbientColor", Scene.Materials.MaterialParameters.PARAMETER_TYPE.FLOAT3 ).AsFloat3.Value = SpecificMaterial.AmbientColor;
					CreateTextureParameter( Material, "AmbientColor", MatParams, "AmbientTexture" );
					MatParams.CreateParameter( "AmbientFactor", Scene.Materials.MaterialParameters.PARAMETER_TYPE.FLOAT ).AsFloat.Value = SpecificMaterial.AmbientFactor;

					MatParams.CreateParameter( "DiffuseColor", Scene.Materials.MaterialParameters.PARAMETER_TYPE.FLOAT3 ).AsFloat3.Value = SpecificMaterial.DiffuseColor;
					MatParams.CreateParameter( "DiffuseFactor", Scene.Materials.MaterialParameters.PARAMETER_TYPE.FLOAT ).AsFloat.Value = SpecificMaterial.DiffuseFactor;
					bool	bHasDiffuseTexture = CreateTextureParameter( Material, "DiffuseColor", MatParams, "DiffuseTexture", "TransparentColor" );
					MatParams.CreateParameter( "HasDiffuseTexture", Scene.Materials.MaterialParameters.PARAMETER_TYPE.BOOL ).AsBool.Value = bHasDiffuseTexture;

					MatParams.CreateParameter( "EmissiveColor", Scene.Materials.MaterialParameters.PARAMETER_TYPE.FLOAT3 ).AsFloat3.Value = SpecificMaterial.EmissiveColor;
					MatParams.CreateParameter( "EmissiveFactor", Scene.Materials.MaterialParameters.PARAMETER_TYPE.FLOAT ).AsFloat.Value = SpecificMaterial.EmissiveFactor;
					CreateTextureParameter( Material, "EmissiveColor", MatParams, "EmissiveTexture" );

					FBXImporter.ObjectProperty	OpacityProp = SpecificMaterial.FindProperty( "Opacity" );
					float	fOpacity = OpacityProp != null ? (float) OpacityProp.Value : 1.0f;
					MatParams.CreateParameter( "Opacity", Scene.Materials.MaterialParameters.PARAMETER_TYPE.FLOAT ).AsFloat.Value = fOpacity;
//					MatParams.IsOpaque = fOpacity >= 1.0f;

					bool	bHasNormalTexture = CreateTextureParameter( Material, "Bump", MatParams, "NormalTexture" );
					MatParams.CreateParameter( "HasNormalTexture", Scene.Materials.MaterialParameters.PARAMETER_TYPE.BOOL ).AsBool.Value = bHasNormalTexture;
				}
				else
					continue;	// Unrecognized hence unsupported material type...

// 				Cirrus.ITechniqueSupportsObjects	RT = m_MaterialMap.MapToTechnique( MatParams );
// 				if ( RT == null )
// 					continue;	// Failed to find a suitable technique for that material...
// 
// 				// Register that material mapping
// 				m_Material2Technique[Material] = RT;
				m_Material2Parameters[Material] = MatParams;
			}
		}

		/// <summary>
		/// Processes a FBX node and converts it into a Cirrus object
		/// </summary>
		/// <param name="_FBXNode"></param>
		/// <param name="_ParentNode"></param>
		protected void	RecurseProcessNode( FBXImporter.Node _FBXNode, Scene.Nodes.Node _ParentNode )
		{
			Scene.Nodes.Node	NewNode = _ParentNode;
			if ( _FBXNode is FBXImporter.NodeMesh )
				NewNode = CreateMesh( _FBXNode as FBXImporter.NodeMesh, _ParentNode );	// Create a new mesh
			else if ( _FBXNode is FBXImporter.NodeCamera )
				NewNode = CreateCamera( _FBXNode as FBXImporter.NodeCamera, _ParentNode );
			else if ( _FBXNode is FBXImporter.NodeLight )
				NewNode = CreateLight( _FBXNode as FBXImporter.NodeLight, _ParentNode );
			else
			{	// Create a default node that is just here to respect the hierarchy for PRS evaluation
				Matrix4x4	LocalTransform = _FBXNode.LocalTransform;
				if ( _ParentNode == null )
				{	// Tweak the root node's transform to match DirectX representation
					LocalTransform.SetRow0( new Vector( 1.0f, 0.0f, 0.0f ) );
					LocalTransform.SetRow1( new Vector( 0.0f, 0.0f, -1.0f ) );
					LocalTransform.SetRow2( new Vector( 0.0f, 1.0f, 0.0f ) );
				}

				NewNode = m_Scene.CreateNode( _FBXNode.Name, _ParentNode, LocalTransform );
			}

			// Recurse through children
			foreach ( FBXImporter.Node Child in _FBXNode.Children )
				RecurseProcessNode( Child, NewNode );
		}

		/// <summary>
		/// Post-processes nodes, attaches light and camera targets, that kind of stuff
		/// </summary>
		/// <param name="_Node"></param>
		protected void	PostProcessNodes( Scene.Nodes.Node _Node )
		{
			if ( _Node is Scene.Nodes.Camera )
				(_Node as Scene.Nodes.Camera).TargetNode = m_Scene.FindNode( _Node.Name + ".Target", true );
			else if ( _Node is Scene.Nodes.Light )
				(_Node as Scene.Nodes.Light).TargetNode = m_Scene.FindNode( _Node.Name + ".Target", true );

			foreach ( Scene.Nodes.Node Child in _Node.Children )
				PostProcessNodes( Child );
		}

		/// <summary>
		/// Creates a Cirrus mesh node
		/// </summary>
		/// <param name="_Scene"></param>
		/// <param name="_FBXMesh"></param>
		/// <returns></returns>
		protected Scene.Nodes.Mesh	CreateMesh( FBXImporter.NodeMesh _FBXMesh, Scene.Nodes.Node _Parent )
		{
			// Create a temporary mesh that will be optimized later, when all meshes have been loaded
			LoaderTempMesh	TempMesh = new LoaderTempMesh( this, _FBXMesh.Name );

			if ( m_bGenerateBoundingBoxes )
				TempMesh.BoundingBox = new WMath.BoundingBox( m_ScaleFactor * _FBXMesh.BoundingBox.m_Min, m_ScaleFactor * _FBXMesh.BoundingBox.m_Max );

			// Handle pivot & reset X-Form
			if ( m_bResetXForm )
				TempMesh.Pivot = _FBXMesh.Pivot;	// Storing the mesh's pivot will have the effect of composing vertices with that matrix, actually performing the "reset X-Form" operation
// 			else
// 				Transform.Pivot = _FBXNode.Pivot;	// Storing the mesh's pivot here will simply compose the mesh's transform with its pivot

			// Setup compulsory vertices and triangles
			Point[]	SourceVertices = _FBXMesh.Vertices;
			Point[]	ScaledVertices = new Point[SourceVertices.Length];
			for ( int VertexIndex=0; VertexIndex < _FBXMesh.VerticesCount; VertexIndex++ )
				ScaledVertices[VertexIndex] = m_ScaleFactor * SourceVertices[VertexIndex];

			TempMesh.SetVertices( ScaledVertices );
			TempMesh.SetFaces( _FBXMesh.Triangles );

			// Setup all the possible recognized layers
			foreach ( FBXImporter.Layer Layer in _FBXMesh.Layers )
				foreach ( FBXImporter.LayerElement LE in Layer.Elements )
				{
					switch ( LE.ElementType )
					{
						case FBXImporter.LayerElement.ELEMENT_TYPE.MATERIAL:
						case FBXImporter.LayerElement.ELEMENT_TYPE.UV:
						case FBXImporter.LayerElement.ELEMENT_TYPE.NORMAL:
						case FBXImporter.LayerElement.ELEMENT_TYPE.TANGENT:
						case FBXImporter.LayerElement.ELEMENT_TYPE.BINORMAL:
						case FBXImporter.LayerElement.ELEMENT_TYPE.VERTEX_COLOR:
						case FBXImporter.LayerElement.ELEMENT_TYPE.SMOOTHING:
							TempMesh.AddLayerElement( LE );
							break;

						default:
							break;	// Other types are not supported (or irrelevant)...
					}
				}

			// Build un-optimized primitives
			TempMesh.BuildPrimitives();

			// Create the cirrus mesh and tie it to our temporary mesh
			FBX.Scene.Nodes.Mesh	Mesh = m_Scene.CreateMesh( _FBXMesh.Name, _Parent, _FBXMesh.LocalTransform );
			m_TempMesh2FinalMesh[TempMesh] = Mesh;

			// Add some properties
			Mesh.Visible = _FBXMesh.Visible;

// 			FBXImporter.ObjectProperty	PropertyCastShadow = _FBXMesh.FindUserProperty( "CastShadow" );
// 			Mesh.CastShadow = PropertyCastShadow != null ? (bool) PropertyCastShadow.Value : true;
			Mesh.CastShadow = true;
			Mesh.ReceiveShadow = true;

			return Mesh;
		}

		/// <summary>
		/// Creates a Cirrus camera node
		/// </summary>
		/// <param name="_FBXMesh"></param>
		/// <param name="_Parent"></param>
		/// <returns></returns>
		protected Scene.Nodes.Camera	CreateCamera( FBXImporter.NodeCamera _FBXCamera, Scene.Nodes.Node _Parent )
		{
			// Create the cirrus mesh and tie it to our temporary mesh
			Scene.Nodes.Camera	Camera = m_Scene.CreateCamera( _FBXCamera.Name, _Parent, _FBXCamera.LocalTransform );

			// Add some properties
			Camera.Visible = _FBXCamera.Visible;
			Camera.Type = (Scene.Nodes.Camera.PROJECTION_TYPE) _FBXCamera.ProjectionType;
			Camera.Target = m_ScaleFactor * (Vector) _FBXCamera.Target;
			Camera.FOV = _FBXCamera.FOVY;
			Camera.AspectRatio = _FBXCamera.AspectRatio;
			Camera.ClipNear = _FBXCamera.NearClipPlane * m_ScaleFactor;
			Camera.ClipFar = _FBXCamera.FarClipPlane * m_ScaleFactor;
			Camera.Roll = _FBXCamera.Roll;

			return Camera;
		}

		/// <summary>
		/// Creates a Cirrus light node
		/// </summary>
		/// <param name="_FBXMesh"></param>
		/// <param name="_Parent"></param>
		/// <returns></returns>
		protected Scene.Nodes.Light	CreateLight( FBXImporter.NodeLight _FBXLight, Scene.Nodes.Node _Parent )
		{
			// Create the cirrus mesh and tie it to our temporary mesh
			Scene.Nodes.Light	Light = m_Scene.CreateLight( _FBXLight.Name, _Parent, _FBXLight.LocalTransform );

			// Add some properties
			Light.Visible = _FBXLight.Visible;
			Light.Type = (Scene.Nodes.Light.LIGHT_TYPE) _FBXLight.LightType;
			Light.CastShadow = _FBXLight.CastShadows;
			Light.Color = _FBXLight.Color;
			Light.Intensity = _FBXLight.Intensity;
			Light.EnableNearAttenuation = _FBXLight.EnableNearAttenuation;
			Light.NearAttenuationStart = _FBXLight.NearAttenuationStart * m_ScaleFactor;
			Light.NearAttenuationEnd = _FBXLight.NearAttenuationEnd * m_ScaleFactor;
			Light.EnableFarAttenuation = _FBXLight.EnableFarAttenuation;
			Light.FarAttenuationStart = _FBXLight.FarAttenuationStart * m_ScaleFactor;
			Light.FarAttenuationEnd = _FBXLight.FarAttenuationEnd * m_ScaleFactor;
			Light.HotSpot = _FBXLight.HotSpot;
			Light.ConeAngle = _FBXLight.ConeAngle;
			Light.DecayType = (Scene.Nodes.Light.DECAY_TYPE) _FBXLight.DecayType;
			Light.DecayStart = _FBXLight.DecayStart * m_ScaleFactor;

			return Light;
		}

		/// <summary>
		/// Optimizes the existing meshes and build the primitives necessary for runtime display
		/// This will attempt to compact identical meshes and also consolidate mesh primitives
		/// </summary>
		protected void	BuildConsolidatedMeshes()
		{
			// 1] Retrieve all existing meshes and compact identical instances
			List<LoaderTempMesh>	CompactedMeshes = new List<LoaderTempMesh>();
			foreach ( LoaderTempMesh M in m_TempMesh2FinalMesh.Keys )
			{
				// Check the existing meshes to see if they might be a master to this mesh
				if ( m_bCompactIdenticalMeshes )
					foreach ( LoaderTempMesh MasterMesh in CompactedMeshes )
						if ( M.MergeWithMasterMesh( MasterMesh ) )
							break;	// We found this mesh's master!

				CompactedMeshes.Add( M );
			}

			// 2] Consolidate master meshes
//			WMath.Global.PushEpsilon( 1e-3f );	// Use this new epsilon for float comparisons in the Math library...

			foreach ( LoaderTempMesh M in CompactedMeshes )
				M.PerformConsolidation();

//			WMath.Global.PopEpsilon();

			// 3] Rebuild slave meshes from consolidated meshes
			foreach ( LoaderTempMesh M in CompactedMeshes )
				M.RebuildFromMasterMesh();


			// 4] Convert the mesh into a clean consolidated mesh
			PrimitiveBuilder	Builder = new PrimitiveBuilder();
			foreach ( LoaderTempMesh M in CompactedMeshes )
			{
				Scene.Nodes.Mesh	TargetMesh = m_TempMesh2FinalMesh[M];

				// Setup basic mesh infos
                TargetMesh.BBox = M.BoundingBox;

				// I know it's a bit of a lousy approximation for the b-sphere but we can always refine it later...
                TargetMesh.BSphere = new BoundingSphere( M.BoundingBox.Center, 0.5f * M.BoundingBox.Dim.Magnitude() );

				// Build primitives
				int	PrimitiveIndex = 0;
				foreach ( LoaderTempMesh.Primitive P in M.ConsolidatedPrimitives )
				{
					if ( P.Material == null )
						throw new Exception( "Primitive \"" + P.Name + "\" has no assigned material!" );

 					Scene.Materials.MaterialParameters	MatParams = m_Material2Parameters[P.Material];

// 					if ( !m_Material2Technique.ContainsKey( P.Material ) )
// 						continue;	// Un-supported...
 
 					// Create the temporary builder that will handle the primitive conversion
					Builder.CreatePrimitive( P, TargetMesh, M.Name + "." + PrimitiveIndex++, MatParams );
				}
			}
		}

		#region Conversion Helpers

// 		public static SharpDX.Vector3	ConvertPoint( Point _Value )
// 		{
//			return new SharpDX.Vector3(_Value.x, _Value.y, _Value.z);
// 		}
// 
//		public static SharpDX.Vector2 ConvertVector( Vector2D _Value )
// 		{
//			return new SharpDX.Vector2(_Value.x, _Value.y);
// 		}
// 
//		public static SharpDX.Vector3 ConvertVector( Vector _Value )
// 		{
//			return new SharpDX.Vector3(_Value.x, _Value.y, _Value.z);
// 		}
// 
//		public static SharpDX.Vector4 ConvertVector( Vector4D _Value )
// 		{
//			return new SharpDX.Vector4(_Value.x, _Value.y, _Value.z, _Value.w);
// 		}
// 
//		public static SharpDX.Matrix		ConvertMatrix( Matrix4x4 _Value )
// 		{
// 			return ConvertMatrix( _Value, 1.0f );
// 		}
// 
//		public static SharpDX.Matrix		ConvertMatrix( Matrix4x4 _Value, float _PositionScaleFactor )
// 		{
//			SharpDX.Matrix Result = new SharpDX.Matrix();
// 			Vector4D	Row = _Value.GetRow0();
// 			Result.Row1 = new SharpDX.Vector4( Row.x, Row.y, Row.z, Row.w );
// 			Row = _Value.GetRow1();
// 			Result.Row2 = new SharpDX.Vector4( Row.x, Row.y, Row.z, Row.w );
// 			Row = _Value.GetRow2();
// 			Result.Row3 = new SharpDX.Vector4( Row.x, Row.y, Row.z, Row.w );
// 			Point4D	Trans = _Value.GetTrans();
// 			Result.Row4 = new SharpDX.Vector4( _PositionScaleFactor * Trans.x, _PositionScaleFactor * Trans.y, _PositionScaleFactor * Trans.z, Trans.w );
// 
// 			return Result;
// 		}

		/// <summary>
		/// Creates a texture parameter from a material property that contains a texture (e.g. DiffuseColor, SpecularColor, etc.)
		/// </summary>
		/// <param name="_Material"></param>
		/// <param name="_PropertyName"></param>
		/// <param name="_MaterialParameters"></param>
		/// <param name="_ParameterName"></param>
		/// <returns>True if a texture is available</returns>
		protected bool	CreateTextureParameter( FBXImporter.Material _Material, string _PropertyName, Scene.Materials.MaterialParameters _MaterialParameters, string _ParameterName )
		{
			return CreateTextureParameter( _Material, _PropertyName, _MaterialParameters, _ParameterName, null );
		}

		protected bool	CreateTextureParameter( FBXImporter.Material _Material, string _PropertyName, Scene.Materials.MaterialParameters _MaterialParameters, string _ParameterName, string _OpacityPropertyName )
		{
			FBXImporter.ObjectProperty	Property = _Material.FindProperty( _PropertyName );
			if ( Property == null )
				return false;	// No such property...
			if ( Property.Textures.Length == 0 )
				return false;	// That property has no texture...

			// Check for opacity
			string	OpacityTextureRelativeFileName = null;
			if ( _OpacityPropertyName != null )
			{
				FBXImporter.ObjectProperty	OpacityProperty = _Material.FindProperty( _OpacityPropertyName );
				if ( OpacityProperty != null && OpacityProperty.Textures.Length != 0 )
					OpacityTextureRelativeFileName = OpacityProperty.Textures[0].RelativeFileName;
			}

			// Create the parameter with that texture
			Scene.Materials.Texture2D	Tex = m_Scene.CreateTexture( Property.Textures[0].RelativeFileName, OpacityTextureRelativeFileName, Property.Textures[0].UseMipMap );
			_MaterialParameters.CreateParameter( _ParameterName, Scene.Materials.MaterialParameters.PARAMETER_TYPE.TEXTURE2D ).AsTexture2D.Value = Tex;

			return true;
		}

		#endregion

		#endregion
	}
}
