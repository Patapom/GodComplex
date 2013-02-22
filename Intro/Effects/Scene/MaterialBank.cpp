#include "../../../GodComplex.h"
#include "MaterialBank.h"

//////////////////////////////////////////////////////////////////////////
// MaterialBank
MaterialBank::MaterialBank( Device& _Device ) : m_Device( _Device ), m_pMaterials( NULL )
{
	m_pTB_Materials = new TB<Material::StaticParameters,64>( _Device, 8, true );
}

MaterialBank::~MaterialBank()
{
	delete m_pTB_Materials;
	DestroyMaterials();
}

void	MaterialBank::AllocateMaterials( int _MaterialsCount )
{
	DestroyMaterials();

	m_MaterialsCount = _MaterialsCount;
	m_pMaterials = new Material[_MaterialsCount];
}

void	MaterialBank::DestroyMaterials()
{
	if ( m_pMaterials == NULL )
		return;

	delete[] m_pMaterials;
	m_pMaterials = NULL;
	m_MaterialsCount = 0;
}

MaterialBank::Material&	MaterialBank::GetMaterialAt( int _Index )
{
	ASSERT( _Index < m_MaterialsCount, "Material index out of range!" );
	return m_pMaterials[_Index];
}

void	MaterialBank::UpdateMaterialsBuffer()
{
	if ( !m_TB_MaterialsDirty )
		return;	// Up to date...

	const float	FALLOFF_GOAL = 0.01f;

	for ( int MaterialIndex=0; MaterialIndex < m_MaterialsCount; MaterialIndex++ )
	{
		Material::StaticParameters&	Params = m_pTB_Materials->m[0];

		// Copy bulk of parameters
		memcpy( &Params, &m_pMaterials[MaterialIndex].GetStatic(), sizeof(Material::StaticParameters) );

		// Process falloff to obtain actual exponential coefficients
		float	x = powf( max( 1e-4f, Params.FalloffX ), Params.ExponentX );	// We must reach the goal at this position
		Params.FalloffX = logf( FALLOFF_GOAL / max( 1e-3f, Params.AmplitudeX ) ) / x;
		float	y = powf( max( 1e-4f, Params.FalloffY ), Params.ExponentY );	// We must reach the goal at this position
		Params.FalloffY = logf( FALLOFF_GOAL / max( 1e-3f, Params.AmplitudeY ) ) / y;
	}

	m_pTB_Materials->UpdateData();
	m_TB_MaterialsDirty = false;
}


//////////////////////////////////////////////////////////////////////////
// Complex "Pom" Material

void	MaterialBank::Material::SetStaticParameters( MaterialBank& _Owner, const char* _pName, const StaticParameters& _Parameters )
{
	m_pOwner = &_Owner;
	m_pName = _pName;
	memcpy( &m_Static, &_Parameters, sizeof(StaticParameters) );

	m_pOwner->m_TB_MaterialsDirty = true;
}

void	MaterialBank::Material::SetDynamicParameters( const DynamicParameters& _Parameters )
{
	memcpy( &m_Dynamic, &_Parameters, sizeof(DynamicParameters) );
}
