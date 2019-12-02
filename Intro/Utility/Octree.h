//////////////////////////////////////////////////////////////////////////
// Octree helper
//
#pragma once

#include "../BaseLib/Containers/List.h"

template<typename T> class	Octree
{
private:	// NESTED TYPES

	struct	Content
	{
		float3	Position;
		float	Radius;
		float	SqRadius;
		float3	BBoxMin;
		float3	BBoxMax;
		T		Value;

		bool	Contains( const float3& _Position ) const
		{
			float3	Center2Position = Position - _Position;
			float	SqDistanceFromCenter = Center2Position.LengthSq();
			return SqDistanceFromCenter <= SqRadius;
		}

		bool	IsCloser( const float3& _Position, float& _SqDistance ) const
		{
			float3	Center2Position = Position - _Position;
			float	SqDistanceFromCenter = Center2Position.LengthSq();
			if ( SqDistanceFromCenter >= _SqDistance )
				return false;

			_SqDistance = SqDistanceFromCenter;
			return true;
		}
	};

public:

	class	Node
	{
	public:	// FIELDS

		Octree&			m_Owner;
		Node*			m_pParent;
		Node*			m_ppCells[8];

		List<const Content*>	m_Content;

	public:	// METHODS

		Node( Octree& _Owner, Node* _pParent );
		~Node();

		int			Append( const Content& _Content, const float3& _Min, float _Size, U32 _Level );
		void		Fetch( const float3& _Position, List<T>& _Result, const float3& _Min, float _Size ) const;
		const T*	FetchNearest( const float3& _Position, const float3& _Min, float _Size, float& _SqDistance ) const;

	private:
		Node&		GetOrCreateChildNode( U32 _X, U32 _Y, U32 _Z );
	};

private:	// FIELDS

	float3			m_Min;
	float3			m_Max;
	float			m_Size;
	float			m_MinCellSize;
	Node*			m_pROOT;

	List<Content>	m_ContentPool;

#ifdef _DEBUG
	U32				m_NodesCount;
	U32				m_NodeLevelsCount;
#endif


public:		// PROPERTIES

public:		// METHODS

	Octree();
	~Octree();

	// Initialize the root node of the octree with global scene diemensions
	//	_MinCellSize, the minimum authorized cell size in the octree
	//	_MaxElementsInOctree, if known, initializes the pool of values to the specified maximum. Leave to default if to be dynamically resized.
	Node&		Init( const float3& _BoundMin, float _Size, float _MinCellSize, U32 _MaxElementsInOctree=0 );

	// Appends a value to the octree
	//	_Position, the position of the sphere containing the value
	//	_Radius, the radius of the sphere containing the value
	//	_Value, the value to append
	// Returns the amount of nodes the value was added to
	int			Append( const float3& _Position, float _Radius, T _Value );

	// Fetches the values overlapping the provided position
	//	_Position, the position to find overlapping values for
	//	_Result, the list that will be populated with values overlapping the provided position
	void		Fetch( const float3& _Position, List<T>& _Result ) const;

	// Fetches the value closest to the provided position
	//	_Distance, the distance to the retrieved value
	const T*	FetchNearest( const float3& _Position, float& _Distance ) const;
};

#include "Octree.inl"