using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using VoxelTG.Effects.SFX;
using VoxelTG.Entities;
using VoxelTG.Jobs;
using VoxelTG.Listeners.Interfaces;
using VoxelTG.Player;
using VoxelTG.Terrain.Blocks;
using VoxelTG.Entities.Items;
using VoxelTG.Effects.VFX;
using VoxelTG.Terrain;
using static VoxelTG.WorldSettings;
using VoxelTG.Config;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG
{
    public class World : MonoBehaviour
    {
        #region // === Variables === \\

        #region  static

        /// <summary>
        /// singleton
        /// </summary>
        public static World Instance;

        // TODO: xml docs
        public static int LoadedChunks { get; private set; }
        public static int TotalChunks { get; private set; }
        public static int Seed { get; private set; }
        public static int RenderDistance { get; private set; }
        public static int CurrentTick { get; private set; }

        public static FastNoise FastNoise { get; private set; }
        public static WorldSave WorldSave { get; private set; } = new WorldSave();
        public static Dictionary<Vector2Int, Chunk> chunks { get; private set; } = new Dictionary<Vector2Int, Chunk>();

        #endregion

        #region public / serializable

        [Header("Prefabs")]
        [SerializeField] private GameObject playerPrefab;

        [Header("References")]
        [SerializeField] private EntityManager entityManager;
        public static EntityManager EntityManager => Instance.entityManager;

        [SerializeField] private SoundManager soundManager;
        public static SoundManager SoundManager => Instance.soundManager;

        [SerializeField] private DroppedItemsManager droppedItemsManager;
        public static DroppedItemsManager DroppedItemsManager => Instance.droppedItemsManager;

        [SerializeField] private ParticleManager particleManager;
        public static ParticleManager ParticleManager => Instance.particleManager;

        [Header("Settings")]
        [SerializeField] private float ticksPerSecond = 20;
        [SerializeField] private float buildChecksPerSecond = 10;
        [SerializeField] private GameObject chunkPrefab;
        [SerializeField] private BiomeColorsObject biomeColors;

        #endregion

        #region private

        private NativeQueue<JobHandle> pendingJobs;
        private static NativeQueue<JobHandle> meshBakingJobs;

        private Queue<Chunk> terrainChunks = new Queue<Chunk>();
        private static Queue<Chunk> terrainCollisionMeshes = new Queue<Chunk>();

        // TODO: try to reduce GC alloc
        private static List<WorldEventQueueData> tickQueue = new List<WorldEventQueueData>();
        private static HashSet<BlockPosition> updatePositions = new HashSet<BlockPosition>();

        private Vector2Int chunkAtPlayerPosition = new Vector2Int(-1, -1);
        private List<Chunk> pooledChunks = new List<Chunk>();

        private int maxChunksToBuildAtOnce;

        #endregion

        #endregion

        #region events

        public static event Action TimeToBuild;
        public static event Action<int> OnTick;

        #endregion

        // TODO: there must be a better way to handle this
        /// <summary>
        /// Save world data to disk
        /// </summary>
        private void SaveChunkData()
        {
            // TODO: enable saving later
            return;
            BinaryFormatter formatter = new BinaryFormatter();
            string path = Application.persistentDataPath + "/world0";

            FileStream stream = new FileStream(path, FileMode.Create);

            WorldSave.playerPosition = SerializableVector3.FromVector3(PlayerController.PlayerTransform.position);
            WorldSave.playerEulerY = PlayerController.PlayerTransform.eulerAngles.y;

            formatter.Serialize(stream, WorldSave);
            stream.Close();

            Debug.Log($"SAVED {WorldSave.savedChunks.Count} CHUNKS");
        }
        
        // TODO: there must be a better way to handle this
        /// <summary>
        /// Load world data from disk if exists
        /// </summary>
        private void LoadChunkData()
        {
            string path = Application.persistentDataPath + "/world0";
            if (File.Exists(path))
            {
                FileStream stream = new FileStream(path, FileMode.Open);
                if (stream.Length > 0)
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    WorldSave = formatter.Deserialize(stream) as WorldSave;
                }
            }
        }

        #region // === Monobehaviour === \\

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void BeforeSceneLoad()
        {
            if (!GameManager.gameCorrectlyLoaded && UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex != 0)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
                return;
            }
        }
