using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using VoxelTG.Entities.Items;
using VoxelTG.Jobs;
using VoxelTG.Player;
using VoxelTG.Player.Inventory;
using VoxelTG.Terrain.Blocks;
using VoxelTG.Terrain.Chunks;
using static VoxelTG.WorldSettings;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Terrain
{
    public class Chunk : MonoBehaviour
    {
        #region // === Variables === \\

        #region serializable

        [SerializeField] private MeshFilter blockMeshFilter;
        [SerializeField] private MeshFilter liquidMeshFilter;
        [SerializeField] private MeshFilter plantsMeshFilter;
        [SerializeField] private MeshCollider blockMeshCollider;

        #endregion

        #region public

        public MeshFilter BlockMeshFilter => blockMeshFilter;
        public MeshCollider BlockMeshCollider => blockMeshCollider;

        public NeighbourChunks NeighbourChunks { get; private set; }

        /// <summary>
        /// Has terrain changed since load
        /// </summary>
        public bool IsTerrainModified { get; private set; }

        public bool ShouldUpdateLiquid
        {
            get => liquidRebuildArray.AsReadOnly()[4];
            set
            {
                if (!World.Instance.IsInRebuildQueue(this))
                    liquidRebuildArray[4] = value;
            }
        }

        public bool NeedsRebuild { get; private set; }

        public bool IsMeshRebuildingInProgress => World.Instance.IsInRebuildQueue(this);

        public NativeHashMap<int, BlockParameter> blockParameters;
        public Dictionary<int, BlockParameter> blockParametersBuffer;
        public NativeArray<bool> liquidRebuildArray;

        /// <summary>
        /// Array containing chunk block structure [x,y,z]
        /// </summary>
        public NativeArray<BlockType> Blocks;

        /// <summary>
        /// Chunk position in World space
        /// </summary>
        public Vector2Int ChunkPosition { get; private set; }

        #endregion

        #region private

        /// <summary>
        /// Array containing information about biomes [x,z]
        /// </summary>
        private NativeArray<BiomeType> biomeTypes;

        private ChunkDissapearingAnimation chunkDissapearingAnimation;
        private ChunkAnimation chunkAnimation;

        private NativeList<float3> blockVerticles;
        private NativeList<int> blockTriangles;
        private NativeList<float2> blockUVs;

        private NativeList<float3> liquidVerticles;
        private NativeList<int> liquidTriangles;
        private NativeList<float2> liquidUVs;

        private NativeList<float3> plantsVerticles;
        private NativeList<int> plantsTriangles;
        private NativeList<float2> plantsUVs;

        private Texture2D biomeColorsTexture;

        #endregion

        #endregion

        #region // === Monobehaviour === \\

        private void OnEnable()
        {
            // init native containers
            Blocks = new NativeArray<BlockType>(FixedChunkSizeXZ * ChunkSizeY * FixedChunkSizeXZ, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            biomeTypes = new NativeArray<BiomeType>(FixedChunkSizeXZ * FixedChunkSizeXZ, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            blockParameters = new NativeHashMap<int, BlockParameter>(512, Allocator.Persistent);
            blockParametersBuffer = new Dictionary<int, BlockParameter>();

            liquidRebuildArray = new NativeArray<bool>(5, Allocator.Persistent);

            blockVerticles = new NativeList<float3>(16384, Allocator.Persistent);
            blockTriangles = new NativeList<int>(32768, Allocator.Persistent);
            blockUVs = new NativeList<float2>(16384, Allocator.Persistent);

            liquidVerticles = new NativeList<float3>(8192, Allocator.Persistent);
            liquidTriangles = new NativeList<int>(16384, Allocator.Persistent);
            liquidUVs = new NativeList<float2>(8192, Allocator.Persistent);

            plantsVerticles = new NativeList<float3>(4096, Allocator.Persistent);
            plantsTriangles = new NativeList<int>(8192, Allocator.Persistent);
            plantsUVs = new NativeList<float2>(4096, Allocator.Persistent);

            if (chunkDissapearingAnimation == null)
                chunkDissapearingAnimation = GetComponent<ChunkDissapearingAnimation>();
            if (chunkAnimation == null)
                chunkAnimation = GetComponent<ChunkAnimation>();

            if (biomeColorsTexture == null)
            {
                biomeColorsTexture = new Texture2D(FixedChunkSizeXZ, FixedChunkSizeXZ, TextureFormat.RGB24, true);
                biomeColorsTexture.filterMode = FilterMode.Bilinear;
                biomeColorsTexture.wrapMode = TextureWrapMode.Clamp;
                biomeColorsTexture.Apply();
            }

            World.OnBuildTick += OnBuildTick;
            StartCoroutine(CheckNeighbours());
        }

        private void OnDisable()
        {
            World.OnBuildTick -= OnBuildTick;

            if (!Blocks.IsCreated)
                return;

            DisposeAndSaveData();
        }

        private void OnDestroy()
        {
            if (!Blocks.IsCreated)
                return;

            SaveDataInWorldDictionary();
        }

        public void DisposeAndSaveData()
        {
            if (!Blocks.IsCreated)
                return;

            // remove from neighbour list
            NeighbourChunks.Remove();
            NeighbourChunks = null;

            // save game before quitting
            SaveDataInWorldDictionary();

            // dispose native containers
            Blocks.Dispose();
            biomeTypes.Dispose();
            blockParameters.Dispose();

            liquidRebuildArray.Dispose();

            blockVerticles.Dispose();
            blockTriangles.Dispose();
            blockUVs.Dispose();

            liquidVerticles.Dispose();
            liquidTriangles.Dispose();
            liquidUVs.Dispose();

            plantsVerticles.Dispose();
            plantsTriangles.Dispose();
            plantsUVs.Dispose();

            // clear meshes
            blockMeshFilter.mesh.Clear();
            liquidMeshFilter.mesh.Clear();
            plantsMeshFilter.mesh.Clear();

            // clear texture
            Destroy(biomeColorsTexture);
        }

        private void OnApplicationQuit()
        {
            DisposeAndSaveData();
        }

        #endregion

        public void Init()
        {
            ChunkPosition = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        }

        private IEnumerator CheckNeighbours()
        {
            yield return null;
            NeighbourChunks = new NeighbourChunks(this);

            yield return new WaitForSeconds(0.2f);
            ShouldUpdateLiquid = true;
        }

        #region // === Mesh methods === \\

        /// <summary>
        /// Generate terrain data and build mesh
        /// </summary>
        public JobHandle GenerateTerrainDataAndBuildMesh(int xPos, int zPos)
        {
            GenerateTerrainData generateTerrainData = new GenerateTerrainData()
            {
                chunkPosX = xPos,
                chunkPosZ = zPos,
                blockData = Blocks,
                biomeTypes = biomeTypes,
                blockParameters = blockParameters,
                noise = World.FastNoise,
                biomeConfigs = Biomes.biomeConfigs,
                random = new Unity.Mathematics.Random((uint)(xPos * 10000 + zPos + 1000))
            };

            JobHandle generationHandle = generateTerrainData.Schedule();
            JobHandle handle = InitCreateMeshDataJob(Blocks).Schedule(generationHandle);
            return handle;
        }

        /// <summary>
        /// Create and schedule mesh rebuilding job
        /// </summary>
        /// <returns>job handle</returns>
        public JobHandle BuildMesh(JobHandle dependency = default)
        {
            return InitCreateMeshDataJob(Blocks).Schedule(dependency);
        }

        /// <summary>
        /// Rebuild mesh (single threaded)
        /// </summary>
        public void BuildMeshInstant()
        {
            InitCreateMeshDataJob(Blocks).Run();
            ApplyMesh();
        }

        private CreateMeshDataJob InitCreateMeshDataJob(NativeArray<BlockType> blocks)
        {
            CreateMeshDataJob createMeshData = new CreateMeshDataJob
            {
                blocks = blocks,
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
            return createMeshData;
        }

        private SimulateLiquidJob InitSimulateLiquidJob(NativeArray<BlockType> blocks)
        {
            SimulateLiquidJob simulateLiquid = new SimulateLiquidJob()
            {
                blocks = blocks,
                blockParameters = blockParameters,
                needsRebuild = liquidRebuildArray,
                maxStepsPerFrame = 1
            };
            return simulateLiquid;
        }

        /// <summary>
        /// Apply values from native containers (jobs) to meshes
        /// </summary>
        public void ApplyMesh(bool rebuildPhysics = true)
        {
            Mesh blockMesh = blockMeshFilter.mesh;
            blockMesh.Clear();

            Mesh liquidMesh = liquidMeshFilter.mesh;
            liquidMesh.Clear();

            Mesh plantsMesh = plantsMeshFilter.mesh;
            plantsMesh.Clear();

            // blocks
            blockMesh.SetVertices<float3>(blockVerticles);
            blockMesh.SetTriangles(blockTriangles.ToArray(), 0, false);
            blockMesh.SetUVs<float2>(0, blockUVs);

            // liquids
            liquidMesh.SetVertices<float3>(liquidVerticles);
            liquidMesh.SetTriangles(liquidTriangles.ToArray(), 0, false);
            liquidMesh.SetUVs<float2>(0, liquidUVs);

            //plants
            plantsMesh.SetVertices<float3>(plantsVerticles);
            plantsMesh.SetTriangles(plantsTriangles.ToArray(), 0, false);
            plantsMesh.SetUVs<float2>(0, plantsUVs);

            blockMeshFilter.mesh = blockMesh;

            // bake mesh immediately if player is near
            if (rebuildPhysics && !World.IsPhysicsBakeEnqueued(blockMesh))
            {
                Vector2 playerPosition = new Vector2(PlayerController.PlayerTransform.position.x, PlayerController.PlayerTransform.position.z);
                if (Vector2.Distance(new Vector2(ChunkPosition.x, ChunkPosition.y), playerPosition) < FixedChunkSizeXZ * 2)
                    blockMeshCollider.sharedMesh = blockMesh;
                else
                    World.SchedulePhysicsBake(blockMesh, blockMeshCollider);
            }

            //liquidMesh.RecalculateNormals();
            liquidMeshFilter.mesh = liquidMesh;

            //plantsMesh.RecalculateNormals();
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

        /// <summary>
        /// Enable/disable chunk mesh renderers
        /// </summary>
        public void SetMeshRenderersActive(bool active)
        {
            foreach (var mr in GetComponentsInChildren<MeshRenderer>())
            {
                mr.enabled = active;
            }
        }

        #endregion

        #region // === Animations === \\

        /// <summary>
        /// Start chunk appear animation
        /// </summary>
        public void Animation()
        {
            ClearAnimations();

            chunkAnimation.enabled = true;
        }

        /// <summary>
        /// Reset all animations
        /// </summary>
        private void ClearAnimations()
        {
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            chunkAnimation.enabled = false;
            chunkDissapearingAnimation.enabled = false;
        }

        /// <summary>
        /// Start chunk dissapearing animation
        /// </summary>
        public void DissapearingAnimation()
        {
            ClearAnimations();
            SaveDataInWorldDictionary();

            chunkDissapearingAnimation.enabled = true;
        }

        #endregion

        #region // === Parameters === \\

        /// <summary>
        /// Set parameter of block
        /// </summary>
        /// <param name="parameter">parameter type</param>
        /// <param name="value">parameter value</param>
        public void SetBlockParameter(int3 position, ParameterType parameterType, byte value)
        {
            NeighbourChunks.SyncNeighbourParameters(position, parameterType, value);
            SetBlockParameterWithoutSync(position, parameterType, value);
        }

        public void SetBlockParameterWithoutSync(int3 position, ParameterType parameterType, byte value)
        {
            blockParametersBuffer[Utils.GetParameterIndex(position, parameterType)] = new BlockParameter(ParameterType.LIQUID_SOURCE_DISTANCE, value);
        }

        /// <summary>
        /// Remove all parameters from block
        /// </summary>
        /// <param name="blockPosition">position of block</param>
        public void RemoveAllParametersAt(BlockPosition blockPosition)
        {
            RemoveAllParametersAt(blockPosition.ToInt3());
        }
        /// <summary>
        /// Remove all parameters from block
        /// </summary>
        /// <param name="x">x position of block</param>
        /// <param name="y">y position of block</param>
        /// <param name="z">z position of block</param>
        public void RemoveAllParametersAt(int x, int y, int z)
        {
            RemoveAllParametersAt(new int3(x, y, z));
        }
        /// <summary>
        /// Remove all parameters from block
        /// </summary>
        /// <param name="position">position of block</param>
        public void RemoveAllParametersAt(int3 position)
        {
            int index = Utils.GetParameterIndex(position, ParameterType.NONE);
            for (int i = 0; i < (byte)ParameterType.LAST; i++)
            {
                int key = index + i;
                if (blockParametersBuffer.ContainsKey(key))
                {
                    blockParametersBuffer.Remove(key);
                    NeighbourChunks.SyncNeighbourParametersRemove(key);
                }
            }
        }

        public void RemoveParameterAtWithoutSync(int3 position)
        {
            int index = Utils.GetParameterIndex(position, ParameterType.NONE);
            for (int i = 0; i < (byte)ParameterType.LAST; i++)
            {
                int key = index + i;
                if (blockParametersBuffer.ContainsKey(key))
                {
                    blockParametersBuffer.Remove(key);
                }
            }
        }

        #endregion

        #region // === Editing methods === \\

        /// <summary>
        /// Get block at position [don't check if not out of range]
        /// </summary>
        /// <param name="blockPos">position of block</param>
        /// <returns>type of block</returns>
        public BlockType GetBlock(int3 blockPos)
        {
            return Blocks[Utils.BlockPosition3DtoIndex(blockPos.x, blockPos.y, blockPos.z)];
        }
        /// <summary>
        /// Get block at position [don't check if not out of range]
        /// </summary>
        /// <param name="x">x position of block</param>
        /// <param name="y">y position of block</param>
        /// <param name="z">z position of block</param>
        /// <returns>type of block</returns>
        public BlockType GetBlock(int x, int y, int z)
        {
            return Blocks[Utils.BlockPosition3DtoIndex(x, y, z)];
        }
        /// <summary>
        /// Get block at position [don't check if not out of range]
        /// </summary>
        /// <param name="blockPos">position of block</param>
        /// <returns>type of block</returns>
        public BlockType GetBlock(BlockPosition blockPos)
        {
            return Blocks[Utils.BlockPosition3DtoIndex(blockPos.x, blockPos.y, blockPos.z)];
        }

        // TODO: add x,y,z and int3
        /// <summary>
        /// Try to get block at position [check if not out of range]
        /// </summary>
        /// <param name="blockPos">position of block</param>
        /// <param name="blockType">out type of block at position</param>
        /// <returns>true if chunk contains block at position</returns>
        public bool TryGetBlock(BlockPosition blockPos, out BlockType blockType)
        {
            if (Utils.IsPositionInChunkBounds(blockPos.x, blockPos.y, blockPos.z))
            {
                blockType = Blocks[Utils.BlockPosition3DtoIndex(blockPos.x, blockPos.y, blockPos.z)];
                return true;
            }

            blockType = BlockType.AIR;
            return false;
        }

        /// <summary>
        /// Set block at position and rebuild mesh - use when you want to place one block, else take a look at
        /// </summary>
        /// <param name="position">position of block</param>
        /// <param name="blockType">type of block you want to place</param>
        /// <param name="blockSettings">settings</param>
        public void SetBlock(int3 position, BlockType blockType, SetBlockSettings blockSettings)
        {
            SetBlock(new BlockPosition(position.x, position.y, position.z), blockType, blockSettings);
        }
        /// <summary>
        /// Set block at position and rebuild mesh - use when you want to place one block, else take a look at
        /// </summary>
        /// <param name="position">position of block</param>
        /// <param name="blockType">type of block you want to place</param>
        /// <param name="blockSettings">settings</param>
        public void SetBlock(int x, int y, int z, BlockType blockType, SetBlockSettings blockSettings)
        {
            SetBlock(new BlockPosition(x, y, z, false), blockType, blockSettings);
        }
        /// <summary>
        /// Set block at position and rebuild mesh - use when you want to place one block, else take a look at
        /// </summary>
        /// <param name="x">x position of block</param>
        /// <param name="y">y position of block</param>
        /// <param name="z">z position of block</param>
        /// <param name="blockType">type of block you want to place</param>
        /// <param name="blockSettings">setttings</param>
        public void SetBlock(BlockPosition blockPosition, BlockType blockType, SetBlockSettings blockSettings)
        {
            if (blockSettings.callDestroyEvent)
                World.InvokeBlockDestroyEvent(new BlockEventData(this, blockPosition, GetBlock(blockPosition)));
            if (blockSettings.callPlaceEvent)
                World.InvokeBlockPlaceEvent(new BlockEventData(this, blockPosition, blockType));
            if (blockSettings.dropItemPickup)
            {
                BlockType currentBlock = GetBlock(blockPosition);
                Vector3 worldPosition = Utils.LocalToWorldPositionVector3Int(ChunkPosition, blockPosition) + new Vector3(0.5f, 0.5f, 0.5f);

                ItemType dropItemType = ItemType.MATERIAL;
                BlockType dropBlockType = currentBlock;
                int count = 1;

                WorldData.GetCustomBlockDrops(currentBlock, ref dropItemType, ref dropBlockType, ref count);

                if (dropItemType == ItemType.MATERIAL)
                    DroppedItemsManager.Instance.DropItem(dropBlockType, worldPosition, amount: count, velocity: blockSettings.droppedItemVelocity, rotate: blockSettings.rotateDroppedItem);
                // TODO: check if name starts with tool etc.
                else
                    // TODO: item objects pool
                    DroppedItemsManager.Instance.DropItem(dropItemType, worldPosition, amount: count, velocity: blockSettings.droppedItemVelocity);
            }

            if (blockType == BlockType.WATER)
                SetBlockParameter(blockPosition.ToInt3(), ParameterType.LIQUID_SOURCE_DISTANCE, WaterSourceMax);

            Blocks[Utils.BlockPosition3DtoIndex(blockPosition)] = blockType;
            IsTerrainModified = true;
            ShouldUpdateLiquid = true;
            NeedsRebuild = true;

            List<Chunk> chunksToBuild = new List<Chunk>();
            chunksToBuild.Add(this);
            NeighbourChunks.SyncNeighbourBlocks(chunksToBuild, blockPosition, blockType);

            foreach (Chunk chunk in chunksToBuild)
            {
                chunk.NeedsRebuild = true;
            }

            UpdateNeighbourBlocks(blockPosition, 10);
        }

        /// <summary>
        /// Set array of blocks
        /// </summary>
        /// <param name="blockData">array containing data of each block you want to place</param>
        /// <param name="destroy">spawn destroy particle</param>
        public void SetBlocks(BlockData[] blockData, SetBlockSettings blockSettings)
        {
            if (blockData == null || blockData.Length == 0)
                return;

            foreach (var data in blockData)
                SetBlock(data.Position, data.BlockType, blockSettings);
        }

        #endregion

        #region // === Block updates === \\

        /// <summary>
        /// Schedule update event on current block and nearby blocks
        /// </summary>
        /// <param name="blockPos">current block position</param>
        /// <param name="ticks">ticks wait before calling update</param>
        private void UpdateNeighbourBlocks(BlockPosition blockPos, int ticks = 1)
        {
            World.ScheduleUpdate(this, blockPos, ticks);

            int x = blockPos.x;
            int y = blockPos.y;
            int z = blockPos.z;

            int[] neighbours = new int[6];
            BlockPosition[] positions = new BlockPosition[]
            {
                new BlockPosition(x, y + 1, z, out neighbours[0]),
                new BlockPosition(x, y - 1, z, out neighbours[1]),
                new BlockPosition(x + 1, y, z, out neighbours[2]),
                new BlockPosition(x - 1, y, z, out neighbours[3]),
                new BlockPosition(x, y, z + 1, out neighbours[4]),
                new BlockPosition(x, y, z - 1, out neighbours[5])
            };
            for (int i = 0; i < 6; i++)
            {
                Chunk chunk = neighbours[i] < 0 ? this : NeighbourChunks[neighbours[i]];
                World.ScheduleUpdate(chunk, positions[i], ticks);
            }
        }

        /// <summary>
        /// Called when block receives update
        /// </summary>
        public void OnBlockUpdate(BlockPosition position, params int[] args)
        {
            BlockType blockType = Blocks[Utils.BlockPosition3DtoIndex(position.x, position.y, position.z)];

            int x = position.x;
            int y = position.y;
            int z = position.z;

            int[] neighbours = new int[4];
            BlockPosition[] sideBlocks = new BlockPosition[]
            {
                new BlockPosition(x, y, z + 1, out neighbours[0]),
                new BlockPosition(x, y, z - 1, out neighbours[1]),
                new BlockPosition(x + 1, y, z, out neighbours[2]),
                new BlockPosition(x - 1, y, z, out neighbours[3])
            };

            BlockPosition[] upDownBlocks = new BlockPosition[]
            {
                new BlockPosition(x, y + 1, z, false),
                new BlockPosition(x, y - 1, z, false)
            };

            Dictionary<BlockFace, BlockEventData> neighbourBlocks = new Dictionary<BlockFace, BlockEventData>();
            neighbourBlocks.Add(BlockFace.TOP, new BlockEventData(this, upDownBlocks[0], GetBlock(upDownBlocks[0])));
            neighbourBlocks.Add(BlockFace.BOTTOM, new BlockEventData(this, upDownBlocks[1], GetBlock(upDownBlocks[1]))); // down

            for (int i = 0; i < 4; i++)
            {
                Chunk chunk = neighbours[i] < 0 ? this : NeighbourChunks[neighbours[i]] == null ? this : NeighbourChunks[neighbours[i]];
                BlockPosition blockPos = sideBlocks[i];
                neighbourBlocks.Add((BlockFace)i + 2, new BlockEventData(chunk, blockPos, chunk.GetBlock(blockPos)));
            }

            World.InvokeBlockUpdateEvent(new BlockEventData(this, position, blockType), neighbourBlocks, args);
        }

        /// <summary>
        /// Listener for World build update timer
        /// </summary>
        private void OnBuildTick(int tick)
        {
            if (!IsMeshRebuildingInProgress)
            {
                if (NeedsRebuild || (ShouldUpdateLiquid && tick % World.Instance.BuildTicksPerWaterUpdate == 0))
                {
                    // copy block parameters
                    if (blockParametersBuffer.Count > 0)
                    {
                        foreach (var pair in blockParametersBuffer)
                        {
                            blockParameters[pair.Key] = pair.Value;
                        }
                        blockParametersBuffer.Clear();
                    }

                    NativeArray<BlockType> blocksBuffer = new NativeArray<BlockType>(Blocks, Allocator.Persistent);
                    var liquidHandle = ShouldUpdateLiquid ? InitSimulateLiquidJob(blocksBuffer).Schedule() : default;
                    var meshHandle = InitCreateMeshDataJob(blocksBuffer).Schedule(liquidHandle);

                    World.Instance.ScheduleMeshRebuild(this, meshHandle, blocksBuffer, NeedsRebuild);
                    NeedsRebuild = false;
                }
            }
        }

        #endregion

        /// <summary>
        /// Save block data
        /// </summary>
        private void SaveDataInWorldDictionary()
        {
            return;

            if (IsTerrainModified && Blocks.IsCreated)
            {
                // TODO: save parameters
                //NativeArray<BlockParameter> blockParameterKeys = blockParameters.GetKeyArray(Allocator.Temp);
                //NativeArray<short> blockParameterValues = blockParameters.GetValueArray(Allocator.Temp);
                ChunkSaveData data = new ChunkSaveData(Blocks.ToArray());

                SerializableVector2Int serializableChunkPos = SerializableVector2Int.FromVector2Int(ChunkPosition);
                // add new key or update existing data
                if (World.WorldSave.savedChunks.ContainsKey(serializableChunkPos))
                    World.WorldSave.savedChunks[serializableChunkPos] = data;
                else
                    World.WorldSave.savedChunks.Add(serializableChunkPos, data);

                //blockParameterKeys.Dispose();
                //blockParameterValues.Dispose();
            }
        }

        public void CreateBiomeTexture()
        {
            StartCoroutine(nameof(ColorDataCoroutine));
        }

        private IEnumerator ColorDataCoroutine()
        {
            NativeArray<Color> biomeColors = new NativeArray<Color>(World.GetBiomeColors(), Allocator.TempJob);
            NativeArray<Color> colors = new NativeArray<Color>(FixedChunkSizeXZ * FixedChunkSizeXZ, Allocator.TempJob);
            CreateBiomeColorData job = new CreateBiomeColorData()
            {
                biomeColors = biomeColors,
                biomes = biomeTypes,
                colors = colors
            };

            JobHandle handle = job.Schedule();
            yield return new WaitUntil(() => handle.IsCompleted);
            handle.Complete();

            biomeColorsTexture.SetPixels(colors.ToArray());
            biomeColorsTexture.Apply();
            blockMeshFilter.GetComponent<MeshRenderer>().material.SetTexture("_BiomeTexture", biomeColorsTexture);
            plantsMeshFilter.GetComponent<MeshRenderer>().material.SetTexture("_BiomeTexture", biomeColorsTexture);

            biomeColors.Dispose();
            colors.Dispose();
        }
    }
}