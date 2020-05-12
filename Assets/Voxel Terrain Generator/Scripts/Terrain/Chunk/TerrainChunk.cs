using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

/*
 * TODO:
 * każdy chunk ma biom, każdy biom ma własne ustawienia
 * usuwanie wody po zniszczeniu źródła
 */
public class TerrainChunk : MonoBehaviour
{
    public MeshFilter blockMeshFilter, liquidMeshFilter, plantsMeshFilter;
    public MeshCollider blockMeshCollider;

    private ChunkDissapearingAnimation chunkDissapearingAnimation;
    private ChunkAnimation chunkAnimation;

    //chunk size
    public const int chunkWidth = 16;
    public const int chunkHeight = 64;
    public const int waterHeight = 28;
    public const int fixedChunkWidth = chunkWidth + 2;

    public const int maxTreeCount = 10;
    public const float minimumTreeDistance = 4;
    public static readonly int2 treeHeigthRange = new int2(4, 8);
    public const float chanceForGrass = 0.15f;

    private bool refreshBlocks;

    public NativeArray<BlockType> blocks;
    public NativeArray<BiomeType> biomeTypes;
    private NativeHashMap<BlockParameter, short> blockParameters;

    private NativeList<float3> blockVerticles;
    private NativeList<int> blockTriangles;
    private NativeList<float2> blockUVs;

    private NativeList<float3> liquidVerticles;
    private NativeList<int> liquidTriangles;
    private NativeList<float2> liquidUVs;

    private NativeList<float3> plantsVerticles;
    private NativeList<int> plantsTriangles;
    private NativeList<float2> plantsUVs;
    public TerrainChunk[] neigbourChunks = new TerrainChunk[0]; // +x, -x, +z, -z

    private List<BlockData> blocksToBuild = new List<BlockData>();
    private Dictionary<BlockParameter, short> parametersToAdd = new Dictionary<BlockParameter, short>();

    public ChunkPos chunkPos;

    #region // === Monobehaviour === \\

    private void Awake()
    {
        blocks = new NativeArray<BlockType>(fixedChunkWidth * chunkHeight * fixedChunkWidth, Allocator.Persistent);
        biomeTypes = new NativeArray<BiomeType>(fixedChunkWidth * fixedChunkWidth, Allocator.Persistent);
        blockParameters = new NativeHashMap<BlockParameter, short>(2048, Allocator.Persistent);

        blockVerticles = new NativeList<float3>(16384, Allocator.Persistent);
        blockTriangles = new NativeList<int>(32768, Allocator.Persistent);
        blockUVs = new NativeList<float2>(16384, Allocator.Persistent);

        liquidVerticles = new NativeList<float3>(8192, Allocator.Persistent);
        liquidTriangles = new NativeList<int>(16384, Allocator.Persistent);
        liquidUVs = new NativeList<float2>(8192, Allocator.Persistent);

        plantsVerticles = new NativeList<float3>(4096, Allocator.Persistent);
        plantsTriangles = new NativeList<int>(8192, Allocator.Persistent);
        plantsUVs = new NativeList<float2>(4096, Allocator.Persistent);

        chunkDissapearingAnimation = GetComponent<ChunkDissapearingAnimation>();
        chunkAnimation = GetComponent<ChunkAnimation>();
    }

    private void OnEnable()
    {
        TerrainGenerator.timeToBuild += BuildBlocks;
        StartCoroutine(CheckNeighbours());
    }

    private void OnDisable()
    {
        TerrainGenerator.timeToBuild -= BuildBlocks;
    }

    private void OnApplicationQuit()
    {
        blocks.Dispose();
        biomeTypes.Dispose();
        blockParameters.Dispose();

        blockVerticles.Dispose();
        blockTriangles.Dispose();
        blockUVs.Dispose();

        liquidVerticles.Dispose();
        liquidTriangles.Dispose();
        liquidUVs.Dispose();

        plantsVerticles.Dispose();
        plantsTriangles.Dispose();
        plantsUVs.Dispose();
    }

    #endregion

