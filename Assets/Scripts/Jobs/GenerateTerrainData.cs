using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using VoxelTG.Terrain;
using VoxelTG.Terrain.Blocks;
// use WorldSettings variables
using static VoxelTG.WorldSettings;
using static VoxelTG.WorldSettings.Biomes;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Jobs
{
    [BurstCompile]
    public struct GenerateTerrainData : IJob
    {
        #region /= Variables

        public int chunkPosX, chunkPosZ;

        [ReadOnly]
        public NativeArray<BiomeConfig> biomeConfigs;
        [ReadOnly]
        public FastNoise noise;

        public NativeArray<BlockType> blockData;
        public NativeArray<BiomeType> biomeTypes;
        public NativeHashMap<BlockParameter, short> blockParameters;

        public Random random;

        #endregion

        public void Execute()
        {
            GenerateBlockTypes();
            GenerateTrees();
        }


        private BiomeType GetBiome(int x, int z, out float height)
        {
            height = noise.GetSimplex(x / BiomeSize, z / BiomeSize) + 1;
            float temperature = noise.GetSimplex((x / BiomeSize) * 0.1f, (z / BiomeSize) * 0.1f) + 1;
            float moistrue = noise.GetSimplex((x / BiomeSize) * 0.25f, (z / BiomeSize) * 0.25f) + 1;

            BiomeType selectedType = BiomeType.PLAINS;
            float minDiff = float.MaxValue;
            for (int i = 0; i < biomeConfigs.Length; i++)
            {
                BiomeConfig config = biomeConfigs[i];
                float diff = math.abs(config.Height - height)
                         + math.abs(config.Temperature - temperature)
                         + math.abs(config.Moistrue - moistrue);

                if (diff < minDiff)
                {
                    minDiff = diff;
                    selectedType = config.Type;
                }
            }

            height -= 1;
            return selectedType;
        }

        private bool TransitionForestPlains(float val, out bool forest)
        {
            forest = false;
            float transition = BiomeTransition * random.NextFloat(0.5f, 5f);
            bool inRange = val > 0.4f - transition && val < 0.4f + transition;
            if (inRange)
            {
                float diff = math.abs(val - (0.4f + transition));
                forest = diff * (1 / BiomeTransition) < random.NextFloat();
            }
            return inRange;
        }

        private void GenerateBlockTypes()
        {
            for (int x = 0; x < FixedChunkSizeX; x++)
            {
                for (int z = 0; z < FixedChunkSizeX; z++)
                {
                    int bix = chunkPosX + x - 1;
                    int biz = chunkPosZ + z - 1;

                    BiomeType biomeType = GetBiome(bix, biz, out float biomeHeight);
                    biomeTypes[Utils.BlockPosition2DtoIndex(x, z)] = biomeType;

                    #region noise

                    float simplex1 = (noise.GetSimplex(bix, biz) + 0.8f) / 2;
                    float simplex2 = (noise.GetSimplex(bix * 0.5f, biz * 0.5f) + 0.8f) / 2;
                    float simplex3 = noise.GetSimplex(bix * 8f, biz * 8f) * 2 - 1;

                    float heightMap = (simplex1 + simplex2) / 2 + (simplex3 * 0.025f);
                    heightMap += biomeHeight * 0.3f;
                    int baseLandHeight = (int)math.round((ChunkSizeY * BaseLandHeightMultipler) + ((ChunkSizeY * BaseLandHeightMultipler) * heightMap));

                    #endregion

                    switch (biomeType)
                    {
                        case BiomeType.FOREST:
                            GenerateForest(bix, biz, baseLandHeight, x, z);
                            break;
                        default:
                            GeneratePlains(bix, biz, baseLandHeight, x, z);
                            break;
                    }
                }
            }
        }

        private void GeneratePlains(int bix, int biz, int baseLandHeight, int x, int z)
        {
            float caveMask = noise.GetSimplex(bix * 0.3f, biz * 0.3f) + 0.3f;

            BlockType lastBlock = BlockType.AIR;
            for (int y = 0; y < ChunkSizeY; y++)
            {
                float caveNoise = noise.GetPerlinFractal(bix * 7.5f, y * 15f, biz * 7.5f);
                BlockType blockType = BlockType.AIR;

                if (caveNoise > math.max(caveMask, 0.2f))
                    blockType = BlockType.AIR;
                else if (y < baseLandHeight && caveNoise > math.max(caveMask, 0.2f) * 0.9f)
                    blockType = BlockType.COBBLESTONE;
                else if (y <= baseLandHeight)
                {
                    if (y == baseLandHeight && y > WaterLevelY - 1)
                        blockType = WorldData.CanPlaceGrass(lastBlock) && random.NextFloat() < 0.2f ? BlockType.GRASS : BlockType.AIR;
                    else if (y == baseLandHeight - 1 && y > WaterLevelY - 1)
                        blockType = BlockType.GRASS_BLOCK;
                    else if (y > baseLandHeight * 0.95)
                        blockType = BlockType.DIRT;
                    else
                    {
                        blockType = BlockType.STONE;
                    }
                }

                if (y >= baseLandHeight && y <= WaterLevelY)
                    blockType = BlockType.WATER;

                if (y <= noise.GetWhiteNoise(bix, biz) * 3)
                    blockType = BlockType.OBSIDIAN;

                lastBlock = blockData[Utils.BlockPosition3DtoIndex(x, y, z)] = blockType;

                if (lastBlock == BlockType.GRASS)       // assing random block type to grass
                    blockParameters.TryAdd(new BlockParameter(new int3(x, y, z), ParameterType.BLOCK_TYPE), (short)random.NextInt(0, 3));
                else if (lastBlock == BlockType.WATER)  // set source distance to 8 (full water block)
                    blockParameters.TryAdd(new BlockParameter(new int3(x, y, z), ParameterType.WATER_SOURCE_DISTANCE), 8);
            }
        }

        private void GenerateForest(int bix, int biz, int baseLandHeight, int x, int z)
        {
            float caveMask = noise.GetSimplex(bix * 0.3f, biz * 0.3f) + 0.3f;

            BlockType lastBlock = BlockType.AIR;
            for (int y = 0; y < ChunkSizeY; y++)
            {
                float caveNoise = noise.GetPerlinFractal(bix * 7.5f, y * 15f, biz * 7.5f);
                BlockType blockType = BlockType.AIR;

                if (caveNoise > math.max(caveMask, 0.2f))
                    blockType = BlockType.AIR;
                else if (y < baseLandHeight && caveNoise > math.max(caveMask, 0.2f) * 0.9f)
                    blockType = BlockType.COBBLESTONE;
                else if (y <= baseLandHeight)
                {
                    if (y == baseLandHeight && y > WaterLevelY - 1)
                        blockType = WorldData.CanPlaceGrass(lastBlock) && random.NextFloat() < 0.1f ? BlockType.GRASS : BlockType.AIR;
                    else if (y == baseLandHeight - 1 && y > WaterLevelY - 1)
                        blockType = BlockType.GRASS_BLOCK;
                    else if (y > baseLandHeight * 0.95)
                        blockType = BlockType.DIRT;
                    else
                    {
                        blockType = BlockType.STONE;
                    }
                }

                if (y >= baseLandHeight && y <= WaterLevelY)
                    blockType = BlockType.WATER;

                if (y <= noise.GetWhiteNoise(bix, biz) * 3)
                    blockType = BlockType.OBSIDIAN;

                lastBlock = blockData[Utils.BlockPosition3DtoIndex(x, y, z)] = blockType;

                if (lastBlock == BlockType.GRASS)       // assing random block type to grass
                    blockParameters.TryAdd(new BlockParameter(new int3(x, y, z), ParameterType.BLOCK_TYPE), (short)random.NextInt(0, 3));
                else if (lastBlock == BlockType.WATER)  // set source distance to 8 (full water block)
                    blockParameters.TryAdd(new BlockParameter(new int3(x, y, z), ParameterType.WATER_SOURCE_DISTANCE), 8);
            }
        }

        private void GenerateTrees()
        {
            float simplex1 = noise.GetSimplex(chunkPosX * .3f, chunkPosZ * .3f);
            float simplex2 = noise.GetSimplex(chunkPosX * 2f, chunkPosZ * 2f);
            float value = (simplex1 + simplex2) / 2;
            if (value > 0)
            {
                int minpos = 4;
                int maxpos = ChunkSizeXZ - 5;

                NativeList<int2> treePositions = new NativeList<int2>(MaxTreesPerChunk, Allocator.Temp);
                for (int i = 0; i < MaxTreesPerChunk; i++)
                {
                    int x = random.NextInt(minpos, maxpos);
                    int z = random.NextInt(minpos, maxpos);

                    BiomeType biomeType = biomeTypes[Utils.BlockPosition2DtoIndex(x, z)];
                    // TODO: bool CanPlaceTree(BiomeType type)
                    if (biomeType != BiomeType.FOREST && biomeType != BiomeType.PLAINS)
                        continue;

                    bool doContinue = false;
                    for (int j = 0; j < treePositions.Length; j++)
                    {
                        int2 pos = treePositions[j];
                        if (math.sqrt((pos.x - x) * (pos.x - x) + (pos.y - x) * (pos.y - x)) < MinDistanceBetweenTrees)
                        {
                            doContinue = true;
                            break;
                        }
                    }
                    if (doContinue)
                        continue;

                    int y = ChunkSizeY - TreeHeightRange.x;
                    if (blockData[Utils.BlockPosition3DtoIndex(x, y, z)] != BlockType.AIR)
                        continue;       // continue if maxTerrainHeigth - minTreeHeigth hits ground

                    // find ground position
                    while (y > 0 && blockData[Utils.BlockPosition3DtoIndex(x, y, z)] == BlockType.AIR)
                        y--;

                    BlockType groundBlock = blockData[Utils.BlockPosition3DtoIndex(x, y, z)];
                    if (!WorldData.CanPlaceTree(groundBlock))
                        continue;       // continue if cant place tree on ground block

                    // add position to position list
                    treePositions.Add(new int2(x, z));

                    TreeSettings treeSettings = Trees.OakTree;
                    // place logs
                    int treeHeight = random.NextInt(treeSettings.TreeHeightRange.x, treeSettings.TreeHeightRange.y);
                    for (int j = 0; j < treeHeight; j++)
                    {
                        if (y + j < ChunkSizeY)
                            blockData[Utils.BlockPosition3DtoIndex(x, y + j, z)] = BlockType.OAK_LOG;
                    }

                    int treeTop = y + treeHeight - 1;
                    int index, xrange, zrange;

                    // <0, 1>
                    xrange = 1;
                    zrange = 1;
                    for (int _y = treeTop; _y <= treeTop + 1; _y++)
                    {
                        for (int _x = -xrange; _x <= xrange; _x++)
                        {
                            for (int _z = -zrange; _z <= zrange; _z++)
                            {
                                index = Utils.BlockPosition3DtoIndex(x + _x, _y, z + _z);
                                if (blockData[index] == BlockType.AIR)
                                {
                                    blockData[index] = BlockType.OAK_LEAVES;
                                }
                            }
                        }

                        // x- z-
                        if (random.NextBool() && AreTypesEqual(BlockType.OAK_LEAVES, x - xrange, _y, z - zrange, out index))
                            blockData[index] = BlockType.AIR;
                        // x- z+
                        if (random.NextBool() && AreTypesEqual(BlockType.OAK_LEAVES, x - xrange, _y, z + zrange, out index))
                            blockData[index] = BlockType.AIR;
                        // x+ z-
                        if (random.NextBool() && AreTypesEqual(BlockType.OAK_LEAVES, x + xrange, _y, z - zrange, out index))
                            blockData[index] = BlockType.AIR;
                        // x+ z+
                        if (random.NextBool() && AreTypesEqual(BlockType.OAK_LEAVES, x + xrange, _y, z + zrange, out index))
                            blockData[index] = BlockType.AIR;
                    }

                    // <-1, -2>
                    xrange = 2;
                    zrange = 2;
                    for (int _y = treeTop - 2; _y <= treeTop - 1; _y++)
                    {
                        for (int _x = -xrange; _x <= xrange; _x++)
                        {
                            for (int _z = -zrange; _z <= zrange; _z++)
                            {
                                index = Utils.BlockPosition3DtoIndex(x + _x, _y, z + _z);
                                if (blockData[index] == BlockType.AIR)
                                {
                                    blockData[index] = BlockType.OAK_LEAVES;
                                }
                            }
                        }

                        // x- z-
                        if (random.NextBool() && AreTypesEqual(BlockType.OAK_LEAVES, x - xrange, _y, z - zrange, out index))
                            blockData[index] = BlockType.AIR;
                        // x- z+
                        if (random.NextBool() && AreTypesEqual(BlockType.OAK_LEAVES, x - xrange, _y, z + zrange, out index))
                            blockData[index] = BlockType.AIR;
                        // x+ z-
                        if (random.NextBool() && AreTypesEqual(BlockType.OAK_LEAVES, x + xrange, _y, z - zrange, out index))
                            blockData[index] = BlockType.AIR;
                        // x+ z+
                        if (random.NextBool() && AreTypesEqual(BlockType.OAK_LEAVES, x + xrange, _y, z + zrange, out index))
                            blockData[index] = BlockType.AIR;
                    }

                    // <-3, -3>
                    xrange = 1;
                    zrange = 1;
                    for (int _y = treeTop - 3; _y <= treeTop - 3; _y++)
                    {
                        for (int _x = -xrange; _x <= xrange; _x++)
                        {
                            for (int _z = -zrange; _z <= zrange; _z++)
                            {
                                index = Utils.BlockPosition3DtoIndex(x + _x, _y, z + _z);
                                if (blockData[index] == BlockType.AIR)
                                {
                                    blockData[index] = BlockType.OAK_LEAVES;
                                }
                            }
                        }

                        // x- z-
                        if (random.NextBool() && AreTypesEqual(BlockType.OAK_LEAVES, x - xrange, _y, z - zrange, out index))
                            blockData[index] = BlockType.AIR;
                        // x- z+
                        if (random.NextBool() && AreTypesEqual(BlockType.OAK_LEAVES, x - xrange, _y, z + zrange, out index))
                            blockData[index] = BlockType.AIR;
                        // x+ z-
                        if (random.NextBool() && AreTypesEqual(BlockType.OAK_LEAVES, x + xrange, _y, z - zrange, out index))
                            blockData[index] = BlockType.AIR;
                        // x+ z+
                        if (random.NextBool() && AreTypesEqual(BlockType.OAK_LEAVES, x + xrange, _y, z + zrange, out index))
                            blockData[index] = BlockType.AIR;
                    }
                }
            }
        }

        #region Utils

        private bool AreTypesEqual(BlockType type, int x, int y, int z, out int index)
        {
            index = Utils.BlockPosition3DtoIndex(x, y, z);
            return blockData[index] == type;
        }
        #endregion
    }
}