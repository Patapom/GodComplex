/////////////////////////////////////////////////////////////////////////////////////////////////////
//
/////////////////////////////////////////////////////////////////////////////////////////////////////
//
Texture2D< uint >	_tex_OctreeNodes0 : register( 16 );	// Pool of consecutive octree node data containing the "Child Tile Pointers"
Texture2D< uint >	_tex_OctreeNodes1 : register( 17 );	// Pool of consecutive octree node data containing the "Brick Pointers"

Texture3D< float4 >	_tex_Bricks_Albedo : register( 18 );
Texture3D< float4 >	_tex_Bricks_NDF : register( 19 );

struct octreeNode_t {
	uint	childTilePointer;
	uint3	brickIndex;

	// True if the node is a leaf
	bool	IsLeaf() { return (childTilePointer & 0x80000000U) != 0; }

	// Gets the pointer to the group of 8 children to fetch (for non leaf nodes)
	uint	GetChildNodesPointer() { return (childTilePointer & 0x7FFFFFFFU); }

//	// The brick index is packed as 10 bits for each component
//	void	GetBrickIndex( out uint3 _XYZ ) {
//		uint	temp = brickPointer;
//		_XYZ.z = temp & 0x3FF;	temp >>= 10;
//		_XYZ.y = temp & 0x3FF;	temp >>= 10;
//		_XYZ.x = temp & 0x3FF;
//	}
};

// Stores the 8 nodes of an octree level
struct octreeNodeLevel_t {
	octreeNode_t	children[8];

	// Reads the 8 child nodes for an entire octree level
	void	Read( uint _nodesPointer ) {
		_nodePointer <<= 3;
		uint	temp = _nodePointer;
		[unroll]
		for ( uint i=0; i < 8; i++ )
			children[i].childTilePointer = _tex_OctreeNodes0[temp++];

		uint3	brickIndex;
		[unroll]
		for ( uint i=0; i < 8; i++ ) {
			uint	brickPointer = _tex_OctreeNodes1[_nodePointer++];
			brickIndex.z = brickPointer & 0x3FF;	brickPointer >>= 10;
			brickIndex.y = brickPointer & 0x3FF;	brickPointer >>= 10;
			brickIndex.x = brickPointer & 0x3FF;
			children[i].brickIndex = brickIndex;
		}
	}
};

// Bricks are packed into multiple 3D textures that are unpacked in the following structures
// NOTE: All data are pre-multiplied by the brick's alpha to enable proper interpolation and mip-mapping
struct brickStatic_t {
	float4	RGBA;				// Surface albedo + opacity
	float3	normal;				// Normal is pre-multiplied by alpha!
	uint	neighborIndices[6];	// Indices to the 6 neigbor bricks
};

struct brickDynamic_t {
	float3	flux;				// Incoming radiance
	float3	direction;			// Incoming direction
};