    private IEnumerator CheckNeighbours()
    {
        yield return new WaitForEndOfFrame();
        ChunkPos[] positions = new ChunkPos[]
        {
            new ChunkPos(chunkPos.x + chunkWidth, chunkPos.z),
            new ChunkPos(chunkPos.x - chunkWidth, chunkPos.z),
            new ChunkPos(chunkPos.x, chunkPos.z + chunkWidth),
            new ChunkPos(chunkPos.x, chunkPos.z - chunkWidth)
        };

        neigbourChunks = new TerrainChunk[]
        {
            TerrainGenerator.chunks.ContainsKey(positions[0]) ? TerrainGenerator.chunks[positions[0]] : null,
            TerrainGenerator.chunks.ContainsKey(positions[1]) ? TerrainGenerator.chunks[positions[1]] : null,
            TerrainGenerator.chunks.ContainsKey(positions[2]) ? TerrainGenerator.chunks[positions[2]] : null,
            TerrainGenerator.chunks.ContainsKey(positions[3]) ? TerrainGenerator.chunks[positions[3]] : null,
        };


        if (neigbourChunks[0] && neigbourChunks[0].neigbourChunks.Length > 0)
            neigbourChunks[0].neigbourChunks[1] = this;
        if (neigbourChunks[1] && neigbourChunks[1].neigbourChunks.Length > 0)
            neigbourChunks[1].neigbourChunks[0] = this;
        if (neigbourChunks[2] && neigbourChunks[2].neigbourChunks.Length > 0)
            neigbourChunks[2].neigbourChunks[3] = this;
        if (neigbourChunks[3] && neigbourChunks[3].neigbourChunks.Length > 0)
            neigbourChunks[3].neigbourChunks[2] = this;
    }

    #region // === Utils === \\

    public static int Index2Dto1D(int x, int z)
    {
        return x * fixedChunkWidth + z;
    }

    public static int Index3Dto1D(int x, int y, int z)
    {
        return (z * fixedChunkWidth * chunkHeight) + (y * fixedChunkWidth) + x;
    }

    public static int Index3Dto1D(int3 blockPos)
    {
        return (blockPos.z * fixedChunkWidth * chunkHeight) + (blockPos.y * fixedChunkWidth) + blockPos.x;
    }

    public static int Index3Dto1D(BlockPos blockPos)
    {
        return (blockPos.z * fixedChunkWidth * chunkHeight) + (blockPos.y * fixedChunkWidth) + blockPos.x;
    }

    #endregion

    #region // === Mesh methods === \\

    public void BuildMesh(NativeQueue<JobHandle> jobHandles, int xPos, int zPos)
    {
        GenerateTerrainData generateTerrainData = new GenerateTerrainData()
        {
            chunkPosX = xPos,
            chunkPosZ = zPos,
            blockData = blocks,
            biomeTypes = biomeTypes,
            blockParameters = blockParameters,
            baseNoise = TerrainGenerator.baseNoise,
            generatorSettings = TerrainGenerator.generatorSettings,
            random = new Unity.Mathematics.Random((uint)(xPos * 10000 + zPos + 1000))
        };

        CreateMeshData createMeshData = new CreateMeshData
        {
            chunkPosX = xPos,
            chunkPosZ = zPos,
            blockData = blocks,
            biomeTypes = biomeTypes,
            blockParameters = blockParameters,

            blockVerticles = blockVerticles,
            blockTriangles = blockTriangles,
            blockUVs = blockUVs,

            liquidVerticles = liquidVerticles,
            liquidTriangles = liquidTriangles,
            liquidUVs = liquidUVs,

            plantsVerticles = plantsVerticles,
            plantsTriangles = plantsTriangles,
            plantsUVs = plantsUVs,

            random = new Unity.Mathematics.Random((uint)(xPos * 10000 + zPos + 1000)),
            baseNoise = TerrainGenerator.baseNoise
        };

        JobHandle handle = createMeshData.Schedule(generateTerrainData.Schedule());
        jobHandles.Enqueue(handle);
    }

    public void BuildMesh(List<JobHandle> jobHandles)
    {
        //refreshBlocks = false;

        CreateMeshData createMeshData = new CreateMeshData
        {
            blockData = blocks,
            biomeTypes = biomeTypes,
            blockParameters = blockParameters,

            blockVerticles = blockVerticles,
            blockTriangles = blockTriangles,
            blockUVs = blockUVs,

            liquidVerticles = liquidVerticles,
            liquidTriangles = liquidTriangles,
            liquidUVs = liquidUVs,

            plantsVerticles = plantsVerticles,
            plantsTriangles = plantsTriangles,
            plantsUVs = plantsUVs
        };

        JobHandle handle = createMeshData.Schedule();
        jobHandles.Add(handle);
    }

