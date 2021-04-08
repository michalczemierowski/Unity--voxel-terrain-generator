using System;
using Unity.Mathematics;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG.Terrain.Blocks
{
    [System.Serializable]
    public struct BlockParameter : IEquatable<BlockParameter>
    {
        public ParameterType Type { get; private set; }
        public byte Value { get; private set; }

        public BlockParameter(ParameterType type, byte value)
        {
            Type = type;
            Value = value;
        }

        public bool Equals(BlockParameter other)
        {
            return Value == other.Value && Type == other.Type;
        }
    }

    public enum ParameterType : byte
    {
        NONE,
        ROTATION,
        LIQUID_SOURCE_DISTANCE,
        BLOCK_TYPE,

        // it should always be at the end
        LAST
    }
}