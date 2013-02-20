//////////////////////////////////////////////////////////////////////////
// This is the main scene manager that handles objects & their primitives
// Each primitive is part of an object and is tied to a bunch of complex materials
// 
#pragma once

#include "../../../NuajAPI/API/Hashtable.h"
// #include "../../../RendererD3D11/Renderer.h"
// #include "../../../RendererD3D11/Device.h"

template<typename> class CB;

class Scene
{
private:	// CONSTANTS

public:		// NESTED TYPES

	class	Object
	{
	public:		// NESTED TYPES

		class	Primitive
		{
		public:		// NESTED TYPES

			struct	MaterialParameters
			{
				Texture2D*	pTextures;
				int			MatIDs[4];
				NjFloat3	Thickness;
				NjFloat3	Extinction;
				NjFloat3	IOR;
			};

			struct	CBPrimitive
			{
				int			MatIDs[4];	// 4 material IDs in [0,255], one for each layer of the primitive
				NjFloat3	Thickness;	// The thickness of the 3 top layers
				float		__Pad0;
				NjFloat3	Extinction;	// The extinction coefficients of the 3 top layers
				float		__Pad1;
				NjFloat3	IOR;		// The IOR of the 3 top layers
				float		__Pad2;

				// TODO: Add tiling + offset for each layer
			};

		protected:	// FIELDS

			Object&				m_Owner;

			CB<CBPrimitive>*	m_pCB_Primitive;
			::Primitive*		m_pPrimitive;		// Actual renderable primitive
			Texture2D*			m_pTextures;		// Texture2DArray with 4 layers + normal + specular

		public:		// PROPERTIES

		public:		// METHODS

			Primitive( Object& _Owner );
			~Primitive();

			void	Render( Material& _Material, bool _bDepthPass=false ) const;

			void	SetRenderPrimitive( ::Primitive& _Primitive );
			void	SetMaterial( MaterialParameters& _Material );
		};

		struct	CBObject
		{
			NjFloat4x4	Local2World;
		};

	protected:		// FIELDS

		Scene&			m_Owner;
		const char*		m_pName;

		bool			m_bPRSDirty;
		NjFloat3		m_Position;
		NjFloat4		m_Rotation;	// Rotation as a quaternion
		NjFloat3		m_Scale;

		CB<CBObject>*	m_pCB_Object;

		// The object's primitives
		int				m_PrimitivesCount;
		Primitive**		m_ppPrimitives;

	public:		// METHODS

		Object( Scene& _Owner, const char* _pName );
		~Object();

		void		SetPRS( const NjFloat3& _Position, const NjFloat4& _Rotation, const NjFloat3& _Scale=NjFloat3::One );

		void		Update( float _Time, float _DeltaTime );
		void		Render( Material& _Material, bool _bDepthPass=false ) const;

		// Primitives management
		void		AllocatePrimitives( int _PrimitivesCount );
		void		DestroyPrimitives();
		Primitive&	GetPrimitiveAt( int _PrimitiveIndex );
	};

private:	// FIELDS

	Device&		m_Device;
	int			m_ObjectsCount;
	Object**	m_ppObjects;


public:		// PROPERTIES

public:		// METHODS

	Scene( Device& _Device );
	~Scene();

	void		Update( float _Time, float _DeltaTime );
	void		Render( Material& _Material, bool _bDepthPass=false ) const;

	// Objects management
	void		AllocateObjects( int _ObjectsCount );
	void		DestroyObjects();
	Object&		CreateObjectAt( int _ObjectIndex, const char* _pName );
};


typedef	Scene::Object::Primitive::MaterialParameters	PrimitiveMaterial;
