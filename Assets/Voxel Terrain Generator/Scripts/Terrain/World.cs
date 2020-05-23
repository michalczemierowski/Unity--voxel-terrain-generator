using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using VoxelTG.Jobs;
using VoxelTG.Listeners.Interfaces;
using VoxelTG.Terrain.Blocks;
using static VoxelTG.Terrain.WorldSettings;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Terrain
{
    public class World : MonoBehaviour
    {
        public static World Instance;

        #region // === Variables === \\

        #region public / serializable

        public static FastNoise baseNoise;
        public static NativeArray<GeneratorSettings> generatorSettings;
        public static NativeArray<FastNoise> biomeNoises;
        public static int seed;

        public static Transform player;

        public static Dictionary<Vector2Int, Chunk> chunks = new Dictionary<Vector2Int, Chunk>();

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

        private Vector2Int curChunk = new Vector2Int(-1, -1);
        private List<Chunk> pooledChunks = new List<Chunk>();

        #endregion

        #endregion

        #region // === Monobehaviour === \\

        private void Awake()
        {
            // Singleton
            if (Instance)
                Destroy(this);
            else
                Instance = this;

            seed = 1337;// UnityEngine.Random.Range(1000000000, int.MaxValue);
            baseNoise = new FastNoise(seed, 0.005f);

            // init native conainters
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

        private void OnApplicationQuit()
        {
            // dispose native containers
            pendingJobs.Dispose();
            meshBakingJobs.Dispose();
            generatorSettings.Dispose();
        }

        private void Start()
        {
            LoadChunks(true);

            InvokeRepeating("ChunkLoading", 1f / buildChecksPerSecond, 1f / buildChecksPerSecond);
            InvokeRepeating("Tick", 1f / ticksPerSecond, 1f / ticksPerSecond);

            InitializeEvents();
            InitializeListeners();
        }

        private void FixedUpdate()
        {
            LoadChunks();
        }

        #endregion

        #region // === Events === \\

        private delegate void OnBlockUpdate(BlockEventData block, Dictionary<BlockFace, BlockEventData> neighbours, params int[] args);
        private static OnBlockUpdate OnBlockUpdateEvent;

        private delegate void OnBlockDestroy(BlockEventData block, params int[] args);
        private static OnBlockDestroy OnBlockDestroyEvent;

        private static Dictionary<BlockType, OnBlockUpdate> OnBlockUpdateEvents = new Dictionary<BlockType, OnBlockUpdate>();
        private static Dictionary<BlockType, OnBlockDestroy> OnBlockDestroyEvents = new Dictionary<BlockType, OnBlockDestroy>();

        private void InitializeEvents()
        {
            int len = System.Enum.GetNames(typeof(BlockType)).Length;
            for (int i = 0; i < len; i++)
            {
                OnBlockUpdateEvents.Add((BlockType)i, OnBlockUpdateEvent);
                OnBlockDestroyEvents.Add((BlockType)i, OnBlockDestroyEvent);
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

            foreach (IBlockDestroyListener listener in GetComponentsInChildren<IBlockDestroyListener>())
            {
                OnBlockDestroyEvents[listener.GetBlockType()] += listener.OnBlockDestroy;
            }

            foreach (IBlockArrayDestroyListener listener in GetComponentsInChildren<IBlockArrayDestroyListener>())
            {
                foreach (BlockType type in listener.GetBlockTypes())
                {
                    OnBlockDestroyEvents[type] += listener.OnBlockDestroy;
                }
            }
        }

        public static void InvokeBlockUpdateEvent(BlockEventData block, Dictionary<BlockFace, BlockEventData> neighbours, params int[] args)
        {
            OnBlockUpdateEvents[block.type]?.Invoke(block, neighbours, args);
        }

        public static void InvokeBlockDestroyEvent(BlockEventData block, params int[] args)
        {
            OnBlockDestroyEvents[block.type]?.Invoke(block, args);
        }

        #endregion

        #region // === Chunk loading methods === \\

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

                chunk.chunkPos = new Vector2Int(xPos, zPos);
                chunk.transform.position = new Vector3(xPos, 0, zPos);
            }
            else
            {
                GameObject chunkGO = Instantiate(terrainChunk, new Vector3(xPos, 0, zPos), Quaternion.identity);
                chunk = chunkGO.GetComponent<Chunk>();
                chunk.chunkPos = new Vector2Int(xPos, zPos);
            }

            // schedule build job
            chunk.BuildMesh(handlers, xPos, zPos);

            // disable mesh renderers
            chunk.SetMeshRenderersActive(false);

            // add chunk to chunk dict
            chunks.Add(new Vector2Int(xPos, zPos), chunk);
        }

        /// <summary>
        /// Load nearby chunks & unload far chunks
        /// </summary>
        /// <param name="instant"></param>
        private void LoadChunks(bool instant = false)
        {
            //the current chunk the player is in
            int curChunkPosX = Mathf.FloorToInt(player.position.x / chunkWidth) * chunkWidth;
            int curChunkPosZ = Mathf.FloorToInt(player.position.z / chunkWidth) * chunkWidth;

            //entered a new chunk
            if (curChunk.x != curChunkPosX || curChunk.y != curChunkPosZ)
            {
                curChunk.x = curChunkPosX;
                curChunk.y = curChunkPosZ;

                for (int i = curChunkPosX - chunkWidth * chunkDist; i <= curChunkPosX + chunkWidth * chunkDist; i += chunkWidth)
                    for (int j = curChunkPosZ - chunkWidth * chunkDist; j <= curChunkPosZ + chunkWidth * chunkDist; j += chunkWidth)
                    {
                        Vector2Int cp = new Vector2Int(i, j);

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
                List<Vector2Int> toDestroy = new List<Vector2Int>();
                foreach (KeyValuePair<Vector2Int, Chunk> c in chunks)
                {
                    Vector2Int cp = c.Key;
                    if (Mathf.Abs(curChunkPosX - cp.x) > chunkWidth * (chunkDist + 3) ||
                        Mathf.Abs(curChunkPosZ - cp.y) > chunkWidth * (chunkDist + 3))
                    {
                        toDestroy.Add(c.Key);
                    }
                }

                // add chunks to pool
                foreach (Vector2Int cp in toDestroy)
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

        #region // === Chunk & Block methods === \\

        /// <summary>
        /// Get block at provided position
        /// </summary>
        /// <param name="x">world position x</param>
        /// <param name="y">world position y</param>
        /// <param name="z">world position z</param>
        /// <returns></returns>
        public static Block GetBlock(int x, int y, int z)
        {
            Chunk chunk = GetChunk(x, z);
            if (!chunk)
                return WorldData.GetBlock(BlockType.AIR);

            BlockPosition bp = new BlockPosition(x, y, z);

            return WorldData.GetBlock(chunk.GetBlock(bp));
        }

        /// <summary>
        /// Get chunk at provided position
        /// </summary>
        /// <param name="x">chunk position x</param>
        /// <param name="z">chunk position y</param>
        /// <returns>chunk</returns>
        public static Chunk GetChunk(float x, float z)
        {
            int chunkPosX = Mathf.FloorToInt((x - 1) / chunkWidth) * chunkWidth;
            int chunkPosZ = Mathf.FloorToInt((z - 1) / chunkWidth) * chunkWidth;

            Vector2Int cp = new Vector2Int(chunkPosX, chunkPosZ);

            Chunk result;
            chunks.TryGetValue(cp, out result);

            return result;
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
        public static bool ScheduleUpdate(Chunk chunk, BlockPosition blockPos, int ticks, params int[] args)
        {
            if (!updatePositions.Contains(blockPos))
            {
                tickQueue.Add(new TickQueueData(chunk, blockPos, ticks, args));
                updatePositions.Add(blockPos);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Schedule update on block - OnBlockUpdate method will be called with
        /// delay of between "minTicsk" and "maxTicks" ticks
        /// </summary>
        /// <param name="chunk">target chunk</param>
        /// <param name="blockPos">local block position</param>
        /// <param name="ticks">minimum(x) and maximum(y) delay (in ticks)</param>
        public static bool ScheduleUpdate(Chunk chunk, BlockPosition blockPos, int2 ticks, params int[] args)
        {
            if (!updatePositions.Contains(blockPos))
            {
                int tick = UnityEngine.Random.Range(ticks.x, ticks.y);
                tickQueue.Add(new TickQueueData(chunk, blockPos, tick, args));
                updatePositions.Add(blockPos);
                return true;
            }
            return false;
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
                    tickQueue.Remove(tick);
                    updatePositions.Remove(tick.blockPos);
                    
                    tick.chunk.OnBlockUpdate(tick.blockPos, tick.args);
                }
            }
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
        public static int3 LocalToWorldPositionInt3(Vector2Int cp, int x, int y, int z)
        {
            return new int3(x + cp.x - 1, y, z + cp.y - 1);
        }

        /// <summary>
        /// Convert local chunk position to world position
        /// </summary>
        /// <param name="cp">chunk position</param>
        /// <param name="position">local block position</param>
        /// <returns></returns>
        public static int3 LocalToWorldPositionInt3(Vector2Int cp, BlockPosition position)
        {
            return new int3(position.x + cp.x - 1, position.y, position.z + cp.y - 1);
        }

        /// <summary>
        /// Convert local chunk position to world position
        /// </summary>
        /// <param name="cp">chunk position</param>
        /// <param name="position">local block position</param>
        /// <returns></returns>
        public static Vector3Int LocalToWorldPositionVector3Int(Vector2Int cp, BlockPosition position)
        {
            return new Vector3Int(position.x + cp.x - 1, position.y, position.z + cp.y - 1);
        }

        /// <summary>
        /// Convert local chunk position to world position
        /// </summary>
        /// <param name="cp">chunk position</param>
        /// <param name="position">local block position</param>
        /// <returns></returns>
        public static int3 LocalToWorldPositionInt3(Vector2Int cp, int3 position)
        {
            return new int3(position.x + cp.x - 1, position.y, position.z + cp.y - 1);
        }

        #endregion
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