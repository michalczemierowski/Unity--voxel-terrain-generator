using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using VoxelTG.Terrain;
using VoxelTG.Terrain.Blocks;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Jobs
{
    [BurstCompile]
    public struct CreateMeshData : IJob
    {
        #region /= Variables

        [ReadOnly] public NativeArray<BlockType> blocks;
        [ReadOnly] public NativeArray<BiomeType> biomeTypes;
        public NativeHashMap<BlockParameter, short> blockParameters;

        public NativeList<float3> blockVerticles;
        public NativeList<int> blockTriangles;
        public NativeList<float2> blockUVs;

        public NativeList<float3> liquidVerticles;
        public NativeList<int> liquidTriangles;
        public NativeList<float2> liquidUVs;

        public NativeList<float3> plantsVerticles;
        public NativeList<int> plantsTriangles;
        public NativeList<float2> plantsUVs;

        #endregion

        public void Execute()
        {
            NativeArray<float3> verts = new NativeArray<float3>(4, Allocator.Temp);
            NativeArray<float2> uv = new NativeArray<float2>(4, Allocator.Temp);
            NativeArray<int> triangles = new NativeArray<int>(6, Allocator.Temp);

            NativeArray<bool> drawFace = new NativeArray<bool>(6, Allocator.Temp);
            NativeArray<bool> flipFace = new NativeArray<bool>(6, Allocator.Temp);

            NativeArray<short> nearbyLiquidSourceDistance = new NativeArray<short>(4, Allocator.Temp);

            for (int x = 1; x < WorldSettings.ChunkSizeXZ + 1; x++)
            {
                for (int z = 1; z < WorldSettings.ChunkSizeXZ + 1; z++)
                {
                    for (int y = 0; y < WorldSettings.ChunkSizeY; y++)
                    {
                        int index = Utils.BlockPosition3DtoIndex(x, y, z);
                        BlockType blockType = blocks[index];

                        if (blockType == BlockType.AIR)
                            continue;

                        int3 blockPos = new int3(x - 1, y, z - 1);
                        int numFaces = 0;

                        BlockStructure block = WorldData.GetBlockData(blockType);

                        short param = 0;

                        // assign default values
                        ref var verticles = ref blockVerticles;
                        ref var uvs = ref blockUVs;
                        ref var tris = ref blockTriangles;

                        // check for visible faces
                        switch (block.shape)
                        {
                            case BlockShape.LIQUID:
                                verticles = ref liquidVerticles;
                                uvs = ref liquidUVs;
                                tris = ref liquidTriangles;

                                // set to full by default to not save full blocks in game saves
                                if (!blockParameters.TryGetValue(new BlockParameter(new int3(x, y, z), ParameterType.WATER_SOURCE_DISTANCE), out param))
                                    param = 8;

                                BlockstateLiquid(drawFace, index, x, y, z);
                                break;
                            case BlockShape.HALF_BLOCK:
                                blockParameters.TryGetValue(new BlockParameter(new int3(x, y, z), ParameterType.ROTATION), out param);

                                BlockstateSolidHalf(drawFace, index, x, y, z, param);
                                break;
                            case BlockShape.GRASS:
                                verticles = ref plantsVerticles;
                                uvs = ref plantsUVs;
                                tris = ref plantsTriangles;

                                blockParameters.TryGetValue(new BlockParameter(new int3(x, y, z), ParameterType.BLOCK_TYPE), out param);

                                BlockstateGrass(drawFace, index, x, y, z);
                                break;
                            default:
                                BlockstateSolid(drawFace, index, x, y, z);
                                break;
                        }

                        // draw faces
                        if (block.shape == BlockShape.LIQUID)
                        {
                            short value;
                            for (int i = 0; i < 6; i++)
                            {
                                if (!drawFace[i])
                                    continue;

                                // R L F B
                                nearbyLiquidSourceDistance[0] = blockParameters.TryGetValue(new BlockParameter(new int3(x + 1, y, z), ParameterType.WATER_SOURCE_DISTANCE), out value) ? value : (short)0;
                                nearbyLiquidSourceDistance[1] = blockParameters.TryGetValue(new BlockParameter(new int3(x - 1, y, z), ParameterType.WATER_SOURCE_DISTANCE), out value) ? value : (short)0;
                                nearbyLiquidSourceDistance[2] = blockParameters.TryGetValue(new BlockParameter(new int3(x, y, z + 1), ParameterType.WATER_SOURCE_DISTANCE), out value) ? value : (short)0;
                                nearbyLiquidSourceDistance[3] = blockParameters.TryGetValue(new BlockParameter(new int3(x, y, z - 1), ParameterType.WATER_SOURCE_DISTANCE), out value) ? value : (short)0;

                                bool reverse = block.GetWaterShape((BlockFace)i, blockPos, verts, uv, param, nearbyLiquidSourceDistance);
                                verticles.AddRange(verts);
                                uvs.AddRange(uv);

                                flipFace[numFaces] = reverse;
                                numFaces++;
                            }
                        }
                        else
                        {
                            for (int i = 0; i < 6; i++)
                            {
                                if (!drawFace[i])
                                    continue;
                                bool reverse = block.GetBlockShape((BlockFace)i, blockPos, verts, uv, param);
                                verticles.AddRange(verts);
                                uvs.AddRange(uv);

                                flipFace[numFaces] = reverse;
                                numFaces++;
                            }
                        }

                        // triangles
                        int tl = verticles.Length - 4 * numFaces;
                        for (int i = 0; i < numFaces; i++)
                        {
                            if (flipFace[i])
                            {
                                triangles[5] = tl + i * 4;
                                triangles[4] = tl + i * 4 + 1;
                                triangles[3] = tl + i * 4 + 2;
                                triangles[2] = tl + i * 4;
                                triangles[1] = tl + i * 4 + 2;
                                triangles[0] = tl + i * 4 + 3;
                            }
                            else
                            {
                                triangles[0] = tl + i * 4;
                                triangles[1] = tl + i * 4 + 1;
                                triangles[2] = tl + i * 4 + 2;
                                triangles[3] = tl + i * 4;
                                triangles[4] = tl + i * 4 + 2;
                                triangles[5] = tl + i * 4 + 3;
                            }

                            tris.AddRange(triangles);
                        }
                    }
                }
            }

            // dispose native arrays
            verts.Dispose();
            uv.Dispose();
            triangles.Dispose();

            drawFace.Dispose();
            flipFace.Dispose();

            nearbyLiquidSourceDistance.Dispose();
        }

        #region blockstate checks

        private void BlockstateSolid(NativeArray<bool> sides, int index, int x, int y, int z)
        {
            sides[0] = y == WorldSettings.ChunkSizeY - 1 ||
                WorldData.GetBlockState(blocks[Utils.NextBlock3DIndexY(index)],
                BlockFace.BOTTOM
            ) != BlockState.SOLID;

            sides[1] = y > 0 &&
                WorldData.GetBlockState(blocks[Utils.PrevBlock3DIndexY(index)],
                BlockFace.TOP
            ) != BlockState.SOLID;

            sides[2] = WorldData.GetBlockState(blocks[Utils.PrevBlock3DIndexZ(index)],
                BlockFace.BACK
            ) != BlockState.SOLID;

            sides[5] = WorldData.GetBlockState(blocks[Utils.NextBlock3DIndexX(index)],
                BlockFace.LEFT
            ) != BlockState.SOLID;

            sides[3] = WorldData.GetBlockState(blocks[Utils.NextBlock3DIndexZ(index)],
                BlockFace.FRONT
            ) != BlockState.SOLID;

            sides[4] = WorldData.GetBlockState(blocks[Utils.PrevBlock3DIndexX(index)],
                BlockFace.RIGHT
            ) != BlockState.SOLID;
        }

        private void BlockstateLiquid(NativeArray<bool> sides, int index, int x, int y, int z)
        {
            short waterSourceDistance;
            if (!blockParameters.TryGetValue(new BlockParameter(new int3(x, y, z), ParameterType.WATER_SOURCE_DISTANCE), out waterSourceDistance))
                waterSourceDistance = 8;

            NativeArray<int> nearbyWaterSources = new NativeArray<int>(4, Allocator.Temp);

            short value;
            nearbyWaterSources[0] = blockParameters.TryGetValue(new BlockParameter(new int3(x, y, z - 1), ParameterType.WATER_SOURCE_DISTANCE), out value) ? value : waterSourceDistance;
            nearbyWaterSources[1] = blockParameters.TryGetValue(new BlockParameter(new int3(x, y, z + 1), ParameterType.WATER_SOURCE_DISTANCE), out value) ? value : waterSourceDistance;
            nearbyWaterSources[2] = blockParameters.TryGetValue(new BlockParameter(new int3(x - 1, y, z), ParameterType.WATER_SOURCE_DISTANCE), out value) ? value : waterSourceDistance;
            nearbyWaterSources[3] = blockParameters.TryGetValue(new BlockParameter(new int3(x + 1, y, z), ParameterType.WATER_SOURCE_DISTANCE), out value) ? value : waterSourceDistance;

            sides[0] = y < WorldSettings.ChunkSizeY - 1 &&
                WorldData.GetBlockState(blocks[Utils.NextBlock3DIndexY(index)]) == BlockState.TRANSPARENT;
            sides[1] = y > 0 &&
                WorldData.GetBlockState(blocks[Utils.PrevBlock3DIndexY(index)]) == BlockState.TRANSPARENT;
            sides[2] = nearbyWaterSources[0] < waterSourceDistance ||
                WorldData.GetBlockState(blocks[Utils.PrevBlock3DIndexZ(index)]) == BlockState.TRANSPARENT;
            sides[3] = nearbyWaterSources[1] < waterSourceDistance ||
                WorldData.GetBlockState(blocks[Utils.NextBlock3DIndexZ(index)]) == BlockState.TRANSPARENT;
            sides[4] = nearbyWaterSources[2] < waterSourceDistance ||
                WorldData.GetBlockState(blocks[Utils.PrevBlock3DIndexX(index)]) == BlockState.TRANSPARENT;
            sides[5] = nearbyWaterSources[3] < waterSourceDistance ||
                WorldData.GetBlockState(blocks[Utils.NextBlock3DIndexX(index)]) == BlockState.TRANSPARENT;
        }

        private void BlockstateSolidHalf(NativeArray<bool> sides, int index, int x, int y, int z, int3 rotation)
        {
            int eulerY = rotation.y / 90;

            int frontIndex = 2 + eulerY > 5 ? 2 + eulerY - 4 : 2 + eulerY;
            int rightIndex = 5 + eulerY > 5 ? 3 + eulerY - 4 : 5 + eulerY;
            int backIndex = 3 + eulerY > 5 ? 4 + eulerY - 4 : 3 + eulerY;
            int leftIndex = 4 + eulerY > 5 ? 5 + eulerY - 4 : 4 + eulerY;

            sides[0] = y < WorldSettings.ChunkSizeY - 1 &&
                WorldData.GetBlockState(blocks[Utils.NextBlock3DIndexY(index)]) != BlockState.SOLID;
            sides[1] = y > 0 &&
                WorldData.GetBlockState(blocks[Utils.PrevBlock3DIndexY(index)]) != BlockState.SOLID;
            sides[frontIndex] = WorldData.GetBlockState(blocks[Utils.PrevBlock3DIndexZ(index)]) != BlockState.SOLID;
            sides[rightIndex] = WorldData.GetBlockState(blocks[Utils.NextBlock3DIndexX(index)]) != BlockState.SOLID;
            sides[backIndex] = WorldData.GetBlockState(blocks[Utils.NextBlock3DIndexZ(index)]) != BlockState.SOLID;
            sides[leftIndex] = WorldData.GetBlockState(blocks[Utils.PrevBlock3DIndexX(index)]) != BlockState.SOLID;
        }

        private void BlockstateGrass(NativeArray<bool> sides, int index, int x, int y, int z)
        {
            BlockstateSolid(sides, index, x, y, z);
            bool visible = sides[0] || sides[1] || sides[2] || sides[3] || sides[4] || sides[5];
            for (int i = 2; i < 6; i++)
            {
                sides[i] = visible;
            }
        }

        #endregion

        #region Utils

        private bool AreTypesEqual(BlockType type, int x, int y, int z)
        {
            return blocks[Utils.BlockPosition3DtoIndex(x, y, z)] == type;
        }

        private bool AreTypesEqual(BlockType type, int x, int y, int z, out int index)
        {
            index = Utils.BlockPosition3DtoIndex(x, y, z);
            return blocks[index] == type;
        }
        #endregion
    }
}
