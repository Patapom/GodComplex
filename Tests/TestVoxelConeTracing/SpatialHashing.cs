// ================================================================================================
// SpatialHashing.h
// 
// 	Implements a spatial hashing scheme by storing 3D positions into a discretized grid cell and using a unique hash to encode the position.
// 	Based on http://www.beosil.com/download/CollisionDetectionHashing_VMV03.pdf
// ================================================================================================
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SharpMath;

namespace VoxelConeTracing
{
	public class SpatialHashing< T > {

		const int	MAX_SUPPORTED_POT = 29;

		public class keyValue_t {
			public keyValue_t		next;
			public uint				hash;
			public float3			position;	// a.k.a. the key...
			public T				value;
		};

		uint			m_maxEntries;
		keyValue_t[]	m_nodesPool = null;
		keyValue_t		m_freeNode = null;

		uint			m_tableSize = 0;
		keyValue_t[]	m_table = null;

		float3			m_cellSize = float3.One;
		float3			m_invCellSize = float3.One;

		uint			m_entriesCount;

		/// <summary>
		/// Gives the amount of entries in the table
		/// </summary>
		public uint		Count	{ get { return m_entriesCount; } }


		// Initializes the tables for the specified maximum amount of entries
		public void			Init( uint _maxEntries )  {
			m_maxEntries = _maxEntries;

			// Initialize hashtable entries count to the next prime number above the nearest power of two of the specified max entries count
			uint	POT = (uint) Math.Ceiling( Math.Log( m_maxEntries ) / Math.Log( 2 ) );
			if ( POT > MAX_SUPPORTED_POT )
				throw new Exception( "Table of primes must be augmented because it only supports up to 2^29 entries!" );
	
			m_tableSize = ms_PowerOfTwoNextPrimes[POT];
			m_table = new keyValue_t[m_tableSize];

			// Initialize the pool of entry nodes
			m_nodesPool = _maxEntries > 0 ? new keyValue_t[_maxEntries] : null;

			// Initialize all
			Clear();
		}


		// Clears the entire table
		public void			Clear() {
			Array.Clear( m_table, 0, m_table.Length );
			if ( m_maxEntries > 0 ) {
				for ( int nodeIndex=0; nodeIndex < m_maxEntries-1; nodeIndex++ ) {
					m_nodesPool[nodeIndex].next = m_nodesPool[nodeIndex+1];
					m_nodesPool[nodeIndex].hash = ~0U;
				}
				m_nodesPool[m_maxEntries-1].next = null;
			}
			m_freeNode = m_nodesPool[0];	// All free!
			m_entriesCount = 0;
		}


		// Sets the resolution of the grid cells by which the positions are descretized
		public void			SetGridCellSize( float3 _cellSize ) {
			if ( _cellSize.x < 1e-6f || _cellSize.y < 1e-6f || _cellSize.z < 1e-6f )
				throw new Exception( "Cell size is too small!" );

			m_cellSize = _cellSize;
			m_invCellSize.Set( 1.0f / _cellSize.x, 1.0f / _cellSize.y, 1.0f / _cellSize.z );
		}

		// Adds a new entry to the table
		// Returns the handle to the added entry
		public keyValue_t	Add( float3 _position, T _value ) {
			uint		hash = ComputeHash( ref _position );

			keyValue_t	firstEntry = m_table[hash];

			keyValue_t	newEntry = Alloc();
			if ( newEntry != null ) {
				newEntry.next = firstEntry;
				newEntry.hash = hash;
				newEntry.position = _position;
				newEntry.value = _value;

				m_table[hash] = newEntry;	// We're the new first entry
			}

			return newEntry;
		}

