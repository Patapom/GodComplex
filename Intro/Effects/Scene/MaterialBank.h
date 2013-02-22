//////////////////////////////////////////////////////////////////////////
// This is the bank of complex BRDF materials
// 
#pragma once

template<typename> class CB;
template<typename,int> class TB;
class	Texture2D;

class MaterialBank
{
private:	// CONSTANTS

public:		// NESTED TYPES

	class	Material
	{
	public:			// NESTED TYPES

		// Static parameters are issued by the BRDF Explorer http://patapom.com/topics/WebGL/BRDF/
		struct	StaticParameters
		{
			// Specular & Fresnel parameters
			float	AmplitudeX;
			float	AmplitudeY;
			float	FalloffX;
			float	FalloffY;
			float	ExponentX;
			float	ExponentY;
			float	Offset;

			// Diffuse parameters
			float	DiffuseReflectance;
			float	DiffuseRoughness;
		};

		// Dynamic parameters are tied to the layer using the material and could vary per-primitive if necessary
		struct	DynamicParameters 
		{
			float	Thickness;			// Material thickness in millimeters
			float	Opacity;			// Opacity in [0,1]
			float	IOR;				// Index of Refraction
			float	Frosting;			// A frosting coefficient in [0,1]
		};

	protected:		// FIELDS

		MaterialBank*		m_pOwner;
		const char*			m_pName;
		StaticParameters	m_Static;
		DynamicParameters	m_Dynamic;

	public:		// PROPERTIES

		const StaticParameters&		GetStatic() const	{ return m_Static; }
		const DynamicParameters&	GetDynamic() const	{ return m_Dynamic; }

	public:		// METHODS

// 		Material();
// 		~Material();

		void	SetStaticParameters( MaterialBank& _Owner, const char* _pName, const StaticParameters& _Parameters );
		void	SetDynamicParameters( const DynamicParameters& _Parameters );
	};


private:	// FIELDS

	Device&		m_Device;
	int			m_MaterialsCount;
	Material*	m_pMaterials;

	// The texture buffer (texture slot #8) that will contain our static parameters
	// Valid for the entire scene
	bool		m_TB_MaterialsDirty;
	TB<Material::StaticParameters,64>*	m_pTB_Materials;


public:		// PROPERTIES

	Material&	GetMaterialAt( int _Index );


public:		// METHODS

	MaterialBank( Device& _Device );
	~MaterialBank();

	void	AllocateMaterials( int _MaterialsCount );
	void	DestroyMaterials();

	// Updates and uploads any change to the material buffer
	void	UpdateMaterialsBuffer();
};
