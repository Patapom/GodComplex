// ================================================================================================
// SpatialHashing.h
// 
// 	Implements a spatial hashing scheme by storing 3D positions into a discretized grid cell and using a unique hash to encode the position.
// 	Based on http://www.beosil.com/download/CollisionDetectionHashing_VMV03.pdf
// ================================================================================================
//
#pragma once

#include "../Types.h"

namespace BaseLib {

template < typename _type_ >
class SpatialHashing {
private:

	struct keyValue_t {
		keyValue_t*		next;
		U32				hash;
		bfloat3			position;	// a.k.a. the key...
		_type_			value;
	};

	int				m_maxEntries;
	keyValue_t*		m_nodesPool;
	keyValue_t*		m_freeNode;

	int				m_tableSize;
	keyValue_t**	m_table;

	bfloat3			m_cellSize;
	bfloat3			m_invCellSize;

	int				m_entriesCount;

public:

	typedef keyValue_t*		entryHandle_t;

	SpatialHashing();
	~SpatialHashing();

	// Initializes the tables for the specified maximum amount of entries
	void			Init( int _maxEntries );

	// Clears the entire table
	void			Clear();

	// Sets the resolution of the grid cells by which the positions are descretized
	void			SetGridCellSize( const bfloat3& _cellSize ) {
		ASSERT( _cellSize.x > 1e-6f && _cellSize.y > 1e-6f && _cellSize.z > 1e-6f, "Cell size is too small!" );
		m_cellSize = _cellSize;
		m_invCellSize.Set( 1.0f / _cellSize.x, 1.0f / _cellSize.y, 1.0f / _cellSize.z );
	}

	// Returns the amount of entries in the table
	int				Num() const		{ return m_entriesCount; }

	// Adds a new entry to the table
	// Returns the handle to the added entry
	entryHandle_t	Add( const bfloat3& _position, const _type_& _value );
	_type_&			Add( const bfloat3& _position );

	// Removes an entry from the table
	bool			Remove( entryHandle_t _handle );

	// Update the entry's position
	bool			Update( entryHandle_t _handle, const bfloat3& _newPosition );

	// Gets the entry's current position and value (optionally, you can also get the hash used by the construct)
	const bfloat3&	GetPositionAndValue( entryHandle_t _handle,  _type_& _value, U32* _hash=NULL );
	const bfloat3&	GetPositionAndValuePtr( entryHandle_t _handle, _type_*& _value, U32* _hash=NULL );

	// Retrieve the first entry by its position
	_type_*			Find( const bfloat3& _position, float _epsilon=1e-3f ) const;

	// Retrieve all coincident entries by their position
	void			FindAll( const bfloat3& _position, List< _type_* >& _result, float _epsilon=1e-3f ) const;

	// Retrieve all coincident entries by their position, also searches in neighbor cells so the results are exhaustive
	// NOTE: Neighbor cells are consulted only if within _epsilon threshold
	void			FindAllIncludeNeighborCells( const bfloat3& _position, List< _type_* >& _result, float _epsilon=1e-3f ) const;

	// Retrieves the the cell for a given position
	void			GetCellIndices( const bfloat3& _position, int& _cellX, int& _cellY, int& _cellZ ) const;

	// Gives the center of a cell given its coordinates
	bfloat3			GetCellCenter( int _cellX, int _cellY, int _cellZ ) const;

	// Fills a list with all the values stored in a given cell
	_type_*			FindFirstValueInCell( int _cellX, int _cellY, int _cellZ ) const;
	void			FindAllValuesInCell( int _cellX, int _cellY, int _cellZ, List< _type_ >& _values ) const;
	void			FindAllValuePointersInCell( int _cellX, int _cellY, int _cellZ, List< _type_* >& _values ) const;

	// Spatial hashing from http://www.beosil.com/download/CollisionDetectionHashing_VMV03.pdf, section 4.1
	//
	static U32	Hash( int X, int Y, int Z ) {
		const U64	p1 = 73856093;
		const U64	p2 = 19349663;
		const U64	p3 = 83492791;

		U64	HashX = (U64) X * p1;
		U64	HashY = (U64) Y * p2;
		U64	HashZ = (U64) Z * p3;
		U64	Hash = HashX ^ HashY ^ HashZ ;
		return U32( Hash );
	}
	U32	ComputeHash( int X, int Y, int Z ) const {
		return SpatialHashing::Hash( X, Y, Z ) % m_tableSize;
	}

