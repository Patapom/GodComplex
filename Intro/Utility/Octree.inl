template<typename T> Octree<T>::Octree()
	: m_pROOT( NULL )
{
}

template<typename T> Octree<T>::~Octree()
{
	delete m_pROOT;
}

template<typename T> typename Octree<T>::Node&	Octree<T>::Init( const float3& _BoundMin, float _Size, float _MinCellSize, U32 _MaxElementsInOctree )
{
	delete m_pROOT;

	m_Min = _BoundMin;
	m_Size = _Size;
	m_Max = _BoundMin + _Size * float3::One;
	m_MinCellSize = _MinCellSize;

	m_pROOT = new Node( *this, NULL );

	if ( _MaxElementsInOctree > 0 )
		m_ContentPool.Init( _MaxElementsInOctree );

#ifdef _DEBUG
	m_NodesCount = 1;
	m_NodeLevelsCount = 1;
#endif

	return *m_pROOT;
}

template<typename T> int	Octree<T>::Append( const float3& _Position, float _Radius, T _Value )
{
	Content&	NewContent = m_ContentPool.Append();
	NewContent.Position = _Position;
	NewContent.Radius = _Radius;
	NewContent.SqRadius = _Radius*_Radius;
	NewContent.BBoxMin = _Position - _Radius * float3::One;
	NewContent.BBoxMax = _Position + _Radius * float3::One;
	NewContent.Value = _Value;

	return m_pROOT->Append( NewContent, m_Min, m_Size, 0 );
}

template<typename T> void	Octree<T>::Fetch( const float3& _Position, List<T>& _Result ) const
{
	m_pROOT->Fetch( _Position, _Result, m_Min, m_Size );
}

template<typename T> const T*	Octree<T>::FetchNearest( const float3& _Position, float& _Distance ) const
{
	float		SqDistance = MAX_FLOAT;
	const T*	pResult = m_pROOT->FetchNearest( _Position, m_Min, m_Size, SqDistance );
	if ( pResult == NULL )
		return NULL;

	_Distance = sqrtf( SqDistance );
	return pResult;
}

template<typename T> Octree<T>::Node::Node( Octree& _Owner, Node* _pParent )
	: m_Owner( _Owner )
	, m_pParent( _pParent )
{
	for ( int i=0; i < 8; i++ )
		m_ppCells[i] = NULL;
}

template<typename T> Octree<T>::Node::~Node()
{
	for ( int i=0; i < 8; i++ )
		delete m_ppCells[i];
}

template<typename T> int	Octree<T>::Node::Append( const Content& _Content, const float3& _Min, float _Size, U32 _Level )
{
#ifdef _DEBUG
	m_Owner.m_NodeLevelsCount = MAX( m_Owner.m_NodeLevelsCount, _Level+1 );
#endif

	float	HalfSize = 0.5f * _Size;
	if (	_Content.Radius >= HalfSize			// Either the content is big enough for that cell
		||	HalfSize <= m_Owner.m_MinCellSize )	// Or we reached the smallest possible cell size
	{	// Store in that node and don't go any further...
		const Content*	PHUCK = &_Content;
		m_Content.Append( PHUCK );
		return 1;
	}

	// Append content to child nodes overlapped by content
	float3	CellCenter = _Min + HalfSize * float3::One;
	U32		XStart = _Content.BBoxMin.x < CellCenter.x ? 0 : 1;
	U32		XEnd = _Content.BBoxMax.x < CellCenter.x ? 1 : 2;
	U32		YStart = _Content.BBoxMin.y < CellCenter.y ? 0 : 1;
	U32		YEnd = _Content.BBoxMax.y < CellCenter.y ? 1 : 2;
	U32		ZStart = _Content.BBoxMin.z < CellCenter.z ? 0 : 1;
	U32		ZEnd = _Content.BBoxMax.z < CellCenter.z ? 1 : 2;
	ASSERT( XEnd > XStart, "Invalid cell span on X!" );
	ASSERT( YEnd > YStart, "Invalid cell span on Y!" );
	ASSERT( ZEnd > ZStart, "Invalid cell span on Z!" );

	int		NodesCount = 0;
	float3	CellMin;
	CellMin.z = _Min.z;
	for ( U32 Z=ZStart; Z < ZEnd; Z++, CellMin.z=CellCenter.z )
	{
		CellMin.y = _Min.y;
		for ( U32 Y=YStart; Y < YEnd; Y++, CellMin.y=CellCenter.y )
		{
			CellMin.x = _Min.x;
			for ( U32 X=XStart; X < XEnd; X++, CellMin.x=CellCenter.x )
			{
				NodesCount += GetOrCreateChildNode( X, Y, Z ).Append( _Content, CellMin, HalfSize, _Level+1 );
			}
		}
	}

	return NodesCount;
}

