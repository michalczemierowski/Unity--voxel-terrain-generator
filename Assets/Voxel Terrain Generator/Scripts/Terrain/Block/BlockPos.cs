public struct BlockPos
{
    public int x, y, z;

    public BlockPos(int x, int y, int z)
    {
        if (x > TerrainChunk.chunkWidth)
            x -= TerrainChunk.chunkWidth;
        else if (x < 1)
            x += TerrainChunk.chunkWidth;

        if (z > TerrainChunk.chunkWidth)
            z -= TerrainChunk.chunkWidth;
        else if (z < 1)
            z += TerrainChunk.chunkWidth;

        this.x = x;
        this.y = y;
        this.z = z;
    }

    public BlockPos(int x, int y, int z, out int neighbour)
    {
        neighbour = -1;
        if (x > TerrainChunk.chunkWidth)
        {
            neighbour = 0;
            x -= TerrainChunk.chunkWidth;
        }
        else if (x < 1)
        {
            neighbour = 1;
            x += TerrainChunk.chunkWidth;
        }

        if (z > TerrainChunk.chunkWidth)
        {
            neighbour = 2;
            z -= TerrainChunk.chunkWidth;
        }
        else if (z < 1)
        {
            neighbour = 3;
            z += TerrainChunk.chunkWidth;
        }

        this.x = x;
        this.y = y;
        this.z = z;
    }

    public override string ToString()
    {
        return "{" + x + ", " + y + ", " + z + "}";
    }
}

