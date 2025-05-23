//////////////////////////////////////////////////////////////////////////
// Loads a binary GCX scene generated by the FBXTestConverter tool
//
#pragma once

class	Scene
{
protected:	// CONSTANTS

public:		// NESTED TYPES

	class ISceneTagger;
	class Material;

	class	Node
	{
	public:

		Scene&				m_Owner;
		enum	TYPE {
			GENERIC = 0,
			MESH,
			LIGHT,
			CAMERA,

			// Special nodes
			PROBE,
		}					m_Type;
		Node*				m_pParent;
		int					m_ChildrenCount;
		Node**				m_ppChildren;
		float4x4			m_Local2Parent;
		float4x4			m_Local2World;

		void*				m_pTag;	// Custom user tag filled with anything the user needs to render the node

	private:
		int					m_ChildIndex;		// Set at the beginning of a ForEach loop
		void				SetChildIndex();

	private:
		Node( Scene& _Owner, Node* _pParent );
		~Node();

		void			Init( const U8*& _pData );
		void			Exit();

		void			PlaceTag( ISceneTagger& _SceneTagger );

		// Override this in your inherited classes to init/exit specific details of the node
		virtual void	InitSpecific( const U8*& _pData )				{}
		virtual void	ExitSpecific()									{}
		virtual void	PlaceTagSpecific( ISceneTagger& _SceneTagger )	{}

		friend class Scene;
	};

	class	Light : public Node
	{
	public:

		enum	LIGHT_TYPE {
			POINT = 0,
			DIRECTIONAL,
			SPOT,
		}					m_LightType;
		float3				m_Color;
		float				m_Intensity;
		float				m_HotSpot;	// For spots only
		float				m_Falloff;	// For spots only

	private:

		Light( Scene& _Owner, Node* _pParent );
		~Light();

		virtual void	InitSpecific( const U8*& _pData ) override;

		friend class Scene;
	};

	class	Camera : public Node
	{
	public:

		float				m_FOV;

	private:

		Camera( Scene& _Owner, Node* _pParent );
		~Camera();

		virtual void	InitSpecific( const U8*& _pData ) override;

		friend class Scene;
	};

	class	Mesh : public Node
	{
	public:	// NESTED TYPES

		class	Primitive
		{
		public:
			::Scene::Material*	m_pMaterial;

			float3				m_LocalBBoxMin;
			float3				m_LocalBBoxMax;
			float3				m_GlobalBBoxMin;
			float3				m_GlobalBBoxMax;

			U32					m_FacesCount;
			U32*				m_pFaces;

			enum	VERTEX_FORMAT
			{
				P3N3G3B3T2,		// Position3, Normal3, Tangent3, BiTangent3, UV2

			}					m_VertexFormat;
			U32					m_VerticesCount;
			void*				m_pVertices;

			void*				m_pTag;	// Custom user tag filled with anything the user needs to render the node

			struct VF_P3N3G3B3T2 {
				float3	P;
				float3	N;
				float3	G;
				float3	B;
				float2	T;
			};

		private:
			Primitive();
			~Primitive();

			void			Init( Mesh& _Owner, const U8*& _pData );

			friend class Mesh;
		};

	public:	// FIELDS

		int					m_PrimitivesCount;
		Primitive*			m_pPrimitives;

		float3				m_LocalBBoxMin;
		float3				m_LocalBBoxMax;
		float3				m_GlobalBBoxMin;
		float3				m_GlobalBBoxMax;

	private:

		Mesh( Scene& _Owner, Node* _pParent );
		~Mesh();

		virtual void	InitSpecific( const U8*& _pData ) override;
		virtual void	PlaceTagSpecific( ISceneTagger& _SceneTagger ) override;

		friend class Scene;
	};

	class	Probe : public Node
	{
	private:

		Probe( Scene& _Owner, Node* _pParent );
		~Probe();

		friend class Scene;
	};

	class	Material
	{
	public:	// NESTED TYPES

		class	Texture
		{
		public:
			U32				m_ID;
			void*			m_pTag;