template<typename T> void	Octree<T>::Node::Fetch( const float3& _Position, List<T>& _Result, const float3& _Min, float _Size ) const
{
	// Collect this node's values
	int	ContentsCount = m_Content.GetCount();
	for ( int ContentIndex=0; ContentIndex < ContentsCount; ContentIndex++ )
	{
		Content&	C = *m_Content[ContentIndex];
		if ( C.Contains( _Position ) )
			_Result.Append( C.Value );
	}

	// Collect child nodes' values
	float	HalfSize = 0.5f * _Size;
	float3	CellCenter = _Min + HalfSize * float3::One;

	U32		ChildIndex = 0;
	float3	CellMin = _Min;
	if ( _Position.x >= CellCenter.x )
	{
		ChildIndex = 1;
		CellMin.x = CellCenter.x;
	}
	if ( _Position.y >= CellCenter.y )
	{
		ChildIndex |= 2;
		CellMin.y = CellCenter.y;
	}
	if ( _Position.z >= CellCenter.z )
	{
		ChildIndex |= 4;
		CellMin.z = CellCenter.z;
	}

	Node*	pChild = m_ppCells[ChildIndex];
	if ( pChild != NULL )
		pChild->Fetch( _Position, _Result, CellMin, HalfSize );
}

template<typename T> const T*	Octree<T>::Node::FetchNearest( const float3& _Position, const float3& _Min, float _Size, float& _SqDistance ) const
{
	// Search this node's values
	const T*	pResult = NULL;
	int	ContentsCount = m_Content.GetCount();
	for ( int ContentIndex=0; ContentIndex < ContentsCount; ContentIndex++ )
	{
		const Content&	C = *m_Content[ContentIndex];
		if ( C.IsCloser( _Position, _SqDistance ) )
			pResult = &C.Value;
	}

	// Search child nodes' values
	float	HalfSize = 0.5f * _Size;
	float3	CellCenter = _Min + HalfSize * float3::One;

	U32		ChildIndex = 0;
	float3	CellMin = _Min;
	if ( _Position.x >= CellCenter.x )
	{
		ChildIndex = 1;
		CellMin.x = CellCenter.x;
	}
	if ( _Position.y >= CellCenter.y )
	{
		ChildIndex |= 2;
		CellMin.y = CellCenter.y;
	}
	if ( _Position.z >= CellCenter.z )
	{
		ChildIndex |= 4;
		CellMin.z = CellCenter.z;
	}

	const T*	pResultChild = NULL;
	Node*		pChild = m_ppCells[ChildIndex];
	if ( pChild != NULL )
		pResultChild = pChild->FetchNearest( _Position, CellMin, HalfSize, _SqDistance );

	return pResultChild != NULL ? pResultChild : pResult;
}

template<typename T> typename Octree<T>::Node&	Octree<T>::Node::GetOrCreateChildNode( U32 _X, U32 _Y, U32 _Z )
{
	U32	ChildIndex = _X | (_Y << 1) | (_Z << 2);
	if ( m_ppCells[ChildIndex] == NULL )
	{
		m_ppCells[ChildIndex] = new Node( m_Owner, this );
#ifdef _DEBUG
		m_Owner.m_NodesCount++;
#endif
	}

	return *m_ppCells[ChildIndex];
}
