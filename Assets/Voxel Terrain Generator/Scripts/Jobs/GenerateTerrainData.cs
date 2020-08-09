using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using VoxelTG.Terrain;
using VoxelTG.Terrain.Blocks;
// use WorldSettings variables
using static VoxelTG.Terrain.WorldSettings;

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
        public NativeArray<GeneratorSettings> generatorSettings;
        [ReadOnly]
        public NativeArray<FastNoise> noises;

        public NativeArray<BlockType> blockData;
        public NativeArray<BiomeType> biomeTypes;
        public NativeHashMap<BlockParameter, short> blockParameters;

        public Random random;

        [ReadOnly]
        public FastNoise baseNoise;

        #endregion

        public void Execute()
        {
            GenerateBlockTypes();
            GenerateTrees();
        }

        private void GenerateBlockTypes()
        {
            BlockType lastBlock = BlockType.AIR;

            for (int x = 0; x < fixedChunkWidth; x++)
            {
                for (int z = 0; z < fixedChunkWidth; z++)
                {
                    int bix = chunkPosX + x - 1;
                    int biz = chunkPosZ + z - 1;

                    BiomeType biomeType = (BiomeType)math.round((baseNoise.GetSimplex(bix / biomeSize, biz / biomeSize) + 1) / possibleBiomes);
                    biomeTypes[Utils.BlockPosition2DtoIndex(x, z)] = biomeType;

                    FastNoise noise = noises[(int)biomeType];

                    GeneratorSettings settings = generatorSettings[(int)biomeType];
                    float baseLandHeightMultipler = settings.baseLandHeightMultipler;

                    #region noise

                    float simplex1 = noise.GetSimplex(bix * 0.8f, biz * 0.8f) * settings.heightMapMultipler;
                    float simplex2 = noise.GetSimplex(bix * 3f, biz * 3f) * (noise.GetSimplex(bix * 0.3f, biz * 0.3f) + 0.5f) * settings.heightMapMultipler;

                    float heightMap = (simplex1 + simplex2);

                    int baseLandHeight = (int)math.round(chunkHeight * baseLandHeightMultipler + heightMap);

                    float caveMask = noise.GetSimplex(bix * 0.3f, biz * 0.3f) + 0.3f;

                    #endregion

                    for (int y = 0; y < chunkHeight; y++)
                    {
                        float cavebaseNoise1 = noise.GetPerlinFractal(bix * 7.5f, y * 15f, biz * 7.5f);

                        BlockType blockType = BlockType.AIR;

                        if (cavebaseNoise1 > math.max(caveMask, 0.2f))
                            blockType = BlockType.AIR;
                        else if (y < baseLandHeight && cavebaseNoise1 > math.max(caveMask, 0.2f) * 0.9f)
                            blockType = BlockType.COBBLESTONE;
                        else if (y <= baseLandHeight)
                        {
                            if (y == baseLandHeight && y > waterHeight - 1)
                                blockType = WorldData.CanPlaceGrass(lastBlock) && random.NextFloat() < settings.chanceForGrass ? settings.plantsBlock : BlockType.AIR;
                            else if (y == baseLandHeight - 1 && y > waterHeight - 1)
                                blockType = settings.topBlock;
                            else if (y > baseLandHeight - 3)
                                blockType = settings.belowBlock;
                            else
                            {
                                blockType = BlockType.STONE;
                            }
                        }

                        if (y >= baseLandHeight && y <= waterHeight)
                            blockType = BlockType.WATER;

                        if (y <= noise.GetWhiteNoise(bix, z) * 3)
                            blockType = BlockType.OBSIDIAN;

                        lastBlock = blockData[Utils.BlockPosition3DtoIndex(x, y, z)] = blockType;


                        if (lastBlock == BlockType.GRASS)       // assing random block type to grass
                            blockParameters.TryAdd(new BlockParameter(new int3(x, y, z), ParameterType.BLOCK_TYPE), (short)RandomInt(0, 3));
                        //else if (lastBlock == BlockType.WATER)  // set source distance to 8 (full water block)
                        //    blockParameters.TryAdd(new BlockParameter(new int3(x, y, z), ParameterType.WATER_SOURCE_DISTANCE), 8);
                    }
                }
            }
        }

        private void GenerateTrees()
        {
            float simplex = baseNoise.GetSimplex(chunkPosX * .3f, chunkPosZ * .3f);

            if (simplex > 0)
            {
                int minpos = 4;
                int maxpos = chunkWidth - 5;

                NativeList<int2> treePositions = new NativeList<int2>(maxTreeCount, Allocator.Temp);
                for (int i = 0; i < maxTreeCount; i++)
                {
                    int x = RandomInt(minpos, maxpos);
                    int z = RandomInt(minpos, maxpos);

                    bool doContinue = false;
                    for (int j = 0; j < treePositions.Length; j++)
                    {
                        int2 pos = treePositions[j];
                        if (math.sqrt((pos.x - x) * (pos.x - x) + (pos.y - x) * (pos.y - x)) < minimumTreeDistance)
                        {
                            doContinue = true;
                            break;
                        }
                    }
                    if (doContinue)
                        continue;

                    int y = chunkHeight - treeHeigthRange.x;
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

                    // place logs
                    int treeHeight = RandomInt(4, 8);
                    for (int j = 0; j < treeHeight; j++)
                    {
                        if (y + j < chunkHeight)
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

        private float RandomFloat(float min, float max)
        {
            return random.NextFloat(min, max);
        }

        private int RandomInt(int min, int max)
        {
            return random.NextInt(min, max);
        }


        private bool AreTypesEqual(BlockType type, int x, int y, int z)
        {
            return blockData[Utils.BlockPosition3DtoIndex(x, y, z)] == type;
        }

        private bool AreTypesEqual(BlockType type, int x, int y, int z, out int index)
        {
            index = Utils.BlockPosition3DtoIndex(x, y, z);
            return blockData[index] == type;
        }
        #endregion
    }
}