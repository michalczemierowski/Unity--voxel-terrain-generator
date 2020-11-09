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
using static VoxelTG.Terrain.WorldSettings;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Terrain
{
    public class Chunk : MonoBehaviour
    {
        #region // === Variables === \\

        #region public

        public MeshFilter blockMeshFilter, liquidMeshFilter, plantsMeshFilter;
        public MeshCollider blockMeshCollider;

        public Chunk[] neigbourChunks = new Chunk[0]; // +x, -x, +z, -z

        public NativeArray<BlockType> blocks;
        public NativeArray<BiomeType> biomeTypes;

        public Vector2Int ChunkPosition;
        public bool needToSaveBlockData { get; private set; }

        #endregion

        #region private

        private ChunkDissapearingAnimation chunkDissapearingAnimation;
        private ChunkAnimation chunkAnimation;

        public NativeHashMap<BlockParameter, short> blockParameters;

        private NativeList<float3> blockVerticles;
        private NativeList<int> blockTriangles;
        private NativeList<float2> blockUVs;

        private NativeList<float3> liquidVerticles;
        private NativeList<int> liquidTriangles;
        private NativeList<float2> liquidUVs;

        private NativeList<float3> plantsVerticles;
        private NativeList<int> plantsTriangles;
        private NativeList<float2> plantsUVs;

        private List<BlockData> blocksToBuild = new List<BlockData>();
        private Dictionary<BlockParameter, short> parametersToAdd = new Dictionary<BlockParameter, short>();

        #endregion

        #endregion

        #region // === Monobehaviour === \\

        private void Awake()
        {
            // init native containers
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
            World.timeToBuild += BuildBlocks;
            StartCoroutine(CheckNeighbours());
        }

        private void OnDisable()
        {
            World.timeToBuild -= BuildBlocks;
        }

        private void OnDestroy()
        {
            SaveDataInWorldDictionary();
        }

        public void DisposeAndSaveData()
        {
            if (!blocks.IsCreated)
                return;
            // save game before quitting
            SaveDataInWorldDictionary();

            // dispose native containers
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

        private void OnApplicationQuit()
        {
            DisposeAndSaveData();
        }

        #endregion

        private IEnumerator CheckNeighbours()
        {
            yield return new WaitForEndOfFrame();
            Vector2Int[] positions = new Vector2Int[]
            {
                new Vector2Int(ChunkPosition.x + chunkWidth, ChunkPosition.y),
                new Vector2Int(ChunkPosition.x - chunkWidth, ChunkPosition.y),
                new Vector2Int(ChunkPosition.x, ChunkPosition.y + chunkWidth),
                new Vector2Int(ChunkPosition.x, ChunkPosition.y - chunkWidth)
            };

            neigbourChunks = new Chunk[]
            {
                World.chunks.ContainsKey(positions[0]) ? World.chunks[positions[0]] : null,
                World.chunks.ContainsKey(positions[1]) ? World.chunks[positions[1]] : null,
                World.chunks.ContainsKey(positions[2]) ? World.chunks[positions[2]] : null,
                World.chunks.ContainsKey(positions[3]) ? World.chunks[positions[3]] : null,
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

        #region // === Mesh methods === \\

        public void GenerateTerrainDataAndBuildMesh(NativeQueue<JobHandle> jobHandles, int xPos, int zPos)
        {
            GenerateTerrainData generateTerrainData = new GenerateTerrainData()
            {
                chunkPosX = xPos,
                chunkPosZ = zPos,
                blockData = blocks,
                biomeTypes = biomeTypes,
                blockParameters = blockParameters,
                noises = World.biomeNoises,
                baseNoise = World.baseNoise,
                generatorSettings = World.generatorSettings,
                random = new Unity.Mathematics.Random((uint)(xPos * 10000 + zPos + 1000))
            };

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

            JobHandle handle = createMeshData.Schedule(generateTerrainData.Schedule());
            jobHandles.Enqueue(handle);
        }

        public void BuildMesh(NativeQueue<JobHandle> jobHandles)
        {
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
            jobHandles.Enqueue(handle);
        }

        public void BuildMesh(List<JobHandle> jobHandles)
        {
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

            blockMesh.RecalculateNormals();
            blockMeshFilter.mesh = blockMesh;

            // bake mesh immediately if player is near
            Vector2 playerPosition = new Vector2(PlayerController.PlayerTransform.position.x, PlayerController.PlayerTransform.position.z);
            if (Vector2.Distance(new Vector2(ChunkPosition.x, ChunkPosition.y), playerPosition) < fixedChunkWidth * 2)
                blockMeshCollider.sharedMesh = blockMesh;
            else
                World.SchedulePhysicsBake(this);

            liquidMesh.RecalculateNormals();
            liquidMeshFilter.mesh = liquidMesh;

            plantsMesh.RecalculateNormals();
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
        public void SetParameters(BlockParameter parameter, short value)
        {
            int3 blockPos = parameter.blockPos;
            // check neighbours
            if (blockPos.x == 16)
            {
                Chunk chunk = neigbourChunks[0];
                if (chunk)
                {
                    BlockParameter neighbourParameter = parameter;
                    neighbourParameter.blockPos = new int3(0, blockPos.y, blockPos.z);

                    if (chunk.blockParameters.ContainsKey(neighbourParameter))
                        chunk.blockParameters[neighbourParameter] = value;
                    else
                        chunk.blockParameters.Add(neighbourParameter, value);
                }
            }
            else if (blockPos.x == 1)
            {
                Chunk chunk = neigbourChunks[1];
                if (chunk)
                {
                    BlockParameter neighbourParameter = parameter;
                    neighbourParameter.blockPos = new int3(17, blockPos.y, blockPos.z);

                    if (chunk.blockParameters.ContainsKey(neighbourParameter))
                        chunk.blockParameters[neighbourParameter] = value;
                    else
                        chunk.blockParameters.Add(neighbourParameter, value);
                }
            }

            if (blockPos.z == 16)
            {
                Chunk chunk = neigbourChunks[2];
                if (chunk)
                {
                    BlockParameter neighbourParameter = parameter;
                    neighbourParameter.blockPos = new int3(blockPos.x, blockPos.y, 0);

                    if (chunk.blockParameters.ContainsKey(neighbourParameter))
                        chunk.blockParameters[neighbourParameter] = value;
                    else
                        chunk.blockParameters.Add(neighbourParameter, value);
                }
            }
            else if (blockPos.z == 1)
            {
                Chunk chunk = neigbourChunks[3];
                if (chunk)
                {
                    BlockParameter neighbourParameter = parameter;
                    neighbourParameter.blockPos = new int3(blockPos.x, blockPos.y, 17);

                    if (chunk.blockParameters.ContainsKey(neighbourParameter))
                        chunk.blockParameters[neighbourParameter] = value;
                    else
                        chunk.blockParameters.Add(neighbourParameter, value);
                }
            }

            if (blockParameters.ContainsKey(parameter))
                blockParameters[parameter] = value;
            else
                blockParameters.Add(parameter, value);
        }

        /// <summary>
        /// Get parameter value
        /// </summary>
        /// <param name="parameter">parameter</param>
        /// <returns>value of parameter</returns>
        public short GetParameterValue(BlockParameter parameter)
        {
            if (blockParameters.ContainsKey(parameter))
            {
                return blockParameters[parameter]; ;
            }

            return 0;
        }

        /// <summary>
        /// Remove all parameters from block
        /// </summary>
        /// <param name="blockPosition">position of block</param>
        public void ClearParameters(BlockPosition blockPosition)
        {
            ClearParameters(blockPosition.ToInt3());
        }
        /// <summary>
        /// Remove all parameters from block
        /// </summary>
        /// <param name="x">x position of block</param>
        /// <param name="y">y position of block</param>
        /// <param name="z">z position of block</param>
        public void ClearParameters(int x, int y, int z)
        {
            ClearParameters(new int3(x, y, z));
        }
        /// <summary>
        /// Remove all parameters from block
        /// </summary>
        /// <param name="blockPos">position of block</param>
        public void ClearParameters(int3 blockPos)
        {
            BlockParameter key = new BlockParameter(blockPos);
            // check neighbours
            if (blockPos.x == 16)
            {
                Chunk chunk = neigbourChunks[0];
                if (chunk)
                {
                    BlockParameter neighbourKey = new BlockParameter(new int3(0, blockPos.y, blockPos.z));
                    while (chunk.blockParameters.ContainsKey(neighbourKey))
                        chunk.blockParameters.Remove(neighbourKey);
                }
            }
            else if (blockPos.x == 1)
            {
                Chunk chunk = neigbourChunks[1];
                if (chunk)
                {
                    BlockParameter neighbourKey = new BlockParameter(new int3(17, blockPos.y, blockPos.z));
                    while (chunk.blockParameters.ContainsKey(neighbourKey))
                        chunk.blockParameters.Remove(neighbourKey);
                }
            }

            if (blockPos.z == 16)
            {
                Chunk chunk = neigbourChunks[2];
                if (chunk)
                {
                    BlockParameter neighbourKey = new BlockParameter(new int3(blockPos.x, blockPos.y, 0));
                    while (chunk.blockParameters.ContainsKey(neighbourKey))
                        chunk.blockParameters.Remove(neighbourKey);
                }
            }
            else if (blockPos.z == 1)
            {
                Chunk chunk = neigbourChunks[3];
                if (chunk)
                {
                    BlockParameter neighbourKey = new BlockParameter(new int3(blockPos.x, blockPos.y, 17));
                    while (chunk.blockParameters.ContainsKey(neighbourKey))
                        chunk.blockParameters.Remove(neighbourKey);
                }
            }

            while (blockParameters.ContainsKey(key))
                blockParameters.Remove(key);
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
            return blocks[Utils.BlockPosition3DtoIndex(blockPos.x, blockPos.y, blockPos.z)];
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
            return blocks[Utils.BlockPosition3DtoIndex(x, y, z)];
        }
        /// <summary>
        /// Get block at position [don't check if not out of range]
        /// </summary>
        /// <param name="blockPos">position of block</param>
        /// <returns>type of block</returns>
        public BlockType GetBlock(BlockPosition blockPos)
        {
            return blocks[Utils.BlockPosition3DtoIndex(blockPos.x, blockPos.y, blockPos.z)];
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
                blockType = blocks[Utils.BlockPosition3DtoIndex(blockPos.x, blockPos.y, blockPos.z)];
                return true;
            }

            blockType = BlockType.AIR;
            return false;
        }

        /// <summary>
        /// Set block at position but don't rebuild mesh
        /// </summary>
        /// <param name="x">x position of block</param>
        /// <param name="y">y position of block</param>
        /// <param name="z">z position of block</param>
        /// <param name="blockType">type of block you want to place</param>
        /// <param name="destroy">spawn destroy particle</param>
        private void SetBlockWithoutRebuild(BlockPosition blockPosition, BlockType blockType, SetBlockSettings blockSettings)
        {
            //if (!Utils.IsPositionInChunkBounds(blockPosition)) return;

            BlockType currentBlock = GetBlock(blockPosition);

            if (currentBlock == BlockType.WATER && blockType != BlockType.WATER)
                ClearParameters(blockPosition);

            if (blockSettings.callDestroyEvent)
                World.InvokeBlockDestroyEvent(new BlockEventData(this, blockPosition, currentBlock));
            if (blockSettings.callPlaceEvent)
                World.InvokeBlockPlaceEvent(new BlockEventData(this, blockPosition, blockType));
            if (blockSettings.dropItemPickup)
            {
                Vector3 worldPosition = Utils.LocalToWorldPositionVector3Int(ChunkPosition, blockPosition) + new Vector3(0.5f, 0.5f, 0.5f);

                ItemType dropItemType = ItemType.MATERIAL;
                BlockType dropBlockType = currentBlock;
                int count = 1;

                WorldData.GetCustomBlockDrops(currentBlock, ref dropItemType, ref dropBlockType, ref count);

                if (dropItemType == ItemType.MATERIAL)
                    DroppedItemsManager.Instance.DropItemMaterial(dropBlockType, worldPosition, count: count, velocity: blockSettings.droppedItemVelocity, rotate: blockSettings.rotateDroppedItem);
                // TODO: check if name starts with tool etc.
                else
                    // TODO: item objects pool
                    DroppedItemsManager.Instance.DropItemTool(dropItemType, worldPosition, count: count, velocity: blockSettings.droppedItemVelocity);
            }

            blocks[Utils.BlockPosition3DtoIndex(blockPosition)] = blockType;
            needToSaveBlockData = true;
        }

        /// <summary>
        /// Set block at position and rebuild mesh - use when you want to place one block, else take a look at
        /// <see cref="SetBlockWithoutRebuild(int, int, int, BlockType, bool)"/> or <see cref="SetBlocks(BlockData[], bool)"/>
        /// </summary>
        /// <param name="position">position of block</param>
        /// <param name="blockType">type of block you want to place</param>
        /// <param name="destroy">spawn destroy particle</param>
        public void SetBlock(int3 position, BlockType blockType, SetBlockSettings blockSettings)
        {
            SetBlock(new BlockPosition(position.x, position.y, position.z), blockType, blockSettings);
        }
        /// <summary>
        /// Set block at position and rebuild mesh - use when you want to place one block, else take a look at
        /// <see cref="SetBlockWithoutRebuild(int, int, int, BlockType, bool)"/> or <see cref="SetBlocks(BlockData[], bool)"/>
        /// </summary>
        /// <param name="position">position of block</param>
        /// <param name="blockType">type of block you want to place</param>
        /// <param name="destroy">spawn destroy particle</param>
        public void SetBlock(int x, int y, int z, BlockType blockType, SetBlockSettings blockSettings)
        {
            SetBlock(new BlockPosition(x, y, z), blockType, blockSettings);
        }
        /// <summary>
        /// Set block at position and rebuild mesh - use when you want to place one block, else take a look at
        /// <see cref="SetBlockWithoutRebuild(int, int, int, BlockType, bool)"/> or <see cref="SetBlocks(BlockData[], bool)"/>
        /// </summary>
        /// <param name="x">x position of block</param>
        /// <param name="y">y position of block</param>
        /// <param name="z">z position of block</param>
        /// <param name="blockType">type of block you want to place</param>
        /// <param name="destroy">spawn destroy particle</param>
        public void SetBlock(BlockPosition blockPosition, BlockType blockType, SetBlockSettings blockSettings)
        {
            List<JobHandle> jobHandles = new List<JobHandle>();
            List<Chunk> chunksToBuild = new List<Chunk>();

            SetBlockWithoutRebuild(blockPosition, blockType, blockSettings);

            // add current chunk
            BuildMesh(jobHandles);
            chunksToBuild.Add(this);

            // check neighbours
            if (blockPosition.x == 16)
            {
                Chunk neighbourChunk = neigbourChunks[0];
                if (neighbourChunk)
                {
                    neighbourChunk.blocks[Utils.BlockPosition3DtoIndex(0, blockPosition.y, blockPosition.z)] = blockType;
                    neighbourChunk.needToSaveBlockData = true;
                    neighbourChunk.BuildMesh(jobHandles);
                    chunksToBuild.Add(neighbourChunk);
                }
            }
            else if (blockPosition.x == 1)
            {
                Chunk neighbourChunk = neigbourChunks[1];
                if (neighbourChunk)
                {
                    neighbourChunk.blocks[Utils.BlockPosition3DtoIndex(17, blockPosition.y, blockPosition.z)] = blockType;
                    neighbourChunk.needToSaveBlockData = true;
                    neighbourChunk.BuildMesh(jobHandles);
                    chunksToBuild.Add(neighbourChunk);
                }
            }

            if (blockPosition.z == 16)
            {
                Chunk neighbourChunk = neigbourChunks[2];
                if (neighbourChunk)
                {
                    neighbourChunk.blocks[Utils.BlockPosition3DtoIndex(blockPosition.x, blockPosition.y, 0)] = blockType;
                    neighbourChunk.needToSaveBlockData = true;
                    neighbourChunk.BuildMesh(jobHandles);
                    chunksToBuild.Add(neighbourChunk);
                }
            }
            else if (blockPosition.z == 1)
            {
                Chunk neighbourChunk = neigbourChunks[3];
                if (neighbourChunk)
                {
                    neighbourChunk.blocks[Utils.BlockPosition3DtoIndex(blockPosition.x, blockPosition.y, 17)] = blockType;
                    neighbourChunk.needToSaveBlockData = true;
                    neighbourChunk.BuildMesh(jobHandles);
                    chunksToBuild.Add(neighbourChunk);
                }
            }

            NativeArray<JobHandle> njobHandles = new NativeArray<JobHandle>(jobHandles.ToArray(), Allocator.TempJob);
            JobHandle.CompleteAll(njobHandles);

            // build meshes
            foreach (Chunk tc in chunksToBuild)
            {
                tc.ApplyMesh();
            }

            // clear & dispose
            jobHandles.Clear();
            chunksToBuild.Clear();
            njobHandles.Dispose();

            UpdateNeighbourBlocks(blockPosition, 10);
        }

        /// <summary>
        /// Set array of blocks
        /// </summary>
        /// <param name="blockDatas">array containing data of each block you want to place</param>
        /// <param name="destroy">spawn destroy particle</param>
        public void SetBlocks(BlockData[] blockDatas, SetBlockSettings blockSettings)
        {
            List<JobHandle> jobHandles = new List<JobHandle>();
            List<Chunk> chunksToBuild = new List<Chunk>();

            bool[] neighboursToBuild = new bool[4];

            for (int i = 0; i < blockDatas.Length; i++)
            {
                BlockPosition blockPosition = blockDatas[i].position;
                BlockType blockType = blockDatas[i].blockType;

                SetBlockWithoutRebuild(blockPosition, blockType, blockSettings);

                // check neighbours
                if (blockPosition.x == 16)
                {
                    Chunk neighbourChunk = neigbourChunks[0];
                    if (neighbourChunk)
                    {
                        neighbourChunk.blocks[Utils.BlockPosition3DtoIndex(0, blockPosition.y, blockPosition.z)] = blockType;
                        neighbourChunk.needToSaveBlockData = true;
                        neighboursToBuild[0] = true;
                    }
                }
                else if (blockPosition.x == 1)
                {
                    Chunk neighbourChunk = neigbourChunks[1];
                    if (neighbourChunk)
                    {
                        neighbourChunk.blocks[Utils.BlockPosition3DtoIndex(17, blockPosition.y, blockPosition.z)] = blockType;
                        neighbourChunk.needToSaveBlockData = true;
                        neighboursToBuild[1] = true;
                    }
                }

                if (blockPosition.z == 16)
                {
                    Chunk neighbourChunk = neigbourChunks[2];
                    if (neighbourChunk)
                    {
                        neighbourChunk.blocks[Utils.BlockPosition3DtoIndex(blockPosition.x, blockPosition.y, 0)] = blockType;
                        neighbourChunk.needToSaveBlockData = true;
                        neighboursToBuild[2] = true;
                    }
                }
                else if (blockPosition.z == 1)
                {
                    Chunk neighbourChunk = neigbourChunks[3];
                    if (neighbourChunk)
                    {
                        neighbourChunk.blocks[Utils.BlockPosition3DtoIndex(blockPosition.x, blockPosition.y, 17)] = blockType;
                        neighbourChunk.needToSaveBlockData = true;
                        neighboursToBuild[3] = true;
                    }
                }

                UpdateNeighbourBlocks(blockDatas[i].position, 10);
            }

            // add current chunk
            BuildMesh(jobHandles);
            chunksToBuild.Add(this);

            for (int j = 0; j < 4; j++)
            {
                if (neighboursToBuild[j])
                {
                    Chunk tc = neigbourChunks[j];
                    tc.BuildMesh(jobHandles);
                    chunksToBuild.Add(tc);
                }
            }

            NativeArray<JobHandle> njobHandles = new NativeArray<JobHandle>(jobHandles.ToArray(), Allocator.Temp);
            JobHandle.CompleteAll(njobHandles);

            // build meshes
            foreach (Chunk tc in chunksToBuild)
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
                Chunk chunk = neighbours[i] < 0 ? this : neigbourChunks[neighbours[i]];
                World.ScheduleUpdate(chunk, positions[i], ticks);
            }
        }

        public void OnBlockUpdate(BlockPosition position, params int[] args)
        {
            BlockType blockType = blocks[Utils.BlockPosition3DtoIndex(position.x, position.y, position.z)];

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
                Chunk chunk = neighbours[i] < 0 ? this : neigbourChunks[neighbours[i]] == null ? this : neigbourChunks[neighbours[i]];
                BlockPosition blockPos = sideBlocks[i];
                neighbourBlocks.Add((BlockFace)i + 2, new BlockEventData(chunk, blockPos, chunk.GetBlock(blockPos)));
            }

            World.InvokeBlockUpdateEvent(new BlockEventData(this, position, blockType), neighbourBlocks, args);
        }

        /// <summary>
        /// Add block to build queue and build it in next mesh update
        /// </summary>
        /// <param name="blockPos">position of block</param>
        /// <param name="blockType">type of block you want to place</param>
        public void AddBlockToBuildList(BlockPosition blockPos, BlockType blockType)
        {
            AddBlockToBuildList(new BlockData(blockType, blockPos));
        }
        /// <summary>
        /// Add block to build queue and build it in next mesh update
        /// </summary>
        /// <param name="data">data of block you want to place</param>
        public void AddBlockToBuildList(BlockData data)
        {
            if (!blocksToBuild.Contains(data))
                blocksToBuild.Add(data);
        }

        /// <summary>
        /// Add parameter to parameter queue and add it in next mesh update
        /// </summary>
        /// <param name="param">parameter you want to set</param>
        /// <param name="value">value of parameter</param>
        /// <param name="overrideIfExists">override value if queue contains parameter of same type</param>
        public void AddParameterToList(BlockParameter param, short value, bool overrideIfExists = true)
        {
            if (!parametersToAdd.ContainsKey(param))
                parametersToAdd.Add(param, value);
            else if (overrideIfExists)
                parametersToAdd[param] = value;
        }

        /// <summary>
        /// Listener for World build update timer
        /// </summary>
        private void BuildBlocks()
        {
            // set parameters
            foreach (var param in parametersToAdd)
            {
                SetParameters(param.Key, param.Value);
            }

            parametersToAdd.Clear();

            // update blocks
            if (blocksToBuild.Count > 0)
            {
                BlockData[] datas = blocksToBuild.ToArray();
                blocksToBuild.Clear();
                SetBlocks(datas, SetBlockSettings.VANISH);
            }
        }

        #endregion

        private void SaveDataInWorldDictionary()
        {
            if (needToSaveBlockData && blocks.IsCreated)
            {
                // TODO: save parameters
                //NativeArray<BlockParameter> blockParameterKeys = blockParameters.GetKeyArray(Allocator.Temp);
                //NativeArray<short> blockParameterValues = blockParameters.GetValueArray(Allocator.Temp);
                ChunkSaveData data = new ChunkSaveData(blocks.ToArray());

                SerializableVector2Int serializableChunkPos = SerializableVector2Int.FromVector2Int(ChunkPosition);
                // add new key or update existing data
                if (World.Instance.worldSave.savedChunks.ContainsKey(serializableChunkPos))
                    World.Instance.worldSave.savedChunks[serializableChunkPos] = data;
                else
                    World.Instance.worldSave.savedChunks.Add(serializableChunkPos, data);

                //blockParameterKeys.Dispose();
                //blockParameterValues.Dispose();
            }
        }
    }
}