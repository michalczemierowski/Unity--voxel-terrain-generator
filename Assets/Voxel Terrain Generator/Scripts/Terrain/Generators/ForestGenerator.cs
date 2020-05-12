using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

[BurstCompile]
public struct ForestGenerator
{
    private FastNoise fastNoise;
    private const int dirtHeight = 3;

    public ForestGenerator(FastNoise fastNoise)
    {
        this.fastNoise = fastNoise;
    }

    public BlockType GetBlockType(int x, int y, int z, bool grass)
    {
        if (y == 0)
            return BlockType.OBSIDIAN;

        #region noise

        float simplex1 = fastNoise.GetSimplex(x * 0.8f, z * 0.8f) * 10;
        float simplex2 = fastNoise.GetSimplex(x * 3f, z * 3f) * 10 * (fastNoise.GetSimplex(x * 0.3f, z * 0.3f) + 0.5f);

        float heightMap = simplex1 + simplex2;

        int baseLandHeight = (int)math.round(TerrainChunk.chunkHeight * .5f + heightMap);

        float caveFastNoise1 = fastNoise.GetPerlinFractal(x * 7.5f, y * 15f, z * 7.5f);
        float caveMask = fastNoise.GetSimplex(x * 0.3f, z * 0.3f) + 0.3f;

        #endregion

        BlockType blockType = BlockType.AIR;

        if (caveFastNoise1 > math.max(caveMask, 0.2f))
            blockType = BlockType.AIR;
        else if (y < baseLandHeight && caveFastNoise1 > math.max(caveMask, 0.2f) * 0.9f)
            blockType = BlockType.COBBLESTONE;
        else if (y <= baseLandHeight)
        {
            if (y == baseLandHeight && y > TerrainChunk.waterHeight - 1)
                blockType = grass ? BlockType.GRASS : BlockType.AIR;
            else if (y == baseLandHeight - 1 && y > TerrainChunk.waterHeight - 1)
                blockType = BlockType.GRASS_BLOCK;
            else if (y > baseLandHeight - dirtHeight)
                blockType = BlockType.DIRT;
            else
            {
                blockType = BlockType.STONE;
            }
        }

        if (y >= baseLandHeight && y <= TerrainChunk.waterHeight)
            return BlockType.WATER;

        if (y <= fastNoise.GetWhiteNoise(x, z) * 3)
            return BlockType.OBSIDIAN;

        return blockType;
    }
}