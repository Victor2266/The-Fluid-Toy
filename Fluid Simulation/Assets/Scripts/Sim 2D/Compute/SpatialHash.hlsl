static const int2 offsets2D[9] =
{
	int2(-1, 1),
	int2(0, 1),
	int2(1, 1),
	int2(-1, 0),
	int2(0, 0),
	int2(1, 0),
	int2(-1, -1),
	int2(0, -1),
	int2(1, -1),
};

// Constants used for hashing
static const uint hashK1 = 15823;
static const uint hashK2 = 9737333;

// Convert floating point position into an integer cell coordinate
int2 GetCell2D(float2 position, float radius)
{
	return (int2)floor(position / radius);
}

// Hash cell coordinate to a single unsigned integer
uint HashCell2D(int2 cell)
{
	cell = (uint2)cell;
	uint a = cell.x * hashK1;
	uint b = cell.y * hashK2;
	return (a + b);
}

// Spreads bits of x by inserting 0s between each bit
uint Spread2(uint x)
{
	x &= 0x0000FFFF;				 // Mask to ensure we only use lower 16 bits
	x = (x ^ (x << 8)) & 0x00FF00FF; // Spread bits to every other byte
	x = (x ^ (x << 4)) & 0x0F0F0F0F; // Spread bits to every other half-byte
	x = (x ^ (x << 2)) & 0x33333333; // Spread bits to every other pair of bits
	x = (x ^ (x << 1)) & 0x55555555; // Spread bits to every other position
	return x;
}

// Z-hash
uint ZHashCell2D(int2 cell)
{
	// Convert negative coordinates to positive by offsetting
	uint2 pos = uint2(cell + 0x8000);

	// Interleave bits from x and y coordinates
	return Spread2(pos.x) | (Spread2(pos.y) << 1);
}

uint KeyFromHash(uint hash, uint tableSize)
{
	return hash % tableSize;
}
