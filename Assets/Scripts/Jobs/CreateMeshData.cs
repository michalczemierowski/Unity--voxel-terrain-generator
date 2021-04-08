using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using VoxelTG.Extensions;
using VoxelTG.Terrain;
using VoxelTG.Terrain.Blocks;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Jobs
{
    [BurstCompile]
    public struct CreateMeshDataJob : IJob
    {
        #region /= Variables

        [ReadOnly] public NativeArray<BlockType> blocks;
        [ReadOnly] public NativeArray<BiomeType> biomeTypes;
        [ReadOnly] public NativeHashMap<int, BlockParameter> blockParameters;

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
                        BlockType blockType = blocks[BlockPosition3DtoIndex(x, y, z)];

                        if (blockType != BlockType.AIR)
                        {
                            int3 blockPos = new int3(x - 1, y, z - 1);
                            int numFaces = 0;

                            BlockStructure block = WorldData.GetBlockData(blockType);

                            byte param = 0;

                            // assign default values
                            var verticles = blockVerticles;
                            var uvs = blockUVs;
                            var tris = blockTriangles;

                            // check for visible faces
                            switch (block.shape)
                            {
                                case BlockShape.LIQUID:
                                    verticles = liquidVerticles;
                                    uvs = liquidUVs;
                                    tris = liquidTriangles;

                                    // set to full by default to not save full blocks in game saves
                                    if (!blockParameters.TryGetParameterValue(new int3(x, y, z), ParameterType.LIQUID_SOURCE_DISTANCE, out param))
                                        param = 8;

                                    BlockstateLiquid(drawFace, x, y, z);
                                    break;
                                case BlockShape.HALF_BLOCK:
                                    blockParameters.TryGetParameterValue(new int3(x, y, z), ParameterType.ROTATION, out param);

                                    BlockstateSolidHalf(drawFace, x, y, z, param);
                                    break;
                                case BlockShape.GRASS:
                                    verticles = plantsVerticles;
                                    uvs = plantsUVs;
                                    tris = plantsTriangles;

                                    blockParameters.TryGetParameterValue(new int3(x, y, z), ParameterType.BLOCK_TYPE, out param);

                                    BlockstateGrass(drawFace, x, y, z);
                                    break;
                                default:
                                    BlockstateSolid(drawFace, x, y, z);
                                    break;
                            }

                            // draw faces
                            for (int i = 0; i < 6; i++)
                            {
                                if (drawFace[i])
                                {
                                    if (block.shape == BlockShape.LIQUID)
                                    {
                                        short value;
                                        // R L F B
                                        nearbyLiquidSourceDistance[0] = blockParameters.GetParameterValue(new int3(x + 1, y, z), ParameterType.LIQUID_SOURCE_DISTANCE);
                                        nearbyLiquidSourceDistance[1] = blockParameters.GetParameterValue(new int3(x - 1, y, z), ParameterType.LIQUID_SOURCE_DISTANCE);
                                        nearbyLiquidSourceDistance[2] = blockParameters.GetParameterValue(new int3(x, y, z + 1), ParameterType.LIQUID_SOURCE_DISTANCE);
                                        nearbyLiquidSourceDistance[3] = blockParameters.GetParameterValue(new int3(x, y, z - 1), ParameterType.LIQUID_SOURCE_DISTANCE);

                                        bool reverse = block.GetWaterShape((BlockFace)i, blockPos, verts, uv, param, nearbyLiquidSourceDistance);
                                        verticles.AddRange(verts);
                                        uvs.AddRange(uv);

                                        flipFace[numFaces] = reverse;
                                        numFaces++;
                                    }
                                    else
                                    {
                                        bool reverse = block.GetBlockShape((BlockFace)i, blockPos, verts, uv, param);
                                        verticles.AddRange(verts);
                                        uvs.AddRange(uv);

                                        flipFace[numFaces] = reverse;
                                        numFaces++;
                                    }
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

        private void BlockstateSolid(NativeArray<bool> sides, int x, int y, int z)
        {
            sides[0] = y == WorldSettings.ChunkSizeY - 1 ||
                WorldData.GetBlockState(blocks[BlockPosition3DtoIndex(x, y + 1, z)],
                BlockFace.BOTTOM
            ) != BlockState.SOLID;

            sides[1] = y > 0 &&
                WorldData.GetBlockState(blocks[BlockPosition3DtoIndex(x, y - 1, z)],
                BlockFace.TOP
            ) != BlockState.SOLID;

            sides[2] = WorldData.GetBlockState(blocks[BlockPosition3DtoIndex(x, y, z - 1)],
                BlockFace.BACK
            ) != BlockState.SOLID;

            sides[5] = WorldData.GetBlockState(blocks[BlockPosition3DtoIndex(x + 1, y, z)],
                BlockFace.LEFT
            ) != BlockState.SOLID;

            sides[3] = WorldData.GetBlockState(blocks[BlockPosition3DtoIndex(x, y, z + 1)],
                BlockFace.FRONT
            ) != BlockState.SOLID;

            sides[4] = WorldData.GetBlockState(blocks[BlockPosition3DtoIndex(x - 1, y, z)],
                BlockFace.RIGHT
            ) != BlockState.SOLID;
        }

        private void BlockstateLiquid(NativeArray<bool> sides, int x, int y, int z)
        {
            byte waterSourceDistance;
            if (!blockParameters.TryGetParameterValue(new int3(x, y, z), ParameterType.LIQUID_SOURCE_DISTANCE, out waterSourceDistance))
                waterSourceDistance = 8;

            NativeArray<int> nearbyWaterSources = new NativeArray<int>(4, Allocator.Temp);

            nearbyWaterSources[0] = blockParameters.GetParameterValue(new int3(x, y, z - 1), ParameterType.LIQUID_SOURCE_DISTANCE);
            nearbyWaterSources[1] = blockParameters.GetParameterValue(new int3(x, y, z + 1), ParameterType.LIQUID_SOURCE_DISTANCE);
            nearbyWaterSources[2] = blockParameters.GetParameterValue(new int3(x - 1, y, z), ParameterType.LIQUID_SOURCE_DISTANCE);
            nearbyWaterSources[3] = blockParameters.GetParameterValue(new int3(x + 1, y, z), ParameterType.LIQUID_SOURCE_DISTANCE);

            sides[0] = y < WorldSettings.ChunkSizeY - 1 &&
                WorldData.GetBlockState(blocks[BlockPosition3DtoIndex(x, y + 1, z)]) == BlockState.TRANSPARENT;
            sides[1] = y > 0 &&
                WorldData.GetBlockState(blocks[BlockPosition3DtoIndex(x, y - 1, z)]) == BlockState.TRANSPARENT;
            sides[2] = nearbyWaterSources[0] < waterSourceDistance ||
                WorldData.GetBlockState(blocks[BlockPosition3DtoIndex(x, y, z - 1)]) == BlockState.TRANSPARENT;
            sides[3] = nearbyWaterSources[1] < waterSourceDistance ||
                WorldData.GetBlockState(blocks[BlockPosition3DtoIndex(x, y, z + 1)]) == BlockState.TRANSPARENT;
            sides[4] = nearbyWaterSources[2] < waterSourceDistance ||
                WorldData.GetBlockState(blocks[BlockPosition3DtoIndex(x - 1, y, z)]) == BlockState.TRANSPARENT;
            sides[5] = nearbyWaterSources[3] < waterSourceDistance ||
                WorldData.GetBlockState(blocks[BlockPosition3DtoIndex(x + 1, y, z)]) == BlockState.TRANSPARENT;

            nearbyWaterSources.Dispose();
        }

        private void BlockstateSolidHalf(NativeArray<bool> sides, int x, int y, int z, int3 rotation)
        {
            int eulerY = rotation.y / 90;

            int frontIndex = 2 + eulerY > 5 ? 2 + eulerY - 4 : 2 + eulerY;
            int rightIndex = 5 + eulerY > 5 ? 3 + eulerY - 4 : 5 + eulerY;
            int backIndex = 3 + eulerY > 5 ? 4 + eulerY - 4 : 3 + eulerY;
            int leftIndex = 4 + eulerY > 5 ? 5 + eulerY - 4 : 4 + eulerY;

            sides[0] = y < WorldSettings.ChunkSizeY - 1 &&
                WorldData.GetBlockState(blocks[BlockPosition3DtoIndex(x, y + 1, z)]) != BlockState.SOLID;
            sides[1] = y > 0 &&
                WorldData.GetBlockState(blocks[BlockPosition3DtoIndex(x, y - 1, z)]) != BlockState.SOLID;
            sides[frontIndex] = WorldData.GetBlockState(blocks[BlockPosition3DtoIndex(x, y, z - 1)]) != BlockState.SOLID;
            sides[rightIndex] = WorldData.GetBlockState(blocks[BlockPosition3DtoIndex(x + 1, y, z)]) != BlockState.SOLID;
            sides[backIndex] = WorldData.GetBlockState(blocks[BlockPosition3DtoIndex(x, y, z + 1)]) != BlockState.SOLID;
            sides[leftIndex] = WorldData.GetBlockState(blocks[BlockPosition3DtoIndex(x - 1, y, z)]) != BlockState.SOLID;
        }

        private void BlockstateGrass(NativeArray<bool> sides, int x, int y, int z)
        {
            BlockstateSolid(sides, x, y, z);
            bool visible = sides[0] || sides[1] || sides[2] || sides[3] || sides[4] || sides[5];
            for (int i = 2; i < 6; i++)
            {
                sides[i] = visible;
            }
        }

        #endregion

        #region Utils

        private int BlockPosition3DtoIndex(int x, int y, int z)
        {
            return (z * WorldSettings.FixedChunkSizeXZ * WorldSettings.ChunkSizeY) + (y * WorldSettings.FixedChunkSizeXZ) + x;
        }

        private bool AreTypesEqual(BlockType type, int x, int y, int z)
        {
            return blocks[BlockPosition3DtoIndex(x, y, z)] == type;
        }

        private bool AreTypesEqual(BlockType type, int x, int y, int z, out int index)
        {
            index = BlockPosition3DtoIndex(x, y, z);
            return blocks[index] == type;
        }

        #endregion
    }
}