			Texture() : m_ID(-1), m_pTag(NULL) {}
		};

	public:	// FIELDS

		Scene&				m_Owner;

		U32					m_ID;
		float3				m_Ambient;
		float3				m_DiffuseAlbedo;
		Texture				m_TexDiffuseAlbedo;
		float3				m_SpecularAlbedo;
		Texture				m_TexSpecularAlbedo;
		float3				m_SpecularExponent;
		Texture				m_TexNormal;
		float3				m_EmissiveColor;

		void*				m_pTag;	// Custom user tag filled with anything the user needs to render the node

	private:

		Material( Scene& _Owner );

		void	Init( const U8*& _pData );
		void	Exit();

		void	PlaceTag( ISceneTagger& _SceneTagger );

		friend class Scene;
	};


	// Interface passed to the scene loading method to tag abstract scene objects with actual rendering data (also called on scene destruction to clear any leftovers)
	class	ISceneTagger
	{
	public:
		// Tags a material with a special user pointer
		virtual void*	TagMaterial( const Scene& _Scene, Material& _Material ) abstract;

		// Tags a texture with a special user pointer
		virtual void*	TagTexture( const Scene& _Scene, Material::Texture& _Texture ) abstract;

		// Tags a node with a special user pointer
		virtual void*	TagNode( const Scene& _Scene, Node& _Node ) abstract;

		// Tags a primitive with a special user pointer
		virtual void*	TagPrimitive( const Scene& _Scene, Mesh& _Mesh, Mesh::Primitive& _Primitive ) abstract;
	};

	// Interface passed to the scene loading method to tag abstract scene objects with actual rendering data
	class	ISceneRenderer
	{
	public:
		// Renders a mesh
		//	_Mesh, the mesh to render
		//	_pMaterialOverride, an optional material used to override the mesh's default material
		//	_SetMaterial, true to setup the mesh's material (usually, set to false when rendering shadow maps that don't need materials) (except alpha-tested materials)
		virtual void	RenderMesh( const Scene::Mesh& _Mesh, ::Shader* _pMaterialOverride, bool _SetMaterial=true ) abstract;
	};

	// Use a visitor class to browse the scene nodes
	class	IVisitor
	{
	public:
		virtual void	HandleNode( Node& _Node ) abstract;
	};

public:		// FIELDS

	int					m_NodesCount;
	int					m_MeshesCount;
	int					m_LightsCount;
	int					m_CamerasCount;
	int					m_ProbesCount;
	Node*				m_pROOT;

	int					m_MaterialsCount;
	Material**			m_ppMaterials;

	const ISceneTagger*	m_pSceneTagger;


public:		// METHODS

	Scene();
	~Scene();	// WARNING: Call "ClearTags" to dispose of your tags prior destruction!


	void			Load( U16 _SceneResourceID );
	void			PlaceTags( ISceneTagger& _SceneTagger );
	void			Render( ISceneRenderer& _SceneRenderer, bool _SetMaterial=true ) const;
	void			Exit();

	// Prefer using that routine that iterates on all nodes, select the node type yourself, rather than the other ForEach method below
	void			ForEach( IVisitor& _Visitor );

	// !WARNING! I don't know how to write a proper depth-first search, this routine is SLOW AS HELL!!
	// Iterates over all the nodes of specific type
	//	_pPrevious, should be NULL for the first call to trigger a new search
	__declspec(deprecated) Node*	ForEach( Node::TYPE _Type, Node* _pPrevious, int _StartAtChild=0 );

private:

	void			Render( const Node* _pNode, ISceneRenderer& _SceneRenderer, bool _SetTextures ) const;

	void			ForEach( IVisitor& _Visitor, Node* _pParent );

	// Helpers
	Node*			CreateNode( Node* _pParent, const U8*& _pData );
	static U32		ReadU16( const U8*& _pData, bool _IsID=false );
	static U32		ReadU32( const U8*& _pData );
	static float	ReadF32( const U8*& _pData );
	static void		ReadEndMaterialMarker( const U8*& _pData );
	static void		ReadEndNodeMarker( const U8*& _pData );
};