		// Removes an entry from the table
		public bool			Remove( keyValue_t _entry ) {
			keyValue_t	previous = null;
			keyValue_t	current = m_table[_entry.hash];
			while ( current != null ) {
				if ( current != _entry ) {
					previous = current;
					current = current.next;
					continue;
				}

				// Found it!

				// Link over that node
				if ( previous != null ) {
					previous.next = current.next;
				} else {
					m_table[_entry.hash] = current.next;
				}

				// Add the node back to the free list
				current.next = m_freeNode;
				m_freeNode = current;

				m_entriesCount--;

				return true;
			}

			return false;	// Not found...
		}

		// Update the entry's position
		public bool			Update( keyValue_t _entry, float3 _newPosition ) {
			_entry.position = _newPosition;
			uint	newHash = ComputeHash( ref _newPosition );
			if ( newHash == _entry.hash ) {
				return true;	// No change in hash...
			}

			// We must first retrieve the existing entry in the linked list of entries sharing that hash
			keyValue_t	previous = null;
			keyValue_t	current = m_table[_entry.hash];
			while ( current != null ) {
				if ( current != _entry ) {
					previous = current;
					current = current.next;
					continue;
				}

				// Found it!

				// Link over that node to remove it from this hash's list of entries
				if ( previous != null ) {
					previous.next = current.next;
				} else {
					m_table[_entry.hash] = current.next;
				}

				// Update its hash
				_entry.hash = newHash;

				// Re-link it to its new position in the table
				_entry.next = m_table[newHash];
				m_table[newHash] = _entry;

				return true;
			}

			return false;	// Not found...
		}

		bool	Compare( ref float3 a, ref float3 b, float _epsilon ) {
			if ( Math.Abs( a.x - b.x ) > _epsilon ) return false;
			if ( Math.Abs( a.y - b.y ) > _epsilon ) return false;
			if ( Math.Abs( a.z - b.z ) > _epsilon ) return false;
			return true;
		}

		// Retrieve the first entry by its position
		public keyValue_t	Find( ref float3 _position, float _epsilon ) {
			uint	hash = ComputeHash( ref _position );

			keyValue_t	current = m_table[hash];
			while ( current != null ) {
				if ( Compare( ref current.position, ref _position, _epsilon ) ) {
					return current;	// Found it!
				}
				current = current.next;
			}

			return null;	// Not found...
		}

		// Retrieve all coincident entries by their position
		public void			FindAll( ref float3 _position, List< keyValue_t > _result, float _epsilon ) {
			uint	hash = ComputeHash( ref _position );

			keyValue_t	current = m_table[hash];
			while ( current != null ) {
				if ( Compare( ref current.position, ref _position, _epsilon ) ) {
					_result.Add( current );	// Another match!
				}
				current = current.next;
			}
		}

		// Retrieve all coincident entries by their position, also searches in neighbor cells so the results are exhaustive
		// NOTE: Neighbor cells are consulted only if within _epsilon threshold
		public void			FindAllIncludeNeighborCells( ref float3 _position, List< keyValue_t > _result, float _epsilon ) {

			int	minCellX, minCellY, minCellZ;
			float3	posMin = _position - _epsilon*float3.One;
			GetCellIndices( ref posMin, out minCellX, out minCellY, out minCellZ );

			int	maxCellX, maxCellY, maxCellZ;
			float3	posMax = _position + _epsilon*float3.One;
			GetCellIndices( ref posMax, out maxCellX, out maxCellY, out maxCellZ );

			for ( int Z=minCellZ; Z <= maxCellZ; Z++ ) {
				for ( int Y=minCellY; Y <= maxCellY; Y++ ) {
					for ( int X=minCellX; X <= maxCellX; X++ ) {
						uint		hash = ComputeHash( X, Y, Z );
						keyValue_t	current = m_table[hash];
						while ( current != null ) {
							if ( Compare( ref current.position, ref _position, _epsilon ) ) {
								_result.Add( current );	// Another match!
							}
							current = current.next;
						}
					}
				}
			}
		}

