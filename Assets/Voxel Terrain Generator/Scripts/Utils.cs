using Unity.Burst;
using VoxelTG.Terrain;

namespace VoxelTG
{
    [BurstCompile]
    public static class Utils
    {
        [BurstCompile]
        public static int ClampInRange(int value, int min, int max)
        {
            int result = value % max;
            return result < min ? max + result : result;
        }

        [BurstCompile]
        public static int Index3Dto1D(int x, int y, int z)
        {
            return (z * Chunk.fixedChunkWidth * Chunk.chunkHeight) + (y * Chunk.fixedChunkWidth) + x;
        }

        [BurstCompile]
        public static int Index2Dto1D(int x, int z)
        {
            return x * Chunk.fixedChunkWidth + z;
        }
    }
}