	U32	ComputeHash( const bfloat3& _position ) const {
		int	cellX, cellY, cellZ;
		GetCellIndices( _position, cellX, cellY, cellZ );
		U32	hash = ComputeHash( cellX, cellY, cellZ );
		return hash;
	}

private:

	keyValue_t*		Alloc();

	static const int	MAX_SUPPORTED_POT = 29;
	static long			ms_PowerOfTwoNextPrimes[];
};

template < typename _type_ >
long SpatialHashing< _type_ >::ms_PowerOfTwoNextPrimes[] = {
	1,			// >= 1				= 2^0
	2,			// >= 2				= 2^1
	5,			// >  4				= 2^2
	11,			// >  8				= 2^3
	17,			// >  16			= 2^4
	37,			// >  32			= 2^5
	67,			// >  64			= 2^6
	131,		// >  128			= 2^7
	257,		// >  256			= 2^8
	521,		// >  512			= 2^9
	1031,		// >  1024			= 2^10
	2053,		// >  2048			= 2^11
	4099,		// >  4096			= 2^12
	8209,		// >  8192			= 2^13
	16411,		// >  16384			= 2^14
	32771,		// >  32768			= 2^15
	65537,		// >  65536			= 2^16
	131101,		// >  131072		= 2^17
	262147,		// >  262144		= 2^18
	524309,		// >  524288		= 2^19
	1048583,	// >  1048576		= 2^20
	2097169,	// >  2097152		= 2^21
	4194319,	// >  4194304		= 2^22
	8388617,	// >  8388608		= 2^23
	16777259,	// >  16777216		= 2^24
	33554467,	// >  33554432		= 2^25
	67108879,	// >  67108864		= 2^26
	134217757,	// >  134217728		= 2^27
	268435459,	// >  268435456		= 2^28
	536870923,	// >  536870912		= 2^29
// 	???????,	// >  1073741824	= 2^30
// 	???????,	// >  2147483648	= 2^31
// 	???????,	// >  4294967296	= 2^32
	// ARKANE: bmayaux (2014-09-01) Add some others if _maxEntries is superior to 2^MAX_SUPPORTED_POT ...
	// NOTE: Also update MAX_SUPPORTED_POT!
};

template < typename _type_ >
SpatialHashing< _type_ >::SpatialHashing()
	: m_maxEntries( 0 )
	, m_tableSize( 0 )
	, m_entriesCount( 0 )
	, m_table( NULL )
	, m_nodesPool( NULL )
	, m_freeNode( NULL ) {

	// Assume a 1 unit grid cell size
	m_cellSize = bfloat3::One;
	m_invCellSize = bfloat3::One;
}

template < typename _type_ >
SpatialHashing< _type_ >::~SpatialHashing() {
	SAFE_DELETE_ARRAY( m_nodesPool );
	SAFE_DELETE_ARRAY( m_table );
}

template < typename _type_ >
void SpatialHashing< _type_ >::Init( int _maxEntries ) {
	m_maxEntries = _maxEntries;

	SAFE_DELETE_ARRAY( m_nodesPool );
	SAFE_DELETE_ARRAY( m_table );

	// Initialize hashtable entries count to the next prime number above the nearest power of two of the specified max entries count
	U32	POT = U32( ceilf( log2f( float( m_maxEntries ) ) ) );
	ASSERT( POT <= MAX_SUPPORTED_POT, "Table of primes must be augmented because it only supports up to 2^29 entries!" );
	
	m_tableSize = ms_PowerOfTwoNextPrimes[POT];
	m_table = new keyValue_t*[m_tableSize];

	// Initialize the pool of entry nodes
	m_nodesPool = _maxEntries > 0 ? new keyValue_t[_maxEntries] : NULL;

	// Initialize all
	Clear();
}

template < typename _type_ >
void SpatialHashing< _type_ >::Clear() {
	memset( m_table, 0, m_tableSize * sizeof(keyValue_t*) );
	if ( m_maxEntries > 0 ) {
		memset( m_nodesPool, 0, m_maxEntries*sizeof(keyValue_t) );
		for ( int nodeIndex=0; nodeIndex < m_maxEntries-1; nodeIndex++ ) {
			m_nodesPool[nodeIndex].next = &m_nodesPool[nodeIndex+1];
			m_nodesPool[nodeIndex].hash = ~0U;
		}
		m_nodesPool[m_maxEntries-1].next = nullptr;
	}
	m_freeNode = m_nodesPool;	// All free!
	m_entriesCount = 0;
}

template < typename _type_ >
typename SpatialHashing< _type_ >::entryHandle_t SpatialHashing< _type_ >::Add( const bfloat3& _position, const _type_& _value ) {
	U32		hash = ComputeHash( _position );

	keyValue_t*	firstEntry = m_table[hash];

	keyValue_t*	newEntry = Alloc();
	if ( newEntry != NULL ) {
		newEntry->next = firstEntry;
		newEntry->hash = hash;
		newEntry->position = _position;
		newEntry->value = _value;

		m_table[hash] = newEntry;	// We're the new first entry
	}

	return static_cast< entryHandle_t >( newEntry );
}


template < typename _type_ >
_type_& SpatialHashing< _type_ >::Add( const bfloat3& _position ) {
	U32		hash = ComputeHash( _position );

	keyValue_t*	firstEntry = m_table[hash];

	keyValue_t*	newEntry = Alloc();
	newEntry->next = firstEntry;
	newEntry->hash = hash;
	newEntry->position = _position;

	m_table[hash] = newEntry;	// We're the new first entry

	return newEntry->value;
}

template < typename _type_ >
bool SpatialHashing< _type_ >::Remove( entryHandle_t _handle ) {
	keyValue_t*	entry = static_cast< keyValue_t* >( _handle );
	ASSERT( entry != NULL, "Invalid entry!" );

	keyValue_t*	previous = nullptr;
	keyValue_t*	current = m_table[entry->hash];
	while ( current != nullptr ) {
		if ( current != entry ) {
			previous = current;
			current = current->next;
			continue;
		}

		// Found it!

		// Link over that node
		if ( previous != nullptr ) {
			previous->next = current->next;
		} else {
			m_table[entry->hash] = current->next;
		}

		// Add the node back to the free list
		current->next = m_freeNode;
		m_freeNode = current;

		m_entriesCount--;

		return true;
	}

	return false;	// Not found...
}

template < typename _type_ >
bool SpatialHashing< _type_ >::Update( entryHandle_t _handle, const bfloat3& _newPosition ) {
	keyValue_t*	entry = static_cast< keyValue_t* >( _handle );
	ASSERT( entry != NULL, "Invalid entry!" );

	entry->position = _newPosition;
	U32	newHash = ComputeHash( _newPosition );
	if ( newHash == entry->hash ) {
		return true;	// No change in hash...
	}

	// We must first retrieve the existing entry in the linked list of entries sharing that hash
	keyValue_t*	previous = nullptr;
	keyValue_t*	current = m_table[entry->hash];
	while ( current != nullptr ) {
		if ( current != entry ) {
			previous = current;
			current = current->next;
			continue;
		}

		// Found it!

		// Link over that node to remove it from this hash's list of entries
		if ( previous != nullptr ) {
			previous->next = current->next;
		} else {
			m_table[entry->hash] = current->next;
		}

		// Update its hash
		entry->hash = newHash;

		// Re-link it to its new position in the table
		entry->next = m_table[newHash];
		m_table[newHash] = entry;

		return true;
	}

	return false;	// Not found...
}

template < typename _type_ >
const bfloat3& SpatialHashing< _type_ >::GetPositionAndValue( entryHandle_t _handle, _type_& _value, U32* _hash ) {
	keyValue_t*	entry = static_cast< keyValue_t* >( _handle );
	ASSERT( entry != NULL, "Invalid entry!" );

	_value = entry->value;
	if ( _hash != NULL ) {
		*_hash = entry->hash;
	}

	return entry->position;
}

template < typename _type_ >
const bfloat3& SpatialHashing< _type_ >::GetPositionAndValuePtr( entryHandle_t _handle, _type_*& _value, U32* _hash ) {
	keyValue_t*	entry = static_cast< keyValue_t* >( _handle );
	ASSERT( entry != NULL, "Invalid entry!" );

	_value = &entry->value;
	if ( _hash != NULL ) {
		*_hash = entry->hash;
	}

	return entry->position;
}

template < typename _type_ >
_type_* SpatialHashing< _type_ >::Find( const bfloat3& _position, float _epsilon ) const {
	U32	hash = ComputeHash( _position );

	keyValue_t*	current = m_table[hash];
	while ( current != nullptr ) {
		if ( current->position.Compare( _position, _epsilon ) ) {
			return &current->value;	// Found it!
		}
		current = current->next;
	}

	return nullptr;	// Not found...
}

template < typename _type_ >
void	SpatialHashing< _type_ >::FindAll( const bfloat3& _position, List< _type_* >& _result, float _epsilon ) const {
	U32	hash = ComputeHash( _position );

	keyValue_t*	current = m_table[hash];
	while ( current != nullptr ) {
		if ( current->position.Compare( _position, _epsilon ) ) {
			_result.Append( &current->value );	// Another match!
		}
		current = current->next;
	}
}

template < typename _type_ >
void	SpatialHashing< _type_ >::FindAllIncludeNeighborCells( const bfloat3& _position, List< _type_* >& _result, float _epsilon ) const {

	int	minCellX, minCellY, minCellZ;
	GetCellIndices( _position - _epsilon*bfloat3::one, minCellX, minCellY, minCellZ );

	int	maxCellX, maxCellY, maxCellZ;
	GetCellIndices( _position + _epsilon*bfloat3::one, maxCellX, maxCellY, maxCellZ );

	for ( int Z=minCellZ; Z <= maxCellZ; Z++ ) {
		for ( int Y=minCellY; Y <= maxCellY; Y++ ) {
			for ( int X=minCellX; X <= maxCellX; X++ ) {
				U32		hash = ComputeHash( X, Y, Z );
				keyValue_t*	current = m_table[hash];
				while ( current != nullptr ) {
					if ( current->position.Compare( _position, _epsilon ) ) {
						_result.Append( &current->value );	// Another match!
					}
					current = current->next;
				}
			}
		}
	}
}

template < typename _type_ >
void	SpatialHashing< _type_ >::GetCellIndices( const bfloat3& _position, int& _cellX, int& _cellY, int& _cellZ ) const {
	_cellX = int( idMath::Floor( _position.x * m_invCellSize.x ) );
	_cellY = int( idMath::Floor( _position.y * m_invCellSize.y ) );
	_cellZ = int( idMath::Floor( _position.z * m_invCellSize.z ) );
}

template < typename _type_ >
bfloat3	SpatialHashing< _type_ >::GetCellCenter( int _cellX, int _cellY, int _cellZ ) const {
	bfloat3	center;
	center.x = (_cellX + 0.5f) * m_cellSize.x;
	center.y = (_cellY + 0.5f) * m_cellSize.y;
	center.z = (_cellZ + 0.5f) * m_cellSize.z;
	return center;
}

template < typename _type_ >
_type_*	SpatialHashing< _type_ >::FindFirstValueInCell( int _cellX, int _cellY, int _cellZ ) const {
	U32		hash = ComputeHash( _cellX, _cellY, _cellZ );	
	keyValue_t*	current = m_table[hash];
	while ( current != nullptr ) {

		int	entryCellX, entryCellY, entryCellZ;
		GetCellIndices( current->position, entryCellX, entryCellY, entryCellZ );
		if ( entryCellX == _cellX && entryCellY == _cellY && entryCellZ == _cellZ ) {
			return &current->value;
		}
		current = current->next;
	}
	return nullptr;
}

template < typename _type_ >
void	SpatialHashing< _type_ >::FindAllValuesInCell( int _cellX, int _cellY, int _cellZ, List< _type_ >& _values ) const {
	U32	hash = ComputeHash( _cellX, _cellY, _cellZ );

	keyValue_t*	current = m_table[hash];
	while ( current != nullptr ) {

		int	entryCellX, entryCellY, entryCellZ;
		GetCellIndices( current->position, entryCellX, entryCellY, entryCellZ );
		if ( entryCellX == _cellX && entryCellY == _cellY && entryCellZ == _cellZ ) {
			_values.Append( current->value );
		}
		current = current->next;
	}
}

template < typename _type_ >
void	SpatialHashing< _type_ >::FindAllValuePointersInCell( int _cellX, int _cellY, int _cellZ, List< _type_* >& _values ) const {
	U32	hash = ComputeHash( _cellX, _cellY, _cellZ );

	keyValue_t*	current = m_table[hash];
	while ( current != nullptr ) {

		int	entryCellX, entryCellY, entryCellZ;
		GetCellIndices( current->position, entryCellX, entryCellY, entryCellZ );
		if ( entryCellX == _cellX && entryCellY == _cellY && entryCellZ == _cellZ ) {
			_values.Append( &current->value );
		}
		current = current->next;
	}
}

template < typename _type_ >
typename SpatialHashing< _type_ >::keyValue_t* SpatialHashing< _type_ >::Alloc() {
	RELEASE_ASSERT( m_freeNode != NULL, "Table is full! Consider increasing _maxEntries!" );
	if ( m_freeNode == NULL ) {
		return NULL;
	}

	keyValue_t*	node = m_freeNode;
	m_freeNode = node->next;
	node->next = nullptr;
	node->hash = ~0U;

	m_entriesCount++;

	return node;
}

}	// namespace BaseLib}