#endif
        private void Awake()
        {
            // Singleton
            if (Instance)
                Destroy(this);
            else
                Instance = this;

            WorldSettings.Init();

            // instantiate player gameobject
            GameObject playerObject = Instantiate(Instance.playerPrefab);
            playerObject.SetActive(false);

            RenderDistance = Settings.GetSetting(SettingsType.RENDER_DISTANCE);
            maxChunksToBuildAtOnce = Settings.GetSetting(SettingsType.MAX_CHUNKS_TO_BUILD_AT_ONCE);

            TotalChunks = 4 * RenderDistance * RenderDistance;

            PathFinding.Init();

            // TODO: enable this later
            //LoadChunkData();

            // TODO: enable this later
            //player.eulerAngles = new Vector3(player.eulerAngles.x, worldSave.playerEulerY, player.eulerAngles.z);
            //player.position = worldSave.playerPosition.ToVector3();

            // TODO: seed is always the same
            Seed = UnityEngine.Random.Range(0, int.MaxValue);
            //Seed = 420;
            FastNoise = new FastNoise(Seed, 0.005f);

            // init native conainters
            pendingJobs = new NativeQueue<JobHandle>(Allocator.Persistent);
            meshBakingJobs = new NativeQueue<JobHandle>(Allocator.Persistent);
        }

        private void OnDestroy()
        {
            SaveChunkData();
            
            PathFinding.Dispose();
            WorldSettings.Dispose();

            // dispose native containers
            pendingJobs.Dispose();
            meshBakingJobs.Dispose();
        }


        private void Start()
        {
            // load chunks
            LoadChunks();

            // start invoking chunk loading task
            InvokeRepeating(nameof(ChunkLoading), 1f / buildChecksPerSecond, 1f / buildChecksPerSecond);
            // start invoking tick task
            InvokeRepeating(nameof(Tick), 1f / ticksPerSecond, 1f / ticksPerSecond);

            InitializeEvents();
            InitializeListeners();
        }

        #endregion

        #region // === Events === \\

        private delegate void OnBlockUpdate(BlockEventData block, Dictionary<BlockFace, BlockEventData> neighbours, params int[] args);
        /// <summary>
        /// Event is called when any neighbour of block has changed
        /// </summary>
        private static OnBlockUpdate OnBlockUpdateEvent;

        private delegate void OnBlockDestroy(BlockEventData block, params int[] args);
        /// <summary>
        /// Event is called when block gets removed
        /// </summary>
        private static OnBlockDestroy OnBlockDestroyEvent;

        private delegate void OnBlockPlace(BlockEventData block, params int[] args);
        /// <summary>
        /// Event is called when block is placed by player
        /// </summary>
        private static OnBlockPlace OnBlockPlaceEvent;

        private static Dictionary<BlockType, OnBlockUpdate> OnBlockUpdateEvents = new Dictionary<BlockType, OnBlockUpdate>();
        private static Dictionary<BlockType, OnBlockDestroy> OnBlockDestroyEvents = new Dictionary<BlockType, OnBlockDestroy>();
        private static Dictionary<BlockType, OnBlockPlace> OnBlockPlaceEvents = new Dictionary<BlockType, OnBlockPlace>();

        /// <summary>
        /// Initialize containters with all block types
        /// </summary>
        private void InitializeEvents()
        {
            int len = System.Enum.GetNames(typeof(BlockType)).Length;
            for (int i = 0; i < len; i++)
            {
                OnBlockUpdateEvents.Add((BlockType)i, OnBlockUpdateEvent);
                OnBlockDestroyEvents.Add((BlockType)i, OnBlockDestroyEvent);
                OnBlockPlaceEvents.Add((BlockType)i, OnBlockPlaceEvent);
            }
        }

        /// <summary>
        /// Search for all event listeners and add them to dictionary
        /// </summary>
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

            foreach (IBlockPlaceListener listener in GetComponentsInChildren<IBlockPlaceListener>())
            {
                OnBlockPlaceEvents[listener.GetBlockType()] += listener.OnBlockPlaced;
            }

            foreach (IBlockArrayPlaceListener listener in GetComponentsInChildren<IBlockArrayPlaceListener>())
            {
                foreach (BlockType type in listener.GetBlockTypes())
                {
                    OnBlockPlaceEvents[type] += listener.OnBlockPlaced;
                }
            }
        }

        /// <summary>
        /// Call BlockUpdateEvent
        /// </summary>
        /// <param name="blockEventData">data of block that is calling update</param>
        /// <param name="neighbourBlocksData">data of neighbour blocks</param>
        public static void InvokeBlockUpdateEvent(BlockEventData blockEventData, Dictionary<BlockFace, BlockEventData> neighbourBlocksData, params int[] args)
        {
            OnBlockUpdateEvents[blockEventData.blockType]?.Invoke(blockEventData, neighbourBlocksData, args);
        }

        /// <summary>
        /// Call BlockDestroyEvent
        /// </summary>
        /// <param name="blockEventData">data of block that is calling update</param>
        public static void InvokeBlockDestroyEvent(BlockEventData blockEventData, params int[] args)
        {
            OnBlockDestroyEvents[blockEventData.blockType]?.Invoke(blockEventData, args);
        }

        /// <summary>
        /// Call BlockDestroyEvent
        /// </summary>
        /// <param name="blockEventData">data of block that is calling update</param>
        public static void InvokeBlockPlaceEvent(BlockEventData blockEventData, params int[] args)
        {
            OnBlockPlaceEvents[blockEventData.blockType]?.Invoke(blockEventData, args);
        }

        #endregion

        #region // === Chunk loading methods === \\

        /// <summary>
        /// Schedule chunk build job
        /// </summary>
        /// <param name="positionX">x position of chunk</param>
        /// <param name="positionZ">z position of chunk</param>
        /// <param name="jobHandles">list of job handles</param>
        /// <param name="chunk">reference to target chunk</param>
        private void BuildChunk(int positionX, int positionZ, NativeQueue<JobHandle> jobHandles, ref Chunk chunk)
        {
            // if pool contains chunk
            if (pooledChunks.Count > 0)
            {
                // get chunk from pool
                chunk = pooledChunks[pooledChunks.Count - 1];
                // enable chunk
                chunk.gameObject.SetActive(true);
                // remove it from pool
                pooledChunks.RemoveAt(pooledChunks.Count - 1);

                // move chunk to target position
                chunk.ChunkPosition = new Vector2Int(positionX, positionZ);
                chunk.transform.position = new Vector3(positionX, 0, positionZ);
            }
            else
            {
                // instantiate new chunk
                GameObject chunkGO = Instantiate(chunkPrefab, new Vector3(positionX, 0, positionZ), Quaternion.identity);
                // move chunk to target position
                chunk = chunkGO.GetComponent<Chunk>();
                chunk.ChunkPosition = new Vector2Int(positionX, positionZ);
            }

            // schedule build job
            SerializableVector2Int serializableChunkPos = SerializableVector2Int.FromVector2Int(chunk.ChunkPosition);
            if (WorldSave.savedChunks.ContainsKey(serializableChunkPos))
            {
                ChunkSaveData data = WorldSave.savedChunks[serializableChunkPos];
                // convert byte[] to BlockType[]
                chunk.blocks.CopyFrom(Array.ConvertAll(data.blocks, value => (BlockType)value)); //data.blocks);
                chunk.BuildMesh(jobHandles);
            }
            else
                chunk.GenerateTerrainDataAndBuildMesh(jobHandles, positionX, positionZ);

            // disable mesh renderers
            chunk.SetMeshRenderersActive(false);

            // add chunk to chunk dict
            chunks.Add(new Vector2Int(positionX, positionZ), chunk);
        }

        /// <summary>
        /// Load nearby chunks & unload far chunks
        /// </summary>
        /// <param name="instant"></param>
        private void LoadChunks(bool instant = false)
        {
            //the current chunk the player is in
            int curChunkPosX = Mathf.FloorToInt(PlayerController.PlayerTransform.position.x / ChunkSizeXZ) * ChunkSizeXZ;
            int curChunkPosZ = Mathf.FloorToInt(PlayerController.PlayerTransform.position.z / ChunkSizeXZ) * ChunkSizeXZ;

            //entered a new chunk
            if (chunkAtPlayerPosition.x != curChunkPosX || chunkAtPlayerPosition.y != curChunkPosZ)
            {
                chunkAtPlayerPosition.x = curChunkPosX;
                chunkAtPlayerPosition.y = curChunkPosZ;

                int startX = curChunkPosX - ChunkSizeXZ * RenderDistance;
                int startZ = curChunkPosZ - ChunkSizeXZ * RenderDistance;
                int maxX = curChunkPosX + ChunkSizeXZ * RenderDistance;
                int maxZ = curChunkPosZ + ChunkSizeXZ * RenderDistance;

                for (int x = startX ; x <= maxX; x += ChunkSizeXZ)
                {
                    for (int z = startZ; z <= maxZ; z += ChunkSizeXZ)
                    {
                        Vector2Int cp = new Vector2Int(x, z);

                        if (!chunks.ContainsKey(cp))
                        {
                            Chunk chunk = null;
                            BuildChunk(x, z, pendingJobs, ref chunk);
                            terrainChunks.Enqueue(chunk);
                        }
                    }
                }

                // unload far chunks
                List<Vector2Int> toDestroy = new List<Vector2Int>();
                foreach (KeyValuePair<Vector2Int, Chunk> c in chunks)
                {
                    Vector2Int cp = c.Key;
                    if (Mathf.Abs(curChunkPosX - cp.x) > ChunkSizeXZ * (RenderDistance + 3) ||
                        Mathf.Abs(curChunkPosZ - cp.y) > ChunkSizeXZ * (RenderDistance + 3))
                    {
                        toDestroy.Add(c.Key);
                    }
                }

                // add chunks to pool
                foreach (Vector2Int cp in toDestroy)
                {
                    Chunk tc = chunks[cp];
                    tc.DissapearingAnimation();

                    LoadedChunks--;

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
            int meshID = chunk.BlockMeshFilter.mesh.GetInstanceID();

            BakePhysicsXMesh bakePhysics = new BakePhysicsXMesh()
            {
                meshID = meshID
            };

            meshBakingJobs.Enqueue(bakePhysics.Schedule());
            terrainCollisionMeshes.Enqueue(chunk);
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
        public static BlockStructure GetBlock(int x, int y, int z)
        {
            Chunk chunk = GetChunk(x, z);
            if (!chunk)
                return WorldData.GetBlockData(BlockType.AIR);

            BlockPosition bp = new BlockPosition(x, y, z);

            return WorldData.GetBlockData(chunk.GetBlock(bp));
        }

        public static BlockPosition GetTopSolidBlock(Vector2Int worldPositon, out Chunk chunk)
        {
            chunk = GetChunk(worldPositon.x, worldPositon.y);
            BlockPosition blockPosition = new BlockPosition(new int3(worldPositon.x, 0, worldPositon.y));
            for (int y = ChunkSizeY - 1; y >= 0; y--)
            {
                blockPosition.y = y;
                int index = Utils.BlockPosition3DtoIndex(blockPosition);
                if (WorldData.GetBlockState(chunk.blocks[index]) == BlockState.SOLID)
                {
                    return blockPosition;
                }
            }

            return new BlockPosition();
        }

        /// <summary>
        /// Get position of top block at specified x and z position
        /// </summary>
        /// <param name="worldPositon">x and z position of block</param>
        /// <param name="chunk">chunk containing this block</param>
        public static BlockPosition GetTopBlockPosition(Vector2Int worldPositon, out Chunk chunk)
        {
            chunk = GetChunk(worldPositon.x, worldPositon.y);
            BlockPosition blockPosition = new BlockPosition(new int3(worldPositon.x, 0, worldPositon.y));
            for (int y = ChunkSizeY - 1; y >= 0; y--)
            {
                blockPosition.y = y;
                int index = Utils.BlockPosition3DtoIndex(blockPosition);
                if (chunk.blocks[index] != BlockType.AIR)
                {
                    return blockPosition;
                }
            }

            return new BlockPosition();
        }

        /// <summary>
        /// Get position of top block at specified x and z position
        /// </summary>
        /// <param name="worldPositon">x and z position of block</param>
        public static BlockPosition GetTopBlockPosition(Vector2Int worldPositon)
        {
            Chunk chunk = GetChunk(worldPositon.x, worldPositon.y);
            BlockPosition blockPosition = new BlockPosition(new int3(worldPositon.x, 0, worldPositon.y));
            for (int y = ChunkSizeY - 1; y >= 0; y--)
            {
                blockPosition.y = y;
                int index = Utils.BlockPosition3DtoIndex(blockPosition);
                if (chunk.blocks[index] != BlockType.AIR)
                {
                    return blockPosition;
                }
            }

            return new BlockPosition();
        }

        /// <summary>
        /// Get chunk at provided position
        /// </summary>
        /// <param name="x">chunk position x</param>
        /// <param name="z">chunk position y</param>
        /// <returns>chunk</returns>
        public static Chunk GetChunk(float x, float z)
        {
            int chunkPosX = Mathf.FloorToInt(x / ChunkSizeXZ) * ChunkSizeXZ;
            int chunkPosZ = Mathf.FloorToInt(z / ChunkSizeXZ) * ChunkSizeXZ;

            Vector2Int cp = new Vector2Int(chunkPosX, chunkPosZ);

            if (chunks.TryGetValue(cp, out Chunk result))
                return result;

            return null;
        }

        /// <summary>
        /// Try to get chunk at specified position
        /// </summary>
        /// <param name="x">chunk position x</param>
        /// <param name="z">chunk position z</param>
        /// <param name="chunk">reference to chunk if found</param>
        /// <returns>true if found</returns>
        public static bool TryGetChunk(float x, float z, out Chunk chunk)
        {
            int chunkPosX = Mathf.FloorToInt(x / ChunkSizeXZ) * ChunkSizeXZ;
            int chunkPosZ = Mathf.FloorToInt(z / ChunkSizeXZ) * ChunkSizeXZ;

            Vector2Int cp = new Vector2Int(chunkPosX, chunkPosZ);

            chunks.TryGetValue(cp, out chunk);
            return chunk != null;
        }

        public static Color GetBiomeColor(BiomeType biomeType)
        {
            return Instance.biomeColors.GetBiomeColor(biomeType);
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
                for (int i = 0; i < maxChunksToBuildAtOnce; i++)
                {
                    if (pendingJobs.Peek().IsCompleted)
                    {
                        pendingJobs.Dequeue().Complete();

                        Chunk tc = terrainChunks.Dequeue();

                        tc.gameObject.SetActive(true);
                        tc.CreateBiomeTexture();
                        tc.Animation();
                        tc.ApplyMesh();

                        LoadedChunks++;

                        if (pendingJobs.Count == 0)
                            break;
                        // TODO: check if player is close
                        // ChunkLoading();
                    }
                    else
                        break;
                }
            }

            while (meshBakingJobs.Count > 0)
            {
                if (meshBakingJobs.Peek().IsCompleted)
                {
                    meshBakingJobs.Dequeue().Complete();

                    Chunk chunk = terrainCollisionMeshes.Dequeue();
                    chunk.BlockMeshCollider.sharedMesh = chunk.BlockMeshFilter.mesh;
                }
            }

            TimeToBuild.Invoke();
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
                tickQueue.Add(new WorldEventQueueData(chunk, blockPos, ticks, args));
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
                tickQueue.Add(new WorldEventQueueData(chunk, blockPos, tick, args));
                updatePositions.Add(blockPos);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Handle world updates
        /// </summary>
        private void Tick()
        {
            LoadChunks();

            foreach (var tick in tickQueue.ToArray())
            {
                tick.ticks -= 1;
                if (tick.ticks <= 0)
                {
                    tickQueue.Remove(tick);
                    updatePositions.Remove(tick.blockPosition);

                    tick.chunk?.OnBlockUpdate(tick.blockPosition, tick.args);
                }
            }
            CurrentTick++;
            OnTick?.Invoke(CurrentTick);
        }

        #endregion

        #region  // == STATIC == \\

        /// <summary>
        /// Called when scene is loaded first time
        /// </summary>
        public static void OnFirstLoadDone()
        {
            Vector2Int playerWorldPos = new Vector2Int((int)PlayerController.PlayerTransform.position.x, (int)PlayerController.PlayerTransform.position.z);

            BlockPosition blockPosition = GetTopBlockPosition(playerWorldPos, out Chunk chunk);
            chunk.SetBlock(blockPosition, BlockType.OBSIDIAN, SetBlockSettings.PLACE);

            Vector3 playerPos = blockPosition.ToVector3Int() + new Vector3(0.5f, 3.5f, 0.5f);

            PlayerController.PlayerTransform.position = playerPos;
            PlayerController.PlayerTransform.gameObject.SetActive(true);
        }

        #endregion
    }

    [Serializable]
    public struct ChunkSaveData
    {
        //public BlockParameter[] blockParameters;
        //public short[] blockParameterValues;
        public byte[] blocks;

        public ChunkSaveData(BlockType[] blocks)
        {
            // convert enum to byte to reduce file size and file size
            this.blocks = Array.ConvertAll(blocks, value => (byte)value);
        }
    }

    [Serializable]
    public class WorldSave
    {
        public SerializableVector3 playerPosition;
        public float playerEulerY;
        public Dictionary<SerializableVector2Int, ChunkSaveData> savedChunks = new Dictionary<SerializableVector2Int, ChunkSaveData>();
    }

    [Serializable]
    /// <summary>
    /// Serializable version of Vector2Int used when saving world to file
    /// </summary>
    public struct SerializableVector2Int
    {
        public int x;
        public int y;

        public Vector2Int ToVector2Int()
        {
            return new Vector2Int(x, y);
        }

        public static SerializableVector2Int FromVector2Int(Vector2Int from)
        {
            return new SerializableVector2Int() { x = from.x, y = from.y };
        }
    }

    [Serializable]
    /// <summary>
    /// Serializable version of Vector3 used when saving world to file
    /// </summary>
    public struct SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }

        public static SerializableVector3 FromVector3(Vector3 from)
        {
            return new SerializableVector3() { x = from.x, y = from.y, z = from.z };
        }
    }
}