    public void ApplyMesh()
    {
        Mesh mesh = new Mesh();
        Mesh liquidMesh = new Mesh();
        Mesh plantsMesh = new Mesh();

        // blocks
        mesh.SetVertices<float3>(blockVerticles);
        mesh.SetTriangles(blockTriangles.ToArray(), 0, false);
        mesh.SetUVs<float2>(0, blockUVs);

        // liquids
        liquidMesh.SetVertices<float3>(liquidVerticles);
        liquidMesh.SetTriangles(liquidTriangles.ToArray(), 0, false);
        liquidMesh.SetUVs<float2>(0, liquidUVs);

        //plants
        plantsMesh.SetVertices<float3>(plantsVerticles);
        plantsMesh.SetTriangles(plantsTriangles.ToArray(), 0, false);
        plantsMesh.SetUVs<float2>(0, plantsUVs);

        //mesh.RecalculateNormals();
        blockMeshFilter.mesh = mesh;
        TerrainGenerator.SchedulePhysicsBake(this);

        //liquidMesh.RecalculateNormals();
        liquidMeshFilter.mesh = liquidMesh;

        plantsMeshFilter.mesh = plantsMesh;

        // clear blocks
        blockVerticles.Clear();
        blockTriangles.Clear();
        blockUVs.Clear();

        // clear liquids
        liquidVerticles.Clear();
        liquidTriangles.Clear();
        liquidUVs.Clear();

        // clear plants
        plantsVerticles.Clear();
        plantsTriangles.Clear();
        plantsUVs.Clear();
    }

    public void SetMeshRenderersActive(bool active)
    {
        foreach (var mr in GetComponentsInChildren<MeshRenderer>())
        {
            mr.enabled = active;
        }
    }

    #endregion

    #region // === Animations === \\

    public void Animation()
    {
        ClearAnimations();

        chunkAnimation.enabled = true;
    }

    private void ClearAnimations()
    {
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        chunkAnimation.enabled = false;
        chunkDissapearingAnimation.enabled = false;
    }

    public void DissapearingAnimation()
    {
        ClearAnimations();

        chunkDissapearingAnimation.enabled = true;
    }

    #endregion

    #region // === Parameters === \\

    public void SetParameters(BlockParameter parameter, short value)
    {
        if (blockParameters.ContainsKey(parameter))
            blockParameters[parameter] = value;
        else
            blockParameters.Add(parameter, value);
    }

    public short GetParameterValue(BlockParameter parameter)
    {
        if (blockParameters.ContainsKey(parameter))
        {
            return blockParameters[parameter]; ;
        }

        return 0;
    }

    public void ClearParameters(int x, int y, int z)
    {
        BlockParameter key = new BlockParameter(new int3(x, y, z));
        while (blockParameters.ContainsKey(key))
            blockParameters.Remove(key);
    }

    public void ClearParameters(int3 blockPos)
    {
        BlockParameter key = new BlockParameter(blockPos);
        while (blockParameters.ContainsKey(key))
            blockParameters.Remove(key);
    }

    #endregion

    #region // === Editing methods === \\

    public BlockType GetBlock(int x, int y, int z)
    {
        // TODO: -11 = out of range przy rozlewaniu wody
        return blocks[Index3Dto1D(x, y, z)];
    }

    public BlockType GetBlock(BlockPos blockPos)
    {
        return blocks[Index3Dto1D(blockPos.x, blockPos.y, blockPos.z)];
    }

    public void SetBlock(int x, int y, int z, BlockType blockType)
    {
        blocks[Index3Dto1D(x, y, z)] = blockType;

        int aboveIndex = Index3Dto1D(x, y + 1, z);
        if (blocks[aboveIndex] == BlockType.GRASS)
        {
            blocks[aboveIndex] = BlockType.AIR;
        }
    }

