using System;
using Unity.Mathematics;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Terrain.Blocks
{
    public struct BlockParameter : IEquatable<BlockParameter>
    {
        public int3 blockPos;
        public ParameterType type;

        public BlockParameter(int3 blockPos, ParameterType type)
        {
            this.blockPos = blockPos;
            this.type = type;
        }

        public BlockParameter(BlockPosition blockPos, ParameterType type)
        {
            this.blockPos = new int3(blockPos.x, blockPos.y, blockPos.z);
            this.type = type;
        }

        public BlockParameter(int3 blockPos)
        {
            this.blockPos = blockPos;
            this.type = ParameterType.NONE;
        }

        public bool Equals(BlockParameter other)
        {
            if (type == ParameterType.NONE)
                return blockPos.Equals(other.blockPos);

            return blockPos.Equals(other.blockPos) && type == other.type;
        }

        public override int GetHashCode()
        {
            return blockPos.GetHashCode() + (int)type;
            //return TerrainChunk.Index3Dto1D(blockPos) + (int)type;
        }
    }

    public enum ParameterType : byte
    {
        NONE,
        ROTATION,
        WATER_SOURCE_DISTANCE,
        BLOCK_TYPE
    }
}