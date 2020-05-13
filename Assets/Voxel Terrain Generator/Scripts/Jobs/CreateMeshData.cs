using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct CreateMeshData : IJob
{
    #region /= Variables

    public int chunkPosX, chunkPosZ;

    public NativeArray<BlockType> blockData;
    public NativeArray<BiomeType> biomeTypes;
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

    public Unity.Mathematics.Random random;
    public FastNoise baseNoise;

    #endregion

    public void Execute()
    {
        NativeArray<float3> verts = new NativeArray<float3>(4, Allocator.Temp);
        NativeArray<float2> uv = new NativeArray<float2>(4, Allocator.Temp);
        NativeArray<int> triangles = new NativeArray<int>(6, Allocator.Temp);

        NativeArray<bool> drawFace = new NativeArray<bool>(6, Allocator.Temp);
        NativeArray<bool> flipFace = new NativeArray<bool>(6, Allocator.Temp);

        NativeArray<short> nearbyLiquidSourceDistance = new NativeArray<short>(4, Allocator.Temp);

        for (int x = 1; x < TerrainChunk.chunkWidth + 1; x++)
        {
            for (int z = 1; z < TerrainChunk.chunkWidth + 1; z++)
            {
                for (int y = 0; y < TerrainChunk.chunkHeight; y++)
                {
                    BlockType blockType = blockData[Index3Dto1D(x, y, z)];

                    if (blockType != BlockType.AIR)
                    {
                        int3 blockPos = new int3(x - 1, y, z - 1);
                        int numFaces = 0;

                        Block block = TerrainData.GetBlock(blockType);

                        short param = 0;

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

                                blockParameters.TryGetValue(new BlockParameter(new int3(x, y, z), ParameterType.WATER_SOURCE_DISTANCE), out param);

                                BlockstateLiquid(drawFace, x, y, z);
                                break;
                            case BlockShape.HALF_BLOCK:
                                blockParameters.TryGetValue(new BlockParameter(new int3(x, y, z), ParameterType.ROTATION), out param);

                                BlockstateSolidHalf(drawFace, x, y, z, param);
                                break;
                            case BlockShape.GRASS:
                                verticles = plantsVerticles;
                                uvs = plantsUVs;
                                tris = plantsTriangles;

                                blockParameters.TryGetValue(new BlockParameter(new int3(x, y, z), ParameterType.BLOCK_TYPE), out param);

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
    }

    #region blockstate checks

    private void BlockstateSolid(NativeArray<bool> sides, int x, int y, int z)
    {
        sides[0] = y < TerrainChunk.chunkHeight - 1 &&
            TerrainData.GetBlockState(blockData[Index3Dto1D(x, y + 1, z)],
            BlockFace.BOTTOM
        ) != BlockState.SOLID;

        sides[1] = y > 0 &&
            TerrainData.GetBlockState(blockData[Index3Dto1D(x, y - 1, z)],
            BlockFace.TOP
        ) != BlockState.SOLID;

        sides[2] = TerrainData.GetBlockState(blockData[Index3Dto1D(x, y, z - 1)],
            BlockFace.BACK
        ) != BlockState.SOLID;

        sides[5] = TerrainData.GetBlockState(blockData[Index3Dto1D(x + 1, y, z)],
            BlockFace.LEFT
        ) != BlockState.SOLID;

        sides[3] = TerrainData.GetBlockState(blockData[Index3Dto1D(x, y, z + 1)],
            BlockFace.FRONT
        ) != BlockState.SOLID;

        sides[4] = TerrainData.GetBlockState(blockData[Index3Dto1D(x - 1, y, z)],
            BlockFace.RIGHT
        ) != BlockState.SOLID;
    }

    private void BlockstateLiquid(NativeArray<bool> sides, int x, int y, int z)
    {
        short waterSourceDistance = blockParameters[new BlockParameter(new int3(x, y, z), ParameterType.WATER_SOURCE_DISTANCE)];
        NativeArray<int> nearbyWaterSources = new NativeArray<int>(4, Allocator.Temp);

        short value;
        nearbyWaterSources[0] = blockParameters.TryGetValue(new BlockParameter(new int3(x, y, z - 1), ParameterType.WATER_SOURCE_DISTANCE), out value) ? value : waterSourceDistance;
        nearbyWaterSources[1] = blockParameters.TryGetValue(new BlockParameter(new int3(x, y, z + 1), ParameterType.WATER_SOURCE_DISTANCE), out value) ? value : waterSourceDistance;
        nearbyWaterSources[2] = blockParameters.TryGetValue(new BlockParameter(new int3(x - 1, y, z), ParameterType.WATER_SOURCE_DISTANCE), out value) ? value : waterSourceDistance;
        nearbyWaterSources[3] = blockParameters.TryGetValue(new BlockParameter(new int3(x + 1, y, z), ParameterType.WATER_SOURCE_DISTANCE), out value) ? value : waterSourceDistance;

        sides[0] = y < TerrainChunk.chunkHeight - 1 &&
            TerrainData.GetBlockState(blockData[Index3Dto1D(x, y + 1, z)]) == BlockState.TRANSPARENT;
        sides[1] = y > 0 &&
            TerrainData.GetBlockState(blockData[Index3Dto1D(x, y - 1, z)]) == BlockState.TRANSPARENT;
        sides[2] = nearbyWaterSources[0] < waterSourceDistance || 
            TerrainData.GetBlockState(blockData[Index3Dto1D(x, y, z - 1)]) == BlockState.TRANSPARENT;
        sides[3] = nearbyWaterSources[1] < waterSourceDistance || 
            TerrainData.GetBlockState(blockData[Index3Dto1D(x, y, z + 1)]) == BlockState.TRANSPARENT;
        sides[4] = nearbyWaterSources[2] < waterSourceDistance || 
            TerrainData.GetBlockState(blockData[Index3Dto1D(x - 1, y, z)]) == BlockState.TRANSPARENT;
        sides[5] = nearbyWaterSources[3] < waterSourceDistance || 
            TerrainData.GetBlockState(blockData[Index3Dto1D(x + 1, y, z)]) == BlockState.TRANSPARENT;
    }

    private void BlockstateSolidHalf(NativeArray<bool> sides, int x, int y, int z, int3 rotation)
    {
        int eulerY = rotation.y / 90;

        int frontIndex = 2 + eulerY > 5 ? 2 + eulerY - 4 : 2 + eulerY;
        int rightIndex = 5 + eulerY > 5 ? 3 + eulerY - 4 : 5 + eulerY;
        int backIndex = 3 + eulerY > 5 ? 4 + eulerY - 4 : 3 + eulerY;
        int leftIndex = 4 + eulerY > 5 ? 5 + eulerY - 4 : 4 + eulerY;

        sides[0] = y < TerrainChunk.chunkHeight - 1 &&
            TerrainData.GetBlockState(blockData[Index3Dto1D(x, y + 1, z)]) != BlockState.SOLID;
        sides[1] = y > 0 &&
            TerrainData.GetBlockState(blockData[Index3Dto1D(x, y - 1, z)]) != BlockState.SOLID;
        sides[frontIndex] = TerrainData.GetBlockState(blockData[Index3Dto1D(x, y, z - 1)]) != BlockState.SOLID;
        sides[rightIndex] = TerrainData.GetBlockState(blockData[Index3Dto1D(x + 1, y, z)]) != BlockState.SOLID;
        sides[backIndex] = TerrainData.GetBlockState(blockData[Index3Dto1D(x, y, z + 1)]) != BlockState.SOLID;
        sides[leftIndex] = TerrainData.GetBlockState(blockData[Index3Dto1D(x - 1, y, z)]) != BlockState.SOLID;
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

    private float RandomFloat(float min, float max)
    {
        return random.NextFloat(min, max);
    }

    private int RandomInt(int min, int max)
    {
        return random.NextInt(min, max);
    }

    private int Index3Dto1D(int x, int y, int z)
    {
        return (z * TerrainChunk.fixedChunkWidth * TerrainChunk.chunkHeight) + (y * TerrainChunk.fixedChunkWidth) + x;
    }

    private int Index2Dto1D(int x, int z)
    {
        return x * TerrainChunk.fixedChunkWidth + z;
    }

    private bool AreTypesEqual(BlockType type, int x, int y, int z)
    {
        return blockData[Index3Dto1D(x, y, z)] == type;
    }

    private bool AreTypesEqual(BlockType type, int x, int y, int z, out int index)
    {
        index = Index3Dto1D(x, y, z);
        return blockData[index] == type;
    }
    #endregion
}
