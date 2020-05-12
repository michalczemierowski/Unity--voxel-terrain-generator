using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

[BurstCompile]
public struct StoneTest
{
    private FastNoise fastNoise;
    private const int dirtHeight = 3;

    public StoneTest(FastNoise fastNoise)
    {
        this.fastNoise = fastNoise;
    }

    public BlockType GetBlockType(int x, int y, int z)
    {
        if (y == 0)
            return BlockType.OBSIDIAN;

        float simplex1 = fastNoise.GetSimplex(x * 0.8f, z * 0.8f) * 10;
        float simplex2 = fastNoise.GetSimplex(x * 3f, z * 3f) * 10 * (fastNoise.GetSimplex(x * 0.3f, z * 0.3f) + 0.5f);

        float heightMap = simplex1 + simplex2;

        int baseLandHeight = (int)math.round(TerrainChunk.chunkHeight * .5f + heightMap);

        if (y < baseLandHeight)
            if (y > baseLandHeight - dirtHeight)
                return BlockType.DIRT;
            else
                return BlockType.STONE;

        if (y >= baseLandHeight && y <= TerrainChunk.waterHeight)
            return BlockType.WATER;

        return BlockType.AIR;
    }
}