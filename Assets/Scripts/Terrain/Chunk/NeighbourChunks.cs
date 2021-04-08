using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using VoxelTG.Extensions;
using VoxelTG.Terrain.Blocks;
using static VoxelTG.WorldSettings;

namespace VoxelTG.Terrain.Chunks
{
    public class NeighbourChunks
    {
        private Chunk[] neighbours;
        private Chunk chunk;

        public NeighbourChunks(Chunk chunk)
        {
            this.chunk = chunk;
            neighbours = new Chunk[9];

            World world = World.Instance;
            if (chunk == null || world == null)
                return;

            Vector2Int chunkPos = chunk.ChunkPosition;
            Direction[] allDirections = (Direction[])System.Enum.GetValues(typeof(Direction));
            foreach (var dir in allDirections)
            {
                var neighbour = World.GetChunk(chunkPos + dir.ToVector2Int() * WorldSettings.ChunkSizeXZ);
                this[dir] = neighbour;
                if (neighbour != null && neighbour.NeighbourChunks != null)
                    neighbour.NeighbourChunks[dir.GetOpposite()] = chunk;
            }
        }

        public void Remove()
        {
            if (this == null)
                return;

            Direction[] allDirections = (Direction[])System.Enum.GetValues(typeof(Direction));
            foreach (var dir in allDirections)
            {
                if (this[dir] != null && this[dir].NeighbourChunks != null)
                    this[dir].NeighbourChunks[dir.GetOpposite()] = null;
            }
        }

        public Chunk this[Direction dir]
        {
            get => neighbours[(int)dir];
            set => neighbours[(int)dir] = value;
        }

        public Chunk this[int dir]
        {
            get => neighbours[dir];
            set => neighbours[dir] = value;
        }

        public bool IsNeighbour(Chunk chunk)
        {
            foreach (var neighbour in neighbours)
                if (neighbour == chunk)
                    return true;

            return false;
        }

        public void SyncNeighbourParameters(int3 position, ParameterType parameterType, byte value)
        {
            // check neighbours
            if (position.x == ChunkSizeXZ && this[Direction.E])
                this[Direction.E].SetBlockParameterWithoutSync(new int3(0, position.y, position.z), parameterType, value);
            else if (position.x == 1 && this[Direction.W])
                this[Direction.W].SetBlockParameterWithoutSync(new int3(ChunkSizeXZ + 1, position.y, position.z), parameterType, value);

            if (position.z == 16 && this[Direction.N])
                this[Direction.N].SetBlockParameterWithoutSync(new int3(position.x, position.y, 0), parameterType, value);
            else if (position.z == 1 && this[Direction.S])
                this[Direction.S].SetBlockParameterWithoutSync(new int3(position.x, position.y, ChunkSizeXZ + 1), parameterType, value);
        }

        public void SyncNeighbourParametersRemove(int3 position)
        {
            // check neighbours
            if (position.x == ChunkSizeXZ && this[Direction.E])
                this[Direction.E].RemoveParameterAtWithoutSync(new int3(0, position.y, position.z));
            else if (position.x == 1 && this[Direction.W])
                this[Direction.W].RemoveParameterAtWithoutSync(new int3(ChunkSizeXZ, position.y, position.z));

            if (position.z == 16 && this[Direction.N])
                this[Direction.N].RemoveParameterAtWithoutSync(new int3(position.x, position.y, 0));
            else if (position.z == 1 && this[Direction.S])
                this[Direction.S].RemoveParameterAtWithoutSync(new int3(position.x, position.y, ChunkSizeXZ));
        }