    public void UpdateBlock(int x, int y, int z, BlockType blockType)
    {
        List<JobHandle> jobHandles = new List<JobHandle>();
        List<TerrainChunk> chunksToBuild = new List<TerrainChunk>();

        SetBlock(x, y, z, blockType);
        //blocks[Index3Dto1D(x, y, z)] = blockType;

        // add current chunk
        BuildMesh(jobHandles);
        chunksToBuild.Add(this);

        // check neighbours
        if (x == 16)
        {
            TerrainChunk tc = neigbourChunks[0];
            if (tc)
            {
                tc.blocks[Index3Dto1D(0, y, z)] = blockType;
                tc.BuildMesh(jobHandles);
                chunksToBuild.Add(tc);
            }
        }
        else if (x == 1)
        {
            TerrainChunk tc = neigbourChunks[1];
            if (tc)
            {
                tc.blocks[Index3Dto1D(17, y, z)] = blockType;
                tc.BuildMesh(jobHandles);
                chunksToBuild.Add(tc);
            }
        }

        if (z == 16)
        {
            TerrainChunk tc = neigbourChunks[2];
            if (tc)
            {
                tc.blocks[Index3Dto1D(x, y, 0)] = blockType;
                tc.BuildMesh(jobHandles);
                chunksToBuild.Add(tc);
            }
        }
        else if (z == 1)
        {
            TerrainChunk tc = neigbourChunks[3];
            if (tc)
            {
                tc.blocks[Index3Dto1D(x, y, 17)] = blockType;
                tc.BuildMesh(jobHandles);
                chunksToBuild.Add(tc);
            }
        }

        NativeArray<JobHandle> njobHandles = new NativeArray<JobHandle>(jobHandles.ToArray(), Allocator.Temp);
        JobHandle.CompleteAll(njobHandles);

        // build meshes
        foreach (TerrainChunk tc in chunksToBuild)
        {
            tc.ApplyMesh();
        }

        // clear & dispose
        jobHandles.Clear();
        chunksToBuild.Clear();
        njobHandles.Dispose();

        UpdateNeighbourBlocks(new BlockPos(x, y, z), 10);
    }

    public void UpdateBlocks(BlockData[] blockDatas)
    {
        List<JobHandle> jobHandles = new List<JobHandle>();
        List<TerrainChunk> chunksToBuild = new List<TerrainChunk>();

        bool[] neighboursToBuild = new bool[4];

        //Vector3Int[] positionsToUpdate = new Vector3Int[6];
        for (int i = 0; i < blockDatas.Length; i++)
        {
            int x = blockDatas[i].position.x;
            int y = blockDatas[i].position.y;
            int z = blockDatas[i].position.z;

            BlockType blockType = blockDatas[i].blockType;
            SetBlock(x, y, z, blockType);
            //blocks[Index3Dto1D(x, y, z)] = blockType;

            // check neighbours
            if (x == 16)
            {
                TerrainChunk tc = neigbourChunks[0];
                if (tc)
                {
                    tc.blocks[Index3Dto1D(0, y, z)] = blockType;
                    neighboursToBuild[0] = true;
                }
            }
            else if (x == 1)
            {
                TerrainChunk tc = neigbourChunks[1];
                if (tc)
                {
                    tc.blocks[Index3Dto1D(17, y, z)] = blockType;
                    neighboursToBuild[1] = true;
                }
            }

            if (z == 16)
            {
                TerrainChunk tc = neigbourChunks[2];
                if (tc)
                {
                    tc.blocks[Index3Dto1D(x, y, 0)] = blockType;
                    neighboursToBuild[2] = true;
                }
            }
            else if (z == 1)
            {
                TerrainChunk tc = neigbourChunks[3];
                if (tc)
                {
                    tc.blocks[Index3Dto1D(x, y, 17)] = blockType;
                    neighboursToBuild[3] = true;
                }
            }

            UpdateNeighbourBlocks(new BlockPos(x, y, z), 10);
        }

        // add current chunk
        BuildMesh(jobHandles);
        chunksToBuild.Add(this);

        for (int j = 0; j < 4; j++)
        {
            if (neighboursToBuild[j])
            {
                TerrainChunk tc = neigbourChunks[j];
                tc.BuildMesh(jobHandles);
                chunksToBuild.Add(tc);
            }
        }

        NativeArray<JobHandle> njobHandles = new NativeArray<JobHandle>(jobHandles.ToArray(), Allocator.Temp);
        JobHandle.CompleteAll(njobHandles);

        // build meshes
        foreach (TerrainChunk tc in chunksToBuild)
        {
            tc.ApplyMesh();
        }

        // clear & dispose
        jobHandles.Clear();
        chunksToBuild.Clear();

        njobHandles.Dispose();
    }

    #endregion

    #region // === Block updates === \\