		// Retrieves the the cell for a given position
		public void			GetCellIndices( ref float3 _position, out int _cellX, out int _cellY, out int _cellZ ) {
			_cellX = (int) Math.Floor( _position.x * m_invCellSize.x );
			_cellY = (int) Math.Floor( _position.y * m_invCellSize.y );
			_cellZ = (int) Math.Floor( _position.z * m_invCellSize.z );
		}

		// Gives the center of a cell given its coordinates
		public float3		GetCellCenter( int _cellX, int _cellY, int _cellZ ) {
			float3	center;
			center.x = (_cellX + 0.5f) * m_cellSize.x;
			center.y = (_cellY + 0.5f) * m_cellSize.y;
			center.z = (_cellZ + 0.5f) * m_cellSize.z;
			return center;
		}

		// Fills a list with all the values stored in a given cell
		public keyValue_t	FindFirstValueInCell( int _cellX, int _cellY, int _cellZ ) {
			uint		hash = ComputeHash( _cellX, _cellY, _cellZ );	
			keyValue_t	current = m_table[hash];
			while ( current != null ) {

				int	entryCellX, entryCellY, entryCellZ;
				GetCellIndices( ref current.position, out entryCellX, out entryCellY, out entryCellZ );
				if ( entryCellX == _cellX && entryCellY == _cellY && entryCellZ == _cellZ ) {
					return current;
				}
				current = current.next;
			}
			return null;
		}
		public void		FindAllValuesInCell( int _cellX, int _cellY, int _cellZ, List< keyValue_t > _values ) {
			uint	hash = ComputeHash( _cellX, _cellY, _cellZ );

			keyValue_t	current = m_table[hash];
			while ( current != null ) {

				int	entryCellX, entryCellY, entryCellZ;
				GetCellIndices( ref current.position, out entryCellX, out entryCellY, out entryCellZ );
				if ( entryCellX == _cellX && entryCellY == _cellY && entryCellZ == _cellZ ) {
					_values.Add( current );
				}
				current = current.next;
			}
		}
		void			FindAllValuePointersInCell( int _cellX, int _cellY, int _cellZ, List< keyValue_t > _values ) {
			uint	hash = ComputeHash( _cellX, _cellY, _cellZ );

			keyValue_t	current = m_table[hash];
			while ( current != null ) {

				int	entryCellX, entryCellY, entryCellZ;
				GetCellIndices( ref current.position, out entryCellX, out entryCellY, out entryCellZ );
				if ( entryCellX == _cellX && entryCellY == _cellY && entryCellZ == _cellZ ) {
					_values.Add( current );
				}
				current = current.next;
			}
		}

		// Spatial hashing from http://www.beosil.com/download/CollisionDetectionHashing_VMV03.pdf, section 4.1
		//
		public static uint	Hash( int X, int Y, int Z ) {
			const ulong	p1 = 73856093;
			const ulong	p2 = 19349663;
			const ulong	p3 = 83492791;

			ulong	hashX = (ulong) X * p1;
			ulong	hashY = (ulong) Y * p2;
			ulong	hashZ = (ulong) Z * p3;
			ulong	hash = hashX ^ hashY ^ hashZ ;
			return (uint) hash;
		}
		public uint	ComputeHash( int X, int Y, int Z ) {
			return Hash( X, Y, Z ) % m_tableSize;
		}

		public uint	ComputeHash( ref float3 _position ) {
			int	cellX, cellY, cellZ;
			GetCellIndices( ref _position, out cellX, out cellY, out cellZ );
			uint	hash = ComputeHash( cellX, cellY, cellZ );
			return hash;
		}

		keyValue_t		Alloc() {
			if ( m_freeNode == null )
				throw new Exception( "Table is full! Consider increasing _maxEntries!" );

			keyValue_t	node = m_freeNode;
			m_freeNode = node.next;
			node.next = null;
			node.hash = ~0U;

			m_entriesCount++;

			return node;
		}

		static uint[]	ms_PowerOfTwoNextPrimes = new uint[] {
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
	}
}