        public void SyncNeighbourBlocks(List<Chunk> chunksToBuild, BlockPosition blockPosition, BlockType blockType)
        {
            // check neighbours
            if (blockPosition.x == ChunkSizeXZ && this[Direction.E])
            {
                var chunk = this[Direction.E];
                chunk.SetBlock(0, blockPosition.y, blockPosition.z, blockType, SetBlockSettings.VANISH);
                chunksToBuild.Add(chunk);
            }
            else if (blockPosition.x == 1 && this[Direction.W])
            {
                var chunk = this[Direction.W];
                chunk.SetBlock(ChunkSizeXZ + 1, blockPosition.y, blockPosition.z, blockType, SetBlockSettings.VANISH);
                chunksToBuild.Add(chunk);
            }

            if (blockPosition.z == 16 && this[Direction.N])
            {
                var chunk = this[Direction.N];
                chunk.SetBlock(blockPosition.x, blockPosition.y, 0, blockType, SetBlockSettings.VANISH);
                chunksToBuild.Add(chunk);
            }
            else if (blockPosition.z == 1 && this[Direction.S])
            {
                var chunk = this[Direction.S];
                chunk.SetBlock(blockPosition.x, blockPosition.y, ChunkSizeXZ + 1, blockType, SetBlockSettings.VANISH);
                chunksToBuild.Add(chunk);
            }
        }

        public void SyncBorders()
        {
            // +x, -x, +z, -z
            if (chunk.liquidRebuildArray[0] && this[Direction.E] != null)
            {
                var neighbour = this[Direction.E];
                for (int y = 0; y < ChunkSizeY; y++)
                {
                    for (int z = 0; z < FixedChunkSizeXZ; z++)
                    {
                        neighbour.Blocks[Utils.BlockPosition3DtoIndex(0, y, z)] = chunk.Blocks[Utils.BlockPosition3DtoIndex(ChunkSizeXZ, y, z)];
                        SyncParameters(neighbour, new int3(ChunkSizeXZ, y, z), new int3(0, y, z));
                    }
                }
                neighbour.ShouldUpdateLiquid = true;
            }
            if (chunk.liquidRebuildArray[1] && this[Direction.W] != null)
            {
                var neighbour = this[Direction.W];
                for (int y = 0; y < ChunkSizeY; y++)
                {
                    for (int z = 0; z < FixedChunkSizeXZ; z++)
                    {
                        neighbour.Blocks[Utils.BlockPosition3DtoIndex(ChunkSizeXZ + 1, y, z)] = chunk.Blocks[Utils.BlockPosition3DtoIndex(1, y, z)];
                        SyncParameters(neighbour, new int3(1, y, z), new int3(ChunkSizeXZ + 1, y, z));
                    }
                }
                neighbour.ShouldUpdateLiquid = true;
            }
            if (chunk.liquidRebuildArray[2] && this[Direction.N] != null)
            {
                var neighbour = this[Direction.N];
                for (int y = 0; y < ChunkSizeY; y++)
                {
                    for (int x = 0; x < FixedChunkSizeXZ; x++)
                    {
                        neighbour.Blocks[Utils.BlockPosition3DtoIndex(x, y, 0)] = chunk.Blocks[Utils.BlockPosition3DtoIndex(x, y, ChunkSizeXZ)];
                        SyncParameters(neighbour, new int3(x, y, ChunkSizeXZ), new int3(x, y, 0));
                    }
                }
                neighbour.ShouldUpdateLiquid = true;
            }
            if (chunk.liquidRebuildArray[3] && this[Direction.S] != null)
            {
                var neighbour = this[Direction.S];
                for (int y = 0; y < ChunkSizeY; y++)
                {
                    for (int x = 0; x < FixedChunkSizeXZ; x++)
                    {
                        neighbour.Blocks[Utils.BlockPosition3DtoIndex(x, y, ChunkSizeXZ + 1)] = chunk.Blocks[Utils.BlockPosition3DtoIndex(x, y, 1)];
                        SyncParameters(neighbour, new int3(x, y, 1), new int3(x, y, ChunkSizeXZ + 1));
                    }
                }
                neighbour.ShouldUpdateLiquid = true;
            }

            for (int i = 0; i < 4; i++)
            {
                chunk.liquidRebuildArray[i] = false;
            }

            void SyncParameters(Chunk neighbour, int3 localPos, int3 otherPos)
            {
                int index = Utils.GetParameterIndex(localPos, ParameterType.NONE);
                for (int i = 1; i < (byte)ParameterType.LAST; i++)
                {
                    int key = index + i;
                    if (chunk.blockParameters.TryGetValue(key, out var param))
                    {
                        neighbour.SetBlockParameterWithoutSync(otherPos, param.Type, param.Value);
                    }
                }
            }
        }
    }
}