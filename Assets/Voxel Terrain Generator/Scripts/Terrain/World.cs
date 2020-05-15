using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using VoxelTG.Jobs;
using VoxelTG.Listeners.Interfaces;
using VoxelTG.Terrain.Blocks;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Terrain
{
    public class World : MonoBehaviour
    {
        #region // === Variables === \\

        #region public

        public static FastNoise baseNoise;
        public static NativeArray<GeneratorSettings> generatorSettings;
        public static NativeArray<FastNoise> biomeNoises;
        public static int seed;

        public static Transform player;

        public static Dictionary<ChunkPos, Chunk> chunks = new Dictionary<ChunkPos, Chunk>();

        #endregion

        #region serializable

        [SerializeField] private float ticksPerSecond = 20;
        [SerializeField] private float buildChecksPerSecond = 10;
        public int chunkDist = 4;
        public GameObject terrainChunk;

        public GeneratorSettings[] generatorSettingsArray;

        #endregion

        #region private

        private NativeQueue<JobHandle> pendingJobs;
        private static NativeQueue<JobHandle> meshBakingJobs;

        private Queue<Chunk> terrainChunks = new Queue<Chunk>();
        private static Queue<MeshBakeData> terrainCollisionMeshes = new Queue<MeshBakeData>();

        public delegate void TimeToBuild();
        public static event TimeToBuild timeToBuild;

        // ticks
        private static List<TickQueueData> tickQueue = new List<TickQueueData>();
        private static HashSet<BlockPosition> updatePositions = new HashSet<BlockPosition>();

        private ChunkPos curChunk = new ChunkPos(-1, -1);
        private List<Chunk> pooledChunks = new List<Chunk>();

        #endregion

        #endregion

        #region // === Monobehaviour === \\

        private void Awake()
        {
            seed = 1337;// UnityEngine.Random.Range(1000000000, int.MaxValue);
            baseNoise = new FastNoise(seed, 0.005f);

            pendingJobs = new NativeQueue<JobHandle>(Allocator.Persistent);
            meshBakingJobs = new NativeQueue<JobHandle>(Allocator.Persistent);
            generatorSettings = new NativeArray<GeneratorSettings>(generatorSettingsArray, Allocator.Persistent);
            biomeNoises = new NativeArray<FastNoise>(generatorSettingsArray.Length, Allocator.Persistent);

            // load noises from settings
            for (int i = 0; i < generatorSettingsArray.Length; i++)
            {
                NoiseSettings noiseSettings = generatorSettingsArray[i].noiseSettings;
                FastNoise noise = new FastNoise(seed, noiseSettings.frequency, noiseSettings.interp, noiseSettings.noiseType,
                                                noiseSettings.octaves, noiseSettings.lancuarity, noiseSettings.gain, noiseSettings.fractalType);
                biomeNoises[i] = noise;
            }
        }

        void Start()
        {
            LoadChunks(true);

            InvokeRepeating("ChunkLoading", 1f / buildChecksPerSecond, 1f / buildChecksPerSecond);
            InvokeRepeating("Tick", 1f / ticksPerSecond, 1f / ticksPerSecond);

            InitializeEvents();
            InitializeListeners();
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

        #region // === Events === \\

        private delegate void OnBlockUpdated(BlockUpdateEventData block, Dictionary<BlockFace, BlockUpdateEventData> neighbours);
        private static OnBlockUpdated OnBlockUpdatedEvent;
        private static Dictionary<BlockType, OnBlockUpdated> OnBlockUpdateEvents = new Dictionary<BlockType, OnBlockUpdated>();

        private void InitializeEvents()
        {
            int len = System.Enum.GetNames(typeof(BlockType)).Length;
            for (int i = 0; i < len; i++)
            {
                OnBlockUpdateEvents.Add((BlockType)i, OnBlockUpdatedEvent);
            }
        }

        private void InitializeListeners()
        {
            foreach (IBlockUpdateListener listener in GetComponentsInChildren<IBlockUpdateListener>())
            {
                OnBlockUpdateEvents[listener.GetBlockType()] += listener.OnBlockUpdate;
            }

            foreach (IBlockArrayUpdateListener listener in GetComponentsInChildren<IBlockArrayUpdateListener>())
            {
                foreach (BlockType type in listener.GetBlockTypes())
                {
                    OnBlockUpdateEvents[type] += listener.OnBlockUpdate;
                }
            }
        }

        public static void InvokeBlockUpdateEvent(BlockType blockType, BlockUpdateEventData block, Dictionary<BlockFace, BlockUpdateEventData> neighbours)
        {
            OnBlockUpdateEvents[blockType]?.Invoke(block, neighbours);
        }

        #endregion

        #region // === Chunk methods === \\

        /// <summary>
        /// Schedule chunk build job
        /// </summary>
        /// <param name="xPos">x position of chunk</param>
        /// <param name="zPos">z position of chunk</param>
        /// <param name="handlers">list of job handles</param>
        /// <param name="chunk">reference to target chunk</param>
        private void BuildChunk(int xPos, int zPos, NativeQueue<JobHandle> handlers, ref Chunk chunk)
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
                chunk = chunkGO.GetComponent<Chunk>();
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
        /// Load nearby chunks & unload far chunks
        /// </summary>
        /// <param name="instant"></param>
        private void LoadChunks(bool instant = false)
        {
            //the current chunk the player is in
            int curChunkPosX = Mathf.FloorToInt(player.position.x / Chunk.chunkWidth) * Chunk.chunkWidth;
            int curChunkPosZ = Mathf.FloorToInt(player.position.z / Chunk.chunkWidth) * Chunk.chunkWidth;

            //entered a new chunk
            if (curChunk.x != curChunkPosX || curChunk.z != curChunkPosZ)
            {
                curChunk.x = curChunkPosX;
                curChunk.z = curChunkPosZ;

                for (int i = curChunkPosX - Chunk.chunkWidth * chunkDist; i <= curChunkPosX + Chunk.chunkWidth * chunkDist; i += Chunk.chunkWidth)
                    for (int j = curChunkPosZ - Chunk.chunkWidth * chunkDist; j <= curChunkPosZ + Chunk.chunkWidth * chunkDist; j += Chunk.chunkWidth)
                    {
                        ChunkPos cp = new ChunkPos(i, j);

                        if (!chunks.ContainsKey(cp))
                        {
                            Chunk c = null;
                            BuildChunk(i, j, pendingJobs, ref c);
                            terrainChunks.Enqueue(c);
                        }
                    }

                // if instant == true, wait for all jobs to complete
                if (instant)
                {
                    JobHandle.CompleteAll(pendingJobs.ToArray(Allocator.TempJob));

                    foreach (Chunk chunk in terrainChunks)
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
                foreach (KeyValuePair<ChunkPos, Chunk> c in chunks)
                {
                    ChunkPos cp = c.Key;
                    if (Mathf.Abs(curChunkPosX - cp.x) > Chunk.chunkWidth * (chunkDist + 3) ||
                        Mathf.Abs(curChunkPosZ - cp.z) > Chunk.chunkWidth * (chunkDist + 3))
                    {
                        toDestroy.Add(c.Key);
                    }
                }

                // add chunks to pool
                foreach (ChunkPos cp in toDestroy)
                {
                    Chunk tc = chunks[cp];
                    tc.DissapearingAnimation();

                    pooledChunks.Add(tc);
                    chunks.Remove(cp);
                }
            }
        }

        /// <summary>
        /// Schedule mesh collider PhysicsX bake job
        /// </summary>
        /// <param name="chunk">target chunk</param>
        public static void SchedulePhysicsBake(Chunk chunk)
        {
            int meshID = chunk.blockMeshFilter.mesh.GetInstanceID();

            BakePhysicsXMesh bakePhysics = new BakePhysicsXMesh()
            {
                meshID = meshID
            };

            meshBakingJobs.Enqueue(bakePhysics.Schedule());
            terrainCollisionMeshes.Enqueue(new MeshBakeData(chunk.blockMeshCollider, chunk.blockMeshFilter.mesh));
        }

        #endregion

        #region // === Utils === \\

        /// <summary>
        /// Convert local chunk position to world position
        /// </summary>
        /// <param name="cp">chunk position</param>
        /// <param name="x">local position x</param>
        /// <param name="y">local position y</param>
        /// <param name="z">local position z</param>
        /// <returns>World block position</returns>
        public static int3 LocalToWorldPositionInt3(ChunkPos cp, int x, int y, int z)
        {
            return new int3(x + cp.x - 1, y, z + cp.z - 1);
        }

        /// <summary>
        /// Convert local chunk position to world position
        /// </summary>
        /// <param name="cp">chunk position</param>
        /// <param name="position">local block position</param>
        /// <returns></returns>
        public static int3 LocalToWorldPositionInt3(ChunkPos cp, BlockPosition position)
        {
            return new int3(position.x + cp.x - 1, position.y, position.z + cp.z - 1);
        }

        /// <summary>
        /// Convert local chunk position to world position
        /// </summary>
        /// <param name="cp">chunk position</param>
        /// <param name="position">local block position</param>
        /// <returns></returns>
        public static Vector3Int LocalToWorldPositionVector3Int(ChunkPos cp, BlockPosition position)
        {
            return new Vector3Int(position.x + cp.x - 1, position.y, position.z + cp.z - 1);
        }

        /// <summary>
        /// Convert local chunk position to world position
        /// </summary>
        /// <param name="cp">chunk position</param>
        /// <param name="position">local block position</param>
        /// <returns></returns>
        public static int3 LocalToWorldPositionInt3(ChunkPos cp, int3 position)
        {
            return new int3(position.x + cp.x - 1, position.y, position.z + cp.z - 1);
        }

        #endregion

        #region // === Getters === \\

        /// <summary>
        /// Get block at provided position
        /// </summary>
        /// <param name="x">world position x</param>
        /// <param name="y">world position y</param>
        /// <param name="z">world position z</param>
        /// <returns></returns>
        public static Block GetBlock(int x, int y, int z)
        {
            int chunkPosX = Mathf.FloorToInt(x / Chunk.chunkWidth) * Chunk.chunkWidth;
            int chunkPosZ = Mathf.FloorToInt(z / Chunk.chunkWidth) * Chunk.chunkWidth;
            ChunkPos cp = new ChunkPos(chunkPosX, chunkPosZ);

            int bix = x - chunkPosX + 1;
            int biy = y;
            int biz = z - chunkPosZ + 1;

            return WorldData.GetBlock(chunks[cp].GetBlock(bix, biy, biz));
        }

        /// <summary>
        /// Get chunk at provided position
        /// </summary>
        /// <param name="x">chunk position x</param>
        /// <param name="z">chunk position y</param>
        /// <returns></returns>
        public static Chunk GetChunk(int x, int z)
        {
            int chunkPosX = Mathf.FloorToInt((float)x / Chunk.chunkWidth) * Chunk.chunkWidth;
            int chunkPosZ = Mathf.FloorToInt((float)z / Chunk.chunkWidth) * Chunk.chunkWidth;

            ChunkPos cp = new ChunkPos(chunkPosX, chunkPosZ);

            return chunks[cp];
        }

        public static Chunk GetChunkByBlockPosition(int x, int z)
        {
            int chunkPosX = Mathf.FloorToInt(x / Chunk.chunkWidth) * Chunk.chunkWidth;
            int chunkPosZ = Mathf.FloorToInt(z / Chunk.chunkWidth) * Chunk.chunkWidth;

            return chunks[new ChunkPos(chunkPosX, chunkPosZ)];
        }
        public static Chunk GetChunkByBlockPosition(float x, float z)
        {
            int chunkPosX = Mathf.FloorToInt(x / Chunk.chunkWidth) * Chunk.chunkWidth;
            int chunkPosZ = Mathf.FloorToInt(z / Chunk.chunkWidth) * Chunk.chunkWidth;

            return chunks[new ChunkPos(chunkPosX, chunkPosZ)];
        }
        #endregion

        #region // === Ticks === \\

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

                    Chunk tc = terrainChunks.Dequeue();

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
        public static void ScheduleUpdate(Chunk chunk, BlockPosition blockPos, int ticks)
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
        public static void ScheduleUpdate(Chunk chunk, BlockPosition blockPos, int minTicks, int maxTicks)
        {
            if (!updatePositions.Contains(blockPos))
            {
                int ticks = UnityEngine.Random.Range(minTicks, maxTicks);
                tickQueue.Add(new TickQueueData(chunk, blockPos, ticks));
                updatePositions.Add(blockPos);
            }
        }

        /// <summary>
        /// Check all queued ticks
        /// </summary>
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
}