    private void UpdateNeighbourBlocks(BlockPos blockPos, int ticks = 2)
    {
        TerrainGenerator.ScheduleUpdate(this, blockPos, ticks);

        int x = blockPos.x;
        int y = blockPos.y;
        int z = blockPos.z;

        int[] neighbours = new int[6];
        BlockPos[] positions = new BlockPos[]
        {
            new BlockPos(x, y + 1, z, out neighbours[0]),
            new BlockPos(x, y - 1, z, out neighbours[1]),
            new BlockPos(x + 1, y, z, out neighbours[2]),
            new BlockPos(x - 1, y, z, out neighbours[3]),
            new BlockPos(x, y, z + 1, out neighbours[4]),
            new BlockPos(x, y, z - 1, out neighbours[5])
        };
        for (int i = 0; i < 6; i++)
        {
            TerrainChunk chunk = neighbours[i] < 0 ? this : neigbourChunks[neighbours[i]];
            TerrainGenerator.ScheduleUpdate(chunk, positions[i], ticks);
        }
    }

    public void OnBlockUpdate(BlockPos position)
    {
        BlockType blockType = blocks[Index3Dto1D(position.x, position.y, position.z)];

        int x = position.x;
        int y = position.y;
        int z = position.z;

        int[] neighbours = new int[4];
        BlockPos[] sideBlocks = new BlockPos[]
        {
            new BlockPos(x + 1, y, z, out neighbours[0]),
            new BlockPos(x - 1, y, z, out neighbours[1]),
            new BlockPos(x, y, z + 1, out neighbours[2]),
            new BlockPos(x, y, z - 1, out neighbours[3])
        };

        BlockPos[] upDownBlocks = new BlockPos[]
        {
            new BlockPos(x, y + 1, z),
            new BlockPos(x, y - 1, z)
        };

        if (blockType == BlockType.GRASS_BLOCK && TerrainData.GetBlockState(blocks[Index3Dto1D(upDownBlocks[0])]) == BlockState.SOLID)
        {
            AddBlockToBuildList(new BlockData(BlockType.DIRT, position));
            return;
        }

        if (blockType == BlockType.WATER)
        {
            short sourceDistance = GetParameterValue(new BlockParameter(position, ParameterType.WATER_SOURCE_DISTANCE));

            BlockType belowBlock = blocks[Index3Dto1D(upDownBlocks[1])];
            if (TerrainData.GetBlockState(belowBlock) == BlockState.SOLID || sourceDistance == 8)
            {
                for (int i = 0; i < 4; i++)
                {
                    TerrainChunk chunk = neighbours[i] < 0 ? this : neigbourChunks[neighbours[i]];
                    BlockPos blockPos = sideBlocks[i];
                    BlockParameter param = new BlockParameter(blockPos, ParameterType.WATER_SOURCE_DISTANCE);

                    int index = Index3Dto1D(blockPos);
                    BlockType type = chunk.blocks[index];

                    if (sourceDistance > 0)
                    {
                        if (type == BlockType.AIR || TerrainData.GetBlockState(type) == BlockState.PLANTS)
                        {
                            chunk.AddBlockToBuildList(new BlockData(BlockType.WATER, blockPos));
                            chunk.AddParameterToList(param, (short)(sourceDistance - 1));
                            //break;
                        }
                        if (type == BlockType.WATER)
                        {
                            if (chunk.GetParameterValue(param) < sourceDistance - 1)
                            {
                                chunk.AddBlockToBuildList(new BlockData(BlockType.WATER, blockPos));
                                chunk.AddParameterToList(param, (short)(sourceDistance - 1));
                            }
                        }
                    }
                }
            }
            else if (belowBlock == BlockType.AIR || TerrainData.GetBlockState(belowBlock) == BlockState.PLANTS)
            {
                AddBlockToBuildList(new BlockData(BlockType.WATER, upDownBlocks[1]));
                AddParameterToList(new BlockParameter(upDownBlocks[1], ParameterType.WATER_SOURCE_DISTANCE), 8);
            }
        }
    }

    public void AddBlockToBuildList(BlockData data)
    {
        if (!blocksToBuild.Contains(data))
            blocksToBuild.Add(data);
    }

    public void AddParameterToList(BlockParameter param, short value)
    {
        if (!parametersToAdd.ContainsKey(param))
        {
            parametersToAdd.Add(param, value);
        }
    }


    private void BuildBlocks()
    {
        if (blocksToBuild.Count == 0) return;

        foreach (var param in parametersToAdd)
        {
            SetParameters(param.Key, param.Value);
        }

        parametersToAdd.Clear();

        BlockData[] datas = blocksToBuild.ToArray();
        blocksToBuild.Clear();
        UpdateBlocks(datas);
        // TODO: GC alloc
    }

    #endregion
}
