using Unity.Burst;
using Unity.Mathematics;
using VoxelTG.Terrain;
using VoxelTG.Terrain.Blocks;

/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/
namespace VoxelTG
{
    [BurstCompile]
    public static class Utils
    {
        [BurstCompile]
        public static int ClampInRange(int value, int min, int max)
        {
            int result = value % max;
            return result < min ? max - math.abs(result) : result;
        }

        [BurstCompile]
        public static int BlockPosition3DtoIndex(BlockPosition pos)
        {
            return (pos.z * WorldSettings.fixedChunkWidth * WorldSettings.chunkHeight) + (pos.y * WorldSettings.fixedChunkWidth) + pos.x;
        }

        [BurstCompile]
        public static int BlockPosition3DtoIndex(int3 pos)
        {
            return (pos.z * WorldSettings.fixedChunkWidth * WorldSettings.chunkHeight) + (pos.y * WorldSettings.fixedChunkWidth) + pos.x;
        }

        [BurstCompile]
        public static int BlockPosition3DtoIndex(int x, int y, int z)
        {
            return (z * WorldSettings.fixedChunkWidth * WorldSettings.chunkHeight) + (y * WorldSettings.fixedChunkWidth) + x;
        }

        [BurstCompile]
        public static int BlockPosition2DtoIndex(int x, int z)
        {
            return x * WorldSettings.fixedChunkWidth + z;
        }
    }
}

