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
		Material::StaticParameters&	Params = m_pTB_Materials->m[MaterialIndex];

		// Copy bulk of parameters
		memcpy( &Params, &m_pMaterials[MaterialIndex].GetStatic(), sizeof(Material::StaticParameters) );

		// Process falloff to obtain actual exponential coefficients
		float	x = powf( max( 1e-4f, Params.FalloffX ), Params.ExponentX );	// We must reach the goal at this position
		Params.FalloffX = logf( FALLOFF_GOAL / max( 1e-3f, Params.AmplitudeX ) ) / x;
		float	y = powf( max( 1e-4f, Params.FalloffY ), Params.ExponentY );	// We must reach the goal at this position
		Params.FalloffY = logf( FALLOFF_GOAL / max( 1e-3f, Params.AmplitudeY ) ) / y;

		// Store amplitudes in log space
// 		Params.AmplitudeX = logf( Params.AmplitudeX );
// 		Params.AmplitudeY = logf( Params.AmplitudeY );

		// Store amplitudes as [0,1] values
		float	MinX = logf(0.001f);
		float	MaxX = logf(2000.0f);
		Params.AmplitudeX = (logf( Params.AmplitudeX ) - MinX) / (MaxX - MinX);

		float	MinY = logf(0.001f);
		float	MaxY = logf(50.0f);
		Params.AmplitudeY = (logf( Params.AmplitudeY ) - MinY) / (MaxY - MinY);


// float	Offset = float(MaterialIndex) / m_MaterialsCount;
// float	A = 0.25f/9;
// Params.AmplitudeX = Offset + A*0.0f;
// Params.AmplitudeY = Offset + A*1.0f;
// Params.FalloffX = Offset + A*2.0f;
// Params.FalloffY = Offset + A*3.0f;
// Params.ExponentX = Offset + A*4.0f;
// Params.ExponentY = Offset + A*5.0f;
// Params.DiffuseReflectance = Offset + A*6.0f;
// Params.DiffuseRoughness = Offset + A*7.0f;
// Params.Offset = Offset + A*8.0f;
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
