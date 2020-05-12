using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public static FastNoise baseNoise;
    public static NativeArray<GeneratorSettings> generatorSettings;
    public static int seed;

    public static Transform player;

    public static Dictionary<ChunkPos, TerrainChunk> chunks = new Dictionary<ChunkPos, TerrainChunk>();

    public int chunkDist = 4;
    public GameObject terrainChunk;
    public List<TerrainChunk> pooledChunks = new List<TerrainChunk>();

    public GeneratorSettings[] generatorSettingsArray;

    private NativeQueue<JobHandle> pendingJobs;
    private static NativeQueue<JobHandle> meshBakingJobs;

    private Queue<TerrainChunk> terrainChunks = new Queue<TerrainChunk>();
    private static Queue<MeshBakeData> terrainCollisionMeshes = new Queue<MeshBakeData>();

    public delegate void TimeToBuild();
    public static event TimeToBuild timeToBuild;

    // ticks
    private static List<TickQueueData> tickQueue = new List<TickQueueData>();
    private static HashSet<BlockPos> updatePositions = new HashSet<BlockPos>();

    ChunkPos curChunk = new ChunkPos(-1, -1);

    #region Monobehaviours

    private void Awake()
    {
        seed = 1337;// UnityEngine.Random.Range(1000000000, int.MaxValue);
        baseNoise = new FastNoise(seed, 0.005f);

        pendingJobs = new NativeQueue<JobHandle>(Allocator.Persistent);
        meshBakingJobs = new NativeQueue<JobHandle>(Allocator.Persistent);
        generatorSettings = new NativeArray<GeneratorSettings>(generatorSettingsArray, Allocator.Persistent);
    }

    void Start()
    {
        LoadChunks(true);

        InvokeRepeating("ChunkLoading", 0.1f, 0.1f);
        InvokeRepeating("Tick", 0.05f, 0.05f);

        //Debug.Log(FastNoise.CalculateFractalBounding());
    }

    private void Update()
    {
        LoadChunks();
    }

    private void OnApplicationQuit()
    {
        pendingJobs.Dispose();
        meshBakingJobs.Dispose();
        generatorSettings.Dispose();
    }

    #endregion

    /// <summary>
    /// Schedule chunk build job
    /// </summary>
    /// <param name="xPos">x position of chunk</param>
    /// <param name="zPos">z position of chunk</param>
    /// <param name="handlers">list of job handles</param>
    /// <param name="chunk">reference to target chunk</param>
    void BuildChunk(int xPos, int zPos, NativeQueue<JobHandle> handlers, ref TerrainChunk chunk)
    {
        if (pooledChunks.Count > 0)//look in the poo first
        {
            chunk = pooledChunks[pooledChunks.Count - 1];
            chunk.gameObject.SetActive(true);
            pooledChunks.RemoveAt(pooledChunks.Count - 1);

            chunk.chunkPos = new ChunkPos(xPos, zPos);
            chunk.transform.position = new Vector3(xPos, 0, zPos);
        }
        else
        {
            GameObject chunkGO = Instantiate(terrainChunk, new Vector3(xPos, 0, zPos), Quaternion.identity);
            chunk = chunkGO.GetComponent<TerrainChunk>();
            chunk.chunkPos = new ChunkPos(xPos, zPos);
        }

        // schedule build job
        chunk.BuildMesh(handlers, xPos, zPos);

        // disable mesh renderers
        chunk.SetMeshRenderersActive(false);

        // add chunk to chunk dict
        chunks.Add(new ChunkPos(xPos, zPos), chunk);
    }

    /// <summary>
    /// Get block type from noise values
    /// </summary>
    /// <param name="x">x position</param>
    /// <param name="y">y position</param>
    /// <param name="z">z position</param>
    /// <returns></returns>
    [System.Obsolete("GetBlockType is deprecated, please use Method2 instead.")]
    public static BlockType GetBlockType(int x, int y, int z, bool grass)
    {
        if (y == 0)
            return BlockType.OAK_LOG;
        //else
        //    return BlockType.Air;


        //print(FastNoise.GetSimplex(x, z));
        float simplex1 = baseNoise.GetSimplex(x * .8f, z * .8f) * 10;
        float simplex2 = baseNoise.GetSimplex(x * 3f, z * 3f) * 10 * (baseNoise.GetSimplex(x * .3f, z * .3f) + .5f);

        float heightMap = simplex1 + simplex2;

        //add the 2d FastNoise to the middle of the terrain chunk
        float baseLandHeight = TerrainChunk.chunkHeight * .5f + heightMap;

        //3d FastNoise for caves and overhangs and such
        float caveFastNoise1 = baseNoise.GetPerlinFractal(x * 5f, y * 10f, z * 5f);
        float caveMask = baseNoise.GetSimplex(x * .3f, z * .3f) + .3f;

        //stone layer heightmap
        float simplexStone1 = baseNoise.GetSimplex(x * 1f, z * 1f) * 10;
        float simplexStone2 = (baseNoise.GetSimplex(x * 5f, z * 5f) + .5f) * 20 * (baseNoise.GetSimplex(x * .3f, z * .3f) + .5f);

        float stoneHeightMap = simplexStone1 + simplexStone2;
        float baseStoneHeight = TerrainChunk.chunkHeight * .25f + stoneHeightMap;


        //float cliffThing = FastNoise.GetSimplex(x * 1f, z * 1f, y) * 10;
        //float cliffThingMask = FastNoise.GetSimplex(x * .4f, z * .4f) + .3f;



        BlockType blockType = BlockType.AIR;

        //under the surface, dirt block
        if (caveFastNoise1 > Mathf.Max(caveMask, .2f))
            blockType = BlockType.AIR;
        else if (y <= baseLandHeight)
        {
            blockType = BlockType.DIRT;

            //just on the surface, use a grass type
            if (y > baseLandHeight - 1 && y > TerrainChunk.waterHeight - 2)
                blockType = grass ? BlockType.GRASS : BlockType.AIR;
            else if (y > baseLandHeight - 2 && y > TerrainChunk.waterHeight - 2)
                blockType = BlockType.GRASS_BLOCK;
            else if (y <= baseStoneHeight)
                blockType = BlockType.STONE;
        }

        /*if(blockType != BlockType.Air)
            blockType = BlockType.Stone;*/

        //if(blockType == BlockType.Air && FastNoise.GetSimplex(x * 4f, y * 4f, z*4f) < 0)
        //  blockType = BlockType.Dirt;

        //if(Mathf.PerlinFastNoise(x * .1f, z * .1f) * 10 + y < TerrainChunk.chunkHeight * .5f)
        //    return BlockType.Grass;

        return blockType;
    }

    void LoadChunks(bool instant = false)
    {
        //the current chunk the player is in
        int curChunkPosX = Mathf.FloorToInt(player.position.x / TerrainChunk.chunkWidth) * TerrainChunk.chunkWidth;
        int curChunkPosZ = Mathf.FloorToInt(player.position.z / TerrainChunk.chunkWidth) * TerrainChunk.chunkWidth;

        //entered a new chunk
        if (curChunk.x != curChunkPosX || curChunk.z != curChunkPosZ)
        {
            curChunk.x = curChunkPosX;
            curChunk.z = curChunkPosZ;

            for (int i = curChunkPosX - TerrainChunk.chunkWidth * chunkDist; i <= curChunkPosX + TerrainChunk.chunkWidth * chunkDist; i += TerrainChunk.chunkWidth)
                for (int j = curChunkPosZ - TerrainChunk.chunkWidth * chunkDist; j <= curChunkPosZ + TerrainChunk.chunkWidth * chunkDist; j += TerrainChunk.chunkWidth)
                {
                    ChunkPos cp = new ChunkPos(i, j);

                    if (!chunks.ContainsKey(cp))
                    {
                        TerrainChunk c = null;
                        BuildChunk(i, j, pendingJobs, ref c);
                        terrainChunks.Enqueue(c);
                    }
                }

            // if instant == true, wait for all jobs to complete
            if (instant)
            {
                JobHandle.CompleteAll(pendingJobs.ToArray(Allocator.TempJob));

                foreach (TerrainChunk chunk in terrainChunks)
                {
                    chunk.ApplyMesh();
                    chunk.SetMeshRenderersActive(true);
                }

                JobHandle.CompleteAll(meshBakingJobs.ToArray(Allocator.TempJob));

                while (terrainCollisionMeshes.Count > 0)
                {
                    MeshBakeData data = terrainCollisionMeshes.Dequeue();
                    data.meshCollider.sharedMesh = data.mesh;
                }

                meshBakingJobs.Clear();
                pendingJobs.Clear();
                terrainChunks.Clear();
            }

            // unload far chunks
            List<ChunkPos> toDestroy = new List<ChunkPos>();
            foreach (KeyValuePair<ChunkPos, TerrainChunk> c in chunks)
            {
                ChunkPos cp = c.Key;
                if (Mathf.Abs(curChunkPosX - cp.x) > TerrainChunk.chunkWidth * (chunkDist + 3) ||
                    Mathf.Abs(curChunkPosZ - cp.z) > TerrainChunk.chunkWidth * (chunkDist + 3))
                {
                    toDestroy.Add(c.Key);
                }
            }

            // add chunks to pool
            foreach (ChunkPos cp in toDestroy)
            {
                TerrainChunk tc = chunks[cp];
                tc.DissapearingAnimation();

                pooledChunks.Add(tc);
                chunks.Remove(cp);
            }
        }
    }

    /// <summary>
    /// Schedule mesh collider PhysicsX bake
    /// </summary>
    /// <param name="chunk">target chunk</param>
    public static void SchedulePhysicsBake(TerrainChunk chunk)
    {
        int meshID = chunk.blockMeshFilter.mesh.GetInstanceID();

        BakePhysicsXMesh bakePhysics = new BakePhysicsXMesh()
        {
            meshID = meshID
        };

        meshBakingJobs.Enqueue(bakePhysics.Schedule());
        terrainCollisionMeshes.Enqueue(new MeshBakeData(chunk.blockMeshCollider, chunk.blockMeshFilter.mesh));
    }

    #region Utils

    /// <summary>
    /// Convert local chunk position to world position
    /// </summary>
    /// <param name="cp">chunk position</param>
    /// <param name="x">local position x</param>
    /// <param name="y">local position y</param>
    /// <param name="z">local position z</param>
    /// <returns>World block position</returns>
    public static int3 LocalToWorldPosition(ChunkPos cp, int x, int y, int z)
    {
        return new int3(x + cp.x - 1, y, z + cp.z - 1);
    }

    /// <summary>
    /// Convert local chunk position to world position
    /// </summary>
    /// <param name="cp">chunk position</param>
    /// <param name="position">local block position</param>
    /// <returns></returns>
    public static int3 LocalToWorldPosition(ChunkPos cp, BlockPos position)
    {
        return new int3(position.x + cp.x - 1, position.y, position.z + cp.z - 1);
    }

    /// <summary>
    /// Convert local chunk position to world position
    /// </summary>
    /// <param name="cp">chunk position</param>
    /// <param name="position">local block position</param>
    /// <returns></returns>
    public static int3 LocalToWorldPosition(ChunkPos cp, int3 position)
    {
        return new int3(position.x + cp.x - 1, position.y, position.z + cp.z - 1);
    }

    #endregion

    #region Getters

    /// <summary>
    /// Get block at provided position
    /// </summary>
    /// <param name="x">world position x</param>
    /// <param name="y">world position y</param>
    /// <param name="z">world position z</param>
    /// <returns></returns>
    public static Block GetBlock(int x, int y, int z)
    {
        int chunkPosX = Mathf.FloorToInt(x / TerrainChunk.chunkWidth) * TerrainChunk.chunkWidth;
        int chunkPosZ = Mathf.FloorToInt(z / TerrainChunk.chunkWidth) * TerrainChunk.chunkWidth;
        ChunkPos cp = new ChunkPos(chunkPosX, chunkPosZ);

        int bix = x - chunkPosX + 1;
        int biy = y;
        int biz = z - chunkPosZ + 1;

        return TerrainData.GetBlock(chunks[cp].GetBlock(bix, biy, biz));
    }

    /// <summary>
    /// Get chunk at provided position
    /// </summary>
    /// <param name="x">chunk position x</param>
    /// <param name="z">chunk position y</param>
    /// <returns></returns>
    public static TerrainChunk GetChunk(int x, int z)
    {
        int chunkPosX = Mathf.FloorToInt((float)x / TerrainChunk.chunkWidth) * TerrainChunk.chunkWidth;
        int chunkPosZ = Mathf.FloorToInt((float)z / TerrainChunk.chunkWidth) * TerrainChunk.chunkWidth;

        ChunkPos cp = new ChunkPos(chunkPosX, chunkPosZ);

        return chunks[cp];
    }

    #endregion

    #region Ticks

    /// <summary>
    /// Checks if chunk building jobs are ready
    /// </summary>
    private void ChunkLoading()
    {
        if (pendingJobs.Count > 0)
        {
            if (pendingJobs.Peek().IsCompleted)
            {
                pendingJobs.Dequeue().Complete();

                TerrainChunk tc = terrainChunks.Dequeue();

                tc.gameObject.SetActive(true);
                tc.Animation();
                tc.ApplyMesh();

                // TODO: check if player is close
                ChunkLoading();
            }
        }

        if (meshBakingJobs.Count > 0)
        {
            if (meshBakingJobs.Peek().IsCompleted)
            {
                meshBakingJobs.Dequeue().Complete();

                MeshBakeData data = terrainCollisionMeshes.Dequeue();
                data.meshCollider.sharedMesh = data.mesh;
            }
        }

        timeToBuild.Invoke();
    }

    /// <summary>
    /// Schedule update on block - OnBlockUpdate method will be called with
    /// delay of "ticks" ticks
    /// </summary>
    /// <param name="chunk">target chunk</param>
    /// <param name="blockPos">local block position</param>
    /// <param name="ticks">delay (in ticks)</param>
    public static void ScheduleUpdate(TerrainChunk chunk, BlockPos blockPos, int ticks)
    {
        if (!updatePositions.Contains(blockPos))
        {
            tickQueue.Add(new TickQueueData(chunk, blockPos, ticks));
            updatePositions.Add(blockPos);
        }
    }

    /// <summary>
    /// Schedule update on block - OnBlockUpdate method will be called with
    /// delay of between "minTicsk" and "maxTicks" ticks
    /// </summary>
    /// <param name="chunk">target chunk</param>
    /// <param name="blockPos">local block position</param>
    /// <param name="minTicks">minimum delay (in ticks)</param>
    /// <param name="maxTicks">maximum delay (in ticks)</param>
    public static void ScheduleUpdate(TerrainChunk chunk, BlockPos blockPos, int minTicks, int maxTicks)
    {
        if (!updatePositions.Contains(blockPos))
        {
            int ticks = UnityEngine.Random.Range(minTicks, maxTicks);
            tickQueue.Add(new TickQueueData(chunk, blockPos, ticks));
            updatePositions.Add(blockPos);
        }
    }

    private void Tick()
    {
        foreach (var tick in tickQueue.ToArray())
        {
            tick.ticks -= 1;
            if (tick.ticks <= 0)
            {
                tick.chunk.OnBlockUpdate(tick.blockPos);

                tickQueue.Remove(tick);
                updatePositions.Remove(tick.blockPos);
            }
        }
    }

    #endregion
}


public struct ChunkPos
{
    public int x, z;
    public ChunkPos(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public override string ToString()
    {
        return "{" + x + ", " + z + "}";
    }
}

public struct MeshBakeData
{
    public MeshCollider meshCollider;
    public Mesh mesh;

    public MeshBakeData(MeshCollider meshCollider, Mesh mesh)
    {
        this.meshCollider = meshCollider;
        this.mesh = mesh;
    }
}